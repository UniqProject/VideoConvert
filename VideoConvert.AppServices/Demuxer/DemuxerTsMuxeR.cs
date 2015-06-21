// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DemuxerTsMuxeR.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Demuxer
{
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// The DemuxerTsMuxeR
    /// </summary>
    public class DemuxerTsMuxeR : EncodeBase, IDemuxerTsMuxeR
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (DemuxerTsMuxeR));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "tsMuxer.exe";

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
        private string _outputFile;

        private readonly Regex _tsMuxerRegex = new Regex(@"^.*?([\d\.]+?)% complete.*$",
            RegexOptions.Singleline | RegexOptions.Multiline);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DemuxerTsMuxeR"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public DemuxerTsMuxeR(IAppConfigService appConfig) : base(appConfig)
        {
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the tsMuxer Process
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
                    RedirectStandardOutput = true
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
                    Log.ErrorFormat("tsMuxer exception: {0}", ex);
                }

                if (started)
                {
                    var output = encoder.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"^.*?Network Optix tsMuxeR\.  Version ([\d\.]+?) .*$",
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
                Log.DebugFormat("tsMuxer \"{0}\" found", verInfo);
            }
            return verInfo;
        }

        /// <summary>
        /// Execute a tsMuxer process.
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
                    throw new Exception("tsMuxer is already running");

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(this._appConfig.ToolsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                this.EncodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: tsMuxer {0}", query);

                this.EncodeProcess.Start();

                this._startTime = DateTime.Now;

                this.EncodeProcess.OutputDataReceived += EncodeProcessDataReceived;
                this.EncodeProcess.BeginOutputReadLine();

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

        /// <summary>
        /// The tsMuxer process has exited.
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
                this.EncodeProcess.CancelOutputRead();
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

            var result = _tsMuxerRegex.Match(line);

            if (result.Success)
            {
                float progress;
                Single.TryParse(result.Groups[1].Value, NumberStyles.Number, this._appConfig.CInfo,
                    out progress);

                double processingSpeed = 0f;
                var secRemaining = 0;
                double remaining = 100 - progress;
                var elapsedTime = DateTime.Now.Subtract(this._startTime);

                if (elapsedTime.TotalSeconds > 0)
                    processingSpeed = progress / elapsedTime.TotalSeconds;

                if (processingSpeed > 0)
                    secRemaining = (int)Math.Round(remaining / processingSpeed, MidpointRounding.ToEven);

                var remainingTime = new TimeSpan(0, 0, secRemaining);

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
                Log.InfoFormat("tsMuxer: {0}", line);
        }

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();
            var meta = new StringBuilder();

            this._outputFile = this._appConfig.DemuxLocation;

            meta.Append("MUXOPT --no-pcr-on-video-pid ");

            if (this._appConfig.TSMuxeRBlurayAudioPES)
                meta.Append("--new-audio-pes ");

            meta.AppendLine("--vbr --demux --vbv-len=500");

            var vidStream = this._currentTask.VideoStream.DemuxStreamId;
            var fps = this._currentTask.VideoStream.Fps;

            var inFile = string.Format("\"{0}\"", this._currentTask.InputFile);

            string codec;
            var streamExt = string.Empty;

            switch (this._currentTask.VideoStream.Format)
            {
                case "VC-1":
                    codec = "V_MS/VFW/WVC1";
                    streamExt = "vc1";
                    break;
                case "AVC":
                    codec = "V_MPEG4/ISO/AVC";
                    streamExt = "264";
                    break;
                case "MPEG Video":
                case "MPEG-2":
                    codec = "V_MPEG-2";
                    streamExt = "mpv";
                    break;
                default:
                    codec = string.Empty;
                    break;
            }

            if (!string.IsNullOrEmpty(codec))
            {
                if (codec != "V_MPEG-2")
                {
                    meta.AppendFormat(this._appConfig.CInfo, "{0}, {1}, fps={2:#.###}, insertSEI, contSPS, ", 
                                      codec, inFile,fps);

                    meta.AppendFormat(this._appConfig.CInfo, "track={0:g}, lang={1}", vidStream, "und");
                }
                else
                {
                    meta.AppendFormat(this._appConfig.CInfo, "{0}, {1}, fps={2:#.###}, track={3:g}, lang={4}", codec,
                                      inFile, this._currentTask.VideoStream.Fps, vidStream, "und");
                }

                this._currentTask.VideoStream.TempFile = string.Format("{0}.track_{1:0000}.{2}",
                                                                       Path.GetFileNameWithoutExtension(this._currentTask.InputFile),
                                                                       this._currentTask.VideoStream.DemuxStreamId, 
                                                                       streamExt);
                this._currentTask.VideoStream.TempFile = Path.Combine(this._appConfig.DemuxLocation,
                                                                      this._currentTask.VideoStream.TempFile);
                meta.AppendLine();
            }

            if (this._currentTask.StereoVideoStream.DemuxRightStreamId > -1
                && this._currentTask.EncodingProfile.StereoType != StereoEncoding.None)
            {
                meta.AppendFormat(this._appConfig.CInfo, "V_MPEG4/ISO/MVC, {0}, fps={1:#.###}, insertSEI, contSPS, ",
                                  inFile, fps);
                meta.AppendFormat(this._appConfig.CInfo, "track={0:g}, lang={1}",
                                  this._currentTask.StereoVideoStream.DemuxRightStreamId, "und");
                meta.AppendLine();

                this._currentTask.StereoVideoStream.LeftTempFile = this._currentTask.VideoStream.TempFile;

                this._currentTask.StereoVideoStream.RightTempFile = string.Format("{0}.track_{1:0000}.mvc",
                                                                                  Path.GetFileNameWithoutExtension(this._currentTask.InputFile),
                                                                                  this._currentTask.StereoVideoStream.DemuxRightStreamId);

                this._currentTask.StereoVideoStream.RightTempFile = Path.Combine(this._appConfig.DemuxLocation,
                                                                                 this._currentTask.StereoVideoStream.RightTempFile);
            }

            foreach (var item in this._currentTask.AudioStreams)
            {
                var itemlang = item.LangCode;
                if ((itemlang == "xx") || (string.IsNullOrEmpty(itemlang)))
                    itemlang = "und";

                switch (item.Format.ToLower())
                {
                    case "pcm":
                        codec = "A_LPCM";
                        streamExt = "lpcm";
                        break;
                    case "ac3":
                    case "ac-3":
                    case "eac3":
                    case "eac-3":
                    case "e-ac-3":
                    case "e-ac3":
                    case "ac3-ex":
                    case "truehd":
                    case "true-hd":
                    case "true hd":
                        codec = "A_AC3";
                        streamExt = "ac3";
                        break;
                    case "dts":
                    case "dts-hd":
                    case "dts-hd hr":
                    case "dts-hd ma":
                        codec = "A_DTS";
                        streamExt = "dts";
                        break;
                    case "mpeg audio":
                        codec = "A_MP3";
                        streamExt = "mp3";
                        break;
                    default:
                        continue;
                }

                var delayString = string.Empty;

                if (item.Delay != 0)
                    delayString = string.Format(this._appConfig.CInfo, "timeshift={0:#}ms,", item.Delay);

                meta.AppendFormat("{0}, {1}, {2} track={3}, lang={4}", codec, inFile, delayString, item.DemuxStreamId, itemlang);
                meta.AppendLine();
                item.TempFile = string.Format("{0}.track_{1:0000}.{2}",
                                              Path.GetFileNameWithoutExtension(this._currentTask.InputFile),
                                              item.DemuxStreamId,
                                              streamExt);
                item.TempFile = Path.Combine(this._appConfig.DemuxLocation,
                                             item.TempFile);
            }

            foreach (var item in this._currentTask.SubtitleStreams)
            {
                var itemlang = item.LangCode;
                if ((itemlang == "xx") || (string.IsNullOrEmpty(itemlang)))
                    itemlang = "und";

                switch (item.Format.ToLower())
                {
                    case "pgs":
                        codec = "S_HDMV/PGS";
                        break;
                    case "utf-8":
                        codec = "S_TEXT/UTF8";
                        break;

                    default:
                        continue;
                }


                var delayString = string.Empty;
                if (item.Delay != int.MinValue)
                    delayString = string.Format(this._appConfig.CInfo, "timeshift={0:#}ms,", item.Delay);

                if (codec == "S_TEXT/UTF8")
                {
                    meta.AppendFormat(this._appConfig.CInfo,
                        "{0}, {1},{2}font-name=\"{3}\",font-size={4:#},font-color={5},bottom-offset={6:g}," +
                        "font-border={7:g},text-align=center,video-width={8:g},video-height={9:g},fps={10:#.###}, track={11:g}, lang={12}",
                        codec, inFile, delayString, this._appConfig.TSMuxeRSubtitleFont,
                        this._appConfig.TSMuxeRSubtitleFontSize,
                        this._appConfig.TSMuxeRSubtitleColor.Replace("#", "0x"),
                        this._appConfig.TSMuxeRBottomOffset, this._appConfig.TSMuxerSubtitleAdditionalBorder,
                        this._currentTask.VideoStream.Width, this._currentTask.VideoStream.Height, fps, item.DemuxStreamId,
                        itemlang);
                    streamExt = "srt";
                }
                else
                {
                    meta.AppendFormat(this._appConfig.CInfo,
                        "{0}, {1},{2}bottom-offset={3:g},font-border={4:g},text-align=center,video-width={5:g}," +
                        "video-height={6:g},fps={7:#.###}, track={8:g}, lang={9}", codec, inFile, delayString,
                        this._appConfig.TSMuxeRBottomOffset, this._appConfig.TSMuxerSubtitleAdditionalBorder,
                        this._currentTask.VideoStream.Width, this._currentTask.VideoStream.Height, fps, item.DemuxStreamId,
                        itemlang);
                    streamExt = "sup";
                }

                meta.AppendLine();

                item.TempFile = string.Format("{0}.track_{1:0000}.{2}",
                                              Path.GetFileNameWithoutExtension(this._currentTask.InputFile),
                                              item.DemuxStreamId,
                                              streamExt);
                item.TempFile = Path.Combine(this._appConfig.DemuxLocation,
                                             item.TempFile);
            }

            this._inputFile = FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation, "meta");
            using (var sw = new StreamWriter(this._inputFile))
                sw.WriteLine(meta.ToString());

            Log.InfoFormat("tsMuxeR Meta: \r\n{0:s}", meta);

            sb.AppendFormat("\"{0}\" \"{1}\"", this._inputFile, this._outputFile);

            return sb.ToString();
        }

        #endregion
    }
}