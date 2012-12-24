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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using VideoConvert.Core.Helpers;
using log4net;
using System.Xml;
using System.Collections.Generic;
using System.Threading;

namespace VideoConvert.Core.Encoder
{
    class DvdAuthor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DvdAuthor));

        private EncodeInfo _jobInfo;
        private const string Executable = "dvdauthor.exe";

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
                    Regex regObj = new Regex(@"^.*?DVDAuthor::dvdauthor, version ([\d\.]+)\.$",
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
                Log.DebugFormat("DVDAuthor \"{0:s}\" found", verInfo);

            return verInfo;
        }

        public void DoEncode(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("dvdauthor_muxing_status");
            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            XmlDocument outSubFile = new XmlDocument();
            XmlDeclaration decl = outSubFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            XmlElement xn = outSubFile.CreateElement("dvdauthor");
            outSubFile.AppendChild(decl);
            outSubFile.AppendChild(xn);
            xn.AppendChild(outSubFile.CreateElement("vmgm"));

            XmlNode titleSet = outSubFile.CreateElement("titleset");
            xn.AppendChild(titleSet);

            XmlNode titles = outSubFile.CreateElement("titles");
            titleSet.AppendChild(titles);

            string chapterString = string.Empty;
            if (_jobInfo.Chapters.Count > 1)
            {
                DateTime dt;
                List<string> tempChapters = new List<string>();

                if (_jobInfo.Input != InputType.InputDvd)
                {
                    foreach (TimeSpan chapter in _jobInfo.Chapters)
                    {
                        dt = DateTime.MinValue.Add(chapter);
                        tempChapters.Add(dt.ToString("H:mm:ss.fff"));
                    }
                }
                else
                {
                    TimeSpan actualTime = new TimeSpan();

                    foreach (TimeSpan chapter in _jobInfo.Chapters)
                    {
                        actualTime = actualTime.Add(chapter);
                        dt = DateTime.MinValue.Add(actualTime);
                        tempChapters.Add(dt.ToString("H:mm:ss.fff"));
                    }
                }
                chapterString = string.Join(",", tempChapters.ToArray());
            }

            foreach (string itemlang in _jobInfo.AudioStreams.Select(item => item.ShortLang))
            {
                LanguageHelper language = LanguageHelper.GetLanguage(string.IsNullOrEmpty(itemlang) ? "xx" : itemlang);
                
                XmlNode audio = outSubFile.CreateElement("audio");
                XmlAttribute audioLang = outSubFile.CreateAttribute("lang");
                audioLang.Value = language.Iso1Lang;

                if (audio.Attributes != null) 
                    audio.Attributes.Append(audioLang);

                titles.AppendChild(audio);
            }

            foreach (SubtitleInfo item in _jobInfo.SubtitleStreams)
            {
                string itemlang = item.LangCode;

                if (string.IsNullOrEmpty(itemlang))
                    itemlang = "xx";

                LanguageHelper language = LanguageHelper.GetLanguage(itemlang);

                if (item.Format != "PGS" && item.Format != "VobSub") continue;

                XmlNode sub = outSubFile.CreateElement("subpicture");
                XmlAttribute subLang = outSubFile.CreateAttribute("lang");
                subLang.Value = language.Iso1Lang;

                if (sub.Attributes != null) 
                    sub.Attributes.Append(subLang);

                titles.AppendChild(sub);
            }

            XmlNode pgc = outSubFile.CreateElement("pgc");
            titles.AppendChild(pgc);

            XmlNode vob = outSubFile.CreateElement("vob");
            XmlAttribute vobFile = outSubFile.CreateAttribute("file");
            vobFile.Value = HttpUtility.HtmlEncode(_jobInfo.VideoStream.TempFile);
            if (vob.Attributes != null)
            {
                vob.Attributes.Append(vobFile);

                if (!string.IsNullOrEmpty(chapterString))
                {
                    XmlAttribute chapters = outSubFile.CreateAttribute("chapters");
                    chapters.Value = chapterString;
                    vob.Attributes.Append(chapters);
                }
            }

            pgc.AppendChild(vob);

            string xmlFile = Processing.CreateTempFile(".xml");

            outSubFile.Save(xmlFile);

            Log.InfoFormat("dvdauthor xml: \r\n{0:s}", outSubFile.OuterXml);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true
                    };


                string outFile = !string.IsNullOrEmpty(_jobInfo.TempOutput) ? _jobInfo.TempOutput : _jobInfo.OutputFile;

                parameter.Arguments = string.Format("-o \"{0}\" -x \"{1}\"", outFile, xmlFile);
                encoder.StartInfo = parameter;

                encoder.ErrorDataReceived += (outputSender, outputEvent) =>
                {
                    string line = outputEvent.Data;
                    if (!string.IsNullOrEmpty(line))
                        Log.InfoFormat("dvdauthor: {0:s}", line);
                };

                Log.InfoFormat("dvdauthor: {0:s}", parameter.Arguments);

                try
                {
                    Directory.Delete(outFile, true);
                }
                catch (Exception exception)
                {
                    Log.ErrorFormat("DVDAuthor exception: {0}", exception.Message);
                }

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("dvdauthor exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

                if (started)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.BeginErrorReadLine();

                    _bw.ReportProgress(-1, status);

                    while (!encoder.HasExited)
                    {
                        if (_bw.CancellationPending)
                            encoder.Kill();

                        Thread.Sleep(200);
                    }

                    encoder.WaitForExit(10000);
                    encoder.CancelErrorRead();

                    _jobInfo.ExitCode = encoder.ExitCode;
                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);

                    if (_jobInfo.ExitCode == 0)
                    {
                        _jobInfo.TempFiles.Add(xmlFile);
                        _jobInfo.TempFiles.Add(_jobInfo.VideoStream.TempFile);
                    }
                }
            }
            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;

            e.Result = _jobInfo;
        }
    }
}
