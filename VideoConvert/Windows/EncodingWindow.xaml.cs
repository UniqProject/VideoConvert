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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using System.Windows.Threading;
using VideoConvert.Core;
using VideoConvert.Core.Encoder;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;
using log4net;

namespace VideoConvert.Windows
{
    /// <summary>
    /// Interaktionslogik für EncodingWindow.xaml
    /// </summary>
    public partial class EncodingWindow
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EncodingWindow));

        public ObservableCollection<EncodeInfo> JobList;
        private int _actualJob;
        private int _actualProcessStep;

        private BackgroundWorker _worker;
        private bool _cancel;
        private bool _appExits;

        public TaskbarItemInfo TaskBar;
        DispatcherTimer _startTimer;

        private readonly ObservableCollection<LogEntry> _logEntries = new ObservableCollection<LogEntry>();

        public ObservableCollection<LogEntry> LogEntries { get { return _logEntries; } }

        private DateTime _startTime = DateTime.MinValue;
        private string status = Processing.GetResourceString("total_progress_time");
        private DateTime _updateTime = DateTime.MinValue;

        public EncodingWindow()
        {
            InitializeComponent();
            Application.Current.Exit += CurrentExit;
            _logEntries.CollectionChanged += (sender, args) =>
                {
                    try
                    {
                        LogView.ScrollIntoView(_logEntries[_logEntries.Count - 1]);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Encoding Log error", ex);
                    }

                };
        }

        void CurrentExit(object sender, ExitEventArgs e)
        {
            if (_worker == null) return;

            if (!_worker.IsBusy) return;

            _worker.CancelAsync();
            _cancel = true;
            _appExits = true;
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            TaskBar.ProgressState = TaskbarItemProgressState.Normal;
            _startTimer = new DispatcherTimer();
            _startTimer.Tick += StartTimerTick;
            _startTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _startTimer.Start();
        }

        void StartTimerTick(object sender, EventArgs e)
        {
            ((DispatcherTimer) sender).Stop();
            _startTime = DateTime.Now;

            foreach (EncodeInfo job in JobList)
            {
                job.CompletedStep = EncodingStep.NotSet;
                job.NextStep = EncodingStep.NotSet;
            }

            int encodingSteps = GetTotalEncodingSteps();
            TotalProgress.Maximum = encodingSteps;

            _actualJob = 0;

            TaskBar.ProgressState = TaskbarItemProgressState.Normal;
            StartEncode();
        }

        void EncodeProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage >= 0)
            {
                ActualProgress.IsIndeterminate = false;
                ActualProgress.Value = e.ProgressPercentage <= 100 ? e.ProgressPercentage : 100D;

                TotalProgress.Value = _actualProcessStep + (ActualProgress.Value / 100D);
                TaskBar.ProgressValue = TotalProgress.Value / TotalProgress.Maximum;

                DateTime now = DateTime.Now;

                if (_updateTime.AddSeconds(1).CompareTo(now) <= 0)
                {
                    DateTime processingTime = DateTime.MinValue.Add(DateTime.Now.Subtract(_startTime));

                    double progressTime = 0;
                    if (TotalProgress.Value > 0)
                        progressTime = processingTime.TimeOfDay.TotalSeconds/TotalProgress.Value;
                    

                    double progressRemain = TotalProgress.Maximum - TotalProgress.Value;

                    if (progressRemain < 0)
                        progressRemain = 0;

                    DateTime remainTime = DateTime.MinValue.Add(TimeSpan.FromSeconds(progressTime*progressRemain));
                    TotalTime.Content = string.Format(status, processingTime, remainTime);

                    _updateTime = now;
                }
            }
            else if (e.ProgressPercentage >= -1)
                ActualProgress.IsIndeterminate = true;
            else
            {
                LogEntry entry = new LogEntry
                    {
                        EntryTime = DateTime.Now,
                        JobName = JobList[_actualJob].JobName,
                        LogText = (string) e.UserState
                    };

                _logEntries.Add(entry);
            }
            if (e.ProgressPercentage >= -1)
                JobStatus.Content = e.UserState;
        }

        void EncodeCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((JobList[_actualJob].NextStep != EncodingStep.MoveOutFile) &&
                (JobList[_actualJob].NextStep != EncodingStep.CopyTempFile))
                JobList[_actualJob] = (EncodeInfo) e.Result;

            _actualProcessStep++;

            EncodeInfo job = JobList[_actualJob];
            DeleteTempFiles(ref job);

            if (!_cancel)
            {
                if (JobList[_actualJob].ExitCode != 0)
                    _actualJob++;
                StartEncode();
            }
            else
            {
                if (_worker != null)
                    _worker.Dispose();
                AbortBtn.IsEnabled = true;
                CloseWindow();
            }
        }

        private void StartEncode()
        {
            if (_actualJob < JobList.Count)
            {
                EncodeInfo job = JobList[_actualJob];

                DetermineNextStep(ref job);
                _worker = new BackgroundWorker();

                switch (job.NextStep)
                {
                    case EncodingStep.CopyTempFile:
                        DoCopyTempFile(job);
                        break;
                    case EncodingStep.Dump:
                        DoDump(job);
                        break;
                    case EncodingStep.Demux:
                        DoDemux(job);
                        break;
                    case EncodingStep.EncodeAudio:
                        DoEncodeAudio(job);
                        break;
                    case EncodingStep.ProcessSubtitle:
                        DoProcessSubtitle(job);
                        break;
                    case EncodingStep.IndexVideo:
                        DoIndexVideo(job);    
                        break;
                    case EncodingStep.GetCropRect:
                        GetCropRect(job);
                        break;
                    case EncodingStep.EncodeVideo:
                        DoEncodeVideo(job);
                        break;
                    case EncodingStep.PreMuxResult:
                        DoPreMuxResult(job);
                        break;
                    case EncodingStep.PremuxSubtitle:
                        DoPremuxSubtitle(job);
                        break;
                    case EncodingStep.MuxResult:
                        DoMuxResult(job);
                        break;
                    case EncodingStep.MoveOutFile:
                        DoMoveOutFile(job);
                        break;
                    case EncodingStep.Done:
                        job.TempFiles.Add(job.TempInput);
                        job.TempFiles.Add(job.TempOutput);
                        DeleteTempFiles(ref job);

                        _actualJob++;
                        break;
                }
                if ((job.NextStep == EncodingStep.Done) &&
                    (_actualJob >= JobList.Count))
                {
                    Log.Info("Job processing done");

                    CloseWindow();
                }
                else if (job.NextStep != EncodingStep.Done)
                    try
                    {
                        Log.Info("Run background worker");
                        _worker.WorkerReportsProgress = true;
                        _worker.WorkerSupportsCancellation = true;

                        _worker.ProgressChanged += EncodeProgressChanged;
                        _worker.RunWorkerCompleted += EncodeCompleted;
                        _worker.RunWorkerAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                else
                    StartEncode();
            } // ActualJob < JobList.Count
            else
            {
                Log.Info("Job processing done");
                CloseWindow();
            }

        }

        private void CloseWindow()
        {
            if (ShutDownAfterFinish.IsChecked == true && !_appExits)
            {
                Log.Info("Closing Encoding dialog and shutting down windows");
                System.Diagnostics.Process.Start("shutdown", "/s /t 10");
            }
            else
                Log.Info("Closing Encoding dialog");

            if (AppSettings.DeleteCompletedJobs)
            {
                Log.Info("Remove Completed Jobs");
                List<EncodeInfo> completed = JobList.Where(encodeInfo => encodeInfo.NextStep == EncodingStep.Done).ToList();

                foreach (EncodeInfo encodeInfo in completed)
                    JobList.Remove(encodeInfo);
            }

            if (Parent != null) ((Grid) Parent).Children.Remove(this);

            TaskBar.ProgressState = TaskbarItemProgressState.None;
        }

        private static void DeleteTempFiles(ref EncodeInfo job)
        {
            if (!AppSettings.DeleteTemporaryFiles) return;

            foreach (string tempFile in job.TempFiles)
            {
                if (tempFile == job.InputFile || tempFile == job.OutputFile || string.IsNullOrEmpty(tempFile))
                    continue;

                FileAttributes fi = new FileInfo(tempFile).Attributes;

                try
                {
                    Log.InfoFormat("Deleting \"{0:s}\"", tempFile);
                    if (fi == FileAttributes.Directory)
                        Directory.Delete(tempFile);
                    else
                        File.Delete(tempFile);
                }
                catch (Exception exc)
                {
                    Log.ErrorFormat("Could not delete File \"{0:s}\" -> {1:s}",
                                    tempFile, exc.Message);
                }
            }
            job.TempFiles.Clear();
        }

        private void DoMoveOutFile(EncodeInfo job)
        {
            FileWorker fw = new FileWorker();
            fw.SetFiles(job.TempOutput, job.OutputFile);
            _worker.DoWork += fw.MoveFile;
            Log.Info("FileWorker.MoveFile()");
        }

        private void DoMuxResult(EncodeInfo job)
        {
            switch (job.EncodingProfile.OutFormat)
            {
                case OutputType.OutputTs:
                case OutputType.OutputM2Ts:
                case OutputType.OutputAvchd:
                case OutputType.OutputBluRay:
                    TsMuxeR tsmuxer = new TsMuxeR();
                    tsmuxer.SetJob(job);
                    _worker.DoWork += tsmuxer.DoEncode;
                    Log.Info("TSMuxer.DoEncode()");
                    break;
                case OutputType.OutputMatroska:
                case OutputType.OutputWebM:
                    MkvMerge mkvmerge = new MkvMerge();
                    mkvmerge.SetJob(job);
                    _worker.DoWork += mkvmerge.DoEncode;
                    Log.Info("MKVmergeEncoder.DoEncode()");
                    break;
                case OutputType.OutputDvd:
                    DvdAuthor dvdauthor = new DvdAuthor();
                    dvdauthor.SetJob(job);
                    _worker.DoWork += dvdauthor.DoEncode;
                    Log.Info("DVDAuthor.DoEncode()");
                    break;
                case OutputType.OutputMp4:
                    MP4Box box = new MP4Box();
                    box.SetJob(job);
                    _worker.DoWork += box.DoEncode;
                    Log.Info("MP4Box.DoEncode()");
                    break;
            }
        }

        private void DoPremuxSubtitle(EncodeInfo job)
        {
            SpuMux subMux = new SpuMux();
            subMux.SetJob(job);
            _worker.DoWork += subMux.Process;
            Log.Info("SpuMux.Process()");
        }

        private void DoPreMuxResult(EncodeInfo job)
        {
            MJpeg mplex = new MJpeg();
            mplex.SetJob(job);
            _worker.DoWork += mplex.DoEncode;
            Log.Info("mjpeg.DoEncode()");
        }

        private void DoEncodeVideo(EncodeInfo job)
        {
            switch (job.VideoProfile.Type)
            {
                case ProfileType.X264:
                    {
                        X264 x264Enc = new X264();
                        x264Enc.SetJob(job);
                        _worker.DoWork += x264Enc.DoEncode;
                        Log.Info("x264Encoder.DoEncode()");
                    }
                    break;
                case ProfileType.HcEnc:
                    {
                        HcEnc hcEnc = new HcEnc();
                        hcEnc.SetJob(job);
                        _worker.DoWork += hcEnc.DoEncodeDvd;
                        Log.Info("HCEnc.DoEncodeDVD()");
                    }
                    break;
                case ProfileType.VP8:
                    {
                        VpxEnc vpxEnc = new VpxEnc();
                        vpxEnc.SetJob(job);
                        _worker.DoWork += vpxEnc.DoEncode;
                        Log.Info("VpxEnc.DoEncode()");
                    }
                    break;
            }
        }

        private void GetCropRect(EncodeInfo job)
        {
            FfMpeg ffmpeg = new FfMpeg();
            ffmpeg.SetJob(job);
            _worker.DoWork += ffmpeg.GetCrop;
            Log.Info("ffmpegEncoder.GetCropRect()");
        }

        private void DoIndexVideo(EncodeInfo job)
        {
            FfmsIndex fIndex = new FfmsIndex();
            fIndex.SetJob(job);
            _worker.DoWork += fIndex.DoIndex;
            Log.Info("ffindex.DoIndex()");
        }

        private void DoProcessSubtitle(EncodeInfo job)
        {
            BdSup2SubTool subtool = new BdSup2SubTool();
            subtool.SetJob(job);
            _worker.DoWork += subtool.DoProcess;
            Log.Info("BDSup2SubTool.DoProcess()");
        }

        private void DoEncodeAudio(EncodeInfo job)
        {
            FfMpeg ffmpeg = new FfMpeg();
            OggEnc oggEnc = new OggEnc();
            Lame lame = new Lame();
            NeroAACEnc aacEnc = new NeroAACEnc();
            switch (job.AudioProfile.Type)
            {
                case ProfileType.AC3:
                    ffmpeg.SetJob(job);
                    _worker.DoWork += ffmpeg.DoEncodeAc3;
                    Log.Info("ffmpeg.DoEncodeAC3()");
                    break;
                case ProfileType.OGG:
                    oggEnc.SetJob(job);
                    _worker.DoWork += oggEnc.DoEncode;
                    Log.Info("oggenc.DoEncode()");
                    break;
                case ProfileType.AAC:
                    aacEnc.SetJob(job);
                    _worker.DoWork += aacEnc.DoEncode;
                    Log.Info("NeroAacEnc.DoEncode()");
                    break;
                case ProfileType.MP3:
                    lame.SetJob(job);
                    _worker.DoWork += lame.DoEncode;
                    Log.Info("lame.DoEncode()");
                    break;
                case ProfileType.Copy:
                    if (job.EncodingProfile.OutFormat == OutputType.OutputDvd &&
                        !Processing.CheckAudioDvdCompatible(job.AudioStreams[job.StreamId]))
                    {
                        ffmpeg.SetJob(job);
                        _worker.DoWork += ffmpeg.DoEncodeAc3;
                        Log.Info("ffmpeg.DoEncodeAC3()");
                    }
                    break;
            }
        }

        private void DoDemux(EncodeInfo job)
        {
            Eac3To eac3Toenc = new Eac3To();
            FfMpeg ffmpeg = new FfMpeg();
            switch (job.Input)
            {
                case InputType.InputBluRay:
                case InputType.InputAvchd:
                case InputType.InputHddvd:
                case InputType.InputDvd:
                    eac3Toenc.SetJob(job);
                    _worker.DoWork += eac3Toenc.DoDemux;
                    Log.Info("eac3toEncoder.DoDemux()");
                    break;
                case InputType.InputAvi:
                case InputType.InputFlash:
                case InputType.InputMp4:
                case InputType.InputWm:
                case InputType.InputMatroska:
                case InputType.InputMpegps:
                case InputType.InputTs:
                case InputType.InputOgg:
                case InputType.InputWebM:
                    ffmpeg.SetJob(job);
                    _worker.DoWork += ffmpeg.DoDemux;
                    Log.Info("ffmpegEncoder.DoDemux()");
                    break;
            }
        }

        private void DoDump(EncodeInfo job)
        {
            switch (job.Input)
            {
                case InputType.InputDvd:
                    MPlayer mplayer = new MPlayer();
                    mplayer.SetJob(job);
                    _worker.DoWork += mplayer.DoDump;
                    Log.Info("MPlayer.DoDump()");
                    break;
            }
        }

        private void DoCopyTempFile(EncodeInfo job)
        {
            FileWorker fw = new FileWorker();
            fw.SetFiles(job.InputFile, job.TempInput);
            _worker.DoWork += fw.CopyFile;
            Log.Info("FileWorker.CopyFile()");
        }

        private static void DetermineNextStep(ref EncodeInfo job)
        {
            switch (job.NextStep)
            {
                case EncodingStep.NotSet:
                    switch (job.Input)
                    {
                        case InputType.InputAvi:
                        case InputType.InputMp4:
                        case InputType.InputMatroska:
                        case InputType.InputTs:
                        case InputType.InputWm:
                        case InputType.InputFlash:
                        case InputType.InputMpegps:
                        case InputType.InputWebM:
                        case InputType.InputOgg:
                            job.NextStep = !string.IsNullOrEmpty(job.TempInput)
                                               ? EncodingStep.CopyTempFile
                                               : EncodingStep.Demux;
                            break;
                        case InputType.InputDvd:
                            job.NextStep = EncodingStep.Dump;
                            break;
                        default:
                            job.NextStep = EncodingStep.Demux;
                            break;
                    }
                    job.ExitCode = 0;
                    break;

                case EncodingStep.CopyTempFile:
                case EncodingStep.Dump:
                    job.NextStep = EncodingStep.Demux;
                    break;

                case EncodingStep.Demux:
                    // if output format for audio is other than copy or 
                    // output is set to copy but the encoding profile is set to output dvd
                    //
                    // and if there are actually some audio streams
                    //
                    // then the next step is to encode the audio streams
                    if ((job.AudioProfile.Type != ProfileType.Copy ||
                         (job.AudioProfile.Type == ProfileType.Copy &&
                          job.EncodingProfile.OutFormat == OutputType.OutputDvd)) &&
                        job.AudioStreams.Count > 0)
                    {
                        job.NextStep = EncodingStep.EncodeAudio;
                        job.StreamId = 0;
                    }
                    else
                        GetSubOrVideoStep(job);
                    break;

                case EncodingStep.EncodeAudio:
                    if (job.AudioStreams.Count - 1 > job.StreamId)
                        job.StreamId++;
                    else
                        GetSubOrVideoStep(job);
                    break;

                case EncodingStep.ProcessSubtitle:
                    if (job.SubtitleStreams.Count - 1 > job.StreamId &&
                        job.EncodingProfile.OutFormat == OutputType.OutputDvd &&
                        AppSettings.JavaInstalled)
                        job.StreamId++;
                    else
                        GetSubOrVideoStep(job);
                    break;

                case EncodingStep.IndexVideo:
                    if (job.EncodingProfile.AutoCropResize &&
                        !job.EncodingProfile.KeepInputResolution &&
                        job.EncodingProfile.OutFormat != OutputType.OutputDvd)
                        job.NextStep = EncodingStep.GetCropRect;
                    else
                    {
                        job.NextStep = EncodingStep.EncodeVideo;
                        job.StreamId = 1;
                    }
                    break;

                case EncodingStep.GetCropRect:
                    job.NextStep = EncodingStep.EncodeVideo;
                    job.StreamId = 1;
                    break;

                case EncodingStep.EncodeVideo:
                    int encodingPasses = 1;
                    switch (job.VideoProfile.Type)
                    {
                        case ProfileType.X264:
                            switch (((X264Profile)job.VideoProfile).EncodingMode)
                            {
                                case 2:
                                    encodingPasses = 2;
                                    break;
                                case 3:
                                    encodingPasses = 3;
                                    break;
                            }
                            break;
                        case ProfileType.VP8:
                            encodingPasses += ((VP8Profile) job.VideoProfile).EncodingMode;
                            break;
                    }
                    if (job.StreamId < encodingPasses)
                        job.StreamId++;
                    else if (job.EncodingProfile.OutFormat == OutputType.OutputDvd)
                        job.NextStep = EncodingStep.PreMuxResult;
                    else
                        job.NextStep = EncodingStep.MuxResult;
                    break;

                case EncodingStep.PreMuxResult:
                    if (job.SubtitleStreams.Count > 0)
                    {
                        job.NextStep = EncodingStep.PremuxSubtitle;
                        job.StreamId = 0;
                    }
                    else
                        job.NextStep = EncodingStep.MuxResult;
                    break;

                case EncodingStep.PremuxSubtitle:
                    if (job.StreamId < job.SubtitleStreams.Count - 1)
                        job.StreamId++;
                    else
                        job.NextStep = EncodingStep.MuxResult;
                    break;

                case EncodingStep.MuxResult:
                    job.NextStep = string.IsNullOrEmpty(job.TempOutput) ? EncodingStep.Done : EncodingStep.MoveOutFile;
                    break;

                case EncodingStep.MoveOutFile:
                    job.NextStep = EncodingStep.Done;
                    break;
            }
        }

        private static void GetSubOrVideoStep(EncodeInfo job)
        {
            if (job.VideoStream != null)
            {
                switch (job.NextStep)
                {
                    case EncodingStep.Demux:
                    case EncodingStep.EncodeAudio:
                        SubtitleInfo sub =
                            job.SubtitleStreams.FirstOrDefault(
                                subInfo =>
                                !subInfo.HardSubIntoVideo && subInfo.KeepOnlyForcedCaptions &&
                                (subInfo.Format.Equals("VobSub") || subInfo.Format.Equals("PGS")));

                        if (job.EncodingProfile.OutFormat == OutputType.OutputDvd &&
                            AppSettings.JavaInstalled &&
                            job.SubtitleStreams.Count > 0)
                        {
                            job.NextStep = EncodingStep.ProcessSubtitle;
                            job.StreamId = 0;
                        }
                        else if (AppSettings.JavaInstalled && sub != null)
                        {
                            job.NextStep = EncodingStep.ProcessSubtitle;
                            job.StreamId = job.SubtitleStreams.IndexOf(sub);
                        }
                        else if (job.VideoProfile.Type != ProfileType.Copy)
                            job.NextStep = EncodingStep.IndexVideo;
                        else if (job.EncodingProfile.OutFormat == OutputType.OutputDvd)
                            job.NextStep = EncodingStep.PreMuxResult;
                        else
                            job.NextStep = EncodingStep.MuxResult;
                        break;
                    case EncodingStep.ProcessSubtitle:
                        if (job.VideoProfile.Type != ProfileType.Copy)
                            job.NextStep = EncodingStep.IndexVideo;
                        else if (job.EncodingProfile.OutFormat == OutputType.OutputDvd)
                            job.NextStep = EncodingStep.PreMuxResult;
                        else
                            job.NextStep = EncodingStep.MuxResult;
                        break;
                }
            }
            else
            {
                job.NextStep = job.EncodingProfile.OutFormat == OutputType.OutputDvd ? EncodingStep.PreMuxResult : EncodingStep.MuxResult;
            }
        }

        private int GetTotalEncodingSteps()
        {
            int encodingSteps = 0;

            foreach (EncodeInfo job in JobList)
            {

                if (!string.IsNullOrEmpty(job.TempInput))
                    encodingSteps++; // create temp file with ascii filename

                encodingSteps++; // demux

                encodingSteps += job.AudioStreams.Count(aud => job.AudioProfile.Type == ProfileType.AC3 ||
                                                               job.AudioProfile.Type == ProfileType.OGG ||
                                                               job.AudioProfile.Type == ProfileType.MP3 ||
                                                               job.AudioProfile.Type == ProfileType.AAC ||
                                                               job.AudioProfile.Type == ProfileType.FLAC);

                encodingSteps +=
                    job.SubtitleStreams.Count(sub => job.EncodingProfile.OutFormat == OutputType.OutputDvd ||
                                                     (sub.KeepOnlyForcedCaptions &&
                                                      !sub.HardSubIntoVideo &&
                                                      (sub.Format.Equals("VobSub") || sub.Format.Equals("PGS"))));

                if (job.VideoStream != null)
                {
                    if (job.VideoProfile.Type != ProfileType.Copy)
                    {
                        encodingSteps++;   // index
                        switch (job.VideoProfile.Type)
                        {
                            case ProfileType.X264:
                                X264Profile videoProfile = (X264Profile)job.VideoProfile;

                                switch (videoProfile.EncodingMode)
                                {
                                    case 2:
                                        encodingSteps += 2;     // 2 pass encoding
                                        break;
                                    case 3:
                                        encodingSteps += 3;     // 3 pass encoding
                                        break;
                                    default:
                                        encodingSteps++;
                                        break;
                                }

                                if (job.EncodingProfile.AutoCropResize && !job.EncodingProfile.KeepInputResolution)
                                    encodingSteps++;    // search croprect

                                break;
                            case ProfileType.HcEnc:
                                encodingSteps += 2;
                                break;
                            case ProfileType.VP8:
                                VP8Profile vp8Profile = (VP8Profile) job.VideoProfile;
                                if (vp8Profile.EncodingMode == 0)
                                    encodingSteps++;
                                else
                                    encodingSteps += 2;

                                if (job.EncodingProfile.AutoCropResize && !job.EncodingProfile.KeepInputResolution)
                                    encodingSteps++;    // search croprect

                                break;
                        }
                    } // end if videoprofile != copy

                    if (job.EncodingProfile.OutFormat == OutputType.OutputDvd)
                    {
                        encodingSteps++;   // premux streams for dvdauthor
                        if (job.SubtitleStreams.Count > 0)
                            encodingSteps += 2;    // premux subtitles
                    }

                } // end if videostream != null

                encodingSteps++;    // mux streams

                if (!string.IsNullOrEmpty(job.TempOutput))
                    encodingSteps++; // move finished file to output destination
            }   // foreach job

            return encodingSteps;
        }

        private void AbortBtnClick(object sender, RoutedEventArgs e)
        {
            if (_worker == null) return;

            Log.Info("Aborting process");
            _worker.CancelAsync();
            _cancel = true;
            AbortBtn.IsEnabled = false;
        }
    }
}
