// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderNeroAac.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Encoder
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Pipes;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Decoder;
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using Interop.Model.Profiles;
    using Interop.Utilities;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using Utilities;
    using ThreadState = System.Threading.ThreadState;

    /// <summary>
    /// The EncoderNeroAac
    /// </summary>
    public class EncoderNeroAac : EncodeBase, IEncoderNeroAac
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (EncoderNeroAac));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "neroAacEnc.exe";

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

        private readonly Regex _encObj = new Regex(@"^.*Processed.*\d*?.*seconds...$",
                                                   RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly Regex _pipeObj = new Regex(@"^([\d\,\.]*?)%.*$",
                                                    RegexOptions.Singleline | RegexOptions.Multiline);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderNeroAac"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public EncoderNeroAac(IAppConfigService appConfig) : base(appConfig)
        {
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the neroAacEnc Process
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
                    RedirectStandardError = true,
                    Arguments = "-help"
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
                    Log.ErrorFormat("neroAacEnc exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardError.ReadToEnd();
                    string[] lines = output.Split(new[] { Environment.NewLine },
                                                  StringSplitOptions.RemoveEmptyEntries);

                    Regex regObj = new Regex(@"^\*.*Package version:.* ([\d\.]+?) .*\*$",
                                             RegexOptions.Singleline | RegexOptions.Multiline);
                    foreach (string line in lines)
                    {
                        Match result = regObj.Match(line);
                        if (!result.Success) continue;

                        verInfo = result.Groups[1].Value.Trim();
                        break;
                    }

                    encoder.WaitForExit(10000);
                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("neroAacEnc \"{0}\" found", verInfo);
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
                    throw new Exception("neroAacEnc is already running");

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                string query = GenerateCommandLine();
                string cliPath = Path.Combine(this._appConfig.ToolsPath, Executable);

                ProcessStartInfo cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };

                this.EncodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: neroAacEnc {0}", query);

                this.DecodeProcess = DecoderBePipe.CreateDecodingProcess(_inputFile, this._appConfig.AvsPluginsPath);

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

        private void DecodeProcessExited(object sender, EventArgs e)
        {
            if (this._encodePipe != null)
            {
                try
                {
                    _encodePipe.EndWaitForConnection(_encodePipeState);
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

        private void EncoderConnected(IAsyncResult ar)
        {
            Log.Info("Encoder Pipe connected");
            _encodePipeState = ar;
            this._pipeReadThread = new Thread(PipeReadThreadStart);
            this._pipeReadThread.Start();
            this._pipeReadThread.Priority = this._appConfig.GetThreadPriority();
        }

        private void PipeReadThreadStart()
        {
            try
            {
                if (DecodeProcess != null)
                    ReadThreadStart();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void ReadThreadStart()
        {
            if (!_encodePipe.IsConnected)
            {
                _encodePipe.WaitForConnection();
            }

            try
            {
                DecodeProcess.StandardOutput.BaseStream.CopyTo(_encodePipe);
            }
            catch (Exception exc)
            {
                Log.Error(exc);
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

        private string GenerateCommandLine()
        {
            StringBuilder sb = new StringBuilder();

            this._audio = this._currentTask.AudioStreams[this._currentTask.StreamId];

            int outChannels = ((AACProfile)this._currentTask.AudioProfile).OutputChannels;
            switch (outChannels)
            {
                case 1:
                    outChannels = 2;
                    break;
                case 2:
                    outChannels = 1;
                    break;
            }
            int outSampleRate = ((AACProfile)this._currentTask.AudioProfile).SampleRate;
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

            int encMode = ((AACProfile)this._currentTask.AudioProfile).EncodingMode;
            int bitrate = ((AACProfile)this._currentTask.AudioProfile).Bitrate * 1000;
            float quality = ((AACProfile)this._currentTask.AudioProfile).Quality;

            AviSynthGenerator avs = new AviSynthGenerator(this._appConfig);

            this._inputFile = avs.GenerateAudioScript(this._audio.TempFile, this._audio.Format, this._audio.FormatProfile,
                                                      this._audio.ChannelCount, outChannels, this._audio.SampleRate,
                                                      outSampleRate);

            this._outputFile = FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation, this._audio.TempFile, "encoded.m4a");

            switch (encMode)
            {
                case 0:
                    sb.AppendFormat(this._appConfig.CInfo, "-br {0:0} ", bitrate);
                    break;
                case 1:
                    sb.AppendFormat(this._appConfig.CInfo, "-cbr {0:0} ", bitrate);
                    break;
                case 2:
                    sb.AppendFormat(this._appConfig.CInfo, "-q {0:0.00} ", quality);
                    break;
            }

            sb.Append("-ignorelength -if - ");
            sb.AppendFormat("-of \"{0}\" ", this._outputFile);

            return sb.ToString();
        }

        /// <summary>
        /// The neroAacEnc process has exited.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        private void EncodeProcessExited(object sender, EventArgs e)
        {
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

        private void DecodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            string line = e.Data;
            if (string.IsNullOrEmpty(line) || !this.IsEncoding) return;

            Match bePipeMatch = _pipeObj.Match(line);
            if (bePipeMatch.Success)
            {
                float progress;
                string tempProgress = bePipeMatch.Groups[1].Value.Replace(",", ".");
                Single.TryParse(tempProgress, NumberStyles.Number, this._appConfig.CInfo, out progress);

                float progressRemaining = 100f - progress;
                TimeSpan elapsedTime = DateTime.Now - _startTime;

                long secRemaining = 0;
                if (elapsedTime.TotalSeconds > 0)
                {
                    double speed = Math.Round(progress / elapsedTime.TotalSeconds, 6);

                    if (speed > 0)
                        secRemaining = (long)Math.Round(progressRemaining / speed, 0);
                    else
                        secRemaining = 0;
                }
                if (secRemaining < 0)
                    secRemaining = 0;

                TimeSpan remainingTime = TimeSpan.FromSeconds(secRemaining);

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
                Log.InfoFormat("bepipe: {0}", line);
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

            Match result = _encObj.Match(line);
            if (!result.Success)
                Log.InfoFormat("neroAacEnc: {0}", line);
        }

        #endregion
    }
}