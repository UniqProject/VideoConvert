// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DecoderFfmsIndex.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The DecoderFfmsIndex
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Decoder
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using log4net;
    using VideoConvert.AppServices.Decoder.Interfaces;
    using VideoConvert.AppServices.Services.Base;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;

    /// <summary>
    /// The DecoderFfmsIndex
    /// </summary>
    public class DecoderFfmsIndex : EncodeBase, IDecoderFfmsIndex
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (DecoderFfmsIndex));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "ffmsindex.exe";

        /// <summary>
        /// Gets the Encoder Process ID
        /// </summary>
        private int _decoderProcessId;

        /// <summary>
        /// Start time of the current Encode;
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// The Current Task
        /// </summary>
        private EncodeInfo _currentTask;

        private string _inputFile;

        private readonly Regex _regObj = new Regex(@"^.*Indexing, please wait\.\.\. ([\d]+)%.*$",
                                                   RegexOptions.Singleline | RegexOptions.Multiline);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DecoderFfmsIndex"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public DecoderFfmsIndex(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the ffmsindex Process
        /// </summary>
        protected Process DecodeProcess { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Execute a ffmsindex demux process.
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
                    throw new Exception("ffmsindex is already running");
                }

                IsEncoding = true;
                _currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(_appConfig.AvsPluginsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = _appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                DecodeProcess = new Process {StartInfo = cliStart};
                Log.Info($"start parameter: ffmsindex {query}");

                DecodeProcess.Start();

                _startTime = DateTime.Now;

                DecodeProcess.OutputDataReceived += DecodeProcessDataReceived;
                DecodeProcess.BeginOutputReadLine();

                _decoderProcessId = DecodeProcess.Id;

                if (_decoderProcessId != -1)
                {
                    DecodeProcess.EnableRaisingEvents = true;
                    DecodeProcess.Exited += DecodeProcessExited;
                }

                DecodeProcess.PriorityClass = _appConfig.GetProcessPriority();

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
                if (DecodeProcess != null && !DecodeProcess.HasExited)
                {
                    DecodeProcess.Kill();
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

            _inputFile = _currentTask.VideoStream.TempFile;

            sb.Append($"-f -t -1 \"{_inputFile}\"");

            return sb.ToString();
        }

        /// <summary>
        /// The ffmsindex process has exited.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        private void DecodeProcessExited(object sender, EventArgs e)
        {
            try
            {
                DecodeProcess.CancelOutputRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            _currentTask.ExitCode = DecodeProcess.ExitCode;
            Log.Info($"Exit Code: {_currentTask.ExitCode:0}");

            if (_currentTask.ExitCode == 0)
            {
                _currentTask.FfIndexFile = _inputFile + ".ffindex";
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
        private void DecodeProcessDataReceived(object sender, DataReceivedEventArgs e)
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

            var result = _regObj.Match(line);
            if (result.Success)
            {
                float progress = Convert.ToInt32(result.Groups[1].Value);
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

                var remainingTime = TimeSpan.FromSeconds(secLeft);

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
                Log.Info($"ffmsindex: {line}");
        }

        #endregion
    }
}