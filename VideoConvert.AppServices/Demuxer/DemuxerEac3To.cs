// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DemuxerEac3To.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using Interop.Utilities;
    using log4net;
    using Services.Base;
    using Services.Interfaces;

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
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the eac3to Process
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

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
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
                    Log.ErrorFormat("eac3to exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    Regex regObj = new Regex(@"^.*eac3to v([\d\.]+),.*$",
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
                Log.DebugFormat("eac3to \"{0}\" found", verInfo);
            }
            return verInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encodeQueueTask"></param>
        /// <exception cref="Exception"></exception>
        public void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (this.IsEncoding)
                    throw new Exception("eac3to is already running");

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                string query = GenerateCommandLine();
                string cliPath = Path.Combine(this._appConfig.ToolsPath, Executable);

                ProcessStartInfo cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                this.EncodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: eac3to {0}", query);

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
            StringBuilder sb = new StringBuilder();

            string baseFileName;

            _inputFile = string.IsNullOrEmpty(this._currentTask.TempInput)
                            ? this._currentTask.InputFile
                            : this._currentTask.TempInput;

            if (string.IsNullOrEmpty(this._currentTask.TempInput))
            {
                baseFileName = Path.Combine(this._appConfig.DemuxLocation,
                    string.IsNullOrEmpty(this._currentTask.TempOutput)
                        ? this._currentTask.BaseName
                        : Path.GetFileNameWithoutExtension(this._currentTask.TempOutput));

                this._currentTask.VideoStream.TempFile =
                    FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation,
                                                    baseFileName, "demuxed.video.mkv");
            }
            else
            {
                baseFileName = Path.Combine(this._appConfig.DemuxLocation,
                                            Path.GetFileNameWithoutExtension(this._currentTask.TempInput));
                this._currentTask.VideoStream.TempFile =
                    FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation, baseFileName,
                        "demuxed.video.mkv");
            }

            sb.AppendFormat("\"{0}\" {1:g}:\"{2}\" ", _inputFile, this._currentTask.VideoStream.StreamId,
                            this._currentTask.VideoStream.TempFile);

            // on stereo sources, decide if stream for right eye should be extracted
            if (this._currentTask.StereoVideoStream.RightStreamId > 0 &&
                this._currentTask.EncodingProfile.StereoType != StereoEncoding.None)
            {
                this._currentTask.StereoVideoStream.RightTempFile =
                                                    FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation,
                                                        this._currentTask.VideoStream.TempFile,
                                                        "right.h264");
                this._currentTask.StereoVideoStream.LeftTempFile =
                                                    FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation,
                                                        this._currentTask.VideoStream.TempFile,
                                                        "left.h264");
                sb.AppendFormat("{0:g}:\"{1}\" {2:g}:\"{3}\" ",
                                this._currentTask.StereoVideoStream.LeftStreamId,
                                this._currentTask.StereoVideoStream.LeftTempFile,
                                this._currentTask.StereoVideoStream.RightStreamId,
                                this._currentTask.StereoVideoStream.RightTempFile);
            }

            string ext;
            string formattedExt;

            // process all audio streams
            foreach (AudioInfo item in this._currentTask.AudioStreams)
            {
                // get file extension for selected stream based on format and format profile
                ext = StreamFormat.GetFormatExtension(item.Format, item.FormatProfile, false);

                formattedExt = string.Format("demuxed.audio.{0:g}.{1}.{2}", item.StreamId, item.LangCode, ext);

                item.TempFile = FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation,
                                                                baseFileName,
                                                                formattedExt);

                sb.AppendFormat("{0:g}:\"{1}\"", item.Id, item.TempFile);
            }

            // process all subtitle streams
            foreach (SubtitleInfo item in this._currentTask.SubtitleStreams)
            {
                ext = StreamFormat.GetFormatExtension(item.Format, String.Empty, false);
                formattedExt = string.Format("demuxed.subtitle.{0:g}.{1}.{2}", item.StreamId, item.LangCode, ext);

                item.TempFile = FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation,
                                                                this._currentTask.TempInput, 
                                                                formattedExt);

                sb.AppendFormat("{0:g}:\"{1}\" ", item.Id, item.TempFile);
                item.RawStream = true;
            }

            // add logfile to tempfiles list for deletion
            this._currentTask.TempFiles.Add(this._currentTask.VideoStream.TempFile.Substring(0,
                                                this._currentTask.VideoStream.TempFile.LastIndexOf('.')) + " - Log.txt");

            sb.Append("-progressNumbers -no2ndpass ");

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

            if (this._currentTask.ExitCode == 0)
            {
                this._currentTask.VideoStream.IsRawStream = false;
                GetStreamInfo();
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

            TimeSpan elapsedTime = DateTime.Now - this._startTime;
            TimeSpan remainingTime = elapsedTime + TimeSpan.FromSeconds(1);
            float progress = 0f;

            Match processingResult = _processingRegex.Match(line);
            Match analyzingResult = _analyzingRegex.Match(line);

            if (analyzingResult.Success)
            {
                progress = Convert.ToInt32(analyzingResult.Groups[1].Value) / 2f;
                float progressLeft = 100f - progress;

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
                float progressLeft = 100f - progress;

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
                Log.InfoFormat("eac3to: {0}", line);

            if (analyzingResult.Success || processingResult.Success)
            {
                EncodeProgressEventArgs eventArgs = new EncodeProgressEventArgs
                {
                    AverageFrameRate = 0,
                    CurrentFrameRate = 0,
                    EstimatedTimeLeft = remainingTime,
                    PercentComplete = progress,
                    Task = 0,
                    TaskCount = 0,
                    ElapsedTime = elapsedTime,
                };
                this.InvokeEncodeStatusChanged(eventArgs);
            }
        }

        private void GetStreamInfo()
        {
            try
            {
                this._currentTask.MediaInfo = GenHelper.GetMediaInfo(this._currentTask.VideoStream.TempFile);
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex);
            }
            finally
            {
                if (this._currentTask.MediaInfo.Video.Count > 0)
                {
                    this._currentTask.VideoStream.Bitrate = this._currentTask.MediaInfo.Video[0].BitRate;
                    this._currentTask.VideoStream.StreamSize = GenHelper.GetFileSize(this._currentTask.VideoStream.TempFile);
                    this._currentTask.VideoStream.FrameCount = this._currentTask.MediaInfo.Video[0].FrameCount;
                    this._currentTask.VideoStream.StreamId = this._currentTask.MediaInfo.Video[0].ID;
                }
            }

            for (int i = 0; i < this._currentTask.AudioStreams.Count; i++)
            {
                AudioInfo aStream = this._currentTask.AudioStreams[i];
                aStream = AudioHelper.GetStreamInfo(aStream);
                this._currentTask.AudioStreams[i] = aStream;
            }

            foreach (SubtitleInfo sStream in this._currentTask.SubtitleStreams)
                sStream.StreamSize = GenHelper.GetFileSize(sStream.TempFile);
        }

        #endregion
    }
}