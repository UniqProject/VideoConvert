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
    using Interfaces;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using log4net.Filter;
    using log4net.Layout;
    using VideoConvert.AppServices.Services;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Model;

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
                return this._langCode;
            }
            set
            {
                this._langCode = value;
                this.NotifyOfPropertyChange(() => this.LangCode);
            }
        }

        public bool ShowOptions
        {
            get
            {
                return this._showOptions;
            }
            set
            {
                this._showOptions = value;
                this.NotifyOfPropertyChange(() => this.ShowOptions);
            }
        }

        public bool ShowMainView
        {
            get
            {
                return this._showMainView;
            }
            set
            {
                this._showMainView = value;
                this.NotifyOfPropertyChange(() => this.ShowMainView);
            }
        }

        public bool ShowEncode
        {
            get
            {
                return this._showEncode;
            }
            set
            {
                this._showEncode = value;
                this.NotifyOfPropertyChange(() => this.ShowEncode);
            }
        }

        public bool ShowChangelog
        {
            get
            {
                return this._showChangelog;
            }
            set
            {
                this._showChangelog = value;
                this.NotifyOfPropertyChange(() => this.ShowChangelog);
            }
        }

        public bool ShowAboutView
        {
            get
            {
                return this._showAboutView;
            }
            set
            {
                this._showAboutView = value;
                this.NotifyOfPropertyChange(() => this.ShowAboutView);
            }
        }

        public string WindowTitle
        {
            get
            {
                return this.Title;
            }
            set
            {
                this.Title = value;
                this.NotifyOfPropertyChange(() => this.Title);
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
                this.NotifyOfPropertyChange(() => this.LastView);
            }
        }

        public ShellWin ActualView
        {
            get { return _actualView; }
            set
            {
                _actualView = value;
                this.NotifyOfPropertyChange(() => this.ActualView);
            }
        }

        public ShellViewModel(IAppConfigService config, IProcessingService processing)
        {
            this._configService = config;
            this._processingService = processing;
            
            DisplayWindow(ShellWin.MainView);
            this.Title = "Video Convert";

            this._configService.PropertyChanged += ConfigServiceOnPropertyChanged;
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
                window = this.LastView;
            else
                if (window != this.ActualView)
                    this.LastView = this.ActualView;

            this.ActualView = window;

            switch (window)
            {
                case ShellWin.MainView:
                    ShowMainView = true;
                    ShowOptions = false;
                    ShowChangelog = false;
                    ShowAboutView = false;
                    ShowEncode = false;
                    if (this.MainViewModel != null)
                    {
                        this.MainViewModel.CheckUpdate();
                    }
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
                    if (this.EncodeViewModel != null)
                        this.EncodeViewModel.StartEncode(jobList);
                    break;
            }
        }

        public bool CanClose()
        {
            if (this.MainViewModel != null)
            {
                this.MainViewModel.Shutdown();
            }
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

            Log.InfoFormat("Use Language: {0}", _configService.UseLanguage);
            Log.InfoFormat("VideoConvert v{0} started", AppConfigService.GetAppVersion().ToString(4));
            Log.InfoFormat("OS-Version: {0}", Environment.OSVersion.VersionString);
            Log.InfoFormat("CPU-Count: {0:g}", Environment.ProcessorCount);
            Log.InfoFormat(".NET Version: {0}", Environment.Version.ToString(4));
            Log.InfoFormat("System Uptime: {0}", TimeSpan.FromMilliseconds(Environment.TickCount).ToString("c"));

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
                Log.InfoFormat("Process Elevated: {0}", elevated.ToString(_configService.CInfo));

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
            this.DisplayWindow(ShellWin.AboutView);
        }
    }
}
