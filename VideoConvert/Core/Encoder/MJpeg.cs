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
using System.Text.RegularExpressions;
using log4net;
using System.Threading;

namespace VideoConvert.Core.Encoder
{
    class MJpeg
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MJpeg));

        private EncodeInfo _jobInfo;
        private BackgroundWorker _bw;

        private const string Executable = "mplex.exe";
        private const string Defaultparams = "-f 8 -r 0 -V -v 0";

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
                    Log.ErrorFormat("mjpegtools exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardError.ReadToEnd();
                    Regex regObj = new Regex(@"^.*mjpegtools.*?version.([\d\.]+).*\(.*$",
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
                Log.DebugFormat("mjpegtools \"{0:s}\" found", verInfo);

            return verInfo;
        }

        public void DoEncode(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("mjpeg_muxing_status");
            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation
                    };

                string input = _jobInfo.VideoStream.TempFile;

                string outFile = Path.ChangeExtension(_jobInfo.VideoStream.TempFile, "premuxed.mpg");

                parameter.Arguments = string.Format("{0} -o \"{1}\" \"{2}\"", Defaultparams, outFile, input);

                parameter.CreateNoWindow = true;
                parameter.UseShellExecute = false;

                parameter.RedirectStandardError = true;

                foreach (AudioInfo aud in _jobInfo.AudioStreams)
                    parameter.Arguments += " \"" + aud.TempFile + "\"";

                encoder.StartInfo = parameter;

                encoder.ErrorDataReceived += OnDataReceived;

                Log.InfoFormat("mplex {0:s}", parameter.Arguments);

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("mplex exception: {0}", ex);
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
                        _jobInfo.VideoStream.TempFile = outFile;

                        foreach (AudioInfo aud in _jobInfo.AudioStreams)
                            _jobInfo.TempFiles.Add(aud.TempFile);

                        _jobInfo.TempFiles.Add(input);
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
            if (!string.IsNullOrEmpty(line))
                Log.InfoFormat("mplex: {0:s}", line);
        }
    }
}
