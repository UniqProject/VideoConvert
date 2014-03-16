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

        public EncodeViewModel(IShellViewModel shellViewModel, IWindowManager windowManager, IQueueProcessor queueProcessor)
        {
            this._shellViewModel = shellViewModel;
            this._queueProcessor = queueProcessor;
            this.WindowManager = windowManager;
            this.WindowTitle = "Encoding";
        }

        public void Abort()
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
            Abort();
        }

        private void QueueProgressChanged(object sender, QueueProgressEventArgs args)
        {
            Type senderType;
            string processingTool = string.Empty;
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
                    break;
                case "DecoderFfmsIndex":
                    processingTool = "Video: Indexing...";
                    break;
                case "DemuxerEac3To":
                case "DemuxerFfmpeg":
                case "DemuxerMplayer":
                    processingTool = "Source: Demultiplexing Streams...";
                    break;
                case "EncoderBdSup2Sub":
                    processingTool = "Subtitle: Processing captions...";
                    break;
                case "EncoderFfmpegAc3":
                    processingTool = "Audio: Encoding to AC-3...";
                    break;
                case "EncoderFfmpegDvd":
                    processingTool = "Video: Encoding DVD Compliant stream...";
                    break;
                case "EncoderLame":
                    processingTool = "Audio: Encoding to MP3...";
                    break;
                case "EncoderNeroAac":
                    processingTool = "Audio: Encoding to AAC...";
                    break;
                case "EncoderOggEnc":
                    processingTool = "Audio: Encoding to OGG...";
                    break;
                case "EncoderX264":
                    processingTool = "Video: Encoding to h.264...";
                    break;
                case "FileWorker":
                    processingTool = "Copying / Moving Files...";
                    break;
                case "MuxerDvdAuthor":
                    processingTool = "Output: Authoring DVD Structure...";
                    break;
                case "MuxerMkvMerge":
                    processingTool = "Output: Multiplexing Streams to MKV (Matroska) File...";
                    break;
                case "MuxerMp4Box":
                    processingTool = "Output: Multiplexing Streams to MP4 File...";
                    break;
                case "MuxerMplex":
                    processingTool = "Output: Multiplexing Media Streams to MPEG Package...";
                    break;
                case "MuxerSpuMux":
                    processingTool = "Output: Multiplexing Subtitle Stream into MPEG Package...";
                    break;
            }

            this.ProgressValue = args.PercentComplete;
            this.TotalProgressValue = args.TotalPercentComplete;

            if (!string.IsNullOrEmpty(args.JobName))
            {
                Execute.OnUIThread(() => LogEntries.Add(new LogEntry
                                                        {
                                                            EntryTime = DateTime.Now,
                                                            JobName = args.JobName,
                                                            LogText = string.IsNullOrEmpty(processingTool) 
                                                                        ? "Processing started" 
                                                                        : processingTool
                                                        }));

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
