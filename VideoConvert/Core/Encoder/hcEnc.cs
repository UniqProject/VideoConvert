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
using System.Collections.Generic;
using System.Linq;
using VideoConvert.Core.CommandLine;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;
using log4net;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ManagedWinapi.Windows;
using System.Threading;
using System.Globalization;
using System.Drawing;

namespace VideoConvert.Core.Encoder
{
    class HcEnc
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HcEnc));

        private EncodeInfo _jobInfo;
        private const string Executable = "HCenc_025.exe";

        private BackgroundWorker _bw;

        private static SystemWindow FindWindowByDialogId(int dialogId, IEnumerable<SystemWindow> source)
        {
            return source.FirstOrDefault(win => win.DialogID == dialogId);
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
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        Arguments = "-noini -wait 5"
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
                    Log.ErrorFormat("hcenc exception: {0}", ex);
                }

                if (started)
                {
                    encoder.WaitForInputIdle(2500);
                    SystemWindow mainWin = new SystemWindow(encoder.MainWindowHandle) {VisibilityFlag = false};

                    Regex regObj = new Regex(@"^.*HCenc ([\d\.]*?)$", RegexOptions.Singleline | RegexOptions.Multiline);
                    Match result = regObj.Match(mainWin.Title);
                    if (result.Success)
                        verInfo = result.Groups[1].Value;

                    encoder.WaitForExit(10000);
                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
                Log.DebugFormat("HCenc \"{0:s}\" found", verInfo);

            return verInfo;
        }

        public void DoEncodeDvd(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("hcenc_encoding_status");
            string progressFmt = Processing.GetResourceString("hcenc_encoding_progress");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string inputFile = _jobInfo.VideoStream.TempFile;
            string outFile = Processing.CreateTempFile(inputFile, "encoded.m2v");

            _jobInfo.AviSynthScript = GenerateAviSynthFile();

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo encoderParameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation
                    };

                float sourceAspect = (float)Math.Round(_jobInfo.VideoStream.AspectRatio, 3);

                int targetAspect = sourceAspect >= 1.4f ? 1 : 0;

                int bitrate = 0;
                if (_jobInfo.EncodingProfile.TargetFileSize > 1)
                    bitrate = Processing.CalculateVideoBitrate(_jobInfo);

                int audBitrate = _jobInfo.AudioStreams.Sum(stream => (int) stream.Bitrate/1000);

                int maxRate = 9800 - audBitrate;

                string iniFile = HcencCommandLineGenerator.Generate((HcEncProfile) _jobInfo.VideoProfile,
                                                                    _jobInfo.AviSynthScript, outFile, targetAspect,
                                                                    bitrate, maxRate);

                encoderParameter.Arguments = string.Format("-ini \"{0}\" ", iniFile);

                encoderParameter.UseShellExecute = true;
                encoderParameter.WindowStyle = ProcessWindowStyle.Minimized;

                encoder.StartInfo = encoderParameter;

                Log.InfoFormat("hcenc {0:s}", encoderParameter.Arguments);

                bool encstarted;
                try
                {
                    encstarted = encoder.Start();
                }
                catch (Exception ex)
                {
                    encstarted = false;
                    Log.ErrorFormat("hcenc exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

                DateTime startTime = DateTime.Now;
                TimeSpan remaining = new TimeSpan(0, 0, 0);

                if (encstarted)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.WaitForInputIdle(10000);

                    SystemWindow mainWin = new SystemWindow(encoder.MainWindowHandle) {VisibilityFlag = false};

                    SystemWindow processedFrames = FindWindowByDialogId(1028, mainWin.AllChildWindows);
                    SystemWindow averageFps = FindWindowByDialogId(1061, mainWin.AllChildWindows);
                    SystemWindow pass = FindWindowByDialogId(1013, mainWin.AllChildWindows);
                    SystemWindow info = FindWindowByDialogId(1053, mainWin.AllChildWindows);
                    SystemWindow currProgress = FindWindowByDialogId(1065, mainWin.AllChildWindows);

                    string lastInfo = string.Empty;

                    while (!encoder.HasExited)
                    {
                        if (_bw.CancellationPending)
                            encoder.Kill();
                        else
                        {
                            try
                            {
                                Regex regObj = new Regex(@"^.*?([\d]*?)%\s*?HCenc.*$",
                                                         RegexOptions.Singleline | RegexOptions.Multiline);
                                Match result = regObj.Match(mainWin.Title);
                                if (result.Success)
                                {
                                    int overallProgress;
                                    Int32.TryParse(result.Groups[1].Value, NumberStyles.Number, AppSettings.CInfo,
                                                   out overallProgress);
                                    double codingFps;
                                    Double.TryParse(averageFps.Content.ShortDescription, NumberStyles.Number,
                                                    AppSettings.CInfo, out codingFps);
                                    int currentProgress;
                                    Int32.TryParse(currProgress.Content.ShortDescription.TrimEnd('%'),
                                                   NumberStyles.Number, AppSettings.CInfo, out currentProgress);
                                    int frame;
                                    Int32.TryParse(processedFrames.Content.ShortDescription, NumberStyles.Number,
                                                   AppSettings.CInfo, out frame);

                                    if (info.Content.ShortDescription != lastInfo)
                                    {
                                        lastInfo = info.Content.ShortDescription;
                                        Log.InfoFormat("hcenc: {0:s}", lastInfo);
                                    }

                                    DateTime now = DateTime.Now;
                                    TimeSpan eta = now.Subtract(startTime);

                                    int percentRemain = 100 - overallProgress;
                                    double secRemaining = 0d;

                                    if (eta.Seconds != 0)
                                    {
                                        double speed = Math.Round(overallProgress/eta.TotalSeconds, 6);

                                        if (speed > 0f)
                                            secRemaining = percentRemain/speed;
                                        else
                                            secRemaining = 0;
                                    }

                                    if (secRemaining > 0)
                                        remaining = new TimeSpan(0, 0, (int) secRemaining);

                                    DateTime ticks1 = new DateTime(eta.Ticks);

                                    string progress = string.Format(progressFmt,
                                                                    frame, codingFps, pass.Content.ShortDescription,
                                                                    currentProgress, ticks1, remaining);

                                    _bw.ReportProgress(overallProgress, progress);
                                }
                            }
                            catch (Exception exception)
                            {
                                Log.ErrorFormat("hcEnc Exception {0}", exception.Message);
                            }
                        }
                        Thread.Sleep(500);
                    }
                    
                    encoder.WaitForExit(10000);
                    _jobInfo.ExitCode = encoder.ExitCode;

                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                    if (_jobInfo.ExitCode == 0)
                    {
                        _jobInfo.TempFiles.Add(inputFile);
                        _jobInfo.VideoStream.TempFile = outFile;
                        _jobInfo.TempFiles.Add(_jobInfo.AviSynthScript);
                        _jobInfo.TempFiles.Add(iniFile);

                        try
                        {
                            _jobInfo.MediaInfo = Processing.GetMediaInfo(_jobInfo.VideoStream.TempFile);
                        }
                        catch (TimeoutException ex)
                        {
                            Log.Error(ex);
                        }

                        _jobInfo.VideoStream = VideoHelper.GetStreamInfo(_jobInfo.MediaInfo, _jobInfo.VideoStream, _jobInfo.EncodingProfile.OutFormat == OutputType.OutputBluRay);
                        _jobInfo.TempFiles.Add(Path.Combine(AppSettings.DemuxLocation, "HC01.lls"));
                        _jobInfo.TempFiles.Add(_jobInfo.FfIndexFile);
                    }
                }
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;
            e.Result = _jobInfo;

        }

        private string GenerateAviSynthFile()
        {
            int targetHeight;

            float sourceFPS = (float)Math.Round(_jobInfo.VideoStream.FPS, 3);
            float targetFPS = 0f;
            bool changeFPS = false;

            int targetWidth = _jobInfo.VideoStream.AspectRatio >= 1.4f ? 1024 : 720;

            if (_jobInfo.Input == InputType.InputDvd)
                _jobInfo.VideoStream.Width =
                    (int) Math.Round(_jobInfo.VideoStream.Height*_jobInfo.VideoStream.AspectRatio, 0);

            if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
            {
                if (_jobInfo.EncodingProfile.SystemType == 0)
                {
                    targetHeight = 576;
                    if (Math.Abs(sourceFPS - 25f) > 0)
                        changeFPS = true;
                    targetFPS = 25f;
                }
                else
                {
                    targetHeight = 480;
                    if (Math.Abs(sourceFPS - 29.970f) > 0 && Math.Abs(sourceFPS - 23.976f) > 0)
                        changeFPS = true;
                    targetFPS = (float)Math.Round(30000f / 1001f, 3);
                }
            }
            else
            {
                targetWidth = _jobInfo.EncodingProfile.TargetWidth;
                targetHeight = (int)Math.Floor(targetWidth / _jobInfo.VideoStream.AspectRatio);
            }

            Size resizeTo = new Size(targetWidth, targetHeight);

            SubtitleInfo sub = _jobInfo.SubtitleStreams.FirstOrDefault(item => item.HardSubIntoVideo);
            string subFile = string.Empty;
            bool keepOnlyForced = false;
            if (sub != null)
            {
                subFile = sub.TempFile;
                keepOnlyForced = sub.KeepOnlyForcedCaptions;
            }

            return AviSynthGenerator.Generate(_jobInfo.VideoStream, changeFPS, targetFPS, resizeTo, StereoEncoding.None,
                                              new StereoVideoInfo(), true, subFile, keepOnlyForced);
        }
    }
}
