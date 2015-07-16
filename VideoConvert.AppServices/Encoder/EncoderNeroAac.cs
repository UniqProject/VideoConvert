// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderNeroAac.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The EncoderNeroAac
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Encoder
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Pipes;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using log4net;
    using VideoConvert.AppServices.Decoder;
    using VideoConvert.AppServices.Encoder.Interfaces;
    using VideoConvert.AppServices.Services.Base;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.AppServices.Utilities;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Model.Profiles;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// The EncoderNeroAac
    /// </summary>
    public class EncoderNeroAac : EncodeBase, IEncoderNeroAac
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (EncoderNeroAac));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "neroAacEnc.exe";

        /// <summary>
        /// Gets the Encoder Process ID
        /// </summary>
        private int _encoderProcessId;

        /// <summary>
        /// Get the Decoder Process ID
        /// </summary>
        private int _decoderProcessId;

        private NamedPipeServerStream _encodePipe;
        private IAsyncResult _encodePipeState;
        private Thread _pipeReadThread;

        /// <summary>
        /// Start time of the current Encode;
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// The Current Task
        /// </summary>
        private EncodeInfo _currentTask;

        private string _inputFile;
        private string _outputFile;

        private AudioInfo _audio;

        private readonly Regex _encObj = new Regex(@"^.*Processed.*\d*?.*seconds...$",
                                                   RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly Regex _pipeObj = new Regex(@"^([\d\,\.]*?)%.*$",
                                                    RegexOptions.Singleline | RegexOptions.Multiline);

        private bool _dataWriteStarted;
        private bool _decoderIsRunning;
        private bool _encoderIsRunning;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderNeroAac"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public EncoderNeroAac(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the neroAacEnc Process
        /// </summary>
        protected Process EncodeProcess { get; set; }

        /// <summary>
        /// Gets or sets the BePipe decoding Process
        /// </summary>
        protected Process DecodeProcess { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads encoder version from its output, use path settings from parameters
        /// </summary>
        /// <param name="encPath">Path to encoder</param>
        /// <returns>Encoder version</returns>
        public static string GetVersionInfo(string encPath)
        {
            var verInfo = string.Empty;

            var localExecutable = Path.Combine(encPath, Executable);

            using (var encoder = new Process())
            {
                var parameter = new ProcessStartInfo(localExecutable)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    Arguments = "-help"
                };
                encoder.StartInfo = parameter;

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.Error($"neroAacEnc exception: {ex}");
                }

                if (started)
                {
                    var output = encoder.StandardError.ReadToEnd();
                    var lines = output.Split(new[] { Environment.NewLine },
                                                  StringSplitOptions.RemoveEmptyEntries);

                    var regObj = new Regex(@"^\*.*Package version:.* ([\d\.]+?) .*\*$",
                                             RegexOptions.Singleline | RegexOptions.Multiline);
                    foreach (var line in lines)
                    {
                        var result = regObj.Match(line);
                        if (!result.Success) continue;

                        verInfo = result.Groups[1].Value.Trim();
                        break;
                    }

                    encoder.WaitForExit(10000);
                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
            {
                Log.Debug($"neroAacEnc \"{verInfo}\" found");
            }
            return verInfo;
        }

        /// <summary>
        /// Execute a neroaacenc process.
        /// This should only be called from the UI thread.
        /// </summary>
        /// <param name="encodeQueueTask">
        /// The encodeQueueTask.
        /// </param>
        public override void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (IsEncoding)
                {
                    encodeQueueTask.ExitCode = -1;
                    throw new Exception("neroAacEnc is already running");
                }

                IsEncoding = true;
                _currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(_appConfig.ToolsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = _appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };

                EncodeProcess = new Process {StartInfo = cliStart};
                Log.Info($"start parameter: neroAacEnc {query}");

                DecodeProcess = DecoderBePipe.CreateDecodingProcess(_inputFile, _appConfig.AvsPluginsPath);

                _encodePipe = new NamedPipeServerStream(_appConfig.EncodeNamedPipeName,
                                                             PipeDirection.InOut,
                                                             3,
                                                             PipeTransmissionMode.Byte,
                                                             PipeOptions.Asynchronous);

                _encodePipeState = _encodePipe.BeginWaitForConnection(EncoderConnected, null);

                EncodeProcess.Start();
                DecodeProcess.Start();

                _startTime = DateTime.Now;

                EncodeProcess.ErrorDataReceived += EncodeProcessDataReceived;
                EncodeProcess.BeginErrorReadLine();

                DecodeProcess.ErrorDataReceived += DecodeProcessDataReceived;
                DecodeProcess.BeginErrorReadLine();

                _encoderProcessId = EncodeProcess.Id;
                _decoderProcessId = DecodeProcess.Id;

                if (_encoderProcessId != -1)
                {
                    EncodeProcess.EnableRaisingEvents = true;
                    EncodeProcess.Exited += EncodeProcessExited;
                    _encoderIsRunning = true;
                }

                if (_decoderProcessId != -1)
                {
                    DecodeProcess.EnableRaisingEvents = true;
                    DecodeProcess.Exited += DecodeProcessExited;
                    _decoderIsRunning = true;
                }

                EncodeProcess.PriorityClass = _appConfig.GetProcessPriority();
                DecodeProcess.PriorityClass = _appConfig.GetProcessPriority();

                // Fire the Encode Started Event
                InvokeEncodeStarted(EventArgs.Empty);
            }
            catch (Exception exc)
            {
                Log.Error(exc);
                _currentTask.ExitCode = -1;
                IsEncoding = false;
                _encoderIsRunning = false;
                _decoderIsRunning = false;
                InvokeEncodeCompleted(new EncodeCompletedEventArgs(false, exc, exc.Message));
            }
        }

        /// <summary>
        /// Kill the CLI process
        /// </summary>
        public override void Stop()
        {
            try
            {
                if (EncodeProcess != null && !EncodeProcess.HasExited)
                {
                    _encoderIsRunning = false;
                    Thread.Sleep(200);
                    EncodeProcess.Kill();
                }
                if (DecodeProcess != null && !DecodeProcess.HasExited)
                {
                    _decoderIsRunning = false;
                    Thread.Sleep(200);
                    DecodeProcess.Kill();
                }
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }
            IsEncoding = false;
        }

        /// <summary>
        /// Shutdown the service.
        /// </summary>
        public void Shutdown()
        {
            // Nothing to do.
        }

        #endregion

        #region Private Helper Methods

        private void DecodeProcessExited(object sender, EventArgs e)
        {
            if (_encodePipe == null) return;

            try
            {
                if (!_encodePipeState.IsCompleted)
                    _encodePipe.EndWaitForConnection(_encodePipeState);
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            _decoderIsRunning = false;
        }

        /// <summary>
        /// The neroAacEnc process has exited.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        private void EncodeProcessExited(object sender, EventArgs e)
        {
            if (_encodePipe != null)
            {
                try
                {
                    if (!_encodePipeState.IsCompleted)
                        _encodePipe.EndWaitForConnection(_encodePipeState);
                }
                catch (Exception exc)
                {
                    Log.Error(exc);
                }
            }

            try
            {
                EncodeProcess.CancelErrorRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            _encoderIsRunning = false;

            _currentTask.ExitCode = EncodeProcess.ExitCode;
            Log.Info($"Exit Code: {_currentTask.ExitCode:0}");

            if (_currentTask.ExitCode == 0)
            {
                _currentTask.TempFiles.Add(_inputFile);
                _currentTask.TempFiles.Add(_audio.TempFile);
                _currentTask.TempFiles.Add(_audio.TempFile + ".d2a");
                _currentTask.TempFiles.Add(_audio.TempFile + ".ffindex");
                _audio.TempFile = _outputFile;
                AudioHelper.GetStreamInfo(_audio);
            }

            _currentTask.CompletedStep = _currentTask.NextStep;
            IsEncoding = false;
            InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
        }

        private void DecodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = e.Data;
            if (string.IsNullOrEmpty(line) || !IsEncoding) return;

            if (line.Contains("Writing Data..."))
                _dataWriteStarted = true;

            var bePipeMatch = _pipeObj.Match(line);
            if (bePipeMatch.Success)
            {
                float progress;
                var tempProgress = bePipeMatch.Groups[1].Value.Replace(",", ".");
                float.TryParse(tempProgress, NumberStyles.Number, _appConfig.CInfo, out progress);

                var progressRemaining = 100f - progress;
                var elapsedTime = DateTime.Now - _startTime;

                long secRemaining = 0;
                if (elapsedTime.TotalSeconds > 0)
                {
                    var speed = Math.Round(progress / elapsedTime.TotalSeconds, 6);

                    if (speed > 0)
                        secRemaining = (long)Math.Round(progressRemaining / speed, 0);
                    else
                        secRemaining = 0;
                }
                if (secRemaining < 0)
                    secRemaining = 0;

                var remainingTime = TimeSpan.FromSeconds(secRemaining);

                var eventArgs = new EncodeProgressEventArgs
                {
                    AverageFrameRate = 0,
                    CurrentFrameRate = 0,
                    EstimatedTimeLeft = remainingTime,
                    PercentComplete = progress,
                    ElapsedTime = elapsedTime,
                };
                InvokeEncodeStatusChanged(eventArgs);
            }
            else
                Log.Info($"bepipe: {line}");
        }

        /// <summary>
        /// process received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && IsEncoding)
            {
                ProcessLogMessage(e.Data);
            }
        }

        private void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            var result = _encObj.Match(line);
            if (!result.Success)
                Log.Info($"neroAacEnc: {line}");
        }

        private void EncoderConnected(IAsyncResult ar)
        {
            Log.Info("Encoder Pipe connected");
            lock (_encodePipe)
            {
                _encodePipe.EndWaitForConnection(ar);
            }

            _pipeReadThread = new Thread(PipeReadThreadStart);
            _pipeReadThread.Start();
            _pipeReadThread.Priority = _appConfig.GetThreadPriority();
        }

        private void PipeReadThreadStart()
        {
            try
            {
                if (DecodeProcess != null && EncodeProcess != null)
                    ReadThreadStart();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void ReadThreadStart()
        {
            try
            {
                // wait for decoder to start writing
                while (!_dataWriteStarted || !_decoderIsRunning || !_encoderIsRunning)
                {
                    Thread.Sleep(100);
                }

                var buffer = new byte[0xA00000]; // 10 MB

                var read = 0;
                do
                {
                    if (_decoderIsRunning)
                        read = DecodeProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);

                    if (_encoderIsRunning)
                        _encodePipe.Write(buffer, 0, read);

                } while (read > 0 && _decoderIsRunning && _encoderIsRunning);

                _encodePipe.Close();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }
        }

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();

            _audio = _currentTask.AudioStreams[_currentTask.StreamId];

            var outChannels = ((AacProfile)_currentTask.AudioProfile).OutputChannels;
            switch (outChannels)
            {
                case 1:
                    outChannels = 2;
                    break;
                case 2:
                    outChannels = 1;
                    break;
            }
            var outSampleRate = ((AacProfile)_currentTask.AudioProfile).SampleRate;
            switch (outSampleRate)
            {
                case 1:
                    outSampleRate = 8000;
                    break;
                case 2:
                    outSampleRate = 11025;
                    break;
                case 3:
                    outSampleRate = 22050;
                    break;
                case 4:
                    outSampleRate = 44100;
                    break;
                case 5:
                    outSampleRate = 48000;
                    break;
                default:
                    outSampleRate = 0;
                    break;
            }

            var encMode = ((AacProfile)_currentTask.AudioProfile).EncodingMode;
            var bitrate = ((AacProfile)_currentTask.AudioProfile).Bitrate * 1000;
            var quality = ((AacProfile)_currentTask.AudioProfile).Quality;

            var avs = new AviSynthGenerator(_appConfig);

            _inputFile = avs.GenerateAudioScript(_audio.TempFile, _audio.Format, _audio.FormatProfile,
                                                      _audio.ChannelCount, outChannels, _audio.SampleRate,
                                                      outSampleRate);

            _outputFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation, _audio.TempFile, "encoded.m4a");

            switch (encMode)
            {
                case 0:
                    sb.Append($"-br {bitrate:0} ");
                    break;
                case 1:
                    sb.Append($"-cbr {bitrate:0} ");
                    break;
                case 2:
                    sb.Append($"-q {quality:0.00} ".ToString(_appConfig.CInfo));
                    break;
            }

            sb.Append($"-ignorelength -if \"{_appConfig.EncodeNamedPipeFullName}\" -of \"{_outputFile}\" ");

            return sb.ToString();
        }

        #endregion
    }
}