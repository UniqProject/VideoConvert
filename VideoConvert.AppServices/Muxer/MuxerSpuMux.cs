// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MuxerSpuMux.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// The MuxerSpuMux
    /// </summary>
    public class MuxerSpuMux : EncodeBase, IMuxerSpuMux
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (MuxerSpuMux));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "spumux.exe";

        /// <summary>
        /// Gets the Encoder Process ID
        /// </summary>
        private int _encoderProcessId;

        /// <summary>
        /// The Current Task
        /// </summary>
        private EncodeInfo _currentTask;

        private string _inputFile;
        private string _outputFile;

        private SubtitleInfo _sub;

        private FileStream _readStream;
        private FileStream _writeStream;

        private Thread _readFileThread;
        private Thread _writeFileThread;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerSpuMux"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public MuxerSpuMux(IAppConfigService appConfig) : base(appConfig)
        {
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the spumux Process
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
                    Log.ErrorFormat("spumux exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"^.*spumux, version ([\d\.]*)\..*$",
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
                Log.DebugFormat("spumux \"{0}\" found", verInfo);
            }
            return verInfo;
        }

        /// <summary>
        /// Execute a spumux process.
        /// This should only be called from the UI thread.
        /// </summary>
        /// <param name="encodeQueueTask">
        /// The encodeQueueTask.
        /// </param>
        public void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (this.IsEncoding)
                    throw new Exception("spumux is already running");

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                string query = GenerateCommandLine();
                string cliPath = Path.Combine(this._appConfig.ToolsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true
                };
                this.EncodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: spumux {0}", query);

                this._readStream = new FileStream(this._inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                this._writeStream = new FileStream(this._outputFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

                _readFileThread = new Thread(StartReadFile);
                _writeFileThread = new Thread(StartWriteFile);

                this.EncodeProcess.Start();

                _readFileThread.Start();
                _writeFileThread.Start();

                this.EncodeProcess.ErrorDataReceived += EncodeProcessDataReceived;
                this.EncodeProcess.BeginErrorReadLine();

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

        private void StartWriteFile()
        {
            var buffer = new byte[0x10000];
            while (this.IsEncoding && !this.EncodeProcess.HasExited)
            {
                int readOut = this.EncodeProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
                if (readOut > 0)
                    this._writeStream.Write(buffer, 0, readOut);
            }
        }

        private void StartReadFile()
        {
            var buffer = new byte[0x2500];

            while (this.IsEncoding && !this.EncodeProcess.HasExited && this._readStream.Position < this._readStream.Length)
            {
                int readOut = this._readStream.Read(buffer, 0, buffer.Length);
                if (readOut > 0)
                    this.EncodeProcess.StandardInput.BaseStream.Write(buffer, 0, readOut);
            }
        }

        /// <summary>
        /// Kill the CLI process
        /// </summary>
        public override void Stop()
        {
            this.IsEncoding = false;
            try
            {
                if (this.EncodeProcess != null && !this.EncodeProcess.HasExited)
                {
                    Thread.Sleep(2000);
                    this.EncodeProcess.Kill();
                    this._readStream.Close();
                    this._writeStream.Close();
                    this._readFileThread.Abort();
                    this._writeFileThread.Abort();
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

            this._sub = this._currentTask.SubtitleStreams[this._currentTask.StreamId];

            this._inputFile = this._currentTask.VideoStream.TempFile;
            this._outputFile = FileSystemHelper.CreateTempFile(this._appConfig.TempPath, this._inputFile,
                                                               string.Format("+{0}.mpg", this._sub.LangCode));

            sb.AppendFormat("-s {0:0} \"{1}\"", this._currentTask.StreamId, this._sub.TempFile);

            return sb.ToString();
        }

        /// <summary>
        /// The spumux process has exited.
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
                this._currentTask.VideoStream.TempFile = this._outputFile;
                this._currentTask.TempFiles.Add(this._inputFile);
                GetTempImages(this._sub.TempFile);
                this._currentTask.TempFiles.Add(this._sub.TempFile);
                this._currentTask.TempFiles.Add(Path.GetDirectoryName(this._sub.TempFile));
            }

            this._currentTask.CompletedStep = this._currentTask.NextStep;
            this.IsEncoding = false;
            this.InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
        }

        private void GetTempImages(string inFile)
        {
            var inSubFile = new XmlDocument();
            inSubFile.Load(inFile);
            XmlNodeList spuList = inSubFile.SelectNodes("//spu");

            if (spuList != null)
                foreach (XmlNode spu in spuList.Cast<XmlNode>().Where(spu => spu.Attributes != null))
                {
                    Debug.Assert(spu.Attributes != null, "spu.Attributes != null");
                    this._currentTask.TempFiles.Add(spu.Attributes["image"].Value);
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

            Log.InfoFormat("spumux: {0}", line);
        }

        #endregion
    }
}