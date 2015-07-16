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
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using log4net;
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
        private readonly IDemuxerTsMuxeR _demuxerTsMuxeR;

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
        /// <param name="demuxerTsMuxeR">
        /// tsMuxeR demux interface
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
                              IDemuxerTsMuxeR demuxerTsMuxeR,
                              //encoder
                              IEncoderBdSup2Sub bdSup2Sub, IEncoderFfmpegAc3 ffmpegAc3,
                              IEncoderFfmpegDvd ffmpegDvd, IEncoderLame lame, IEncoderNeroAac neroAac,
                              IEncoderOggEnc oggEnc, IEncoderX264 x264, IEncoderFfmpegX264 ffmpegX264,
                              //muxer
                              IFileWorker fileWorker, IMuxerDvdAuthor dvdAuthor, IMuxerMkvMerge mkvMerge,
                              IMuxerMp4Box mp4Box, IMuxerMplex mplex, IMuxerSpuMux spuMux, IMuxerTsMuxeR tsMuxeR)
        {
            _appConfig = appConfig;
            _processingService = processingService;

            _ffmpegGetCrop = ffmpegGetCrop;
            _ffmsIndex = ffmsIndex;

            _eac3To = eac3To;
            _ffmpegDemuxer = ffmpegDemuxer;
            _mplayerDemuxer = mplayerDemuxer;
            _mkvExtractSubtitle = mkvExtractSubtitle;
            _demuxerTsMuxeR = demuxerTsMuxeR;

            _bdSup2Sub = bdSup2Sub;
            _ffmpegAc3 = ffmpegAc3;
            _ffmpegDvd = ffmpegDvd;
            _lame = lame;
            _neroAac = neroAac;
            _oggEnc = oggEnc;
            _x264 = x264;
            _ffmpegX264 = ffmpegX264;

            _fileWorker = fileWorker;
            _dvdAuthor = dvdAuthor;
            _mkvMerge = mkvMerge;
            _mp4Box = mp4Box;
            _mplex = mplex;
            _spuMux = spuMux;
            _tsMuxeR = tsMuxeR;
        }

        /// <summary>
        /// Invoke the Queue Status Changed Event.
        /// </summary>
        /// <param name="e">
        /// The QueueProgressEventArgs.
        /// </param>
        public void InvokeQueueStatusChanged(QueueProgressEventArgs e)
        {
            var handler = QueueProgressChanged;
            handler?.Invoke(_currentEncoder, e);
        }

        /// <summary>
        /// Invoke the Encode Completed Event
        /// </summary>
        /// <param name="e">
        /// The QueueCompletedEventArgs.
        /// </param>
        public void InvokeQueueCompleted(QueueCompletedEventArgs e)
        {
            var handler = QueueCompleted;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Invoke the Encode Started Event
        /// </summary>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        public void InvokeQueueStarted(EventArgs e)
        {
            var handler = QueueStarted;
            handler?.Invoke(this, e);
        }

        private void EncodeCompleted(object sender, EncodeCompletedEventArgs args)
        {
            _finishedSteps++;

            _currentEncoder.EncodeCompleted -= EncodeCompleted;
            _currentEncoder.EncodeStarted -= EncodeStarted;
            _currentEncoder.EncodeStatusChanged -= EncoderProgressStatus;
            _currentEncoder = null;

            DeleteTempFiles();
            if (_currentJob != null && _currentJob.ExitCode == 0)
            {
                InvokeQueueStatusChanged(new QueueProgressEventArgs
                {
                    JobName = string.Empty,
                    AverageFrameRate = 0f,
                    CurrentFrameRate = 0f,
                    CurrentFrame = 0,
                    TotalFrames = 0,
                    ElapsedTime = new TimeSpan(),
                    EstimatedTimeLeft = new TimeSpan(),
                    PercentComplete = 0,
                    TotalPercentComplete = (_finishedSteps * _fullTaskPercent),
                    Pass = 0,
                });
            
                GetNextStep();

                if (_currentJob.NextStep == EncodingStep.Done &&
                    _queueList.IndexOf(_currentJob) < _queueList.Count - 1)
                {
                    _currentJob = GetNextJob();
                    GetNextStep();
                }
                else if (_currentJob.NextStep == EncodingStep.Done &&
                         _queueList.IndexOf(_currentJob) == _queueList.Count - 1)
                {
                    InvokeQueueCompleted(new QueueCompletedEventArgs(true,null,string.Empty));
                    return;
                }

                ExecuteNextStep();
            }
            else
            {
                var currentJob = _currentJob;
                var exitCode = -1;
                if (currentJob != null)
                    exitCode = currentJob.ExitCode;
                InvokeQueueCompleted(new QueueCompletedEventArgs(false,
                    new ApplicationException("Encoder exited with code " + exitCode),
                    "Encoder exited with code " + exitCode));
            }
        }

        private void EncoderProgressStatus(object sender, EncodeProgressEventArgs args)
        {
            var totalPercent = (_finishedSteps * _fullTaskPercent) +
                                  (_fullTaskPercent * args.PercentComplete / 100d);
            
            InvokeQueueStatusChanged(new QueueProgressEventArgs
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
            InvokeQueueStatusChanged(new QueueProgressEventArgs
            {
                JobName = _currentJob.JobName,
                AverageFrameRate = 0f,
                CurrentFrameRate = 0f,
                CurrentFrame = 0,
                TotalFrames = 0,
                ElapsedTime = new TimeSpan(),
                EstimatedTimeLeft = new TimeSpan(),
                PercentComplete = 0,
                TotalPercentComplete = (_finishedSteps * _fullTaskPercent),
                Pass = _currentJob.StreamId,
            });
        }

        /// <summary>
        /// Stops queue processing
        /// </summary>
        public void Stop()
        {
            _currentEncoder?.Stop();
        }

        /// <summary>
        /// Starts queue processing
        /// </summary>
        /// <param name="queue"></param>
        public async void StartProcessing(ObservableCollection<EncodeInfo> queue)
        {
            InvokeQueueStarted(EventArgs.Empty);

            _queueList = queue;
            _processingSteps = await Task.Run(() => CountProcessingSteps());
            _fullTaskPercent = 100f / _processingSteps;

            try
            {
                _currentJob = GetNextJob();
            }
            catch (Exception ex)
            {
                _currentJob = null;
                InvokeQueueCompleted(new QueueCompletedEventArgs(false, ex, ex.Message)); 
                return;
            }

            _finishedSteps = 0;

            GetNextStep();

            ExecuteNextStep();
        }

        private int CountProcessingSteps()
        {
            var encodingSteps = 0;

            foreach (var job in _queueList)
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

                if (_appConfig.CreateXbmcInfoFile && (job.MovieInfo != null || job.EpisodeInfo != null))
                    encodingSteps++; // create xbmc info files
            }   // foreach job

            return encodingSteps;
        }

        private EncodeInfo GetNextJob()
        {
            return _queueList.First(info => info.NextStep == EncodingStep.NotSet);
        }

        private void GetNextStep()
        {
            int nextIndex;
            switch (_currentJob.NextStep)
            {
                case EncodingStep.NotSet:
                    switch (_currentJob.Input)
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
                            _currentJob.NextStep = !string.IsNullOrEmpty(_currentJob.TempInput)
                                                        ? EncodingStep.CopyTempFile
                                                        : EncodingStep.Demux;
                            break;
                        case InputType.InputDvd:
                            _currentJob.NextStep = EncodingStep.Dump;
                            break;
                        default:
                            _currentJob.NextStep = EncodingStep.Demux;
                            break;
                    }
                    _currentJob.ExitCode = 0;
                    break;

                case EncodingStep.CopyTempFile:
                case EncodingStep.Dump:
                    _currentJob.NextStep = EncodingStep.Demux;
                    break;

                case EncodingStep.Demux:
                    // if output format for audio is other than copy or 
                    // output is set to copy but the encoding profile is set to output dvd
                    //
                    // and if there are actually some audio streams
                    //
                    // then the next step is to encode the audio streams
                    if ((_currentJob.AudioProfile.Type != ProfileType.Copy ||
                         (_currentJob.AudioProfile.Type == ProfileType.Copy &&
                          _currentJob.EncodingProfile.OutFormat == OutputType.OutputDvd)) &&
                        _currentJob.AudioStreams.Count > 0)
                    {
                        _currentJob.NextStep = EncodingStep.EncodeAudio;
                        _currentJob.StreamId = 0;
                    }
                    else
                        GetSubOrVideoStep();
                    break;

                case EncodingStep.EncodeAudio:
                    if (_currentJob.AudioStreams.Count - 1 > _currentJob.StreamId)
                        _currentJob.StreamId++;
                    else
                        GetSubOrVideoStep();
                    break;

                case EncodingStep.IndexVideo:
                    if (_currentJob.EncodingProfile.AutoCropResize &&
                        !_currentJob.EncodingProfile.KeepInputResolution &&
                        _currentJob.EncodingProfile.OutFormat != OutputType.OutputDvd)
                        _currentJob.NextStep = EncodingStep.GetCropRect;
                    else
                    {
                        _currentJob.NextStep = EncodingStep.EncodeVideo;
                        _currentJob.StreamId = 1;
                    }
                    break;

                case EncodingStep.GetCropRect:
                    _currentJob.NextStep = EncodingStep.EncodeVideo;
                    _currentJob.StreamId = 1;
                    break;

                case EncodingStep.EncodeVideo:
                    var encodingPasses = 1;
                    switch (_currentJob.VideoProfile.Type)
                    {
                        case ProfileType.X264:
                            switch (((X264Profile)_currentJob.VideoProfile).EncodingMode)
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
                            encodingPasses += ((Vp8Profile)_currentJob.VideoProfile).EncodingMode;
                            break;
                    }
                    if (_currentJob.StreamId < encodingPasses)
                        _currentJob.StreamId++;
                    else
                        GetSubOrVideoStep();
                    break;

                case EncodingStep.DemuxSubtitle:
                    nextIndex = _currentJob.SubtitleStreams.FindIndex(_currentJob.StreamId, info => info.RawStream == false);

                    if (_currentJob.SubtitleStreams.Count - 1 > _currentJob.StreamId && nextIndex > -1)
                        _currentJob.StreamId = nextIndex;
                    else
                        GetSubOrVideoStep();
                    break;
                case EncodingStep.ProcessSubtitle:
                    nextIndex = _currentJob.SubtitleStreams.FindIndex(_currentJob.StreamId, info => info.NeedConversion);
                    if (_currentJob.SubtitleStreams.Count - 1 > _currentJob.StreamId &&
                        _appConfig.JavaInstalled && _appConfig.BDSup2SubInstalled && nextIndex > -1)
                        _currentJob.StreamId = nextIndex;
                    else
                        GetSubOrVideoStep();
                    break;

                case EncodingStep.PreMuxResult:
                    if (_currentJob.SubtitleStreams.Count > 0)
                    {
                        _currentJob.NextStep = EncodingStep.PremuxSubtitle;
                        _currentJob.StreamId = 0;
                    }
                    else
                        _currentJob.NextStep = EncodingStep.MuxResult;
                    break;

                case EncodingStep.PremuxSubtitle:
                    if (_currentJob.StreamId < _currentJob.SubtitleStreams.Count - 1)
                        _currentJob.StreamId++;
                    else
                        _currentJob.NextStep = EncodingStep.MuxResult;
                    break;

                case EncodingStep.MuxResult:
                    if (string.IsNullOrEmpty(_currentJob.TempOutput))
                        if (_appConfig.CreateXbmcInfoFile &&
                            (_currentJob.MovieInfo != null || _currentJob.EpisodeInfo != null))
                            _currentJob.NextStep = EncodingStep.WriteInfoFile;
                        else
                            _currentJob.NextStep = EncodingStep.Done;
                    else
                        _currentJob.NextStep = EncodingStep.MoveOutFile;
                    break;

                case EncodingStep.MoveOutFile:
                    if (_appConfig.CreateXbmcInfoFile && (_currentJob.MovieInfo != null || _currentJob.EpisodeInfo != null))
                        _currentJob.NextStep = EncodingStep.WriteInfoFile;
                    else
                        _currentJob.NextStep = EncodingStep.Done;
                    break;

                case EncodingStep.WriteInfoFile:
                    _currentJob.NextStep = EncodingStep.Done;
                    break;
            }
        }

        private void GetSubOrVideoStep()
        {
            if (_currentJob.VideoStream != null)
            {
                switch (_currentJob.CompletedStep)
                {
                    case EncodingStep.Demux:
                    case EncodingStep.EncodeAudio:
                    case EncodingStep.EncodeVideo:
                    case EncodingStep.DemuxSubtitle:
                        var sub = _currentJob.SubtitleStreams.FirstOrDefault(subInfo => subInfo.NeedConversion);
                        var demuxSubtitleIndex = _currentJob.SubtitleStreams.FindIndex(info => info.RawStream == false);

                        if (_currentJob.VideoProfile.Type != ProfileType.Copy && !_currentJob.VideoStream.Encoded)
                            _currentJob.NextStep = EncodingStep.IndexVideo;
                        else if (demuxSubtitleIndex > -1)
                        {
                            _currentJob.NextStep = EncodingStep.DemuxSubtitle;
                            _currentJob.StreamId = demuxSubtitleIndex;
                        }
                        else if (((_appConfig.JavaInstalled && _appConfig.BDSup2SubInstalled) ||
                                  _currentJob.EncodingProfile.OutFormat == OutputType.OutputMp4) && sub != null)
                        {
                            _currentJob.NextStep = EncodingStep.ProcessSubtitle;
                            _currentJob.StreamId = _currentJob.SubtitleStreams.IndexOf(sub);
                        }
                        else if (_currentJob.EncodingProfile.OutFormat == OutputType.OutputDvd)
                            _currentJob.NextStep = EncodingStep.PreMuxResult;
                        else
                            _currentJob.NextStep = EncodingStep.MuxResult;
                        break;

                    case EncodingStep.ProcessSubtitle:
                        _currentJob.NextStep = _currentJob.EncodingProfile.OutFormat == OutputType.OutputDvd
                                                                                                ? EncodingStep.PreMuxResult
                                                                                                : EncodingStep.MuxResult;
                        break;
                }
            }
            else
            {
                _currentJob.NextStep = _currentJob.EncodingProfile.OutFormat == OutputType.OutputDvd
                    ? EncodingStep.PreMuxResult
                    : EncodingStep.MuxResult;
            }
        }

        private void ExecuteNextStep()
        {
            switch (_currentJob.NextStep)
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
                    _currentJob.TempFiles.Add(_currentJob.TempInput);
                    _currentJob.TempFiles.Add(_currentJob.TempOutput);
                    DeleteTempFiles();
                    break;
            }
        }

        private void ExecuteGenericEncoder()
        {
            _currentEncoder.EncodeStarted += EncodeStarted;
            _currentEncoder.EncodeStatusChanged += EncoderProgressStatus;
            _currentEncoder.EncodeCompleted += EncodeCompleted;
            _currentEncoder.Start(_currentJob);
        }

        private void ExecuteCopyTempFile()
        {
            _currentEncoder = _fileWorker;

            ExecuteGenericEncoder();
        }

        private void ExecuteDump()
        {
            _currentEncoder = _mplayerDemuxer;

            ExecuteGenericEncoder();
        }

        private void ExecuteDemux()
        {
            switch (_currentJob.Input)
            {
                case InputType.InputBluRay:
                    _currentEncoder = _demuxerTsMuxeR;
                    break;
                case InputType.InputAvchd:
                case InputType.InputHddvd:
                    _currentEncoder = _eac3To;
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
                    _currentEncoder = _ffmpegDemuxer;
                    break;
            }

            ExecuteGenericEncoder();
        }

        private void ExecuteEncodeAudio()
        {
            switch (_currentJob.AudioProfile.Type)
            {
                case ProfileType.Ac3:
                    _currentEncoder = _ffmpegAc3;
                    break;
                case ProfileType.Ogg:
                    _currentEncoder = _oggEnc;
                    break;
                case ProfileType.Aac:
                    _currentEncoder = _neroAac;
                    break;
                case ProfileType.Mp3:
                    _currentEncoder = _lame;
                    break;
                case ProfileType.Copy:
                    if (_currentJob.EncodingProfile.OutFormat == OutputType.OutputDvd &&
                        !_processingService.CheckAudioDvdCompatible(_currentJob.AudioStreams[_currentJob.StreamId]))
                    {
                        _currentEncoder = _ffmpegAc3;
                    }
                    break;
            }

            ExecuteGenericEncoder();
        }

        private void ExecuteDemuxSubtitle()
        {
            _currentEncoder = _mkvExtractSubtitle;

            ExecuteGenericEncoder();
        }

        private void ExecuteProcessSubtitle()
        {
            switch (_currentJob.EncodingProfile.OutFormat)
            {
                case OutputType.OutputMp4:
                    // TODO: Text Subtitle converter
                    break;
                default:
                    _currentEncoder = _bdSup2Sub;
                    break;
            }

            ExecuteGenericEncoder();
        }

        private void ExecuteIndexVideo()
        {
            _currentEncoder = _ffmsIndex;

            ExecuteGenericEncoder();
        }

        private void ExecuteGetCropRect()
        {
            _currentEncoder = _ffmpegGetCrop;

            ExecuteGenericEncoder();
        }

        private void ExecuteEncodeVideo()
        {
            switch (_currentJob.VideoProfile.Type)
            {
                case ProfileType.X264:
                    // TODO: Switch encoder based on settings
                    //this._currentEncoder = this._x264;
                    _currentEncoder = _ffmpegX264;
                    break;
                case ProfileType.Mpeg2Video:
                    _currentEncoder = _ffmpegDvd;
                    break;
                case ProfileType.Vp8:
                    // TODO: VPXEnc
                    break;
            }

            ExecuteGenericEncoder();
        }

        private void ExecutePremuxResult()
        {
            _currentEncoder = _mplex;

            ExecuteGenericEncoder();
        }

        private void ExecutePremuxSubtitle()
        {
            _currentEncoder = _spuMux;

            ExecuteGenericEncoder();
        }

        private void ExecuteMuxResult()
        {
            switch (_currentJob.EncodingProfile.OutFormat)
            {
                case OutputType.OutputMatroska:
                case OutputType.OutputWebM:
                    _currentEncoder = _mkvMerge;
                    break;
                case OutputType.OutputMp4:
                    _currentEncoder = _mp4Box;
                    break;
                case OutputType.OutputTs:
                case OutputType.OutputM2Ts:
                case OutputType.OutputBluRay:
                case OutputType.OutputAvchd:
                    _currentEncoder = _tsMuxeR;
                    break;
                case OutputType.OutputDvd:
                    _currentEncoder = _dvdAuthor;
                    break;
            }

            ExecuteGenericEncoder();
        }

        private void ExecuteMoveOutFile()
        {
            _currentEncoder = _fileWorker;

            ExecuteGenericEncoder();
        }

        private void ExecuteWriteInfoFile()
        {
            // TODO: InfoWriter
            ExecuteGenericEncoder();
        }

        private void DeleteTempFiles()
        {
            if (!_appConfig.DeleteTemporaryFiles) return;

            foreach (var tempFile in _currentJob.TempFiles)
            {
                if (tempFile == _currentJob.InputFile || tempFile == _currentJob.OutputFile ||
                    string.IsNullOrEmpty(tempFile))
                    continue;

                var fi = new FileInfo(tempFile).Attributes;

                try
                {
                    Log.Info($"Deleting \"{tempFile}\"");
                    if (fi == FileAttributes.Directory)
                        Directory.Delete(tempFile);
                    else
                        File.Delete(tempFile);
                }
                catch (Exception exc)
                {
                    Log.Error($"Could not delete File \"{tempFile}\" -> {exc.Message}");
                }
            }
            _currentJob.TempFiles.Clear();
        }
    }
}
