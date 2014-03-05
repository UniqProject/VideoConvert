// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderBdSup2Sub.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The Encoder BdSup2Sub
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Encoder
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using Interop.Model.Subtitles;
    using Interop.Utilities;
    using Interop.Utilities.Subtitles;
    using log4net;
    using Services.Base;
    using Services.Interfaces;

    /// <summary>
    /// The Encoder BdSup2Sub
    /// </summary>
    public class EncoderBdSup2Sub : EncodeBase, IEncoderBdSup2Sub
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (EncoderBdSup2Sub));

        #region Private Variables

        private readonly IAppConfigService _appConfig;
        private const string Executable = "BDSup2Sub.jar";

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

        private SubtitleInfo _subtitle;

        private string _outputFile;
        private string _inputFile;

        private readonly Regex _readCaptions = new Regex(@"^#>\s+?(\d*?)\s\(.*$",
                                                         RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly Regex _decodeFrames = new Regex(@"^Decoding frame\s(\d*)/(\d*)\s*?.*$",
                                                         RegexOptions.Singleline | RegexOptions.Multiline);

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderBdSup2Sub"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public EncoderBdSup2Sub(IAppConfigService appConfig) : base(appConfig)
        {
            this._appConfig = appConfig;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the BDSup2Sub Process
        /// </summary>
        protected Process EncodeProcess { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads encoder version from its output, use path settings from parameters
        /// </summary>
        /// <param name="encPath">Path to encoder</param>
        /// <param name="javaPath">Path to java installation</param>
        /// <returns>Encoder version</returns>
        public static string GetVersionInfo(string encPath, string javaPath)
        {
            var verInfo = string.Empty;

            var localExecutable = Path.Combine(encPath, Executable);
            var query = string.Format("-jar \"{0}\" -V", localExecutable);

            using (var encoder = new Process())
            {
                var parameter = new ProcessStartInfo(javaPath, query)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = false,
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
                    Log.ErrorFormat("BDSup2Sub exception: {0}", ex);
                }

                if (started)
                {
                    var output = encoder.StandardOutput.ReadToEnd();
                    var regObj = new Regex(@"^BDSup2Sub\s*?([\d\.]*).?$",
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
                Log.DebugFormat("BDSup2Sub \"{0}\" found", verInfo);
            }
            return verInfo;
        }

        /// <summary>
        /// Execute a BDSup2Sub demux process.
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
                    throw new Exception("BDSup2Sub is already running");

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                var query = GenerateCommandLine();
                var cliPath = this._appConfig.JavaInstallPath;

                var cliStart = new ProcessStartInfo(cliPath, query)
                {
                    WorkingDirectory = this._appConfig.DemuxLocation,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                this.EncodeProcess = new Process {StartInfo = cliStart};
                Log.InfoFormat("start parameter: {0} {1}", cliPath, query);

                this.EncodeProcess.Start();

                this._startTime = DateTime.Now;

                this.EncodeProcess.OutputDataReceived += EncodeProcessDataReceived;
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
            var localExecutable = Path.Combine(this._appConfig.ToolsPath, Executable);
            sb.AppendFormat("-jar \"{0}\" ", localExecutable);

            this._subtitle = this._currentTask.SubtitleStreams[this._currentTask.StreamId];

            var targetRes = -1;

            if (this._currentTask.EncodingProfile.OutFormat == OutputType.OutputDvd)
                targetRes = this._currentTask.EncodingProfile.SystemType == 0 ? 576 : 480;

            _inputFile = _subtitle.TempFile;

            TextSubtitle textSub = null;
            switch (_subtitle.Format)
            {
                case "SSA":
                case "ASS":
                    textSub = SSAReader.ReadFile(_inputFile);
                    break;
                case "UTF-8":
                    textSub = SrtReader.ReadFile(_inputFile);
                    break;
            }

            var inFileDir = Path.GetDirectoryName(_inputFile);
            if (string.IsNullOrEmpty(inFileDir))
                inFileDir = string.Empty;

            var inFileName = Path.GetFileNameWithoutExtension(_inputFile);
            if (string.IsNullOrEmpty(inFileName))
                inFileName = string.Empty;

            var outPath = Path.Combine(inFileDir, inFileName);

            if (Directory.Exists(outPath))
                Directory.Delete(outPath, true);
            Directory.CreateDirectory(outPath, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));

            var inFileFullName = Path.GetFileName(_inputFile);
            if (string.IsNullOrEmpty(inFileFullName))
                inFileFullName = string.Empty;

            _outputFile = Path.Combine(outPath, inFileFullName);

            if (textSub != null)
            {
                var xmlFile = Path.ChangeExtension(_outputFile, "xml");
                if (BdnExport.WriteBdnXmlFile(textSub,
                                              xmlFile,
                                              this._currentTask.VideoStream.Width,
                                              this._currentTask.VideoStream.Height,
                                              this._currentTask.VideoStream.Fps))
                {
                    _subtitle.Format = "XML";
                    this._currentTask.TempFiles.Add(_inputFile);
                    _subtitle.TempFile = xmlFile;
                    _inputFile = xmlFile;
                }
            }

            if (this._currentTask.EncodingProfile.OutFormat == OutputType.OutputDvd)
                _outputFile = Path.ChangeExtension(_outputFile, "processed.xml");
            else if (_subtitle.KeepOnlyForcedCaptions)
                _outputFile = Path.ChangeExtension(_outputFile, "forced.sup");
            else if (_subtitle.Format == "XML" || _subtitle.Format == "VobSub")
                _outputFile = Path.ChangeExtension(_outputFile, "sup");

            var targetFps = this._currentTask.VideoStream.FrameMode.Trim().ToLowerInvariant() == "frame doubling"
                              ? this._currentTask.VideoStream.Fps * 2
                              : this._currentTask.VideoStream.Fps;
            var fpsMode = "keep";

            if (Math.Abs(targetFps - this._currentTask.VideoStream.Fps) > 0)
                fpsMode = targetFps.ToString("0.000", _appConfig.CInfo);

            sb.AppendFormat(_appConfig.CInfo, "\"{0}\" --output \"{1}\" --fps-target {2} --palette-mode keep ",
                            _inputFile, _outputFile, fpsMode);

            if (_subtitle.KeepOnlyForcedCaptions)
                sb.AppendFormat("--forced-only ");

            if (this._currentTask.EncodingProfile.OutFormat == OutputType.OutputDvd)
                sb.AppendFormat(_appConfig.CInfo, " --resolution {0:0} ", targetRes);

            return sb.ToString();
        }

        /// <summary>
        /// The BDSup2Sub process has exited.
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
                this._currentTask.TempFiles.Add(_inputFile);
                if (_subtitle.Format == "XML")
                {
                    GetTempImages(_inputFile);
                }
                if (_subtitle.Format == "VobSub")
                    this._currentTask.TempFiles.Add(Path.ChangeExtension(_inputFile, "sub"));

                if (this._currentTask.EncodingProfile.OutFormat == OutputType.OutputDvd)
                {
                    this._currentTask.TempFiles.Add(_outputFile);

                    _subtitle.TempFile = GenerateSpuMuxSubtitle(_outputFile);
                    _subtitle.Format = "SpuMux";
                }
                else
                {
                    _subtitle.TempFile = _outputFile;
                    _subtitle.Format = "PGS";
                }
                _subtitle.NeedConversion = false;
            }

            this._currentTask.CompletedStep = this._currentTask.NextStep;
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

            var remainingTime = new TimeSpan();
            var progress = 0f;
            var elapsedTime = new TimeSpan();

            double processingSpeed = 0f;
            var secRemaining = 0;

            var resultReadCaptions = _readCaptions.Match(line);
            var resultDecodeFrames = _decodeFrames.Match(line);
            if (resultReadCaptions.Success)
            {
                elapsedTime = DateTime.Now - this._startTime;
                progress = 0f;
                remainingTime = elapsedTime + TimeSpan.FromSeconds(1D);
            }
            else if (resultDecodeFrames.Success)
            {
                int actFrame, maxFrames;

                Int32.TryParse(resultDecodeFrames.Groups[1].Value, NumberStyles.Number, this._appConfig.CInfo,
                               out actFrame);
                Int32.TryParse(resultDecodeFrames.Groups[2].Value, NumberStyles.Number, this._appConfig.CInfo,
                               out maxFrames);

                var remaining = maxFrames - actFrame;

                progress = actFrame / (float)maxFrames * 100f;
                elapsedTime = DateTime.Now - this._startTime;

                if (elapsedTime.TotalSeconds > 0)
                    processingSpeed = actFrame / elapsedTime.TotalSeconds;

                if (processingSpeed > 0)
                    secRemaining = (int)Math.Round(remaining / processingSpeed, MidpointRounding.ToEven);

                remainingTime = TimeSpan.FromSeconds(secRemaining);
            }
            else
                Log.InfoFormat("BDSup2Sub: {0}", line);

            if (resultReadCaptions.Success || resultDecodeFrames.Success)
            {
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
        }

        private void GetTempImages(string inFile)
        {
            var inSubFile = new XmlDocument();
            inSubFile.Load(inFile);
            var spuList = inSubFile.SelectNodes("//Graphic");

            if (spuList != null)
                foreach (XmlNode spu in spuList)
                {
                    var fileName = spu.InnerText;

                    if (string.IsNullOrEmpty(Path.GetDirectoryName(fileName)))
                    {
                        var filePath = Path.GetDirectoryName(inFile);
                        if (string.IsNullOrEmpty(filePath))
                            filePath = string.Empty;
                        fileName = Path.Combine(filePath, fileName);
                    }
                    this._currentTask.TempFiles.Add(fileName);
                }
        }

        // TODO: this is kinda messy stuff
        private string GenerateSpuMuxSubtitle(string inFile)
        {
            var inSubFile = new XmlDocument();
            var outSubFile = new XmlDocument();
            var decl = outSubFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            var xn = outSubFile.CreateElement("subpictures");
            outSubFile.AppendChild(decl);
            outSubFile.AppendChild(xn);
            var stream = outSubFile.CreateElement("stream");

            try
            {
                inSubFile.Load(inFile);
            }
            catch (Exception)
            {
                return string.Empty;
            }
            var events = inSubFile.SelectNodes("//Event");

            if (events != null)
                foreach (XmlNode Event in events)
                {
                    if (Event.Attributes == null) continue;

                    var inTc =
                        DateTime.ParseExact(Event.Attributes["InTC"].Value, "hh:mm:ss:ff", this._appConfig.CInfo).TimeOfDay;
                    var outTc =
                        DateTime.ParseExact(Event.Attributes["OutTC"].Value, "hh:mm:ss:ff", this._appConfig.CInfo).TimeOfDay;
                    var forced = Boolean.Parse(Event.Attributes["Forced"].Value);

                    var graphic = Event.SelectSingleNode("Graphic");

                    if (graphic == null) continue;
                    if (graphic.Attributes == null) continue;

                    var xOffset = Int32.Parse(graphic.Attributes["X"].Value, this._appConfig.CInfo);
                    var yOffset = Int32.Parse(graphic.Attributes["Y"].Value, this._appConfig.CInfo);
                    var imageFile = graphic.InnerText;

                    var spu = outSubFile.CreateElement("spu");

                    var spuStart = outSubFile.CreateAttribute("start");
                    spuStart.Value = inTc.ToString("T", this._appConfig.CInfo);
                    spu.Attributes.Append(spuStart);

                    var spuEnd = outSubFile.CreateAttribute("end");
                    spuEnd.Value = outTc.ToString("T", this._appConfig.CInfo);
                    spu.Attributes.Append(spuEnd);

                    Color first;
                    var inPath = Path.GetDirectoryName(inFile);
                    if (string.IsNullOrEmpty(inPath))
                        inPath = string.Empty;

                    using (Bitmap bit = new Bitmap(Path.Combine(inPath, imageFile)),
                                  newBit = bit.Clone(new Rectangle(0, 0, bit.Width, bit.Height), PixelFormat.Format1bppIndexed))
                    {
                        var oldImage = Path.Combine(inPath, imageFile);
                        this._currentTask.TempFiles.Add(oldImage);
                        imageFile = Path.ChangeExtension(oldImage, "encoded.png");
                        first = newBit.Palette.Entries[0];

                        newBit.MakeTransparent(first);
                        newBit.Save(imageFile, ImageFormat.Png);
                    }

                    var spuImage = outSubFile.CreateAttribute("image");
                    spuImage.Value = imageFile;
                    spu.Attributes.Append(spuImage);

                    var spuTColor = outSubFile.CreateAttribute("transparent");
                    spuTColor.Value = string.Format("#{0:X2}{1:X2}{2:X2}", first.R, first.G, first.B);
                    spu.Attributes.Append(spuTColor);

                    var spuForce = outSubFile.CreateAttribute("force");
                    spuForce.Value = forced ? "yes" : "no";
                    spu.Attributes.Append(spuForce);

                    var spuXOffset = outSubFile.CreateAttribute("xoffset");
                    spuXOffset.Value = xOffset.ToString(this._appConfig.CInfo);
                    spu.Attributes.Append(spuXOffset);

                    var spuYOffset = outSubFile.CreateAttribute("yoffset");
                    spuYOffset.Value = yOffset.ToString(this._appConfig.CInfo);
                    spu.Attributes.Append(spuYOffset);

                    stream.AppendChild(spu);
                }
            xn.AppendChild(stream);

            var outFile = Path.ChangeExtension(inFile, "spumux.xml");

            outSubFile.Save(outFile);

            return outFile;
        }

        #endregion
    }
}