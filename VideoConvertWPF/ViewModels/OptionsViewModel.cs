// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionsViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvertWPF.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Windows.Media;
    using Caliburn.Micro;
    using Interfaces;
    using VideoConvert.AppServices.Services;
    using ILog = log4net.ILog;
    using LogManager = log4net.LogManager;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Model.MediaInfo;
    using Action = System.Action;

    public class OptionsViewModel : ViewModelBase, IOptionsViewModel
    {
        private readonly IShellViewModel _shellViewModel;

        private readonly IAppConfigService _configService;
        public IProcessingService ProcessingService { get; set; }
        #region private properties

        private static readonly ILog Log = LogManager.GetLogger(typeof(OptionsViewModel));
        private int _selectedTab;
        private bool _keepStreamOrder;
        private bool _filterLoopingPlaylists;
        private bool _filterShortPlaylists;
        private int _filterShortPlaylistsValue;
        private bool _enableSSIFScan;
        private bool _useHardwareRendering;
        private bool _showChangeLog;
        private int _processPriority;
        private int _updateFrequency;
        private DateTime _lastUpdateRun;
        private bool _deleteCompletedJobs;
        private bool _deleteTemporaryFiles;
        private bool _use64BitEncoders;
        private bool _useFfmpegScaling;
        private bool _useOptimizedEncoders;
        private bool _limitDecoderThreads;
        private bool _createXbmcInfoFile;
        private string _demuxLocation;
        private string _outputLocation;
        private string _toolsPath;
        private string _javaInstallPath;
        private string _movieDBLastLanguage;
        private string _movieDBLastFallbackLanguage;
        private string _movieDBLastRatingCountry;
        private string _movieDBPreferredCertPrefix;
        private string _movieDBLastFallbackRatingCountry;
        private string _movieDBFallbackCertPrefix;
        private int _movieDBRatingSrc;
        private string _tvDBCachePath;
        private string _tvDBParseString;
        private string _tvDBPreferredLanguage;
        private string _tvDBFallbackLanguage;
        private bool _tsMuxeRUseAsyncIo;
        private bool _tsMuxeRBlurayAudioPES;
        private bool _tsMuxeRVideoTimingInfo;
        private bool _tsMuxeRAddVideoPPS;
        private string _tsMuxeRSubtitleFont;
        private int _tsMuxeRSubtitleFontSize;
        private Color _tsMuxeRSubtitleColor;
        private int _tsMuxerSubtitleAdditionalBorder;
        private int _tsMuxeRBottomOffset;
        private Dictionary<string, string> _encoderList;
        private bool _reloadButtonEnabled;
        private string _statusLabel;
        private bool _showStatusWindow;

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the Program Status Toolbar Label
        /// This indicates the status of HandBrake
        /// </summary>
        public string StatusLabel
        {
            get
            {
                return string.IsNullOrEmpty(this._statusLabel) ? "Ready" : this._statusLabel;
            }

            set
            {
                if (!Equals(this._statusLabel, value))
                {
                    this._statusLabel = value;
                    this.NotifyOfPropertyChange(() => this.StatusLabel);
                }
            }
        }

        public bool ShowStatusWindow
        {
            get
            {
                return this._showStatusWindow;
            }

            set
            {
                this._showStatusWindow = value;
                this.NotifyOfPropertyChange(() => this.ShowStatusWindow);
            }
        }

        public string WindowTitle
        {
            get { return this.Title; }
            set
            {
                this.Title = value;
                this.NotifyOfPropertyChange(() => this.Title);
            }
        }

        public string Version
        {
            get { return AppConfigService.GetAppVersionStr(); }
        }

        public int SelectedTab
        {
            get
            {
                return _selectedTab;
            }
            set
            {
                _selectedTab = value;
                this.NotifyOfPropertyChange(() => this.SelectedTab);
            }
        }

        public bool KeepStreamOrder
        {
            get
            {
                return _keepStreamOrder;
            }
            set
            {
                _keepStreamOrder = value;
                this.NotifyOfPropertyChange(() => this.KeepStreamOrder);
            }
        }

        public bool FilterLoopingPlaylists
        {
            get
            {
                return _filterLoopingPlaylists;
            }
            set
            {
                _filterLoopingPlaylists = value;
                this.NotifyOfPropertyChange(() => this.FilterLoopingPlaylists);
            }
        }

        public bool FilterShortPlaylists
        {
            get
            {
                return _filterShortPlaylists;
            }
            set
            {
                _filterShortPlaylists = value;
                this.NotifyOfPropertyChange(() => this.FilterShortPlaylists);
            }
        }

        public int FilterShortPlaylistsValue
        {
            get
            {
                return _filterShortPlaylistsValue;
            }
            set
            {
                _filterShortPlaylistsValue = value;
                this.NotifyOfPropertyChange(() => this.FilterShortPlaylistsValue);
            }
        }

        public bool EnableSSIFScan
        {
            get
            {
                return _enableSSIFScan;
            }
            set
            {
                _enableSSIFScan = value;
                this.NotifyOfPropertyChange(() => this.EnableSSIFScan);
            }
        }

        public bool UseHardwareRendering
        {
            get
            {
                return _useHardwareRendering;
            }
            set
            {
                _useHardwareRendering = value;
                this.NotifyOfPropertyChange(() => this.UseHardwareRendering);
            }
        }

        public bool ShowChangeLog
        {
            get
            {
                return _showChangeLog;
            }
            set
            {
                _showChangeLog = value;
                this.NotifyOfPropertyChange(() => this.ShowChangeLog);
            }
        }

        public int ProcessPriority
        {
            get
            {
                return _processPriority;
            }
            set
            {
                _processPriority = value;
                this.NotifyOfPropertyChange(() => this.ProcessPriority);
            }
        }

        public int UpdateFrequency
        {
            get
            {
                return _updateFrequency;
            }
            set
            {
                _updateFrequency = value;
                this.NotifyOfPropertyChange(() => this.UpdateFrequency);
            }
        }

        public DateTime LastUpdateRun
        {
            get
            {
                return _lastUpdateRun;
            }
            set
            {
                _lastUpdateRun = value;
                this.NotifyOfPropertyChange(() => this.LastUpdateRun);
            }
        }

        public bool CreateXbmcInfoFile
        {
            get
            {
                return _createXbmcInfoFile;
            }
            set
            {
                _createXbmcInfoFile = value;
                this.NotifyOfPropertyChange(() => this.CreateXbmcInfoFile);
            }
        }

        public bool LimitDecoderThreads
        {
            get
            {
                return _limitDecoderThreads;
            }
            set
            {
                _limitDecoderThreads = value;
                this.NotifyOfPropertyChange(() => this.LimitDecoderThreads);
            }
        }

        public bool UseOptimizedEncoders
        {
            get
            {
                return _useOptimizedEncoders;
            }
            set
            {
                _useOptimizedEncoders = value;
                this.NotifyOfPropertyChange(() => this.UseOptimizedEncoders);
            }
        }

        public bool UseFfmpegScaling
        {
            get
            {
                return _useFfmpegScaling;
            }
            set
            {
                _useFfmpegScaling = value;
                this.NotifyOfPropertyChange(() => this.UseFfmpegScaling);
            }
        }

        public bool Use64BitEncoders
        {
            get
            {
                return _use64BitEncoders;
            }
            set
            {
                _use64BitEncoders = value;
                this.NotifyOfPropertyChange(() => this.Use64BitEncoders);
            }
        }

        public bool DeleteTemporaryFiles
        {
            get
            {
                return _deleteTemporaryFiles;
            }
            set
            {
                _deleteTemporaryFiles = value;
                this.NotifyOfPropertyChange(() => this.DeleteTemporaryFiles);
            }
        }

        public bool DeleteCompletedJobs
        {
            get
            {
                return _deleteCompletedJobs;
            }
            set
            {
                _deleteCompletedJobs = value;
                this.NotifyOfPropertyChange(() => this.DeleteCompletedJobs);
            }
        }

        public string JavaInstallPath
        {
            get
            {
                return _javaInstallPath;
            }
            set
            {
                _javaInstallPath = value;
                this.NotifyOfPropertyChange(() => this.JavaInstallPath);
            }
        }

        public string ToolsPath
        {
            get
            {
                return _toolsPath;
            }
            set
            {
                _toolsPath = value;
                this.NotifyOfPropertyChange(() => this.ToolsPath);
            }
        }

        public string OutputLocation
        {
            get
            {
                return _outputLocation;
            }
            set
            {
                _outputLocation = value;
                this.NotifyOfPropertyChange(() => this.OutputLocation);
            }
        }

        public string DemuxLocation
        {
            get
            {
                return _demuxLocation;
            }
            set
            {
                _demuxLocation = value;
                this.NotifyOfPropertyChange(() => this.DemuxLocation);
            }
        }

        public string TvDBFallbackLanguage
        {
            get
            {
                return _tvDBFallbackLanguage;
            }
            set
            {
                _tvDBFallbackLanguage = value;
                this.NotifyOfPropertyChange(() => this.TvDBFallbackLanguage);
            }
        }

        public string TvDBPreferredLanguage
        {
            get
            {
                return _tvDBPreferredLanguage;
            }
            set
            {
                _tvDBPreferredLanguage = value;
                this.NotifyOfPropertyChange(() => this.TvDBPreferredLanguage);
            }
        }

        public string TvDBParseString
        {
            get
            {
                return _tvDBParseString;
            }
            set
            {
                _tvDBParseString = value;
                this.NotifyOfPropertyChange(() => this.TvDBParseString);
            }
        }

        public string TvDBCachePath
        {
            get
            {
                return _tvDBCachePath;
            }
            set
            {
                _tvDBCachePath = value;
                this.NotifyOfPropertyChange(() => this.TvDBCachePath);
            }
        }

        public int MovieDBRatingSrc
        {
            get
            {
                return _movieDBRatingSrc;
            }
            set
            {
                _movieDBRatingSrc = value;
                this.NotifyOfPropertyChange(() => this.MovieDBRatingSrc);
            }
        }

        public string MovieDBFallbackCertPrefix
        {
            get
            {
                return _movieDBFallbackCertPrefix;
            }
            set
            {
                _movieDBFallbackCertPrefix = value;
                this.NotifyOfPropertyChange(() => this.MovieDBFallbackCertPrefix);
            }
        }

        public string MovieDBLastFallbackRatingCountry
        {
            get
            {
                return _movieDBLastFallbackRatingCountry;
            }
            set
            {
                _movieDBLastFallbackRatingCountry = value;
                this.NotifyOfPropertyChange(() => this.MovieDBLastFallbackRatingCountry);
            }
        }

        public string MovieDBPreferredCertPrefix
        {
            get
            {
                return _movieDBPreferredCertPrefix;
            }
            set
            {
                _movieDBPreferredCertPrefix = value;
                this.NotifyOfPropertyChange(() => this.MovieDBPreferredCertPrefix);
            }
        }

        public string MovieDBLastRatingCountry
        {
            get
            {
                return _movieDBLastRatingCountry;
            }
            set
            {
                _movieDBLastRatingCountry = value;
                this.NotifyOfPropertyChange(() => this.MovieDBLastRatingCountry);
            }
        }

        public string MovieDBLastFallbackLanguage
        {
            get
            {
                return _movieDBLastFallbackLanguage;
            }
            set
            {
                _movieDBLastFallbackLanguage = value;
                this.NotifyOfPropertyChange(() => this.MovieDBLastFallbackLanguage);
            }
        }

        public string MovieDBLastLanguage
        {
            get
            {
                return _movieDBLastLanguage;
            }
            set
            {
                _movieDBLastLanguage = value;
                this.NotifyOfPropertyChange(() => this.MovieDBLastLanguage);
            }
        }

        public bool TSMuxeRAddVideoPPS
        {
            get
            {
                return _tsMuxeRAddVideoPPS;
            }
            set
            {
                _tsMuxeRAddVideoPPS = value;
                this.NotifyOfPropertyChange(() => this.TSMuxeRAddVideoPPS);
            }
        }

        public bool TSMuxeRVideoTimingInfo
        {
            get
            {
                return _tsMuxeRVideoTimingInfo;
            }
            set
            {
                _tsMuxeRVideoTimingInfo = value;
                this.NotifyOfPropertyChange(() => this.TSMuxeRVideoTimingInfo);
            }
        }

        public bool TSMuxeRBlurayAudioPES
        {
            get
            {
                return _tsMuxeRBlurayAudioPES;
            }
            set
            {
                _tsMuxeRBlurayAudioPES = value;
                this.NotifyOfPropertyChange(() => this.TSMuxeRBlurayAudioPES);
            }
        }

        public bool TSMuxeRUseAsyncIO
        {
            get
            {
                return _tsMuxeRUseAsyncIo;
            }
            set
            {
                _tsMuxeRUseAsyncIo = value;
                this.NotifyOfPropertyChange(() => this.TSMuxeRUseAsyncIO);
            }
        }

        public int TSMuxeRBottomOffset
        {
            get
            {
                return _tsMuxeRBottomOffset;
            }
            set
            {
                _tsMuxeRBottomOffset = value;
                this.NotifyOfPropertyChange(() => this.TSMuxeRBottomOffset);
            }
        }

        public int TSMuxerSubtitleAdditionalBorder
        {
            get
            {
                return _tsMuxerSubtitleAdditionalBorder;
            }
            set
            {
                _tsMuxerSubtitleAdditionalBorder = value;
                this.NotifyOfPropertyChange(() => this.TSMuxerSubtitleAdditionalBorder);
            }
        }

        public Color TSMuxeRSubtitleColor
        {
            get
            {
                return _tsMuxeRSubtitleColor;
            }
            set
            {
                _tsMuxeRSubtitleColor = value;
                this.NotifyOfPropertyChange(() => this.TSMuxeRSubtitleColor);
            }
        }

        public int TSMuxeRSubtitleFontSize
        {
            get
            {
                return _tsMuxeRSubtitleFontSize;
            }
            set
            {
                _tsMuxeRSubtitleFontSize = value;
                this.NotifyOfPropertyChange(() => this.TSMuxeRSubtitleFontSize);
            }
        }

        public string TSMuxeRSubtitleFont
        {
            get
            {
                return _tsMuxeRSubtitleFont;
            }
            set
            {
                _tsMuxeRSubtitleFont = value;
                this.NotifyOfPropertyChange(() => this.TSMuxeRSubtitleFont);
            }
        }

        public Dictionary<string, string> EncoderList
        {
            get
            {
                return _encoderList;
            }
            set
            {
                _encoderList = value;
                this.NotifyOfPropertyChange(()=>this.EncoderList);
            }
        }

        public bool ReloadButtonEnabled
        {
            get
            {
                return _reloadButtonEnabled;
            }
            set
            {
                _reloadButtonEnabled = value;
                this.NotifyOfPropertyChange(() => this.ReloadButtonEnabled);
            }
        }

        #endregion

        public OptionsViewModel(IShellViewModel shellViewModel, IWindowManager windowManager, IAppConfigService config)
        {
            this._shellViewModel = shellViewModel;
            this.WindowManager = windowManager;
            this._configService = config;
            this.WindowTitle = "Options";
            this._configService.PropertyChanged += AppConfigOnSettingChanged;
        }

        public override void OnLoad()
        {
            base.OnLoad();

            Task.Run(new Action(LoadSettings));

            Task.Run(new Action(CreateEncoderList));
        }

        protected void LoadSettings()
        {
            // General 
            this.KeepStreamOrder = this._configService.KeepStreamOrder;
            this.FilterLoopingPlaylists = this._configService.FilterLoopingPlaylists;
            this.FilterShortPlaylists = this._configService.FilterShortPlaylists;
            this.FilterShortPlaylistsValue = this._configService.FilterShortPlaylistsValue;
            this.EnableSSIFScan = this._configService.EnableSSIF;

            // System
            this.ProcessPriority = this._configService.ProcessPriority;
            this.UseHardwareRendering = this._configService.UseHardwareRendering;
            this.ShowChangeLog = this._configService.ShowChangeLog;

            // Autoupdate
            this.UpdateFrequency = this._configService.UpdateFrequency;
            this.LastUpdateRun = this._configService.LastUpdateRun;

            // Encoding
            this.DeleteCompletedJobs = this._configService.DeleteCompletedJobs;
            this.DeleteTemporaryFiles = this._configService.DeleteTemporaryFiles;
            this.Use64BitEncoders = this._configService.Use64BitEncoders;
            this.UseFfmpegScaling = this._configService.UseFfmpegScaling;
            this.UseOptimizedEncoders = this._configService.UseOptimizedEncoders;
            this.LimitDecoderThreads = this._configService.LimitDecoderThreads;
            this.CreateXbmcInfoFile = this._configService.CreateXbmcInfoFile;

            // Directories
            this.DemuxLocation = this._configService.DemuxLocation;
            this.OutputLocation = this._configService.OutputLocation;

            //Encoder
            this.ToolsPath = this._configService.ToolsPath;
            this.JavaInstallPath = this._configService.JavaInstallPath;

            // MovieDB Settings
            this.MovieDBLastLanguage = this._configService.MovieDBLastLanguage;
            this.MovieDBLastFallbackLanguage = this._configService.MovieDBLastFallbackLanguage;
            this.MovieDBLastRatingCountry = this._configService.MovieDBLastRatingCountry;
            this.MovieDBPreferredCertPrefix = this._configService.MovieDBPreferredCertPrefix;
            this.MovieDBLastFallbackRatingCountry = this._configService.MovieDBLastFallbackRatingCountry;
            this.MovieDBFallbackCertPrefix = this._configService.MovieDBFallbackCertPrefix;
            this.MovieDBRatingSrc = this._configService.MovieDBRatingSrc;

            // TheTVDB Settings
            this.TvDBCachePath = this._configService.TvDBCachePath;
            this.TvDBParseString = this._configService.TvDBParseString;
            this.TvDBPreferredLanguage = this._configService.TvDBPreferredLanguage;
            this.TvDBFallbackLanguage = this._configService.TvDBFallbackLanguage;

            // tsMuxeR Settings
            this.TSMuxeRUseAsyncIO = this._configService.TSMuxeRUseAsyncIO;
            this.TSMuxeRBlurayAudioPES = this._configService.TSMuxeRBlurayAudioPES;
            this.TSMuxeRVideoTimingInfo = this._configService.TSMuxeRVideoTimingInfo;
            this.TSMuxeRAddVideoPPS = this._configService.TSMuxeRAddVideoPPS;

            // Subtitles
            this.TSMuxeRSubtitleFont = this._configService.TSMuxeRSubtitleFont;
            this.TSMuxeRSubtitleFontSize = this._configService.TSMuxeRSubtitleFontSize;

            var fromString = ColorConverter.ConvertFromString(this._configService.TSMuxeRSubtitleColor);
            if (fromString != null)
                this.TSMuxeRSubtitleColor = (Color) fromString;

            this.TSMuxerSubtitleAdditionalBorder = this._configService.TSMuxerSubtitleAdditionalBorder;
            this.TSMuxeRBottomOffset = this._configService.TSMuxeRBottomOffset;
        }

        private void AppConfigOnSettingChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "LastUpdateRun":
                    // fix wrong date displayed after an update check
                    this.LastUpdateRun = this._configService.LastUpdateRun;
                    break;
            }
        }

        public void Close()
        {
            this.Save();
            this._shellViewModel.DisplayWindow(ShellWin.MainView);
        }

        private void Save()
        {
            // General 
            this._configService.KeepStreamOrder = this.KeepStreamOrder;
            this._configService.FilterLoopingPlaylists = this.FilterLoopingPlaylists;
            this._configService.FilterShortPlaylists = this.FilterShortPlaylists;
            this._configService.FilterShortPlaylistsValue = this.FilterShortPlaylistsValue;
            this._configService.EnableSSIF = this.EnableSSIFScan;

            // System
            this._configService.ProcessPriority = this.ProcessPriority;
            this._configService.UseHardwareRendering = this.UseHardwareRendering;
            this._configService.ShowChangeLog = this.ShowChangeLog;

            // Autoupdate
            this._configService.UpdateFrequency = this.UpdateFrequency;

            // Encoding
            this._configService.DeleteCompletedJobs = this.DeleteCompletedJobs;
            this._configService.DeleteTemporaryFiles = this.DeleteTemporaryFiles;
            this._configService.Use64BitEncoders = this.Use64BitEncoders;
            this._configService.UseFfmpegScaling = this.UseFfmpegScaling;
            this._configService.UseOptimizedEncoders = this.UseOptimizedEncoders;
            this._configService.LimitDecoderThreads = this.LimitDecoderThreads;
            this._configService.CreateXbmcInfoFile = this.CreateXbmcInfoFile;

            // Directories
            this._configService.DemuxLocation = this.DemuxLocation;
            this._configService.OutputLocation = this.OutputLocation;

            //Encoder
            this._configService.ToolsPath = this.ToolsPath;
            this._configService.JavaInstallPath = this.JavaInstallPath;

            // MovieDB Settings
            this._configService.MovieDBLastLanguage = this.MovieDBLastLanguage;
            this._configService.MovieDBLastFallbackLanguage = this.MovieDBLastFallbackLanguage;
            this._configService.MovieDBLastRatingCountry = this.MovieDBLastRatingCountry;
            this._configService.MovieDBPreferredCertPrefix = this.MovieDBPreferredCertPrefix;
            this._configService.MovieDBLastFallbackRatingCountry = this.MovieDBLastFallbackRatingCountry;
            this._configService.MovieDBFallbackCertPrefix = this.MovieDBFallbackCertPrefix;
            this._configService.MovieDBRatingSrc = this.MovieDBRatingSrc;

            // TheTVDB Settings
            this._configService.TvDBCachePath = this.TvDBCachePath;
            this._configService.TvDBParseString = this.TvDBParseString;
            this._configService.TvDBPreferredLanguage = this.TvDBPreferredLanguage;
            this._configService.TvDBFallbackLanguage = this.TvDBFallbackLanguage;

            // tsMuxeR Settings
            this._configService.TSMuxeRUseAsyncIO = this.TSMuxeRUseAsyncIO;
            this._configService.TSMuxeRBlurayAudioPES = this.TSMuxeRBlurayAudioPES;
            this._configService.TSMuxeRVideoTimingInfo = this.TSMuxeRVideoTimingInfo;
            this._configService.TSMuxeRAddVideoPPS = this.TSMuxeRAddVideoPPS;

            // Subtitles
            this._configService.TSMuxeRSubtitleFont = this.TSMuxeRSubtitleFont;
            this._configService.TSMuxeRSubtitleFontSize = this.TSMuxeRSubtitleFontSize;
            this._configService.TSMuxeRSubtitleColor = this.TSMuxeRSubtitleColor.ToString();
            this._configService.TSMuxerSubtitleAdditionalBorder = this.TSMuxerSubtitleAdditionalBorder;
            this._configService.TSMuxeRBottomOffset = this.TSMuxeRBottomOffset;
        }

        public void SelectTempLocation()
        {
            string folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
                DemuxLocation = folder;
        }

        public void SelectOutputLocation()
        {
            string folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
                OutputLocation = folder;
        }

        public void SelectEncoderLocation()
        {
            string folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
                ToolsPath = folder;
        }

        public void SelectTvDbCachePath()
        {
            string folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
                TvDBCachePath = folder;
        }

        public async void ReloadEncoders()
        {
            this.ReloadButtonEnabled = false;
            this.StatusLabel = "Reloading Encoder List";
            this.ShowStatusWindow = true;

            await Task.Run(new System.Action(LoadEncoders));

            await Task.Run(new System.Action(CreateEncoderList));

            this.ReloadButtonEnabled = true;
            this.StatusLabel = "Ready";
            this.ShowStatusWindow = false;
        }

        public void LoadEncoders()
        {
            if (!string.IsNullOrEmpty(ToolsPath))
            {
                ProcessingService.GetAppVersions(ToolsPath, JavaInstallPath);
            }
        }

        private void CreateEncoderList()
        {
            Version bdVersion = Assembly.GetAssembly(typeof(BDInfoLib.BDROM.TSCodecAC3)).GetName().Version;
            MediaInfo mi = new MediaInfo();
            string miVer = mi.Option("Info_Version");
            mi.Close();
            this.EncoderList = new Dictionary<string, string>
            {
                {"x264", this._configService.Lastx264Ver},
                {"x264 64bit", this._configService.Lastx26464Ver},
                {"ffmpeg", this._configService.LastffmpegVer},
                {"ffmpeg 64bit", this._configService.Lastffmpeg64Ver},
                {"eac3to", this._configService.Lasteac3ToVer},
                {"lsdvd", this._configService.LastlsdvdVer},
                {"MkvToolnix", this._configService.LastMKVMergeVer},
                {"mplayer", this._configService.LastMplayerVer},
                {"tsMuxeR", this._configService.LastTSMuxerVer},
                {"MJPEG tools", this._configService.LastMJPEGToolsVer},
                {"DVDAuthor", this._configService.LastDVDAuthorVer},
                {"MP4Box", this._configService.LastMp4BoxVer},
                {"HCenc", this._configService.LastHcEncVer},
                {"OggEnc2", this._configService.LastOggEncVer},
                {"OggEnc2 Lancer Build", this._configService.LastOggEncLancerVer},
                {"NeroAacEnc", this._configService.LastNeroAacEncVer},
                {"LAME", this._configService.LastLameVer},
                {"LAME 64bit", this._configService.LastLame64Ver},
                {"vpxEnc", this._configService.LastVpxEncVer},
                {"BDSup2Sub", this._configService.LastBDSup2SubVer},
                {"BDInfo Library", string.Format("{0:g}.{1:g}.{2:g}", bdVersion.Major, bdVersion.Minor, bdVersion.Build)},
                {"MediaInfo Library", miVer.Replace("MediaInfoLib - v", string.Empty)},
                {"AviSynth", this._configService.LastAviSynthVer}
            };
            this.ReloadButtonEnabled = true;
        }

        public void SelectJavaLocation()
        {
            string folder = GetFilePath("java.exe");
            if (!string.IsNullOrEmpty(folder))
            {
                JavaInstallPath = folder;
            }
        }

        private static string GetFolder()
        {
            string result = string.Empty;

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
                result = folderBrowser.SelectedPath;

            return result;
        }

        private static string GetFilePath(string file = "")
        {
            string result = string.Empty;

            OpenFileDialog fileDialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(file))
            {
                fileDialog.Filter = string.Format("{0}|{0}", file);
            }
            if (fileDialog.ShowDialog() == DialogResult.OK)
                result = fileDialog.FileName;

            return result;
        }
    }
}