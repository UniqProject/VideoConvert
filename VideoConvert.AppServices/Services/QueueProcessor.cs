// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueProcessor.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The Queue Processor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Services
{
    using log4net;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using VideoConvert.AppServices.Decoder.Interfaces;
    using VideoConvert.AppServices.Demuxer.Interfaces;
    using VideoConvert.AppServices.Encoder.Interfaces;
    using VideoConvert.AppServices.Muxer.Interfaces;
    using VideoConvert.AppServices.Services.Base.Interfaces;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Model.Profiles;

    /// <summary>
    /// Queue Processor
    /// </summary>
    public class QueueProcessor : IQueueProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(QueueProcessor));

        private ObservableCollection<EncodeInfo> _queueList = new ObservableCollection<EncodeInfo>();

        private readonly IAppConfigService _appConfig;
        private readonly IProcessingService _processingService;

        private readonly IDecoderFfmpegGetCrop _ffmpegGetCrop;
        private readonly IDecoderFfmsIndex _ffmsIndex;

        private readonly IDemuxerEac3To _eac3To;
        private readonly IDemuxerFfmpeg _ffmpegDemuxer;
        private readonly IDemuxerMplayer _mplayerDemuxer;
        private readonly IDemuxerMkvExtractSubtitle _mkvExtractSubtitle;

        private readonly IEncoderBdSup2Sub _bdSup2Sub;
        private readonly IEncoderFfmpegAc3 _ffmpegAc3;
        private readonly IEncoderFfmpegDvd _ffmpegDvd;
        private readonly IEncoderLame _lame;
        private readonly IEncoderNeroAac _neroAac;
        private readonly IEncoderOggEnc _oggEnc;
        private readonly IEncoderX264 _x264;
        private readonly IEncoderFfmpegX264 _ffmpegX264;

        private readonly IFileWorker _fileWorker;
        private readonly IMuxerDvdAuthor _dvdAuthor;
        private readonly IMuxerMkvMerge _mkvMerge;
        private readonly IMuxerMp4Box _mp4Box;
        private readonly IMuxerMplex _mplex;
        private readonly IMuxerSpuMux _spuMux;
        private readonly IMuxerTsMuxeR _tsMuxeR;

        private IEncodeBase _currentEncoder;

        private int _processingSteps;
        private int _finishedSteps;

        private double _fullTaskPercent = 100f;

        private EncodeInfo _currentJob;

        /// <summary>
        /// Fires when Queue processing starts.
        /// </summary>
        public event EventHandler QueueStarted;

        /// <summary>
        /// Fires when Queue processing finishes.
        /// </summary>
        public event QueueCompletedStatus QueueCompleted;

        /// <summary>
        /// Queue has progressed
        /// </summary>
        public event QueueProgressStatus QueueProgressChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueProcessor"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// Application configuration
        /// </param>
        /// <param name="processingService">
        /// Processing Service
        /// </param>
        /// <param name="ffmpegGetCrop">
        /// ffmpeg crop interface
        /// </param>
        /// <param name="ffmsIndex">
        /// ffmsindex interface
        /// </param>
        /// <param name="eac3To">
        /// eac3to interface
        /// </param>
        /// <param name="ffmpegDemuxer">
        /// ffmpeg demux interface
        /// </param>
        /// <param name="mplayerDemuxer">
        /// mplayer demux interface
        /// </param>
        /// <param name="mkvExtractSubtitle">
        /// mkvextract subtitle demux interface
        /// </param>
        /// <param name="bdSup2Sub">
        /// BDSup2Sub interface
        /// </param>
        /// <param name="ffmpegAc3">
        /// ffmpeg AC-3 encoder Interface
        /// </param>
        /// <param name="ffmpegDvd">
        /// ffmpeg DVD encoder Interface
        /// </param>
        /// <param name="lame">
        /// lame encoder Interface
        /// </param>
        /// <param name="neroAac">
        /// NeroAacEnc Interface
        /// </param>
        /// <param name="oggEnc">
        /// OggEnc Interface
        /// </param>
        /// <param name="x264">
        /// x264 encoder Interface
        /// </param>
        /// <param name="ffmpegX264">
        /// ffmpeg x264 encoder Interface
        /// </param>
        /// <param name="fileWorker">
        /// FileWorker Interface
        /// </param>
        /// <param name="dvdAuthor">
        /// DVDAuthor Interface
        /// </param>
        /// <param name="mkvMerge">
        /// mkvMerge Interface
        /// </param>
        /// <param name="mp4Box">
        /// mp4box Interface
        /// </param>
        /// <param name="mplex">
        /// Mplex Interface
        /// </param>
        /// <param name="spuMux">
        /// SpuMux Interface
        /// </param>
        /// <param name="tsMuxeR">
        /// tsMuxeR Interface
        /// </param>
        public QueueProcessor(IAppConfigService appConfig, IProcessingService processingService,
                              //decoder
                              IDecoderFfmpegGetCrop ffmpegGetCrop, IDecoderFfmsIndex ffmsIndex,
                              //demuxer
                              IDemuxerEac3To eac3To, IDemuxerFfmpeg ffmpegDemuxer,
                              IDemuxerMplayer mplayerDemuxer, IDemuxerMkvExtractSubtitle mkvExtractSubtitle,
                              //encoder
                              IEncoderBdSup2Sub bdSup2Sub, IEncoderFfmpegAc3 ffmpegAc3,
                              IEncoderFfmpegDvd ffmpegDvd, IEncoderLame lame, IEncoderNeroAac neroAac,
                              IEncoderOggEnc oggEnc, IEncoderX264 x264, IEncoderFfmpegX264 ffmpegX264,
                              //muxer
                              IFileWorker fileWorker, IMuxerDvdAuthor dvdAuthor, IMuxerMkvMerge mkvMerge,
                              IMuxerMp4Box mp4Box, IMuxerMplex mplex, IMuxerSpuMux spuMux, IMuxerTsMuxeR tsMuxeR)
        {
            this._appConfig = appConfig;
            this._processingService = processingService;

            this._ffmpegGetCrop = ffmpegGetCrop;
            this._ffmsIndex = ffmsIndex;

            this._eac3To = eac3To;
            this._ffmpegDemuxer = ffmpegDemuxer;
            this._mplayerDemuxer = mplayerDemuxer;
            this._mkvExtractSubtitle = mkvExtractSubtitle;

            this._bdSup2Sub = bdSup2Sub;
            this._ffmpegAc3 = ffmpegAc3;
            this._ffmpegDvd = ffmpegDvd;
            this._lame = lame;
            this._neroAac = neroAac;
            this._oggEnc = oggEnc;
            this._x264 = x264;
            this._ffmpegX264 = ffmpegX264;

            this._fileWorker = fileWorker;
            this._dvdAuthor = dvdAuthor;
            this._mkvMerge = mkvMerge;
            this._mp4Box = mp4Box;
            this._mplex = mplex;
            this._spuMux = spuMux;
            this._tsMuxeR = tsMuxeR;
        }

        /// <summary>
        /// Invoke the Queue Status Changed Event.
        /// </summary>
        /// <param name="e">
        /// The QueueProgressEventArgs.
        /// </param>
        public void InvokeQueueStatusChanged(QueueProgressEventArgs e)
        {
            var handler = this.QueueProgressChanged;
            if (handler != null)
            {
                handler(this._currentEncoder, e);
            }
        }

        /// <summary>
        /// Invoke the Encode Completed Event
        /// </summary>
        /// <param name="e">
        /// The QueueCompletedEventArgs.
        /// </param>
        public void InvokeQueueCompleted(QueueCompletedEventArgs e)
        {
            var handler = this.QueueCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Invoke the Encode Started Event
        /// </summary>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        public void InvokeQueueStarted(EventArgs e)
        {
            var handler = this.QueueStarted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void EncodeCompleted(object sender, EncodeCompletedEventArgs args)
        {
            this._finishedSteps++;

            this._currentEncoder.EncodeCompleted -= EncodeCompleted;
            this._currentEncoder.EncodeStarted -= EncodeStarted;
            this._currentEncoder.EncodeStatusChanged -= EncoderProgressStatus;
            this._currentEncoder = null;

            DeleteTempFiles();
            if (this._currentJob != null && this._currentJob.ExitCode == 0)
            {
                this.InvokeQueueStatusChanged(new QueueProgressEventArgs
                {
                    JobName = string.Empty,
                    AverageFrameRate = 0f,
                    CurrentFrameRate = 0f,
                    CurrentFrame = 0,
                    TotalFrames = 0,
                    ElapsedTime = new TimeSpan(),
                    EstimatedTimeLeft = new TimeSpan(),
                    PercentComplete = 0,
                    TotalPercentComplete = (this._finishedSteps * this._fullTaskPercent),
                    Pass = 0,
                });
            
                GetNextStep();

                if (this._currentJob.NextStep == EncodingStep.Done &&
                    this._queueList.IndexOf(this._currentJob) < this._queueList.Count - 1)
                {
                    this._currentJob = GetNextJob();
                    GetNextStep();
                }
                else if (this._currentJob.NextStep == EncodingStep.Done &&
                         this._queueList.IndexOf(this._currentJob) == this._queueList.Count - 1)
                {
                    this.InvokeQueueCompleted(new QueueCompletedEventArgs(true,null,string.Empty));
                    return;
                }

                ExecuteNextStep();
            }
            else
            {
                var currentJob = this._currentJob;
                var exitCode = -1;
                if (currentJob != null)
                    exitCode = currentJob.ExitCode;
                this.InvokeQueueCompleted(new QueueCompletedEventArgs(false,
                    new ApplicationException("Encoder exited with code " + exitCode),
                    "Encoder exited with code " + exitCode));
            }
        }

        private void EncoderProgressStatus(object sender, EncodeProgressEventArgs args)
        {
            double totalPercent = (this._finishedSteps * this._fullTaskPercent) +
                                  (this._fullTaskPercent * args.PercentComplete / 100d);
            
            this.InvokeQueueStatusChanged(new QueueProgressEventArgs
            {
                JobName = string.Empty,
                AverageFrameRate = args.AverageFrameRate,
                CurrentFrameRate = args.CurrentFrameRate,
                CurrentFrame = args.CurrentFrame,
                TotalFrames = args.TotalFrames,
                ElapsedTime = args.ElapsedTime,
                EstimatedTimeLeft = args.EstimatedTimeLeft,
                PercentComplete = args.PercentComplete,
                TotalPercentComplete = totalPercent,
                Pass = args.Pass,
            });
        }

        private void EncodeStarted(object sender, EventArgs eventArgs)
        {
            this.InvokeQueueStatusChanged(new QueueProgressEventArgs
            {
                JobName = this._currentJob.JobName,
                AverageFrameRate = 0f,
                CurrentFrameRate = 0f,
                CurrentFrame = 0,
                TotalFrames = 0,
                ElapsedTime = new TimeSpan(),
                EstimatedTimeLeft = new TimeSpan(),
                PercentComplete = 0,
                TotalPercentComplete = (this._finishedSteps * this._fullTaskPercent),
                Pass = this._currentJob.StreamId,
            });
        }

        /// <summary>
        /// Stops queue processing
        /// </summary>
        public void Stop()
        {
            if (this._currentEncoder != null)
            {
                this._currentEncoder.Stop();
            }
        }

        /// <summary>
        /// Starts queue processing
        /// </summary>
        /// <param name="queue"></param>
        public async void StartProcessing(ObservableCollection<EncodeInfo> queue)
        {
            this.InvokeQueueStarted(EventArgs.Empty);

            this._queueList = queue;
            this._processingSteps = await Task.Run(() => CountProcessingSteps());
            this._fullTaskPercent = 100f / this._processingSteps;

            try
            {
                this._currentJob = GetNextJob();
            }
            catch (Exception ex)
            {
                this._currentJob = null;
                this.InvokeQueueCompleted(new QueueCompletedEventArgs(false, ex, ex.Message)); 
                return;
            }

            _finishedSteps = 0;

            GetNextStep();

            ExecuteNextStep();
        }

        private int CountProcessingSteps()
        {
            int encodingSteps = 0;

            foreach (EncodeInfo job in this._queueList)
            {

                if (!string.IsNullOrEmpty(job.TempInput))
                    encodingSteps++; // create temp file with ascii filename

                encodingSteps++; // demux

                encodingSteps +=
                    job.AudioStreams.Count(
                        aud =>
                            job.AudioProfile.Type == ProfileType.Ac3 || job.AudioProfile.Type == ProfileType.Ogg ||
                            job.AudioProfile.Type == ProfileType.Mp3 || job.AudioProfile.Type == ProfileType.Aac ||
                            job.AudioProfile.Type == ProfileType.Flac);

                // demux subtitles
                encodingSteps += job.SubtitleStreams.Count(sub => sub.RawStream == false);

                // process subtitles
                encodingSteps += job.SubtitleStreams.Count(sub => sub.NeedConversion);

                if (job.VideoStream != null)
                {
                    if (job.VideoProfile.Type != ProfileType.Copy)
                    {
                        encodingSteps++;   // index
                        switch (job.VideoProfile.Type)
                        {
                            case ProfileType.X264:
                                var videoProfile = (X264Profile)job.VideoProfile;

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
                            case ProfileType.Vp8:
                                var vp8Profile = (Vp8Profile)job.VideoProfile;
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
                            encodingSteps += job.SubtitleStreams.Count;    // premux subtitles
                    }

                } // end if videostream != null

                encodingSteps++;    // mux streams

                if (!string.IsNullOrEmpty(job.TempOutput))
                    encodingSteps++; // move finished file to output destination

                if (this._appConfig.CreateXbmcInfoFile && (job.MovieInfo != null || job.EpisodeInfo != null))
                    encodingSteps++; // create xbmc info files
            }   // foreach job

            return encodingSteps;
        }

        private EncodeInfo GetNextJob()
        {
            return this._queueList.First(info => info.NextStep == EncodingStep.NotSet);
        }

        private void GetNextStep()
        {
            int nextIndex;
            switch (this._currentJob.NextStep)
            {
                case EncodingStep.NotSet:
                    switch (this._currentJob.Input)
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
                            this._currentJob.NextStep = !string.IsNullOrEmpty(this._currentJob.TempInput)
                                                        ? EncodingStep.CopyTempFile
                                                        : EncodingStep.Demux;
                            break;
                        case InputType.InputDvd:
                            this._currentJob.NextStep = EncodingStep.Dump;
                            break;
                        default:
                            this._currentJob.NextStep = EncodingStep.Demux;
                            break;
                    }
                    this._currentJob.ExitCode = 0;
                    break;

                case EncodingStep.CopyTempFile:
                case EncodingStep.Dump:
                    this._currentJob.NextStep = EncodingStep.Demux;
                    break;

                case EncodingStep.Demux:
                    // if output format for audio is other than copy or 
                    // output is set to copy but the encoding profile is set to output dvd
                    //
                    // and if there are actually some audio streams
                    //
                    // then the next step is to encode the audio streams
                    if ((this._currentJob.AudioProfile.Type != ProfileType.Copy ||
                         (this._currentJob.AudioProfile.Type == ProfileType.Copy &&
                          this._currentJob.EncodingProfile.OutFormat == OutputType.OutputDvd)) &&
                        this._currentJob.AudioStreams.Count > 0)
                    {
                        this._currentJob.NextStep = EncodingStep.EncodeAudio;
                        this._currentJob.StreamId = 0;
                    }
                    else
                        GetSubOrVideoStep();
                    break;

                case EncodingStep.EncodeAudio:
                    if (this._currentJob.AudioStreams.Count - 1 > this._currentJob.StreamId)
                        this._currentJob.StreamId++;
                    else
                        GetSubOrVideoStep();
                    break;

                case EncodingStep.IndexVideo:
                    if (this._currentJob.EncodingProfile.AutoCropResize &&
                        !this._currentJob.EncodingProfile.KeepInputResolution &&
                        this._currentJob.EncodingProfile.OutFormat != OutputType.OutputDvd)
                        this._currentJob.NextStep = EncodingStep.GetCropRect;
                    else
                    {
                        this._currentJob.NextStep = EncodingStep.EncodeVideo;
                        this._currentJob.StreamId = 1;
                    }
                    break;

                case EncodingStep.GetCropRect:
                    this._currentJob.NextStep = EncodingStep.EncodeVideo;
                    this._currentJob.StreamId = 1;
                    break;

                case EncodingStep.EncodeVideo:
                    int encodingPasses = 1;
                    switch (this._currentJob.VideoProfile.Type)
                    {
                        case ProfileType.X264:
                            switch (((X264Profile)this._currentJob.VideoProfile).EncodingMode)
                            {
                                case 2:
                                    encodingPasses = 2;
                                    break;
                                case 3:
                                    encodingPasses = 3;
                                    break;
                            }
                            break;
                        case ProfileType.Vp8:
                            encodingPasses += ((Vp8Profile)this._currentJob.VideoProfile).EncodingMode;
                            break;
                    }
                    if (this._currentJob.StreamId < encodingPasses)
                        this._currentJob.StreamId++;
                    else
                        GetSubOrVideoStep();
                    break;

                case EncodingStep.DemuxSubtitle:
                    nextIndex = this._currentJob.SubtitleStreams.FindIndex(this._currentJob.StreamId, info => info.RawStream == false);

                    if (this._currentJob.SubtitleStreams.Count - 1 > this._currentJob.StreamId && nextIndex > -1)
                        this._currentJob.StreamId = nextIndex;
                    else
                        GetSubOrVideoStep();
                    break;
                case EncodingStep.ProcessSubtitle:
                    nextIndex = this._currentJob.SubtitleStreams.FindIndex(this._currentJob.StreamId, info => info.NeedConversion);
                    if (this._currentJob.SubtitleStreams.Count - 1 > this._currentJob.StreamId &&
                        this._appConfig.JavaInstalled && this._appConfig.BDSup2SubInstalled && nextIndex > -1)
                        this._currentJob.StreamId = nextIndex;
                    else
                        GetSubOrVideoStep();
                    break;

                case EncodingStep.PreMuxResult:
                    if (this._currentJob.SubtitleStreams.Count > 0)
                    {
                        this._currentJob.NextStep = EncodingStep.PremuxSubtitle;
                        this._currentJob.StreamId = 0;
                    }
                    else
                        this._currentJob.NextStep = EncodingStep.MuxResult;
                    break;

                case EncodingStep.PremuxSubtitle:
                    if (this._currentJob.StreamId < this._currentJob.SubtitleStreams.Count - 1)
                        this._currentJob.StreamId++;
                    else
                        this._currentJob.NextStep = EncodingStep.MuxResult;
                    break;

                case EncodingStep.MuxResult:
                    if (string.IsNullOrEmpty(this._currentJob.TempOutput))
                        if (this._appConfig.CreateXbmcInfoFile &&
                            (this._currentJob.MovieInfo != null || this._currentJob.EpisodeInfo != null))
                            this._currentJob.NextStep = EncodingStep.WriteInfoFile;
                        else
                            this._currentJob.NextStep = EncodingStep.Done;
                    else
                        this._currentJob.NextStep = EncodingStep.MoveOutFile;
                    break;

                case EncodingStep.MoveOutFile:
                    if (this._appConfig.CreateXbmcInfoFile && (this._currentJob.MovieInfo != null || this._currentJob.EpisodeInfo != null))
                        this._currentJob.NextStep = EncodingStep.WriteInfoFile;
                    else
                        this._currentJob.NextStep = EncodingStep.Done;
                    break;

                case EncodingStep.WriteInfoFile:
                    this._currentJob.NextStep = EncodingStep.Done;
                    break;
            }
        }

        private void GetSubOrVideoStep()
        {
            if (this._currentJob.VideoStream != null)
            {
                switch (this._currentJob.CompletedStep)
                {
                    case EncodingStep.Demux:
                    case EncodingStep.EncodeAudio:
                    case EncodingStep.EncodeVideo:
                    case EncodingStep.DemuxSubtitle:
                        var sub = this._currentJob.SubtitleStreams.FirstOrDefault(subInfo => subInfo.NeedConversion);
                        var demuxSubtitleIndex = this._currentJob.SubtitleStreams.FindIndex(info => info.RawStream == false);

                        if (this._currentJob.VideoProfile.Type != ProfileType.Copy && !this._currentJob.VideoStream.Encoded)
                            this._currentJob.NextStep = EncodingStep.IndexVideo;
                        else if (demuxSubtitleIndex > -1)
                        {
                            this._currentJob.NextStep = EncodingStep.DemuxSubtitle;
                            this._currentJob.StreamId = demuxSubtitleIndex;
                        }
                        else if (((this._appConfig.JavaInstalled && this._appConfig.BDSup2SubInstalled) ||
                                  this._currentJob.EncodingProfile.OutFormat == OutputType.OutputMp4) && sub != null)
                        {
                            this._currentJob.NextStep = EncodingStep.ProcessSubtitle;
                            this._currentJob.StreamId = this._currentJob.SubtitleStreams.IndexOf(sub);
                        }
                        else if (this._currentJob.EncodingProfile.OutFormat == OutputType.OutputDvd)
                            this._currentJob.NextStep = EncodingStep.PreMuxResult;
                        else
                            this._currentJob.NextStep = EncodingStep.MuxResult;
                        break;

                    case EncodingStep.ProcessSubtitle:
                        this._currentJob.NextStep = this._currentJob.EncodingProfile.OutFormat == OutputType.OutputDvd
                                                                                                ? EncodingStep.PreMuxResult
                                                                                                : EncodingStep.MuxResult;
                        break;
                }
            }
            else
            {
                this._currentJob.NextStep = this._currentJob.EncodingProfile.OutFormat == OutputType.OutputDvd
                    ? EncodingStep.PreMuxResult
                    : EncodingStep.MuxResult;
            }
        }

        private void ExecuteNextStep()
        {
            switch (this._currentJob.NextStep)
            {
                case EncodingStep.CopyTempFile:
                    ExecuteCopyTempFile();
                    break;
                case EncodingStep.Dump:
                    ExecuteDump();
                    break;
                case EncodingStep.Demux:
                    ExecuteDemux();
                    break;
                case EncodingStep.EncodeAudio:
                    ExecuteEncodeAudio();
                    break;
                case EncodingStep.DemuxSubtitle:
                    ExecuteDemuxSubtitle();
                    break;
                case EncodingStep.ProcessSubtitle:
                    ExecuteProcessSubtitle();
                    break;
                case EncodingStep.IndexVideo:
                    ExecuteIndexVideo();
                    break;
                case EncodingStep.GetCropRect:
                    ExecuteGetCropRect();
                    break;
                case EncodingStep.EncodeVideo:
                    ExecuteEncodeVideo();
                    break;
                case EncodingStep.PreMuxResult:
                    ExecutePremuxResult();
                    break;
                case EncodingStep.PremuxSubtitle:
                    ExecutePremuxSubtitle();
                    break;
                case EncodingStep.MuxResult:
                    ExecuteMuxResult();
                    break;
                case EncodingStep.MoveOutFile:
                    ExecuteMoveOutFile();
                    break;
                case EncodingStep.WriteInfoFile:
                    ExecuteWriteInfoFile();
                    break;
                case EncodingStep.Done:
                    this._currentJob.TempFiles.Add(this._currentJob.TempInput);
                    this._currentJob.TempFiles.Add(this._currentJob.TempOutput);
                    DeleteTempFiles();
                    break;
            }
        }

        private void ExecuteGenericEncoder()
        {
            this._currentEncoder.EncodeStarted += EncodeStarted;
            this._currentEncoder.EncodeStatusChanged += EncoderProgressStatus;
            this._currentEncoder.EncodeCompleted += EncodeCompleted;
            this._currentEncoder.Start(this._currentJob);
        }

        private void ExecuteCopyTempFile()
        {
            this._currentEncoder = this._fileWorker;

            ExecuteGenericEncoder();
        }

        private void ExecuteDump()
        {
            this._currentEncoder = this._mplayerDemuxer;

            ExecuteGenericEncoder();
        }

        private void ExecuteDemux()
        {
            switch (this._currentJob.Input)
            {
                case InputType.InputBluRay:
                case InputType.InputAvchd:
                case InputType.InputHddvd:
                    this._currentEncoder = this._eac3To;
                    break;
                case InputType.InputDvd:
                case InputType.InputAvi:
                case InputType.InputFlash:
                case InputType.InputMp4:
                case InputType.InputWm:
                case InputType.InputMatroska:
                case InputType.InputMpegps:
                case InputType.InputTs:
                case InputType.InputOgg:
                case InputType.InputWebM:
                    this._currentEncoder = this._ffmpegDemuxer;
                    break;
            }

            ExecuteGenericEncoder();
        }

        private void ExecuteEncodeAudio()
        {
            switch (this._currentJob.AudioProfile.Type)
            {
                case ProfileType.Ac3:
                    this._currentEncoder = this._ffmpegAc3;
                    break;
                case ProfileType.Ogg:
                    this._currentEncoder = this._oggEnc;
                    break;
                case ProfileType.Aac:
                    this._currentEncoder = this._neroAac;
                    break;
                case ProfileType.Mp3:
                    this._currentEncoder = this._lame;
                    break;
                case ProfileType.Copy:
                    if (this._currentJob.EncodingProfile.OutFormat == OutputType.OutputDvd &&
                        !this._processingService.CheckAudioDvdCompatible(this._currentJob.AudioStreams[this._currentJob.StreamId]))
                    {
                        this._currentEncoder = this._ffmpegAc3;
                    }
                    break;
            }

            ExecuteGenericEncoder();
        }

        private void ExecuteDemuxSubtitle()
        {
            this._currentEncoder = this._mkvExtractSubtitle;

            ExecuteGenericEncoder();
        }

        private void ExecuteProcessSubtitle()
        {
            switch (this._currentJob.EncodingProfile.OutFormat)
            {
                case OutputType.OutputMp4:
                    // TODO: Text Subtitle converter
                    break;
                default:
                    this._currentEncoder = this._bdSup2Sub;
                    break;
            }

            ExecuteGenericEncoder();
        }

        private void ExecuteIndexVideo()
        {
            this._currentEncoder = this._ffmsIndex;

            ExecuteGenericEncoder();
        }

        private void ExecuteGetCropRect()
        {
            this._currentEncoder = this._ffmpegGetCrop;

            ExecuteGenericEncoder();
        }

        private void ExecuteEncodeVideo()
        {
            switch (this._currentJob.VideoProfile.Type)
            {
                case ProfileType.X264:
                    //this._currentEncoder = this._x264;
                    this._currentEncoder = this._ffmpegX264;
                    break;
                case ProfileType.Mpeg2Video:
                    this._currentEncoder = this._ffmpegDvd;
                    break;
                case ProfileType.Vp8:
                    // TODO: VPXEnc
                    break;
            }

            ExecuteGenericEncoder();
        }

        private void ExecutePremuxResult()
        {
            this._currentEncoder = this._mplex;

            ExecuteGenericEncoder();
        }

        private void ExecutePremuxSubtitle()
        {
            this._currentEncoder = this._spuMux;

            ExecuteGenericEncoder();
        }

        private void ExecuteMuxResult()
        {
            switch (this._currentJob.EncodingProfile.OutFormat)
            {
                case OutputType.OutputMatroska:
                case OutputType.OutputWebM:
                    this._currentEncoder = this._mkvMerge;
                    break;
                case OutputType.OutputMp4:
                    this._currentEncoder = this._mp4Box;
                    break;
                case OutputType.OutputTs:
                case OutputType.OutputM2Ts:
                case OutputType.OutputBluRay:
                case OutputType.OutputAvchd:
                    this._currentEncoder = this._tsMuxeR;
                    break;
                case OutputType.OutputDvd:
                    this._currentEncoder = this._dvdAuthor;
                    break;
            }

            ExecuteGenericEncoder();
        }

        private void ExecuteMoveOutFile()
        {
            this._currentEncoder = this._fileWorker;

            ExecuteGenericEncoder();
        }

        private void ExecuteWriteInfoFile()
        {
            // TODO: InfoWriter
            ExecuteGenericEncoder();
        }

        private void DeleteTempFiles()
        {
            if (!this._appConfig.DeleteTemporaryFiles) return;

            foreach (string tempFile in this._currentJob.TempFiles)
            {
                if (tempFile == this._currentJob.InputFile || tempFile == this._currentJob.OutputFile ||
                    string.IsNullOrEmpty(tempFile))
                    continue;

                var fi = new FileInfo(tempFile).Attributes;

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
            this._currentJob.TempFiles.Clear();
        }
    }
}
