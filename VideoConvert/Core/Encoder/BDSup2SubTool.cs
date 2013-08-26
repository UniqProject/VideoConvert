//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using VideoConvert.Core.Helpers;
using log4net;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using VideoConvert.Core.Subtitles;

namespace VideoConvert.Core.Encoder
{
    public class BdSup2SubTool
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(BdSup2SubTool));

        /// <summary>
        /// Executable filename
        /// </summary>
        private const string Executable = "BDSup2Sub.jar";

        private EncodeInfo _jobInfo;
        private BackgroundWorker _bw;

        /// <summary>
        /// Sets job for processing
        /// </summary>
        /// <param name="job">Job to process</param>
        public void SetJob(EncodeInfo job)
        {
            _jobInfo = job;
        }

        /// <summary>
        /// Reads encoder version from its output, use standard path settings
        /// </summary>
        /// <returns>Encoder version</returns>
        public string GetVersionInfo()
        {
            return GetVersionInfo(AppSettings.ToolsPath, AppSettings.JavaInstallPath);
        }

        /// <summary>
        /// Reads encoder version from its output, use path settings from parameters
        /// </summary>
        /// <param name="encPath">Path to encoder</param>
        /// <param name="javaPath">Path to java installation</param>
        /// <returns>Encoder version</returns>
        public string GetVersionInfo(string encPath, string javaPath)
        {
            string verInfo = string.Empty;

            string localExecutable = Path.Combine(encPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(javaPath)
                    {
                        Arguments = string.Format("-jar \"{0}\" -V", localExecutable),
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
                    string output = encoder.StandardOutput.ReadToEnd();
                    Regex regObj = new Regex(@"^BDSup2Sub\s*?([\d\.]*).?$",
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
                Log.DebugFormat("BDSup2Sub \"{0:s}\" found", verInfo);

            return verInfo;
        }

        private readonly string _status = Processing.GetResourceString("bdsup2sub_convert_subtitles_status");

        /// <summary>
        /// Main processing function, called by BackgroundWorker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DoProcess(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string createSubtitle = Processing.GetResourceString("bdsup2sub_convert_subtitle_create");

            _bw.ReportProgress(-10, _status);
            _bw.ReportProgress(0, _status);

            string javaExecutable = AppSettings.JavaInstallPath;
            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            SubtitleInfo sub = _jobInfo.SubtitleStreams[_jobInfo.StreamId];

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("-jar \"{0}\" ", localExecutable);

            int targetRes = -1;

            if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                targetRes = _jobInfo.EncodingProfile.SystemType == 0 ? 576 : 480;

            string inFile = sub.TempFile;

            TextSubtitle textSub = null;
            switch (sub.Format)
            {
                case "SSA":
                case "ASS":
                    textSub = SSAReader.ReadFile(inFile);
                    break;
                case "UTF-8":
                    textSub = SRTReader.ReadFile(inFile);
                    break;
            }

            string inFileDir = Path.GetDirectoryName(inFile);
            if (string.IsNullOrEmpty(inFileDir))
                inFileDir = string.Empty;

            string inFileName = Path.GetFileNameWithoutExtension(inFile);
            if (string.IsNullOrEmpty(inFileName))
                inFileName = string.Empty;

            string outPath = Path.Combine(inFileDir, inFileName);

            if (Directory.Exists(outPath))
                Directory.Delete(outPath, true);
            Directory.CreateDirectory(outPath, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));

            string inFileFullName = Path.GetFileName(inFile);
            if (string.IsNullOrEmpty(inFileFullName))
                inFileFullName = string.Empty;

            string outFile = Path.Combine(outPath, inFileFullName);

            if (textSub != null)
            {
                string xmlFile = Path.ChangeExtension(outFile, "xml");
                if (BDNExport.WriteBDNXmlFile(textSub, xmlFile, _jobInfo.VideoStream.Width, _jobInfo.VideoStream.Height,
                    _jobInfo.VideoStream.FPS))
                {
                    sub.Format = "XML";
                    _jobInfo.TempFiles.Add(inFile);
                    sub.TempFile = xmlFile;
                    inFile = xmlFile;
                }
            }

            if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                outFile = Path.ChangeExtension(outFile, "processed.xml");
            else if (sub.KeepOnlyForcedCaptions)
                outFile = Path.ChangeExtension(outFile, "forced.sup");
            else if (sub.Format == "XML" || sub.Format == "VobSub")
                outFile = Path.ChangeExtension(outFile, "sup");

            float targetFPS = _jobInfo.VideoStream.FrameMode.Trim().ToLowerInvariant() == "frame doubling"
                        ? _jobInfo.VideoStream.FPS * 2
                        : _jobInfo.VideoStream.FPS;
            string fpsMode = "keep";

            if (Math.Abs(targetFPS - _jobInfo.VideoStream.FPS) > 0)
                fpsMode = targetFPS.ToString("0.000", AppSettings.CInfo);

            sb.AppendFormat(AppSettings.CInfo, "\"{0:s}\" --output \"{1:s}\" --fps-target {2} --palette-mode keep ", inFile, outFile, fpsMode);

            if (sub.KeepOnlyForcedCaptions)
                sb.AppendFormat("--forced-only ");

            if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                sb.AppendFormat(AppSettings.CInfo, " --resolution {0:0} ", targetRes);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(javaExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = sb.ToString()
                    };

                encoder.StartInfo = parameter;
                encoder.OutputDataReceived += EncoderOnOutputDataReceived; 

                Log.InfoFormat("BDSup2Sub: {0:s}", parameter.Arguments);

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("BDSup2Sub exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

                if (started)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.BeginOutputReadLine();

                    while (!encoder.HasExited)
                    {
                        if (_bw.CancellationPending)
                            encoder.Kill();

                        Thread.Sleep(200);
                    }
                    _jobInfo.ExitCode = encoder.ExitCode;
                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                }

                if (_jobInfo.ExitCode == 0)
                {
                    _jobInfo.TempFiles.Add(inFile);
                    if (sub.Format == "XML")
                    {
                        GetTempImages(inFile);
                    }
                    if (sub.Format == "VobSub")
                        _jobInfo.TempFiles.Add(Path.ChangeExtension(inFile, "sub"));

                    if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                    {
                        _jobInfo.TempFiles.Add(outFile);
                        _bw.ReportProgress(-1, createSubtitle);

                        sub.TempFile = GenerateSpuMuxSubtitle(outFile);
                        sub.Format = "SpuMux";
                    }
                    else
                    {
                        sub.TempFile = outFile;
                        sub.Format = "PGS";
                    }
                    sub.NeedConversion = false;
                }
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;

            e.Result = _jobInfo;
        }

        private void GetTempImages(string inFile)
        {
            XmlDocument inSubFile = new XmlDocument();
            inSubFile.Load(inFile);
            XmlNodeList spuList = inSubFile.SelectNodes("//Graphic");

            if (spuList != null)
                foreach (XmlNode spu in spuList)
                {
                    string fileName = spu.InnerText;

                    if (string.IsNullOrEmpty(Path.GetDirectoryName(fileName)))
                    {
                        string filePath = Path.GetDirectoryName(inFile);
                        if (string.IsNullOrEmpty(filePath))
                            filePath = string.Empty;
                        fileName = Path.Combine(filePath, fileName);
                    }
                    _jobInfo.TempFiles.Add(fileName);
                }
        }

        private readonly Regex _readCaptions = new Regex(@"^#>\s+?(\d*?)\s\(.*$",
                                                         RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly Regex _decodeFrames = new Regex(@"^Decoding frame\s(\d*)/(\d*)\s*?.*$",
                                                         RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly string _subtitleLoadStatus = Processing.GetResourceString("bdsup2sub_convert_subtitle_load");
        private readonly string _subtitleProcess = Processing.GetResourceString("bdsup2sub_convert_subtitle_process");

        /// <summary>
        /// Parses output from the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="outputEvent"></param>
        private void EncoderOnOutputDataReceived(object sender, DataReceivedEventArgs outputEvent)
        {
            string line = outputEvent.Data;
            if (string.IsNullOrEmpty(line)) return;

            Match resultReadCaptions = _readCaptions.Match(line);
            Match resultDecodeFrames = _decodeFrames.Match(line);

            string status;

            if (resultReadCaptions.Success)
            {
                if (!String.IsNullOrEmpty(_subtitleLoadStatus))
                {
                    status = string.Format(_subtitleLoadStatus, resultReadCaptions.Groups[1].Value);
                    _bw.ReportProgress(-1, status);
                }
            }
            else if (resultDecodeFrames.Success)
            {
                int actFrame, maxFrames;

                Int32.TryParse(resultDecodeFrames.Groups[1].Value, NumberStyles.Number, AppSettings.CInfo,
                               out actFrame);
                Int32.TryParse(resultDecodeFrames.Groups[2].Value, NumberStyles.Number, AppSettings.CInfo,
                               out maxFrames);

                int progress = (int)Math.Round(actFrame / (double)maxFrames * 100d, 0);

                if (!String.IsNullOrEmpty(_subtitleProcess))
                {
                    status = string.Format(_subtitleProcess, actFrame, maxFrames, progress);
                    _bw.ReportProgress(progress, status);
                }
            }
            else
                Log.InfoFormat("BDSup2Sub: {0:s}", line);
        }

        /// <summary>
        /// Processes xml file generated by the application and replace with image captions valid for dvd subtitles.
        /// </summary>
        /// <param name="inFile">Path to input xml file</param>
        /// <returns>Path to output xml file</returns>
        private string GenerateSpuMuxSubtitle(string inFile)
        {
            XmlDocument inSubFile = new XmlDocument();
            XmlDocument outSubFile = new XmlDocument();
            XmlDeclaration decl = outSubFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            XmlElement xn = outSubFile.CreateElement("subpictures");
            outSubFile.AppendChild(decl);
            outSubFile.AppendChild(xn);
            XmlElement stream = outSubFile.CreateElement("stream");

            try
            {
                inSubFile.Load(inFile);
            }
            catch (Exception)
            {
                return string.Empty;
            }
            XmlNodeList events = inSubFile.SelectNodes("//Event");

            if (events != null)
                foreach (XmlNode Event in events)
                {
                    if (Event.Attributes == null) continue;

                    TimeSpan inTc =
                        DateTime.ParseExact(Event.Attributes["InTC"].Value, "hh:mm:ss:ff", AppSettings.CInfo).TimeOfDay;
                    TimeSpan outTc =
                        DateTime.ParseExact(Event.Attributes["OutTC"].Value, "hh:mm:ss:ff", AppSettings.CInfo).TimeOfDay;
                    bool forced = Boolean.Parse(Event.Attributes["Forced"].Value);

                    XmlNode graphic = Event.SelectSingleNode("Graphic");

                    if (graphic == null) continue;
                    if (graphic.Attributes == null) continue;

                    int xOffset = Int32.Parse(graphic.Attributes["X"].Value, AppSettings.CInfo);
                    int yOffset = Int32.Parse(graphic.Attributes["Y"].Value, AppSettings.CInfo);
                    string imageFile = graphic.InnerText;

                    XmlElement spu = outSubFile.CreateElement("spu");

                    XmlAttribute spuStart = outSubFile.CreateAttribute("start");
                    spuStart.Value = inTc.ToString("T", AppSettings.CInfo);
                    spu.Attributes.Append(spuStart);

                    XmlAttribute spuEnd = outSubFile.CreateAttribute("end");
                    spuEnd.Value = outTc.ToString("T", AppSettings.CInfo);
                    spu.Attributes.Append(spuEnd);

                    Color first;
                    string inPath = Path.GetDirectoryName(inFile);
                    if (string.IsNullOrEmpty(inPath))
                        inPath = string.Empty;

                    using (Bitmap bit = new Bitmap(Path.Combine(inPath, imageFile)),
                                  newBit = bit.Clone(new Rectangle(0, 0, bit.Width, bit.Height), PixelFormat.Format1bppIndexed))
                    {
                        string oldImage = Path.Combine(inPath, imageFile);
                        _jobInfo.TempFiles.Add(oldImage);
                        imageFile = Path.ChangeExtension(oldImage, "encoded.png");
                        first = newBit.Palette.Entries[0];

                        newBit.MakeTransparent(first);
                        newBit.Save(imageFile, ImageFormat.Png);
                    }

                    XmlAttribute spuImage = outSubFile.CreateAttribute("image");
                    spuImage.Value = imageFile;
                    spu.Attributes.Append(spuImage);

                    XmlAttribute spuTColor = outSubFile.CreateAttribute("transparent");
                    spuTColor.Value = string.Format("#{0:X2}{1:X2}{2:X2}", first.R, first.G, first.B);
                    spu.Attributes.Append(spuTColor);

                    XmlAttribute spuForce = outSubFile.CreateAttribute("force");
                    spuForce.Value = forced ? "yes":"no";
                    spu.Attributes.Append(spuForce);

                    XmlAttribute spuXOffset = outSubFile.CreateAttribute("xoffset");
                    spuXOffset.Value = xOffset.ToString(AppSettings.CInfo);
                    spu.Attributes.Append(spuXOffset);

                    XmlAttribute spuYOffset = outSubFile.CreateAttribute("yoffset");
                    spuYOffset.Value = yOffset.ToString(AppSettings.CInfo);
                    spu.Attributes.Append(spuYOffset);

                    stream.AppendChild(spu);
                }
            xn.AppendChild(stream);

            string outFile = Path.ChangeExtension(inFile, "spumux.xml");

            outSubFile.Save(outFile);

            return outFile;
        }

    }
}
