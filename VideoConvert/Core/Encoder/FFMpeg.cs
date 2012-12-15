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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using VideoConvert.Core.CommandLine;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;
using log4net;
using System.Text;
using System.Threading;

namespace VideoConvert.Core.Encoder
{
    class FfMpeg
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FfMpeg));

        private EncodeInfo _jobInfo;
        private const string Executable = "avconv.exe";

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
                    Log.ErrorFormat("avconv exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardError.ReadToEnd();
                    Regex regObj = new Regex(@"^.*avconv version ([\w\d\.\-_]+),.*$",
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
                Log.DebugFormat("avconv \"{0:s}\" found", verInfo);
            }

            return verInfo;
        }

        public void DoDemux(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("ffmpeg_demuxing_status");
            string progressFormat = Processing.GetResourceString("ffmpeg_demuxing_progress");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);


            string inputFile = string.IsNullOrEmpty(_jobInfo.TempInput) ? _jobInfo.InputFile : _jobInfo.TempInput;

            _jobInfo.VideoStream.TempFile = inputFile;
            _jobInfo.MediaInfo = Processing.GetMediaInfo(inputFile);

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-i \"{0}\" ", inputFile);

            for (int i = 0; i < _jobInfo.AudioStreams.Count; i++)
            {
                AudioInfo item = _jobInfo.AudioStreams[i];

                string ext = StreamFormat.GetFormatExtension(item.Format, item.FormatProfile, false);

                string acodec;

                switch (ext)
                {
                    case "flac":
                        acodec = "flac";
                        break;
                    case "wav":
                        acodec = "pcm_s16le";
                        break;
                    default:
                        acodec = "copy";
                        break;
                }

                string formattedExt = string.Format("demuxed.{0:g}.{1}.{2}", item.StreamId, item.LangCode, ext);

                item.TempFile =
                    Processing.CreateTempFile(
                        string.IsNullOrEmpty(_jobInfo.TempInput) ? _jobInfo.JobName : _jobInfo.TempInput, formattedExt);

                sb.AppendFormat("-map 0:a:{0:0} -vn -c:a {1} -y \"{2}\" ", item.StreamKindId, acodec, item.TempFile);

                _jobInfo.AudioStreams[i] = item;
            }
            
            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            Regex regObj = new Regex(@"^size=\s+?(\d+)[\w\s]+?time=([\d\.]+).+$",
                                     RegexOptions.Singleline | RegexOptions.Multiline);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                                                 {
                                                     WorkingDirectory = AppSettings.DemuxLocation,
                                                     Arguments = sb.ToString(),
                                                     CreateNoWindow = true,
                                                     UseShellExecute = false,
                                                     RedirectStandardError = true
                                                 };
                encoder.StartInfo = parameter;

                encoder.ErrorDataReceived += (outputSender, outputEvent) =>
                {
                    string line = outputEvent.Data;

                    if (string.IsNullOrEmpty(line)) return;

                    Match result = regObj.Match(line);
                    if (result.Success)
                    {
                        double secDemux;
                        Double.TryParse(result.Groups[2].Value, NumberStyles.Number, AppSettings.CInfo, out secDemux);
                        int progress = (int)Math.Floor(secDemux / _jobInfo.VideoStream.Length * 100d);

                        string progressStr = string.Format(progressFormat, Path.GetFileName(_jobInfo.InputFile),
                                                           progress);
                        _bw.ReportProgress(progress, progressStr);
                    }
                    else
                    {
                        Log.InfoFormat("avconv: {0:s}", line);
                    }
                };

                Log.InfoFormat("avconv {0:s}", parameter.Arguments);

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("avconv exception: {0}", ex);
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
                    encoder.WaitForExit();
                    encoder.CancelErrorRead();

                    _jobInfo.ExitCode = encoder.ExitCode;

                    if (_jobInfo.ExitCode == 0)
                        _jobInfo.VideoStream.TempFile = inputFile;

                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                }
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;
            e.Result = _jobInfo;
        }

        public void DoEncodeAc3(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("ffmpeg_encoding_audio_status");
            string encProgressFmt = Processing.GetResourceString("ffmpeg_encoding_audio_progress");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            AudioInfo item = _jobInfo.AudioStreams[_jobInfo.StreamId];

            int outChannels = -1;
            int outSampleRate = -1;

            switch (_jobInfo.AudioProfile.Type)
            {
                case ProfileType.AC3:
                    outChannels = ((AC3Profile) _jobInfo.AudioProfile).OutputChannels;
                    switch (outChannels)
                    {
                        case 0:
                            if (item.ChannelCount > 6)
                                outChannels = 6;
                            break;
                        case 1:
                            outChannels = 2;
                            break;
                        case 2:
                            outChannels = 3;
                            break;
                        case 3:
                            outChannels = 4;
                            break;
                        case 4:
                            outChannels = 1;
                            break;
                    }
                    outSampleRate = ((AC3Profile) _jobInfo.AudioProfile).SampleRate;
                    switch (outSampleRate)
                    {
                        case 1:
                            outSampleRate = 8000;
                            break;
                        case 2:
                            outSampleRate = 11025;
                            break;
                        case 3:
                            outSampleRate = 22050;
                            break;
                        case 4:
                            outSampleRate = 44100;
                            break;
                        case 5:
                            outSampleRate = 48000;
                            break;
                        default:
                            outSampleRate = 0;
                            break;
                    }
                    break;
                case ProfileType.Copy:
                    outChannels = item.ChannelCount > 6 ? 6 : item.ChannelCount;
                    outSampleRate = item.SampleRate;
                    if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd && outSampleRate != 48000)
                    {
                        outSampleRate = 48000;
                    }
                    break;
            }
            string inputFile = AviSynthGenerator.GenerateAudioScript(item.TempFile, item.Format, item.FormatProfile,
                                                                     item.ChannelCount, outChannels, item.SampleRate,
                                                                     outSampleRate, (long) _jobInfo.VideoStream.Length);
            string outFile = Processing.CreateTempFile(item.TempFile, "encoded.ac3");

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            Regex regObj = new Regex(@"^size=.\s*?([\d]*?)kB\s*?time=\s*?([\d\.]*)\s*?bitrate=\s*?([\d\.]*?)kbit.*$",
                                     RegexOptions.Singleline | RegexOptions.Multiline);

            DateTime startTime = DateTime.Now;

            using (Process encoder = new Process(),
                           decoder = BePipe.GenerateProcess(inputFile))
            {
                ProcessStartInfo encoderParameter = new ProcessStartInfo(localExecutable)
                                                        {
                                                            WorkingDirectory = AppSettings.DemuxLocation,
                                                            Arguments =
                                                                FfmpegCommandLineGenerator.GenerateAC3EncodeLine(
                                                                    _jobInfo,
                                                                    item,
                                                                    "-",
                                                                    outFile),
                                                            CreateNoWindow = true,
                                                            UseShellExecute = false,
                                                            RedirectStandardError = true,
                                                            RedirectStandardInput = true
                                                        };
                encoder.StartInfo = encoderParameter;

                AudioInfo localItem = item;
                DateTime time = startTime;
                encoder.ErrorDataReceived += (outputSender, outputEvent) =>
                {
                    string line = outputEvent.Data;

                    if (string.IsNullOrEmpty(line)) return;

                    Match result = regObj.Match(line);
                    if (result.Success)
                    {
                        int progress = -1;
                        float procTime;
                        Single.TryParse(result.Groups[2].Value, NumberStyles.Number, AppSettings.CInfo, out procTime);

                        if (procTime > 0f)
                            progress = (int)Math.Round(procTime / localItem.Length * 100, 0);

                        TimeSpan eta = DateTime.Now.Subtract(time);
                        double timeRemaining = localItem.Length - procTime;
                        long secRemaining = 0;

                        if (eta.Seconds != 0)
                        {
                            double speed = Math.Round(procTime / eta.TotalSeconds, 2);

                            if (speed > 1)
                                secRemaining = (long)Math.Round(timeRemaining / speed, 0);
                            else
                                secRemaining = 0;
                        }

                        if (secRemaining < 0)
                            secRemaining = 0;

                        TimeSpan remaining = new TimeSpan(0, 0, (int)secRemaining);

                        DateTime ticks1 = new DateTime(eta.Ticks);

                        string encProgress = string.Format(encProgressFmt, ticks1, remaining);
                        _bw.ReportProgress(progress, encProgress);
                    }
                    else
                    {
                        Log.InfoFormat("avconv: {0:s}", line);
                    }
                };

                Log.InfoFormat("avconv {0:s}", encoderParameter.Arguments);

                bool encStarted;
                bool decStarted;
                try
                {
                    encStarted = encoder.Start();
                }
                catch (Exception ex)
                {
                    encStarted = false;
                    Log.ErrorFormat("avconv exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

                try
                {
                    decStarted = decoder.Start();
                }
                catch (Exception ex)
                {
                    decStarted = false;
                    Log.ErrorFormat("bepipe exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

                if (encStarted && decStarted)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.BeginErrorReadLine();
                    decoder.PriorityClass = AppSettings.GetProcessPriority();

                    Processing.CopyStreamToStream(decoder.StandardOutput.BaseStream, encoder.StandardInput.BaseStream, 32768,
                                                  (src, dst, exc) =>
                                                      {
                                                          src.Close();
                                                          dst.Close();

                                                          if (exc == null) return;

                                                          Log.Debug(exc.Message);
                                                          Log.Debug(exc.StackTrace);
                                                      });

                    while (!encoder.HasExited)
                    {
                        if (_bw.CancellationPending)
                        {
                            encoder.Kill();
                            decoder.Kill();
                        }
                        Thread.Sleep(200);
                    }
                    encoder.WaitForExit();
                    encoder.CancelErrorRead();
                    decoder.WaitForExit();

                    _jobInfo.ExitCode = encoder.ExitCode;
                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);

                    if (_jobInfo.ExitCode == 0)
                    {
                        _jobInfo.TempFiles.Add(inputFile);
                        _jobInfo.TempFiles.Add(item.TempFile);
                        _jobInfo.TempFiles.Add(item.TempFile + ".d2a");
                        _jobInfo.TempFiles.Add(item.TempFile + ".ffindex");
                        item.TempFile = outFile;
                        AudioHelper.GetStreamInfo(item);
                    }
                }
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;
            e.Result = _jobInfo;
        }

        public void GetCrop(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;
            
            string status = Processing.GetResourceString("ffmpeg_get_croprect_status");
            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0);

            int frames;
            string inputFile = AviSynthGenerator.GenerateCropDetect(_jobInfo.VideoStream.TempFile,
                                                                    _jobInfo.VideoStream.FPS,
                                                                    _jobInfo.VideoStream.Length, out frames);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            Regex cropReg = new Regex(@"^.*crop=(\d*):(\d*):(\d*):(\d*).*$",
                                      RegexOptions.Singleline | RegexOptions.Multiline);
            Regex frameReg = new Regex(@"^.*frame=\s*(\d*).*$", RegexOptions.Singleline | RegexOptions.Multiline);

            using (Process encoder = new Process())
            {
                ProcessStartInfo encoderParameter = new ProcessStartInfo(localExecutable)
                                                        {
                                                            WorkingDirectory = AppSettings.DemuxLocation,
                                                            Arguments = string.Format(AppSettings.CInfo,
                                                                                      " -threads {0:g} -i \"{1:s}\" -vf cropdetect -vcodec rawvideo -an -sn -f matroska -y NUL",
                                                                                      Environment.ProcessorCount + 1,
                                                                                      inputFile),
                                                            CreateNoWindow = true,
                                                            UseShellExecute = false,
                                                            RedirectStandardError = true
                                                        };
                encoder.StartInfo = encoderParameter;

                encoder.ErrorDataReceived += (outputSender, outputEvent) =>
                {
                    string line = outputEvent.Data;

                    if (string.IsNullOrEmpty(line)) return;

                    Match cropResult = cropReg.Match(line);
                    Match frameResult = frameReg.Match(line);
                    if (cropResult.Success)
                    {
                        Point loc = new Point();
                        int tempVal;

                        Int32.TryParse(cropResult.Groups[3].Value, NumberStyles.Number, AppSettings.CInfo, out tempVal);
                        loc.X = tempVal;

                        Int32.TryParse(cropResult.Groups[4].Value, NumberStyles.Number, AppSettings.CInfo, out tempVal);
                        loc.Y = tempVal;

                        _jobInfo.VideoStream.CropRect.Location = loc;

                        Int32.TryParse(cropResult.Groups[1].Value, NumberStyles.Number, AppSettings.CInfo, out tempVal);
                        _jobInfo.VideoStream.CropRect.Width = tempVal;

                        Int32.TryParse(cropResult.Groups[2].Value, NumberStyles.Number, AppSettings.CInfo, out tempVal);
                        _jobInfo.VideoStream.CropRect.Height = tempVal;

                    }
                    else if (frameResult.Success)
                    {
                        int tempVal;

                        Int32.TryParse(frameResult.Groups[1].Value, NumberStyles.Number, AppSettings.CInfo, out tempVal);
                        int progress = (int)Math.Round((float)tempVal / frames * 100, 0);

                        _bw.ReportProgress(progress, status);
                    }
                    else
                    {
                        Log.InfoFormat("avconv: {0:s}", line);
                    }
                };

                Log.InfoFormat("avconv {0:s}", encoderParameter.Arguments);

                bool encstarted;
                try
                {
                    encstarted = encoder.Start();
                }
                catch (Exception ex)
                {
                    encstarted = false;
                    Log.ErrorFormat("avconv exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

                if (encstarted)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.BeginErrorReadLine();

                    _bw.ReportProgress(-1, status);

                    _jobInfo.VideoStream.CropRect = new Rectangle();

                    while (!encoder.HasExited)
                    {
                        if (_bw.CancellationPending)
                            encoder.Kill();
                        Thread.Sleep(200);
                    }
                    encoder.WaitForExit();
                    encoder.CancelErrorRead();

                    _jobInfo.ExitCode = encoder.ExitCode;
                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                }
            }

            int mod16Temp;
            int mod16Width = Math.DivRem(_jobInfo.VideoStream.Width, 16, out mod16Temp);
            mod16Width *= 16;

            int mod16Height = Math.DivRem(_jobInfo.VideoStream.Height, 16, out mod16Temp);
            mod16Height *= 16;

            if (_jobInfo.VideoStream.CropRect.Width == mod16Width)
            {
                _jobInfo.VideoStream.CropRect.Width = _jobInfo.VideoStream.Width;
                Point lPoint = _jobInfo.VideoStream.CropRect.Location;
                lPoint.X = 0;
                _jobInfo.VideoStream.CropRect.Location = lPoint;
            }

            if (_jobInfo.VideoStream.CropRect.Height == mod16Height)
            {
                _jobInfo.VideoStream.CropRect.Height = _jobInfo.VideoStream.Height;
                Point lPoint = _jobInfo.VideoStream.CropRect.Location;
                lPoint.Y = 0;
                _jobInfo.VideoStream.CropRect.Location = lPoint;
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;
            _jobInfo.TempFiles.Add(inputFile);
            e.Result = _jobInfo;
        }

        public static Process GenerateDecodeProcess(string scriptName)
        {
            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = localExecutable,
                Arguments =
                    String.Format(AppSettings.CInfo, "-i \"{0}\" -f yuv4mpegpipe -",
                                  scriptName),
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            Process ffmpeg = new Process { StartInfo = info };

            ffmpeg.ErrorDataReceived += (sender, args) =>
                                            {
                                                string line = args.Data;
                                                if (string.IsNullOrEmpty(line)) return;

                                                Regex frameReg = new Regex(@"^.*frame=\s*(\d*).*$", RegexOptions.Singleline | RegexOptions.Multiline);
                                                Match frameResult = frameReg.Match(line);
                                                if (!frameResult.Success)
                                                    Log.Info(line);

                                            };
            Log.Info("ffmpeg decoding process created!");
            Log.Info("params: avconv " + ffmpeg.StartInfo.Arguments);

            return ffmpeg;
        }
    }
}
