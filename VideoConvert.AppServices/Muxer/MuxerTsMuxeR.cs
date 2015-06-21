// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MuxerTsMuxeR.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Muxer
{
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// The MuxerTsMuxeR
    /// </summary>
    public class MuxerTsMuxeR : EncodeBase, IMuxerTsMuxeR
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (MuxerTsMuxeR));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "tsmuxer.exe";

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

        private readonly Regex _muxRegex = new Regex(@"^.*?([\d\.]+?)% complete.*$",
            RegexOptions.Singleline | RegexOptions.Multiline);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerTsMuxeR"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public MuxerTsMuxeR(IAppConfigService appConfig) : base(appConfig)
        {
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the tsmuxer Process
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
                    Log.ErrorFormat("tsmuxer exception: {0}", ex);
                }

                if (started)
                {
                    var output = encoder.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"^.*?Network Optix tsMuxeR\.  Version ([\d\.]+?)\. .*$",
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
                Log.DebugFormat("tsmuxer \"{0}\" found", verInfo);
            }
            return verInfo;
        }

        /// <summary>
        /// Execute a tsmuxer process.
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
                    throw new Exception("tsmuxer is already running");

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
                Log.InfoFormat("start parameter: tsmuxer {0}", query);

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
        /// The tsmuxer process has exited.
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
                this._currentTask.TempFiles.Add(this._currentTask.VideoStream.TempFile);
                foreach (var item in this._currentTask.AudioStreams)
                    this._currentTask.TempFiles.Add(item.TempFile);
                foreach (var item in this._currentTask.SubtitleStreams)
                    this._currentTask.TempFiles.Add(item.TempFile);
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

            var result = _muxRegex.Match(line);

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
                Log.InfoFormat("tsmuxer: {0}", line);
        }

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();
            var meta = new StringBuilder();

            this._outputFile = !string.IsNullOrEmpty(this._currentTask.TempOutput)
                                ? this._currentTask.TempOutput
                                : this._currentTask.OutputFile;

            var vidStream = 0;
            string codec;

            meta.Append("MUXOPT --no-pcr-on-video-pid ");

            if (this._appConfig.TSMuxeRBlurayAudioPES)
                meta.Append("--new-audio-pes ");

            meta.Append("--vbr ");

            switch (this._currentTask.EncodingProfile.OutFormat)
            {
                case OutputType.OutputM2Ts:
                case OutputType.OutputTs:
                    break;
                case OutputType.OutputBluRay:
                    meta.Append("--blu-ray ");
                    break;
                case OutputType.OutputAvchd:
                    meta.Append("--avchd ");
                    break;
            }

            switch (this._currentTask.EncodingProfile.OutFormat)
            {
                case OutputType.OutputBluRay:
                case OutputType.OutputAvchd:
                    var targetSize = VideoHelper.GetVideoDimensions(this._currentTask.VideoStream.PicSize,
                                                                     this._currentTask.VideoStream.AspectRatio,
                                                                     this._currentTask.EncodingProfile.OutFormat);
                    if (this._currentTask.VideoStream.Width < targetSize.Width ||
                        this._currentTask.VideoStream.Height < targetSize.Height)
                        meta.Append("--insertBlankPL ");
                    break;
            }

            if (this._currentTask.Chapters.Count > 1)
            {
                var chapTimes = new List<string>();
                var actualTime = new TimeSpan();
                var isDvd = this._currentTask.Input == InputType.InputDvd;
                
                foreach (var chapter in this._currentTask.Chapters)
                {
                    actualTime = isDvd ? actualTime.Add(chapter) : chapter;
                    var dt = DateTime.MinValue.Add(actualTime);
                    chapTimes.Add(dt.ToString("H:mm:ss.fff"));
                }

                meta.AppendFormat("--custom-chapters={0} ", string.Join(";", chapTimes));
            }

            meta.AppendLine("--vbv-len=500");

            var sourceVidCodec = this._currentTask.VideoStream.Format;
            switch (this._currentTask.Input)
            {
                case InputType.InputAvi:
                case InputType.InputMp4:
                case InputType.InputMatroska:
                case InputType.InputTs:
                case InputType.InputWm:
                case InputType.InputFlash:
                    vidStream = this._currentTask.VideoStream.StreamId;
                    break;

                case InputType.InputDvd:
                    vidStream = 1;
                    break;
                case InputType.InputBluRay:
                case InputType.InputAvchd:
                case InputType.InputHddvd:
                    vidStream = this._currentTask.VideoStream.IsRawStream ? 0 : this._currentTask.VideoStream.StreamId;
                    break;
            }

            var fps = this._currentTask.VideoStream.FrameMode.Trim().ToLowerInvariant() == "frame doubling"
                        ? this._currentTask.VideoStream.Fps * 2
                        : this._currentTask.VideoStream.Fps;

            switch (sourceVidCodec)
            {
                case "VC-1":
                    codec = "V_MS/VFW/WVC1";
                    break;
                case "AVC":
                    codec = "V_MPEG4/ISO/AVC";
                    break;
                case "MPEG Video":
                case "MPEG-2":
                    codec = this._currentTask.VideoStream.FormatProfile == "Version 2" ? "V_MPEG-2" : string.Empty;
                    break;
                default:
                    codec = string.Empty;
                    break;
            }

            var inFile = string.Format("\"{0}\"", this._currentTask.VideoStream.TempFile);

            if (!string.IsNullOrEmpty(codec))
            {
                if (codec != "V_MPEG-2")
                {
                    meta.AppendFormat(this._appConfig.CInfo, "{0}, {1}, fps={2:#.###}, insertSEI, contSPS, ", codec,
                                        inFile,
                                        fps);

                    meta.AppendFormat(this._appConfig.CInfo, "track={0:g}, lang={1}", vidStream, "und");
                }
                else
                {
                    meta.AppendFormat(this._appConfig.CInfo, "{0}, {1}, fps={2:#.###}, track={3:g}, lang={4}", codec,
                                      inFile, this._currentTask.VideoStream.Fps, vidStream, "und");
                }
                meta.AppendLine();
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
                        break;
                    case "dts":
                    case "dts-hd":
                    case "dts-hd hr":
                    case "dts-hd ma":
                        codec = "A_DTS";
                        break;
                    case "mpeg audio":
                        codec = "A_MP3";
                        break;
                    default:
                        continue;
                }

                var delayString = string.Empty;

                if (item.Delay != 0)
                    delayString = string.Format(this._appConfig.CInfo, "timeshift={0:#}ms,", item.Delay);

                inFile = item.TempFile;

                meta.AppendFormat("{0}, {1}, {2} track=1, lang={3}", codec, inFile, delayString, itemlang);
                meta.AppendLine();
            }

            foreach (var item in this._currentTask.SubtitleStreams)
            {
                if (item.HardSubIntoVideo || !File.Exists(item.TempFile)) continue;

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

                var tempFile = string.Empty;
                var subId = -1;

                if (!string.IsNullOrEmpty(item.TempFile))
                {
                    tempFile = string.Format("\"{0}\"", item.TempFile);
                    subId = 1;
                }

                var delayString = string.Empty;
                if (item.Delay != int.MinValue)
                    delayString = string.Format(this._appConfig.CInfo, "timeshift={0:#}ms,", item.Delay);

                if (codec == "S_TEXT/UTF8")
                    meta.AppendFormat(this._appConfig.CInfo,
                        "{0}, {1},{2}font-name=\"{3}\",font-size={4:#},font-color={5},bottom-offset={6:g}," +
                        "font-border={7:g},text-align=center,video-width={8:g},video-height={9:g},fps={10:#.###}, track={11:g}, lang={12}",
                        codec, tempFile, delayString, this._appConfig.TSMuxeRSubtitleFont,
                        this._appConfig.TSMuxeRSubtitleFontSize,
                        this._appConfig.TSMuxeRSubtitleColor.Replace("#","0x"),
                        this._appConfig.TSMuxeRBottomOffset, this._appConfig.TSMuxerSubtitleAdditionalBorder,
                        this._currentTask.VideoStream.Width, this._currentTask.VideoStream.Height, fps, subId,
                        itemlang);
                else
                    meta.AppendFormat(this._appConfig.CInfo,
                        "{0}, {1},{2}bottom-offset={3:g},font-border={4:g},text-align=center,video-width={5:g}," +
                        "video-height={6:g},fps={7:#.###}, track={8:g}, lang={9}", codec, tempFile, delayString,
                        this._appConfig.TSMuxeRBottomOffset, this._appConfig.TSMuxerSubtitleAdditionalBorder,
                        this._currentTask.VideoStream.Width, this._currentTask.VideoStream.Height, fps, subId,
                        itemlang);

                meta.AppendLine();
            }

            this._inputFile = FileSystemHelper.CreateTempFile(this._appConfig.TempPath, "meta");
            using(var sw = new StreamWriter(this._inputFile))
                sw.WriteLine(meta.ToString());

            Log.InfoFormat("tsMuxeR Meta: \r\n{0:s}", meta);

            this._currentTask.TempFiles.Add(this._inputFile);

            sb.AppendFormat("\"{0}\" \"{1}\"", this._inputFile, this._outputFile);

            return sb.ToString();
        }

        #endregion
    }
}