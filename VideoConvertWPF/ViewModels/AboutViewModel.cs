// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AboutViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvertWPF.ViewModels
{
    using Caliburn.Micro;
    using VideoConvertWPF.ViewModels.Interfaces;

    public class AboutViewModel : ViewModelBase, IAboutViewModel
    {
        private readonly IShellViewModel _shellViewModel;

        public string WindowTitle
        {
            get { return Title; }
            set
            {
                Title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public AboutViewModel(IShellViewModel shellViewModel, IWindowManager windowManager)
        {
            _shellViewModel = shellViewModel;
            WindowManager = windowManager;
            WindowTitle = "About";
        }

        public void Close()
        {
            _shellViewModel.DisplayWindow(ShellWin.LastView);
        }
    }
}