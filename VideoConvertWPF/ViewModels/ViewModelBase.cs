// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModelBase.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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

    public class ViewModelBase : Screen, IViewModelBase
    {
        private string _title;
        private bool _updateAvail;
        private bool _hasLoaded;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyOfPropertyChange(nameof(Title));
            }
        }

        public bool UpdateAvail
        {
            get
            {
                return _updateAvail;
            }
            set
            {
                _updateAvail = value;
                NotifyOfPropertyChange(nameof(UpdateAvail));
            }
        }

        public IWindowManager WindowManager { get; set; }

        public void Load()
        {
            if (_hasLoaded) return;
            _hasLoaded = true;
            OnLoad();
        }

        public virtual void OnLoad()
        {
            
        }
    }
}
