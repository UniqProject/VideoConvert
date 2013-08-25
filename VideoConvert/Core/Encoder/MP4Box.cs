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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using log4net;

namespace VideoConvert.Core.Encoder
{
    class MP4Box
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MP4Box));

        private EncodeInfo _jobInfo;
        private const string Executable = "mp4box.exe";

        private readonly string _progressFormat = Processing.GetResourceString("mp4box_muxing_progress");
        private readonly string _importFormat = Processing.GetResourceString("mp4box_import_format");
        private readonly Regex _importingReg = new Regex(@"^Importing ([\w-\d]*|ISO File): \|.+?\| \((\d+?)\/\d+?\)$",
                                                         RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly Regex _progressReg = new Regex(@"^ISO File Writing: \|.+?\| \((\d+?)\/\d+?\)$",
                                                        RegexOptions.Singleline | RegexOptions.Multiline);

        private BackgroundWorker _bw;

        public void SetJob(EncodeInfo job)
        {
            _jobInfo = job;
        }

        public string GetVersionInfo()
        {
            return GetVersionInfo(AppSettings.ToolsPath);
        }

        public string GetVersionInfo(string encPath)
        {
            string verInfo = string.Empty;

            string localExecutable = Path.Combine(encPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation,
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
                    Log.ErrorFormat("mp4box exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    if (string.IsNullOrEmpty(output))
                        output = encoder.StandardError.ReadToEnd();
                    Regex regObj = new Regex(@"^MP4Box.+?version ([\d\w\.\-\ \(\)]+)\s*$",
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
                Log.DebugFormat("mp4box \"{0:s}\" found", verInfo);

            return verInfo;
        }

        public void DoEncode(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            StringBuilder sb = new StringBuilder();

            float fps = _jobInfo.VideoStream.FPS;
            int vidStream;

            string status = Processing.GetResourceString("mp4box_muxing_status");
            string chapterName = Processing.GetResourceString("mp4box_chapter_format");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string tempExt = Path.GetExtension(_jobInfo.VideoStream.TempFile);
            if (_jobInfo.VideoStream.IsRawStream)
                vidStream = 0;
            else if ((_jobInfo.Input == InputType.InputAvi) && (!_jobInfo.VideoStream.Encoded))
                vidStream = 0;
            else if ((_jobInfo.VideoStream.Encoded) || (tempExt == ".mp4"))
                vidStream = 0;
            else
                vidStream = _jobInfo.VideoStream.StreamId;

            string outFile = !string.IsNullOrEmpty(_jobInfo.TempOutput) ? _jobInfo.TempOutput : _jobInfo.OutputFile;


            string fpsStr = string.Empty;
            if (_jobInfo.VideoStream.IsRawStream)
            {
                if (_jobInfo.VideoStream.FrameRateEnumerator == 0 || AppSettings.LastMp4BoxVer.StartsWith("0.5"))
                    fpsStr = string.Format(AppSettings.CInfo, ":fps={0:0.000}", fps);
                else
                    fpsStr = string.Format(":fps={0:g}/{1:g}",
                                           _jobInfo.VideoStream.FrameRateEnumerator,
                                           _jobInfo.VideoStream.FrameRateDenominator);
            }

            sb.AppendFormat(AppSettings.CInfo,
                            "-add \"{0}#video:trackID={1:g}{2}:lang=eng\" -keep-sys ",
                            _jobInfo.VideoStream.TempFile, vidStream,fpsStr);

            foreach (AudioInfo item in _jobInfo.AudioStreams)
            {
                string itemlang = item.LangCode;

                if ((itemlang == "xx") || (string.IsNullOrEmpty(itemlang)))
                    itemlang = "und";

                string delayString = string.Empty;

                if (item.Delay != 0)
                    delayString = string.Format(AppSettings.CInfo, ":delay={0:#}", item.Delay);

                sb.AppendFormat(AppSettings.CInfo,
                                "-add \"{0}#audio:lang={1:s}{2:s}\" -keep-sys ",
                                item.TempFile,
                                itemlang,
                                delayString);
            }

            foreach (SubtitleInfo item in _jobInfo.SubtitleStreams)
            {
                if (item.Format.ToLowerInvariant() != "utf-8") continue;
                if (!File.Exists(item.TempFile)) continue;

                string itemlang = item.LangCode;

                if ((itemlang == "xx") || (string.IsNullOrEmpty(itemlang)))
                    itemlang = "und";

                string delayString = string.Empty;

                if (item.Delay != 0)
                    delayString = string.Format(AppSettings.CInfo, ":delay={0:#}", item.Delay);

                sb.AppendFormat(AppSettings.CInfo,
                                "-add \"{0}#lang={1:s}{2:s}:name={3:s}\" -keep-sys ",
                                item.TempFile,
                                itemlang,
                                delayString,
                                Helpers.LanguageHelper.GetLanguage(itemlang).FullLang);
            }

            string chapterString = string.Empty;
            if (_jobInfo.Chapters.Count > 1)
            {
                string chapterFile =
                    Processing.CreateTempFile(
                        !string.IsNullOrEmpty(_jobInfo.TempOutput) ? _jobInfo.TempOutput : _jobInfo.OutputFile,
                        "chapters.ttxt");

                XmlDocument chapDoc = new XmlDocument();
                XmlDeclaration decl = chapDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                chapDoc.AppendChild(decl);

                XmlElement xn = chapDoc.CreateElement("TextStream");
                XmlAttribute att = chapDoc.CreateAttribute("version");
                att.Value = "1.1";
                xn.Attributes.Append(att);

                int temp;
                int subHeight = Math.DivRem(_jobInfo.VideoStream.Height, 3, out temp);
                subHeight += temp;

                XmlElement header = chapDoc.CreateElement("TextStreamHeader");
                att = chapDoc.CreateAttribute("width");
                att.Value = _jobInfo.VideoStream.Width.ToString("g");
                header.Attributes.Append(att);

                att = chapDoc.CreateAttribute("height");
                att.Value = subHeight.ToString("g");
                header.Attributes.Append(att);

                att = chapDoc.CreateAttribute("layer");
                att.Value = "0";
                header.Attributes.Append(att);

                att = chapDoc.CreateAttribute("translation_x");
                att.Value = "0";
                header.Attributes.Append(att);

                att = chapDoc.CreateAttribute("translation_y");
                att.Value = "0";
                header.Attributes.Append(att);


                XmlElement sampleDescription = chapDoc.CreateElement("TextSampleDescription");
                att = chapDoc.CreateAttribute("horizontalJustification");
                att.Value = "center";
                sampleDescription.Attributes.Append(att);

                att = chapDoc.CreateAttribute("verticalJustification");
                att.Value = "bottom";
                sampleDescription.Attributes.Append(att);

                att = chapDoc.CreateAttribute("backColor");
                att.Value = "0 0 0 0";
                sampleDescription.Attributes.Append(att);

                att = chapDoc.CreateAttribute("verticalText");
                att.Value = "no";
                sampleDescription.Attributes.Append(att);

                att = chapDoc.CreateAttribute("fillTextRegion");
                att.Value = "no";
                sampleDescription.Attributes.Append(att);

                att = chapDoc.CreateAttribute("continuousKaraoke");
                att.Value = "no";
                sampleDescription.Attributes.Append(att);

                att = chapDoc.CreateAttribute("scroll");
                att.Value = "None";
                sampleDescription.Attributes.Append(att);


                XmlElement fontTable = chapDoc.CreateElement("FontTable");
                XmlElement fontTableEntry = chapDoc.CreateElement("FontTableEntry");
                att = chapDoc.CreateAttribute("fontName");
                att.Value = "Arial";
                fontTableEntry.Attributes.Append(att);

                att = chapDoc.CreateAttribute("fontID");
                att.Value = "1";
                fontTableEntry.Attributes.Append(att);
                fontTable.AppendChild(fontTableEntry);


                XmlElement textBox = chapDoc.CreateElement("TextBox");
                att = chapDoc.CreateAttribute("top");
                att.Value = "0";
                textBox.Attributes.Append(att);

                att = chapDoc.CreateAttribute("left");
                att.Value = "0";
                textBox.Attributes.Append(att);

                att = chapDoc.CreateAttribute("bottom");
                att.Value = _jobInfo.VideoStream.Height.ToString("g");
                textBox.Attributes.Append(att);

                att = chapDoc.CreateAttribute("right");
                att.Value = _jobInfo.VideoStream.Width.ToString("g");
                textBox.Attributes.Append(att);


                XmlElement styleEntry = chapDoc.CreateElement("Style");
                att = chapDoc.CreateAttribute("styles");
                att.Value = "Normal";
                styleEntry.Attributes.Append(att);

                att = chapDoc.CreateAttribute("fontID");
                att.Value = "1";
                styleEntry.Attributes.Append(att);

                att = chapDoc.CreateAttribute("fontSize");
                att.Value = "32";
                styleEntry.Attributes.Append(att);

                att = chapDoc.CreateAttribute("color");
                att.Value = "ff ff ff ff";
                styleEntry.Attributes.Append(att);


                sampleDescription.AppendChild(fontTable);
                sampleDescription.AppendChild(textBox);
                sampleDescription.AppendChild(styleEntry);
                header.AppendChild(sampleDescription);
                xn.AppendChild(header);

                DateTime dt;

                if (_jobInfo.Input != InputType.InputDvd)
                {
                    for (int j = 0; j < _jobInfo.Chapters.Count; j++)
                    {
                        dt = DateTime.MinValue.Add(_jobInfo.Chapters[j]);
                        XmlElement timeEntry = CreateTimeEntry(chapDoc, dt, chapterName, j + 1);
                        xn.AppendChild(timeEntry);
                    }
                }
                else
                {
                    TimeSpan actualTime = new TimeSpan();
                    for (int j = 0; j < _jobInfo.Chapters.Count; j++)
                    {
                        actualTime = actualTime.Add(_jobInfo.Chapters[j]);
                        dt = DateTime.MinValue.Add(actualTime);
                        XmlElement timeEntry = CreateTimeEntry(chapDoc, dt, chapterName, j + 1);
                        xn.AppendChild(timeEntry);
                    }
                }

                chapDoc.AppendChild(xn);
                Log.Info("chapterFile: " + chapDoc.InnerXml);
                chapDoc.Save(chapterFile);
                
                chapterString = string.Format(" -add \"{0}:chap\"", chapterFile);
                _jobInfo.TempFiles.Add(chapterFile);
            }

            sb.AppendFormat(AppSettings.CInfo, "{0} ", chapterString);
            
            string tempPath = AppSettings.DemuxLocation;
            string tool = string.Format("{0} v{1}", AppSettings.GetProductName(),
                                        AppSettings.GetAppVersion().ToString(4));

            sb.AppendFormat(AppSettings.CInfo, "-itags tool=\"{0}\" -tmp \"{1}\" -v -new \"{2}\"", tool, tempPath, outFile);
            

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation,
                        Arguments = sb.ToString(),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                encoder.StartInfo = parameter;

                encoder.OutputDataReceived += OnDataReceived;
                encoder.ErrorDataReceived += OnDataReceived;

                Log.InfoFormat("mp4box {0:s}", parameter.Arguments);

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("mp4box exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

                if (started)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.BeginOutputReadLine();
                    encoder.BeginErrorReadLine();

                    while (!encoder.HasExited)
                    {
                        if (_bw.CancellationPending)
                            encoder.Kill();
                        Thread.Sleep(200);
                    }

                    encoder.WaitForExit(10000);
                    encoder.CancelOutputRead();
                    encoder.CancelErrorRead();

                    _jobInfo.ExitCode = encoder.ExitCode;
                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                    if (_jobInfo.ExitCode == 0)
                    {
                        _jobInfo.TempFiles.Add(_jobInfo.VideoStream.TempFile);
                        foreach (AudioInfo item in _jobInfo.AudioStreams)
                            _jobInfo.TempFiles.Add(item.TempFile);
                        foreach (SubtitleInfo item in _jobInfo.SubtitleStreams)
                            _jobInfo.TempFiles.Add(item.TempFile);
                    }
                }
            }




            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;

            e.Result = _jobInfo;
        }

        private void OnDataReceived(object outputSender, DataReceivedEventArgs outputEvent)
        {
            string line = outputEvent.Data;
            if (string.IsNullOrEmpty(line)) return;

            Match progressResult = _progressReg.Match(line);
            Match importResult = _importingReg.Match(line);
            if (progressResult.Success)
            {
                int progress;
                Int32.TryParse(progressResult.Groups[1].Value, NumberStyles.Number, AppSettings.CInfo,
                               out progress);
                string progressStatus = string.Format(_progressFormat,
                                                      Path.GetFileName(_jobInfo.OutputFile),
                                                      progress);
                _bw.ReportProgress(progress, progressStatus);
            }
            else if (importResult.Success)
            {
                int progress;
                Int32.TryParse(importResult.Groups[2].Value, NumberStyles.Number, AppSettings.CInfo,
                               out progress);
                string progressStatus = string.Format(_importFormat,
                                                      importResult.Groups[1].Value,
                                                      progress);
                _bw.ReportProgress(0, progressStatus);
            }
            else
                Log.InfoFormat("mp4box: {0:s}", line);
        }

        private static XmlElement CreateTimeEntry(XmlDocument xmlDocument, DateTime dateTime, string chapterName, int chapterNum)
        {
            XmlElement rElement = xmlDocument.CreateElement("TextSample");
            XmlAttribute att = xmlDocument.CreateAttribute("sampleTime");
            att.Value = dateTime.ToString("HH:mm:ss.fff");
            rElement.Attributes.Append(att);
            rElement.InnerText = string.Format(chapterName, chapterNum);
            return rElement;
        }
    }
}
