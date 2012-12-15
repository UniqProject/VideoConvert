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

namespace VideoConvert.Core.Encoder
{
    public class BdSup2SubTool
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BdSup2SubTool));

        private EncodeInfo _jobInfo;
        private const string Executable = "BDSup2Sub400.jar";

        private BackgroundWorker _bw;

        public void SetJob(EncodeInfo job)
        {
            _jobInfo = job;
        }

        public string GetVersionInfo()
        {
            return GetVersionInfo(AppSettings.ToolsPath, AppSettings.JavaInstallPath);
        }

        public string GetVersionInfo(string encPath, string javaPath)
        {
            string verInfo = string.Empty;

            string localExecutable = Path.Combine(encPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(javaPath)
                                                 {
                                                     Arguments = string.Format("-jar \"{0}\" /?", localExecutable),
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
                    Regex regObj = new Regex(@"^BDSup2Sub\s*?([\d\.]*?)\s-.*$",
                                             RegexOptions.Singleline | RegexOptions.Multiline);
                    Match result = regObj.Match(output);
                    if (result.Success)
                    {
                        verInfo = result.Groups[1].Value;
                    }
                    if (!encoder.HasExited)
                    {
                        encoder.Kill();
                    }
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("BDSup2Sub \"{0:s}\" found", verInfo);
            }

            return verInfo;
        }

        public void DoProcess(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("bdsup2sub_convert_subtitles_status");
            string subtitleLoadStatus = Processing.GetResourceString("bdsup2sub_convert_subtitle_load");
            string subtitleProcess = Processing.GetResourceString("bdsup2sub_convert_subtitle_process");
            string createSubtitle = Processing.GetResourceString("bdsup2sub_convert_subtitle_create");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string javaExecutable = AppSettings.JavaInstallPath;
            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            SubtitleInfo sub = _jobInfo.SubtitleStreams[_jobInfo.StreamId];

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("-jar \"{0}\" ", localExecutable);

            int targetRes = _jobInfo.EncodingProfile.SystemType == 0 ? 576 : 480;

            string inFile = sub.TempFile;

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

            if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                outFile = Path.ChangeExtension(outFile, "processed.xml");
            else if (sub.KeepOnlyForcedCaptions)
                outFile = Path.ChangeExtension(outFile, "forced.sup");

            sb.AppendFormat(AppSettings.CInfo, "\"{0:s}\" \"{1:s}\" /fps:keep /palmode:keep ", inFile, outFile);

            if (sub.KeepOnlyForcedCaptions)
                sb.AppendFormat("/forced+ ");

            if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                sb.AppendFormat(AppSettings.CInfo, " /res:{0:0} ", targetRes);

            Regex readCaptions = new Regex(@"^#>\s+?(\d*?)\s\(.*$", RegexOptions.Singleline | RegexOptions.Multiline);
            Regex decodeFrames = new Regex(@"^Decoding frame\s(\d*)/(\d*)\s*?.*$",
                                           RegexOptions.Singleline | RegexOptions.Multiline);

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

                encoder.OutputDataReceived += (outputSender, outputEvent) =>
                {
                    string line = outputEvent.Data;

                    if (string.IsNullOrEmpty(line)) return;

                    Match resultReadCaptions = readCaptions.Match(line);
                    Match resultDecodeFrames = decodeFrames.Match(line);

                    if (resultReadCaptions.Success)
                    {
                        if (!String.IsNullOrEmpty(subtitleLoadStatus))
                        {
                            status = string.Format(subtitleLoadStatus, resultReadCaptions.Groups[1].Value);
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

                        if (!String.IsNullOrEmpty(subtitleProcess))
                        {
                            status = string.Format(subtitleProcess, actFrame, maxFrames, progress);
                            _bw.ReportProgress(progress, status);
                        }                        
                    }
                    else
                    {
                        Log.InfoFormat("BDSup2Sub: {0:s}", line);
                    }
                };

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
                        {
                            encoder.Kill();
                        }
                        Thread.Sleep(200);
                    }
                    _jobInfo.ExitCode = encoder.ExitCode;
                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                }

                if (_jobInfo.ExitCode == 0)
                {
                    _jobInfo.TempFiles.Add(inFile);
                    if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                    {
                        _jobInfo.TempFiles.Add(outFile);
                        _bw.ReportProgress(-1, createSubtitle);

                        sub.TempFile = GenerateSpuMuxSubtitle(outFile);
                    }
                    else
                        sub.TempFile = outFile;
                }
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;

            e.Result = _jobInfo;
        }

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
// ReSharper disable AssignNullToNotNullAttribute
                    using (Bitmap bit = new Bitmap(Path.Combine(Path.GetDirectoryName(inFile), imageFile)),
// ReSharper restore AssignNullToNotNullAttribute
                                  newBit = bit.Clone(new Rectangle(0, 0, bit.Width, bit.Height), PixelFormat.Format1bppIndexed))
                    {
// ReSharper disable AssignNullToNotNullAttribute
                        string oldImage = Path.Combine(Path.GetDirectoryName(inFile), imageFile);
// ReSharper restore AssignNullToNotNullAttribute
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
