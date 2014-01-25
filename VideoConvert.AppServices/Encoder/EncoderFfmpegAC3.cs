// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderFfmpegAC3.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
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
    using Decoder;
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using Interop.Model.Profiles;
    using Interop.Utilities;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using Utilities;
    using ThreadState = System.Threading.ThreadState;

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

        /// <summary>
        /// Gets the Encoder Process ID
        /// </summary>
        private int _encoderProcessId;

        private int _decoderProcessId;

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

        private Ac3Profile _audioProfile;

        private readonly Regex _ac3EncReg = new Regex(@"^size=.\s*?([\d]*?)kB\s*?time=\s*?([\d\.\:]*)\s*?bitrate=\s*?([\d\.]*?)kbit.*$",
                                                      RegexOptions.Singleline | RegexOptions.Multiline);

        private readonly Regex _bePipeReg = new Regex(@"^([\d\,\.]*?)%.*$",
                                                      RegexOptions.Singleline | RegexOptions.Multiline);

        private NamedPipeServerStream _encodePipe;
        private IAsyncResult _encodePipeState;
        private Thread _pipeReadThread;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderFfmpegAc3"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public EncoderFfmpegAc3(IAppConfigService appConfig) : base(appConfig)
        {
            this._appConfig = appConfig;
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
                    Log.ErrorFormat("ffmpeg exception: {0}", ex);
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
            if (Log.IsDebugEnabled)
            {
                if (use64Bit)
                    Log.Debug("Selected 64 bit encoder");
                Log.DebugFormat("ffmpeg \"{0}\" found", verInfo);
            }
            return verInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encodeQueueTask"></param>
        /// <exception cref="Exception"></exception>
        public void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (this.IsEncoding)
                    throw new Exception("ffmpeg is already running");

                var use64BitEncoder = this._appConfig.Use64BitEncoders &&
                                       this._appConfig.Ffmpeg64Installed &&
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
                Log.InfoFormat("start parameter: ffmpeg {0}", query);

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
            int[] bitrateList = {64, 128, 160, 192, 224, 256, 288, 320, 352, 384, 448, 512, 576, 640};
            int[] sampleRateArr = { 0, 8000, 11025, 22050, 44100, 48000 };
            int[] channelArr = { 0, 2, 3, 4, 1 };

            this._audio = this._currentTask.AudioStreams[this._currentTask.StreamId];
            this._audioProfile = (Ac3Profile) this._currentTask.AudioProfile;

            var sb = new StringBuilder();

            var outChannels = -1;
            var outSampleRate = -1;
            var bitrate = 0;
            var channels = 0;
            var drc = false;

            switch (this._currentTask.AudioProfile.Type)
            {
                case ProfileType.AC3:
                    outChannels = this._audioProfile.OutputChannels;
                    channels = this._audioProfile.OutputChannels;

                    outChannels = channelArr[outChannels];
                    if (this._audio.ChannelCount > 6)
                        outChannels = 6;

                    outSampleRate = this._audioProfile.SampleRate;
                    outSampleRate = sampleRateArr[outSampleRate];
                    bitrate = this._audioProfile.Bitrate;
                    drc = this._audioProfile.ApplyDynamicRangeCompression;
                    break;

                case ProfileType.Copy:
                    outChannels = this._audio.ChannelCount > 6 ? 6 : this._audio.ChannelCount;
                    channels = this._audioProfile.OutputChannels;
                    outSampleRate = this._audio.SampleRate;
                    bitrate = this._audioProfile.Bitrate;
                    if (this._currentTask.EncodingProfile.OutFormat == OutputType.OutputDvd
                        && (outSampleRate != 48000 || bitrate > 10))
                    {
                        outSampleRate = 48000;
                        if (bitrate > 10)
                            bitrate = 10;

                    }
                    break;
            }

            var avs = new AviSynthGenerator(this._appConfig);

            this._inputFile = avs.GenerateAudioScript(this._audio.TempFile,
                                                      this._audio.Format,
                                                      this._audio.FormatProfile,
                                                      this._audio.ChannelCount,
                                                      outChannels,
                                                      this._audio.SampleRate,
                                                      outSampleRate);

            this._outputFile = FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation,
                                                               this._audio.TempFile,
                                                               "encoded.ac3");

            sb.AppendFormat("-f wav -i \"{0}\" -c:a ac3", this._appConfig.EncodeNamedPipeFullName);
            bitrate = bitrateList[bitrate];
            sb.AppendFormat(" -b:a {0:0}k", bitrate);

            if (channels == 2 || channels == 3)
                sb.Append(" -dsur_mode 1");

            if (drc)
                sb.Append(" -dialnorm -27");

            sb.AppendFormat(" -vn -y \"{0}\"", _outputFile);

            return sb.ToString();
        }

        private void EncoderConnected(IAsyncResult ar)
        {
            Log.Info("Encoder Pipe connected");
            this._encodePipeState = ar;
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
            if (!_encodePipe.IsConnected)
            {
                _encodePipe.WaitForConnection();
            }

            try
            {
                DecodeProcess.StandardOutput.BaseStream.CopyTo(_encodePipe);
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }
        }

        private void DecodeProcessExited(object sender, EventArgs e)
        {
            if (this._encodePipe != null)
            {
                try
                {
                    _encodePipe.EndWaitForConnection(_encodePipeState);
                }
                catch (Exception exc)
                {
                    Log.Error(exc);
                }

                if (this._pipeReadThread != null && this._pipeReadThread.ThreadState == ThreadState.Running)
                    this._pipeReadThread.Abort();
                this.DecodeProcess.WaitForExit();

                if (this._encodePipe.IsConnected)
                    _encodePipe.Disconnect();
            }
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

        private void DecodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = e.Data;
            if (string.IsNullOrEmpty(line) || !this.IsEncoding) return;

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
                    Task = 0,
                    TaskCount = 0,
                    ElapsedTime = elapsedTime,
                };
                this.InvokeEncodeStatusChanged(eventArgs);
            }
            else
                Log.InfoFormat("bepipe: {0}", line);
        }

        /// <summary>
        /// process received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data) || !this.IsEncoding) return;

            this.ProcessLogMessage(e.Data);
        }

        private void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            var result = _ac3EncReg.Match(line);
            if (!result.Success)
                Log.InfoFormat("ffmpeg: {0}", line);
        }

        #endregion
    }
}