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
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using log4net;
    using VideoConvert.AppServices.Demuxer.Interfaces;
    using VideoConvert.AppServices.Services.Base;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;
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
            _appConfig = appConfig;
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
                    Log.Error($"tsMuxer exception: {ex}");
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
                Log.Debug($"tsMuxer \"{verInfo}\" found");
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
                if (IsEncoding)
                    throw new Exception("tsMuxer is already running");

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
                Log.Info($"start parameter: tsMuxer {query}");

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
                _currentTask.TempFiles.Add(_inputFile);
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

            var result = _tsMuxerRegex.Match(line);

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
                Log.Info($"tsMuxer: {line}");
        }

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();
            var meta = new StringBuilder();

            _outputFile = _appConfig.DemuxLocation;

            meta.Append("MUXOPT --no-pcr-on-video-pid ");

            if (_appConfig.TSMuxeRBlurayAudioPES)
                meta.Append("--new-audio-pes ");

            meta.AppendLine("--vbr --demux --vbv-len=500");

            var vidStream = _currentTask.VideoStream.DemuxStreamId;
            var fps = _currentTask.VideoStream.Fps;

            var inFile = $"\"{_currentTask.InputFile}\"";

            string codec;
            var streamExt = string.Empty;

            switch (_currentTask.VideoStream.Format)
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
                    meta.Append($"{codec}, {inFile}, fps={fps:#.###}, insertSEI, ".ToString(_appConfig.CInfo));
                    meta.Append($"track={vidStream:0}, lang=und");
                }
                else
                {
                    meta.Append($"{codec}, {inFile}, fps={fps:#.###}, ".ToString(_appConfig.CInfo));
                    meta.Append($"track={vidStream:0}, lang=und");
                }

                _currentTask.VideoStream.TempFile =
                    $"{Path.GetFileNameWithoutExtension(_currentTask.InputFile)}.track_{_currentTask.VideoStream.DemuxStreamId:0000}.{streamExt}";
                _currentTask.VideoStream.TempFile = Path.Combine(_appConfig.DemuxLocation,
                                                                      _currentTask.VideoStream.TempFile);
                meta.AppendLine();
            }

            if (_currentTask.StereoVideoStream.DemuxRightStreamId > -1
                && _currentTask.EncodingProfile.StereoType != StereoEncoding.None)
            {
                meta.Append($"V_MPEG4/ISO/MVC, {inFile}, fps={fps:#.###}, insertSEI, ".ToString(_appConfig.CInfo));
                meta.Append($"track={_currentTask.StereoVideoStream.DemuxRightStreamId:0}, lang=und");
                meta.AppendLine();

                _currentTask.StereoVideoStream.LeftTempFile = _currentTask.VideoStream.TempFile;

                _currentTask.StereoVideoStream.RightTempFile =
                    $"{Path.GetFileNameWithoutExtension(_currentTask.InputFile)}.track_{_currentTask.StereoVideoStream.DemuxRightStreamId:0000}.mvc";

                _currentTask.StereoVideoStream.RightTempFile = Path.Combine(_appConfig.DemuxLocation,
                                                                                 _currentTask.StereoVideoStream.RightTempFile);
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
                    delayString = $"timeshift={item.Delay:0}ms,";

                meta.Append($"{codec}, {inFile}, {delayString} track={item.DemuxStreamId:0}, lang={itemlang}");
                meta.AppendLine();
                item.TempFile =
                    $"{Path.GetFileNameWithoutExtension(_currentTask.InputFile)}.track_{item.DemuxStreamId:0000}.{streamExt}";
                item.TempFile = Path.Combine(_appConfig.DemuxLocation,
                                             item.TempFile);
            }

            foreach (var item in _currentTask.SubtitleStreams)
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
                    delayString = $" timeshift={item.Delay:0}ms, ";

                if (codec == "S_TEXT/UTF8")
                {
                    meta.Append($"{codec}, {inFile},{delayString}font-name=\"{_appConfig.TSMuxeRSubtitleFont}\",");
                    meta.Append($"font-size={_appConfig.TSMuxeRSubtitleFontSize:0},font-color={_appConfig.TSMuxeRSubtitleColor.Replace("#", "0x")},");
                    meta.Append($"bottom-offset={_appConfig.TSMuxeRBottomOffset:0},font-border={_appConfig.TSMuxerSubtitleAdditionalBorder:0},");
                    meta.Append($"text-align=center,video-width={_currentTask.VideoStream.Width:0},video-height={_currentTask.VideoStream.Height:0},");
                    meta.Append($"fps={fps:#.###}, ".ToString(_appConfig.CInfo));
                    meta.Append($"track={item.DemuxStreamId:0}, lang={itemlang}");
                    streamExt = "srt";
                }
                else
                {
                    meta.Append($"{codec}, {inFile},{delayString}");
                    meta.Append($"fps={fps:#.###}, ".ToString(_appConfig.CInfo));
                    meta.Append($"track={item.DemuxStreamId:0}, lang={itemlang}");
                    streamExt = "sup";
                }

                meta.AppendLine();

                item.TempFile =
                    $"{Path.GetFileNameWithoutExtension(_currentTask.InputFile)}.track_{item.DemuxStreamId:0000}.{streamExt}";
                item.TempFile = Path.Combine(_appConfig.DemuxLocation,
                                             item.TempFile);
            }

            _inputFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation, "meta");
            using (var sw = new StreamWriter(_inputFile))
                sw.WriteLine(meta.ToString());

            Log.Info($"tsMuxeR Meta: \r\n{meta}");

            sb.Append($"\"{_inputFile}\" \"{_outputFile}\"");

            return sb.ToString();
        }

        #endregion
    }
}