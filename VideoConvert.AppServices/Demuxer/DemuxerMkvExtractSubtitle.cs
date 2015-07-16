// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DemuxerMkvExtractSubtitle.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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
    /// The DemuxerMkvExtractSubtitle
    /// </summary>
    public class DemuxerMkvExtractSubtitle : EncodeBase, IDemuxerMkvExtractSubtitle
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (DemuxerMkvExtractSubtitle));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "mkvextract.exe";
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

        private string _inputFile;
        private string _outputFile;

        private SubtitleInfo _subtitle;

        private readonly Regex _demuxRegex = new Regex(@"^.?Progress: ([\d]+?)%.*$",
            RegexOptions.Singleline | RegexOptions.Multiline);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DemuxerMkvExtractSubtitle"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public DemuxerMkvExtractSubtitle(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the mkvextract Process
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
                    RedirectStandardOutput = true,
                    Arguments =  DefaultParams + " -V"
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
                    Log.Error($"mkvextract exception: {ex}");
                }

                if (started)
                {
                    var output = encoder.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"mkvextract v([\d\.]+ \(.*\)).*$",
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
                Log.Debug($"mkvextract \"{verInfo}\" found");
            }
            return verInfo;
        }

        /// <summary>
        /// Execute a mkvmerge demux process.
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
                    throw new Exception("mkvextract is already running");
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
                EncodeProcess = new Process {StartInfo = cliStart};
                Log.Info($"start parameter: mkvextract {query}");

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
        /// The mkvextract process has exited.
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

            if (_currentTask.ExitCode <= 1)
            {
                _currentTask.ExitCode = 0;
                _currentTask.TempFiles.Add(_inputFile);
                _subtitle.RawStream = true;
                _subtitle.TempFile = _outputFile;
                if (_subtitle.Format == "VobSub")
                {
                    _currentTask.TempFiles.Add(_outputFile);
                    _subtitle.TempFile = Path.ChangeExtension(_outputFile, "idx");
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

            var result = _demuxRegex.Match(line);

            if (result.Success)
            {
                int progress;
                double processingSpeed = 0f;
                var secRemaining = 0;
                
                int.TryParse(result.Groups[1].Value, NumberStyles.Number, _appConfig.CInfo, out progress);
                var remaining = 100 - progress;

                var elapsedTime = DateTime.Now.Subtract(_startTime);

                if (elapsedTime.TotalSeconds > 0)
                    processingSpeed = progress / elapsedTime.TotalSeconds;

                if (processingSpeed > 0)
                    secRemaining = (int) Math.Round(remaining/processingSpeed, MidpointRounding.ToEven);

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
                Log.Info($"mkvextract: {line}");
        }

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();
            sb.Append(DefaultParams);

            _subtitle = _currentTask.SubtitleStreams[_currentTask.StreamId];
            _inputFile = _subtitle.TempFile;
            var ext = StreamFormat.GetFormatExtension(_subtitle.Format, "", true);
            string formattedExt = $"raw.{ext}";

            _outputFile = FileSystemHelper.CreateTempFile(_appConfig.TempPath, _inputFile, formattedExt);

            sb.Append($"tracks \"{_inputFile}\" 0:\"{_outputFile}\" ");

            return sb.ToString();
        }

        #endregion
    }
}