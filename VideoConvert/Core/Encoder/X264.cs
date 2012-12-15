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
using VideoConvert.Core.CommandLine;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;
using log4net;
using System.Globalization;
using System.Drawing;
using System.Threading;

namespace VideoConvert.Core.Encoder
{
    class X264
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(X264));

        private EncodeInfo _jobInfo;
        private const string Executable = "x264.exe";

        long _frameCount;
        private BackgroundWorker _bw;

        public struct Features
        {
            public bool HasGpac;
            public bool HasLavf;
            public bool HasAvs;
        }

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
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable, "--version")
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
                    Log.ErrorFormat("x264 encoder exception: {0}", ex);
                }


                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    Regex regObj = new Regex(@"^x264.+?(\d)\.(\d+)\.([\dM]+)",
                                             RegexOptions.Singleline | RegexOptions.Multiline);
                    Match result = regObj.Match(output);
                    if (result.Success)
                    {
                        verInfo = string.Format("Core: {0} Build {1}", result.Groups[2].Value, result.Groups[3].Value);
                    }

                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("x264 \"{0:s}\" found", verInfo);
            }

            return verInfo;
        }

        public Features GetFeatures()
        {
            Features verInfo = new Features {HasAvs = false, HasGpac = false, HasLavf = false};

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable, "--help")
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
                    Log.ErrorFormat("x264 encoder exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    Regex regObj = new Regex(@"^.*lavf support \((\w.+)\) or.*\((\w.+)\).*$", RegexOptions.Singleline);
                    Match result = regObj.Match(output);
                    if (result.Success)
                    {
                        verInfo.HasLavf = result.Groups[1].Value == "yes";
                    }

                    Regex regObj1 = new Regex(@"^.*or Avisynth if compiled with support \((\w+)\).*$",
                                              RegexOptions.Singleline);
                    Match result1 = regObj1.Match(output);
                    if (result1.Success)
                    {
                        verInfo.HasAvs = result1.Groups[1].Value == "yes";
                    }

                    Regex regObj2 = new Regex(@"^.*MP4 if compiled with GPAC support \((\w+)\).*$",
                                              RegexOptions.Singleline);
                    Match result2 = regObj2.Match(output);
                    if (result.Success)
                    {
                        verInfo.HasGpac = result2.Groups[1].Value == "yes";
                    }

                    if (!encoder.HasExited)
                        encoder.Kill();
                }

            }

            return verInfo;
        }

        public void DoEncode(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string passStr = Processing.GetResourceString("x264_pass");
            string status = Processing.GetResourceString("x264_encoding_status");
            string progressFormat = Processing.GetResourceString("x264_encoding_progress");

            //progress vars
            DateTime startTime = DateTime.Now;
            TimeSpan remaining = new TimeSpan(0, 0, 0);
            // end progress

            X264Profile encProfile = (X264Profile)_jobInfo.VideoProfile;

            if (!_jobInfo.EncodingProfile.Deinterlace && _jobInfo.VideoStream.Interlaced)
                _jobInfo.VideoStream.Interlaced = false;

            Size resizeTo = VideoHelper.GetTargetSize(_jobInfo);

            if (string.IsNullOrEmpty(_jobInfo.AviSynthScript))
                GenerateAviSynthScript(resizeTo);

            string inputFile = _jobInfo.AviSynthScript;
            string outFile =
                Processing.CreateTempFile(
                    string.IsNullOrEmpty(_jobInfo.TempOutput) ? _jobInfo.JobName : _jobInfo.TempOutput, "encoded.264");

            int targetBitrate = 0;
            if (_jobInfo.EncodingProfile.TargetFileSize > 0)
            {
                targetBitrate = Processing.CalculateVideoBitrate(_jobInfo);
            }

            int encodeMode = encProfile.EncodingMode;
            string pass = string.Empty;
            if ((encodeMode == 2) || (encodeMode == 3))
            {
                pass = string.Format(" {1} {0:0}; ", _jobInfo.StreamId, passStr);
            }

            _bw.ReportProgress(-10, status + pass.Replace("; ", string.Empty));
            _bw.ReportProgress(0, status);

            string argument = X264CommandLineGenerator.Generate(encProfile,
                                                                targetBitrate,
                                                                resizeTo.Width,
                                                                resizeTo.Height,
                                                                _jobInfo.StreamId,
                                                                _jobInfo.VideoStream.FrameRateEnumerator,
                                                                _jobInfo.VideoStream.FrameRateDenominator,
                                                                _jobInfo.EncodingProfile.StereoType,
                                                                false,
                                                                _jobInfo.VideoStream.PicSize,
                                                                inputFile,
                                                                outFile);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            Regex frameInformation = new Regex(@"^\D?([\d]+).*frames: ([\d\.]+) fps, ([\d\.]+).*$",
                                               RegexOptions.Singleline | RegexOptions.Multiline);
            Regex fullFrameInformation =
                new Regex(@"^\[[\d\.]+?%\] ([\d]+?)/([\d]+?) frames, ([\d\.]+?) fps, ([\d\.]+?) kb/s.*$",
                          RegexOptions.Singleline | RegexOptions.Multiline);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                                                 {
                                                     WorkingDirectory = AppSettings.DemuxLocation,
                                                     Arguments = argument,
                                                     CreateNoWindow = true,
                                                     UseShellExecute = false,
                                                     RedirectStandardError = true
                                                 };
                encoder.StartInfo = parameter;

                encoder.ErrorDataReceived += (outputSender, outputEvent) =>
                                                 {
                                                     string line = outputEvent.Data;

                                                     if (string.IsNullOrEmpty(line)) return;

                                                     Match result = frameInformation.Match(line);
                                                     Match result2 = fullFrameInformation.Match(line);

                                                     // ReSharper disable AccessToModifiedClosure
                                                     TimeSpan eta = DateTime.Now.Subtract(startTime);
                                                     // ReSharper restore AccessToModifiedClosure
                                                     long current;
                                                     long framesRemaining;
                                                     long secRemaining = 0;

                                                     float encBitrate;
                                                     float fps;
                                                     DateTime ticks;
                                                     double codingFPS;
                                                     if (result.Success)
                                                     {
                                                         Int64.TryParse(result.Groups[1].Value, NumberStyles.Number,
                                                                        AppSettings.CInfo, out current);
                                                         framesRemaining = _frameCount - current;

                                                         if (eta.Seconds != 0)
                                                         {
                                                             //Frames per Second
                                                             codingFPS = Math.Round(current/eta.TotalSeconds, 2);

                                                             if (codingFPS > 1)
                                                                 secRemaining = framesRemaining/(int) codingFPS;
                                                             else
                                                                 secRemaining = 0;
                                                         }

                                                         if (secRemaining > 0)
                                                             remaining = new TimeSpan(0, 0, (int) secRemaining);

                                                         ticks = new DateTime(eta.Ticks);

                                                         Single.TryParse(result.Groups[2].Value, NumberStyles.Number,
                                                                         AppSettings.CInfo, out fps);
                                                         Single.TryParse(result.Groups[3].Value, NumberStyles.Number,
                                                                         AppSettings.CInfo, out encBitrate);

                                                         string progress = string.Format(progressFormat,
                                                                                         current, _frameCount,
                                                                                         fps,
                                                                                         encBitrate,
                                                                                         remaining, ticks, pass);
                                                         _bw.ReportProgress((int) (((float) current/_frameCount)*100),
                                                                            progress);

                                                     }
                                                     else if (result2.Success)
                                                     {
                                                         Int64.TryParse(result2.Groups[1].Value, NumberStyles.Number,
                                                                        AppSettings.CInfo, out current);
                                                         Int64.TryParse(result2.Groups[2].Value, NumberStyles.Number,
                                                                        AppSettings.CInfo, out _frameCount);

                                                         framesRemaining = _frameCount - current;

                                                         if (eta.Seconds != 0)
                                                         {
                                                             //Frames per Second
                                                             codingFPS = Math.Round(current/eta.TotalSeconds, 2);

                                                             if (codingFPS > 1)
                                                                 secRemaining = framesRemaining/(int) codingFPS;
                                                             else
                                                                 secRemaining = 0;
                                                         }

                                                         if (secRemaining > 0)
                                                             remaining = new TimeSpan(0, 0, (int) secRemaining);

                                                         ticks = new DateTime(eta.Ticks);

                                                         Single.TryParse(result2.Groups[3].Value, NumberStyles.Number,
                                                                         AppSettings.CInfo, out fps);
                                                         Single.TryParse(result2.Groups[4].Value, NumberStyles.Number,
                                                                         AppSettings.CInfo, out encBitrate);

                                                         string progress = string.Format(progressFormat,
                                                                                         current, _frameCount,
                                                                                         fps,
                                                                                         encBitrate,
                                                                                         remaining, ticks, pass);
                                                         _bw.ReportProgress((int) (((float) current/_frameCount)*100),
                                                                            progress);
                                                     }
                                                     else
                                                     {
                                                         Log.InfoFormat("x264: {0:s}", line);
                                                     }
                                                 };

                Log.InfoFormat("start parameter: x264 {0:s}", argument);

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("x264 encoder exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

                startTime = DateTime.Now;

                if (started)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.BeginErrorReadLine();
                    
                    while (!encoder.HasExited)
                    {
                        if (_bw.CancellationPending)
                            encoder.Kill();
                        Thread.Sleep(200);
                    }
                    encoder.CancelErrorRead();

                    _jobInfo.ExitCode = encoder.ExitCode;
                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                }
            }

            if (_jobInfo.ExitCode == 0)
            {
                if ((encProfile.EncodingMode == 2 && _jobInfo.StreamId == 2) ||
                    (encProfile.EncodingMode == 3 && _jobInfo.StreamId == 3) ||
                    (encProfile.EncodingMode < 2 || _jobInfo.StreamId > 3))
                {
                    _jobInfo.MediaInfo = Processing.GetMediaInfo(_jobInfo.VideoStream.TempFile);
                    _jobInfo.VideoStream.Encoded = true;
                    _jobInfo.VideoStream.IsRawStream = true;

                    _jobInfo.TempFiles.Add(_jobInfo.VideoStream.TempFile);
                    _jobInfo.VideoStream.TempFile = outFile;
                    _jobInfo.VideoStream = VideoHelper.GetStreamInfo(_jobInfo.VideoStream);

                    _jobInfo.TempFiles.Add(Path.Combine(AppSettings.DemuxLocation, "x264_2pass.log"));
                    _jobInfo.TempFiles.Add(Path.Combine(AppSettings.DemuxLocation, "x264_2pass.log.mbtree"));
                    _jobInfo.TempFiles.Add(_jobInfo.AviSynthScript);
                    _jobInfo.TempFiles.Add(_jobInfo.FfIndexFile);
                    _jobInfo.TempFiles.Add(_jobInfo.AviSynthStereoConfig);
                }
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;
            e.Result = _jobInfo;
        }

        private void GenerateAviSynthScript(Size resizeTo)
        {
            SubtitleInfo sub = _jobInfo.SubtitleStreams.FirstOrDefault(item => item.HardSubIntoVideo);
            string subFile = string.Empty;
            bool keepOnlyForced = false;
            if (sub != null)
            {
                subFile = sub.TempFile;
                keepOnlyForced = sub.KeepOnlyForcedCaptions;
            }

            if ((_jobInfo.EncodingProfile.OutFormat == OutputType.OutputBluRay) || (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputAvchd))
            {
                _jobInfo.AviSynthScript = AviSynthGenerator.Generate(_jobInfo.VideoStream,
                                                                     false,
                                                                     0f,
                                                                     resizeTo,
                                                                     StereoEncoding.None, new StereoVideoInfo(),
                                                                     false,
                                                                     subFile,
                                                                     keepOnlyForced);
            }
            else
            {
                _jobInfo.AviSynthScript = AviSynthGenerator.Generate(_jobInfo.VideoStream,
                                                                     false,
                                                                     0f,
                                                                     resizeTo, 
                                                                     _jobInfo.EncodingProfile.StereoType,
                                                                     _jobInfo.StereoVideoStream,
                                                                     false,
                                                                     subFile,
                                                                     keepOnlyForced);
                if (!string.IsNullOrEmpty(AviSynthGenerator.StereoConfigFile))
                    _jobInfo.AviSynthStereoConfig = AviSynthGenerator.StereoConfigFile;
            }
        }
    }
}
