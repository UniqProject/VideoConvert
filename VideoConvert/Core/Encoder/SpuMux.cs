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
using log4net;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace VideoConvert.Core.Encoder
{
    class SpuMux
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SpuMux));

        private EncodeInfo _jobInfo;
        private const string Executable = "spumux.exe";

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
                    Regex regObj = new Regex(@"^.*spumux, version ([\d\.]*)\..*$",
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
                Log.DebugFormat("spumux \"{0:s}\" found", verInfo);
            }

            return verInfo;
        }

        public void Process(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("spumux_muxing_status");
            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            SubtitleInfo sub = _jobInfo.SubtitleStreams[_jobInfo.StreamId];

            if (!sub.HardSubIntoVideo && File.Exists(sub.TempFile))
            {
                string inFile = _jobInfo.VideoStream.TempFile;
                string outFile = Processing.CreateTempFile(inFile, string.Format("+{0}.mpg", sub.LangCode));

                using (Process encoder = new Process())
                {
                    ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                                                     {
                                                         WorkingDirectory = AppSettings.DemuxLocation,
                                                         CreateNoWindow = true,
                                                         UseShellExecute = false,
                                                         RedirectStandardError = true,
                                                         RedirectStandardOutput = true,
                                                         RedirectStandardInput = true,
                                                         Arguments =
                                                             string.Format("-s{0:0} \"{1}\"", _jobInfo.StreamId,
                                                                           sub.TempFile)
                                                     };



                    encoder.StartInfo = parameter;

                    Log.InfoFormat("spumux {0:s}", parameter.Arguments);

                    FileStream readStream = new FileStream(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    FileStream writeStream = new FileStream(outFile, FileMode.Create, FileAccess.ReadWrite,
                                                            FileShare.Read);

                    encoder.ErrorDataReceived += (s, ea) => Log.InfoFormat("spumux: {0:s}", ea.Data);

                    bool started;
                    try
                    {
                        started = encoder.Start();
                    }
                    catch (Exception ex)
                    {
                        started = false;
                        Log.ErrorFormat("spumux exception: {0}", ex);
                        _jobInfo.ExitCode = -1;
                    }

                    DateTime update = DateTime.Now;

                    if (started)
                    {
                        encoder.PriorityClass = AppSettings.GetProcessPriority();
                        encoder.BeginErrorReadLine();
                        Processing.CopyStreamToStream(readStream, encoder.StandardInput.BaseStream, 0x2500,
                                                      (src, dst, exc) =>
                                                          {
                                                              src.Close();
                                                              dst.Close();

                                                              if (exc == null) return;

                                                              Log.Debug(exc.Message);
                                                              Log.Debug(exc.StackTrace);
                                                          });

                        byte[] bufferOut = new byte[0x10000];
                        int readOut;

                        while (!encoder.HasExited)
                        {
                            if (_bw.CancellationPending)
                            {
                                encoder.Kill();
                            }

                            if ((readOut = encoder.StandardOutput.BaseStream.Read(bufferOut, 0, bufferOut.Length)) > 0)
                                writeStream.Write(bufferOut, 0, readOut);

                            try
                            {
                                if (DateTime.Now.Subtract(update).Milliseconds > 500)
                                {
                                    int progress =
                                        (int) Math.Round(readStream.Position/(double) readStream.Length*100d, 0);

                                    _bw.ReportProgress(progress, status);
                                    update = DateTime.Now;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Debug(ex.Message);
                                Log.Debug(ex.StackTrace);
                            }
                        }

                        if (!encoder.StandardOutput.EndOfStream)
                        {
                            if ((readOut = encoder.StandardOutput.BaseStream.Read(bufferOut, 0, bufferOut.Length)) > 0)
                                writeStream.Write(bufferOut, 0, readOut);
                        }
                        encoder.CancelErrorRead();

                        _jobInfo.ExitCode = encoder.ExitCode;
                        Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                        if (_jobInfo.ExitCode == 0)
                        {
                            _jobInfo.VideoStream.TempFile = outFile;
                            _jobInfo.TempFiles.Add(inFile);
                            GetTempImages(sub.TempFile);
                            _jobInfo.TempFiles.Add(sub.TempFile);
                            _jobInfo.TempFiles.Add(Path.GetDirectoryName(sub.TempFile));
                        }
                    }
                    readStream.Dispose();
                    writeStream.Dispose();
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
            XmlNodeList spuList = inSubFile.SelectNodes("//spu");

            if (spuList != null)
                foreach (XmlNode spu in spuList.Cast<XmlNode>().Where(spu => spu.Attributes != null))
                {
                    Debug.Assert(spu.Attributes != null, "spu.Attributes != null");
                    _jobInfo.TempFiles.Add(spu.Attributes["image"].Value);
                }
        }
    }
}
