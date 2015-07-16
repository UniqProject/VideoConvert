// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MuxerSpuMux.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The MuxerSpuMux
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
    using log4net;
    using VideoConvert.AppServices.Muxer.Interfaces;
    using VideoConvert.AppServices.Services.Base;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;
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
            _appConfig = appConfig;
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
                    Log.Error($"spumux exception: {ex}");
                }

                if (started)
                {
                    var output = encoder.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"^.*spumux, version ([\d\.]*)\..*$",
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
                Log.Debug($"spumux \"{verInfo}\" found");
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
        public override void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (IsEncoding)
                {
                    encodeQueueTask.ExitCode = -1;
                    throw new Exception("spumux is already running");
                }

                IsEncoding = true;
                _currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(_appConfig.ToolsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = _appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true
                };
                EncodeProcess = new Process {StartInfo = cliStart};
                Log.Info($"start parameter: spumux {query}");

                _readStream = new FileStream(_inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                _writeStream = new FileStream(_outputFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

                _readFileThread = new Thread(StartReadFile);
                _writeFileThread = new Thread(StartWriteFile);

                EncodeProcess.Start();

                _readFileThread.Start();
                _writeFileThread.Start();

                EncodeProcess.ErrorDataReceived += EncodeProcessDataReceived;
                EncodeProcess.BeginErrorReadLine();

                _encoderProcessId = EncodeProcess.Id;

                if (_encoderProcessId != -1)
                {
                    EncodeProcess.EnableRaisingEvents = true;
                    EncodeProcess.Exited += EncodeProcessExited;
                }

                EncodeProcess.PriorityClass = _appConfig.GetProcessPriority();

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

        private void StartWriteFile()
        {
            var buffer = new byte[0x10000];
            while (IsEncoding && !EncodeProcess.HasExited)
            {
                var readOut = EncodeProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
                if (readOut > 0)
                    _writeStream.Write(buffer, 0, readOut);
            }
        }

        private void StartReadFile()
        {
            var buffer = new byte[0x2500];

            while (IsEncoding && !EncodeProcess.HasExited && _readStream.Position < _readStream.Length)
            {
                var readOut = _readStream.Read(buffer, 0, buffer.Length);
                if (readOut > 0)
                    EncodeProcess.StandardInput.BaseStream.Write(buffer, 0, readOut);
            }
        }

        /// <summary>
        /// Kill the CLI process
        /// </summary>
        public override void Stop()
        {
            IsEncoding = false;
            try
            {
                if (EncodeProcess == null || EncodeProcess.HasExited) return;

                Thread.Sleep(2000);
                EncodeProcess.Kill();
                _readStream.Close();
                _writeStream.Close();
                _readFileThread.Abort();
                _writeFileThread.Abort();
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

            _sub = _currentTask.SubtitleStreams[_currentTask.StreamId];

            _inputFile = _currentTask.VideoStream.TempFile;
            _outputFile = FileSystemHelper.CreateTempFile(_appConfig.TempPath, _inputFile,
                                                          $"+{_sub.LangCode}.mpg");

            sb.Append($"-s {_currentTask.StreamId:0} \"{_sub.TempFile}\"");

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
                EncodeProcess.CancelErrorRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            _currentTask.ExitCode = EncodeProcess.ExitCode;
            Log.Info($"Exit Code: {_currentTask.ExitCode:0}");

            if (_currentTask.ExitCode == 0)
            {
                _currentTask.VideoStream.TempFile = _outputFile;
                _currentTask.TempFiles.Add(_inputFile);
                GetTempImages(_sub.TempFile);
                _currentTask.TempFiles.Add(_sub.TempFile);
                _currentTask.TempFiles.Add(Path.GetDirectoryName(_sub.TempFile));
            }

            _currentTask.CompletedStep = _currentTask.NextStep;
            IsEncoding = false;
            InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
        }

        private void GetTempImages(string inFile)
        {
            var inSubFile = new XmlDocument();
            inSubFile.Load(inFile);
            var spuList = inSubFile.SelectNodes("//spu");

            if (spuList == null) return;

            foreach (var spu in spuList.Cast<XmlNode>().Where(spu => spu.Attributes != null))
            {
                Debug.Assert(spu.Attributes != null, "spu.Attributes != null");
                _currentTask.TempFiles.Add(spu.Attributes["image"].Value);
            }
        }

        /// <summary>
        /// process received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncodeProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && IsEncoding)
            {
                ProcessLogMessage(e.Data);
            }
        }

        private static void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            Log.Info($"spumux: {line}");
        }

        #endregion
    }
}