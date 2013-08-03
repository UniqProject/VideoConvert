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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Documents;
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
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(FfMpeg));

        /// <summary>
        /// Executable filename
        /// </summary>
        private const string Executable = "ffmpeg.exe";

        /// <summary>
        /// 64bit Executable filename
        /// </summary>
        private const string Executable64 = "ffmpeg_64.exe";

        private EncodeInfo _jobInfo;
        private BackgroundWorker _bw;

        private readonly Regex _demuxReg = new Regex(@"^size=\s+?(\d+)[\w\s]+?time=([\d\.\:]+).+$",
                                                   RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly string _demuxProgressFormat = Processing.GetResourceString("ffmpeg_demuxing_progress");

        private readonly Regex _ac3EncReg =
            new Regex(@"^size=.\s*?([\d]*?)kB\s*?time=\s*?([\d\.\:]*)\s*?bitrate=\s*?([\d\.]*?)kbit.*$",
                      RegexOptions.Singleline | RegexOptions.Multiline);
        private readonly string _ac3EncProgressFmt = Processing.GetResourceString("ffmpeg_encoding_audio_progress");

        private readonly Regex _cropReg = new Regex(@"^.*crop=(\d*):(\d*):(\d*):(\d*).*$",
                                                    RegexOptions.Singleline | RegexOptions.Multiline);
        private int _cropDetectFrames;
        private readonly string _cropDetectStatus = Processing.GetResourceString("ffmpeg_get_croprect_status");

        private static readonly Regex FrameReg = new Regex(@"^.*frame=\s*(\d*).*$", RegexOptions.Singleline | RegexOptions.Multiline);

        private AudioInfo _localItem = new AudioInfo();
        private DateTime _encodingStart = DateTime.Now;

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
        /// <param name="use64Bit">Defines whether the 64bit encoder should be used</param>
        /// <returns>Encoder version</returns>
        public string GetVersionInfo(bool use64Bit)
        {
            return GetVersionInfo(AppSettings.ToolsPath, use64Bit);
        }

        /// <summary>
        /// Reads encoder version from its output, use path settings from parameters
        /// </summary>
        /// <param name="encPath">Path to encoder</param>
        /// <param name="use64Bit"></param>
        /// <returns>Encoder version</returns>
        public string GetVersionInfo(string encPath, bool use64Bit)
        {
            string verInfo = string.Empty;

            if (use64Bit && !Environment.Is64BitOperatingSystem) return string.Empty;

            string localExecutable = Path.Combine(encPath, use64Bit ? Executable64 : Executable);

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
                    Log.ErrorFormat("ffmpeg exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardError.ReadToEnd();
                    Regex regObj = new Regex(@"^.*ffmpeg version ([\w\d\.\-_]+)[, ].*$",
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
            {
                if (use64Bit)
                    Log.Debug("Selected 64 bit encoder");
                Log.DebugFormat("ffmpeg \"{0:s}\" found", verInfo);
            }
            return verInfo;
        }

        /// <summary>
        /// Demux processing function, called by BackgroundWorker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DoDemux(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("ffmpeg_demuxing_status");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string inputFile = string.IsNullOrEmpty(_jobInfo.TempInput) ? _jobInfo.InputFile : _jobInfo.TempInput;
            _jobInfo.VideoStream.TempFile = inputFile;
            try
            {
                _jobInfo.MediaInfo = Processing.GetMediaInfo(inputFile);
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex);
            }
            
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("-i \"{0}\" ", inputFile);

            bool hasStreams = false;

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

                hasStreams = true;
                _jobInfo.AudioStreams[i] = item;
            }
            
            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);
            if (hasStreams)
            {
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
                    encoder.ErrorDataReceived += DemuxOnErrorDataReceived;

                    Log.InfoFormat("ffmpeg {0:s}", parameter.Arguments);

                    bool started;
                    try
                    {
                        started = encoder.Start();
                    }
                    catch (Exception ex)
                    {
                        started = false;
                        Log.ErrorFormat("ffmpeg exception: {0}", ex);
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

                        if (_jobInfo.ExitCode == 0)
                            _jobInfo.VideoStream.TempFile = inputFile;

                        Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                    }
                }
            }
            else
                _jobInfo.ExitCode = 0;

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;
            e.Result = _jobInfo;
        }

        /// <summary>
        /// Parses demux output from the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="outputEvent"></param>
        private void DemuxOnErrorDataReceived(object sender, DataReceivedEventArgs outputEvent)
        {
            string line = outputEvent.Data;
            if (string.IsNullOrEmpty(line)) return;

            Match result = _demuxReg.Match(line);
            if (result.Success)
            {
                TimeSpan procTime;
                TimeSpan.TryParseExact(result.Groups[2].Value, @"hh\:mm\:ss\.ff", AppSettings.CInfo, out procTime);

                double secDemux = procTime.TotalSeconds;
                int progress = (int)Math.Floor(secDemux / _jobInfo.VideoStream.Length * 100d);

                string progressStr = string.Format(_demuxProgressFormat, Path.GetFileName(_jobInfo.InputFile),
                                                   progress);
                _bw.ReportProgress(progress, progressStr);
            }
            else
                Log.InfoFormat("ffmpeg: {0:s}", line);
        }

        /// <summary>
        /// AC3 encode processing function, called by BackgroundWorker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DoEncodeAc3(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            bool use64BitEncoder = AppSettings.Use64BitEncoders &&
                                   AppSettings.Ffmpeg64Installed &&
                                   Environment.Is64BitOperatingSystem;

            int[] sampleRateArr = {0, 8000, 11025, 22050, 44100, 48000};
            int[] channelArr = {0, 2, 3, 4, 1};

            string status = Processing.GetResourceString("ffmpeg_encoding_audio_status");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            AudioInfo item = _jobInfo.AudioStreams[_jobInfo.StreamId];

            int outChannels = -1;
            int outSampleRate = -1;

            switch (_jobInfo.AudioProfile.Type)
            {
                case ProfileType.AC3:
                    outChannels = ((AC3Profile) _jobInfo.AudioProfile).OutputChannels;
                    outChannels = channelArr[outChannels];
                    if (item.ChannelCount > 6)
                        outChannels = 6;

                    outSampleRate = ((AC3Profile) _jobInfo.AudioProfile).SampleRate;
                    outSampleRate = sampleRateArr[outSampleRate];
                    break;
                case ProfileType.Copy:
                    outChannels = item.ChannelCount > 6 ? 6 : item.ChannelCount;
                    outSampleRate = item.SampleRate;
                    if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd && outSampleRate != 48000)
                        outSampleRate = 48000;
                    break;
            }
            string inputFile = AviSynthGenerator.GenerateAudioScript(item.TempFile, item.Format, item.FormatProfile,
                                                                     item.ChannelCount, outChannels, item.SampleRate,
                                                                     outSampleRate);
            string outFile = Processing.CreateTempFile(item.TempFile, "encoded.ac3");

            string localExecutable = Path.Combine(AppSettings.ToolsPath, use64BitEncoder ? Executable64 : Executable);

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
                                "-",
                                outFile),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardInput = true
                    };
                encoder.StartInfo = encoderParameter;

                _localItem = item;
                _encodingStart = startTime;
                encoder.ErrorDataReceived += Ac3EncodeOnErrorDataReceived;

                Log.InfoFormat("ffmpeg {0:s}", encoderParameter.Arguments);

                bool encStarted;
                bool decStarted;
                try
                {
                    encStarted = encoder.Start();
                }
                catch (Exception ex)
                {
                    encStarted = false;
                    Log.ErrorFormat("ffmpeg exception: {0}", ex);
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

                    Processing.CopyStreamToStream(decoder.StandardOutput.BaseStream, encoder.StandardInput.BaseStream,
                                                  32768,
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

                    encoder.WaitForExit(10000);
                    encoder.CancelErrorRead();
                    decoder.WaitForExit(10000);

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

        /// <summary>
        /// Parses encode output from the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="outputEvent"></param>
        private void Ac3EncodeOnErrorDataReceived(object sender, DataReceivedEventArgs outputEvent)
        {
            string line = outputEvent.Data;
            if (string.IsNullOrEmpty(line)) return;

            Match result = _ac3EncReg.Match(line);
            if (result.Success)
            {
                int progress = -1;
                TimeSpan procTime;
                TimeSpan.TryParseExact(result.Groups[2].Value, @"hh\:mm\:ss\.ff", AppSettings.CInfo, out procTime);

                double secProcessed = procTime.TotalSeconds;

                if (secProcessed > 0f)
                    progress = (int)Math.Round(secProcessed / _localItem.Length * 100, 0);

                TimeSpan eta = DateTime.Now.Subtract(_encodingStart);
                double timeRemaining = _localItem.Length - secProcessed;
                long secRemaining = 0;

                if (eta.Seconds != 0)
                {
                    double speed = Math.Round(secProcessed / eta.TotalSeconds, 2);

                    if (speed > 1)
                        secRemaining = (long)Math.Round(timeRemaining / speed, 0);
                    else
                        secRemaining = 0;
                }

                if (secRemaining < 0)
                    secRemaining = 0;

                TimeSpan remaining = new TimeSpan(0, 0, (int)secRemaining);
                DateTime ticks1 = new DateTime(eta.Ticks);

                string encProgress = string.Format(_ac3EncProgressFmt, ticks1, remaining);

                if (progress < -1)
                    progress = -1;
                _bw.ReportProgress(progress, encProgress);
            }
            else
                Log.InfoFormat("ffmpeg: {0:s}", line);
        }

        /// <summary>
        /// crop detection function, called by BackgroundWorker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GetCrop(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;


            _bw.ReportProgress(-10, _cropDetectStatus);
            _bw.ReportProgress(0);

            string inputFile = AviSynthGenerator.GenerateCropDetect(_jobInfo.VideoStream.TempFile,
                                                                    _jobInfo.VideoStream.FPS,
                                                                    _jobInfo.VideoStream.Length, out _cropDetectFrames);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

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
                encoder.ErrorDataReceived += CropDetectOnErrorDataReceived;

                Log.InfoFormat("ffmpeg {0:s}", encoderParameter.Arguments);

                bool encstarted;
                try
                {
                    encstarted = encoder.Start();
                }
                catch (Exception ex)
                {
                    encstarted = false;
                    Log.ErrorFormat("ffmpeg exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

                if (encstarted)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.BeginErrorReadLine();

                    _bw.ReportProgress(-1, _cropDetectStatus);

                    _jobInfo.VideoStream.CropRect = new Rectangle();

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

        /// <summary>
        /// Parses crop detection log output from the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="outputEvent"></param>
        private void CropDetectOnErrorDataReceived(object sender, DataReceivedEventArgs outputEvent)
        {
            string line = outputEvent.Data;
            if (string.IsNullOrEmpty(line)) return;

            Match cropResult = _cropReg.Match(line);
            Match frameResult = FrameReg.Match(line);
            if (cropResult.Success)
            {
                Point loc = new Point();
                int tempVal;

                Int32.TryParse(cropResult.Groups[3].Value, NumberStyles.Number, AppSettings.CInfo,
                               out tempVal);
                loc.X = tempVal;

                Int32.TryParse(cropResult.Groups[4].Value, NumberStyles.Number, AppSettings.CInfo,
                               out tempVal);
                loc.Y = tempVal;

                _jobInfo.VideoStream.CropRect.Location = loc;

                Int32.TryParse(cropResult.Groups[1].Value, NumberStyles.Number, AppSettings.CInfo,
                               out tempVal);
                _jobInfo.VideoStream.CropRect.Width = tempVal;

                Int32.TryParse(cropResult.Groups[2].Value, NumberStyles.Number, AppSettings.CInfo,
                               out tempVal);
                _jobInfo.VideoStream.CropRect.Height = tempVal;

            }
            else if (frameResult.Success)
            {
                int tempVal;

                Int32.TryParse(frameResult.Groups[1].Value, NumberStyles.Number, AppSettings.CInfo,
                               out tempVal);
                int progress = (int)Math.Round((float)tempVal / _cropDetectFrames * 100, 0);

                _bw.ReportProgress(progress, _cropDetectStatus);
            }
            else
                Log.InfoFormat("ffmpeg: {0:s}", line);
        }

        /// <summary>
        /// Generates decoding process which outputs yuv4mpeg data to stdout
        /// </summary>
        /// <param name="scriptName">Path to input AviSynth script</param>
        /// <param name="useScaling"></param>
        /// <param name="originalSize"></param>
        /// <param name="fromAr"></param>
        /// <param name="cropRect"></param>
        /// <param name="resize"></param>
        /// <returns>Configured process</returns>
        public static Process GenerateDecodeProcess(string scriptName, bool useScaling, Size originalSize, float fromAr, Rectangle cropRect, Size resize)
        {
            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            List<string> filterArray = new List<string>();

            string filterChain = string.Empty;

            if (useScaling)
            {
                if (!cropRect.IsEmpty)
                {
                    int temp;
                    Math.DivRem(cropRect.X, 2, out temp);
                    cropRect.X += temp;
                    Math.DivRem(cropRect.Y, 2, out temp);
                    cropRect.Y += temp;
                    Math.DivRem(cropRect.Width, 2, out temp);
                    cropRect.Width += temp;
                    Math.DivRem(cropRect.Height, 2, out temp);
                    cropRect.Height += temp;

                    if ((cropRect.X > 0) || (cropRect.Y > 0) || (cropRect.Width < originalSize.Width) ||
                        (cropRect.Height < originalSize.Height))
                    {
                        filterArray.Add(string.Format("crop={0:D}:{1:D}:{2:D}:{3:D}", cropRect.Width, cropRect.Height, cropRect.X, cropRect.Y));
                    }
                }
                int calculatedWidth = originalSize.Width;
                int calculatedHeight = originalSize.Height;

                if (!resize.IsEmpty)
                {
                    float toAr = (float)Math.Round(resize.Width / (float)resize.Height, 3);
                    fromAr = (float) Math.Round(fromAr, 3);
                    int temp;
                    if (fromAr > toAr) // source aspectratio higher than target aspectratio
                    {

                        calculatedWidth = resize.Width;
                        calculatedHeight = (int) (calculatedWidth/fromAr);

                        Math.DivRem(calculatedWidth, 2, out temp);
                        calculatedWidth += temp;
                        Math.DivRem(calculatedHeight, 2, out temp);
                        calculatedHeight += temp;
                    }
                    else if (Math.Abs(fromAr - toAr) <= 0)  // source and target aspectratio equals
                    {
                        calculatedWidth = resize.Width;
                        calculatedHeight = (int) (calculatedWidth/toAr);

                        Math.DivRem(calculatedWidth, 2, out temp);
                        calculatedWidth += temp;
                        Math.DivRem(calculatedHeight, 2, out temp);
                        calculatedHeight += temp;
                    }
                    else
                    {
                        calculatedHeight = resize.Height;
                        calculatedWidth = (int) (calculatedHeight/toAr);

                        Math.DivRem(calculatedWidth, 2, out temp);
                        calculatedWidth += temp;
                        Math.DivRem(calculatedHeight, 2, out temp);
                        calculatedHeight += temp;
                    }

                    filterArray.Add(string.Format("scale={0:D}:{1:D}",calculatedWidth, calculatedHeight));
                }

                if (!resize.IsEmpty && (calculatedHeight < resize.Height || calculatedWidth < resize.Width))
                {
                    int posLeft = (int) Math.Ceiling((decimal) (resize.Width - calculatedWidth)/2);
                    int posTop = (int) Math.Ceiling((decimal) (resize.Height - calculatedHeight)/2);
                    filterArray.Add(string.Format("pad={0:D}:{1:D}:{2:D}:{3:D}", resize.Width, resize.Height,
                        posLeft > 0 ? posLeft : 0, posTop > 0 ? posTop : 0));
                }
            }

            if (filterArray.Count > 0)
            {
                filterChain = string.Format("-vf \"{0}\" ", string.Join(",", filterArray));
            }

            ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = localExecutable,
                    Arguments =
                        String.Format(AppSettings.CInfo, "-i \"{0}\" {1} -f yuv4mpegpipe -y \"{2}\"",
                                      scriptName, filterChain, AppSettings.DecodeNamedPipeFullName),
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
            Process ffmpeg = new Process { StartInfo = info };

            ffmpeg.ErrorDataReceived += DecodeOnErrorDataReceived;

            Log.Info("ffmpeg decoding process created!");
            Log.Info("params: ffmpeg " + ffmpeg.StartInfo.Arguments);

            return ffmpeg;
        }

        /// <summary>
        /// Parses decode log output from the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void DecodeOnErrorDataReceived(object sender, DataReceivedEventArgs args)
        {
            string line = args.Data;
            if (string.IsNullOrEmpty(line)) return;

            Match frameResult = FrameReg.Match(line);
            if (!frameResult.Success)
                Log.Info(line);
        }
    }
}
