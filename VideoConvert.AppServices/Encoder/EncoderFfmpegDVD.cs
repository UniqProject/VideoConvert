// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderFfmpegDVD.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The EncoderFfmpegDVD
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Encoder
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using log4net;
    using VideoConvert.AppServices.Encoder.Interfaces;
    using VideoConvert.AppServices.Services.Base;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.AppServices.Utilities;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Model.Profiles;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// The EncoderFfmpegDVD
    /// </summary>
    public class EncoderFfmpegDvd : EncodeBase, IEncoderFfmpegDvd
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (EncoderFfmpegDvd));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "ffmpeg.exe";

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

        private Mpeg2VideoProfile _encProfile;

        private string _inputFile;
        private string _outputFile;

        private int _encodingPass;

        private readonly Regex _frameReg = new Regex(@"^.*frame=\s*(\d*)\s*fps=\s*([\d\.]*).*time=\s*([\d\.\:]*).*bitrate=\s*([\d\.]*).*kbits/s.*$",
                                                     RegexOptions.Singleline | RegexOptions.Multiline);

        private long _frameCount;
        private TimeSpan _remainingTime;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderFfmpegDvd"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public EncoderFfmpegDvd(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
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
            if (Log.IsDebugEnabled)
            {
                Log.Debug($"ffmpeg \"{verInfo}\" found");
            }
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
                EncodeProcess = new Process {StartInfo = cliStart};
                Log.Info($"start parameter: ffmpeg {query}");

                EncodeProcess.Start();

                _startTime = DateTime.Now;

                EncodeProcess.ErrorDataReceived += EncodeProcessDataReceived;
                EncodeProcess.BeginErrorReadLine();

                _encoderProcessId = EncodeProcess.Id;

                if (_encoderProcessId != -1)
                {
                    EncodeProcess.EnableRaisingEvents = true;
                    EncodeProcess.Exited += EncodeProcessExited;
                }

                EncodeProcess.PriorityClass = _appConfig.GetProcessPriority();

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
                if (EncodeProcess != null && !EncodeProcess.HasExited)
                {
                    EncodeProcess.Kill();
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
                EncodeProcess.CancelErrorRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            _currentTask.ExitCode = EncodeProcess.ExitCode;
            Log.Info($"Exit Code: {_currentTask.ExitCode:0}");

            if (_currentTask.ExitCode == 0)
            {
                if (_encProfile.EncodingMode == 0 ||
                    (_encProfile.EncodingMode == 1 && _encodingPass == 2))
                {
                    _currentTask.VideoStream.IsRawStream = false;
                    _currentTask.VideoStream.Encoded = true;

                    _currentTask.TempFiles.Add(_inputFile);
                    _currentTask.VideoStream.TempFile = _outputFile;
                    _currentTask.TempFiles.Add(_currentTask.AviSynthScript);

                    try
                    {
                        _currentTask.MediaInfo = GenHelper.GetMediaInfo(_currentTask.VideoStream.TempFile);
                    }
                    catch (TimeoutException ex)
                    {
                        Log.Error(ex);
                    }

                    _currentTask.VideoStream = VideoHelper.GetStreamInfo(_currentTask.MediaInfo,
                                                    _currentTask.VideoStream,
                                                    _currentTask.EncodingProfile.OutFormat == OutputType.OutputBluRay);
                    _currentTask.TempFiles.Add(_currentTask.FfIndexFile);
                }
            }

            _currentTask.CompletedStep = _currentTask.NextStep;
            IsEncoding = false;
            InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
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

            var elapsedTime = DateTime.Now.Subtract(_startTime);

            var result = _frameReg.Match(line);
            // groups:
            // 1: actual frame
            // 2: fps
            // 3: position in stream (time)
            // 4: bitrate
            if (result.Success)
            {
                long current;
                long.TryParse(result.Groups[1].Value, NumberStyles.Number,
                               _appConfig.CInfo, out current);

                var framesRemaining = _frameCount - current;

                var progress = ((float)current / _frameCount) * 100;

                var codingFps = 0f;
                if (elapsedTime.Seconds != 0) // prevent division by zero
                {
                    //Frames per Second
                    codingFps = (float)Math.Round(current / elapsedTime.TotalSeconds, 2);
                }

                long secRemaining;
                if (codingFps > 1) // prevent another division by zero
                    secRemaining = framesRemaining / (int)codingFps;
                else
                    secRemaining = 0;

                if (secRemaining > 0)
                    _remainingTime = new TimeSpan(0, 0, (int)secRemaining);

                float fps;
                float.TryParse(result.Groups[2].Value, NumberStyles.Number,
                                _appConfig.CInfo, out fps);
                float encBitrate;
                float.TryParse(result.Groups[4].Value, NumberStyles.Number,
                                _appConfig.CInfo, out encBitrate);

                
                var eventArgs = new EncodeProgressEventArgs
                {
                    AverageFrameRate = codingFps,
                    CurrentFrameRate = fps,
                    CurrentFrame = current,
                    TotalFrames = _frameCount,
                    EstimatedTimeLeft = _remainingTime,
                    PercentComplete = progress,
                    ElapsedTime = elapsedTime,
                };
                InvokeEncodeStatusChanged(eventArgs);
                
            }
            else
                Log.Info($"ffmpeg: {line}");
        }

        private string GenerateCommandLine()
        {
            string[] mbdArray = { "simple", "bits", "rd" };
            string[] cmpArray = { "sad", "sse", "satd", "dct", "psnr", "bit", "rd" };

            var sb = new StringBuilder();
            _encProfile = _currentTask.VideoProfile as Mpeg2VideoProfile;

            if (_encProfile == null) return string.Empty;

            _inputFile = _currentTask.VideoStream.TempFile;
            _outputFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                               _inputFile,
                                                               "encoded.m2v");

            _frameCount = _currentTask.VideoStream.FrameCount;

            #region AviSynth script generation

            var targetSys = _currentTask.EncodingProfile.SystemType;
            int targetHeight;

            var sourceFps = (float)Math.Round(_currentTask.VideoStream.Fps, 3);
            var targetFps = 0f;
            var changeFps = false;

            var sourceAspect = (float)Math.Round(_currentTask.VideoStream.AspectRatio, 3);

            var targetWidth = sourceAspect >= 1.4f ? 1024 : 720;

            if (_currentTask.Input == InputType.InputDvd)
                _currentTask.VideoStream.Width =
                    (int)Math.Round(_currentTask.VideoStream.Height * sourceAspect, 0);

            if (_currentTask.EncodingProfile.OutFormat == OutputType.OutputDvd)
            {
                if (targetSys == 0)
                {
                    targetHeight = 576;
                    if (Math.Abs(sourceFps - 25f) > 0)
                        changeFps = true;
                    targetFps = 25f;
                }
                else
                {
                    targetHeight = 480;
                    if (Math.Abs(sourceFps - 29.970f) > 0 && Math.Abs(sourceFps - 23.976f) > 0)
                        changeFps = true;
                    targetFps = (float)Math.Round(30000f / 1001f, 3);
                }
            }
            else
            {
                targetWidth = _currentTask.EncodingProfile.TargetWidth;
                targetHeight = (int)Math.Floor(targetWidth / sourceAspect);
            }

            var resizeTo = new Size(targetWidth, targetHeight);

            var sub = _currentTask.SubtitleStreams.FirstOrDefault(item => item.HardSubIntoVideo);
            var subFile = string.Empty;
            var keepOnlyForced = false;
            if (sub != null)
            {
                subFile = sub.TempFile;
                keepOnlyForced = sub.KeepOnlyForcedCaptions;
            }

            if (string.IsNullOrEmpty(_currentTask.AviSynthScript))
            {
                var avs = new AviSynthGenerator(_appConfig);
                _currentTask.AviSynthScript = avs.Generate(_currentTask.VideoStream,
                                                                changeFps,
                                                                targetFps,
                                                                resizeTo,
                                                                StereoEncoding.None,
                                                                new StereoVideoInfo(),
                                                                true,
                                                                subFile,
                                                                keepOnlyForced,
                                                                false);
            }

            #endregion

            #region bitrate calculation

            var bitrate = _currentTask.EncodingProfile.TargetFileSize > 1
                ? VideoHelper.CalculateVideoBitrate(_currentTask)
                : _encProfile.Bitrate;

            var audBitrate = _currentTask.AudioStreams.Sum(stream => (int)stream.Bitrate / 1000);
            var maxRate = 9800 - audBitrate;

            #endregion


            _encodingPass = _encProfile.EncodingMode == 0 ? 0 : _currentTask.StreamId;

            var targetSysStr = targetSys == 0 ? "pal" : "ntsc";

            sb.Append($" -i \"{_currentTask.AviSynthScript}\" -map 0:v");
            sb.Append($" -target {targetSysStr}-dvd");
            sb.Append($" -b:v {bitrate:0}k -maxrate {maxRate:0}k -qmin 1");

            sb.Append($" -aspect {sourceAspect:0.000}".ToString(_appConfig.CInfo));

            if (_encodingPass > 0)
            {
                sb.Append($" -pass {_encodingPass:0}");
            }

            if (_encProfile.MbDecision > 0)
            {
                sb.Append($" -mbd {mbdArray[_encProfile.MbDecision]}");
            }

            if (_encProfile.Trellis > 0)
            {
                sb.Append($" -trellis {_encProfile.Trellis:0}");
            }

            if (_encProfile.Cmp > 0)
            {
                sb.Append($" -cmp {cmpArray[_encProfile.Cmp]}");
            }

            if (_encProfile.SubCmp > 0)
            {
                sb.Append($" -subcmp {cmpArray[_encProfile.SubCmp]}");
            }

            if (_encProfile.DcPrecision > 0)
            {
                sb.Append($" -dc {_encProfile.DcPrecision}");
            }

            if (_encProfile.ClosedGops)
            {
                sb.Append(" -flags cgop -mpv_flags strict_gop -sc_threshold 1000000000");
            }

            if (!_encProfile.AutoGop)
            {
                sb.Append($" -g {_encProfile.GopLength:0}");
            }

            sb.Append($" -f rawvideo -y \"{_outputFile}\"");
            return sb.ToString();
        }

        #endregion
    }
}


