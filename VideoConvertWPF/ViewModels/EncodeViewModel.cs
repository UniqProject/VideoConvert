namespace VideoConvertWPF.ViewModels
{
    using Caliburn.Micro;
    using Interfaces;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;

    public class EncodeViewModel : ViewModelBase, IEncodeViewModel
    {
        private readonly IShellViewModel _shellViewModel;
        private readonly ObservableCollection<LogEntry> _logEntries = new ObservableCollection<LogEntry>();
        private readonly IQueueProcessor _queueProcessor;
        private IAppConfigService _appConfig;
        
        private double _progressValue;
        private double _totalProgressValue;
        private string _jobStatus;

        public ObservableCollection<EncodeInfo> JobCollection { get; set; }

        public ObservableCollection<LogEntry> LogEntries { get { return _logEntries; } }

        public string WindowTitle
        {
            get { return this.Title; }
            set
            {
                this.Title = value;
                this.NotifyOfPropertyChange(() => this.Title);
            }
        }

        public double ProgressValue
        {
            get { return this._progressValue; }
            set
            {
                this._progressValue = value;
                this.NotifyOfPropertyChange(() => this.ProgressValue);
            }
        }

        public double TotalProgressValue
        {
            get { return this._totalProgressValue; }
            set
            {
                this._totalProgressValue = value;
                this.NotifyOfPropertyChange(() => this.TotalProgressValue);
            }
        }

        public string JobStatus
        {
            get { return this._jobStatus; }
            set
            {
                this._jobStatus = value;
                this.NotifyOfPropertyChange(() => this.JobStatus);
            }
        }

        public EncodeViewModel(IShellViewModel shellViewModel, IWindowManager windowManager, IQueueProcessor queueProcessor, IAppConfigService appConfig)
        {
            this._shellViewModel = shellViewModel;
            this._queueProcessor = queueProcessor;
            this._appConfig = appConfig;
            this.WindowManager = windowManager;
            this.WindowTitle = "Encoding";
        }

        public void Abort()
        {
            if (this._queueProcessor != null)
                this._queueProcessor.Stop();
        }

        private void Close()
        {
            this._shellViewModel.DisplayWindow(ShellWin.LastView);
        }

        public void StartEncode(ObservableCollection<EncodeInfo> jobCollection)
        {
            this.JobCollection = jobCollection;
            this._queueProcessor.QueueStarted += QueueStarted;
            this._queueProcessor.QueueProgressChanged += QueueProgressChanged;
            this._queueProcessor.QueueCompleted += QueueCompleted;
            this._queueProcessor.StartProcessing(this.JobCollection);
        }

        private void QueueCompleted(object sender, QueueCompletedEventArgs args)
        {
            this._queueProcessor.QueueCompleted -= QueueCompleted;
            this._queueProcessor.QueueProgressChanged -= QueueProgressChanged;
            this._queueProcessor.QueueStarted -= QueueStarted;

            Execute.OnUIThread(() =>
            {
                var finishedList = JobCollection.Where(encodeInfo => encodeInfo.NextStep == EncodingStep.Done).ToList();
                foreach (var encodeInfo in finishedList)
                {
                    this.JobCollection.Remove(encodeInfo);
                }
            }); 
            Close();
        }

        private void QueueProgressChanged(object sender, QueueProgressEventArgs args)
        {
            Type senderType;
            string processingTool = string.Empty;
            string progress = string.Empty;
            string elapsedTime = new DateTime(args.ElapsedTime.Ticks).ToString("HH:mm:ss");
            string eta = new DateTime(args.EstimatedTimeLeft.Ticks).ToString("HH:mm:ss");

            try
            {
                senderType = sender.GetType();
            }
            catch (Exception)
            {
                senderType = new Object().GetType();
            }
            switch (senderType.Name)
            {
                case "DecoderFfmpegGetCrop":
                    processingTool = "Video: Calculating Crop Rectangle...";
                    progress = string.Format("Calculating Crop Rectangle: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "DecoderFfmsIndex":
                    processingTool = "Video: Indexing...";
                    progress = string.Format("Indexing Video Stream: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "DemuxerEac3To":
                case "DemuxerFfmpeg":
                case "DemuxerMplayer":
                case "DemuxerTsMuxeR":
                    processingTool = "Source: Demultiplexing Streams...";
                    progress = string.Format("Demultiplexing Streams: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "EncoderBdSup2Sub":
                    processingTool = "Subtitle: Processing captions...";
                    progress = string.Format("Processing Subtitle Captions: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "EncoderFfmpegAc3":
                    processingTool = "Audio: Encoding to AC-3...";
                    progress = string.Format("Encoding Stream to AC-3: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "EncoderFfmpegDvd":
                    processingTool = string.Format("Video: Encoding DVD Compliant stream, encoding pass {0:0}...", args.Pass);
                    progress = string.Format(this._appConfig.CInfo,
                                             "Encoding Video Stream: {0,3:0}%, {1,6:0} / {2,6:0} Frames, "
                                             + "{3,5:0.0} FPS, " + "Time elapsed: {4}, ETA: {5}",
                                             args.PercentComplete, args.CurrentFrame, args.TotalFrames, 
                                             args.AverageFrameRate, elapsedTime, eta);
                    break;
                case "EncoderLame":
                    processingTool = "Audio: Encoding to MP3...";
                    progress = string.Format("Encoding Stream to MP3: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "EncoderNeroAac":
                    processingTool = "Audio: Encoding to AAC...";
                    progress = string.Format("Encoding Stream to AAC: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "EncoderOggEnc":
                    processingTool = "Audio: Encoding to OGG...";
                    progress = string.Format("Encoding Stream to OGG: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "EncoderX264":
                case "EncoderFfmpegX264":
                    processingTool = string.Format("Video: Encoding to h.264, encoding pass {0:0}...", args.Pass);
                    progress = string.Format(this._appConfig.CInfo,
                                             "Encoding Video Stream: {0,3:0}%, {1,6:0} / {2,6:0} Frames, "
                                             + "{3,5:0.0} FPS, " + "Time elapsed: {4}, ETA: {5}",
                                             args.PercentComplete, args.CurrentFrame, args.TotalFrames, 
                                             args.AverageFrameRate, elapsedTime, eta);
                    break;
                case "FileWorker":
                    processingTool = "Copying / Moving Files...";
                    progress = string.Format("Copying / Moving Files: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "MuxerDvdAuthor":
                    processingTool = "Output: Authoring DVD Structure...";
                    progress = string.Format("Authoring DVD Structure: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "MuxerMkvMerge":
                    processingTool = "Output: Multiplexing Streams to MKV (Matroska) File...";
                    progress = string.Format("Multiplexing Output File: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "MuxerMp4Box":
                    processingTool = "Output: Multiplexing Streams to MP4 File...";
                    progress = string.Format("Multiplexing Output File: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "MuxerMplex":
                    processingTool = "Output: Multiplexing Media Streams to MPEG Package...";
                    progress = string.Format("Multiplexing MPEG Package: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
                case "MuxerSpuMux":
                    processingTool = "Output: Multiplexing Subtitle Stream into MPEG Package...";
                    progress = string.Format("Multiplexing Subtitle Stream into MPEG Package: {0,3:0}%, Time elapsed: {1}, ETA: {2}",
                                             args.PercentComplete, elapsedTime, eta);
                    break;
            }

            this.ProgressValue = args.PercentComplete;
            this.TotalProgressValue = args.TotalPercentComplete;
            this.JobStatus = progress;

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
