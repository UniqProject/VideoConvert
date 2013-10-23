// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderFfmpegDVD.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using Interop.Model.Profiles;
    using Interop.Utilities;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using Utilities;

    /// <summary>
    /// The EncoderFfmpegDVD
    /// </summary>
    public class EncoderFfmpegDVD : EncodeBase, IEncoderFfmpegDVD
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (EncoderFfmpegDVD));

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
        /// Initializes a new instance of the <see cref="EncoderFfmpegDVD"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public EncoderFfmpegDVD(IAppConfigService appConfig) : base(appConfig)
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
        /// <returns>Encoder version</returns>
        public static string GetVersionInfo(string encPath)
        {
            string verInfo = string.Empty;

            string localExecutable = Path.Combine(encPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
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
                    string output = encoder.StandardError.ReadToEnd();
                    Regex regObj = new Regex(@"^.*ffmpeg version ([\w\d\.\-_]+)[, ].*$",
                        RegexOptions.Singleline | RegexOptions.Multiline);
                    Match result = regObj.Match(output);
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

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                string query = GenerateCommandLine();
                string cliPath = Path.Combine(this._appConfig.ToolsPath, Executable);

                ProcessStartInfo cliStart = new ProcessStartInfo(cliPath, query)
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
            string[] mbdArray = {"simple", "bits", "rd"};
            string[] cmpArray = {"sad", "sse", "satd", "dct", "psnr", "bit", "rd"};

            StringBuilder sb = new StringBuilder();
            this._encProfile = this._currentTask.VideoProfile as Mpeg2VideoProfile;

            if (this._encProfile == null) return string.Empty;

            this._inputFile = this._currentTask.VideoStream.TempFile;
            this._outputFile = FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation, 
                                                               this._inputFile,
                                                               "encoded.m2v");

            this._frameCount = this._currentTask.VideoStream.FrameCount;

            #region AviSynth script generation

            int targetSys = this._currentTask.EncodingProfile.SystemType;
            int targetHeight;

            float sourceFPS = (float)Math.Round(this._currentTask.VideoStream.FPS, 3);
            float targetFPS = 0f;
            bool changeFPS = false;

            float sourceAspect = (float)Math.Round(this._currentTask.VideoStream.AspectRatio, 3);

            int targetWidth = sourceAspect >= 1.4f ? 1024 : 720;

            if (this._currentTask.Input == InputType.InputDvd)
                this._currentTask.VideoStream.Width =
                    (int)Math.Round(this._currentTask.VideoStream.Height * sourceAspect, 0);

            if (this._currentTask.EncodingProfile.OutFormat == OutputType.OutputDvd)
            {
                if (targetSys == 0)
                {
                    targetHeight = 576;
                    if (Math.Abs(sourceFPS - 25f) > 0)
                        changeFPS = true;
                    targetFPS = 25f;
                }
                else
                {
                    targetHeight = 480;
                    if (Math.Abs(sourceFPS - 29.970f) > 0 && Math.Abs(sourceFPS - 23.976f) > 0)
                        changeFPS = true;
                    targetFPS = (float)Math.Round(30000f / 1001f, 3);
                }
            }
            else
            {
                targetWidth = this._currentTask.EncodingProfile.TargetWidth;
                targetHeight = (int)Math.Floor(targetWidth / sourceAspect);
            }

            Size resizeTo = new Size(targetWidth, targetHeight);

            SubtitleInfo sub = this._currentTask.SubtitleStreams.FirstOrDefault(item => item.HardSubIntoVideo);
            string subFile = string.Empty;
            bool keepOnlyForced = false;
            if (sub != null)
            {
                subFile = sub.TempFile;
                keepOnlyForced = sub.KeepOnlyForcedCaptions;
            }

            if (string.IsNullOrEmpty(this._currentTask.AviSynthScript))
            {
                AviSynthGenerator avs = new AviSynthGenerator(this._appConfig);
                this._currentTask.AviSynthScript = avs.Generate(this._currentTask.VideoStream,
                                                                changeFPS,
                                                                targetFPS,
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

            int bitrate = this._currentTask.EncodingProfile.TargetFileSize > 1
                ? VideoHelper.CalculateVideoBitrate(this._currentTask)
                : this._encProfile.Bitrate;

            int audBitrate = this._currentTask.AudioStreams.Sum(stream => (int)stream.Bitrate / 1000);
            int maxRate = 9800 - audBitrate;

            #endregion

            
            this._encodingPass = this._encProfile.EncodingMode == 0 ? 0 : this._currentTask.StreamId;

            sb.AppendFormat(" -i \"{0}\" -map 0:v", this._currentTask.AviSynthScript);
            sb.AppendFormat(" -target {0}-dvd", targetSys == 0 ? "pal" : "ntsc");
            sb.AppendFormat(" -b:v {0:0}k -maxrate {1:0}k -qmin 1", bitrate, maxRate);

            sb.AppendFormat(this._appConfig.CInfo, " -aspect {0:0.000}", sourceAspect);

            if (this._encodingPass > 0)
            {
                sb.AppendFormat(" -pass {0:0}", this._encodingPass);
            }

            if (this._encProfile.MBDecision > 0)
            {
                sb.AppendFormat(" -mbd {0}", mbdArray[this._encProfile.MBDecision]);
            }

            if (this._encProfile.Trellis > 0)
            {
                sb.AppendFormat(" -trellis {0:0}", this._encProfile.Trellis);
            }

            if (this._encProfile.CMP > 0)
            {
                sb.AppendFormat(" -cmp {0}", cmpArray[this._encProfile.CMP]);
            }

            if (this._encProfile.SubCMP > 0)
            {
                sb.AppendFormat(" -subcmp {0}", cmpArray[this._encProfile.SubCMP]);
            }

            if (this._encProfile.DCPrecision > 0)
            {
                sb.AppendFormat(" -dc {0}", this._encProfile.DCPrecision);
            }

            if (this._encProfile.ClosedGops)
            {
                sb.Append(" -flags cgop -mpv_flags strict_gop -sc_threshold 1000000000");
            }

            if (!this._encProfile.AutoGOP)
            {
                sb.AppendFormat(" -g {0:0}", this._encProfile.GopLength);
            }

            sb.AppendFormat(" -f rawvideo -y \"{0}\"", _outputFile);
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
                if (this._encProfile.EncodingMode == 0 ||
                    (this._encProfile.EncodingMode == 1 && this._encodingPass == 2))
                {
                    this._currentTask.VideoStream.IsRawStream = false;
                    this._currentTask.VideoStream.Encoded = true;

                    this._currentTask.TempFiles.Add(_inputFile);
                    this._currentTask.VideoStream.TempFile = _outputFile;
                    this._currentTask.TempFiles.Add(this._currentTask.AviSynthScript);

                    try
                    {
                        this._currentTask.MediaInfo = GenHelper.GetMediaInfo(this._currentTask.VideoStream.TempFile);
                    }
                    catch (TimeoutException ex)
                    {
                        Log.Error(ex);
                    }

                    this._currentTask.VideoStream = VideoHelper.GetStreamInfo(this._currentTask.MediaInfo,
                                                    this._currentTask.VideoStream,
                                                    this._currentTask.EncodingProfile.OutFormat == OutputType.OutputBluRay);
                    this._currentTask.TempFiles.Add(this._currentTask.FfIndexFile);
                }
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

            TimeSpan elapsedTime = DateTime.Now.Subtract(_startTime);

            Match result = _frameReg.Match(line);
            // groups:
            // 1: actual frame
            // 2: fps
            // 3: position in stream (time)
            // 4: bitrate
            if (result.Success)
            {
                long current;
                Int64.TryParse(result.Groups[1].Value, NumberStyles.Number,
                               _appConfig.CInfo, out current);

                long framesRemaining = _frameCount - current;

                float progress = ((float)current / _frameCount) * 100;

                float codingFPS = 0f;
                if (elapsedTime.Seconds != 0) // prevent division by zero
                {
                    //Frames per Second
                    codingFPS = (float)Math.Round(current / elapsedTime.TotalSeconds, 2);
                }

                long secRemaining;
                if (codingFPS > 1) // prevent another division by zero
                    secRemaining = framesRemaining / (int)codingFPS;
                else
                    secRemaining = 0;

                if (secRemaining > 0)
                    this._remainingTime = new TimeSpan(0, 0, (int)secRemaining);

                float fps = 0f;
                Single.TryParse(result.Groups[2].Value, NumberStyles.Number,
                                _appConfig.CInfo, out fps);
                float encBitrate;
                Single.TryParse(result.Groups[4].Value, NumberStyles.Number,
                                _appConfig.CInfo, out encBitrate);

                
                EncodeProgressEventArgs eventArgs = new EncodeProgressEventArgs
                {
                    AverageFrameRate = codingFPS,
                    CurrentFrameRate = fps,
                    EstimatedTimeLeft = this._remainingTime,
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