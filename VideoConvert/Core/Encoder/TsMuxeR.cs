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
using System.Text;
using System.Drawing;
using System.Threading;
using System.Linq;

namespace VideoConvert.Core.Encoder
{
    class TsMuxeR
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TsMuxeR));

        private EncodeInfo _jobInfo;
        private const string Executable = "tsmuxer.exe";
        private readonly string _defaultparams = string.Empty;

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
                        RedirectStandardOutput = true,
                        Arguments = _defaultparams + " -V"
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
                    Log.ErrorFormat("tsmuxer exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    Regex regObj = new Regex(@"^.*?SmartLabs tsMuxeR\.  Version ([\d\.]+?) .*$",
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
                Log.DebugFormat("tsMuxeR \"{0:s}\" found", verInfo);

            return verInfo;
        }

        public void DoEncode(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("tsmuxer_muxing_status");
            string progressFormat = Processing.GetResourceString("tsmuxer_muxing_progress");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            Regex regObj = new Regex(@"^.*?([\d\.]+?)% complete.*$", RegexOptions.Singleline | RegexOptions.Multiline);
            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation
                    };

                string metaFile = GenerateCommandLine();

                _jobInfo.TempFiles.Add(metaFile);

                string outFile = !string.IsNullOrEmpty(_jobInfo.TempOutput) ? _jobInfo.TempOutput : _jobInfo.OutputFile;

                parameter.Arguments = string.Format(AppSettings.CInfo, "\"{0:s}\" \"{1:s}\"", metaFile, outFile);
                parameter.CreateNoWindow = true;
                parameter.UseShellExecute = false;
                parameter.RedirectStandardOutput = true;

                encoder.StartInfo = parameter;

                encoder.OutputDataReceived += (outputSender, outputEvent) =>
                    {
                        string line = outputEvent.Data;
                        if (string.IsNullOrEmpty(line)) return;

                        Match result = regObj.Match(line);
                        if (result.Success)
                        {
                            float tempProgress;
                            Single.TryParse(result.Groups[1].Value, NumberStyles.Number, AppSettings.CInfo,
                                            out tempProgress);

                            int progress = Convert.ToInt32(Math.Ceiling(tempProgress));

                            string progressStr = string.Format(progressFormat,
                                                               _jobInfo.OutputFile,
                                                               progress);
                            _bw.ReportProgress(progress, progressStr);

                        }
                        else
                        {
                            Log.InfoFormat("tsMuxeR: {0:s}", line);
                        }
                    };

                Log.InfoFormat("tsMuxeR: {0:s}", parameter.Arguments);

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("tsmuxer exception: {0}", ex);
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
                    if (_jobInfo.ExitCode == 0)
                    {
                        _jobInfo.TempFiles.Add(_jobInfo.VideoStream.TempFile);
                        foreach (AudioInfo item in _jobInfo.AudioStreams)
                            _jobInfo.TempFiles.Add(item.TempFile);
                        foreach (SubtitleInfo item in _jobInfo.SubtitleStreams)
                            _jobInfo.TempFiles.Add(item.TempFile);
                    }
                }
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;

            e.Result = _jobInfo;
        }

        private string GenerateCommandLine()
        {
            string videoNotSupported = Processing.GetResourceString("tsmuxer_videoformat_not_supported");
            string audioNotSupported = Processing.GetResourceString("tsmuxer_audioformat_not_supported");
            string subtitleNotSupported = Processing.GetResourceString("tsmuxer_subtitleformat_not_supported");

            int vidStream = 0;
            StringBuilder meta = new StringBuilder();
            string codec;

            Log.InfoFormat("Job {0}", _jobInfo);

            meta.Append("MUXOPT --no-pcr-on-video-pid ");

            if (AppSettings.TSMuxeRBlurayAudioPES)
                meta.Append("--new-audio-pes ");

            meta.Append("--vbr ");

            switch (_jobInfo.EncodingProfile.OutFormat)
            {
                case OutputType.OutputM2Ts:
                case OutputType.OutputTs:
                    break;
                case OutputType.OutputBluRay:
                    meta.Append("--blu-ray ");
                    break;
                case OutputType.OutputAvchd:
                    meta.Append("--avchd ");
                    break;
            }

            if ((_jobInfo.EncodingProfile.OutFormat == OutputType.OutputBluRay) ||
                (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputAvchd))
            {
                Size targetSize = Processing.GetVideoDimensions(_jobInfo.VideoStream.PicSize,
                                                                _jobInfo.VideoStream.AspectRatio,
                                                                _jobInfo.EncodingProfile.OutFormat);
                if (_jobInfo.VideoStream.Width < targetSize.Width || _jobInfo.VideoStream.Height < targetSize.Height)
                    meta.Append("--insertBlankPL ");
            }

            if (_jobInfo.Chapters.Count > 1)
            {
                meta.Append("--custom-chapters=");

                DateTime dt;
                if (_jobInfo.Input != InputType.InputDvd)
                {
                    for (int j = 0; j < _jobInfo.Chapters.Count; j++)
                    {
                        dt = DateTime.MinValue.Add(_jobInfo.Chapters[j]);
                        meta.Append(dt.ToString("H:mm:ss.fff"));

                        if (j < _jobInfo.Chapters.Count - 1)
                            meta.Append(";");
                    }
                }
                else
                {
                    TimeSpan actualTime = new TimeSpan();
                    for (int j = 0; j < _jobInfo.Chapters.Count; j++)
                    {
                        actualTime = actualTime.Add(_jobInfo.Chapters[j]);
                        dt = DateTime.MinValue.Add(actualTime);
                        meta.Append(dt.ToString("H:mm:ss.fff"));

                        if (j < _jobInfo.Chapters.Count - 1)
                            meta.Append(";");
                    }
                }
                meta.Append(" ");
            } // chapters count > 1

            meta.AppendLine("--vbv-len=500");

            string sourceVidCodec = _jobInfo.VideoStream.Format;
            switch (_jobInfo.Input)
            {
                case InputType.InputAvi:
                case InputType.InputMp4:
                case InputType.InputMatroska:
                case InputType.InputTs:
                case InputType.InputWm:
                case InputType.InputFlash:
                    vidStream = _jobInfo.VideoStream.StreamId;
                    break;

                case InputType.InputDvd:
                    vidStream = 1;
                    break;
                case InputType.InputBluRay:
                case InputType.InputAvchd:
                case InputType.InputHddvd:
                    vidStream = _jobInfo.VideoStream.IsRawStream ? 0 : _jobInfo.VideoStream.StreamId;
                    break;
            }

            switch (sourceVidCodec)
            {
                case "VC-1":
                    codec = "V_MS/VFW/WVC1";
                    break;
                case "AVC":
                    codec = "V_MPEG4/ISO/AVC";
                    break;
                case "MPEG Video":
                case "MPEG-2":
                    if (_jobInfo.VideoStream.FormatProfile == "Version 2")
                    {
                        codec = "V_MPEG-2";
                    }
                    else
                    {
                        codec = string.Empty;
                        _bw.ReportProgress(-10, videoNotSupported);
                    }
                    break;
                default:
                    codec = string.Empty;
                    _bw.ReportProgress(-10, videoNotSupported);
                    break;
            }

            string inFile = string.Format("\"{0}\"", _jobInfo.VideoStream.TempFile);

            if (!string.IsNullOrEmpty(codec))
            {
                if (codec != "V_MPEG-2")
                {
                    meta.AppendFormat(AppSettings.CInfo, "{0:s}, {1:s}, fps={2:#.###}, insertSEI, contSPS, ", codec,
                                      inFile, _jobInfo.VideoStream.FPS);

                    meta.AppendFormat(AppSettings.CInfo, "track={0:g}, lang={1:s}", vidStream, "und");
                }
                else
                {
                    meta.AppendFormat(AppSettings.CInfo, "{0:s}, {1:s}, fps={2:#.###}, track={3:g}, lang={4:s}", codec,
                                      inFile, _jobInfo.VideoStream.FPS, vidStream, "und");
                }
                meta.AppendLine();
            }

            foreach (AudioInfo item in _jobInfo.AudioStreams)
            {
                string itemlang = item.LangCode;
                if ((itemlang == "xx") || (string.IsNullOrEmpty(itemlang)))
                    itemlang = "und";

                switch (item.Format.ToLower())
                {
                    case "pcm":
                        codec = "A_LPCM";
                        break;
                    case "ac3":
                    case "ac-3":
                    case "eac3":
                    case "eac-3":
                    case "e-ac-3":
                    case "e-ac3":
                    case "ac3-ex":
                    case "truehd":
                    case "true-hd":
                    case "true hd":
                        codec = "A_AC3";
                        break;
                    case "dts":
                    case "dts-hd":
                    case "dts-hd hr":
                    case "dts-hd ma":
                        codec = "A_DTS";
                        break;
                    case "mpeg audio":
                        codec = "A_MP3";
                        break;
                    default:
                        _bw.ReportProgress(-10, audioNotSupported);
                        continue;
                }

                string delayString = string.Empty;

                if (item.Delay != 0)
                    delayString = string.Format(AppSettings.CInfo, "timeshift={0:#}ms,", item.Delay);

                inFile = item.TempFile;

                meta.AppendFormat(AppSettings.CInfo, "{0:s}, {1:s}, {2:s} track=1, lang={3:s}", codec, inFile,
                                  delayString, itemlang);
                meta.AppendLine();
            }

            foreach (SubtitleInfo item in _jobInfo.SubtitleStreams.Where(item => !item.HardSubIntoVideo && File.Exists(item.TempFile)))
            {
                string itemlang = item.LangCode;
                if ((itemlang == "xx") || (string.IsNullOrEmpty(itemlang)))
                    itemlang = "und";

                switch (item.Format.ToLower())
                {
                    case "pgs":
                        codec = "S_HDMV/PGS";
                        break;
                    case "utf-8":
                        codec = "S_TEXT/UTF8";
                        break;

                    default:
                        _bw.ReportProgress(-10, subtitleNotSupported);
                        continue;
                }

                string tempFile = string.Empty;
                int subId = -1;

                if (!string.IsNullOrEmpty(item.TempFile))
                {
                    tempFile = string.Format("\"{0}\"", item.TempFile);
                    subId = 1;
                }

                string delayString = string.Empty;
                if (item.Delay != int.MinValue)
                    delayString = string.Format(AppSettings.CInfo, "timeshift={0:#}ms,", item.Delay);

                if (codec == "S_TEXT/UTF8")
                    meta.AppendFormat(AppSettings.CInfo,
                                      "{0:s}, {1:s},{2:s}font-name=\"{3:s}\",font-size={4:#},font-color={5:s},bottom-offset={6:g}," +
                                      "font-border={7:g},text-align=center,video-width={8:g},video-height={9:g},fps={10:#.###}, track={11:g}, lang={12:s}",
                                      codec,
                                      tempFile,
                                      delayString,
                                      AppSettings.TSMuxeRSubtitleFont.Source,
                                      AppSettings.TSMuxeRSubtitleFontSize,
                                      string.Format("0x00{0:x}{1:x}{2:x}",
                                                    AppSettings.TSMuxeRSubtitleColor.R,
                                                    AppSettings.TSMuxeRSubtitleColor.G,
                                                    AppSettings.TSMuxeRSubtitleColor.B),
                                      AppSettings.TSMuxeRBottomOffset,
                                      AppSettings.TSMuxerSubtitleAdditionalBorder,
                                      _jobInfo.VideoStream.Width,
                                      _jobInfo.VideoStream.Height,
                                      _jobInfo.VideoStream.FPS,
                                      subId,
                                      itemlang);
                else
                    meta.AppendFormat(AppSettings.CInfo,
                                      "{0:s}, {1:s},{2:s}bottom-offset={3:g},font-border={4:g},text-align=center,video-width={5:g}," +
                                      "video-height={6:g},fps={7:#.###}, track={8:g}, lang={9:s}",
                                      codec,
                                      tempFile,
                                      delayString,
                                      AppSettings.TSMuxeRBottomOffset,
                                      AppSettings.TSMuxerSubtitleAdditionalBorder,
                                      _jobInfo.VideoStream.Width,
                                      _jobInfo.VideoStream.Height,
                                      _jobInfo.VideoStream.FPS,
                                      subId,
                                      itemlang);

                meta.AppendLine();
            }

            string metaFile = Processing.CreateTempFile("meta");
            using (StreamWriter sw = new StreamWriter(metaFile))
                sw.WriteLine(meta.ToString());

            Log.InfoFormat("tsMuxeR Meta: \r\n{0:s}", meta);

            return metaFile;
        }
    }
}
