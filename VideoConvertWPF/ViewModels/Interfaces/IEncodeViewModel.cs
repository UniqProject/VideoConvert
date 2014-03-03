namespace VideoConvertWPF.ViewModels.Interfaces
{
    using System.Collections.ObjectModel;
    using VideoConvert.Interop.Model;

    public interface IEncodeViewModel
    {
        ObservableCollection<EncodeInfo> JobCollection { get; set; }
        ObservableCollection<LogEntry> LogEntries { get; }
        void Abort();
        void StartEncode(ObservableCollection<EncodeInfo> jobCollection);
    }
}