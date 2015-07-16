// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MuxerMp4Box.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The MuxerMp4Box
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Muxer
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using log4net;
    using VideoConvert.AppServices.Muxer.Interfaces;
    using VideoConvert.AppServices.Services;
    using VideoConvert.AppServices.Services.Base;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// The MuxerMp4Box
    /// </summary>
    public class MuxerMp4Box : EncodeBase, IMuxerMp4Box
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (MuxerMp4Box));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "mp4box.exe";

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

        private string _outputFile;
        private int _lastImportPercent;
        private int _streamImportCount;
        private int _muxStepCount;
        private int _muxStep;
        private float _singleStep;

        private readonly Regex _importingRegex = new Regex(@"^Importing ([\w-\d\(\) ]*|ISO File): \|.+?\| \((\d+?)\/\d+?\)$",
                                                           RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly Regex _progressRegex = new Regex(@"^ISO File Writing: \|.+?\| \((\d+?)\/\d+?\)$",
                                                        RegexOptions.Singleline | RegexOptions.Multiline);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MuxerMp4Box"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public MuxerMp4Box(IAppConfigService appConfig) : base(appConfig)
        {
            _appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the mp4box Process
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
                    Arguments = "-version",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
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
                    Log.Error($"mp4box exception: {ex}");
                }

                if (started)
                {
                    var output = encoder.StandardOutput.ReadToEnd();
                    if (string.IsNullOrEmpty(output))
                        output = encoder.StandardError.ReadToEnd();
                    var regObj = new Regex(@"^MP4Box.+?version ([\d\w\.\-\ \(\)]+)\s*$",
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
                Log.Debug($"mp4box \"{verInfo}\" found");
            }
            return verInfo;
        }

        /// <summary>
        /// execute a mp4box mux process
        /// </summary>
        /// <param name="encodeQueueTask">
        /// The encodeQueueTask.
        /// </param>
        /// <exception cref="Exception"></exception>
        public override void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (IsEncoding)
                {
                    encodeQueueTask.ExitCode = -1;
                    throw new Exception("mp4box is already running");
                }

                IsEncoding = true;
                _currentTask = encodeQueueTask;
                
                _lastImportPercent = 0;
                _streamImportCount = 0;
                _muxStep = 0;
                
                var query = GenerateCommandLine();

                _muxStepCount = _streamImportCount + 1;
                _singleStep = 1f / _muxStepCount;

                var cliPath = Path.Combine(_appConfig.ToolsPath, Executable);

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = _appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                EncodeProcess = new Process {StartInfo = cliStart};
                Log.Info($"start parameter: mp4box {query}");

                EncodeProcess.Start();

                _startTime = DateTime.Now;
                
                EncodeProcess.ErrorDataReceived += EncodeProcessDataReceived;
                EncodeProcess.BeginErrorReadLine();

                EncodeProcess.OutputDataReceived += EncodeProcessDataReceived;
                EncodeProcess.BeginOutputReadLine();

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

            var fps = _currentTask.VideoStream.Fps;
            int vidStream;

            var tempExt = Path.GetExtension(_currentTask.VideoStream.TempFile);
            if (_currentTask.VideoStream.IsRawStream)
                vidStream = 0;
            else if ((_currentTask.Input == InputType.InputAvi) && (!_currentTask.VideoStream.Encoded))
                vidStream = 0;
            else if ((_currentTask.VideoStream.Encoded) || (tempExt == ".mp4"))
                vidStream = 0;
            else
                vidStream = _currentTask.VideoStream.StreamId;

            _outputFile = !string.IsNullOrEmpty(_currentTask.TempOutput)
                        ? _currentTask.TempOutput
                        : _currentTask.OutputFile;

            var fpsStr = string.Empty;
            if (_currentTask.VideoStream.IsRawStream)
            {
                fpsStr = _currentTask.VideoStream.FrameRateEnumerator == 0 || _appConfig.LastMp4BoxVer.StartsWith("0.5")
                    ? $":fps={fps:0.000}".ToString(_appConfig.CInfo)
                    : $":fps={_currentTask.VideoStream.FrameRateEnumerator:0}/{_currentTask.VideoStream.FrameRateDenominator:0}";
            }

            sb.Append($"-add \"{_currentTask.VideoStream.TempFile}#video:trackID={vidStream:0}{fpsStr}:lang=eng\" -keep-sys ");

            _streamImportCount = 1;

            foreach (var item in _currentTask.AudioStreams)
            {
                var itemlang = item.LangCode;

                if ((itemlang == "xx") || (string.IsNullOrEmpty(itemlang)))
                    itemlang = "und";

                var delayString = string.Empty;

                if (item.Delay != 0)
                    delayString = $":delay={item.Delay:0}";

                sb.Append($"-add \"{item.TempFile}#audio:lang={itemlang}{delayString}\" -keep-sys ");
                _streamImportCount++;
            }

            foreach (var item in _currentTask.SubtitleStreams)
            {
                if (item.Format.ToLowerInvariant() != "utf-8") continue;
                if (!File.Exists(item.TempFile)) continue;

                var itemlang = item.LangCode;

                if ((itemlang == "xx") || (string.IsNullOrEmpty(itemlang)))
                    itemlang = "und";

                var delayString = string.Empty;

                if (item.Delay != 0)
                    delayString = $":delay={item.Delay:0}";

                sb.Append($"-add \"{item.TempFile}#lang={itemlang}{delayString}:name={LanguageHelper.GetLanguage(itemlang).FullLang}\" -keep-sys ");
                _streamImportCount++;
            }

            if (_currentTask.Chapters.Count > 1)
            {
                var chapterFile = FileSystemHelper.CreateTempFile(
                                                        _appConfig.DemuxLocation,
                                                        !string.IsNullOrEmpty(_currentTask.TempOutput)
                                                            ? _currentTask.TempOutput
                                                            : _currentTask.OutputFile,
                                                        "chapters.ttxt");

                var xmlSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ",
                    Encoding = Encoding.UTF8,
                    NewLineHandling = NewLineHandling.Entitize,
                    ConformanceLevel = ConformanceLevel.Auto,
                    CloseOutput = true
                };
                var writer = XmlWriter.Create(chapterFile, xmlSettings);
                writer.WriteStartDocument(true);
                writer.WriteStartElement("TextStream");
                writer.WriteAttributeString("version", "1.1");

                int temp;
                var subHeight = Math.DivRem(_currentTask.VideoStream.Height, 3, out temp);
                subHeight += temp;

                writer.WriteStartElement("TextStreamHeader");
                writer.WriteAttributeString("width", _currentTask.VideoStream.Width.ToString("G"));
                writer.WriteAttributeString("height", subHeight.ToString("G"));
                writer.WriteAttributeString("layer", "0");
                writer.WriteAttributeString("translation_x", "0");
                writer.WriteAttributeString("translation_y", "0");

                writer.WriteStartElement("TextSampleDescription");
                writer.WriteAttributeString("horizontalJustification", "center");
                writer.WriteAttributeString("verticalJustification", "bottom");
                writer.WriteAttributeString("backColor", "0 0 0 0");
                writer.WriteAttributeString("verticalText", "no");
                writer.WriteAttributeString("fillTextRegion", "no");
                writer.WriteAttributeString("continousKaraoke", "no");
                writer.WriteAttributeString("scroll", "None");

                writer.WriteStartElement("FontTable");

                writer.WriteStartElement("FontTableEntry");
                writer.WriteAttributeString("fontName", "Arial");
                writer.WriteAttributeString("fontID", "1");
                writer.WriteEndElement(); // FontTableEntry

                writer.WriteEndElement(); // FontTable

                writer.WriteStartElement("TextBox");
                writer.WriteAttributeString("top", "0");
                writer.WriteAttributeString("left", "0");
                writer.WriteAttributeString("bottom", _currentTask.VideoStream.Height.ToString("G"));
                writer.WriteAttributeString("right", _currentTask.VideoStream.Width.ToString("G"));
                writer.WriteEndElement(); // TextBox

                writer.WriteStartElement("Style");
                writer.WriteAttributeString("styles", "Normal");
                writer.WriteAttributeString("fontID", "1");
                writer.WriteAttributeString("fontSize", "32");
                writer.WriteAttributeString("color", "ff ff ff ff");
                writer.WriteEndElement(); // Style

                writer.WriteEndElement(); // TextSampleDescription

                writer.WriteEndElement(); // TextStreamHeader

                for (var index = 0; index < _currentTask.Chapters.Count; index++)
                {
                    var dt = DateTime.MinValue.Add(_currentTask.Chapters[index]);
                    writer.WriteStartElement("TextSample");
                    writer.WriteAttributeString("sampleTime", dt.ToString("HH:mm:ss.fff"));
                    writer.WriteValue($"Chapter {index + 1:0}");
                    writer.WriteEndElement(); // TextSample
                }

                writer.WriteEndElement(); // TextStream
                writer.WriteEndDocument();

                writer.Flush();
                writer.Close();

                sb.Append($" -add \"{chapterFile}:chap\"");
                _currentTask.TempFiles.Add(chapterFile);
                _streamImportCount++;
            }

            var tool = $"{AppConfigService.GetProductName()} v{AppConfigService.GetAppVersion().ToString(4)}";
            var tempPath = _appConfig.DemuxLocation;

            sb.Append($"-itags tool=\"{tool}\" -tmp \"{tempPath}\" -new \"{_outputFile}\"");

            return sb.ToString();
        }

        /// <summary>
        /// The mp4box process has exited.
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
                EncodeProcess.CancelOutputRead();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }

            _currentTask.ExitCode = EncodeProcess.ExitCode;
            Log.Info($"Exit Code: {_currentTask.ExitCode:0}");

            if (_currentTask.ExitCode == 0)
            {
                _currentTask.TempFiles.Add(_currentTask.VideoStream.TempFile);
                foreach (var stream in _currentTask.AudioStreams)
                    _currentTask.TempFiles.Add(stream.TempFile);
                foreach (var stream in _currentTask.SubtitleStreams)
                    _currentTask.TempFiles.Add(stream.TempFile);
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
            
            line = line.Trim();
            if (string.IsNullOrEmpty(line)) return;

            var progressResult = _progressRegex.Match(line);
            var importResult = _importingRegex.Match(line);
            var elapsedTime = DateTime.Now - _startTime;

            var progress = 0f;
            double processingSpeed = 0f;
            var secRemaining = 0;

            if (progressResult.Success)
            {
                int progressTmp;
                int.TryParse(progressResult.Groups[1].Value, NumberStyles.Number, _appConfig.CInfo,
                               out progressTmp);
                progress = ((_muxStepCount - 1)*100*_singleStep) + (progressTmp*_singleStep);
            }
            else if (importResult.Success)
            {
                int progressTmp;
                int.TryParse(importResult.Groups[2].Value, NumberStyles.Number, _appConfig.CInfo,
                               out progressTmp);
                if (progressTmp < _lastImportPercent)
                    _muxStep++;
                _lastImportPercent = progressTmp;

                progress = (_muxStep * 100 * _singleStep) + (progressTmp * _singleStep);
            }
            else
                Log.Info($"mp4box: {line}");

            if (!progressResult.Success && !importResult.Success) return;

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
            InvokeEncodeStatusChanged(eventArgs);
        }

        #endregion
    }
}