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
using System.Text.RegularExpressions;
using log4net;
using System.Threading;

namespace VideoConvert.Core.Encoder
{
    class MPlayer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MPlayer));

        private EncodeInfo _jobInfo;
        private const string Executable = "mplayer.exe";

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
                    Log.ErrorFormat("mplayer exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    Regex regObj = new Regex(@"^MPlayer ([\w\.].*) .*\(C\).*$",
                                             RegexOptions.Singleline | RegexOptions.Multiline);
                    Match result = regObj.Match(output);
                    if (result.Success)
                    {
                        verInfo = result.Groups[1].Value;
                    }
                }
                if (started)
                {
                    if (!encoder.HasExited)
                    {
                        encoder.Kill();
                    }
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("mplayer \"{0:s}\" found", verInfo);
            }

            return verInfo;
        }

        public void DoEncode(object sender, DoWorkEventArgs e)
        {

        }

        public void DoDump(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;
            string argument = string.Empty;

            string status = Processing.GetResourceString("mplayer_dump_status");
            string progressFormat = Processing.GetResourceString("mplayer_dump_progress");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            Regex regObj = new Regex(@"^dump: .*\(~([\d\.]+?)%\)$", RegexOptions.Singleline | RegexOptions.Multiline);


            using (Process encoder = new Process())
            {
                if (_jobInfo.Input == InputType.InputDvd)
                {
                    string dumpOutput =
                        Processing.CreateTempFile(
                            string.IsNullOrEmpty(_jobInfo.TempOutput) ? _jobInfo.OutputFile : _jobInfo.TempOutput,
                            "dump.mpg");

                    _jobInfo.DumpOutput = dumpOutput;

                    string chapterText = string.Empty;
                    if (_jobInfo.SelectedDvdChapters.Length > 0)
                    {
                        int posMinus = _jobInfo.SelectedDvdChapters.IndexOf('-');
                        chapterText = string.Format(posMinus == -1 ? "-chapter {0}-{0}" : "-chapter {0}",
                                                    _jobInfo.SelectedDvdChapters);
                    }

                    string inputFile = _jobInfo.InputFile;

                    if (string.IsNullOrEmpty(Path.GetDirectoryName(inputFile)))
                    {
                        int pos = inputFile.LastIndexOf(Path.DirectorySeparatorChar);
                        inputFile = inputFile.Remove(pos);
                    }

                    argument +=
                        string.Format("-dvd-device \"{0}\" dvdnav://{1:g} {3} -nocache -dumpstream -dumpfile \"{2}\"",
                                      inputFile, _jobInfo.TrackId, dumpOutput, chapterText);
                }

                string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                                                 {
                                                     Arguments = argument,
                                                     CreateNoWindow = true,
                                                     UseShellExecute = false,
                                                     RedirectStandardOutput = true
                                                 };

                encoder.StartInfo = parameter;

                encoder.OutputDataReceived += (outputSender, outputEvent) =>
                {
                    string line = outputEvent.Data;

                    if (string.IsNullOrEmpty(line)) return;

                    Match result = regObj.Match(line);
                    if (result.Success)
                    {
                        float tempProgress;
                        Single.TryParse(result.Groups[1].Value, NumberStyles.Number, AppSettings.CInfo, out tempProgress);
                        int progress = (int)Math.Round(tempProgress, 0);

                        string progressStr = string.Format(progressFormat, _jobInfo.TrackId, _jobInfo.InputFile,
                                                           progress);
                        _bw.ReportProgress(progress, progressStr);
                    }
                    else
                        Log.InfoFormat("mplayer: {0:s}", line);
                };

                Log.InfoFormat("mplayer {0:s}", argument);

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("mplayer exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

                if (started)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.BeginOutputReadLine();

                    _bw.ReportProgress(-1, status);

                    while (!encoder.HasExited)
                    {
                        if (_bw.CancellationPending)
                            encoder.Kill();
                        Thread.Sleep(200);
                    }
                    encoder.CancelOutputRead();

                    encoder.WaitForExit();
                    _jobInfo.ExitCode = encoder.ExitCode;
                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                }
            }
            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;

            e.Result = _jobInfo;

        }

        /*
         mencoder -ovc lavc -of rawvideo -mpegopts format=dvd:tsaf -vf scale=720:576,harddup 
         -lavcopts vcodec=mpeg2video:vrc_buf_size=1835:vrc_maxrate=9800:vbitrate=8000:keyint=15:trell:mbd=2:precmp=2:subcmp=2:cmp=2:dia=-10:predia=-10:cbp:mv0:vqmin=1:lmin=1:dc=10:vstrict=0:aspect=16/9 
         -o test.m2v C:\Users\Juergen\AppData\Local\Temp\tmp2032.avs
         * */
    }
}
