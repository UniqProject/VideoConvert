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
    class FfmsIndex
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FfmsIndex));

        private EncodeInfo _jobInfo;
        private const string Executable = "ffmsindex.exe";

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
                    Log.ErrorFormat("ffmsindex exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    Regex regObj = new Regex(@"^.*FFmpegSource2 indexing app.*$",
                                             RegexOptions.Singleline | RegexOptions.Multiline);
                    Match result = regObj.Match(output);
                    if (result.Success)
                        verInfo = "installed, no version info";

                    encoder.WaitForExit(10000);
                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
                Log.DebugFormat("ffmsindex \"{0:s}\" found", verInfo);

            return verInfo;
        }

        public void DoIndex(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("ffmsindex_indexing_status");
            string progressFmt = Processing.GetResourceString("ffmsindex_indexing_progress");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string localExecutable = Path.Combine(AppSettings.AppPath, "AvsPlugins", Executable);

            Regex regObj = new Regex(@"^.*Indexing, please wait\.\.\. ([\d]+)%.*$",
                                     RegexOptions.Singleline | RegexOptions.Multiline);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments =
                            string.Format("-f -t 0 \"{0}\"", _jobInfo.VideoStream.TempFile)
                    };
                encoder.StartInfo = parameter;

                encoder.OutputDataReceived += (outputSender, outputEvent) =>
                    {
                        string line = outputEvent.Data;
                        if (string.IsNullOrEmpty(line)) return;

                        Match result = regObj.Match(line);
                        if (result.Success)
                        {
                            string progress = string.Format(progressFmt,
                                                            result.Groups[1].Value);
                            _bw.ReportProgress(Convert.ToInt32(result.Groups[1].Value),
                                               progress);
                        }
                        else
                            Log.InfoFormat("ffmsindex: {0:s}", line);
                    };

                Log.InfoFormat("ffmsindex {0:s}", parameter.Arguments);

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("ffmsindex exception: {0}", ex);
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

                    encoder.WaitForExit(10000);
                    encoder.CancelOutputRead();

                    _jobInfo.ExitCode = encoder.ExitCode;
                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                }

                if (_jobInfo.ExitCode == 0)
                    _jobInfo.FfIndexFile = _jobInfo.VideoStream.TempFile + ".ffindex";
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;

            e.Result = _jobInfo;
        }
    }
}
