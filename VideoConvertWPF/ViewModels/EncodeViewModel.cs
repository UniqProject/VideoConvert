// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodeViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VideoConvertWPF.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Caliburn.Micro;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;
    using VideoConvertWPF.ViewModels.Interfaces;

    public class EncodeViewModel : ViewModelBase, IEncodeViewModel
    {
        private readonly IShellViewModel _shellViewModel;
        private readonly IQueueProcessor _queueProcessor;
        private readonly IAppConfigService _appConfig;
        
        private double _progressValue;
        private double _totalProgressValue;
        private string _jobStatus;

        public ObservableCollection<EncodeInfo> JobCollection { get; set; }

        public ObservableCollection<LogEntry> LogEntries { get; } = new ObservableCollection<LogEntry>();

        public string WindowTitle
        {
            get { return Title; }
            set
            {
                Title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                NotifyOfPropertyChange(() => ProgressValue);
            }
        }

        public double TotalProgressValue
        {
            get { return _totalProgressValue; }
            set
            {
                _totalProgressValue = value;
                NotifyOfPropertyChange(() => TotalProgressValue);
            }
        }

        public string JobStatus
        {
            get { return _jobStatus; }
            set
            {
                _jobStatus = value;
                NotifyOfPropertyChange(() => JobStatus);
            }
        }

        public EncodeViewModel(IShellViewModel shellViewModel, IWindowManager windowManager, IQueueProcessor queueProcessor, IAppConfigService appConfig)
        {
            _shellViewModel = shellViewModel;
            _queueProcessor = queueProcessor;
            _appConfig = appConfig;
            WindowManager = windowManager;
            WindowTitle = "Encoding";
        }

        public void Abort()
        {
            _queueProcessor?.Stop();
        }

        private void Close()
        {
            _shellViewModel.DisplayWindow(ShellWin.LastView);
        }

        public void StartEncode(ObservableCollection<EncodeInfo> jobCollection)
        {
            JobCollection = jobCollection;
            _queueProcessor.QueueStarted += QueueStarted;
            _queueProcessor.QueueProgressChanged += QueueProgressChanged;
            _queueProcessor.QueueCompleted += QueueCompleted;
            _queueProcessor.StartProcessing(JobCollection);
        }

        private void QueueCompleted(object sender, QueueCompletedEventArgs args)
        {
            _queueProcessor.QueueCompleted -= QueueCompleted;
            _queueProcessor.QueueProgressChanged -= QueueProgressChanged;
            _queueProcessor.QueueStarted -= QueueStarted;

            Execute.OnUIThread(() =>
            {
                var finishedList = JobCollection.Where(encodeInfo => encodeInfo.NextStep == EncodingStep.Done).ToList();
                foreach (var encodeInfo in finishedList)
                {
                    JobCollection.Remove(encodeInfo);
                }
            }); 
            Close();
        }

        private void QueueProgressChanged(object sender, QueueProgressEventArgs args)
        {
            Type senderType;
            var processingTool = string.Empty;
            var progress = string.Empty;
            var elapsedTime = new DateTime(args.ElapsedTime.Ticks).ToString("HH:mm:ss");
            var eta = new DateTime(args.EstimatedTimeLeft.Ticks).ToString("HH:mm:ss");

            try
            {
                senderType = sender.GetType();
            }
            catch (Exception)
            {
                senderType = new object().GetType();
            }
            switch (senderType.Name)
            {
                case "DecoderFfmpegGetCrop":
                    processingTool = "Video: Calculating Crop Rectangle...";
                    progress = $"Calculating Crop Rectangle: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "DecoderFfmsIndex":
                    processingTool = "Video: Indexing...";
                    progress = $"Indexing Video Stream: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "DemuxerEac3To":
                case "DemuxerFfmpeg":
                case "DemuxerMplayer":
                case "DemuxerTsMuxeR":
                    processingTool = "Source: Demultiplexing Streams...";
                    progress = $"Demultiplexing Streams: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "EncoderBdSup2Sub":
                    processingTool = "Subtitle: Processing captions...";
                    progress = $"Processing Subtitle Captions: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "EncoderFfmpegAc3":
                    processingTool = "Audio: Encoding to AC-3...";
                    progress = $"Encoding Stream to AC-3: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "EncoderFfmpegDvd":
                    processingTool = $"Video: Encoding DVD Compliant stream, encoding pass {args.Pass:0}...";
                    progress =  $"Encoding Video Stream: {args.PercentComplete,3:0}%, {args.CurrentFrame,6:0} / {args.TotalFrames,6:0} Frames, ";
                    progress += $"{args.AverageFrameRate,5:0.0} FPS, ".ToString(_appConfig.CInfo);
                    progress += $"Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "EncoderLame":
                    processingTool = "Audio: Encoding to MP3...";
                    progress = $"Encoding Stream to MP3: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "EncoderNeroAac":
                case "EncoderQaac":
                    processingTool = "Audio: Encoding to AAC...";
                    progress = $"Encoding Stream to AAC: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "EncoderOggEnc":
                    processingTool = "Audio: Encoding to OGG...";
                    progress = $"Encoding Stream to OGG: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "EncoderX264":
                case "EncoderFfmpegX264":
                    processingTool = $"Video: Encoding to h.264, encoding pass {args.Pass:0}...";
                    progress =  $"Encoding Video Stream: {args.PercentComplete,3:0}%, {args.CurrentFrame,6:0} / {args.TotalFrames,6:0} Frames, ";
                    progress += $"{args.AverageFrameRate,5:0.0} FPS, ".ToString(_appConfig.CInfo);
                    progress += $"Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "FileWorker":
                    processingTool = "Copying / Moving Files...";
                    progress = $"Copying / Moving Files: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "MuxerDvdAuthor":
                    processingTool = "Output: Authoring DVD Structure...";
                    progress = $"Authoring DVD Structure: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "MuxerMkvMerge":
                    processingTool = "Output: Multiplexing Streams to MKV (Matroska) File...";
                    progress = $"Multiplexing Output File: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "MuxerMp4Box":
                    processingTool = "Output: Multiplexing Streams to MP4 File...";
                    progress = $"Multiplexing Output File: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "MuxerMplex":
                    processingTool = "Output: Multiplexing Media Streams to MPEG Package...";
                    progress = $"Multiplexing MPEG Package: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
                case "MuxerSpuMux":
                    processingTool = "Output: Multiplexing Subtitle Stream into MPEG Package...";
                    progress = $"Multiplexing Subtitle Stream into MPEG Package: {args.PercentComplete,3:0}%, Time elapsed: {elapsedTime}, ETA: {eta}";
                    break;
            }

            ProgressValue = args.PercentComplete;
            TotalProgressValue = args.TotalPercentComplete;
            JobStatus = progress;

            if (!string.IsNullOrEmpty(args.JobName))
            {
                Execute.OnUIThread(() => LogEntries.Insert(0, new LogEntry
                                                              {
                                                                  EntryTime = DateTime.Now,
                                                                  JobName = args.JobName,
                                                                  LogText = string.IsNullOrEmpty(processingTool)
                                                                            ? "Processing started"
                                                                            : processingTool
                                                              }
                                                          )
                                  );

            }
        }

        private void QueueStarted(object sender, EventArgs eventArgs)
        {
            //throw new NotImplementedException();
            LogEntries.Add(new LogEntry
            {
                EntryTime = DateTime.Now,
                JobName = "Queue",
                LogText = "Queue Started"
            });
        }
    }
}
