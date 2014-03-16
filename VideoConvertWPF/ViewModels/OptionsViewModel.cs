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
    using Caliburn.Micro;
    using Interfaces;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using VideoConvert.AppServices.Services;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Model.MediaInfo;
    using Action = System.Action;

    public class OptionsViewModel : ViewModelBase, IOptionsViewModel
    {
        private readonly IShellViewModel _shellViewModel;

        private readonly IAppConfigService _configService;
        public IProcessingService ProcessingService { get; set; }
        #region private properties

        private int _selectedTab;
        private bool _keepStreamOrder;
        private bool _filterLoopingPlaylists;
        private bool _filterShortPlaylists;
        private int _filterShortPlaylistsValue;
        private bool _enableSsifScan;
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
        private string _movieDbLastLanguage;
        private string _movieDbLastFallbackLanguage;
        private string _movieDbLastRatingCountry;
        private string _movieDbPreferredCertPrefix;
        private string _movieDbLastFallbackRatingCountry;
        private string _movieDbFallbackCertPrefix;
        private int _movieDbRatingSrc;
        private string _tvDbCachePath;
        private string _tvDbParseString;
        private string _tvDbPreferredLanguage;
        private string _tvDbFallbackLanguage;
        private bool _tsMuxeRUseAsyncIo;
        private bool _tsMuxeRBlurayAudioPes;
        private bool _tsMuxeRVideoTimingInfo;
        private bool _tsMuxeRAddVideoPps;
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

        public bool EnableSsifScan
        {
            get
            {
                return _enableSsifScan;
            }
            set
            {
                _enableSsifScan = value;
                this.NotifyOfPropertyChange(() => this.EnableSsifScan);
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

        public string TvDbFallbackLanguage
        {
            get
            {
                return _tvDbFallbackLanguage;
            }
            set
            {
                _tvDbFallbackLanguage = value;
                this.NotifyOfPropertyChange(() => this.TvDbFallbackLanguage);
            }
        }

        public string TvDbPreferredLanguage
        {
            get
            {
                return _tvDbPreferredLanguage;
            }
            set
            {
                _tvDbPreferredLanguage = value;
                this.NotifyOfPropertyChange(() => this.TvDbPreferredLanguage);
            }
        }

        public string TvDbParseString
        {
            get
            {
                return _tvDbParseString;
            }
            set
            {
                _tvDbParseString = value;
                this.NotifyOfPropertyChange(() => this.TvDbParseString);
            }
        }

        public string TvDbCachePath
        {
            get
            {
                return _tvDbCachePath;
            }
            set
            {
                _tvDbCachePath = value;
                this.NotifyOfPropertyChange(() => this.TvDbCachePath);
            }
        }

        public int MovieDbRatingSrc
        {
            get
            {
                return _movieDbRatingSrc;
            }
            set
            {
                _movieDbRatingSrc = value;
                this.NotifyOfPropertyChange(() => this.MovieDbRatingSrc);
            }
        }

        public string MovieDbFallbackCertPrefix
        {
            get
            {
                return _movieDbFallbackCertPrefix;
            }
            set
            {
                _movieDbFallbackCertPrefix = value;
                this.NotifyOfPropertyChange(() => this.MovieDbFallbackCertPrefix);
            }
        }

        public string MovieDbLastFallbackRatingCountry
        {
            get
            {
                return _movieDbLastFallbackRatingCountry;
            }
            set
            {
                _movieDbLastFallbackRatingCountry = value;
                this.NotifyOfPropertyChange(() => this.MovieDbLastFallbackRatingCountry);
            }
        }

        public string MovieDbPreferredCertPrefix
        {
            get
            {
                return _movieDbPreferredCertPrefix;
            }
            set
            {
                _movieDbPreferredCertPrefix = value;
                this.NotifyOfPropertyChange(() => this.MovieDbPreferredCertPrefix);
            }
        }

        public string MovieDbLastRatingCountry
        {
            get
            {
                return _movieDbLastRatingCountry;
            }
            set
            {
                _movieDbLastRatingCountry = value;
                this.NotifyOfPropertyChange(() => this.MovieDbLastRatingCountry);
            }
        }

        public string MovieDbLastFallbackLanguage
        {
            get
            {
                return _movieDbLastFallbackLanguage;
            }
            set
            {
                _movieDbLastFallbackLanguage = value;
                this.NotifyOfPropertyChange(() => this.MovieDbLastFallbackLanguage);
            }
        }

        public string MovieDbLastLanguage
        {
            get
            {
                return _movieDbLastLanguage;
            }
            set
            {
                _movieDbLastLanguage = value;
                this.NotifyOfPropertyChange(() => this.MovieDbLastLanguage);
            }
        }

        public bool TsMuxeRAddVideoPps
        {
            get
            {
                return _tsMuxeRAddVideoPps;
            }
            set
            {
                _tsMuxeRAddVideoPps = value;
                this.NotifyOfPropertyChange(() => this.TsMuxeRAddVideoPps);
            }
        }

        public bool TsMuxeRVideoTimingInfo
        {
            get
            {
                return _tsMuxeRVideoTimingInfo;
            }
            set
            {
                _tsMuxeRVideoTimingInfo = value;
                this.NotifyOfPropertyChange(() => this.TsMuxeRVideoTimingInfo);
            }
        }

        public bool TsMuxeRBlurayAudioPes
        {
            get
            {
                return _tsMuxeRBlurayAudioPes;
            }
            set
            {
                _tsMuxeRBlurayAudioPes = value;
                this.NotifyOfPropertyChange(() => this.TsMuxeRBlurayAudioPes);
            }
        }

        public bool TsMuxeRUseAsyncIo
        {
            get
            {
                return _tsMuxeRUseAsyncIo;
            }
            set
            {
                _tsMuxeRUseAsyncIo = value;
                this.NotifyOfPropertyChange(() => this.TsMuxeRUseAsyncIo);
            }
        }

        public int TsMuxeRBottomOffset
        {
            get
            {
                return _tsMuxeRBottomOffset;
            }
            set
            {
                _tsMuxeRBottomOffset = value;
                this.NotifyOfPropertyChange(() => this.TsMuxeRBottomOffset);
            }
        }

        public int TsMuxerSubtitleAdditionalBorder
        {
            get
            {
                return _tsMuxerSubtitleAdditionalBorder;
            }
            set
            {
                _tsMuxerSubtitleAdditionalBorder = value;
                this.NotifyOfPropertyChange(() => this.TsMuxerSubtitleAdditionalBorder);
            }
        }

        public Color TsMuxeRSubtitleColor
        {
            get
            {
                return _tsMuxeRSubtitleColor;
            }
            set
            {
                _tsMuxeRSubtitleColor = value;
                this.NotifyOfPropertyChange(() => this.TsMuxeRSubtitleColor);
            }
        }

        public int TsMuxeRSubtitleFontSize
        {
            get
            {
                return _tsMuxeRSubtitleFontSize;
            }
            set
            {
                _tsMuxeRSubtitleFontSize = value;
                this.NotifyOfPropertyChange(() => this.TsMuxeRSubtitleFontSize);
            }
        }

        public string TsMuxeRSubtitleFont
        {
            get
            {
                return _tsMuxeRSubtitleFont;
            }
            set
            {
                _tsMuxeRSubtitleFont = value;
                this.NotifyOfPropertyChange(() => this.TsMuxeRSubtitleFont);
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
            this.EnableSsifScan = this._configService.EnableSSIF;

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
            this.MovieDbLastLanguage = this._configService.MovieDBLastLanguage;
            this.MovieDbLastFallbackLanguage = this._configService.MovieDBLastFallbackLanguage;
            this.MovieDbLastRatingCountry = this._configService.MovieDBLastRatingCountry;
            this.MovieDbPreferredCertPrefix = this._configService.MovieDBPreferredCertPrefix;
            this.MovieDbLastFallbackRatingCountry = this._configService.MovieDBLastFallbackRatingCountry;
            this.MovieDbFallbackCertPrefix = this._configService.MovieDBFallbackCertPrefix;
            this.MovieDbRatingSrc = this._configService.MovieDBRatingSrc;

            // TheTVDB Settings
            this.TvDbCachePath = this._configService.TvDBCachePath;
            this.TvDbParseString = this._configService.TvDBParseString;
            this.TvDbPreferredLanguage = this._configService.TvDBPreferredLanguage;
            this.TvDbFallbackLanguage = this._configService.TvDBFallbackLanguage;

            // tsMuxeR Settings
            this.TsMuxeRUseAsyncIo = this._configService.TSMuxeRUseAsyncIO;
            this.TsMuxeRBlurayAudioPes = this._configService.TSMuxeRBlurayAudioPES;
            this.TsMuxeRVideoTimingInfo = this._configService.TSMuxeRVideoTimingInfo;
            this.TsMuxeRAddVideoPps = this._configService.TSMuxeRAddVideoPPS;

            // Subtitles
            this.TsMuxeRSubtitleFont = this._configService.TSMuxeRSubtitleFont;
            this.TsMuxeRSubtitleFontSize = this._configService.TSMuxeRSubtitleFontSize;

            var fromString = ColorConverter.ConvertFromString(this._configService.TSMuxeRSubtitleColor);
            if (fromString != null)
                this.TsMuxeRSubtitleColor = (Color) fromString;

            this.TsMuxerSubtitleAdditionalBorder = this._configService.TSMuxerSubtitleAdditionalBorder;
            this.TsMuxeRBottomOffset = this._configService.TSMuxeRBottomOffset;
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
            this._configService.EnableSSIF = this.EnableSsifScan;

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
            this._configService.MovieDBLastLanguage = this.MovieDbLastLanguage;
            this._configService.MovieDBLastFallbackLanguage = this.MovieDbLastFallbackLanguage;
            this._configService.MovieDBLastRatingCountry = this.MovieDbLastRatingCountry;
            this._configService.MovieDBPreferredCertPrefix = this.MovieDbPreferredCertPrefix;
            this._configService.MovieDBLastFallbackRatingCountry = this.MovieDbLastFallbackRatingCountry;
            this._configService.MovieDBFallbackCertPrefix = this.MovieDbFallbackCertPrefix;
            this._configService.MovieDBRatingSrc = this.MovieDbRatingSrc;

            // TheTVDB Settings
            this._configService.TvDBCachePath = this.TvDbCachePath;
            this._configService.TvDBParseString = this.TvDbParseString;
            this._configService.TvDBPreferredLanguage = this.TvDbPreferredLanguage;
            this._configService.TvDBFallbackLanguage = this.TvDbFallbackLanguage;

            // tsMuxeR Settings
            this._configService.TSMuxeRUseAsyncIO = this.TsMuxeRUseAsyncIo;
            this._configService.TSMuxeRBlurayAudioPES = this.TsMuxeRBlurayAudioPes;
            this._configService.TSMuxeRVideoTimingInfo = this.TsMuxeRVideoTimingInfo;
            this._configService.TSMuxeRAddVideoPPS = this.TsMuxeRAddVideoPps;

            // Subtitles
            this._configService.TSMuxeRSubtitleFont = this.TsMuxeRSubtitleFont;
            this._configService.TSMuxeRSubtitleFontSize = this.TsMuxeRSubtitleFontSize;
            this._configService.TSMuxeRSubtitleColor = this.TsMuxeRSubtitleColor.ToString();
            this._configService.TSMuxerSubtitleAdditionalBorder = this.TsMuxerSubtitleAdditionalBorder;
            this._configService.TSMuxeRBottomOffset = this.TsMuxeRBottomOffset;
        }

        public void SelectTempLocation()
        {
            var folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
                DemuxLocation = folder;
        }

        public void SelectOutputLocation()
        {
            var folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
                OutputLocation = folder;
        }

        public void SelectEncoderLocation()
        {
            var folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
                ToolsPath = folder;
        }

        public void SelectTvDbCachePath()
        {
            var folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
                TvDbCachePath = folder;
        }

        public async void ReloadEncoders()
        {
            this.ReloadButtonEnabled = false;
            this.StatusLabel = "Reloading Encoder List";
            this.ShowStatusWindow = true;

            await Task.Run(new Action(LoadEncoders));

            await Task.Run(new Action(CreateEncoderList));

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
            var bdVersion = Assembly.GetAssembly(typeof(BDInfoLib.BDROM.TSCodecAC3)).GetName().Version;
            var mi = new MediaInfo();
            var miVer = mi.Option("Info_Version");
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
            var folder = GetFilePath("java.exe");
            if (!string.IsNullOrEmpty(folder))
            {
                JavaInstallPath = folder;
            }
        }

        private static string GetFolder()
        {
            var result = string.Empty;

            var folderBrowser = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };
            if (folderBrowser.ShowDialog() == CommonFileDialogResult.Ok)
                result = folderBrowser.FileName;

            return result;
        }

        private static string GetFilePath(string file = "")
        {
            var result = string.Empty;

            var fileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true,
            };
            if (!string.IsNullOrEmpty(file))
            {
                fileDialog.Filters.Add(new CommonFileDialogFilter(file, Path.GetExtension(file)));
                fileDialog.DefaultFileName = "java.exe";
            }
            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                result = fileDialog.FileName;

            return result;
        }
    }
}