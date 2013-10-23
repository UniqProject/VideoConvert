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
    using Interfaces;

    public class AboutViewModel : ViewModelBase, IAboutViewModel
    {
        private readonly IShellViewModel _shellViewModel;

        public string WindowTitle
        {
            get { return this.Title; }
            set
            {
                this.Title = value;
                this.NotifyOfPropertyChange(() => this.Title);
            }
        }

        public AboutViewModel(IShellViewModel shellViewModel, IWindowManager windowManager)
        {
            this._shellViewModel = shellViewModel;
            this.WindowManager = windowManager;
            this.WindowTitle = "About";
        }

        public void Close()
        {
            this._shellViewModel.DisplayWindow(ShellWin.LastView);
        }
    }
}