// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShellViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvertWPF.ViewModels.Interfaces
{
    using System.Collections.Generic;
    using VideoConvert.Interop.Model;

    public interface IShellViewModel
    {
        string LangCode { get; set; }

        void DisplayWindow(ShellWin window);
        void DisplayWindow(ShellWin window, EncodeInfo inputInfo);
        void DisplayWindow(ShellWin window, List<EncodeInfo> jobList);
        void DisplayWindow(ShellWin window, EncodeInfo inputInfo, List<EncodeInfo> jobList);

        bool CanClose();
    }
}
