// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeLogViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System.ComponentModel;
using System.IO;
using Caliburn.Micro;
using VideoConvertWPF.ViewModels.Interfaces;
using ILog = log4net.ILog;
using LogManager = log4net.LogManager;

namespace VideoConvertWPF.ViewModels
{
    using VideoConvert.AppServices.Services.Interfaces;

    public class ChangeLogViewModel : ViewModelBase, IChangeLogViewModel
    {
        private readonly IShellViewModel _shellViewModel;

        public IAppConfigService ConfigService { get; set; }

        private static readonly ILog Log = LogManager.GetLogger(typeof(MainViewModel));
        private string _langCode;
        private string _changeLogText;

        public string LangCode
        {
            get
            {
                return _shellViewModel.LangCode;
            }
            set
            {
                _shellViewModel.LangCode = value;
                this.NotifyOfPropertyChange(() => this.LangCode);
            }
        }

        public string ChangeLogText
        {
            get
            {
                return _changeLogText;
            }
            set
            {
                _changeLogText = value;
                this.NotifyOfPropertyChange(() => this.ChangeLogText);
            }
        }

        public ChangeLogViewModel(IShellViewModel shellViewModel, IWindowManager windowManager)
        {
            this._shellViewModel = shellViewModel;
            this.WindowManager = windowManager;
            this.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "LangCode")
            {
                ReloadChangeLogText();
            }
        }

        private void ReloadChangeLogText()
        {
            string localizedFileName = Path.ChangeExtension("CHANGELOG", this.LangCode);
            using (TextReader reader = new StreamReader(Path.Combine(ConfigService.AppPath, localizedFileName)))
            {
                this.ChangeLogText = reader.ReadToEnd();
            }
        }

        public override void OnLoad()
        {
            base.OnLoad();
            this.LangCode = "de-DE";
        }

        public void Close()
        {
            this._shellViewModel.DisplayWindow(ShellWin.MainView);
        }
    }
}