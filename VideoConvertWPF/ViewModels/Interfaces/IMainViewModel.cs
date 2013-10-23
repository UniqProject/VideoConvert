using System.Collections.ObjectModel;

namespace VideoConvertWPF.ViewModels.Interfaces
{
    using VideoConvert.Interop.Model;

    public interface IMainViewModel
    {
        ObservableCollection<EncodeInfo> JobCollection { get; set; }
        void CheckUpdate();
        void Shutdown();
    }
}