/* dvd parameters from ffmpeg
        enum { PAL, NTSC, FILM, UNKNOWN } norm = UNKNOWN;
        static const char *const frame_rates[] = { "25", "30000/1001", "24000/1001" }; 

        opt_video_codec(o, "c:v", "mpeg2video");
        opt_audio_codec(o, "c:a", "ac3");
        parse_option(o, "f", "dvd", options);

        parse_option(o, "s", norm == PAL ? "720x576" : "720x480", options);
        parse_option(o, "r", frame_rates[norm], options);
        parse_option(o, "pix_fmt", "yuv420p", options);
        av_dict_set(&o->g->codec_opts, "g", norm == PAL ? "15" : "18", 0);

        av_dict_set(&o->g->codec_opts, "b:v", "6000000", 0);
        av_dict_set(&o->g->codec_opts, "maxrate", "9000000", 0);
        av_dict_set(&o->g->codec_opts, "minrate", "0", 0); // 1500000;
        av_dict_set(&o->g->codec_opts, "bufsize", "1835008", 0); // 224*1024*8;

        av_dict_set(&o->g->format_opts, "packetsize", "2048", 0);  // from www.mpucoder.com: DVD sectors contain 2048 bytes of data, this is also the size of one pack.
        av_dict_set(&o->g->format_opts, "muxrate", "10080000", 0); // from mplex project: data_rate = 1260000. mux_rate = data_rate * 8

        av_dict_set(&o->g->codec_opts, "b:a", "448000", 0);
        parse_option(o, "ar", "48000", options);

*/