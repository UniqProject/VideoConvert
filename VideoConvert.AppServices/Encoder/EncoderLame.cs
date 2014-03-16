// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderLame.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The EncoderLame
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Encoder
{
    using Decoder;
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using Interop.Model.Profiles;
    using Interop.Utilities;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Pipes;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Utilities;
    using ThreadState = System.Threading.ThreadState;

    /// <summary>
    /// The EncoderLame
    /// </summary>
    public class EncoderLame : EncodeBase, IEncoderLame
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (EncoderLame));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "lame.exe";
        private const string Executable64 = "lame_64.exe";

        /// <summary>
        /// Gets the Encoder Process ID
        /// </summary>
        private int _encoderProcessId;

        /// <summary>
        /// Start time of the current Encode;
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// The Current Task
        /// </summary>
        private EncodeInfo _currentTask;

        private string _inputFile;

        private readonly Regex _pipeObj = new Regex(@"^([\d\,\.]*?)%.*$",
                                                    RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly Regex _bePipeReg = new Regex(@"^([\d\,\.]*?)%.*$",
                                                      RegexOptions.Singleline | RegexOptions.Multiline);

        private Mp3Profile _encProfile;
        private AudioInfo _audio;
        private string _outputFile;
        private NamedPipeServerStream _encodePipe;
        private IAsyncResult _encodePipeState;
        private Thread _pipeReadThread;
        private int _decoderProcessId;
        private bool _dataWriteStarted;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderLame"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public EncoderLame(IAppConfigService appConfig) : base(appConfig)
        {
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the lame Process
        /// </summary>
        protected Process EncodeProcess { get; set; }

        /// <summary>
        /// Gets or sets the BePipe decode process
        /// </summary>
        public Process DecodeProcess { get; set; }

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
                    Log.ErrorFormat("lame exception: {0}", ex);
                }

                if (started)
                {
                    var output = encoder.StandardError.ReadToEnd();
                    var regObj = new Regex(@"^LAME.*?version\s*?([\d\.]*?)\s*?\(.*\).*$",
                        RegexOptions.Singleline | RegexOptions.Multiline);
                    var result = regObj.Match(output);
                    if (result.Success)
                        verInfo = result.Groups[1].Value.Trim();

                    encoder.WaitForExit(10000);
                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
            {
                if (use64Bit)
                    Log.Debug("Selected 64 bit encoder");
                Log.DebugFormat("lame \"{0}\" found", verInfo);
            }
            return verInfo;
        }

        /// <summary>
        /// Execute a lame process.
        /// This should only be called from the UI thread.
        /// </summary>
        /// <param name="encodeQueueTask">
        /// The encodeQueueTask.
        /// </param>
        public override void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (this.IsEncoding)
                    throw new Exception("lame is already running");

                var use64BitEncoder = this._appConfig.Use64BitEncoders &&
                                       this._appConfig.Lame64Installed &&
                                       Environment.Is64BitOperatingSystem;

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(this._appConfig.ToolsPath, use64BitEncoder ? Executable64 : Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };
                this.EncodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: lame {0}", query);

                this.DecodeProcess = DecoderBePipe.CreateDecodingProcess(_inputFile, this._appConfig.AvsPluginsPath);

                this._encodePipe = new NamedPipeServerStream(this._appConfig.EncodeNamedPipeName,
                                                             PipeDirection.InOut,
                                                             3,
                                                             PipeTransmissionMode.Byte,
                                                             PipeOptions.Asynchronous);

                this._encodePipeState = this._encodePipe.BeginWaitForConnection(EncoderConnected, null);

                this.EncodeProcess.Start();
                this.DecodeProcess.Start();

                this._startTime = DateTime.Now;


                this.EncodeProcess.ErrorDataReceived += EncodeProcessDataReceived;
                this.EncodeProcess.BeginErrorReadLine();

                this.DecodeProcess.ErrorDataReceived += DecodeProcessDataReceived;
                this.DecodeProcess.BeginErrorReadLine();

                this._encoderProcessId = this.EncodeProcess.Id;
                this._decoderProcessId = this.DecodeProcess.Id;

                if (this._encoderProcessId != -1)
                {
                    this.EncodeProcess.EnableRaisingEvents = true;
                    this.EncodeProcess.Exited += EncodeProcessExited;
                }

                if (this._decoderProcessId != -1)
                {
                    this.DecodeProcess.EnableRaisingEvents = true;
                    this.DecodeProcess.Exited += DecodeProcessExited;
                }

                this.EncodeProcess.PriorityClass = this._appConfig.GetProcessPriority();
                this.DecodeProcess.PriorityClass = this._appConfig.GetProcessPriority();

                // Fire the Encode Started Event
                this.InvokeEncodeStarted(EventArgs.Empty);
            }
            catch (Exception exc)
            {
                Log.Error(exc);
                this._currentTask.ExitCode = -1;
                this.IsEncoding = false;
                this.InvokeEncodeCompleted(new EncodeCompletedEventArgs(false, exc, exc.Message));
            }
        }

        private void DecodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = e.Data;
            if (string.IsNullOrEmpty(line) || !this.IsEncoding) return;

            if (line.Contains("Writing Data..."))
                this._dataWriteStarted = true;

            var bePipeMatch = _bePipeReg.Match(line);
            if (bePipeMatch.Success)
            {
                float progress;
                var tempProgress = bePipeMatch.Groups[1].Value.Replace(",", ".");
                Single.TryParse(tempProgress, NumberStyles.Number, this._appConfig.CInfo, out progress);

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
                this.InvokeEncodeStatusChanged(eventArgs);
            }
            else
                Log.InfoFormat("bepipe: {0}", line);
        }

        private void DecodeProcessExited(object sender, EventArgs e)
        {
            if (this._encodePipe != null)
            {
                try
                {
                    if (!this._encodePipeState.IsCompleted)
                        this._encodePipe.EndWaitForConnection(this._encodePipeState);
                }
                catch (Exception exc)
                {
                    Log.Error(exc);
                }

                if (this._pipeReadThread != null && this._pipeReadThread.ThreadState == ThreadState.Running)
                    this._pipeReadThread.Abort();
                this.DecodeProcess.WaitForExit();

                if (this._encodePipe.IsConnected)
                    this._encodePipe.Disconnect();
            }
        }

        private void EncoderConnected(IAsyncResult ar)
        {
            Log.Info("Encoder Pipe connected");
            this._encodePipe.EndWaitForConnection(ar);
            this._pipeReadThread = new Thread(PipeReadThreadStart);
            this._pipeReadThread.Start();
            this._pipeReadThread.Priority = this._appConfig.GetThreadPriority();
        }

        private void PipeReadThreadStart()
        {
            try
            {
                if (DecodeProcess != null)
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
                while (!this._dataWriteStarted)
                {
                    Thread.Sleep(100);
                }

                var buffer = new byte[0xA00000]; // 10 MB

                int read = this.DecodeProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
                while (read > 0 && !this.DecodeProcess.HasExited)
                {
                    this._encodePipe.Write(buffer, 0, read);
                    if (!this.DecodeProcess.HasExited)
                        read = this.DecodeProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
                }
                this._encodePipe.Close();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }
        }

        /// <summary>
        /// Kill the CLI process
        /// </summary>
        public override void Stop()
        {
            try
            {
                if (this.EncodeProcess != null && !this.EncodeProcess.HasExited)
                {
                    this.EncodeProcess.Kill();
                }
                if (this.DecodeProcess != null && !this.DecodeProcess.HasExited)
                {
                    this.DecodeProcess.Kill();
                }
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }
            this.IsEncoding = false;
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

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();

            this._encProfile = this._currentTask.AudioProfile as Mp3Profile;

            if (this._encProfile == null) return string.Empty;

            _audio = this._currentTask.AudioStreams[this._currentTask.StreamId];

            var outChannels = this._encProfile.OutputChannels;
            switch (outChannels)
            {
                case 0:
                    outChannels = _audio.ChannelCount > 2 ? 2 : 0;
                    break;
                case 1:
                    outChannels = 1;
                    break;
            }
            var outSampleRate = this._encProfile.SampleRate;
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

            var encMode = this._encProfile.EncodingMode;
            var bitrate = this._encProfile.Bitrate;
            var quality = this._encProfile.Quality;
            var preset = this._encProfile.Preset;

            var avs = new AviSynthGenerator(this._appConfig);
            this._inputFile = avs.GenerateAudioScript(_audio.TempFile, _audio.Format, _audio.FormatProfile,
                                                      _audio.ChannelCount, outChannels, _audio.SampleRate,
                                                      outSampleRate);
            this._outputFile = FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation, _audio.TempFile, "encoded.mp3");

            switch (encMode)
            {
                case 2:
                    sb.AppendFormat(this._appConfig.CInfo, "-V {0:0} ", quality);
                    break;
                case 0:
                    sb.AppendFormat("--preset {0:0} ", bitrate);
                    break;
                case 1:
                    sb.AppendFormat("--preset cbr {0:0} ", bitrate);
                    break;
                case 3:
                    sb.AppendFormat("--preset {0} ", preset);
                    break;
            }

            sb.AppendFormat("\"{0}\" ", this._appConfig.EncodeNamedPipeFullName);
            sb.AppendFormat("\"{0}\" ", _outputFile);

            return sb.ToString();
        }

        /// <summary>
        /// The lame process has exited.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        private void EncodeProcessExited(object sender, EventArgs e)
        {
            if (this._encodePipe != null)
            {
                try
                {
                    if (!this._encodePipeState.IsCompleted)
                        this._encodePipe.EndWaitForConnection(this._encodePipeState);
                }
                catch (Exception exc)
                {
                    Log.Error(exc);
                }

                try
                {
                    this._encodePipe.Close();
                }
                catch (Exception exc)
                {
                    Log.Error(exc);
                }
            }

            if (this._pipeReadThread != null && this._pipeReadThread.ThreadState == ThreadState.Running)
                this._pipeReadThread.Abort();
            this.EncodeProcess.WaitForExit();

            try
            {
                this.EncodeProcess.CancelErrorRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            this._currentTask.ExitCode = EncodeProcess.ExitCode;
            Log.InfoFormat("Exit Code: {0:g}", this._currentTask.ExitCode);

            if (this._currentTask.ExitCode == 0)
            {
                this._currentTask.TempFiles.Add(this._inputFile);
                this._currentTask.TempFiles.Add(_audio.TempFile);
                this._currentTask.TempFiles.Add(_audio.TempFile + ".d2a");
                this._currentTask.TempFiles.Add(_audio.TempFile + ".ffindex");
                _audio.TempFile = _outputFile;
                AudioHelper.GetStreamInfo(_audio);
            }

            this._currentTask.CompletedStep = this._currentTask.NextStep;
            this.IsEncoding = false;
            this.InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
        }

        /// <summary>
        /// process received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && this.IsEncoding)
            {
                this.ProcessLogMessage(e.Data);
            }
        }

        private void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            var result = _pipeObj.Match(line);
            if (!result.Success)
                Log.InfoFormat("lame: {0}", line);
        }

        #endregion
    }
}