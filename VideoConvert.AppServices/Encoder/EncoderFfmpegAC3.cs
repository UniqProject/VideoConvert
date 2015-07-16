// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderFfmpegAC3.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The EncoderFfmpegAC3
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
    /// The EncoderFfmpegAC3
    /// </summary>
    public class EncoderFfmpegAc3 : EncodeBase, IEncoderFfmpegAc3
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (EncoderFfmpegAc3));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "ffmpeg.exe";
        private const string Executable64 = "ffmpeg_64.exe";

        private int _encoderProcessId;
        private int _decoderProcessId;

        private DateTime _startTime;

        private EncodeInfo _currentTask;

        private string _inputFile;
        private string _outputFile;

        private AudioInfo _audio;
        private Ac3Profile _audioProfile;

        private readonly Regex _ac3EncReg = new Regex(@"^size=.\s*?([\d]*?)kB\s*?time=\s*?([\d\.\:]*)\s*?bitrate=\s*?([\d\.]*?)kbit.*$",
                                                      RegexOptions.Singleline | RegexOptions.Multiline);

        private readonly Regex _bePipeReg = new Regex(@"^([\d\,\.]*?)%.*$",
                                                      RegexOptions.Singleline | RegexOptions.Multiline);

        private NamedPipeServerStream _encodePipe;
        private IAsyncResult _encodePipeState;
        private Thread _pipeReadThread;

        private bool _dataWriteStarted;
        private bool _decoderIsRunning;
        private bool _encoderIsRunning;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderFfmpegAc3"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public EncoderFfmpegAc3(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the ffmpeg Process
        /// </summary>
        protected Process EncodeProcess { get; set; }

        /// <summary>
        /// Gets or sets the BePipe decode process
        /// </summary>
        protected Process DecodeProcess { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads encoder version from its output, use path settings from parameters
        /// </summary>
        /// <param name="encPath">Path to encoder</param>
        /// <param name="use64Bit"></param>
        /// <returns>Encoder version</returns>
        public static string GetVersionInfo(string encPath, bool use64Bit)
        {
            var verInfo = string.Empty;

            if (use64Bit && !Environment.Is64BitOperatingSystem) return string.Empty;

            var localExecutable = Path.Combine(encPath, use64Bit ? Executable64 : Executable);

            using (var encoder = new Process())
            {
                var parameter = new ProcessStartInfo(localExecutable)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
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
                    Log.Error($"ffmpeg exception: {ex}");
                }

                if (started)
                {
                    var output = encoder.StandardError.ReadToEnd();
                    var regObj = new Regex(@"^.*ffmpeg version ([\w\d\.\-_]+)[, ].*$",
                        RegexOptions.Singleline | RegexOptions.Multiline);
                    var result = regObj.Match(output);
                    if (result.Success)
                        verInfo = result.Groups[1].Value;

                    encoder.WaitForExit(10000);
                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (!Log.IsDebugEnabled) return verInfo;

            if (use64Bit)
                Log.Debug("Selected 64 bit encoder");
            Log.Debug($"ffmpeg \"{verInfo}\" found");

            return verInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encodeQueueTask"></param>
        /// <exception cref="Exception"></exception>
        public override void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (IsEncoding)
                {
                    encodeQueueTask.ExitCode = -1;
                    throw new Exception("ffmpeg is already running");
                }

                var use64BitEncoder = _appConfig.Use64BitEncoders &&
                                       _appConfig.Ffmpeg64Installed &&
                                       Environment.Is64BitOperatingSystem;

                IsEncoding = true;
                _currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(_appConfig.ToolsPath, use64BitEncoder ? Executable64 : Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = _appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };
                EncodeProcess = new Process {StartInfo = cliStart};
                Log.Info($"start parameter: ffmpeg {query}");

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
        /// The ffmpeg process has exited.
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
            Log.Info($"Exit Code: {_currentTask.ExitCode:g}");

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

            var bePipeMatch = _bePipeReg.Match(line);
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
            if (string.IsNullOrEmpty(e.Data) || !IsEncoding) return;

            ProcessLogMessage(e.Data);
        }

        private void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            var result = _ac3EncReg.Match(line);
            if (!result.Success)
                Log.Info($"ffmpeg: {line}");
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
                // wait for data from bepipe, otherwise ffmpeg aborts after reading wav header with this message:
                // Invalid data found when processing input
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
            int[] bitrateList = { 64, 128, 160, 192, 224, 256, 288, 320, 352, 384, 448, 512, 576, 640 };
            int[] sampleRateArr = { 0, 8000, 11025, 22050, 44100, 48000 };
            int[] channelArr = { 0, 2, 3, 4, 1 };

            _audio = _currentTask.AudioStreams[_currentTask.StreamId];
            _audioProfile = (Ac3Profile)_currentTask.AudioProfile;

            var sb = new StringBuilder();

            var outChannels = -1;
            var outSampleRate = -1;
            var bitrate = 0;
            var channels = 0;
            var drc = false;

            switch (_currentTask.AudioProfile.Type)
            {
                case ProfileType.Ac3:
                    outChannels = _audioProfile.OutputChannels;
                    channels = _audioProfile.OutputChannels;

                    outChannels = channelArr[outChannels];
                    if (_audio.ChannelCount > 6)
                        outChannels = 6;

                    outSampleRate = _audioProfile.SampleRate;
                    outSampleRate = sampleRateArr[outSampleRate];

                    bitrate = _audioProfile.Bitrate;
                    bitrate = bitrateList[bitrate];

                    drc = _audioProfile.ApplyDynamicRangeCompression;
                    break;

                case ProfileType.Copy:
                    outChannels = _audio.ChannelCount > 6 ? 6 : _audio.ChannelCount;
                    channels = _audioProfile.OutputChannels;
                    outSampleRate = _audio.SampleRate;
                    bitrate = _audioProfile.Bitrate;
                    if (_currentTask.EncodingProfile.OutFormat == OutputType.OutputDvd
                        && (outSampleRate != 48000 || bitrate > 10))
                    {
                        outSampleRate = 48000;
                        if (bitrate > 10)
                            bitrate = 10;

                    }
                    break;
            }

            var avs = new AviSynthGenerator(_appConfig);

            _inputFile = avs.GenerateAudioScript(_audio.TempFile,
                                                      _audio.Format,
                                                      _audio.FormatProfile,
                                                      _audio.ChannelCount,
                                                      outChannels,
                                                      _audio.SampleRate,
                                                      outSampleRate);

            _outputFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                               _audio.TempFile,
                                                               "encoded.ac3");

            sb.Append($"-f wav -i \"{_appConfig.EncodeNamedPipeFullName}\" -c:a ac3");
            sb.Append($" -b:a {bitrate:0}k");

            if (channels == 2 || channels == 3)
                sb.Append(" -dsur_mode 1");

            if (drc)
                sb.Append(" -dialnorm -27");

            sb.Append($" -vn -y \"{_outputFile}\"");

            return sb.ToString();
        }

        #endregion
    }
}