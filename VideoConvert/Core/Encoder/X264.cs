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
using System.IO.Pipes;
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
        private const string Executable64 = "x264_64.exe";

        private long _frameCount;
        private DateTime _startTime = DateTime.Now;
        private TimeSpan _remaining = new TimeSpan(0, 0, 0);
        private readonly string _progressFormat = Processing.GetResourceString("x264_encoding_progress");
        private string _pass = string.Empty;
        private readonly Regex _frameInformation = 
            new Regex(@"^\D?([\d]+).*frames: ([\d\.]+) fps, ([\d\.]+).*$",
                      RegexOptions.Singleline | RegexOptions.Multiline);

        private readonly Regex _fullFrameInformation =
            new Regex(@"^\[[\d\.]+?%\] ([\d]+?)/([\d]+?) frames, ([\d\.]+?) fps, ([\d\.]+?) kb/s.*$",
                      RegexOptions.Singleline | RegexOptions.Multiline);

        private BackgroundWorker _bw;

        public void SetJob(EncodeInfo job)
        {
            _jobInfo = job;
        }

        public string GetVersionInfo(bool use64Bit)
        {
            return GetVersionInfo(AppSettings.ToolsPath, use64Bit);
        }

        public string GetVersionInfo(string encPath, bool use64Bit)
        {
            string verInfo = string.Empty;

            if (use64Bit && !Environment.Is64BitOperatingSystem) return string.Empty;

            string localExecutable = Path.Combine(encPath, use64Bit ? Executable64 : Executable);

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
                        verInfo = string.Format("Core: {0} Build {1}", result.Groups[2].Value, result.Groups[3].Value);

                    encoder.WaitForExit(10000);
                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
            {
                if (use64Bit)
                    Log.Debug("Selected 64 bit encoder");
                Log.DebugFormat("x264 \"{0:s}\" found", verInfo);
            }

            return verInfo;
        }

        public void DoEncode(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string passStr = Processing.GetResourceString("x264_pass");
            string status = Processing.GetResourceString("x264_encoding_status");

            bool use64BitEncoder = AppSettings.Use64BitEncoders &&
                                   AppSettings.X26464Installed &&
                                   Environment.Is64BitOperatingSystem;

            X264Profile encProfile = (X264Profile)_jobInfo.VideoProfile;

            if (!_jobInfo.EncodingProfile.Deinterlace && _jobInfo.VideoStream.Interlaced)
                _jobInfo.VideoStream.Interlaced = false;

            Size resizeTo = VideoHelper.GetTargetSize(_jobInfo);

            if (string.IsNullOrEmpty(_jobInfo.AviSynthScript))
                GenerateAviSynthScript(resizeTo);

            string inputFile = _jobInfo.AviSynthScript;
            string outFile =
                Processing.CreateTempFile(
                    string.IsNullOrEmpty(_jobInfo.TempOutput) ? _jobInfo.BaseName : _jobInfo.TempOutput, "encoded.264");

            int targetBitrate = 0;
            if (_jobInfo.EncodingProfile.TargetFileSize > 0)
                targetBitrate = Processing.CalculateVideoBitrate(_jobInfo);

            int encodeMode = encProfile.EncodingMode;
            if ((encodeMode == 2) || (encodeMode == 3))
                _pass = string.Format(" {1} {0:0}; ", _jobInfo.StreamId, passStr);

            _frameCount = _jobInfo.VideoStream.FrameCount;

            _bw.ReportProgress(-10, status + _pass.Replace("; ", string.Empty));
            _bw.ReportProgress(0, status);

            string argument = X264CommandLineGenerator.Generate(encProfile,
                                                                targetBitrate,
                                                                resizeTo.Width,
                                                                resizeTo.Height,
                                                                _jobInfo.StreamId,
                                                                _jobInfo.VideoStream.FrameRateEnumerator,
                                                                _jobInfo.VideoStream.FrameRateDenominator,
                                                                _jobInfo.EncodingProfile.StereoType,
                                                                _jobInfo.VideoStream.PicSize,

                                                                // check if we use 64 bit version
                                                                //use64BitEncoder ? "-" : inputFile,
                                                                "-",
                                                                outFile);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, use64BitEncoder ? Executable64 : Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation,
                        Arguments = argument,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardInput = use64BitEncoder
                    };
                encoder.StartInfo = parameter;

                encoder.ErrorDataReceived += OnDataReceived;
                
                Log.InfoFormat("start parameter: x264 {0:s}", argument);

                bool started;
                bool decStarted;

                NamedPipeServerStream decodePipe = new NamedPipeServerStream(AppSettings.DecodeNamedPipeName,
                                                                             PipeDirection.InOut, 3,
                                                                             PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                decodePipe.BeginWaitForConnection(DecoderConnected, null);

                Size originalSize = new Size(_jobInfo.VideoStream.Width, _jobInfo.VideoStream.Height);
                if (_jobInfo.VideoStream.Width < _jobInfo.VideoStream.Height*_jobInfo.VideoStream.AspectRatio)
                {
                    originalSize.Width = (int)(_jobInfo.VideoStream.Height*_jobInfo.VideoStream.AspectRatio);
                    int temp;
                    Math.DivRem(originalSize.Width, 2, out temp);
                    originalSize.Width += temp;
                }

                Process decoder = FfMpeg.GenerateDecodeProcess(inputFile,
                    AppSettings.Use64BitEncoders && AppSettings.UseFfmpegScaling,
                    originalSize, _jobInfo.VideoStream.AspectRatio,
                    _jobInfo.VideoStream.CropRect, resizeTo);
                try
                {
                    decStarted = decoder.Start();
                }
                catch (Exception ex)
                {
                    decStarted = false;
                    Log.ErrorFormat("avconv exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }    

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

                _startTime = DateTime.Now;

                if (started && decStarted)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.BeginErrorReadLine();

                    decoder.PriorityClass = AppSettings.GetProcessPriority();
                    decoder.BeginErrorReadLine();

                    Thread pipeReadThread = new Thread(() =>
                    {
                        try
                        {
                            if (encoder != null) ReadThreadStart(decodePipe, encoder);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    });
                    pipeReadThread.Start();
                    pipeReadThread.Priority = ThreadPriority.BelowNormal;
                    decoder.Exited += (o, args) =>
                        {
                            try
                            {
                                decodePipe.Disconnect();
                                decodePipe.Close();
                                decodePipe.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex);
                            }
                        };
                    encoder.Exited += (o, args) => pipeReadThread.Abort();

                    while (!encoder.HasExited && !decoder.HasExited)
                    {
                        if (_bw.CancellationPending)
                        {
                            encoder.Kill();
                            decoder.Kill();
                        }
                        Thread.Sleep(200);
                    }
                    encoder.WaitForExit(10000);
                    encoder.CancelErrorRead();
                    decoder.WaitForExit(10000);
                    decoder.CancelErrorRead();

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
                    

                    _jobInfo.VideoStream.Encoded = true;
                    _jobInfo.VideoStream.IsRawStream = true;

                    _jobInfo.TempFiles.Add(_jobInfo.VideoStream.TempFile);
                    _jobInfo.VideoStream.TempFile = outFile;

                    try
                    {
                        _jobInfo.MediaInfo = Processing.GetMediaInfo(_jobInfo.VideoStream.TempFile);
                    }
                    catch (TimeoutException ex)
                    {
                        Log.Error(ex);
                    }
                    _jobInfo.VideoStream = VideoHelper.GetStreamInfo(_jobInfo.MediaInfo, _jobInfo.VideoStream,
                                                                     _jobInfo.EncodingProfile.OutFormat ==
                                                                     OutputType.OutputBluRay);

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

        private void OnDataReceived(object outputSender, DataReceivedEventArgs outputEvent)
        {
            string line = outputEvent.Data;

            if (string.IsNullOrEmpty(line)) return;

            Match frameMatch = _frameInformation.Match(line);
            Match fullFrameMatch = _fullFrameInformation.Match(line);

            TimeSpan eta = DateTime.Now.Subtract(_startTime);

            long current;
            long framesRemaining;
            long secRemaining = 0;

            float encBitrate;
            float fps;
            DateTime ticks;
            double codingFPS;
            if (frameMatch.Success)
            {
                Int64.TryParse(frameMatch.Groups[1].Value, NumberStyles.Number,
                               AppSettings.CInfo, out current);
                framesRemaining = _frameCount - current;

                if (eta.Seconds != 0)
                {
                    //Frames per Second
                    codingFPS = Math.Round(current / eta.TotalSeconds, 2);

                    if (codingFPS > 1)
                        secRemaining = framesRemaining / (int)codingFPS;
                    else
                        secRemaining = 0;
                }

                if (secRemaining > 0)
                    _remaining = new TimeSpan(0, 0, (int)secRemaining);

                ticks = new DateTime(eta.Ticks);

                Single.TryParse(frameMatch.Groups[2].Value, NumberStyles.Number,
                                AppSettings.CInfo, out fps);
                Single.TryParse(frameMatch.Groups[3].Value, NumberStyles.Number,
                                AppSettings.CInfo, out encBitrate);

                string progress = string.Format(_progressFormat,
                                                current, _frameCount,
                                                fps,
                                                encBitrate,
                                                _remaining, ticks, _pass);
                _bw.ReportProgress((int)(((float)current / _frameCount) * 100),
                                   progress);

            }
            else if (fullFrameMatch.Success)
            {
                Int64.TryParse(fullFrameMatch.Groups[1].Value, NumberStyles.Number,
                               AppSettings.CInfo, out current);
                Int64.TryParse(fullFrameMatch.Groups[2].Value, NumberStyles.Number,
                               AppSettings.CInfo, out _frameCount);

                framesRemaining = _frameCount - current;

                if (eta.Seconds != 0)
                {
                    //Frames per Second
                    codingFPS = Math.Round(current / eta.TotalSeconds, 2);

                    if (codingFPS > 1)
                        secRemaining = framesRemaining / (int)codingFPS;
                    else
                        secRemaining = 0;
                }

                if (secRemaining > 0)
                    _remaining = new TimeSpan(0, 0, (int)secRemaining);

                ticks = new DateTime(eta.Ticks);

                Single.TryParse(fullFrameMatch.Groups[3].Value, NumberStyles.Number,
                                AppSettings.CInfo, out fps);
                Single.TryParse(fullFrameMatch.Groups[4].Value, NumberStyles.Number,
                                AppSettings.CInfo, out encBitrate);

                string progress = string.Format(_progressFormat,
                                                current, _frameCount,
                                                fps,
                                                encBitrate,
                                                _remaining, ticks, _pass);
                _bw.ReportProgress((int)(((float)current / _frameCount) * 100),
                                   progress);
            }
            else
            {
                Log.InfoFormat("x264: {0:s}", line);
            }
        }

        private static void DecoderConnected(IAsyncResult ar)
        {
            Log.Info("decoder pipe connected");
        }

        private void ReadThreadStart(NamedPipeServerStream decodePipe, Process encoder)
        {
            if (!decodePipe.IsConnected)
                decodePipe.WaitForConnection();

            try
            {
                decodePipe.CopyTo(encoder.StandardInput.BaseStream);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            

            encoder.StandardInput.BaseStream.Close();
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

            if ((_jobInfo.EncodingProfile.OutFormat == OutputType.OutputBluRay) ||
                (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputAvchd))
                _jobInfo.AviSynthScript = AviSynthGenerator.Generate(_jobInfo.VideoStream,
                                                                     false,
                                                                     0f,
                                                                     resizeTo,
                                                                     StereoEncoding.None, new StereoVideoInfo(),
                                                                     false,
                                                                     subFile,
                                                                     keepOnlyForced,
                                                                     AppSettings.Use64BitEncoders && AppSettings.UseFfmpegScaling);
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
                                                                     keepOnlyForced,
                                                                     AppSettings.Use64BitEncoders && AppSettings.UseFfmpegScaling);
                if (!string.IsNullOrEmpty(AviSynthGenerator.StereoConfigFile))
                    _jobInfo.AviSynthStereoConfig = AviSynthGenerator.StereoConfigFile;
            }
        }
    }
}
