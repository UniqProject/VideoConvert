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
using System.Linq;
using System.Text.RegularExpressions;
using VideoConvert.Core.Profiles;
using log4net;
using System.Text;
using System.Threading;

namespace VideoConvert.Core.Encoder
{
    class MkvMerge
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MkvMerge));

        private EncodeInfo _jobInfo;
        private const string Executable = "mkvmerge.exe";
        private const string Defaultparams = "--ui-language en ";

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
                        Arguments = Defaultparams + " -V"
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
                    Log.ErrorFormat("mkvmerge exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardOutput.ReadToEnd();
                    Regex regObj = new Regex(@"^mkvmerge v([\d\.]+ \(.*\)).*built.*$",
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
                Log.DebugFormat("mkvmerge \"{0:s}\" found", verInfo);

            return verInfo;
        }

        public void DoEncode(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            StringBuilder sb = new StringBuilder();
            sb.Append(Defaultparams);

            float fps = _jobInfo.VideoStream.FPS;
            int vidStream;

            string status = _jobInfo.EncodingProfile.OutFormat == OutputType.OutputMatroska
                                ? Processing.GetResourceString("mkvmerge_muxing_status")
                                : Processing.GetResourceString("mkvmerge_muxing_webm_status");
            string chapterLongTime = Processing.GetResourceString("mkvmerge_chapter_format_long_time");
            string chapterLongName = Processing.GetResourceString("mkvmerge_chapter_format_long_name");
            string chapterShortTime = Processing.GetResourceString("mkvmerge_chapter_format_short_time");
            string chapterShortName = Processing.GetResourceString("mkvmerge_chapter_format_short_name");
            string progressFormat = Processing.GetResourceString("mkvmerge_muxing_progress");

            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string tempExt = Path.GetExtension(_jobInfo.VideoStream.TempFile);
            if (_jobInfo.VideoStream.IsRawStream ||
                (_jobInfo.Input == InputType.InputAvi && !_jobInfo.VideoStream.Encoded) ||
                _jobInfo.VideoStream.Encoded)
                vidStream = 0;
            else if (!_jobInfo.VideoStream.Encoded && (tempExt == ".mp4" || tempExt == ".mkv"))
                vidStream = Math.Max(_jobInfo.VideoStream.StreamId - 1, 0);
            else
                vidStream = _jobInfo.VideoStream.StreamId;

            string streamOrder = string.Format(" --track-order 0:{0:g}", vidStream);

            string outFile = !string.IsNullOrEmpty(_jobInfo.TempOutput) ? _jobInfo.TempOutput : _jobInfo.OutputFile;

            sb.AppendFormat(AppSettings.CInfo, "-o \"{0:s}\" ", outFile);

            if (_jobInfo.EncodingProfile.OutFormat == OutputType.OutputWebM)
                sb.Append("--webm ");

            string fpsStr = string.Empty;
            if (_jobInfo.VideoStream.IsRawStream)
            {
                if (_jobInfo.VideoStream.FrameRateEnumerator == 0)
                    fpsStr = string.Format("--default-duration {1:g}:{0:0.000}fps", fps, vidStream);
                else
                    fpsStr = string.Format("--default-duration {0:g}:{1:g}/{2:g}fps",
                                           vidStream,
                                           _jobInfo.VideoStream.FrameRateEnumerator,
                                           _jobInfo.VideoStream.FrameRateDenominator);
            }

            int stereoMode;

            switch (_jobInfo.EncodingProfile.StereoType)
            {
                case StereoEncoding.None:
                    stereoMode = 0;
                    break;
                case StereoEncoding.FullSideBySideLeft:
                case StereoEncoding.HalfSideBySideLeft:
                    stereoMode = 1;
                    break;
                case StereoEncoding.FullSideBySideRight:
                case StereoEncoding.HalfSideBySideRight:
                    stereoMode = 11;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            sb.AppendFormat(AppSettings.CInfo,
                            "--language {1:g}:eng {0:s} --default-track {1:g}:yes --forced-track {1:g}:yes ",
                            fpsStr, vidStream);

            if (stereoMode > 0)
                sb.AppendFormat(AppSettings.CInfo, "--stereo-mode {1:g}:{0:g} ", stereoMode, vidStream);

            sb.AppendFormat(AppSettings.CInfo,
                            "-d {1:g} -A -S --no-global-tags --no-chapters --compression {1:g}:none \"{0:s}\" ",
                            _jobInfo.VideoStream.TempFile, vidStream);

            int i = 1;
            bool defaultAudioExists = false;
            foreach (AudioInfo item in _jobInfo.AudioStreams)
            {
                string isDefault;
                if (item.MkvDefault && !defaultAudioExists)
                {
                    isDefault = "yes";
                    defaultAudioExists = true;
                }
                else
                    isDefault = "no";

                string itemlang = item.LangCode;

                if (itemlang == "xx" || string.IsNullOrEmpty(itemlang))
                    itemlang = "und";

                string delayString = string.Empty;

                if (item.Delay != 0)
                    delayString = string.Format(AppSettings.CInfo, "--sync 0:{0:#}", item.Delay);

                int itemStream = 0;

                if (Path.GetExtension(item.TempFile) == ".mkv")
                    itemStream = 1;

                sb.AppendFormat(AppSettings.CInfo,
                                "--language {0:g}:{1:s} {2:s} --default-track {0:g}:{3:s} --forced-track {0:g}:no -D -a {0:g} ",
                                itemStream,
                                itemlang,
                                delayString,
                                isDefault);
                sb.AppendFormat(AppSettings.CInfo,
                                "-S --no-global-tags --no-chapters --compression {1:g}:none \"{0:s}\" ",
                                item.TempFile, itemStream);

                streamOrder += string.Format(AppSettings.CInfo, ",{0:g}:0", i);
                i++;
            }

            bool defaultSubExists = false;

            if (_jobInfo.EncodingProfile.OutFormat != OutputType.OutputWebM)
            {
                foreach (SubtitleInfo item in _jobInfo.SubtitleStreams.Where(item => !item.HardSubIntoVideo && File.Exists(item.TempFile)))
                {
                    string isDefault;
                    if (item.MkvDefault && !defaultSubExists) 
                    {
                        isDefault = "yes";
                        defaultSubExists = true;
                    }
                    else 
                        isDefault = "no";

                    string itemlang = item.LangCode;

                    int subId;

                    if (itemlang == "xx" || string.IsNullOrEmpty(itemlang))
                        itemlang = "und";

                    string subFile = item.TempFile;

                    if (string.IsNullOrEmpty(subFile))
                    {
                        subFile = _jobInfo.InputFile;
                        subId = item.Id;
                    }
                    else
                        subId = 0;

                    string delayString = string.Empty;

                    if (subFile != _jobInfo.InputFile && (item.Delay != 0 && item.Delay != int.MinValue))
                        delayString = string.Format(AppSettings.CInfo, "--sync {0:g}:{1:g}", subId, item.Delay);

                    sb.AppendFormat(AppSettings.CInfo,
                                    "--language {0:g}:{1:s} {2:s} --default-track {0:g}:{3:s} --forced-track {0:g}:no -s {0:g} ",
                                    subId,
                                    itemlang,
                                    delayString,
                                    isDefault);

                    sb.AppendFormat(AppSettings.CInfo,
                                    "-D -A --no-global-tags --no-chapters --compression {1:g}:none \"{0:s}\" ",
                                    subFile, subId);

                    streamOrder += string.Format(AppSettings.CInfo, ",{0:g}:{1:g}", i, subId);
                    i++;
                }
            }

            string chapterString = string.Empty;

            if (_jobInfo.Chapters.Count > 1 && _jobInfo.EncodingProfile.OutFormat != OutputType.OutputWebM)
            {
                string chapterFile =
                    Processing.CreateTempFile(
                        !string.IsNullOrEmpty(_jobInfo.TempOutput) ? _jobInfo.TempOutput : _jobInfo.OutputFile,
                        "chapters.txt");

                using (StreamWriter chapters = new StreamWriter(chapterFile))
                {

                    string chapterFormatTimes;
                    string chapterFormatNames;

                    if (_jobInfo.Chapters.Count > 100)
                    {
                        chapterFormatTimes = chapterLongTime;
                        chapterFormatNames = chapterLongName;
                    }
                    else
                    {
                        chapterFormatTimes = chapterShortTime;
                        chapterFormatNames = chapterShortName;
                    }

                    DateTime dt;

                    if (_jobInfo.Input != InputType.InputDvd)
                    {
                        for (int j = 0; j < _jobInfo.Chapters.Count; j++)
                        {
                            dt = DateTime.MinValue.Add(_jobInfo.Chapters[j]);
                            chapters.WriteLine(string.Format(chapterFormatTimes, j + 1, dt.ToString("H:mm:ss.fff")));
                            chapters.WriteLine(string.Format(chapterFormatNames, j + 1));
                        }
                    }
                    else
                    {
                        TimeSpan actualTime = new TimeSpan();
                        for (int j = 0; j < _jobInfo.Chapters.Count; j++)
                        {
                            actualTime = actualTime.Add(_jobInfo.Chapters[j]);
                            dt = DateTime.MinValue.Add(actualTime);
                            chapters.WriteLine(string.Format(chapterFormatTimes, j + 1, dt.ToString("H:mm:ss.fff")));
                            chapters.WriteLine(string.Format(chapterFormatNames, j + 1));
                        }
                    }
                }

                chapterString = string.Format(" --chapters \"{0}\"", chapterFile);
                _jobInfo.TempFiles.Add(chapterFile);
            }

            sb.AppendFormat(AppSettings.CInfo, "{0:s} --compression -1:none {1:s}", chapterString, streamOrder);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);
            Regex regObj = new Regex(@"^.?Progress: ([\d]+?)%.*$", RegexOptions.Singleline | RegexOptions.Multiline);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation,
                        Arguments = sb.ToString(),
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
                            int progress;
                            Int32.TryParse(result.Groups[1].Value, NumberStyles.Number, AppSettings.CInfo, out progress);
                            string progressStatus = string.Format(progressFormat,
                                                                  Path.GetFileName(_jobInfo.OutputFile),
                                                                  progress);
                            _bw.ReportProgress(progress, progressStatus);
                        }
                        else
                            Log.InfoFormat("mkvmerge: {0:s}", line);
                    };

                Log.InfoFormat("mkvmerge {0:s}", parameter.Arguments);

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("mkvmerge exception: {0}", ex);
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
                    if (_jobInfo.ExitCode < 2)
                    {
                        if (_jobInfo.ExitCode == 1)
                        {
                            string warningStr = Processing.GetResourceString("process_finish_warnings");
                            _bw.ReportProgress(-10, warningStr);
                            _jobInfo.ExitCode = 0;
                        }

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
    }
}
