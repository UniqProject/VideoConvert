// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DecoderFfmpegGetCrop.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Decoder
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using Utilities;

    /// <summary>
    /// The DecoderFfmpegGetCrop
    /// </summary>
    public class DecoderFfmpegGetCrop : EncodeBase, IDecoderFfmpegGetCrop
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (DecoderFfmpegGetCrop));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "ffmpeg.exe";
        private const string Executable64 = "ffmpeg_64.exe";

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

        private readonly Regex _cropReg = new Regex(@"^.*crop=(\d*):(\d*):(\d*):(\d*).*$",
                                                    RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly Regex _frameReg = new Regex(@"^.*frame=\s*(\d*).*$", 
                                                     RegexOptions.Singleline | RegexOptions.Multiline);

        private int _cropDetectFrames;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DecoderFfmpegGetCrop"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public DecoderFfmpegGetCrop(IAppConfigService appConfig) : base(appConfig)
        {
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the ffmpeg Process
        /// </summary>
        protected Process EncodeProcess { get; set; }

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
        /// Execute a ffmpeg crop detection process.
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
                    throw new Exception("ffmpeg is already running");

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(this._appConfig.ToolsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                };
                this.EncodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: ffmpeg {0}", query);

                this.EncodeProcess.Start();

                this._startTime = DateTime.Now;

                this.EncodeProcess.ErrorDataReceived += EncodeProcessDataReceived;
                this.EncodeProcess.BeginErrorReadLine();

                this._encoderProcessId = this.EncodeProcess.Id;

                if (this._encoderProcessId != -1)
                {
                    this.EncodeProcess.EnableRaisingEvents = true;
                    this.EncodeProcess.Exited += EncodeProcessExited;
                }

                this.EncodeProcess.PriorityClass = this._appConfig.GetProcessPriority();

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

            var avs = new AviSynthGenerator(this._appConfig);
            _inputFile = avs.GenerateCropDetect(this._currentTask.VideoStream.TempFile,
                                                this._currentTask.VideoStream.Fps,
                                                this._currentTask.VideoStream.Length,
                                                new Size(this._currentTask.VideoStream.Width,
                                                    this._currentTask.VideoStream.Height),
                                                this._currentTask.VideoStream.AspectRatio,
                                                out _cropDetectFrames);

            sb.AppendFormat(this._appConfig.CInfo, 
                            "-threads {0:g} -i \"{1}\" -vf cropdetect -vcodec rawvideo -an -sn -f matroska -y NUL", 
                            Environment.ProcessorCount + 1,
                            _inputFile);

            this._currentTask.VideoStream.CropRect = new Rectangle();

            return sb.ToString();
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
                this._currentTask.TempFiles.Add(_inputFile);
            }

            FixCropReg();

            this._currentTask.CompletedStep = this._currentTask.NextStep;
            this.IsEncoding = false;
            this.InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
        }

        private void FixCropReg()
        {
            int mod16Temp;
            var mod16Width = Math.DivRem(this._currentTask.VideoStream.Width, 16, out mod16Temp);
            mod16Width *= 16;

            var mod16Height = Math.DivRem(this._currentTask.VideoStream.Height, 16, out mod16Temp);
            mod16Height *= 16;

            if (this._currentTask.VideoStream.CropRect.Width == mod16Width)
            {
                this._currentTask.VideoStream.CropRect.Width = this._currentTask.VideoStream.Width;
                var lPoint = this._currentTask.VideoStream.CropRect.Location;
                lPoint.X = 0;
                this._currentTask.VideoStream.CropRect.Location = lPoint;
            }

            if (this._currentTask.VideoStream.CropRect.Height == mod16Height)
            {
                this._currentTask.VideoStream.CropRect.Height = this._currentTask.VideoStream.Height;
                var lPoint = this._currentTask.VideoStream.CropRect.Location;
                lPoint.Y = 0;
                this._currentTask.VideoStream.CropRect.Location = lPoint;
            }
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

            var cropResult = _cropReg.Match(line);
            var frameResult = _frameReg.Match(line);
            if (cropResult.Success)
            {
                var loc = new Point();
                int tempVal;

                Int32.TryParse(cropResult.Groups[3].Value, NumberStyles.Number, this._appConfig.CInfo,
                               out tempVal);
                loc.X = tempVal;

                Int32.TryParse(cropResult.Groups[4].Value, NumberStyles.Number, this._appConfig.CInfo,
                               out tempVal);
                loc.Y = tempVal;

                this._currentTask.VideoStream.CropRect.Location = loc;

                Int32.TryParse(cropResult.Groups[1].Value, NumberStyles.Number, this._appConfig.CInfo,
                               out tempVal);
                this._currentTask.VideoStream.CropRect.Width = tempVal;

                Int32.TryParse(cropResult.Groups[2].Value, NumberStyles.Number, this._appConfig.CInfo,
                               out tempVal);
                this._currentTask.VideoStream.CropRect.Height = tempVal;

            }
            else if (frameResult.Success)
            {
                int actualFrame;

                Int32.TryParse(frameResult.Groups[1].Value, NumberStyles.Number, this._appConfig.CInfo,
                               out actualFrame);
                var progress = (float)actualFrame / _cropDetectFrames * 100;
                var progressLeft = 100f - progress;

                var elapsedTime = DateTime.Now - this._startTime;

                var speed = 0f;
                if (elapsedTime.TotalSeconds > 0)
                {
                    speed = actualFrame / progress;
                }

                var remainingSecs = 0f;
                if (speed > 0)
                {
                    remainingSecs = progressLeft * speed;
                }

                var remainingTime = TimeSpan.FromSeconds(remainingSecs);

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
                Log.InfoFormat("ffmpeg: {0}", line);
        }

        #endregion
    }
}