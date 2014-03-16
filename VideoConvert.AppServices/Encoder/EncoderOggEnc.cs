// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderOggEnc.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The EncoderOggEnc
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Encoder
{
    using Decoder;
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
    using System.IO.Pipes;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using VideoConvert.AppServices.Utilities;
    using VideoConvert.Interop.Model.Profiles;
    using VideoConvert.Interop.Utilities;
    using ThreadState = System.Threading.ThreadState;

    /// <summary>
    /// The EncoderOggEnc
    /// </summary>
    public class EncoderOggEnc : EncodeBase, IEncoderOggEnc
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (EncoderOggEnc));

        #region Private Variables

        private readonly IAppConfigService _appConfig;

        /// <summary>
        /// Gets the Encoder Process ID
        /// </summary>
        private int _encoderProcessId;

        /// <summary>
        /// Get the Decoder Process ID
        /// </summary>
        private int _decoderProcessId;

        private NamedPipeServerStream _encodePipe;
        private IAsyncResult _encodePipeState;
        private Thread _pipeReadThread;

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

        private AudioInfo _audio;

        private readonly Regex _encObj = new Regex(@"^.*Encoding\s*?\[.*\].*$",
            RegexOptions.Singleline | RegexOptions.Multiline);

        private readonly Regex _pipeObj = new Regex(@"^([\d\,\.]*?)%.*$",
            RegexOptions.Singleline | RegexOptions.Multiline);

        private bool _dataWriteStarted;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderOggEnc"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public EncoderOggEnc(IAppConfigService appConfig) : base(appConfig)
        {
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the OggEnc Process
        /// </summary>
        protected Process EncodeProcess { get; set; }

        /// <summary>
        /// Gets or sets the BePipe decoding Process
        /// </summary>
        protected Process DecodeProcess { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads encoder version from its output, use path settings from parameters
        /// </summary>
        /// <param name="encPath">Path to encoder</param>
        /// <param name="optimized"></param>
        /// <param name="appConfig"></param>
        /// <returns>Encoder version</returns>
        public static string GetVersionInfo(string encPath, bool optimized, IAppConfigService appConfig)
        {
            var verInfo = string.Empty;

            var localExecutable = Path.Combine(encPath, BuildExecutable(optimized, appConfig));

            using (var encoder = new Process())
            {
                var parameter = new ProcessStartInfo(localExecutable)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = "-h"
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
                    Log.ErrorFormat("OggEnc exception: {0}", ex);
                }

                if (started)
                {
                    var output = encoder.StandardOutput.ReadToEnd();
                    const string regexNonOptimized = @"^OggEnc\s+?v([\w\d\.\(\)\s]+?)$";
                    const string regexOptimized = @"^OggEnc\s+?v([\d\.]*?)\s+?\(.+?based on (.+?)\s+?\[.+?\]\).*?$";

                    var regObj = new Regex(regexNonOptimized, RegexOptions.Singleline | RegexOptions.Multiline);
                    var result = regObj.Match(output);
                    if (result.Success)
                        verInfo = result.Groups[1].Value.Trim();
                    else
                    {
                        regObj = new Regex(regexOptimized, RegexOptions.Singleline | RegexOptions.Multiline);
                        result = regObj.Match(output);
                        if (result.Success)
                            verInfo = string.Format("{0} ({1})", result.Groups[1].Value, result.Groups[2].Value);
                    }

                    encoder.WaitForExit(10000);
                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
            {
                if (optimized)
                    Log.Debug("Optimized encoder");
                Log.DebugFormat("OggEnc \"{0}\" found", verInfo);
            }
            return verInfo;
        }

        /// <summary>
        /// Starts encoding process with given Encode Job
        /// </summary>
        /// <param name="encodeQueueTask">Job to encode</param>
        public override void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (this.IsEncoding)
                {
                    encodeQueueTask.ExitCode = -1;
                    throw new Exception("OggEnc is already running");
                }

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(this._appConfig.ToolsPath, BuildExecutable(this._appConfig.UseOptimizedEncoders, this._appConfig));

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };

                this.EncodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: OggEnc {0}", query);

                this.DecodeProcess = DecoderBePipe.CreateDecodingProcess(this._inputFile, this._appConfig.AvsPluginsPath);

                this._encodePipe = new NamedPipeServerStream(this._appConfig.EncodeNamedPipeName,
                                                             PipeDirection.InOut,
                                                             3,
                                                             PipeTransmissionMode.Byte,
                                                             PipeOptions.Asynchronous);

                this._encodePipeState = this._encodePipe.BeginWaitForConnection(EncoderConnected, null);

                this.EncodeProcess.Start();
                this.DecodeProcess.Start();

                this._startTime = DateTime.Now;

                this.EncodeProcess.ErrorDataReceived += EncodeProcessDataReceived;
                this.EncodeProcess.BeginErrorReadLine();

                this.DecodeProcess.ErrorDataReceived += DecodeProcessDataReceived;
                this.DecodeProcess.BeginErrorReadLine();

                this._encoderProcessId = this.EncodeProcess.Id;
                this._decoderProcessId = this.DecodeProcess.Id;

                if (this._encoderProcessId != -1)
                {
                    this.EncodeProcess.EnableRaisingEvents = true;
                    this.EncodeProcess.Exited += EncodeProcessExited;
                }

                if (this._decoderProcessId != -1)
                {
                    this.DecodeProcess.EnableRaisingEvents = true;
                    this.DecodeProcess.Exited += DecodeProcessExited;
                }

                this.EncodeProcess.PriorityClass = this._appConfig.GetProcessPriority();
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
                if (this.EncodeProcess != null && !this.EncodeProcess.HasExited)
                {
                    this.EncodeProcess.Kill();
                }
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

        private static string BuildExecutable (bool optimized, IAppConfigService appConfig)
        {
            string fName = "oggenc2";
            if (optimized)
            {
                if (appConfig.SupportedCpuExtensions.SSE3 == 1)
                {
                    fName += "_SSE3";
                    if (Environment.Is64BitOperatingSystem && appConfig.Use64BitEncoders)
                        fName += "_64";
                }
                else if (appConfig.SupportedCpuExtensions.SSE2 == 1)
                    fName += "_SSE2";
                else if (appConfig.SupportedCpuExtensions.SSE == 1)
                    fName += "_SSE";
            }
            fName += ".exe";

            return fName;
        }

        private string GenerateCommandLine()
        {
            var sb = new StringBuilder();

            this._audio = this._currentTask.AudioStreams[this._currentTask.StreamId];

            var outChannels = ((OggProfile)this._currentTask.AudioProfile).OutputChannels;
            switch (outChannels)
            {
                case 1:
                    outChannels = 2;
                    break;
                case 2:
                    outChannels = 1;
                    break;
            }
            var outSampleRate = ((OggProfile)this._currentTask.AudioProfile).SampleRate;
            switch (outSampleRate)
            {
                case 1:
                    outSampleRate = 8000;
                    break;
                case 2:
                    outSampleRate = 11025;
                    break;
                case 3:
                    outSampleRate = 22050;
                    break;
                case 4:
                    outSampleRate = 44100;
                    break;
                case 5:
                    outSampleRate = 48000;
                    break;
                default:
                    outSampleRate = 0;
                    break;
            }

            var encMode = ((OggProfile)this._currentTask.AudioProfile).EncodingMode;
            var bitrate = ((OggProfile)this._currentTask.AudioProfile).Bitrate * 1000;
            var quality = ((OggProfile)this._currentTask.AudioProfile).Quality;

            var avs = new AviSynthGenerator(this._appConfig);

            this._inputFile = avs.GenerateAudioScript(this._audio.TempFile, this._audio.Format, this._audio.FormatProfile,
                                                      this._audio.ChannelCount, outChannels, this._audio.SampleRate,
                                                      outSampleRate);

            this._outputFile = FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation, this._audio.TempFile, "encoded.ogg");

            if (encMode == 2)
                sb.AppendFormat("-q {0:0.00} ", quality);
            else
            {
                if (encMode == 1)
                    sb.Append("--managed ");
                sb.AppendFormat("-b {0:0} ", bitrate);
            }

            sb.AppendFormat("-o \"{0}\" ", this._outputFile);
            sb.AppendFormat("--ignorelength \"{0}\" ", this._appConfig.EncodeNamedPipeFullName);

            return sb.ToString();
        }

        /// <summary>
        /// The OggEnc process has exited.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        private void EncodeProcessExited(object sender, EventArgs e)
        {
            if (this._encodePipe != null)
            {
                try
                {
                    if (!this._encodePipeState.IsCompleted)
                        this._encodePipe.EndWaitForConnection(this._encodePipeState);
                }
                catch (Exception exc)
                {
                    Log.Error(exc);
                }

                try
                {
                    this._encodePipe.Close();
                }
                catch (Exception exc)
                {
                    Log.Error(exc);
                }
            }

            if (this._pipeReadThread != null && this._pipeReadThread.ThreadState == ThreadState.Running)
                this._pipeReadThread.Abort();
            this.EncodeProcess.WaitForExit();

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
                this._currentTask.TempFiles.Add(this._inputFile);
                this._currentTask.TempFiles.Add(this._audio.TempFile);
                this._currentTask.TempFiles.Add(this._audio.TempFile + ".d2a");
                this._currentTask.TempFiles.Add(this._audio.TempFile + ".ffindex");
                this._audio.TempFile = this._outputFile;
                AudioHelper.GetStreamInfo(this._audio);
            }

            this._currentTask.CompletedStep = this._currentTask.NextStep;
            this.IsEncoding = false;
            this.InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
        }

        private void DecodeProcessExited(object sender, EventArgs e)
        {
            if (this._encodePipe != null)
            {
                try
                {
                    if (!this._encodePipeState.IsCompleted)
                        this._encodePipe.EndWaitForConnection(this._encodePipeState);
                }
                catch (Exception exc)
                {
                    Log.Error(exc);
                }

                if (this._pipeReadThread != null && this._pipeReadThread.ThreadState == ThreadState.Running)
                    this._pipeReadThread.Abort();
                this.DecodeProcess.WaitForExit();

                if (this._encodePipe.IsConnected)
                    _encodePipe.Disconnect();
            }
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

            var result = _encObj.Match(line);
            if (!result.Success)
                Log.InfoFormat("OggEnc: {0}", line);
        }

        private void DecodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            var line = e.Data;
            if (string.IsNullOrEmpty(line) || !this.IsEncoding) return;

            if (line.Contains("Writing Data..."))
                this._dataWriteStarted = true;

            var bePipeMatch = _pipeObj.Match(line);
            if (bePipeMatch.Success)
            {
                float progress;
                var tempProgress = bePipeMatch.Groups[1].Value.Replace(",", ".");
                Single.TryParse(tempProgress, NumberStyles.Number, this._appConfig.CInfo, out progress);

                var progressRemaining = 100f - progress;
                var elapsedTime = DateTime.Now - _startTime;

                long secRemaining = 0;
                if (elapsedTime.TotalSeconds > 0)
                {
                    var speed = Math.Round(progress/elapsedTime.TotalSeconds, 6);

                    if (speed > 0)
                        secRemaining = (long) Math.Round(progressRemaining/speed, 0);
                    else
                        secRemaining = 0;
                }
                if (secRemaining < 0)
                    secRemaining = 0;

                var remainingTime = TimeSpan.FromSeconds(secRemaining);

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
                Log.InfoFormat("bepipe: {0}", line);
        }

        #endregion

        #region Stream Piping functions

        private void EncoderConnected(IAsyncResult ar)
        {
            Log.Info("Encoder Pipe connected");
            this._encodePipe.EndWaitForConnection(ar);
            this._pipeReadThread = new Thread(PipeReadThreadStart);
            this._pipeReadThread.Start();
            this._pipeReadThread.Priority = this._appConfig.GetThreadPriority();
        }

        private void PipeReadThreadStart()
        {
            try
            {
                if (this.DecodeProcess != null)
                    ReadThreadStart();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void ReadThreadStart()
        {
            try
            {
                // wait for decoder to start writing
                while (!this._dataWriteStarted)
                {
                    Thread.Sleep(100);
                }

                var buffer = new byte[0xA00000]; // 10 MB

                int read = this.DecodeProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);

                while (read > 0 && !this.DecodeProcess.HasExited)
                {
                    _encodePipe.Write(buffer, 0, read);
                    if (!this.DecodeProcess.HasExited)
                        read = this.DecodeProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
                }
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }
        }

        #endregion
    }
}