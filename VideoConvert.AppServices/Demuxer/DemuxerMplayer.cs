// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DemuxerMplayer.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The DemuxerMplayer
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
    /// The DemuxerMplayer
    /// </summary>
    public class DemuxerMplayer : EncodeBase, IDemuxerMplayer
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (DemuxerMplayer));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "mplayer.exe";

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
        private string _outputFile;

        private readonly Regex _regObj = new Regex(@"^dump: .*\(~([\d\.]+?)%\)$",
            RegexOptions.Singleline | RegexOptions.Multiline);
        
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DemuxerMplayer"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public DemuxerMplayer(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the mplayer Process
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
                    Log.Error($"mplayer exception: {ex}");
                }

                if (started)
                {
                    var output = demuxer.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"^MPlayer ([\w\.].*) .*\(C\).*$",
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
                Log.Debug($"mplayer \"{verInfo}\" found");
            }
            return verInfo;
        }

        /// <summary>
        /// Execute a mplayer demux process.
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
                    throw new Exception("mplayer is already running");
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
                Log.Info($"start parameter: mplayer {query}");

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

        /// <summary>
        /// The mplayer process has exited.
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

            var result = _regObj.Match(line);

            if (result.Success)
            {
                float progress;
                float.TryParse(result.Groups[1].Value, NumberStyles.Number, _appConfig.CInfo, out progress);
                var elapsedTime = DateTime.Now - _startTime;

                double processingSpeed = 0f;
                var secRemaining = 0;

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
                Log.Info($"mplayer: {line}");
        }

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();

            _inputFile = _currentTask.InputFile;
            _outputFile =
                FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                string.IsNullOrEmpty(_currentTask.TempOutput)
                                                    ? _currentTask.OutputFile
                                                    : _currentTask.TempOutput,
                                                "dump.mpg");

            _currentTask.DumpOutput = _outputFile;

            var chapterText = string.Empty;
            if (_currentTask.SelectedDvdChapters.Length > 0)
            {
                var posDash = _currentTask.SelectedDvdChapters.IndexOf('-');

                chapterText = $"-chapter {_currentTask.SelectedDvdChapters}";
                if (posDash == -1)
                    chapterText += $"-{_currentTask.SelectedDvdChapters}";
            }

            if (string.IsNullOrEmpty(Path.GetDirectoryName(_inputFile)))
            {
                var pos = _inputFile.LastIndexOf(Path.DirectorySeparatorChar);
                _inputFile = _inputFile.Remove(pos);
            }

            sb.Append($"-dvd-device \"{_inputFile}\" dvdnav://{_currentTask.TrackId:0} {chapterText} -nocache -dumpstream -dumpfile \"{_outputFile}\"");

            return sb.ToString();
        }

        #endregion
    }
}