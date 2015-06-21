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
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
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
            this._appConfig = appConfig;
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
            string verInfo = string.Empty;

            string localExecutable = Path.Combine(encPath, Executable);

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
                    Log.ErrorFormat("mkvextract exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"mkvextract v([\d\.]+ \(.*\)).*$",
                        RegexOptions.Singleline | RegexOptions.Multiline);
                    Match result = regObj.Match(output);
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
                Log.DebugFormat("mkvextract \"{0}\" found", verInfo);
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
                if (this.IsEncoding)
                {
                    encodeQueueTask.ExitCode = -1;
                    throw new Exception("mkvextract is already running");
                }

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                string query = GenerateCommandLine();
                string cliPath = Path.Combine(this._appConfig.ToolsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                this.EncodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: mkvextract {0}", query);

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
                this.EncodeProcess.CancelOutputRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            this._currentTask.ExitCode = EncodeProcess.ExitCode;
            Log.InfoFormat("Exit Code: {0:g}", this._currentTask.ExitCode);

            if (this._currentTask.ExitCode <= 1)
            {
                this._currentTask.ExitCode = 0;
                this._currentTask.TempFiles.Add(this._inputFile);
                this._subtitle.RawStream = true;
                this._subtitle.TempFile = this._outputFile;
                if (this._subtitle.Format == "VobSub")
                {
                    this._currentTask.TempFiles.Add(this._outputFile);
                    this._subtitle.TempFile = Path.ChangeExtension(this._outputFile, "idx");
                }
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

            Match result = _demuxRegex.Match(line);

            if (result.Success)
            {
                int progress;
                double processingSpeed = 0f;
                var secRemaining = 0;
                
                Int32.TryParse(result.Groups[1].Value, NumberStyles.Number, this._appConfig.CInfo, out progress);
                int remaining = 100 - progress;

                var elapsedTime = DateTime.Now.Subtract(this._startTime);

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
                this.InvokeEncodeStatusChanged(eventArgs);
                
            }
            else
                Log.InfoFormat("mkvextract: {0}", line);
        }

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();
            sb.Append(DefaultParams);

            this._subtitle = this._currentTask.SubtitleStreams[this._currentTask.StreamId];
            this._inputFile = this._subtitle.TempFile;
            string ext = StreamFormat.GetFormatExtension(this._subtitle.Format, "", true);
            string formattedExt = string.Format("raw.{0}", ext);

            this._outputFile = FileSystemHelper.CreateTempFile(this._appConfig.TempPath, this._inputFile, formattedExt);

            sb.AppendFormat("tracks \"{0}\" 0:\"{1}\" ", this._inputFile, this._outputFile);

            return sb.ToString();
        }

        #endregion
    }
}