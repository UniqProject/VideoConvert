// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMainViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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

    public interface IMainViewModel
    {
        ObservableCollection<EncodeInfo> JobCollection { get; set; }
        void CheckUpdate();
        void Shutdown();
    }
}