// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEncodeViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------
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