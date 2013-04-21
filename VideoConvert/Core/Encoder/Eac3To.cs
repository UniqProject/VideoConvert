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
using VideoConvert.Core.CommandLine;
using VideoConvert.Core.Helpers;
using log4net;
using System.Threading;

namespace VideoConvert.Core.Encoder
{
    class Eac3To
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(Eac3To));

        /// <summary>
        /// Executable filename
        /// </summary>
        private const string Executable = "eac3to.exe";

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
            return GetVersionInfo(AppSettings.ToolsPath);
        }

        /// <summary>
        /// Reads encoder version from its output, use path settings from parameters
        /// </summary>
        /// <param name="encPath">Path to encoder</param>
        /// <returns>Encoder version</returns>
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
                    Log.ErrorFormat("eac3to exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    Regex regObj = new Regex(@"^.*eac3to v([\d\.]+),.*$",
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
                Log.DebugFormat("eac3to v{0:s} found", verInfo);

            return verInfo;
        }

        /// <summary>
        /// Main processing function, called by BackgroundWorker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DoDemux(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("eac3to_demuxing_status");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation,
                        Arguments =
                            Eac3ToCommandLineGenerator.GenerateDemuxLine(ref _jobInfo),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };

                encoder.StartInfo = parameter;

                encoder.OutputDataReceived += EncoderOnOutputDataReceived;

                Log.InfoFormat("eac3to {0:s}", parameter.Arguments);

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("eac3to exception: {0}", ex);
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
                {
                    _jobInfo.VideoStream.IsRawStream = false;
                    GetStreamInfo();
                }
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;

            e.Result = _jobInfo;
        }

        private readonly Regex _processingRegex = new Regex(@"^.*process: ([\d]+)%.*$",
                                                            RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly Regex _analyzingRegex = new Regex(@"^.*analyze: ([\d]+)%.*$",
                                                          RegexOptions.Singleline | RegexOptions.Multiline);

        private readonly string _demuxFormat = Processing.GetResourceString("eac3to_demuxing_progress");
        private readonly string _analyzeFormat = Processing.GetResourceString("eac3to_analyze");
        
        /// <summary>
        /// Parses output from the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="outputEvent"></param>
        private void EncoderOnOutputDataReceived(object sender, DataReceivedEventArgs outputEvent)
        {
            string line = outputEvent.Data;
            if (string.IsNullOrEmpty(line)) return;

            string status = string.Empty;

            Match processingResult = _processingRegex.Match(line);
            Match analyzingResult = _analyzingRegex.Match(line);

            if (processingResult.Success)
            {
                int progress = Convert.ToInt32(processingResult.Groups[1].Value);

                if (!String.IsNullOrEmpty(_demuxFormat))
                    status = string.Format(_demuxFormat, Path.GetFileName(_jobInfo.InputFile), progress);

                _bw.ReportProgress(progress, status);
            }
            else if (analyzingResult.Success)
            {
                if (!String.IsNullOrEmpty(_analyzeFormat))
                    status = string.Format(_analyzeFormat, Path.GetFileName(_jobInfo.InputFile),
                                           analyzingResult.Groups[1].Value);

                _bw.ReportProgress(0, status);
            }
            else
                Log.InfoFormat("eac3to: {0:s}", line);
        }

        private void GetStreamInfo()
        {
            if (_jobInfo.Input == InputType.InputDvd)
                _jobInfo.VideoStream = VideoHelper.GetStreamInfo(_jobInfo.MediaInfo, _jobInfo.VideoStream, false);
            else
            {
                try
                {
                    _jobInfo.MediaInfo = Processing.GetMediaInfo(_jobInfo.VideoStream.TempFile);
                }
                catch (TimeoutException ex)
                {
                    Log.Error(ex);
                }
                finally
                {
                    if (_jobInfo.MediaInfo.Video.Count > 0)
                    {
                        _jobInfo.VideoStream.Bitrate = _jobInfo.MediaInfo.Video[0].BitRate;
                        _jobInfo.VideoStream.StreamSize = Processing.GetFileSize(_jobInfo.VideoStream.TempFile);
                        _jobInfo.VideoStream.FrameCount = _jobInfo.MediaInfo.Video[0].FrameCount;
                        _jobInfo.VideoStream.StreamId = _jobInfo.MediaInfo.Video[0].ID;
                    }
                }
                
                
            }

            for (int i = 0; i < _jobInfo.AudioStreams.Count; i++)
            {
                AudioInfo aStream = _jobInfo.AudioStreams[i];
                aStream = AudioHelper.GetStreamInfo(aStream);
                _jobInfo.AudioStreams[i] = aStream;
            }

            foreach (SubtitleInfo sStream in _jobInfo.SubtitleStreams)
                sStream.StreamSize = Processing.GetFileSize(sStream.TempFile);
        }
    }
}
