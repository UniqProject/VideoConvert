// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DecoderFfmpegGetCrop.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The DecoderFfmpegGetCrop
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
    using log4net;
    using VideoConvert.AppServices.Decoder.Interfaces;
    using VideoConvert.AppServices.Services.Base;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.AppServices.Utilities;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;

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
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the ffmpeg Process
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

            using (var decoder = new Process())
            {
                var parameter = new ProcessStartInfo(localExecutable)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                };
                decoder.StartInfo = parameter;

                bool started;
                try
                {
                    started = decoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.Error($"ffmpeg exception: {ex}");
                }

                if (started)
                {
                    var output = decoder.StandardError.ReadToEnd();
                    var regObj = new Regex(@"^.*ffmpeg version ([\w\d\.\-_]+)[, ].*$",
                        RegexOptions.Singleline | RegexOptions.Multiline);
                    var result = regObj.Match(output);
                    if (result.Success)
                        verInfo = result.Groups[1].Value;

                    decoder.WaitForExit(10000);
                    if (!decoder.HasExited)
                        decoder.Kill();
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
                if (IsEncoding)
                {
                    encodeQueueTask.ExitCode = -1;
                    throw new Exception("ffmpeg is already running");
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
                };
                DecodeProcess = new Process {StartInfo = cliStart};
                Log.Info($"start parameter: ffmpeg {query}");

                DecodeProcess.Start();

                _startTime = DateTime.Now;

                DecodeProcess.ErrorDataReceived += DecodeProcessDataReceived;
                DecodeProcess.BeginErrorReadLine();

                _decoderProcessId = DecodeProcess.Id;

                if (_decoderProcessId != -1)
                {
                    DecodeProcess.EnableRaisingEvents = true;
                    DecodeProcess.Exited += DecodeProcessExited;
                }

                DecodeProcess.PriorityClass = _appConfig.GetProcessPriority();

                // Fire the Encode Started Event
                InvokeEncodeStarted(EventArgs.Empty);
            }
            catch (Exception exc)
            {
                Log.Error(exc);
                _currentTask.ExitCode = -1;
                IsEncoding = false;
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
                if (DecodeProcess != null && !DecodeProcess.HasExited)
                {
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

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();

            var avs = new AviSynthGenerator(_appConfig);
            _inputFile = avs.GenerateCropDetect(_currentTask.VideoStream.TempFile,
                                                _currentTask.VideoStream.Fps,
                                                _currentTask.VideoStream.Length,
                                                new Size(_currentTask.VideoStream.Width,
                                                    _currentTask.VideoStream.Height),
                                                _currentTask.VideoStream.AspectRatio,
                                                out _cropDetectFrames);

            sb.Append($"-threads {Environment.ProcessorCount + 1:0} -i \"{_inputFile}\" -vf cropdetect -vcodec rawvideo -an -sn -f matroska -y NUL");

            _currentTask.VideoStream.CropRect = new Rectangle();

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
        private void DecodeProcessExited(object sender, EventArgs e)
        {
            try
            {
                DecodeProcess.CancelErrorRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            _currentTask.ExitCode = DecodeProcess.ExitCode;
            Log.Info($"Exit Code: {_currentTask.ExitCode:0}");

            if (_currentTask.ExitCode == 0)
            {
                _currentTask.TempFiles.Add(_inputFile);
            }

            FixCropReg();

            _currentTask.CompletedStep = _currentTask.NextStep;
            IsEncoding = false;
            InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
        }

        private void FixCropReg()
        {
            int mod16Temp;
            var mod16Width = Math.DivRem(_currentTask.VideoStream.Width, 16, out mod16Temp);
            mod16Width *= 16;

            var mod16Height = Math.DivRem(_currentTask.VideoStream.Height, 16, out mod16Temp);
            mod16Height *= 16;

            Point lPoint;
            if (_currentTask.VideoStream.CropRect.Width == mod16Width)
            {
                _currentTask.VideoStream.CropRect.Width = _currentTask.VideoStream.Width;
                lPoint = _currentTask.VideoStream.CropRect.Location;
                lPoint.X = 0;
                _currentTask.VideoStream.CropRect.Location = lPoint;
            }

            if (_currentTask.VideoStream.CropRect.Height != mod16Height) return;

            _currentTask.VideoStream.CropRect.Height = _currentTask.VideoStream.Height;
            lPoint = _currentTask.VideoStream.CropRect.Location;
            lPoint.Y = 0;
            _currentTask.VideoStream.CropRect.Location = lPoint;
        }

        /// <summary>
        /// process received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && IsEncoding)
            {
                ProcessLogMessage(e.Data);
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

                int.TryParse(cropResult.Groups[3].Value, NumberStyles.Number, _appConfig.CInfo,
                               out tempVal);
                loc.X = tempVal;

                int.TryParse(cropResult.Groups[4].Value, NumberStyles.Number, _appConfig.CInfo,
                               out tempVal);
                loc.Y = tempVal;

                _currentTask.VideoStream.CropRect.Location = loc;

                int.TryParse(cropResult.Groups[1].Value, NumberStyles.Number, _appConfig.CInfo,
                               out tempVal);
                _currentTask.VideoStream.CropRect.Width = tempVal;

                int.TryParse(cropResult.Groups[2].Value, NumberStyles.Number, _appConfig.CInfo,
                               out tempVal);
                _currentTask.VideoStream.CropRect.Height = tempVal;

            }
            else if (frameResult.Success)
            {
                int actualFrame;

                int.TryParse(frameResult.Groups[1].Value, NumberStyles.Number, _appConfig.CInfo,
                               out actualFrame);
                var progress = (float)actualFrame / _cropDetectFrames * 100;
                var progressLeft = 100f - progress;

                var elapsedTime = DateTime.Now - _startTime;

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
                    ElapsedTime = elapsedTime,
                };
                InvokeEncodeStatusChanged(eventArgs);
            }
            else
                Log.Info($"ffmpeg: {line}");
        }

        #endregion
    }
}