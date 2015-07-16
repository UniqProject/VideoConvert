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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using log4net;
    using VideoConvert.AppServices.Muxer.Interfaces;
    using VideoConvert.AppServices.Services.Base;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;
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
            _appConfig = appConfig;
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
                    Log.Error($"tsmuxer exception: {ex}");
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
                Log.Debug($"tsmuxer \"{verInfo}\" found");
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
                if (IsEncoding)
                    throw new Exception("tsmuxer is already running");

                IsEncoding = true;
                _currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(_appConfig.ToolsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = _appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                EncodeProcess = new Process {StartInfo = cliStart};
                Log.Info($"start parameter: tsmuxer {query}");

                EncodeProcess.Start();

                _startTime = DateTime.Now;

                EncodeProcess.OutputDataReceived += EncodeProcessDataReceived;
                EncodeProcess.BeginOutputReadLine();

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
                EncodeProcess.CancelOutputRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            _currentTask.ExitCode = EncodeProcess.ExitCode;
            Log.Info($"Exit Code: {_currentTask.ExitCode:0}");

            if (_currentTask.ExitCode == 0)
            {
                _currentTask.TempFiles.Add(_currentTask.VideoStream.TempFile);
                foreach (var item in _currentTask.AudioStreams)
                    _currentTask.TempFiles.Add(item.TempFile);
                foreach (var item in _currentTask.SubtitleStreams)
                    _currentTask.TempFiles.Add(item.TempFile);
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

            var result = _muxRegex.Match(line);

            if (result.Success)
            {
                float progress;
                float.TryParse(result.Groups[1].Value, NumberStyles.Number, _appConfig.CInfo,
                    out progress);

                double processingSpeed = 0f;
                var secRemaining = 0;
                double remaining = 100 - progress;
                var elapsedTime = DateTime.Now.Subtract(_startTime);

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
                InvokeEncodeStatusChanged(eventArgs);
            }
            else
                Log.Info($"tsmuxer: {line}");
        }

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();
            var meta = new StringBuilder();

            _outputFile = !string.IsNullOrEmpty(_currentTask.TempOutput)
                                ? _currentTask.TempOutput
                                : _currentTask.OutputFile;

            var vidStream = 0;
            string codec;

            meta.Append("MUXOPT --no-pcr-on-video-pid ");

            if (_appConfig.TSMuxeRBlurayAudioPES)
                meta.Append("--new-audio-pes ");

            meta.Append("--vbr ");

            switch (_currentTask.EncodingProfile.OutFormat)
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

            switch (_currentTask.EncodingProfile.OutFormat)
            {
                case OutputType.OutputBluRay:
                case OutputType.OutputAvchd:
                    var targetSize = VideoHelper.GetVideoDimensions(_currentTask.VideoStream.PicSize,
                                                                     _currentTask.VideoStream.AspectRatio,
                                                                     _currentTask.EncodingProfile.OutFormat);
                    if (_currentTask.VideoStream.Width < targetSize.Width ||
                        _currentTask.VideoStream.Height < targetSize.Height)
                        meta.Append("--insertBlankPL ");
                    break;
            }

            if (_currentTask.Chapters.Count > 1)
            {
                var chapTimes = new List<string>();
                var actualTime = new TimeSpan();
                var isDvd = _currentTask.Input == InputType.InputDvd;
                
                foreach (var chapter in _currentTask.Chapters)
                {
                    actualTime = isDvd ? actualTime.Add(chapter) : chapter;
                    var dt = DateTime.MinValue.Add(actualTime);
                    chapTimes.Add(dt.ToString("H:mm:ss.fff"));
                }

                meta.Append($"--custom-chapters={string.Join("; ", chapTimes)} ");
            }

            meta.AppendLine("--vbv-len=500");

            var sourceVidCodec = _currentTask.VideoStream.Format;
            switch (_currentTask.Input)
            {
                case InputType.InputAvi:
                case InputType.InputMp4:
                case InputType.InputMatroska:
                case InputType.InputTs:
                case InputType.InputWm:
                case InputType.InputFlash:
                    vidStream = _currentTask.VideoStream.StreamId;
                    break;

                case InputType.InputDvd:
                    vidStream = 1;
                    break;
                case InputType.InputBluRay:
                case InputType.InputAvchd:
                case InputType.InputHddvd:
                    vidStream = _currentTask.VideoStream.IsRawStream ? 0 : _currentTask.VideoStream.StreamId;
                    break;
            }

            var fps = _currentTask.VideoStream.FrameMode.Trim().ToLowerInvariant() == "frame doubling"
                        ? _currentTask.VideoStream.Fps * 2
                        : _currentTask.VideoStream.Fps;

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
                    codec = _currentTask.VideoStream.FormatProfile == "Version 2" ? "V_MPEG-2" : string.Empty;
                    break;
                default:
                    codec = string.Empty;
                    break;
            }

            var inFile = $"\"{_currentTask.VideoStream.TempFile}\"";

            if (!string.IsNullOrEmpty(codec))
            {
                meta.Append($"{codec}, {inFile}, fps={fps:#.###}, ".ToString(_appConfig.CInfo));

                if (codec != "V_MPEG-2")
                    meta.Append("insertSEI, ");

                meta.Append($"track={vidStream:0}, lang=und");
                meta.AppendLine();
            }

            foreach (var item in _currentTask.AudioStreams)
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
                    delayString = $"timeshift={item.Delay:0}ms,";

                inFile = item.TempFile;

                meta.Append($"{codec}, {inFile}, {delayString} track=1, lang={itemlang}");
                meta.AppendLine();
            }

            foreach (var item in _currentTask.SubtitleStreams)
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
                    tempFile = $"\"{item.TempFile}\"";
                    subId = 1;
                }

                var delayString = string.Empty;
                if (item.Delay != int.MinValue)
                    delayString = $"timeshift={item.Delay:0}ms,";

                meta.Append($"{codec}, {tempFile},{delayString}");

                if (codec == "S_TEXT/UTF8")
                {
                    meta.Append($"font-name=\"{_appConfig.TSMuxeRSubtitleFont}\",font-size={_appConfig.TSMuxeRSubtitleFontSize:0},");
                    meta.Append($"font-color={_appConfig.TSMuxeRSubtitleColor.Replace("#", "0x")},bottom-offset={_appConfig.TSMuxeRBottomOffset:0},");
                    meta.Append($"font-border={_appConfig.TSMuxerSubtitleAdditionalBorder:0},text-align=center,");
                    meta.Append($"video-width={_currentTask.VideoStream.Width:0},video-height={_currentTask.VideoStream.Height:0},");
                }

                meta.Append($"fps={fps:#.###}, track={subId:0}, lang={itemlang}".ToString(_appConfig.CInfo));
                
                meta.AppendLine();
            }

            _inputFile = FileSystemHelper.CreateTempFile(_appConfig.TempPath, "meta");
            using(var sw = new StreamWriter(_inputFile))
                sw.WriteLine(meta.ToString());

            Log.Info($"tsMuxeR Meta: {Environment.NewLine}{meta}");

            _currentTask.TempFiles.Add(_inputFile);

            sb.Append($"\"{_inputFile}\" \"{_outputFile}\"");

            return sb.ToString();
        }

        #endregion
    }
}