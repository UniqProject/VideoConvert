// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MuxerMkvMerge.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The MuxerMkvMerge
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Muxer
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
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
    /// The MuxerMkvMerge
    /// </summary>
    public class MuxerMkvMerge : EncodeBase, IMuxerMkvMerge
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (MuxerMkvMerge));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "mkvmerge.exe";
        private const string DefaultParams = "--ui-language en ";

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

        private string _outFile;

        private readonly Regex _regObj = new Regex(@"^.?Progress: ([\d]+?)%.*$",
            RegexOptions.Singleline | RegexOptions.Multiline);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerMkvMerge"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public MuxerMkvMerge(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the mkvmerge Process
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

            const string query = DefaultParams + "-V";

            using (var encoder = new Process())
            {
                var parameter = new ProcessStartInfo(localExecutable, query)
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
                    Log.Error($"mkvmerge exception: {ex}");
                }

                if (started)
                {
                    var output = encoder.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"^mkvmerge v([\d\.]+ \(.*\)).*built.*$",
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
                Log.Debug($"mkvmerge \"{verInfo}\" found");
            }
            return verInfo;
        }

        /// <summary>
        /// Execute a mkvmerge mux process.
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
                    throw new Exception("mkvmerge is already running");
                }

                IsEncoding = true;
                _currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var mkvmergeCliPath = Path.Combine(_appConfig.ToolsPath, Executable);

                var cliStart = new ProcessStartInfo(mkvmergeCliPath, query)
                {
                    WorkingDirectory = _appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                EncodeProcess = new Process { StartInfo = cliStart };
                Log.Info($"start parameter: mkvmerge {query}");

                EncodeProcess.Start();

                _startTime = DateTime.Now;

                EncodeProcess.OutputDataReceived += MuxerDataReceived;
                EncodeProcess.BeginOutputReadLine();

                _encoderProcessId = EncodeProcess.Id;

                // Set the encoder process exit trigger
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
                IsEncoding = false;
                _currentTask.ExitCode = -1;
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

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();
            sb.Append(DefaultParams);

            var fps = _currentTask.VideoStream.Fps;
            int vidStream;

            var tempExt = Path.GetExtension(_currentTask.VideoStream.TempFile);
            if (_currentTask.VideoStream.IsRawStream ||
                (_currentTask.Input == InputType.InputAvi && !_currentTask.VideoStream.Encoded) ||
                _currentTask.VideoStream.Encoded)
                vidStream = 0;
            else if (!_currentTask.VideoStream.Encoded && (tempExt == ".mp4" || tempExt == ".mkv" || tempExt == ".ts"))
                vidStream = Math.Max(_currentTask.VideoStream.StreamId - 1, 0);
            else
                vidStream = _currentTask.VideoStream.StreamId;

            var streamOrder = $" --track-order 0:{vidStream:g}";

            _outFile = !string.IsNullOrEmpty(_currentTask.TempOutput)
                       ? _currentTask.TempOutput
                       : _currentTask.OutputFile;

            sb.Append($"-o \"{_outFile}\" ");

            if (_currentTask.EncodingProfile.OutFormat == OutputType.OutputWebM)
                sb.Append("--webm ");

            var fpsStr = string.Empty;
            if (_currentTask.VideoStream.IsRawStream)
            {
                fpsStr = _currentTask.VideoStream.FrameRateEnumerator == 0
                    ? $"--default-duration {vidStream:0}:{fps:0.000}fps".ToString(_appConfig.CInfo)
                    : $"--default-duration {vidStream:0}:{_currentTask.VideoStream.FrameRateEnumerator:0}/{_currentTask.VideoStream.FrameRateDenominator:0}fps";
            }

            int stereoMode;

            switch (_currentTask.EncodingProfile.StereoType)
            {
                case StereoEncoding.None:
                    stereoMode = 0;
                    break;
                case StereoEncoding.FullSideBySideLeft:
                case StereoEncoding.HalfSideBySideLeft:
                    stereoMode = 1;
                    break;
                case StereoEncoding.FullSideBySideRight:
                case StereoEncoding.HalfSideBySideRight:
                    stereoMode = 11;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            sb.Append($"--language {vidStream:0}:eng {fpsStr} --default-track {vidStream:0}:yes --forced-track {vidStream:0}:yes ");

            if (stereoMode > 0)
                sb.Append($"--stereo-mode {vidStream:0}:{stereoMode:0} ");

            sb.Append($"-d {vidStream:0} -A -S --no-global-tags --no-chapters --compression {vidStream:0}:none \"{_currentTask.VideoStream.TempFile}\" ");

            var i = 1;
            var defaultAudioExists = false;
            foreach (var item in _currentTask.AudioStreams)
            {
                string isDefault;
                if (item.MkvDefault && !defaultAudioExists)
                {
                    isDefault = "yes";
                    defaultAudioExists = true;
                }
                else
                    isDefault = "no";

                var itemlang = item.LangCode;

                if (itemlang == "xx" || string.IsNullOrEmpty(itemlang))
                    itemlang = "und";

                var delayString = string.Empty;

                if (item.Delay != 0)
                    delayString = $"--sync 0:{item.Delay:0}";

                var itemStream = 0;

                if (Path.GetExtension(item.TempFile) == ".mkv")
                    itemStream = 1;

                sb.Append($"--language {itemStream:0}:{itemlang} {delayString} --default-track {itemStream:0}:{isDefault} --forced-track {itemStream:0}:no -D -a {itemStream:0} ");
                sb.Append($"-S --no-global-tags --no-chapters --compression {itemStream:0}:none \"{item.TempFile}\" ");

                streamOrder += $",{i:0}:0";
                i++;
            }

            var defaultSubExists = false;
            if (_currentTask.EncodingProfile.OutFormat != OutputType.OutputWebM)
            {
                foreach (var item in _currentTask.SubtitleStreams.Where(item => !item.HardSubIntoVideo && File.Exists(item.TempFile)))
                {
                    string isDefault;
                    if (item.MkvDefault && !defaultSubExists)
                    {
                        isDefault = "yes";
                        defaultSubExists = true;
                    }
                    else
                        isDefault = "no";

                    var itemlang = item.LangCode;

                    int subId;

                    if (itemlang == "xx" || string.IsNullOrEmpty(itemlang))
                        itemlang = "und";

                    var subFile = item.TempFile;

                    if (string.IsNullOrEmpty(subFile))
                    {
                        subFile = _currentTask.InputFile;
                        subId = item.Id;
                    }
                    else
                        subId = 0;

                    var delayString = string.Empty;

                    if (subFile != _currentTask.InputFile && (item.Delay != 0 && item.Delay != int.MinValue))
                        delayString = $"--sync {subId:0}:{item.Delay:0}";

                    sb.Append($"--language {subId:0}:{itemlang} {delayString} --default-track {subId:0}:{isDefault} --forced-track {subId:0}:no -s {subId:0} ");

                    sb.Append($"-D -A --no-global-tags --no-chapters --compression {subId:0}:none \"{subFile}\" ");

                    streamOrder += $",{i:g}:{subId:g}";
                    i++;
                }
            }

            var chapterString = string.Empty;

            if (_currentTask.Chapters.Count > 1 && _currentTask.EncodingProfile.OutFormat != OutputType.OutputWebM)
            {
                var chapterFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                                    !string.IsNullOrEmpty(_currentTask.TempOutput)
                                                                        ? _currentTask.TempOutput
                                                                        : _currentTask.OutputFile,
                                                                    "chapters.txt");

                using (var chapters = new StreamWriter(chapterFile))
                {
                    var actualTime = new TimeSpan();

                    for (var j = 0; j < _currentTask.Chapters.Count; j++)
                    {
                        DateTime dt;
                        if (_currentTask.Input != InputType.InputDvd)
                            dt = DateTime.MinValue.Add(_currentTask.Chapters[j]);
                        else
                        {
                            actualTime = actualTime.Add(_currentTask.Chapters[j]);
                            dt = DateTime.MinValue.Add(actualTime);
                        }
                        chapters.WriteLine($"CHAPTER{j + 1:000}={dt.ToString("H:mm:ss.fff")}");
                        chapters.WriteLine($"CHAPTER{j + 1:000}NAME=Chapter {j + 1:0}");
                    }
                }

                chapterString = $" --chapters \"{chapterFile}\"";
                _currentTask.TempFiles.Add(chapterFile);
            }

            sb.Append($"{chapterString} --compression -1:none {streamOrder}");

            return sb.ToString();
        }

        /// <summary>
        /// The mkvmerge process has exited.
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

            if (_currentTask.ExitCode < 2)
            {
                _currentTask.TempFiles.Add(_currentTask.VideoStream.TempFile);
                foreach (var item in _currentTask.AudioStreams)
                    _currentTask.TempFiles.Add(item.TempFile);
                foreach (var item in _currentTask.SubtitleStreams)
                    _currentTask.TempFiles.Add(item.TempFile);

                _currentTask.ExitCode = 0;
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
        private void MuxerDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && IsEncoding)
            {
                ProcessLogMessage(e.Data);
            }
        }

        private void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            var result = _regObj.Match(line);

            double processingSpeed = 0f;
            var secRemaining = 0;
            if (result.Success)
            {
                int progress;
                int.TryParse(result.Groups[1].Value, NumberStyles.Number, _appConfig.CInfo, out progress);

                var elapsedTime = DateTime.Now - _startTime;

                if (elapsedTime.TotalSeconds > 0)
                    processingSpeed = progress / elapsedTime.TotalSeconds;

                if (processingSpeed > 0)
                    secRemaining = (int)Math.Round((100D - progress) / processingSpeed, MidpointRounding.ToEven);

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
                Log.Info($"mkvmerge: {line}");
        }

        #endregion
    }
}