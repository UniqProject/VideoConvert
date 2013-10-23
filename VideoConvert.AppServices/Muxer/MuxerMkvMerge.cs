// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MuxerMkvMerge.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
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
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using Interop.Utilities;
    using log4net;
    using Services.Base;
    using Services.Interfaces;

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
            this._appConfig = appConfig;
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
            string verInfo = string.Empty;

            string localExecutable = Path.Combine(encPath, Executable);

            const string query = DefaultParams + "-V";

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable, query)
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
                    Log.ErrorFormat("mkvmerge exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    Regex regObj = new Regex(@"^mkvmerge v([\d\.]+ \(.*\)).*built.*$",
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
                Log.DebugFormat("mkvmerge \"{0}\" found", verInfo);
            }
            return verInfo;
        }

        public void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (this.IsEncoding)
                    throw new Exception("mkvmerge is already running");

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                string query = GenerateCommandLine();
                string mkvmergeCliPath = Path.Combine(this._appConfig.ToolsPath, Executable);

                ProcessStartInfo cliStart = new ProcessStartInfo(mkvmergeCliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                this.EncodeProcess = new Process { StartInfo = cliStart };
                Log.InfoFormat("start parameter: mkvmerge {0}", query);

                this.EncodeProcess.Start();

                this._startTime = DateTime.Now;

                this.EncodeProcess.OutputDataReceived += MuxerDataReceived;
                this.EncodeProcess.BeginOutputReadLine();

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
                this.IsEncoding = false;
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
            sb.Append(DefaultParams);

            float fps = this._currentTask.VideoStream.FPS;
            int vidStream;

            string tempExt = Path.GetExtension(this._currentTask.VideoStream.TempFile);
            if (this._currentTask.VideoStream.IsRawStream ||
                (this._currentTask.Input == InputType.InputAvi && !this._currentTask.VideoStream.Encoded) ||
                this._currentTask.VideoStream.Encoded)
                vidStream = 0;
            else if (!this._currentTask.VideoStream.Encoded && (tempExt == ".mp4" || tempExt == ".mkv"))
                vidStream = Math.Max(this._currentTask.VideoStream.StreamId - 1, 0);
            else
                vidStream = this._currentTask.VideoStream.StreamId;

            string streamOrder = string.Format(" --track-order 0:{0:g}", vidStream);

            _outFile = !string.IsNullOrEmpty(this._currentTask.TempOutput)
                       ? this._currentTask.TempOutput
                       : this._currentTask.OutputFile;

            sb.AppendFormat("-o \"{0}\" ", _outFile);

            if (this._currentTask.EncodingProfile.OutFormat == OutputType.OutputWebM)
                sb.Append("--webm ");

            string fpsStr = string.Empty;
            if (this._currentTask.VideoStream.IsRawStream)
            {
                if (this._currentTask.VideoStream.FrameRateEnumerator == 0)
                    fpsStr = string.Format(_appConfig.CInfo, "--default-duration {1:g}:{0:0.000}fps", fps, vidStream);
                else
                    fpsStr = string.Format(_appConfig.CInfo, "--default-duration {0:g}:{1:g}/{2:g}fps",
                                           vidStream,
                                           this._currentTask.VideoStream.FrameRateEnumerator,
                                           this._currentTask.VideoStream.FrameRateDenominator);
            }

            int stereoMode;

            switch (this._currentTask.EncodingProfile.StereoType)
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

            sb.AppendFormat(_appConfig.CInfo,
                            "--language {1:g}:eng {0:s} --default-track {1:g}:yes --forced-track {1:g}:yes ",
                            fpsStr, vidStream);

            if (stereoMode > 0)
                sb.AppendFormat(_appConfig.CInfo, "--stereo-mode {1:g}:{0:g} ", stereoMode, vidStream);

            sb.AppendFormat(_appConfig.CInfo,
                            "-d {1:g} -A -S --no-global-tags --no-chapters --compression {1:g}:none \"{0:s}\" ",
                            this._currentTask.VideoStream.TempFile, vidStream);

            int i = 1;
            bool defaultAudioExists = false;
            foreach (AudioInfo item in this._currentTask.AudioStreams)
            {
                string isDefault;
                if (item.MkvDefault && !defaultAudioExists)
                {
                    isDefault = "yes";
                    defaultAudioExists = true;
                }
                else
                    isDefault = "no";

                string itemlang = item.LangCode;

                if (itemlang == "xx" || string.IsNullOrEmpty(itemlang))
                    itemlang = "und";

                string delayString = string.Empty;

                if (item.Delay != 0)
                    delayString = string.Format(_appConfig.CInfo, "--sync 0:{0:#}", item.Delay);

                int itemStream = 0;

                if (Path.GetExtension(item.TempFile) == ".mkv")
                    itemStream = 1;

                sb.AppendFormat(_appConfig.CInfo,
                                "--language {0:g}:{1:s} {2:s} --default-track {0:g}:{3:s} --forced-track {0:g}:no -D -a {0:g} ",
                                itemStream,
                                itemlang,
                                delayString,
                                isDefault);
                sb.AppendFormat(_appConfig.CInfo,
                                "-S --no-global-tags --no-chapters --compression {1:g}:none \"{0:s}\" ",
                                item.TempFile, itemStream);

                streamOrder += string.Format(_appConfig.CInfo, ",{0:g}:0", i);
                i++;
            }

            bool defaultSubExists = false;
            if (this._currentTask.EncodingProfile.OutFormat != OutputType.OutputWebM)
            {
                foreach (SubtitleInfo item in this._currentTask.SubtitleStreams.Where(item => !item.HardSubIntoVideo && File.Exists(item.TempFile)))
                {
                    string isDefault;
                    if (item.MkvDefault && !defaultSubExists)
                    {
                        isDefault = "yes";
                        defaultSubExists = true;
                    }
                    else
                        isDefault = "no";

                    string itemlang = item.LangCode;

                    int subId;

                    if (itemlang == "xx" || string.IsNullOrEmpty(itemlang))
                        itemlang = "und";

                    string subFile = item.TempFile;

                    if (string.IsNullOrEmpty(subFile))
                    {
                        subFile = this._currentTask.InputFile;
                        subId = item.Id;
                    }
                    else
                        subId = 0;

                    string delayString = string.Empty;

                    if (subFile != this._currentTask.InputFile && (item.Delay != 0 && item.Delay != int.MinValue))
                        delayString = string.Format(_appConfig.CInfo, "--sync {0:g}:{1:g}", subId, item.Delay);

                    sb.AppendFormat(_appConfig.CInfo,
                                    "--language {0:g}:{1:s} {2:s} --default-track {0:g}:{3:s} --forced-track {0:g}:no -s {0:g} ",
                                    subId,
                                    itemlang,
                                    delayString,
                                    isDefault);

                    sb.AppendFormat(_appConfig.CInfo,
                                    "-D -A --no-global-tags --no-chapters --compression {1:g}:none \"{0:s}\" ",
                                    subFile, subId);

                    streamOrder += string.Format(_appConfig.CInfo, ",{0:g}:{1:g}", i, subId);
                    i++;
                }
            }

            string chapterString = string.Empty;

            if (this._currentTask.Chapters.Count > 1 && this._currentTask.EncodingProfile.OutFormat != OutputType.OutputWebM)
            {
                string chapterFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation,
                                                                    !string.IsNullOrEmpty(this._currentTask.TempOutput)
                                                                        ? this._currentTask.TempOutput
                                                                        : this._currentTask.OutputFile,
                                                                    "chapters.txt");

                using (StreamWriter chapters = new StreamWriter(chapterFile))
                {

                    string chapterFormatTimes;
                    string chapterFormatNames;

                    if (this._currentTask.Chapters.Count > 100)
                    {
                        chapterFormatTimes = "CHAPTER{0:000}={1:s}";
                        chapterFormatNames = "CHAPTER{0:000}NAME=Chapter {0:g}";
                    }
                    else
                    {
                        chapterFormatTimes = "CHAPTER{0:00}={1:s}";
                        chapterFormatNames = "CHAPTER{0:00}NAME=Chapter {0:g}";
                    }

                    DateTime dt;

                    if (this._currentTask.Input != InputType.InputDvd)
                    {
                        for (int j = 0; j < this._currentTask.Chapters.Count; j++)
                        {
                            dt = DateTime.MinValue.Add(this._currentTask.Chapters[j]);
                            chapters.WriteLine(chapterFormatTimes, j + 1, dt.ToString("H:mm:ss.fff"));
                            chapters.WriteLine(chapterFormatNames, j + 1);
                        }
                    }
                    else
                    {
                        TimeSpan actualTime = new TimeSpan();
                        for (int j = 0; j < this._currentTask.Chapters.Count; j++)
                        {
                            actualTime = actualTime.Add(this._currentTask.Chapters[j]);
                            dt = DateTime.MinValue.Add(actualTime);
                            chapters.WriteLine(chapterFormatTimes, j + 1, dt.ToString("H:mm:ss.fff"));
                            chapters.WriteLine(chapterFormatNames, j + 1);
                        }
                    }
                }

                chapterString = string.Format(" --chapters \"{0}\"", chapterFile);
                this._currentTask.TempFiles.Add(chapterFile);
            }

            sb.AppendFormat("{0} --compression -1:none {1}", chapterString, streamOrder);

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
                this.EncodeProcess.CancelOutputRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            this._currentTask.ExitCode = EncodeProcess.ExitCode;
            Log.InfoFormat("Exit Code: {0:g}", this._currentTask.ExitCode);

            if (this._currentTask.ExitCode < 2)
            {
                this._currentTask.TempFiles.Add(this._currentTask.VideoStream.TempFile);
                foreach (AudioInfo item in this._currentTask.AudioStreams)
                    this._currentTask.TempFiles.Add(item.TempFile);
                foreach (SubtitleInfo item in this._currentTask.SubtitleStreams)
                    this._currentTask.TempFiles.Add(item.TempFile);

                this._currentTask.ExitCode = 0;
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
        private void MuxerDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && this.IsEncoding)
            {
                this.ProcessLogMessage(e.Data);
            }
        }

        private void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            Match result = _regObj.Match(line);

            double processingSpeed = 0f;
            int secRemaining = 0;
            if (result.Success)
            {
                int progress;
                Int32.TryParse(result.Groups[1].Value, NumberStyles.Number, _appConfig.CInfo, out progress);

                TimeSpan elapsedTime = DateTime.Now - this._startTime;

                if (elapsedTime.TotalSeconds > 0)
                    processingSpeed = progress / elapsedTime.TotalSeconds;

                if (processingSpeed > 0)
                    secRemaining = (int)Math.Round((100D - progress) / processingSpeed, MidpointRounding.ToEven);

                TimeSpan remainingTime = new TimeSpan(0, 0, secRemaining);

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
            else
                Log.InfoFormat("mkvmerge: {0}", line);
        }

        #endregion
    }
}