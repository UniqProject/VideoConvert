// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvertWPF.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using log4net.Filter;
    using log4net.Layout;
    using VideoConvert.AppServices.Services;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Model;
    using VideoConvertWPF.ViewModels.Interfaces;

    [Export(typeof(IShellViewModel))]
    class ShellViewModel: ViewModelBase, IShellViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ShellViewModel));
        private static bool _clearLog;
        private bool _showOptions;
        private bool _showMainView;
        private bool _showEncode;
        private bool _showChangelog;
        private string _langCode;

        public IMainViewModel MainViewModel { get; set; }
        public IOptionsViewModel OptionsViewModel { get; set; }
        public IChangeLogViewModel ChangeLogViewModel { get; set; }
        public IAboutViewModel AboutViewModel { get; set; }
        public IEncodeViewModel EncodeViewModel { get; set; }

        private readonly IAppConfigService _configService;
        private readonly IProcessingService _processingService;
        private ShellWin _lastView;
        private bool _showAboutView;
        private ShellWin _actualView;

        public string LangCode
        {
            get
            {
                return _langCode;
            }
            set
            {
                _langCode = value;
                NotifyOfPropertyChange(() => LangCode);
            }
        }

        public bool ShowOptions
        {
            get
            {
                return _showOptions;
            }
            set
            {
                _showOptions = value;
                NotifyOfPropertyChange(() => ShowOptions);
            }
        }

        public bool ShowMainView
        {
            get
            {
                return _showMainView;
            }
            set
            {
                _showMainView = value;
                NotifyOfPropertyChange(() => ShowMainView);
            }
        }

        public bool ShowEncode
        {
            get
            {
                return _showEncode;
            }
            set
            {
                _showEncode = value;
                NotifyOfPropertyChange(() => ShowEncode);
            }
        }

        public bool ShowChangelog
        {
            get
            {
                return _showChangelog;
            }
            set
            {
                _showChangelog = value;
                NotifyOfPropertyChange(() => ShowChangelog);
            }
        }

        public bool ShowAboutView
        {
            get
            {
                return _showAboutView;
            }
            set
            {
                _showAboutView = value;
                NotifyOfPropertyChange(() => ShowAboutView);
            }
        }

        public string WindowTitle
        {
            get
            {
                return Title;
            }
            set
            {
                Title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public ShellWin LastView
        {
            get
            {
                return _lastView;
            }
            set
            {
                _lastView = value;
                NotifyOfPropertyChange(() => LastView);
            }
        }

        public ShellWin ActualView
        {
            get { return _actualView; }
            set
            {
                _actualView = value;
                NotifyOfPropertyChange(() => ActualView);
            }
        }

        public ShellViewModel(IAppConfigService config, IProcessingService processing)
        {
            _configService = config;
            _processingService = processing;
            
            DisplayWindow(ShellWin.MainView);
            Title = "Video Convert";

            _configService.PropertyChanged += ConfigServiceOnPropertyChanged;
        }

        private void ConfigServiceOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "UseDebug":
                    ReconfigureLogger();
                    break;
            }
        }

        public void DisplayWindow(ShellWin window)
        {
            DisplayWindow(window, null, null);
        }

        public void DisplayWindow(ShellWin window, EncodeInfo inputInfo)
        {
            DisplayWindow(window, inputInfo, null);
        }

        public void DisplayWindow(ShellWin window, ObservableCollection<EncodeInfo> jobList)
        {
            DisplayWindow(window, null, jobList);
        }

        public void DisplayWindow(ShellWin window, EncodeInfo inputInfo, ObservableCollection<EncodeInfo> jobList)
        {
            if (window == ShellWin.LastView)
                window = LastView;
            else
                if (window != ActualView)
                    LastView = ActualView;

            ActualView = window;

            switch (window)
            {
                case ShellWin.MainView:
                    ShowMainView = true;
                    ShowOptions = false;
                    ShowChangelog = false;
                    ShowAboutView = false;
                    ShowEncode = false;
                    MainViewModel?.CheckUpdate();
                    break;
                case ShellWin.OptionsView:
                    ShowOptions = true;
                    ShowMainView = false;
                    ShowChangelog = false;
                    ShowAboutView = false;
                    ShowEncode = false;
                    break;
                case ShellWin.ChangelogView:
                    ShowOptions = false;
                    ShowMainView = false;
                    ShowChangelog = true;
                    ShowAboutView = false;
                    ShowEncode = false;
                    break;
                case ShellWin.AboutView:
                    ShowOptions = false;
                    ShowMainView = false;
                    ShowChangelog = false;
                    ShowAboutView = true;
                    ShowEncode = false;
                    break;
                case ShellWin.EncodeView:
                    ShowOptions = false;
                    ShowMainView = false;
                    ShowChangelog = false;
                    ShowAboutView = false;
                    ShowEncode = true;
                    EncodeViewModel?.StartEncode(jobList);
                    break;
            }
        }

        public bool CanClose()
        {
            MainViewModel?.Shutdown();
            return true;
        }

        public override void OnLoad()
        {
            base.OnLoad();

            ReconfigureLogger();
        }

        internal void ReconfigureLogger()
        {
            var logFile = Path.Combine(_configService.AppSettingsPath, "ErrorLog_");

            if (Log.Logger.Repository.Configured)
            {
                Log.Logger.Repository.Shutdown();
                Log.Logger.Repository.ResetConfiguration();
            }

            if (_clearLog)
            {
                try
                {
                    File.Delete(logFile);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                _clearLog = false;
            }

            var layout = new XmlLayoutSchemaLog4j(true);

            var filter = new LevelRangeFilter
            {
                LevelMin = _configService.UseDebug ? Level.All : Level.Warn,
                AcceptOnMatch = true
            };

            layout.ActivateOptions();

            var fileAppender = new RollingFileAppender
            {
                PreserveLogFileNameExtension = true,
                StaticLogFileName = false,
                DatePattern = "yyyyMMdd'.xml'",
                RollingStyle = RollingFileAppender.RollingMode.Date,
                ImmediateFlush = true,
                File = logFile,
                Encoding = new UTF8Encoding(true),
                Layout = layout,
                MaxSizeRollBackups = 60
            };

            fileAppender.AddFilter(filter);
            fileAppender.ActivateOptions();

            BasicConfigurator.Configure(fileAppender);

            Log.Info($"Use Language: {_configService.UseLanguage}");
            Log.Info($"VideoConvert v{AppConfigService.GetAppVersion().ToString(4)} started");
            Log.Info($"OS-Version: {Environment.OSVersion.VersionString}");
            Log.Info($"CPU-Count: {Environment.ProcessorCount:0}");
            Log.Info($".NET Version: {Environment.Version.ToString(4)}");
            Log.Info($"System Uptime: {TimeSpan.FromMilliseconds(Environment.TickCount).ToString("c")}");

            var elevated = false;
            try
            {
                elevated = _processingService.IsProcessElevated();
            }
            catch (Exception)
            {
                Log.Error("Could not determine process elevation status");
            }

            if (Environment.OSVersion.Version.Major >= 6)
                Log.Info($"Process Elevated: {elevated}");

            Extensions supExt;
            CpuExtensions.GetExtensions(out supExt);
            InspectCpuExtensions(supExt);

            if (_configService.UseDebug)
            {
                Log.Info("Debug information enabled");
            }
        }

        private void InspectCpuExtensions(Extensions supExt)
        {
            _configService.SupportedCpuExtensions = supExt;
            var ext = (from field in supExt.GetType().GetFields() 
                                where (int) field.GetValue(supExt) == 1 
                                select field.Name).ToList();
            Log.Info("Supported CPU Extensions: " + string.Join(", ", ext));
        }

        public void ShowAbout()
        {
            DisplayWindow(ShellWin.AboutView);
        }
    }
}
