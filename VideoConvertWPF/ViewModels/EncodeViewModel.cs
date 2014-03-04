namespace VideoConvertWPF.ViewModels
{
    using System.Collections.ObjectModel;
    using Caliburn.Micro;
    using Interfaces;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Model;

    public class EncodeViewModel : ViewModelBase, IEncodeViewModel
    {
        private readonly IShellViewModel _shellViewModel;
        private readonly ObservableCollection<LogEntry> _logEntries = new ObservableCollection<LogEntry>();
        private readonly IQueueProcessor _queueProcessor;

        public ObservableCollection<EncodeInfo> JobCollection { get; set; }

        public string WindowTitle
        {
            get { return this.Title; }
            set
            {
                this.Title = value;
                this.NotifyOfPropertyChange(() => this.Title);
            }
        }

        public ObservableCollection<LogEntry> LogEntries { get { return _logEntries; } }

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
            this._queueProcessor.StartProcessing(this.JobCollection);
        }
    }
}
