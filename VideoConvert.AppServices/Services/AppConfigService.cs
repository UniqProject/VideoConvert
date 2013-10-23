// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppConfigService.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
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
    using Interfaces;
    using Interop.Model;
    using Interop.Utilities;
    using log4net;

    public class AppConfigService : IAppConfigService, INotifyPropertyChanged
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AppConfigService));

        private readonly IUserSettingService _settings;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public AppConfigService(IUserSettingService settings)
        {
            this._settings = settings;
        }

        private T GetSetting<T>(string sName)
        {
            if (_settings == null) return default(T);

            return _settings.GetUserSetting<T>(sName);
        }

        private void SetSetting(string sName, object sValue)
        {
            if (_settings == null) return;

            _settings.SetUserSetting(sName, sValue);
        }

        public string DecodeNamedPipeName
        {
            get { return String.Format("{0}_decodePipe", GetProductName()); }
        }

        public string DecodeNamedPipeFullName
        {
            get { return String.Format(@"\\.\pipe\{0}", DecodeNamedPipeName); }
        }

        public string EncodeNamedPipeName
        {
            get { return String.Format("{0}_encodePipe", GetProductName()); }
        }

        public string EncodeNamedPipeFullName
        {
            get { return String.Format(@"\\.\pipe\{0}", EncodeNamedPipeName); }
        }

        public bool UseAviSynthMT
        {
            get { return GetSetting<bool>(SettingConstants.UseAviSynthMT); }
            set { SetSetting(SettingConstants.UseAviSynthMT, value); }
        }

        public bool UseHQDeinterlace
        {
            get { return GetSetting<bool>(SettingConstants.UseHQDeinterlace); }
            set { SetSetting(SettingConstants.UseHQDeinterlace, value); }
        }

        public bool EnableSSIF
        {
            get { return GetSetting<bool>(SettingConstants.EnableSSIF); }
            set
            {
                SetSetting(SettingConstants.EnableSSIF, value);
            }
        }

        public bool FilterLoopingPlaylists
        {
            get { return GetSetting<bool>(SettingConstants.FilterLoopingPlaylists); }
            set
            {
                SetSetting(SettingConstants.FilterLoopingPlaylists, value);
            }
        }

        public bool FilterShortPlaylists
        {
            get { return GetSetting<bool>(SettingConstants.FilterShortPlaylists); }
            set
            {
                SetSetting(SettingConstants.FilterShortPlaylists, value);
            }
        }

        public int FilterShortPlaylistsValue
        {
            get { return GetSetting<int>(SettingConstants.FilterShortPlaylistsValue); }
            set
            {
                SetSetting(SettingConstants.FilterShortPlaylistsValue, value);
            }
        }

        public bool KeepStreamOrder
        {
            get { return GetSetting<bool>(SettingConstants.KeepStreamOrder); }
            set
            {
                SetSetting(SettingConstants.KeepStreamOrder, value);
            }
        }

        public string ToolsPath
        {
            get
            {
                string tPath = GetSetting<string>(SettingConstants.ToolsPath);
                if (String.IsNullOrEmpty(tPath))
                    tPath = Path.Combine(CommonAppSettingsPath, "codecs");
                return tPath;
            }

            set { SetSetting(SettingConstants.ToolsPath, value); }
        }

        public string JavaInstallPath
        {
            get { return GetSetting<string>(SettingConstants.JavaInstallPath); }
            set { SetSetting(SettingConstants.JavaInstallPath, value); }
        }

        public bool JavaInstalled
        {
            get { return !String.IsNullOrEmpty(JavaInstallPath); }
        }

        public string OutputLocation
        {
            get { return GetSetting<string>(SettingConstants.OutputLocation); }
            set { SetSetting(SettingConstants.OutputLocation, value); }
        }

        public string DemuxLocation
        {
            get { return GetSetting<string>(SettingConstants.DemuxLocation); }
            set { SetSetting(SettingConstants.DemuxLocation, value); }
        }

        public string Lastx264Ver
        {
            get { return GetSetting<string>(SettingConstants.Lastx264Ver); }
            set { SetSetting(SettingConstants.Lastx264Ver, value); }
        }

        public bool X264Installed
        {
            get { return !String.IsNullOrEmpty(Lastx264Ver); }
        }

        public string Lastx26464Ver
        {
            get { return GetSetting<string>(SettingConstants.Lastx26464Ver); }
            set { SetSetting(SettingConstants.Lastx26464Ver, value); }
        }

        public bool X26464Installed
        {
            get { return !String.IsNullOrEmpty(Lastx26464Ver); }
        }

        public string LastffmpegVer
        {
            get { return GetSetting<string>(SettingConstants.LastffmpegVer); }
            set { SetSetting(SettingConstants.LastffmpegVer, value); }
        }

        public bool FfmpegInstalled
        {
            get { return !String.IsNullOrEmpty(LastffmpegVer); }
        }

        public string Lastffmpeg64Ver
        {
            get { return GetSetting<string>(SettingConstants.Lastffmpeg64Ver); }
            set { SetSetting(SettingConstants.Lastffmpeg64Ver, value); }
        }

        public bool Ffmpeg64Installed
        {
            get { return !String.IsNullOrEmpty(Lastffmpeg64Ver); }
        }

        public string Lasteac3ToVer
        {
            get { return GetSetting<string>(SettingConstants.Lasteac3ToVer); }
            set { SetSetting(SettingConstants.Lasteac3ToVer, value); }
        }

        public bool Eac3ToInstalled
        {
            get { return !String.IsNullOrEmpty(Lasteac3ToVer); }
        }

        public string LastHcEncVer
        {
            get { return GetSetting<string>(SettingConstants.LastHcEncVer); }
            set { SetSetting(SettingConstants.LastHcEncVer, value); }
        }

        public bool HcEncInstalled
        {
            get { return !String.IsNullOrEmpty(LastHcEncVer); }
        }

        public string LastlsdvdVer
        {
            get { return GetSetting<string>(SettingConstants.LastlsdvdVer); }
            set { SetSetting(SettingConstants.LastlsdvdVer, value); }
        }

        public bool LsDvdInstalled
        {
            get { return !String.IsNullOrEmpty(LastlsdvdVer); }
        }

        public string LastMKVMergeVer
        {
            get { return GetSetting<string>(SettingConstants.LastMKVMergeVer); }
            set { SetSetting(SettingConstants.LastMKVMergeVer, value); }
        }

        public bool MKVMergeInstalled
        {
            get { return !String.IsNullOrEmpty(LastMKVMergeVer); }
        }

        public string LastMplayerVer
        {
            get { return GetSetting<string>(SettingConstants.LastMplayerVer); }
            set { SetSetting(SettingConstants.LastMplayerVer, value); }
        }

        public bool MplayerInstalled
        {
            get { return !String.IsNullOrEmpty(LastMplayerVer); }
        }

        public string LastTSMuxerVer
        {
            get { return GetSetting<string>(SettingConstants.LastTSMuxerVer); }
            set { SetSetting(SettingConstants.LastTSMuxerVer, value); }
        }

        public bool TsMuxerInstalled
        {
            get { return !String.IsNullOrEmpty(LastTSMuxerVer); }
        }

        public string LastAviSynthVer
        {
            get { return GetSetting<string>(SettingConstants.LastAviSynthVer); }
            set { SetSetting(SettingConstants.LastAviSynthVer, value); }
        }

        public bool AviSynthInstalled
        {
            get { return !String.IsNullOrEmpty(LastAviSynthVer); }
        }

        public string LastAviSynthPluginsVer
        {
            get { return GetSetting<string>(SettingConstants.LastAviSynthPluginsVer); }
            set { SetSetting(SettingConstants.LastAviSynthPluginsVer, value); }
        }

        public string LastBDSup2SubVer
        {
            get { return GetSetting<string>(SettingConstants.LastBDSup2SubVer); }
            set { SetSetting(SettingConstants.LastBDSup2SubVer, value); }
        }

        public bool BDSup2SubInstalled
        {
            get { return !String.IsNullOrEmpty(LastBDSup2SubVer); }
        }

        public string LastMp4BoxVer
        {
            get { return GetSetting<string>(SettingConstants.LastMp4BoxVer); }
            set { SetSetting(SettingConstants.LastMp4BoxVer, value); }
        }

        public bool MP4BoxInstalled
        {
            get { return !String.IsNullOrEmpty(LastMp4BoxVer); }
        }

        public string LastMJPEGToolsVer
        {
            get { return GetSetting<string>(SettingConstants.LastMJPEGToolsVer); }
            set { SetSetting(SettingConstants.LastMJPEGToolsVer, value); }
        }

        public bool MjpegToolsInstalled
        {
            get { return !String.IsNullOrEmpty(LastMJPEGToolsVer); }
        }

        public string LastDVDAuthorVer
        {
            get { return GetSetting<string>(SettingConstants.LastDVDAuthorVer); }
            set { SetSetting(SettingConstants.LastDVDAuthorVer, value); }
        }

        public bool DVDAuthorInstalled
        {
            get { return !String.IsNullOrEmpty(LastDVDAuthorVer); }
        }

        public string LastOggEncVer
        {
            get { return GetSetting<string>(SettingConstants.LastOggEncVer); }
            set { SetSetting(SettingConstants.LastOggEncVer, value); }
        }

        public bool OggEncInstalled
        {
            get { return !String.IsNullOrEmpty(LastOggEncVer); }
        }

        public string LastOggEncLancerVer
        {
            get { return GetSetting<string>(SettingConstants.LastOggEncLancerVer); }
            set { SetSetting(SettingConstants.LastOggEncLancerVer, value); }
        }

        public bool OggEncLancerInstalled
        {
            get { return !String.IsNullOrEmpty(LastOggEncLancerVer); }
        }

        public string LastNeroAacEncVer
        {
            get { return GetSetting<string>(SettingConstants.LastNeroAacEncVer); }
            set { SetSetting(SettingConstants.LastNeroAacEncVer, value); }
        }

        public bool NeroAacEncInstalled
        {
            get { return !String.IsNullOrEmpty(LastNeroAacEncVer); }
        }

        public string LastLameVer
        {
            get { return GetSetting<string>(SettingConstants.LastLameVer); }
            set { SetSetting(SettingConstants.LastLameVer, value); }
        }

        public bool LameInstalled
        {
            get { return !String.IsNullOrEmpty(LastLameVer); }
        }

        public string LastLame64Ver
        {
            get { return GetSetting<string>(SettingConstants.LastLame64Ver); }
            set { SetSetting(SettingConstants.LastLame64Ver, value); }
        }

        public bool Lame64Installed
        {
            get { return !String.IsNullOrEmpty(LastLame64Ver); }
        }

        public string LastVpxEncVer
        {
            get { return GetSetting<string>(SettingConstants.LastVpxEncVer); }
            set { SetSetting(SettingConstants.LastVpxEncVer, value); }
        }

        public bool VpxEncInstalled
        {
            get { return !String.IsNullOrEmpty(LastVpxEncVer); }
        }

        public bool FirstStart
        {
            get { return GetSetting<bool>(SettingConstants.FirstStart); }
            set { SetSetting(SettingConstants.FirstStart, value); }
        }

        public bool ReloadToolVersions
        {
            get { return GetSetting<bool>(SettingConstants.ReloadToolVersions); }
            set { SetSetting(SettingConstants.ReloadToolVersions, value); }
        }

        public bool TSMuxeRUseAsyncIO
        {
            get { return GetSetting<bool>(SettingConstants.TSMuxeRUseAsyncIO); }
            set { SetSetting(SettingConstants.TSMuxeRUseAsyncIO, value); }
        }

        public bool TSMuxeRBlurayAudioPES
        {
            get { return GetSetting<bool>(SettingConstants.TSMuxeRBlurayAudioPES); }
            set { SetSetting(SettingConstants.TSMuxeRBlurayAudioPES, value); }
        }

        public int TSMuxerSubtitleAdditionalBorder
        {
            get { return GetSetting<int>(SettingConstants.TSMuxerSubtitleAdditionalBorder); }
            set { SetSetting(SettingConstants.TSMuxerSubtitleAdditionalBorder, value); }
        }

        public int TSMuxeRBottomOffset
        {
            get { return GetSetting<int>(SettingConstants.TSMuxeRBottomOffset); }
            set { SetSetting(SettingConstants.TSMuxeRBottomOffset, value); }
        }

        public string TSMuxeRSubtitleFont
        {
            get { return GetSetting<string>(SettingConstants.TSMuxeRSubtitleFont); }
            set { SetSetting(SettingConstants.TSMuxeRSubtitleFont, value); }
        }

        public string TSMuxeRSubtitleColor
        {
            get { return GetSetting<string>(SettingConstants.TSMuxeRSubtitleColor); }
            set { SetSetting(SettingConstants.TSMuxeRSubtitleColor, value); }
        }

        public int TSMuxeRSubtitleFontSize
        {
            get { return GetSetting<int>(SettingConstants.TSMuxeRSubtitleFontSize); }
            set { SetSetting(SettingConstants.TSMuxeRSubtitleFontSize, value); }
        }

        public bool TSMuxeRVideoTimingInfo
        {
            get { return GetSetting<bool>(SettingConstants.TSMuxeRVideoTimingInfo); }
            set { SetSetting(SettingConstants.TSMuxeRVideoTimingInfo, value); }
        }

        public bool TSMuxeRAddVideoPPS
        {
            get { return GetSetting<bool>(SettingConstants.TSMuxeRAddVideoPPS); }
            set { SetSetting(SettingConstants.TSMuxeRAddVideoPPS, value); }
        }

        public bool DeleteCompletedJobs
        {
            get { return GetSetting<bool>(SettingConstants.DeleteCompletedJobs); }
            set { SetSetting(SettingConstants.DeleteCompletedJobs, value); }
        }

        public int ProcessPriority
        {
            get { return GetSetting<int>(SettingConstants.ProcessPriority); }
            set { SetSetting(SettingConstants.ProcessPriority, value); }
        }

        public bool DeleteTemporaryFiles
        {
            get { return GetSetting<bool>(SettingConstants.DeleteTemporaryFiles); }
            set { SetSetting(SettingConstants.DeleteTemporaryFiles, value); }
        }

        public bool UseDebug
        {
            get { return GetSetting<bool>(SettingConstants.UseDebug); }
            set
            {
                SetSetting(SettingConstants.UseDebug, value);
                OnPropertyChanged("UseDebug");
            }
        }

        public bool Use64BitEncoders
        {
            get { return GetSetting<bool>(SettingConstants.Use64BitEncoders); }
            set { SetSetting(SettingConstants.Use64BitEncoders, value); }
        }

        public bool UseOptimizedEncoders
        {
            get { return GetSetting<bool>(SettingConstants.UseOptimizedEncoders); }
            set { SetSetting(SettingConstants.UseOptimizedEncoders, value); }
        }

        public bool UseHardwareRendering
        {
            get { return GetSetting<bool>(SettingConstants.UseHardwareRendering); }
            set { SetSetting(SettingConstants.UseHardwareRendering, value); }
        }

        public string UseLanguage
        {
            get { return GetSetting<string>(SettingConstants.UseLanguage); }
            set { SetSetting(SettingConstants.UseLanguage, value); }
        }

        public string LastSelectedProfile
        {
            get { return GetSetting<string>(SettingConstants.LastSelectedProfile); }
            set { SetSetting(SettingConstants.LastSelectedProfile, value); }
        }

        public string LastProfilesVer
        {
            get { return GetSetting<string>(SettingConstants.LastProfilesVer); }
            set { SetSetting(SettingConstants.LastProfilesVer, value); }
        }

        public bool UpdateVersions
        {
            get { return GetSetting<bool>(SettingConstants.UpdateVersions); }
            set { SetSetting(SettingConstants.UpdateVersions, value); }
        }

        public int UpdateFrequency
        {
            get { return GetSetting<int>(SettingConstants.UpdateFrequency); }
            set { SetSetting(SettingConstants.UpdateFrequency, value); }
        }

        public DateTime LastUpdateRun
        {
            get { return GetSetting<DateTime>(SettingConstants.LastUpdateRun); }
            set
            {
                SetSetting(SettingConstants.LastUpdateRun, value);
                OnPropertyChanged("LastUpdateRun");
            }
        }

        public bool ShowChangeLog
        {
            get { return GetSetting<bool>(SettingConstants.ShowChangeLog); }
            set { SetSetting(SettingConstants.ShowChangeLog, value); }
        }

        public bool CreateXbmcInfoFile
        {
            get { return GetSetting<bool>(SettingConstants.CreateXbmcInfoFile); }
            set { SetSetting(SettingConstants.CreateXbmcInfoFile, value); }
        }

        public string MovieDBLastLanguage
        {
            get { return GetSetting<string>(SettingConstants.MovieDBLastLanguage); }
            set { SetSetting(SettingConstants.MovieDBLastLanguage, value); }
        }

        public string MovieDBLastRatingCountry
        {
            get { return GetSetting<string>(SettingConstants.MovieDBLastRatingCountry); }
            set { SetSetting(SettingConstants.MovieDBLastRatingCountry, value); }
        }

        public string MovieDBLastFallbackLanguage
        {
            get { return GetSetting<string>(SettingConstants.MovieDBLastFallbackLanguage); }
            set { SetSetting(SettingConstants.MovieDBLastFallbackLanguage, value); }
        }

        public string MovieDBLastFallbackRatingCountry
        {
            get { return GetSetting<string>(SettingConstants.MovieDBLastFallbackRatingCountry); }
            set { SetSetting(SettingConstants.MovieDBLastFallbackRatingCountry, value); }
        }

        public string MovieDBPreferredCertPrefix
        {
            get { return GetSetting<string>(SettingConstants.MovieDBPreferredCertPrefix); }
            set { SetSetting(SettingConstants.MovieDBPreferredCertPrefix, value); }
        }

        public string MovieDBFallbackCertPrefix
        {
            get { return GetSetting<string>(SettingConstants.MovieDBFallbackCertPrefix); }
            set { SetSetting(SettingConstants.MovieDBFallbackCertPrefix, value); }
        }

        public int MovieDBRatingSrc
        {
            get { return GetSetting<int>(SettingConstants.MovieDBRatingSrc); }
            set { SetSetting(SettingConstants.MovieDBRatingSrc, value); }
        }

        public string TvDBCachePath
        {
            get
            {
                string strPath = GetSetting<string>(SettingConstants.TvDBCachePath);

                if (string.IsNullOrEmpty(strPath))
                {
                    strPath = Path.Combine(CommonAppSettingsPath, "TvDBCache");
                    if (!Directory.Exists(strPath))
                        Directory.CreateDirectory(strPath, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));
                }
                return strPath;
            }
            set { SetSetting(SettingConstants.TvDBCachePath, value); }
        }

        public string TvDBParseString
        {
            get
            {
                string strValue = GetSetting<string>(SettingConstants.TvDBParseString);
                if (string.IsNullOrEmpty(strValue))
                    strValue = "%show% - S%season%E%episode% - %episode_name%";
                return strValue;
            }
            set { SetSetting(SettingConstants.TvDBParseString, value); }
        }

        public string TvDBPreferredLanguage
        {
            get { return GetSetting<string>(SettingConstants.TvDBPreferredLanguage); }
            set { SetSetting(SettingConstants.TvDBPreferredLanguage, value); }
        }

        public string TvDBFallbackLanguage
        {
            get { return GetSetting<string>(SettingConstants.TvDBFallbackLanguage); }
            set { SetSetting(SettingConstants.TvDBFallbackLanguage, value); }
        }

        public int LastSelectedSource
        {
            get { return GetSetting<int>(SettingConstants.LastSelectedSource); }
            set { SetSetting(SettingConstants.LastSelectedSource, value); }
        }

        public bool UseFfmpegScaling
        {
            get { return GetSetting<bool>(SettingConstants.UseFfmpegScaling); }
            set { SetSetting(SettingConstants.UseFfmpegScaling, value); }
        }

        public bool LimitDecoderThreads
        {
            get { return GetSetting<bool>(SettingConstants.LimitDecoderThreads); }
            set { SetSetting(SettingConstants.LimitDecoderThreads, value); }
        }

        public Extensions SupportedCpuExtensions { get; set; }

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
                    return String.Empty;
                }
            }
        }

        public string AppSettingsPath
        {
            get
            {
                try
                {
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    appDataPath = Path.Combine(appDataPath, GetCompanyName(), GetProductName());
                    return appDataPath;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return String.Empty;
                }
            }
        }

        public static string GetAppSettingsPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            appDataPath = Path.Combine(appDataPath, GetCompanyName(), GetProductName());
            return appDataPath;
        }

        public string CommonAppSettingsPath
        {
            get
            {
                try
                {
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    appDataPath = Path.Combine(appDataPath, GetCompanyName(), GetProductName());
                    return appDataPath;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return String.Empty;
                }
            }
        }

        public string TempPath
        {
            get
            {
                try
                {
                    string tempPath = Path.GetTempPath();
                    tempPath = Path.Combine(tempPath, GetProductName() + "-" + Guid.NewGuid());
                    return tempPath;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return String.Empty;
                }
            }
        }

        public CultureInfo CInfo
        {
            get { return CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"); }
        }

        public Version UpdaterVersion
        {
            get
            {
                Version result = new Version(0, 0, 0, 0);
                string tVersion = GetSetting<string>(SettingConstants.UpdaterVersion);
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
                string tVersion = String.Format("{0:0}.{1:0}.{2:0}.{3:0}", value.Major,
                                                                           value.Minor,
                                                                           value.Build,
                                                                           value.Revision);
                SetSetting(SettingConstants.UpdaterVersion, tVersion);
            }
        }

        public string UpdaterPath
        {
            get { return Path.Combine(CommonAppSettingsPath, "Updater"); }
        }

        public string AvsPluginsPath
        {
            get { return Path.Combine(AppPath, "AvsPlugins"); }
        }

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
        /// <returns></returns>
        public static Version GetAppVersion()
        {
            return GetAssembly().GetName().Version;
        }

        private static Assembly GetAssembly()
        {
            return Assembly.GetEntryAssembly();
        }

        public static string GetAppVersionStr()
        {
            Version version = GetAppVersion();
            return string.Format("{0:0}.{1:0}.{2:0}.{3:0}",
                                  version.Major, 
                                  version.Minor, 
                                  version.Build, 
                                  version.Revision);
        }

        public DateTime GetAppBuildDate()
        {
            DateTime result = new DateTime(2011, 01, 13);
            Version appVersion = GetAppVersion();
            result = result.AddDays(appVersion.Build);
            result = result.Add(TimeSpan.ParseExact(appVersion.Revision.ToString("0000"), "hhmm", CInfo));

            return result;
        }

        public static string GetCompanyName()
        {
            string companyName = String.Empty;

            Assembly myAssembly = GetAssembly();
            object[] attributes = myAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);

            if (attributes.Length > 0)
            {
                AssemblyCompanyAttribute aca = attributes[0] as AssemblyCompanyAttribute;

                if (aca != null)
                    companyName = aca.Company;
            }

            return companyName;
        }

        public static string GetProductName()
        {
            string productName = String.Empty;

            Assembly myAssembly = GetAssembly();
            object[] attributes = myAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);

            if (attributes.Length > 0)
            {
                AssemblyProductAttribute apa = attributes[0] as AssemblyProductAttribute;

                if (apa != null)
                    productName = apa.Product;
            }
            return productName;
        }
    }
}
