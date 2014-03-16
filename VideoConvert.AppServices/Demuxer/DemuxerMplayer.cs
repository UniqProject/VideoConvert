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
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using Interop.Utilities;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

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
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the mplayer Process
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
                    Log.ErrorFormat("mplayer exception: {0}", ex);
                }

                if (started)
                {
                    var output = encoder.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"^MPlayer ([\w\.].*) .*\(C\).*$",
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
                Log.DebugFormat("mplayer \"{0}\" found", verInfo);
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
                if (this.IsEncoding)
                {
                    encodeQueueTask.ExitCode = -1;
                    throw new Exception("mplayer is already running");
                }

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(this._appConfig.ToolsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                this.EncodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: mplayer {0}", query);

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

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();

            this._inputFile = this._currentTask.InputFile;
            this._outputFile =
                FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation,
                                                string.IsNullOrEmpty(this._currentTask.TempOutput)
                                                    ? this._currentTask.OutputFile
                                                    : this._currentTask.TempOutput,
                                                "dump.mpg");

            this._currentTask.DumpOutput = this._outputFile;
            
            var chapterText = string.Empty;
            if (this._currentTask.SelectedDvdChapters.Length > 0)
            {
                var posMinus = this._currentTask.SelectedDvdChapters.IndexOf('-');
                chapterText = string.Format(posMinus == -1 ? "-chapter {0}-{0}" : "-chapter {0}",
                                            this._currentTask.SelectedDvdChapters);
            }

            if (string.IsNullOrEmpty(Path.GetDirectoryName(this._inputFile)))
            {
                var pos = this._inputFile.LastIndexOf(Path.DirectorySeparatorChar);
                this._inputFile = this._inputFile.Remove(pos);
            }

            sb.AppendFormat("-dvd-device \"{0}\" dvdnav://{1:g} {3} -nocache -dumpstream -dumpfile \"{2}\"",
                            this._inputFile, this._currentTask.TrackId, this._outputFile, chapterText);

            return sb.ToString();
        }

        /// <summary>
        /// The mplayer process has exited.
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

            var result = _regObj.Match(line);

            if (result.Success)
            {
                float progress;
                Single.TryParse(result.Groups[1].Value, NumberStyles.Number, this._appConfig.CInfo, out progress);
                var elapsedTime = DateTime.Now - this._startTime;

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
                this.InvokeEncodeStatusChanged(eventArgs);
                
            }
            else
                Log.InfoFormat("mplayer: {0}", line);
        }

        #endregion
    }
}