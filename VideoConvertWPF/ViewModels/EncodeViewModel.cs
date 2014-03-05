namespace VideoConvertWPF.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using Caliburn.Micro;
    using Interfaces;
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
            Abort();
        }

        private void QueueProgressChanged(object sender, QueueProgressEventArgs args)
        {
            Type senderType;
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
                    
            }

            this.ProgressValue = args.PercentComplete;
            this.TotalProgressValue = args.TotalPercentComplete;

            if (!string.IsNullOrEmpty(args.JobName))
            {
                Execute.OnUIThread(() =>    LogEntries.Add(new LogEntry
                                            {
                                                EntryTime = DateTime.Now,
                                                JobName = args.JobName,
                                                LogText = "Processing started"
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
