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
    using Interfaces;

    public class ViewModelBase : Screen, IViewModelBase
    {
        private string _title;
        private bool _updateAvail;
        private bool _hasLoaded;

        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
                this.NotifyOfPropertyChange("Title");
            }
        }

        public bool UpdateAvail
        {
            get
            {
                return this._updateAvail;
            }
            set
            {
                this._updateAvail = value;
                this.NotifyOfPropertyChange("UpdateAvail");
            }
        }

        public IWindowManager WindowManager { get; set; }

        public void Load()
        {
            if (!this._hasLoaded)
            {
                this._hasLoaded = true;
                this.OnLoad();
            }
        }

        public virtual void OnLoad()
        {
            
        }
    }
}
