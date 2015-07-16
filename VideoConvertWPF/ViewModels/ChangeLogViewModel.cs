// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeLogViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvertWPF.ViewModels
{
    using System.ComponentModel;
    using System.IO;
    using Caliburn.Micro;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvertWPF.ViewModels.Interfaces;

    public class ChangeLogViewModel : ViewModelBase, IChangeLogViewModel
    {
        private readonly IShellViewModel _shellViewModel;

        public IAppConfigService ConfigService { get; set; }

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
                NotifyOfPropertyChange(() => LangCode);
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
                NotifyOfPropertyChange(() => ChangeLogText);
            }
        }

        public ChangeLogViewModel(IShellViewModel shellViewModel, IWindowManager windowManager)
        {
            _shellViewModel = shellViewModel;
            WindowManager = windowManager;
            PropertyChanged += OnPropertyChanged;
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
            var localizedFileName = Path.ChangeExtension("CHANGELOG", LangCode);
            using (TextReader reader = new StreamReader(Path.Combine(ConfigService.AppPath, localizedFileName)))
            {
                ChangeLogText = reader.ReadToEnd();
            }
        }

        public override void OnLoad()
        {
            base.OnLoad();
            LangCode = "de-DE";
        }

        public void Close()
        {
            _shellViewModel.DisplayWindow(ShellWin.MainView);
        }
    }
}