// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppConfigService.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The Application Config Service
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Services
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using log4net;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// The Application Config Service
    /// </summary>
    public class AppConfigService : IAppConfigService, INotifyPropertyChanged
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AppConfigService));

        private readonly IUserSettingService _settings;

        /// <summary>
        /// The Property Changed Event Handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fired on property change
        /// </summary>
        /// <param name="e"></param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Fired on property change
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="settings"><see cref="IUserSettingService"/></param>
        public AppConfigService(IUserSettingService settings)
        {
            _settings = settings;
        }

        private T GetSetting<T>(string sName)
        {
            return _settings == null ? default(T) : _settings.GetUserSetting<T>(sName);
        }

        private void SetSetting(string sName, object sValue)
        {
            _settings?.SetUserSetting(sName, sValue);
        }

        /// <summary>
        /// Internal decoding Named Pipe
        /// </summary>
        public string DecodeNamedPipeName => $"{GetProductName()}_decodePipe";

        /// <summary>
        /// External decoding Named Pipe
        /// </summary>
        public string DecodeNamedPipeFullName => $@"\\.\pipe\{DecodeNamedPipeName}";

        /// <summary>
        /// Internal encoding Named Pipe
        /// </summary>
        public string EncodeNamedPipeName => $"{GetProductName()}_encodePipe";

        /// <summary>
        /// External encoding Named Pipe
        /// </summary>
        public string EncodeNamedPipeFullName => $@"\\.\pipe\{EncodeNamedPipeName}";

        /// <summary>
        /// Use MT-Enabled AviSynth
        /// </summary>
        public bool UseAviSynthMT
        {
            get { return GetSetting<bool>(SettingConstants.UseAviSynthMt); }
            set { SetSetting(SettingConstants.UseAviSynthMt, value); }
        }

        /// <summary>
        /// Use High Quality Deinterlacing
        /// </summary>
        public bool UseHQDeinterlace
        {
            get { return GetSetting<bool>(SettingConstants.UseHqDeinterlace); }
            set { SetSetting(SettingConstants.UseHqDeinterlace, value); }
        }

        /// <summary>
        /// Enable SSIF Scanning with BDInfoLib
        /// </summary>
        public bool EnableSSIF
        {
            get { return GetSetting<bool>(SettingConstants.EnableSsif); }
            set
            {
                SetSetting(SettingConstants.EnableSsif, value);
            }
        }

        /// <summary>
        /// Enable filtering of looping Playlists with BDInfoLib
        /// </summary>
        public bool FilterLoopingPlaylists
        {
            get { return GetSetting<bool>(SettingConstants.FilterLoopingPlaylists); }
            set
            {
                SetSetting(SettingConstants.FilterLoopingPlaylists, value);
            }
        }

        /// <summary>
        /// Enable short playlist filtering with BDInfoLib
        /// </summary>
        public bool FilterShortPlaylists
        {
            get { return GetSetting<bool>(SettingConstants.FilterShortPlaylists); }
            set
            {
                SetSetting(SettingConstants.FilterShortPlaylists, value);
            }
        }

        /// <summary>
        /// Minimal playlist length
        /// </summary>
        public int FilterShortPlaylistsValue
        {
            get { return GetSetting<int>(SettingConstants.FilterShortPlaylistsValue); }
            set
            {
                SetSetting(SettingConstants.FilterShortPlaylistsValue, value);
            }
        }

        /// <summary>
        /// Keep original Stream order
        /// </summary>
        public bool KeepStreamOrder
        {
            get { return GetSetting<bool>(SettingConstants.KeepStreamOrder); }
            set
            {
                SetSetting(SettingConstants.KeepStreamOrder, value);
            }
        }

        /// <summary>
        /// Location of encoder executables
        /// </summary>
        public string ToolsPath
        {
            get
            {
                var tPath = GetSetting<string>(SettingConstants.ToolsPath);
                if (string.IsNullOrEmpty(tPath))
                    tPath = Path.Combine(CommonAppSettingsPath, "codecs");
                return tPath;
            }

            set { SetSetting(SettingConstants.ToolsPath, value); }
        }

        /// <summary>
        /// Path to java.exe
        /// </summary>
        public string JavaInstallPath
        {
            get { return GetSetting<string>(SettingConstants.JavaInstallPath); }
            set { SetSetting(SettingConstants.JavaInstallPath, value); }
        }

        /// <summary>
        /// Is Java installed?
        /// </summary>
        public bool JavaInstalled => !string.IsNullOrEmpty(JavaInstallPath);

        /// <summary>
        /// Path to output files
        /// </summary>
        public string OutputLocation
        {
            get { return GetSetting<string>(SettingConstants.OutputLocation); }
            set { SetSetting(SettingConstants.OutputLocation, value); }
        }

        /// <summary>
        /// Temp files location
        /// </summary>
        public string DemuxLocation
        {
            get { return GetSetting<string>(SettingConstants.DemuxLocation); }
            set { SetSetting(SettingConstants.DemuxLocation, value); }
        }

        /// <summary>
        /// Last detected x264 version
        /// </summary>
        public string Lastx264Ver
        {
            get { return GetSetting<string>(SettingConstants.Lastx264Ver); }
            set { SetSetting(SettingConstants.Lastx264Ver, value); }
        }

        /// <summary>
        /// Is x264 installed?
        /// </summary>
        public bool X264Installed => !string.IsNullOrEmpty(Lastx264Ver);

        /// <summary>
        /// Last detected x264 version - 64bit build
        /// </summary>
        public string Lastx26464Ver
        {
            get { return GetSetting<string>(SettingConstants.Lastx26464Ver); }
            set { SetSetting(SettingConstants.Lastx26464Ver, value); }
        }

        /// <summary>
        /// Is 64bit build of x264 installed?
        /// </summary>
        public bool X26464Installed => !string.IsNullOrEmpty(Lastx26464Ver);

        /// <summary>
        /// Last detected ffmpeg version
        /// </summary>
        public string LastffmpegVer
        {
            get { return GetSetting<string>(SettingConstants.LastffmpegVer); }
            set { SetSetting(SettingConstants.LastffmpegVer, value); }
        }

        /// <summary>
        /// Is ffmpeg installed?
        /// </summary>
        public bool FfmpegInstalled => !string.IsNullOrEmpty(LastffmpegVer);

        /// <summary>
        /// Last detected 64bit ffmpeg version
        /// </summary>
        public string Lastffmpeg64Ver
        {
            get { return GetSetting<string>(SettingConstants.Lastffmpeg64Ver); }
            set { SetSetting(SettingConstants.Lastffmpeg64Ver, value); }
        }

        /// <summary>
        /// Is 64bit ffmpeg installed?
        /// </summary>
        public bool Ffmpeg64Installed => !string.IsNullOrEmpty(Lastffmpeg64Ver);

        /// <summary>
        /// Last detected eac3to version
        /// </summary>
        public string Lasteac3ToVer
        {
            get { return GetSetting<string>(SettingConstants.Lasteac3ToVer); }
            set { SetSetting(SettingConstants.Lasteac3ToVer, value); }
        }

        /// <summary>
        /// Is eac3to installed
        /// </summary>
        public bool Eac3ToInstalled => !string.IsNullOrEmpty(Lasteac3ToVer);

        /// <summary>
        /// Last detected hcEnc version
        /// </summary>
        public string LastHcEncVer
        {
            get { return GetSetting<string>(SettingConstants.LastHcEncVer); }
            set { SetSetting(SettingConstants.LastHcEncVer, value); }
        }

        /// <summary>
        /// Is hcEnc installed?
        /// </summary>
        public bool HcEncInstalled => !string.IsNullOrEmpty(LastHcEncVer);

        /// <summary>
        /// Last detected lsdvd version
        /// </summary>
        public string LastlsdvdVer
        {
            get { return GetSetting<string>(SettingConstants.LastlsdvdVer); }
            set { SetSetting(SettingConstants.LastlsdvdVer, value); }
        }

        /// <summary>
        /// Is lsdvd installed?
        /// </summary>
        public bool LsDvdInstalled => !string.IsNullOrEmpty(LastlsdvdVer);

        /// <summary>
        /// Last detected mkvmerge version
        /// </summary>
        public string LastMKVMergeVer
        {
            get { return GetSetting<string>(SettingConstants.LastMkvMergeVer); }
            set { SetSetting(SettingConstants.LastMkvMergeVer, value); }
        }

        /// <summary>
        /// Is mkvmerge installed?
        /// </summary>
        public bool MKVMergeInstalled => !string.IsNullOrEmpty(LastMKVMergeVer);

        /// <summary>
        /// Last detected mplayer version
        /// </summary>
        public string LastMplayerVer
        {
            get { return GetSetting<string>(SettingConstants.LastMplayerVer); }
            set { SetSetting(SettingConstants.LastMplayerVer, value); }
        }

        /// <summary>
        /// Is mplayer installed?
        /// </summary>
        public bool MplayerInstalled => !string.IsNullOrEmpty(LastMplayerVer);

        /// <summary>
        /// Last detected tsMuxeR version
        /// </summary>
        public string LastTSMuxerVer
        {
            get { return GetSetting<string>(SettingConstants.LastTsMuxerVer); }
            set { SetSetting(SettingConstants.LastTsMuxerVer, value); }
        }

        /// <summary>
        /// Is tsMuxeR installed?
        /// </summary>
        public bool TsMuxerInstalled => !string.IsNullOrEmpty(LastTSMuxerVer);

        /// <summary>
        /// Last detected AviSynth version
        /// </summary>
        public string LastAviSynthVer
        {
            get { return GetSetting<string>(SettingConstants.LastAviSynthVer); }
            set { SetSetting(SettingConstants.LastAviSynthVer, value); }
        }

        /// <summary>
        /// Is AviSynth installed?
        /// </summary>
        public bool AviSynthInstalled => !string.IsNullOrEmpty(LastAviSynthVer);

        /// <summary>
        /// Last detected AviSynth plugins version
        /// </summary>
        public string LastAviSynthPluginsVer
        {
            get { return GetSetting<string>(SettingConstants.LastAviSynthPluginsVer); }
            set { SetSetting(SettingConstants.LastAviSynthPluginsVer, value); }
        }

        /// <summary>
        /// Last detected BDSup2Sub version
        /// </summary>
        public string LastBDSup2SubVer
        {
            get { return GetSetting<string>(SettingConstants.LastBdSup2SubVer); }
            set { SetSetting(SettingConstants.LastBdSup2SubVer, value); }
        }

        /// <summary>
        /// Is BDSup2Sub installed?
        /// </summary>
        public bool BDSup2SubInstalled => !string.IsNullOrEmpty(LastBDSup2SubVer);

        /// <summary>
        /// Last detected mp4box version
        /// </summary>
        public string LastMp4BoxVer
        {
            get { return GetSetting<string>(SettingConstants.LastMp4BoxVer); }
            set { SetSetting(SettingConstants.LastMp4BoxVer, value); }
        }

        /// <summary>
        /// Is mp4box installed?
        /// </summary>
        public bool MP4BoxInstalled => !string.IsNullOrEmpty(LastMp4BoxVer);

        /// <summary>
        /// Last detected mjpeg tools version
        /// </summary>
        public string LastMJPEGToolsVer
        {
            get { return GetSetting<string>(SettingConstants.LastMjpegToolsVer); }
            set { SetSetting(SettingConstants.LastMjpegToolsVer, value); }
        }

        /// <summary>
        /// Is mjpeg tools installed?
        /// </summary>
        public bool MjpegToolsInstalled => !string.IsNullOrEmpty(LastMJPEGToolsVer);

        /// <summary>
        /// Last detected DVDAuthor version
        /// </summary>
        public string LastDVDAuthorVer
        {
            get { return GetSetting<string>(SettingConstants.LastDvdAuthorVer); }
            set { SetSetting(SettingConstants.LastDvdAuthorVer, value); }
        }

        /// <summary>
        /// Is DVDAuthor installed?
        /// </summary>
        public bool DVDAuthorInstalled => !string.IsNullOrEmpty(LastDVDAuthorVer);

        /// <summary>
        /// Last detected oggenc version
        /// </summary>
        public string LastOggEncVer
        {
            get { return GetSetting<string>(SettingConstants.LastOggEncVer); }
            set { SetSetting(SettingConstants.LastOggEncVer, value); }
        }

        /// <summary>
        /// Is oggenc installed?
        /// </summary>
        public bool OggEncInstalled => !string.IsNullOrEmpty(LastOggEncVer);

        /// <summary>
        /// Last detected Lancer build of oggenc
        /// </summary>
        public string LastOggEncLancerVer
        {
            get { return GetSetting<string>(SettingConstants.LastOggEncLancerVer); }
            set { SetSetting(SettingConstants.LastOggEncLancerVer, value); }
        }

        /// <summary>
        /// Is Lancer build of oggenc installed?
        /// </summary>
        public bool OggEncLancerInstalled => !string.IsNullOrEmpty(LastOggEncLancerVer);

        /// <summary>
        /// Last detected NeroAacEnc version
        /// </summary>
        public string LastNeroAacEncVer
        {
            get { return GetSetting<string>(SettingConstants.LastNeroAacEncVer); }
            set { SetSetting(SettingConstants.LastNeroAacEncVer, value); }
        }

        /// <summary>
        /// Is NeroAacEnc installed?
        /// </summary>
        public bool NeroAacEncInstalled => !string.IsNullOrEmpty(LastNeroAacEncVer);

        /// <summary>
        /// Last detected Lame version
        /// </summary>
        public string LastLameVer
        {
            get { return GetSetting<string>(SettingConstants.LastLameVer); }
            set { SetSetting(SettingConstants.LastLameVer, value); }
        }

        /// <summary>
        /// Is Lame installed?
        /// </summary>
        public bool LameInstalled => !string.IsNullOrEmpty(LastLameVer);

        /// <summary>
        /// Last detected 64bit Lame version
        /// </summary>
        public string LastLame64Ver
        {
            get { return GetSetting<string>(SettingConstants.LastLame64Ver); }
            set { SetSetting(SettingConstants.LastLame64Ver, value); }
        }

        /// <summary>
        /// Is 64bit Lame installed?
        /// </summary>
        public bool Lame64Installed => !string.IsNullOrEmpty(LastLame64Ver);

        /// <summary>
        /// Last detected VpxEnc version
        /// </summary>
        public string LastVpxEncVer
        {
            get { return GetSetting<string>(SettingConstants.LastVpxEncVer); }
            set { SetSetting(SettingConstants.LastVpxEncVer, value); }
        }

        /// <summary>
        /// Is VpxEnc installed?
        /// </summary>
        public bool VpxEncInstalled => !string.IsNullOrEmpty(LastVpxEncVer);

        /// <summary>
        /// Is this the first time application launch?
        /// </summary>
        public bool FirstStart
        {
            get { return GetSetting<bool>(SettingConstants.FirstStart); }
            set { SetSetting(SettingConstants.FirstStart, value); }
        }

        /// <summary>
        /// Reload encoder versions
        /// </summary>
        public bool ReloadToolVersions
        {
            get { return GetSetting<bool>(SettingConstants.ReloadToolVersions); }
            set { SetSetting(SettingConstants.ReloadToolVersions, value); }
        }

        /// <summary>
        /// Use Async I/0 with tsMuxeR
        /// </summary>
        public bool TSMuxeRUseAsyncIO
        {
            get { return GetSetting<bool>(SettingConstants.TsMuxeRUseAsyncIO); }
            set { SetSetting(SettingConstants.TsMuxeRUseAsyncIO, value); }
        }

        /// <summary>
        /// Set Blu-Ray audio PES with tsMuxeR
        /// </summary>
        public bool TSMuxeRBlurayAudioPES
        {
            get { return GetSetting<bool>(SettingConstants.TsMuxeRBlurayAudioPes); }
            set { SetSetting(SettingConstants.TsMuxeRBlurayAudioPes, value); }
        }

        /// <summary>
        /// Subtitle additional Border
        /// </summary>
        public int TSMuxerSubtitleAdditionalBorder
        {
            get { return GetSetting<int>(SettingConstants.TsMuxerSubtitleAdditionalBorder); }
            set { SetSetting(SettingConstants.TsMuxerSubtitleAdditionalBorder, value); }
        }

        /// <summary>
        /// Subtitle bottom Offset
        /// </summary>
        public int TSMuxeRBottomOffset
        {
            get { return GetSetting<int>(SettingConstants.TsMuxeRBottomOffset); }
            set { SetSetting(SettingConstants.TsMuxeRBottomOffset, value); }
        }

        /// <summary>
        /// Subtitle font
        /// </summary>
        public string TSMuxeRSubtitleFont
        {
            get { return GetSetting<string>(SettingConstants.TsMuxeRSubtitleFont); }
            set { SetSetting(SettingConstants.TsMuxeRSubtitleFont, value); }
        }

        /// <summary>
        /// Subtitle font color
        /// </summary>
        public string TSMuxeRSubtitleColor
        {
            get { return GetSetting<string>(SettingConstants.TsMuxeRSubtitleColor); }
            set { SetSetting(SettingConstants.TsMuxeRSubtitleColor, value); }
        }

        /// <summary>
        /// Subtitle font size
        /// </summary>
        public int TSMuxeRSubtitleFontSize
        {
            get { return GetSetting<int>(SettingConstants.TsMuxeRSubtitleFontSize); }
            set { SetSetting(SettingConstants.TsMuxeRSubtitleFontSize, value); }
        }

        /// <summary>
        /// Add picture timing info for video tracks
        /// </summary>
        public bool TSMuxeRVideoTimingInfo
        {
            get { return GetSetting<bool>(SettingConstants.TsMuxeRVideoTimingInfo); }
            set { SetSetting(SettingConstants.TsMuxeRVideoTimingInfo, value); }
        }

        /// <summary>
        /// Continually insert SPS/PPS for video tracks
        /// </summary>
        public bool TSMuxeRAddVideoPPS
        {
            get { return GetSetting<bool>(SettingConstants.TsMuxeRAddVideoPps); }
            set { SetSetting(SettingConstants.TsMuxeRAddVideoPps, value); }
        }

        /// <summary>
        /// Remove completed jobs from list
        /// </summary>
        public bool DeleteCompletedJobs
        {
            get { return GetSetting<bool>(SettingConstants.DeleteCompletedJobs); }
            set { SetSetting(SettingConstants.DeleteCompletedJobs, value); }
        }

        /// <summary>
        /// Process priority of encoding processes
        /// </summary>
        public int ProcessPriority
        {
            get { return GetSetting<int>(SettingConstants.ProcessPriority); }
            set { SetSetting(SettingConstants.ProcessPriority, value); }
        }

        /// <summary>
        /// Delete temp files
        /// </summary>
        public bool DeleteTemporaryFiles
        {
            get { return GetSetting<bool>(SettingConstants.DeleteTemporaryFiles); }
            set { SetSetting(SettingConstants.DeleteTemporaryFiles, value); }
        }

        /// <summary>
        /// Enable debugging
        /// </summary>
        public bool UseDebug
        {
            get { return GetSetting<bool>(SettingConstants.UseDebug); }
            set
            {
                SetSetting(SettingConstants.UseDebug, value);
                OnPropertyChanged("UseDebug");
            }
        }

        /// <summary>
        /// Make use of 64bit encoders
        /// </summary>
        public bool Use64BitEncoders
        {
            get { return GetSetting<bool>(SettingConstants.Use64BitEncoders); }
            set { SetSetting(SettingConstants.Use64BitEncoders, value); }
        }

        /// <summary>
        /// Make use of optimized encoders
        /// </summary>
        public bool UseOptimizedEncoders
        {
            get { return GetSetting<bool>(SettingConstants.UseOptimizedEncoders); }
            set { SetSetting(SettingConstants.UseOptimizedEncoders, value); }
        }

        /// <summary>
        /// Enable WPF hardware rendering
        /// </summary>
        public bool UseHardwareRendering
        {
            get { return GetSetting<bool>(SettingConstants.UseHardwareRendering); }
            set { SetSetting(SettingConstants.UseHardwareRendering, value); }
        }

        /// <summary>
        /// Set application language
        /// </summary>
        public string UseLanguage
        {
            get { return GetSetting<string>(SettingConstants.UseLanguage); }
            set { SetSetting(SettingConstants.UseLanguage, value); }
        }

        /// <summary>
        /// Last selected encoding profile
        /// </summary>
        public string LastSelectedProfile
        {
            get { return GetSetting<string>(SettingConstants.LastSelectedProfile); }
            set { SetSetting(SettingConstants.LastSelectedProfile, value); }
        }

        /// <summary>
        /// Last detected profile list version
        /// </summary>
        public string LastProfilesVer
        {
            get { return GetSetting<string>(SettingConstants.LastProfilesVer); }
            set { SetSetting(SettingConstants.LastProfilesVer, value); }
        }

        /// <summary>
        /// Update versions
        /// </summary>
        public bool UpdateVersions
        {
            get { return GetSetting<bool>(SettingConstants.UpdateVersions); }
            set { SetSetting(SettingConstants.UpdateVersions, value); }
        }

        /// <summary>
        /// Update checking frequency
        /// </summary>
        public int UpdateFrequency
        {
            get { return GetSetting<int>(SettingConstants.UpdateFrequency); }
            set { SetSetting(SettingConstants.UpdateFrequency, value); }
        }

        /// <summary>
        /// Date of last update
        /// </summary>
        public DateTime LastUpdateRun
        {
            get { return GetSetting<DateTime>(SettingConstants.LastUpdateRun); }
            set
            {
                SetSetting(SettingConstants.LastUpdateRun, value);
                OnPropertyChanged("LastUpdateRun");
            }
        }

        /// <summary>
        /// Show changelog after update
        /// </summary>
        public bool ShowChangeLog
        {
            get { return GetSetting<bool>(SettingConstants.ShowChangeLog); }
            set { SetSetting(SettingConstants.ShowChangeLog, value); }
        }

        /// <summary>
        /// Create XBMC info file (single DB entry)
        /// </summary>
        public bool CreateXbmcInfoFile
        {
            get { return GetSetting<bool>(SettingConstants.CreateXbmcInfoFile); }
            set { SetSetting(SettingConstants.CreateXbmcInfoFile, value); }
        }

        /// <summary>
        /// Last selected language for MovieDB
        /// </summary>
        public string MovieDBLastLanguage
        {
            get { return GetSetting<string>(SettingConstants.MovieDbLastLanguage); }
            set { SetSetting(SettingConstants.MovieDbLastLanguage, value); }
        }

        /// <summary>
        /// Last selected Rating country for MovieDB
        /// </summary>
        public string MovieDBLastRatingCountry
        {
            get { return GetSetting<string>(SettingConstants.MovieDbLastRatingCountry); }
            set { SetSetting(SettingConstants.MovieDbLastRatingCountry, value); }
        }

        /// <summary>
        /// Last selected fallback language for MovieDB
        /// </summary>
        public string MovieDBLastFallbackLanguage
        {
            get { return GetSetting<string>(SettingConstants.MovieDbLastFallbackLanguage); }
            set { SetSetting(SettingConstants.MovieDbLastFallbackLanguage, value); }
        }

        /// <summary>
        /// Last selected fallback rating country for MovieDB
        /// </summary>
        public string MovieDBLastFallbackRatingCountry
        {
            get { return GetSetting<string>(SettingConstants.MovieDbLastFallbackRatingCountry); }
            set { SetSetting(SettingConstants.MovieDbLastFallbackRatingCountry, value); }
        }

        /// <summary>
        /// Preferred certification prefix for MovieDB
        /// </summary>
        public string MovieDBPreferredCertPrefix
        {
            get { return GetSetting<string>(SettingConstants.MovieDbPreferredCertPrefix); }
            set { SetSetting(SettingConstants.MovieDbPreferredCertPrefix, value); }
        }

        /// <summary>
        /// Fallback certification prefix for MovieDB
        /// </summary>
        public string MovieDBFallbackCertPrefix
        {
            get { return GetSetting<string>(SettingConstants.MovieDbFallbackCertPrefix); }
            set { SetSetting(SettingConstants.MovieDbFallbackCertPrefix, value); }
        }

        /// <summary>
        /// Rating source for MovieDB
        /// </summary>
        public int MovieDBRatingSrc
        {
            get { return GetSetting<int>(SettingConstants.MovieDbRatingSrc); }
            set { SetSetting(SettingConstants.MovieDbRatingSrc, value); }
        }

        /// <summary>
        /// Cache path for TvDBLib
        /// </summary>
        public string TvDBCachePath
        {
            get
            {
                var strPath = GetSetting<string>(SettingConstants.TvDbCachePath);

                if (!string.IsNullOrEmpty(strPath)) return strPath;

                strPath = Path.Combine(CommonAppSettingsPath, "TvDBCache");
                if (!Directory.Exists(strPath))
                    Directory.CreateDirectory(strPath, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));

                return strPath;
            }
            set { SetSetting(SettingConstants.TvDbCachePath, value); }
        }

        /// <summary>
        /// Parse string for TVDBLib
        /// </summary>
        public string TvDBParseString
        {
            get
            {
                var strValue = GetSetting<string>(SettingConstants.TvDbParseString);
                if (string.IsNullOrEmpty(strValue))
                    strValue = "%show% - S%season%E%episode% - %episode_name%";
                return strValue;
            }
            set { SetSetting(SettingConstants.TvDbParseString, value); }
        }

        /// <summary>
        /// Preferred language for TVDBLib
        /// </summary>
        public string TvDBPreferredLanguage
        {
            get { return GetSetting<string>(SettingConstants.TvDbPreferredLanguage); }
            set { SetSetting(SettingConstants.TvDbPreferredLanguage, value); }
        }

        /// <summary>
        /// Fallback language for TVDBLib
        /// </summary>
        public string TvDBFallbackLanguage
        {
            get { return GetSetting<string>(SettingConstants.TvDbFallbackLanguage); }
            set { SetSetting(SettingConstants.TvDbFallbackLanguage, value); }
        }

        /// <summary>
        /// Last selected scraping source
        /// </summary>
        public int LastSelectedSource
        {
            get { return GetSetting<int>(SettingConstants.LastSelectedSource); }
            set { SetSetting(SettingConstants.LastSelectedSource, value); }
        }

        /// <summary>
        /// Enable ffmpeg scaling/cropping
        /// </summary>
        public bool UseFfmpegScaling
        {
            get { return GetSetting<bool>(SettingConstants.UseFfmpegScaling); }
            set { SetSetting(SettingConstants.UseFfmpegScaling, value); }
        }

        /// <summary>
        /// Limit decoding threads for ffms
        /// </summary>
        public bool LimitDecoderThreads
        {
            get { return GetSetting<bool>(SettingConstants.LimitDecoderThreads); }
            set { SetSetting(SettingConstants.LimitDecoderThreads, value); }
        }

        /// <summary>
        /// List of supported CPU extensions
        /// </summary>
        public Extensions SupportedCpuExtensions { get; set; }

        /// <summary>
        /// Path to Application executable
        /// </summary>
        public string AppPath
        {
            get
            {
                try
                {
                    return Path.GetDirectoryName(GetAssembly().Location);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Location where all the settings are stored
        /// </summary>
        public string AppSettingsPath
        {
            get
            {
                try
                {
                    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    appDataPath = Path.Combine(appDataPath, GetCompanyName(), GetProductName());
                    return appDataPath;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Get <see cref="AppSettingsPath"/>
        /// </summary>
        /// <returns><see cref="AppSettingsPath"/></returns>
        public static string GetAppSettingsPath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            appDataPath = Path.Combine(appDataPath, GetCompanyName(), GetProductName());
            return appDataPath;
        }

        /// <summary>
        /// Path to Common Application Data
        /// </summary>
        public string CommonAppSettingsPath
        {
            get
            {
                try
                {
                    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    appDataPath = Path.Combine(appDataPath, GetCompanyName(), GetProductName());
                    return appDataPath;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// System temporary files folder
        /// </summary>
        public string TempPath
        {
            get
            {
                try
                {
                    var tempPath = Path.GetTempPath();
                    tempPath = Path.Combine(tempPath, GetProductName() + "-" + Guid.NewGuid());
                    return tempPath;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Global culture info
        /// </summary>
        public CultureInfo CInfo => CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");

        /// <summary>
        /// Version of our Updater
        /// </summary>
        public Version UpdaterVersion
        {
            get
            {
                var result = new Version(0, 0, 0, 0);
                var tVersion = GetSetting<string>(SettingConstants.UpdaterVersion);
                try
                {
                    result = Version.Parse(tVersion);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }

                return result;
            }
            set
            {
                var tVersion = $"{value.Major:0}.{value.Minor:0}.{value.Build:0}.{value.Revision:0}";
                SetSetting(SettingConstants.UpdaterVersion, tVersion);
            }
        }

        /// <summary>
        /// Path to our Updater
        /// </summary>
        public string UpdaterPath => Path.Combine(CommonAppSettingsPath, "Updater");

        /// <summary>
        /// Path to AviSynth plugins
        /// </summary>
        public string AvsPluginsPath => Path.Combine(AppPath, "AvsPlugins");

        /// <summary>
        /// Get Process Priority from stored setting
        /// </summary>
        /// <returns></returns>
        public ProcessPriorityClass GetProcessPriority()
        {
            switch (ProcessPriority)
            {
                case 0:
                    return ProcessPriorityClass.RealTime;
                case 1:
                    return ProcessPriorityClass.High;
                case 2:
                    return ProcessPriorityClass.AboveNormal;
                default:
                    return ProcessPriorityClass.Normal;
                case 4:
                    return ProcessPriorityClass.BelowNormal;
                case 5:
                    return ProcessPriorityClass.Idle;
            }
        }

        /// <summary>
        /// Get Thread Priority from stored setting
        /// </summary>
        /// <returns></returns>
        public ThreadPriority GetThreadPriority()
        {
            switch (ProcessPriority)
            {
                case 0:
                case 1: 
                    return ThreadPriority.Highest;
                case 2:
                    return ThreadPriority.AboveNormal;
                default:
                    return ThreadPriority.Normal;
                case 4:
                    return ThreadPriority.BelowNormal;
                case 5:
                    return ThreadPriority.Lowest;
            }
        }

        /// <summary>
        /// returns the Version object containing AssemblyVersion of this application
        /// </summary>
        /// <returns><see cref="Version"/></returns>
        public static Version GetAppVersion()
        {
            return GetAssembly().GetName().Version;
        }

        private static Assembly GetAssembly()
        {
            return Assembly.GetEntryAssembly();
        }

        /// <summary>
        /// Get Version String of Main Application executable
        /// </summary>
        /// <returns>Version String of Main Application executable</returns>
        public static string GetAppVersionStr()
        {
            var version = GetAppVersion();
            return $"{version.Major:0}.{version.Minor:0}.{version.Build:0}.{version.Revision:0}";
        }

        /// <summary>
        /// Get Company Name from Main Application executable
        /// </summary>
        /// <returns>Company Name</returns>
        public static string GetCompanyName()
        {
            var companyName = string.Empty;

            var myAssembly = GetAssembly();
            var attributes = myAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);

            if (attributes.Length <= 0) return companyName;

            var aca = attributes[0] as AssemblyCompanyAttribute;

            if (aca != null)
                companyName = aca.Company;

            return companyName;
        }

        /// <summary>
        /// Get Product Name from Main Application executable
        /// </summary>
        /// <returns>Product Name</returns>
        public static string GetProductName()
        {
            var productName = string.Empty;

            var myAssembly = GetAssembly();
            var attributes = myAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);

            if (attributes.Length <= 0) return productName;

            var apa = attributes[0] as AssemblyProductAttribute;

            if (apa != null)
                productName = apa.Product;
            return productName;
        }
    }
}
