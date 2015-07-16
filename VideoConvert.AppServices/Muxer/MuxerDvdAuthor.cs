// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MuxerDvdAuthor.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The MuxerDvdAuthor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Muxer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using log4net;
    using VideoConvert.AppServices.Muxer.Interfaces;
    using VideoConvert.AppServices.Services.Base;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// The MuxerDvdAuthor
    /// </summary>
    public class MuxerDvdAuthor : EncodeBase, IMuxerDvdAuthor
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (MuxerDvdAuthor));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "dvdauthor.exe";

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

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerDvdAuthor"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public MuxerDvdAuthor(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the dvdauthor Process
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
                    Log.Error($"dvdauthor exception: {ex}");
                }

                if (started)
                {
                    var output = encoder.StandardError.ReadToEnd();
                    var regObj = new Regex(@"^.*DVDAuthor::dvdauthor, version ([\d\.\+]*)\..+?$",
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
                Log.Debug($"dvdauthor \"{verInfo}\" found");
            }
            return verInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encodeQueueTask"></param>
        /// <exception cref="Exception"></exception>
        public override void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (IsEncoding)
                {
                    encodeQueueTask.ExitCode = -1;
                    throw new Exception("dvdauthor is already running");
                }

                IsEncoding = true;
                _currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = Path.Combine(_appConfig.ToolsPath, Executable);
                
                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                };
                EncodeProcess = new Process { StartInfo = cliStart };
                Log.Info($"start parameter: dvdauthor {query}");

                EncodeProcess.Start();

                _startTime = DateTime.Now;

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
                InvokeEncodeCompleted(new EncodeCompletedEventArgs(false, exc, exc.Message));
            }
        }

        /// <summary>
        /// Kill the CLI process
        /// </summary>
        public override void Stop()
        {
            try
            {
                if (EncodeProcess != null && !EncodeProcess.HasExited)
                {
                    EncodeProcess.Kill();
                }
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }
            IsEncoding = false;
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
            _outputFile = !string.IsNullOrEmpty(_currentTask.TempOutput)
                            ? _currentTask.TempOutput
                            : _currentTask.OutputFile;

            if (Directory.Exists(_outputFile))
                Directory.Delete(_outputFile, true);

            _inputFile = GenerateXmlInput();
            Log.Info($"dvdauthor xml: {Environment.NewLine}{File.ReadAllText(_inputFile)}");

            sb.Append($"-x \"{_inputFile}\"");

            return sb.ToString();
        }

        private string GenerateXmlInput()
        {
            var xmlFile = FileSystemHelper.CreateTempFile(_appConfig.DemuxLocation, ".xml");

            var chapterString = string.Empty;
            if (_currentTask.Chapters.Count > 1)
            {
                DateTime dt;
                var tempChapters = new List<string>();

                if (_currentTask.Input != InputType.InputDvd)
                {
                    foreach (var chapter in _currentTask.Chapters)
                    {
                        dt = DateTime.MinValue.Add(chapter);
                        tempChapters.Add(dt.ToString("H:mm:ss.fff"));
                    }
                }
                else
                {
                    var actualTime = new TimeSpan();

                    foreach (var chapter in _currentTask.Chapters)
                    {
                        actualTime = actualTime.Add(chapter);
                        dt = DateTime.MinValue.Add(actualTime);
                        tempChapters.Add(dt.ToString("H:mm:ss.fff"));
                    }
                }
                chapterString = string.Join(",", tempChapters.ToArray());
            }

            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                ConformanceLevel = ConformanceLevel.Auto,
                CloseOutput = true
            };
            var writer = XmlWriter.Create(xmlFile, xmlSettings);

            writer.WriteStartDocument(true);
            writer.WriteStartElement("dvdauthor");
            writer.WriteAttributeString("format", _currentTask.EncodingProfile.SystemType == 0 ? "pal" : "ntsc");
            writer.WriteAttributeString("dest", _outputFile.Replace(@"\", "/"));

            writer.WriteStartElement("vmgm");
            writer.WriteEndElement(); // vmgm

            writer.WriteStartElement("titleset");
            writer.WriteStartElement("titles");

            foreach (var language in _currentTask.AudioStreams.Select(item => item.ShortLang).Select(itemlang => LanguageHelper.GetLanguage(string.IsNullOrEmpty(itemlang) ? "xx" : itemlang)))
            {
                writer.WriteStartElement("audio");
                writer.WriteAttributeString("lang", language.Iso1Lang);
                writer.WriteEndElement();
            }

            foreach (var item in _currentTask.SubtitleStreams)
            {
                var language =
                    LanguageHelper.GetLanguage(string.IsNullOrEmpty(item.LangCode) ? "xx" : item.LangCode);

                if (item.Format != "PGS" && item.Format != "VobSub") continue;

                writer.WriteStartElement("subpicture");
                writer.WriteAttributeString("lang", language.Iso1Lang);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("pgc");
            writer.WriteStartElement("vob");
            writer.WriteAttributeString("file", _currentTask.VideoStream.TempFile);
            if (!string.IsNullOrEmpty(chapterString))
                writer.WriteAttributeString("chapters", chapterString);

            writer.WriteEndElement(); //vob
            writer.WriteEndElement(); //pgc
            writer.WriteEndElement(); //titles
            writer.WriteEndElement(); //titleset
            writer.WriteEndElement(); //dvdauthor
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();

            return xmlFile;
        }

        /// <summary>
        /// The dvdauthor process has exited.
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
                _currentTask.TempFiles.Add(_inputFile);
                _currentTask.TempFiles.Add(_currentTask.VideoStream.TempFile);
            }

            _currentTask.CompletedStep = _currentTask.NextStep;
            IsEncoding = false;
            InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
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

        private void ProcessLogMessage(string line)
        {
            if (string.IsNullOrEmpty(line)) return;

            Log.Info($"dvdauthor: {line}");

            var elapsedTime = DateTime.Now - _startTime;
            var remainingTime = elapsedTime + TimeSpan.FromSeconds(1);


            var eventArgs = new EncodeProgressEventArgs
            {
                AverageFrameRate = 0,
                CurrentFrameRate = 0,
                EstimatedTimeLeft = remainingTime,
                PercentComplete = -1,
                ElapsedTime = elapsedTime,
            };
            InvokeEncodeStatusChanged(eventArgs);
        }

        #endregion
    }
}