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
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

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
            this._appConfig = appConfig;
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
                if (this.IsEncoding)
                {
                    encodeQueueTask.ExitCode = -1;
                    throw new Exception("ffmsindex is already running");
                }

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(this._appConfig.AvsPluginsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                this.DecodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: ffmsindex {0}", query);

                this.DecodeProcess.Start();

                this._startTime = DateTime.Now;

                this.DecodeProcess.OutputDataReceived += DecodeProcessDataReceived;
                this.DecodeProcess.BeginOutputReadLine();

                this._decoderProcessId = this.DecodeProcess.Id;

                if (this._decoderProcessId != -1)
                {
                    this.DecodeProcess.EnableRaisingEvents = true;
                    this.DecodeProcess.Exited += DecodeProcessExited;
                }

                this.DecodeProcess.PriorityClass = this._appConfig.GetProcessPriority();

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
                if (this.DecodeProcess != null && !this.DecodeProcess.HasExited)
                {
                    this.DecodeProcess.Kill();
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

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();

            this._inputFile = this._currentTask.VideoStream.TempFile;

            sb.AppendFormat("-f -t -1 \"{0}\"", this._inputFile);

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
                this.DecodeProcess.CancelOutputRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            this._currentTask.ExitCode = DecodeProcess.ExitCode;
            Log.InfoFormat("Exit Code: {0:g}", this._currentTask.ExitCode);

            if (this._currentTask.ExitCode == 0)
            {
                this._currentTask.FfIndexFile = this._inputFile + ".ffindex";
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
        private void DecodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && this.IsEncoding)
            {
                this.ProcessLogMessage(e.Data);
            }
        }

        private void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            var elapsedTime = DateTime.Now - this._startTime;

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
                this.InvokeEncodeStatusChanged(eventArgs);
            }
            else
                Log.InfoFormat("ffmsindex: {0}", line);
        }

        #endregion
    }
}