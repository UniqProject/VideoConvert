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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;
using log4net;

namespace VideoConvert.Core.Encoder
{
    class Lame
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Lame));

        private EncodeInfo _jobInfo;
        private const string Executable = "lame.exe";

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
                    Log.ErrorFormat("lame exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardError.ReadToEnd();
                    Regex regObj = new Regex(@"^LAME.*?version\s*?([\d\.]*?)\s*?\(.*\).*$",
                                             RegexOptions.Singleline | RegexOptions.Multiline);
                    Match result = regObj.Match(output);
                    if (result.Success)
                        verInfo = result.Groups[1].Value.Trim();

                    encoder.WaitForExit(10000);
                    if (!encoder.HasExited)
                        encoder.Kill();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
                Log.DebugFormat("Lame \"{0:s}\" found", verInfo);

            return verInfo;
        }

        public void DoEncode(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("lame_encoding_audio_status");
            string encProgressFmt = Processing.GetResourceString("lame_encoding_audio_progress");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            AudioInfo item = _jobInfo.AudioStreams[_jobInfo.StreamId];

            int outChannels = ((MP3Profile)_jobInfo.AudioProfile).OutputChannels;
            switch (outChannels)
            {
                case 0:
                    outChannels = item.ChannelCount > 2 ? 2 : 0;
                    break;
                case 1:
                    outChannels = 1;
                    break;
            }
            int outSampleRate = ((MP3Profile)_jobInfo.AudioProfile).SampleRate;
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

            int encMode = ((MP3Profile) _jobInfo.AudioProfile).EncodingMode;
            int bitrate = ((MP3Profile) _jobInfo.AudioProfile).Bitrate;
            int quality = ((MP3Profile) _jobInfo.AudioProfile).Quality;
            string preset = ((MP3Profile) _jobInfo.AudioProfile).Preset;

            string inputFile = AviSynthGenerator.GenerateAudioScript(item.TempFile, item.Format, item.FormatProfile,
                                                                     item.ChannelCount, outChannels, item.SampleRate,
                                                                     outSampleRate);
            string outFile = Processing.CreateTempFile(item.TempFile, "encoded.mp3");

            StringBuilder sb = new StringBuilder();

            switch (encMode)
            {
                case 2:
                    sb.AppendFormat(AppSettings.CInfo, "-V {0:0} ", quality);
                    break;
                case 0:
                    sb.AppendFormat("--preset {0:0} ", bitrate);
                    break;
                case 1:
                    sb.AppendFormat("--preset cbr {0:0} ", bitrate);
                    break;
                case 3:
                    sb.AppendFormat("--preset {0} ", preset);
                    break;
            }

            
            sb.Append("- ");
            sb.AppendFormat(AppSettings.CInfo, "\"{0}\" ", outFile);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            Regex pipeObj = new Regex(@"^([\d\,\.]*?)%.*$", RegexOptions.Singleline | RegexOptions.Multiline);

            DateTime startTime = DateTime.Now;

            using (Process encoder = new Process(),
                           decoder = BePipe.GenerateProcess(inputFile))
            {
                ProcessStartInfo encoderParameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation,
                        Arguments =
                            sb.ToString(),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardInput = true
                    };
                encoder.StartInfo = encoderParameter;

                decoder.StartInfo.RedirectStandardError = true;

                DateTime time = startTime;

                decoder.ErrorDataReceived += (o, args) =>
                    {
                        string line = args.Data;
                        if (string.IsNullOrEmpty(line)) return;

                        Match result = pipeObj.Match(line);
                        if (result.Success)
                        {
                            float temp;

                            string tempProgress = result.Groups[1].Value.Replace(",", ".");

                            Single.TryParse(tempProgress, NumberStyles.Number,
                                            AppSettings.CInfo, out temp);
                            int progress = (int) Math.Floor(temp);
                            float progressRemaining = 100 - temp;

                            TimeSpan eta = DateTime.Now.Subtract(time);
                            long secRemaining = 0;
                            if (eta.Seconds != 0)
                            {
                                double speed = Math.Round(temp/eta.TotalSeconds, 6);

                                if (speed > 0)
                                    secRemaining =
                                        (long) Math.Round(progressRemaining/speed, 0);
                                else
                                    secRemaining = 0;
                            }
                            if (secRemaining < 0)
                                secRemaining = 0;

                            TimeSpan remaining = new TimeSpan(0, 0, (int) secRemaining);
                            DateTime ticks1 = new DateTime(eta.Ticks);

                            string encProgress = string.Format(encProgressFmt, ticks1,
                                                               remaining);
                            _bw.ReportProgress(progress, encProgress);
                        }
                        else
                            Log.InfoFormat("bepipe: {0:s}", line);
                    };

                encoder.ErrorDataReceived += (outputSender, outputEvent) =>
                    {
                        string line = outputEvent.Data;

                        if (string.IsNullOrEmpty(line)) return;

                        Log.InfoFormat("lame: {0:s}", line);
                    };

                Log.InfoFormat("lame {0:s}", encoderParameter.Arguments);

                bool encStarted;
                bool decStarted;
                try
                {
                    encStarted = encoder.Start();
                }
                catch (Exception ex)
                {
                    encStarted = false;
                    Log.ErrorFormat("lame exception: {0}", ex);
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
                    decoder.BeginErrorReadLine();

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
                    decoder.CancelErrorRead();

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
    }
}
