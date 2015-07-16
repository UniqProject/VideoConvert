// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DemuxerFfmpeg.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The ffmpeg demuxer
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Demuxer
{
    using System;
    using System.Diagnostics;
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
    /// The ffmpeg demuxer
    /// </summary>
    public class DemuxerFfmpeg : EncodeBase, IDemuxerFfmpeg
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(DemuxerFfmpeg));
        
        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "ffmpeg.exe";
        private const string Executable64 = "ffmpeg_64.exe";

        /// <summary>
        /// Gets the Encoder Process ID
        /// </summary>
        private int _demuxerProcessId;

        /// <summary>
        /// Start time of the current Encode;
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// The Current Task
        /// </summary>
        private EncodeInfo _currentTask;

        private string _inputFile;

        private readonly Regex _demuxReg = new Regex(@"^.*size=\s*?(\d+)[\w\s]+?time=([\d\.\:]+).+$",
                                                   RegexOptions.Singleline | RegexOptions.Multiline);

        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="DemuxerFfmpeg"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public DemuxerFfmpeg(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets The x264 Process
        /// </summary>
        protected Process DemuxProcess { get; set; }

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

            using (var demuxer = new Process())
            {
                var parameter = new ProcessStartInfo(localExecutable)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                };
                demuxer.StartInfo = parameter;

                bool started;
                try
                {
                    started = demuxer.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.Error($"ffmpeg exception: {ex}");
                }

                if (started)
                {
                    var output = demuxer.StandardError.ReadToEnd();
                    var regObj = new Regex(@"^.*ffmpeg version ([\w\d\.\-_]+)[, ].*$",
                        RegexOptions.Singleline | RegexOptions.Multiline);
                    var result = regObj.Match(output);
                    if (result.Success)
                        verInfo = result.Groups[1].Value;

                    demuxer.WaitForExit(10000);
                    if (!demuxer.HasExited)
                        demuxer.Kill();
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
        /// Execute a ffmpeg demux process.
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

                var use64BitEncoder = _appConfig.Use64BitEncoders &&
                                       _appConfig.Ffmpeg64Installed &&
                                       Environment.Is64BitOperatingSystem;

                if (_currentTask.Input == InputType.InputDvd)
                    _inputFile = _currentTask.DumpOutput;
                else
                    _inputFile = string.IsNullOrEmpty(_currentTask.TempInput)
                                ? _currentTask.InputFile
                                : _currentTask.TempInput;
                _currentTask.VideoStream.TempFile = _inputFile;

                try
                {
                    _currentTask.MediaInfo = GenHelper.GetMediaInfo(_inputFile);
                    if (_currentTask.Input == InputType.InputDvd)
                    {
                        _currentTask.VideoStream = VideoHelper.GetStreamInfo(_currentTask.MediaInfo,
                                                                                  _currentTask.VideoStream,
                                                                                  false);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }

                var query = GenerateCommandLine();
                var ffmpegCliPath = Path.Combine(_appConfig.ToolsPath,
                                                  use64BitEncoder ? Executable64 : Executable);

                var cliStart = new ProcessStartInfo(ffmpegCliPath, query)
                {
                    WorkingDirectory = _appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                };
                DemuxProcess = new Process { StartInfo = cliStart };
                Log.Info($"start parameter: ffmpeg {query}");

                DemuxProcess.Start();

                _startTime = DateTime.Now;

                DemuxProcess.ErrorDataReceived += DemuxDataReceived;
                DemuxProcess.BeginErrorReadLine();

                _demuxerProcessId = DemuxProcess.Id;

                // Set the encoder process exit trigger
                if (_demuxerProcessId != -1)
                {
                    DemuxProcess.EnableRaisingEvents = true;
                    DemuxProcess.Exited += DemuxProcessExited;
                }

                DemuxProcess.PriorityClass = _appConfig.GetProcessPriority();

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
                if (DemuxProcess != null && !DemuxProcess.HasExited)
                {
                    DemuxProcess.Kill();
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
        private void DemuxProcessExited(object sender, EventArgs e)
        {
            try
            {
                DemuxProcess.CancelErrorRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            _currentTask.ExitCode = DemuxProcess.ExitCode;
            Log.Info($"Exit Code: {_currentTask.ExitCode:0}");

            if (_currentTask.ExitCode == 0)
            {
                if (_currentTask.Input == InputType.InputDvd)
                {
                    _currentTask.TempFiles.Add(_inputFile);
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
        private void DemuxDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && IsEncoding)
            {
                ProcessLogMessage(e.Data);
            }
        }

        private void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            var result = _demuxReg.Match(line);
            
            double processingSpeed = 0f;
            var secRemaining = 0;

            if (result.Success)
            {
                TimeSpan streamPosition;
                TimeSpan.TryParseExact(result.Groups[2].Value, @"hh\:mm\:ss\.ff", _appConfig.CInfo, out streamPosition);
                var secDemux = streamPosition.TotalSeconds;

                var remainingStreamTime = _currentTask.VideoStream.Length - secDemux;
                
                var elapsedTime = DateTime.Now - _startTime;

                if (elapsedTime.TotalSeconds > 0)
                    processingSpeed = secDemux / elapsedTime.TotalSeconds;

                if (processingSpeed > 0)
                    secRemaining = (int) Math.Round(remainingStreamTime/processingSpeed, MidpointRounding.ToEven);

                var remainingTime = new TimeSpan(0, 0, secRemaining);

                var progress = (float) Math.Round(secDemux/_currentTask.VideoStream.Length*100d);

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

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();

            if (_currentTask.Input == InputType.InputDvd)
                sb.Append("-probesize 2147483647 -analyzeduration 2147483647 -fflags genpts ");

            sb.Append($"-i \"{_inputFile}\" ");

            string baseName;
            string ext;

            var formattedExt = "demuxed.video.mkv";

            if (string.IsNullOrEmpty(_currentTask.TempInput))
                baseName = string.IsNullOrEmpty(_currentTask.TempOutput)
                           ? _currentTask.BaseName
                           : _currentTask.TempOutput;
            else
                baseName = _currentTask.TempInput;

            _currentTask.VideoStream.TempFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                                                     baseName,
                                                                                     formattedExt);

            var streamID = _currentTask.Input == InputType.InputDvd
                              ? $"#0x{_currentTask.VideoStream.StreamId + 479:X}"
                              : $"0:v:{_currentTask.VideoStream.StreamKindID:0}";

            sb.Append($"-map {streamID} -c:v copy -y \"{_currentTask.VideoStream.TempFile}\" ");

            foreach (var item in _currentTask.AudioStreams)
            {
                ext = StreamFormat.GetFormatExtension(item.Format, item.FormatProfile, false);

                string acodec;
                switch (ext)
                {
                    case "flac":
                        acodec = "flac";
                        break;
                    case "wav":
                        acodec = "pcm_s16le";
                        break;
                    default:
                        acodec = "copy";
                        break;
                }

                formattedExt = $"demuxed.audio.{item.StreamId:g}.{item.LangCode}.{ext}";

                item.TempFile =
                    FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation, baseName, formattedExt);

                if (_currentTask.Input == InputType.InputDvd)
                {
                    var dvdStreamId = item.StreamId;
                    if (string.CompareOrdinal(item.Format.ToLowerInvariant(), "mpeg1") == 0 ||
                        string.CompareOrdinal(item.Format.ToLowerInvariant(), "mpeg2") == 0)
                        dvdStreamId += 256;
                    streamID = $"#0x{dvdStreamId:X}";
                }
                else
                    streamID = $"0:a:{item.StreamKindId:0}";

                sb.Append($"-map {streamID} -c:a {acodec} -y \"{item.TempFile}\" ");
            }

            foreach (var item in _currentTask.SubtitleStreams)
            {
                ext = "mkv";

                formattedExt = $"demuxed.subtitle.{item.StreamId:g}.{item.LangCode}.{ext}";

                item.TempFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation, baseName, formattedExt);

                item.RawStream = false;

                streamID = _currentTask.Input == InputType.InputDvd
                    ? $"#0x{item.StreamId:X}"
                    : $"0:s:{item.StreamKindId:0}";

                var codec = "copy";

                if (item.Format == "VobSub")
                    codec = "dvd_subtitle";

                sb.Append($"-map {streamID} -c:s {codec} -y \"{item.TempFile}\" ");
            }

            return sb.ToString();
        }

        #endregion

    }
}