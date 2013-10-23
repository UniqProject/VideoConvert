// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MuxerDvdAuthor.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
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
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using Interop.Utilities;
    using log4net;
    using Services.Base;
    using Services.Interfaces;

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
            this._appConfig = appConfig;
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
            string verInfo = string.Empty;

            string localExecutable = Path.Combine(encPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
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
                    Log.ErrorFormat("dvdauthor exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardError.ReadToEnd();
                    Regex regObj = new Regex(@"^.*DVDAuthor::dvdauthor, version ([\d\.\+]*)\..+?$",
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
                Log.DebugFormat("dvdauthor \"{0}\" found", verInfo);
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
                    throw new Exception("dvdauthor is already running");

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                string query = GenerateCommandLine();
                string cliPath = Path.Combine(this._appConfig.ToolsPath, Executable);
                
                ProcessStartInfo cliStart = new ProcessStartInfo(cliPath, query)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                };
                this.EncodeProcess = new Process { StartInfo = cliStart };
                Log.InfoFormat("start parameter: dvdauthor {0}", query);

                this.EncodeProcess.Start();

                this._startTime = DateTime.Now;

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
            _outputFile = !string.IsNullOrEmpty(this._currentTask.TempOutput)
                            ? this._currentTask.TempOutput
                            : this._currentTask.OutputFile;

            if (Directory.Exists(_outputFile))
                Directory.Delete(_outputFile, true);

            _inputFile = GenerateXmlInput();
            Log.InfoFormat("dvdauthor xml: {0}{1}", Environment.NewLine, File.ReadAllText(_inputFile));

            sb.AppendFormat("-x \"{0}\"", _inputFile);

            return sb.ToString();
        }

        private string GenerateXmlInput()
        {
            string xmlFile = FileSystemHelper.CreateTempFile(this._appConfig.DemuxLocation, ".xml");

            string chapterString = string.Empty;
            if (this._currentTask.Chapters.Count > 1)
            {
                DateTime dt;
                List<string> tempChapters = new List<string>();

                if (this._currentTask.Input != InputType.InputDvd)
                {
                    foreach (TimeSpan chapter in this._currentTask.Chapters)
                    {
                        dt = DateTime.MinValue.Add(chapter);
                        tempChapters.Add(dt.ToString("H:mm:ss.fff"));
                    }
                }
                else
                {
                    TimeSpan actualTime = new TimeSpan();

                    foreach (TimeSpan chapter in this._currentTask.Chapters)
                    {
                        actualTime = actualTime.Add(chapter);
                        dt = DateTime.MinValue.Add(actualTime);
                        tempChapters.Add(dt.ToString("H:mm:ss.fff"));
                    }
                }
                chapterString = string.Join(",", tempChapters.ToArray());
            }

            XmlWriterSettings xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                ConformanceLevel = ConformanceLevel.Auto,
                CloseOutput = true
            };
            XmlWriter writer = XmlWriter.Create(xmlFile, xmlSettings);

            writer.WriteStartDocument(true);
            writer.WriteStartElement("dvdauthor");
            writer.WriteAttributeString("format", this._currentTask.EncodingProfile.SystemType == 0 ? "pal" : "ntsc");
            writer.WriteAttributeString("dest", _outputFile.Replace(@"\", "/"));

            writer.WriteStartElement("vmgm");
            writer.WriteEndElement(); // vmgm

            writer.WriteStartElement("titleset");
            writer.WriteStartElement("titles");

            foreach (string itemlang in this._currentTask.AudioStreams.Select(item => item.ShortLang))
            {
                LanguageHelper language = LanguageHelper.GetLanguage(string.IsNullOrEmpty(itemlang) ? "xx" : itemlang);

                writer.WriteStartElement("audio");
                writer.WriteAttributeString("lang", language.Iso1Lang);
                writer.WriteEndElement();
            }

            foreach (SubtitleInfo item in this._currentTask.SubtitleStreams)
            {
                LanguageHelper language =
                    LanguageHelper.GetLanguage(string.IsNullOrEmpty(item.LangCode) ? "xx" : item.LangCode);

                if (item.Format != "PGS" && item.Format != "VobSub") continue;

                writer.WriteStartElement("subpicture");
                writer.WriteAttributeString("lang", language.Iso1Lang);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("pgc");
            writer.WriteStartElement("vob");
            writer.WriteAttributeString("file", this._currentTask.VideoStream.TempFile);
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
                this._currentTask.TempFiles.Add(_inputFile);
                this._currentTask.TempFiles.Add(this._currentTask.VideoStream.TempFile);
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

            Log.InfoFormat("dvdauthor: {0}", line);

            TimeSpan elapsedTime = DateTime.Now - this._startTime;
            TimeSpan remainingTime = elapsedTime + TimeSpan.FromSeconds(1);


            EncodeProgressEventArgs eventArgs = new EncodeProgressEventArgs
            {
                AverageFrameRate = 0,
                CurrentFrameRate = 0,
                EstimatedTimeLeft = remainingTime,
                PercentComplete = -1,
                Task = 0,
                TaskCount = 0,
                ElapsedTime = elapsedTime,
            };
            this.InvokeEncodeStatusChanged(eventArgs);
        }

        #endregion
    }
}