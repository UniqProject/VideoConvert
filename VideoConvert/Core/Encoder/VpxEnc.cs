using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using VideoConvert.Core.CommandLine;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;
using log4net;

namespace VideoConvert.Core.Encoder
{
    class VpxEnc
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VpxEnc));

        private EncodeInfo _jobInfo;
        private const string Executable = "vpxenc.exe";

        long _frameCount;
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
                    Log.ErrorFormat("vpxenc exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardError.ReadToEnd();
                    Regex regObj = new Regex(@".*VP8 Encoder (v[-.\w\d]*)", RegexOptions.Singleline | RegexOptions.Multiline);
                    Match result = regObj.Match(output);
                    if (result.Success)
                    {
                        verInfo = result.Groups[1].Value.Trim();
                    }

                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("VpxEnc \"{0:s}\" found", verInfo);
            }

            return verInfo;
        }

        public void DoEncode(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string passStr = Processing.GetResourceString("vp8_pass");
            string status = Processing.GetResourceString("vp8_encoding_status");
            string progressFormat = Processing.GetResourceString("vp8_encoding_progress");

            //progress vars
            DateTime startTime = DateTime.Now;
            TimeSpan remaining = new TimeSpan(0, 0, 0);
            // end progress

            VP8Profile encProfile = (VP8Profile)_jobInfo.VideoProfile;

            if (!_jobInfo.EncodingProfile.Deinterlace && _jobInfo.VideoStream.Interlaced)
                _jobInfo.VideoStream.Interlaced = false;

            Size resizeTo = VideoHelper.GetTargetSize(_jobInfo);

            if (string.IsNullOrEmpty(_jobInfo.AviSynthScript))
                GenerateAviSynthScript(resizeTo);

            string inputFile = _jobInfo.AviSynthScript;
            string outFile =
                Processing.CreateTempFile(
                    string.IsNullOrEmpty(_jobInfo.TempOutput) ? _jobInfo.JobName : _jobInfo.TempOutput, "encoded.webm");

            _frameCount = _jobInfo.VideoStream.FrameCount;

            int targetBitrate = 0;
            if (_jobInfo.EncodingProfile.TargetFileSize > 0)
            {
                targetBitrate = Processing.CalculateVideoBitrate(_jobInfo);
            }

            int encodeMode = encProfile.EncodingMode;
            string pass = string.Empty;
            if (encodeMode == 1)
            {
                pass = string.Format(" {1} {0:0}; ", _jobInfo.StreamId, passStr);
            }

            _bw.ReportProgress(-10, status + pass.Replace("; ", string.Empty));
            _bw.ReportProgress(0, status);

            string argument = VP8CommandLineGenerator.Generate(encProfile,
                                                                targetBitrate,
                                                                resizeTo.Width,
                                                                resizeTo.Height,
                                                                _jobInfo.StreamId,
                                                                _jobInfo.VideoStream.FrameRateEnumerator,
                                                                _jobInfo.VideoStream.FrameRateDenominator,
                                                                false,
                                                                _jobInfo.VideoStream.PicSize,
                                                                outFile);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            Regex frameInformation = new Regex(@"^.*Pass\s\d\/\d frame \s*\d*\/(\d*).*$",
                                               RegexOptions.Singleline | RegexOptions.Multiline);

            using (Process encoder = new Process(),
                           decoder = FfMpeg.GenerateDecodeProcess(inputFile))
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                                                 {
                                                     WorkingDirectory = AppSettings.DemuxLocation,
                                                     Arguments = argument,
                                                     CreateNoWindow = true,
                                                     UseShellExecute = false,
                                                     RedirectStandardError = true,
                                                     RedirectStandardInput = true
                                                 };
                encoder.StartInfo = parameter;

                encoder.ErrorDataReceived += (outputSender, outputEvent) =>
                                                 {
                                                     string line = outputEvent.Data;

                                                     if (string.IsNullOrEmpty(line)) return;

                                                     Match result = frameInformation.Match(line);

                                                     // ReSharper disable AccessToModifiedClosure
                                                     TimeSpan eta = DateTime.Now.Subtract(startTime);
                                                     // ReSharper restore AccessToModifiedClosure
                                                     long secRemaining = 0;

                                                     if (result.Success)
                                                     {
                                                         long current;
                                                         Int64.TryParse(result.Groups[1].Value, NumberStyles.Number,
                                                                        AppSettings.CInfo, out current);
                                                         long framesRemaining = _frameCount - current;
                                                         float fps = 0f;
                                                         if (eta.Seconds != 0)
                                                         {
                                                             //Frames per Second
                                                             double codingFPS = Math.Round(current/eta.TotalSeconds, 2);

                                                             if (codingFPS > 1)
                                                             {
                                                                 secRemaining = framesRemaining/(int) codingFPS;
                                                                 fps = (float) codingFPS;
                                                             }
                                                             else
                                                                 secRemaining = 0;
                                                         }

                                                         if (secRemaining > 0)
                                                             remaining = new TimeSpan(0, 0, (int) secRemaining);

                                                         DateTime ticks = new DateTime(eta.Ticks);

                                                         string progress = string.Format(progressFormat,
                                                                                         current, _frameCount,
                                                                                         fps,
                                                                                         remaining, ticks, pass);
                                                         _bw.ReportProgress((int) (((float) current/_frameCount)*100),
                                                                            progress);
                                                     }
                                                     else
                                                     {
                                                         Log.InfoFormat("vpxenc: {0:s}", line);
                                                     }
                                                 };

                Log.InfoFormat("start parameter: vpxenc {0:s}", argument);

                bool started;
                bool decStarted;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("vpxenc exception: {0}", ex);
                    _jobInfo.ExitCode = -1;
                }

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

                startTime = DateTime.Now;

                if (started && decStarted)
                {
                    encoder.PriorityClass = AppSettings.GetProcessPriority();
                    encoder.BeginErrorReadLine();
                    decoder.PriorityClass = AppSettings.GetProcessPriority();
                    decoder.BeginErrorReadLine();

                    Processing.CopyStreamToStream(decoder.StandardOutput.BaseStream, encoder.StandardInput.BaseStream, 1024 * 1024,
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
                    decoder.CancelErrorRead();

                    _jobInfo.ExitCode = encoder.ExitCode;
                    Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                }
            }

            if (_jobInfo.ExitCode == 0)
            {
                if ((encProfile.EncodingMode == 1 && _jobInfo.StreamId == 2) ||
                    encProfile.EncodingMode == 0)
                {
                    _jobInfo.VideoStream.Encoded = true;
                    _jobInfo.VideoStream.IsRawStream = false;

                    _jobInfo.TempFiles.Add(_jobInfo.VideoStream.TempFile);
                    _jobInfo.VideoStream.TempFile = outFile;
                    _jobInfo.MediaInfo = Processing.GetMediaInfo(_jobInfo.VideoStream.TempFile);
                    _jobInfo.VideoStream = VideoHelper.GetStreamInfo(_jobInfo.VideoStream);

                    string statsFile = Processing.CreateTempFile(outFile, "stats");
                    _jobInfo.TempFiles.Add(statsFile);
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