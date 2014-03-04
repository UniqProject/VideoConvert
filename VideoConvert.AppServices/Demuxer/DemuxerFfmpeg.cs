// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DemuxerFfmpeg.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets The x264 Process
        /// </summary>
        protected Process EncodeProcess { get; set; }

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

            using (var encoder = new Process())
            {
                var parameter = new ProcessStartInfo(localExecutable)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
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
                    Log.ErrorFormat("ffmpeg exception: {0}", ex);
                }

                if (started)
                {
                    var output = encoder.StandardError.ReadToEnd();
                    var regObj = new Regex(@"^.*ffmpeg version ([\w\d\.\-_]+)[, ].*$",
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
                if (use64Bit)
                    Log.Debug("Selected 64 bit encoder");
                Log.DebugFormat("ffmpeg \"{0}\" found", verInfo);
            }
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
                if (this.IsEncoding)
                    throw new Exception("ffmpeg is already running");

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                var use64BitEncoder = this._appConfig.Use64BitEncoders &&
                                       this._appConfig.Ffmpeg64Installed &&
                                       Environment.Is64BitOperatingSystem;

                if (this._currentTask.Input == InputType.InputDvd)
                    _inputFile = this._currentTask.DumpOutput;
                else
                    _inputFile = string.IsNullOrEmpty(this._currentTask.TempInput)
                                ? this._currentTask.InputFile
                                : this._currentTask.TempInput;
                this._currentTask.VideoStream.TempFile = _inputFile;

                try
                {
                    this._currentTask.MediaInfo = GenHelper.GetMediaInfo(_inputFile);
                    if (this._currentTask.Input == InputType.InputDvd)
                    {
                        this._currentTask.VideoStream = VideoHelper.GetStreamInfo(this._currentTask.MediaInfo,
                                                                                  this._currentTask.VideoStream,
                                                                                  false);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }

                var query = GenerateCommandLine();
                var ffmpegCliPath = Path.Combine(this._appConfig.ToolsPath,
                                                  use64BitEncoder ? Executable64 : Executable);

                var cliStart = new ProcessStartInfo(ffmpegCliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                };
                this.EncodeProcess = new Process { StartInfo = cliStart };
                Log.InfoFormat("start parameter: ffmpeg {0}", query);

                this.EncodeProcess.Start();

                this._startTime = DateTime.Now;

                this.EncodeProcess.ErrorDataReceived += DemuxerDataReceived;
                this.EncodeProcess.BeginErrorReadLine();

                this._encoderProcessId = this.EncodeProcess.Id;

                // Set the encoder process exit trigger
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

            if (this._currentTask.Input == InputType.InputDvd)
                sb.Append("-probesize 2147483647 -analyzeduration 2147483647 -fflags genpts ");

            sb.AppendFormat("-i \"{0}\" ", _inputFile);

            string baseName;
            string ext;

            var formattedExt = "demuxed.video.mkv";

            if (string.IsNullOrEmpty(this._currentTask.TempInput))
                baseName = string.IsNullOrEmpty(this._currentTask.TempOutput)
                           ? this._currentTask.BaseName
                           : this._currentTask.TempOutput;
            else
                baseName = this._currentTask.TempInput;

            this._currentTask.VideoStream.TempFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                                                     baseName,
                                                                                     formattedExt);

            var streamID = this._currentTask.Input == InputType.InputDvd
                              ? string.Format("#0x{0:X}", this._currentTask.VideoStream.StreamId + 479)
                              : string.Format("0:v:{0:0}", this._currentTask.VideoStream.StreamKindID);

            sb.AppendFormat("-map {0} -c:v copy -y \"{1}\" ", streamID, this._currentTask.VideoStream.TempFile);

            foreach (var item in this._currentTask.AudioStreams)
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

                formattedExt = string.Format("demuxed.audio.{0:g}.{1}.{2}", item.StreamId, item.LangCode, ext);

                item.TempFile =
                    FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation, baseName, formattedExt);

                if (this._currentTask.Input == InputType.InputDvd)
                {
                    var dvdStreamId = item.StreamId;
                    if (String.CompareOrdinal(item.Format.ToLowerInvariant(), "mpeg1") == 0 ||
                        String.CompareOrdinal(item.Format.ToLowerInvariant(), "mpeg2") == 0)
                        dvdStreamId += 256;
                    streamID = string.Format("#0x{0:X}", dvdStreamId);
                }
                else
                    streamID = string.Format("0:a:{0:0}", item.StreamKindId);

                sb.AppendFormat("-map {0} -c:a {1} -y \"{2}\" ", streamID, acodec, item.TempFile);
            }

            foreach (var item in this._currentTask.SubtitleStreams)
            {
                ext = "mkv";

                formattedExt = string.Format("demuxed.subtitle.{0:g}.{1}.{2}", item.StreamId, item.LangCode, ext);

                item.TempFile = FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation, baseName, formattedExt);

                item.RawStream = false;

                streamID = this._currentTask.Input == InputType.InputDvd
                    ? string.Format("#0x{0:X}", item.StreamId)
                    : string.Format("0:s:{0:0}", item.StreamKindId);

                var codec = "copy";

                if (item.Format == "VobSub")
                    codec = "dvd_subtitle";

                sb.AppendFormat("-map {0} -c:s {1} -y \"{2}\" ", streamID, codec, item.TempFile);
            }

            return sb.ToString();
        }

        /// <summary>
        /// The ffmpeg process has exited.
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
                this.EncodeProcess.CancelErrorRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            this._currentTask.ExitCode = EncodeProcess.ExitCode;
            Log.InfoFormat("Exit Code: {0:g}", this._currentTask.ExitCode);

            if (this._currentTask.ExitCode == 0)
            {
                if (this._currentTask.Input == InputType.InputDvd)
                {
                    this._currentTask.TempFiles.Add(this._inputFile);
                }
            }

            this._currentTask.CompletedStep = this._currentTask.NextStep;
            this.InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
        }

        /// <summary>
        /// process received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DemuxerDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && this.IsEncoding)
            {
                this.ProcessLogMessage(e.Data);
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

                var remainingStreamTime = this._currentTask.VideoStream.Length - secDemux;
                
                var elapsedTime = DateTime.Now - this._startTime;

                if (elapsedTime.TotalSeconds > 0)
                    processingSpeed = secDemux / elapsedTime.TotalSeconds;

                if (processingSpeed > 0)
                    secRemaining = (int) Math.Round(remainingStreamTime/processingSpeed, MidpointRounding.ToEven);

                var remainingTime = new TimeSpan(0, 0, secRemaining);

                var progress = (float) Math.Round(secDemux/this._currentTask.VideoStream.Length*100d);

                var eventArgs = new EncodeProgressEventArgs
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
            else
                Log.InfoFormat("ffmpeg: {0}", line);
        }

        #endregion

    }
}