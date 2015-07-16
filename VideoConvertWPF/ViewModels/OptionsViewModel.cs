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
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using Caliburn.Micro;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using VideoConvert.AppServices.Services;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Model.MediaInfo;
    using VideoConvertWPF.ViewModels.Interfaces;
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
                return string.IsNullOrEmpty(_statusLabel) ? "Ready" : _statusLabel;
            }

            set
            {
                if (Equals(_statusLabel, value)) return;
                _statusLabel = value;
                NotifyOfPropertyChange(() => StatusLabel);
            }
        }

        public bool ShowStatusWindow
        {
            get
            {
                return _showStatusWindow;
            }

            set
            {
                _showStatusWindow = value;
                NotifyOfPropertyChange(() => ShowStatusWindow);
            }
        }

        public string WindowTitle
        {
            get { return Title; }
            set
            {
                Title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public string Version => AppConfigService.GetAppVersionStr();

        public int SelectedTab
        {
            get
            {
                return _selectedTab;
            }
            set
            {
                _selectedTab = value;
                NotifyOfPropertyChange(() => SelectedTab);
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
                NotifyOfPropertyChange(() => KeepStreamOrder);
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
                NotifyOfPropertyChange(() => FilterLoopingPlaylists);
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
                NotifyOfPropertyChange(() => FilterShortPlaylists);
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
                NotifyOfPropertyChange(() => FilterShortPlaylistsValue);
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
                NotifyOfPropertyChange(() => EnableSsifScan);
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
                NotifyOfPropertyChange(() => UseHardwareRendering);
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
                NotifyOfPropertyChange(() => ShowChangeLog);
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
                NotifyOfPropertyChange(() => ProcessPriority);
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
                NotifyOfPropertyChange(() => UpdateFrequency);
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
                NotifyOfPropertyChange(() => LastUpdateRun);
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
                NotifyOfPropertyChange(() => CreateXbmcInfoFile);
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
                NotifyOfPropertyChange(() => LimitDecoderThreads);
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
                NotifyOfPropertyChange(() => UseOptimizedEncoders);
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
                NotifyOfPropertyChange(() => UseFfmpegScaling);
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
                NotifyOfPropertyChange(() => Use64BitEncoders);
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
                NotifyOfPropertyChange(() => DeleteTemporaryFiles);
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
                NotifyOfPropertyChange(() => DeleteCompletedJobs);
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
                NotifyOfPropertyChange(() => JavaInstallPath);
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
                NotifyOfPropertyChange(() => ToolsPath);
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
                NotifyOfPropertyChange(() => OutputLocation);
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
                NotifyOfPropertyChange(() => DemuxLocation);
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
                NotifyOfPropertyChange(() => TvDbFallbackLanguage);
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
                NotifyOfPropertyChange(() => TvDbPreferredLanguage);
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
                NotifyOfPropertyChange(() => TvDbParseString);
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
                NotifyOfPropertyChange(() => TvDbCachePath);
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
                NotifyOfPropertyChange(() => MovieDbRatingSrc);
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
                NotifyOfPropertyChange(() => MovieDbFallbackCertPrefix);
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
                NotifyOfPropertyChange(() => MovieDbLastFallbackRatingCountry);
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
                NotifyOfPropertyChange(() => MovieDbPreferredCertPrefix);
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
                NotifyOfPropertyChange(() => MovieDbLastRatingCountry);
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
                NotifyOfPropertyChange(() => MovieDbLastFallbackLanguage);
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
                NotifyOfPropertyChange(() => MovieDbLastLanguage);
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
                NotifyOfPropertyChange(() => TsMuxeRAddVideoPps);
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
                NotifyOfPropertyChange(() => TsMuxeRVideoTimingInfo);
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
                NotifyOfPropertyChange(() => TsMuxeRBlurayAudioPes);
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
                NotifyOfPropertyChange(() => TsMuxeRUseAsyncIo);
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
                NotifyOfPropertyChange(() => TsMuxeRBottomOffset);
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
                NotifyOfPropertyChange(() => TsMuxerSubtitleAdditionalBorder);
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
                NotifyOfPropertyChange(() => TsMuxeRSubtitleColor);
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
                NotifyOfPropertyChange(() => TsMuxeRSubtitleFontSize);
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
                NotifyOfPropertyChange(() => TsMuxeRSubtitleFont);
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
                NotifyOfPropertyChange(()=>EncoderList);
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
                NotifyOfPropertyChange(() => ReloadButtonEnabled);
            }
        }

        #endregion

        public OptionsViewModel(IShellViewModel shellViewModel, IWindowManager windowManager, IAppConfigService config)
        {
            _shellViewModel = shellViewModel;
            WindowManager = windowManager;
            _configService = config;
            WindowTitle = "Options";
            _configService.PropertyChanged += AppConfigOnSettingChanged;
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
            KeepStreamOrder = _configService.KeepStreamOrder;
            FilterLoopingPlaylists = _configService.FilterLoopingPlaylists;
            FilterShortPlaylists = _configService.FilterShortPlaylists;
            FilterShortPlaylistsValue = _configService.FilterShortPlaylistsValue;
            EnableSsifScan = _configService.EnableSSIF;

            // System
            ProcessPriority = _configService.ProcessPriority;
            UseHardwareRendering = _configService.UseHardwareRendering;
            ShowChangeLog = _configService.ShowChangeLog;

            // Autoupdate
            UpdateFrequency = _configService.UpdateFrequency;
            LastUpdateRun = _configService.LastUpdateRun;

            // Encoding
            DeleteCompletedJobs = _configService.DeleteCompletedJobs;
            DeleteTemporaryFiles = _configService.DeleteTemporaryFiles;
            Use64BitEncoders = _configService.Use64BitEncoders;
            UseFfmpegScaling = _configService.UseFfmpegScaling;
            UseOptimizedEncoders = _configService.UseOptimizedEncoders;
            LimitDecoderThreads = _configService.LimitDecoderThreads;
            CreateXbmcInfoFile = _configService.CreateXbmcInfoFile;

            // Directories
            DemuxLocation = _configService.DemuxLocation;
            OutputLocation = _configService.OutputLocation;

            //Encoder
            ToolsPath = _configService.ToolsPath;
            JavaInstallPath = _configService.JavaInstallPath;

            // MovieDB Settings
            MovieDbLastLanguage = _configService.MovieDBLastLanguage;
            MovieDbLastFallbackLanguage = _configService.MovieDBLastFallbackLanguage;
            MovieDbLastRatingCountry = _configService.MovieDBLastRatingCountry;
            MovieDbPreferredCertPrefix = _configService.MovieDBPreferredCertPrefix;
            MovieDbLastFallbackRatingCountry = _configService.MovieDBLastFallbackRatingCountry;
            MovieDbFallbackCertPrefix = _configService.MovieDBFallbackCertPrefix;
            MovieDbRatingSrc = _configService.MovieDBRatingSrc;

            // TheTVDB Settings
            TvDbCachePath = _configService.TvDBCachePath;
            TvDbParseString = _configService.TvDBParseString;
            TvDbPreferredLanguage = _configService.TvDBPreferredLanguage;
            TvDbFallbackLanguage = _configService.TvDBFallbackLanguage;

            // tsMuxeR Settings
            TsMuxeRUseAsyncIo = _configService.TSMuxeRUseAsyncIO;
            TsMuxeRBlurayAudioPes = _configService.TSMuxeRBlurayAudioPES;
            TsMuxeRVideoTimingInfo = _configService.TSMuxeRVideoTimingInfo;
            TsMuxeRAddVideoPps = _configService.TSMuxeRAddVideoPPS;

            // Subtitles
            TsMuxeRSubtitleFont = _configService.TSMuxeRSubtitleFont;
            TsMuxeRSubtitleFontSize = _configService.TSMuxeRSubtitleFontSize;

            var fromString = ColorConverter.ConvertFromString(_configService.TSMuxeRSubtitleColor);
            if (fromString != null)
                TsMuxeRSubtitleColor = (Color) fromString;

            TsMuxerSubtitleAdditionalBorder = _configService.TSMuxerSubtitleAdditionalBorder;
            TsMuxeRBottomOffset = _configService.TSMuxeRBottomOffset;
        }

        private void AppConfigOnSettingChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "LastUpdateRun":
                    // fix wrong date displayed after an update check
                    LastUpdateRun = _configService.LastUpdateRun;
                    break;
            }
        }

        public void Close()
        {
            Save();
            _shellViewModel.DisplayWindow(ShellWin.MainView);
        }

        private void Save()
        {
            // General 
            _configService.KeepStreamOrder = KeepStreamOrder;
            _configService.FilterLoopingPlaylists = FilterLoopingPlaylists;
            _configService.FilterShortPlaylists = FilterShortPlaylists;
            _configService.FilterShortPlaylistsValue = FilterShortPlaylistsValue;
            _configService.EnableSSIF = EnableSsifScan;

            // System
            _configService.ProcessPriority = ProcessPriority;
            _configService.UseHardwareRendering = UseHardwareRendering;
            _configService.ShowChangeLog = ShowChangeLog;

            // Autoupdate
            _configService.UpdateFrequency = UpdateFrequency;

            // Encoding
            _configService.DeleteCompletedJobs = DeleteCompletedJobs;
            _configService.DeleteTemporaryFiles = DeleteTemporaryFiles;
            _configService.Use64BitEncoders = Use64BitEncoders;
            _configService.UseFfmpegScaling = UseFfmpegScaling;
            _configService.UseOptimizedEncoders = UseOptimizedEncoders;
            _configService.LimitDecoderThreads = LimitDecoderThreads;
            _configService.CreateXbmcInfoFile = CreateXbmcInfoFile;

            // Directories
            _configService.DemuxLocation = DemuxLocation;
            _configService.OutputLocation = OutputLocation;

            //Encoder
            _configService.ToolsPath = ToolsPath;
            _configService.JavaInstallPath = JavaInstallPath;

            // MovieDB Settings
            _configService.MovieDBLastLanguage = MovieDbLastLanguage;
            _configService.MovieDBLastFallbackLanguage = MovieDbLastFallbackLanguage;
            _configService.MovieDBLastRatingCountry = MovieDbLastRatingCountry;
            _configService.MovieDBPreferredCertPrefix = MovieDbPreferredCertPrefix;
            _configService.MovieDBLastFallbackRatingCountry = MovieDbLastFallbackRatingCountry;
            _configService.MovieDBFallbackCertPrefix = MovieDbFallbackCertPrefix;
            _configService.MovieDBRatingSrc = MovieDbRatingSrc;

            // TheTVDB Settings
            _configService.TvDBCachePath = TvDbCachePath;
            _configService.TvDBParseString = TvDbParseString;
            _configService.TvDBPreferredLanguage = TvDbPreferredLanguage;
            _configService.TvDBFallbackLanguage = TvDbFallbackLanguage;

            // tsMuxeR Settings
            _configService.TSMuxeRUseAsyncIO = TsMuxeRUseAsyncIo;
            _configService.TSMuxeRBlurayAudioPES = TsMuxeRBlurayAudioPes;
            _configService.TSMuxeRVideoTimingInfo = TsMuxeRVideoTimingInfo;
            _configService.TSMuxeRAddVideoPPS = TsMuxeRAddVideoPps;

            // Subtitles
            _configService.TSMuxeRSubtitleFont = TsMuxeRSubtitleFont;
            _configService.TSMuxeRSubtitleFontSize = TsMuxeRSubtitleFontSize;
            _configService.TSMuxeRSubtitleColor = TsMuxeRSubtitleColor.ToString();
            _configService.TSMuxerSubtitleAdditionalBorder = TsMuxerSubtitleAdditionalBorder;
            _configService.TSMuxeRBottomOffset = TsMuxeRBottomOffset;
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
            ReloadButtonEnabled = false;
            StatusLabel = "Reloading Encoder List";
            ShowStatusWindow = true;

            await Task.Run(new Action(LoadEncoders));

            await Task.Run(new Action(CreateEncoderList));

            ReloadButtonEnabled = true;
            StatusLabel = "Ready";
            ShowStatusWindow = false;
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
            EncoderList = new Dictionary<string, string>
            {
                {"x264", _configService.Lastx264Ver},
                {"x264 64bit", _configService.Lastx26464Ver},
                {"ffmpeg", _configService.LastffmpegVer},
                {"ffmpeg 64bit", _configService.Lastffmpeg64Ver},
                {"eac3to", _configService.Lasteac3ToVer},
                {"lsdvd", _configService.LastlsdvdVer},
                {"MkvToolnix", _configService.LastMKVMergeVer},
                {"mplayer", _configService.LastMplayerVer},
                {"tsMuxeR", _configService.LastTSMuxerVer},
                {"MJPEG tools", _configService.LastMJPEGToolsVer},
                {"DVDAuthor", _configService.LastDVDAuthorVer},
                {"MP4Box", _configService.LastMp4BoxVer},
                {"HCenc", _configService.LastHcEncVer},
                {"OggEnc2", _configService.LastOggEncVer},
                {"OggEnc2 Lancer Build", _configService.LastOggEncLancerVer},
                {"NeroAacEnc", _configService.LastNeroAacEncVer},
                {"LAME", _configService.LastLameVer},
                {"LAME 64bit", _configService.LastLame64Ver},
                {"vpxEnc", _configService.LastVpxEncVer},
                {"BDSup2Sub", _configService.LastBDSup2SubVer},
                {"BDInfo Library", $"{bdVersion.Major:g}.{bdVersion.Minor:g}.{bdVersion.Build:g}"},
                {"MediaInfo Library", miVer.Replace("MediaInfoLib - v", string.Empty)},
                {"AviSynth", _configService.LastAviSynthVer}
            };
            ReloadButtonEnabled = true;
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