// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DemuxerEac3To.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The DemuxerEac3To
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
    /// The DemuxerEac3To
    /// </summary>
    public class DemuxerEac3To : EncodeBase, IDemuxerEac3To
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (DemuxerEac3To));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "eac3to.exe";

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

        private readonly Regex _processingRegex = new Regex(@"^.*process: ([\d]+)%.*$",
                                                            RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly Regex _analyzingRegex = new Regex(@"^.*analyze: ([\d]+)%.*$",
                                                           RegexOptions.Singleline | RegexOptions.Multiline);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DemuxerEac3To"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public DemuxerEac3To(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the eac3to Process
        /// </summary>
        protected Process DemuxProcess { get; set; }

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

            using (var demuxer = new Process())
            {
                var parameter = new ProcessStartInfo(localExecutable)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
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
                    Log.Error($"eac3to exception: {ex}");
                }

                if (started)
                {
                    var output = demuxer.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"^.*eac3to v([\d\.]+),.*$",
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
            if (Log.IsDebugEnabled)
            {
                Log.Debug($"eac3to \"{verInfo}\" found");
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
                    throw new Exception("eac3to is already running");
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
                    RedirectStandardOutput = true
                };
                DemuxProcess = new Process {StartInfo = cliStart};
                Log.Info($"start parameter: eac3to {query}");

                DemuxProcess.Start();

                _startTime = DateTime.Now;

                DemuxProcess.OutputDataReceived += DemuxProcessDataReceived;
                DemuxProcess.BeginOutputReadLine();

                _demuxerProcessId = DemuxProcess.Id;

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

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();

            string baseFileName;

            _inputFile = string.IsNullOrEmpty(_currentTask.TempInput)
                            ? _currentTask.InputFile
                            : _currentTask.TempInput;

            if (string.IsNullOrEmpty(_currentTask.TempInput))
            {
                baseFileName = Path.Combine(_appConfig.DemuxLocation,
                    string.IsNullOrEmpty(_currentTask.TempOutput)
                        ? _currentTask.BaseName
                        : Path.GetFileNameWithoutExtension(_currentTask.TempOutput));

                _currentTask.VideoStream.TempFile =
                    FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                    baseFileName, "demuxed.video.mkv");
            }
            else
            {
                baseFileName = Path.Combine(_appConfig.DemuxLocation,
                                            Path.GetFileNameWithoutExtension(_currentTask.TempInput));
                _currentTask.VideoStream.TempFile =
                    FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation, baseFileName,
                        "demuxed.video.mkv");
            }

            sb.Append($"\"{_inputFile}\" {_currentTask.VideoStream.StreamId:0}:\"{_currentTask.VideoStream.TempFile}\" ");

            // on stereo sources, decide if stream for right eye should be extracted
            if (_currentTask.StereoVideoStream.RightStreamId > 0 &&
                _currentTask.EncodingProfile.StereoType != StereoEncoding.None)
            {
                _currentTask.StereoVideoStream.RightTempFile =
                                                    FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                        _currentTask.VideoStream.TempFile,
                                                        "right.h264");
                _currentTask.StereoVideoStream.LeftTempFile =
                                                    FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                        _currentTask.VideoStream.TempFile,
                                                        "left.h264");
                sb.Append($"{_currentTask.StereoVideoStream.LeftStreamId:0}:\"{_currentTask.StereoVideoStream.LeftTempFile}\" ");
                sb.Append($"{_currentTask.StereoVideoStream.RightStreamId:0}:\"{_currentTask.StereoVideoStream.RightTempFile}\" ");
            }

            string ext;
            string formattedExt;

            // process all audio streams
            foreach (var item in _currentTask.AudioStreams)
            {
                // get file extension for selected stream based on format and format profile
                ext = StreamFormat.GetFormatExtension(item.Format, item.FormatProfile, false);

                formattedExt = $"demuxed.audio.{item.StreamId:g}.{item.LangCode}.{ext}";

                item.TempFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                                baseFileName,
                                                                formattedExt);

                sb.Append($"{item.Id:0}:\"{item.TempFile}\" ");
            }

            // process all subtitle streams
            foreach (var item in _currentTask.SubtitleStreams)
            {
                ext = StreamFormat.GetFormatExtension(item.Format, string.Empty, false);
                formattedExt = $"demuxed.subtitle.{item.StreamId:g}.{item.LangCode}.{ext}";

                item.TempFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                                _currentTask.TempInput, 
                                                                formattedExt);

                sb.Append($"{item.Id:0}:\"{item.TempFile}\" ");
                item.RawStream = true;
            }

            // add logfile to tempfiles list for deletion
            _currentTask.TempFiles.Add(_currentTask.VideoStream.TempFile.Substring(0,
                                                _currentTask.VideoStream.TempFile.LastIndexOf('.')) + " - Log.txt");

            sb.Append("-progressNumbers ");

            return sb.ToString();
        }

        /// <summary>
        /// The eac3to process has exited.
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
                DemuxProcess.CancelOutputRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            _currentTask.ExitCode = DemuxProcess.ExitCode;
            Log.Info($"Exit Code: {_currentTask.ExitCode:0}");

            if (_currentTask.ExitCode == 0)
            {
                _currentTask.VideoStream.IsRawStream = false;
                GetStreamInfo();
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
        private void DemuxProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && IsEncoding)
            {
                ProcessLogMessage(e.Data);
            }
        }

        private void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            var elapsedTime = DateTime.Now - _startTime;
            var remainingTime = elapsedTime + TimeSpan.FromSeconds(1);
            var progress = 0f;

            var processingResult = _processingRegex.Match(line);
            var analyzingResult = _analyzingRegex.Match(line);

            if (analyzingResult.Success)
            {
                progress = Convert.ToInt32(analyzingResult.Groups[1].Value) / 2f;
                var progressLeft = 100f - progress;

                double speed = 0f;
                if (elapsedTime.TotalSeconds > 0)
                {
                    speed = progress / elapsedTime.TotalSeconds;
                }

                long secLeft = 0;
                if (speed > 0)
                {
                    secLeft = (int)Math.Floor(progressLeft * speed);
                }

                remainingTime = TimeSpan.FromSeconds(secLeft);
            }
            else if (processingResult.Success)
            {
                progress = 50 + Convert.ToInt32(analyzingResult.Groups[1].Value) / 2f;
                var progressLeft = 100f - progress;

                double speed = 0f;
                if (elapsedTime.TotalSeconds > 0)
                {
                    speed = progress / elapsedTime.TotalSeconds;
                }

                long secLeft = 0;
                if (speed > 0)
                {
                    secLeft = (int)Math.Floor(progressLeft * speed);
                }

                remainingTime = TimeSpan.FromSeconds(secLeft);
            }
            else
                Log.Info($"eac3to: {line}");

            if (!analyzingResult.Success && !processingResult.Success) return;

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

        private void GetStreamInfo()
        {
            try
            {
                _currentTask.MediaInfo = GenHelper.GetMediaInfo(_currentTask.VideoStream.TempFile);
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex);
            }
            finally
            {
                if (_currentTask.MediaInfo.Video.Count > 0)
                {
                    _currentTask.VideoStream.Bitrate = _currentTask.MediaInfo.Video[0].BitRate;
                    _currentTask.VideoStream.StreamSize = GenHelper.GetFileSize(_currentTask.VideoStream.TempFile);
                    _currentTask.VideoStream.FrameCount = _currentTask.MediaInfo.Video[0].FrameCount;
                    _currentTask.VideoStream.StreamId = _currentTask.MediaInfo.Video[0].ID;
                }
            }

            for (var i = 0; i < _currentTask.AudioStreams.Count; i++)
            {
                var aStream = _currentTask.AudioStreams[i];
                aStream = AudioHelper.GetStreamInfo(aStream);
                _currentTask.AudioStreams[i] = aStream;
            }

            foreach (var sStream in _currentTask.SubtitleStreams)
                sStream.StreamSize = GenHelper.GetFileSize(sStream.TempFile);
        }

        #endregion
    }
}