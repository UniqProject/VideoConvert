//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using BDInfo;
using VideoConvert.Core.Helpers;
using VideoConvert.Properties;
using log4net;

namespace VideoConvert.Core
{
    public class AppSettings
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(AppSettings));

        public const string MovieDBApiKey = "3c0a6fc7bb8fea5432a4e21ec32be907";
        public const string TheTVDBApiKey = "1DBEA8A1430711B7";

        public static string DecodeNamedPipeName
        {
            get { return String.Format("{0}_decodePipe", GetProductName()); }
        }

        public static string DecodeNamedPipeFullName
        {
            get { return String.Format(@"\\.\pipe\{0}", DecodeNamedPipeName); }
        }

        public static string EncodeNamedPipeName
        {
            get { return String.Format("{0}_encodePipe", GetProductName()); }
        }

        public static string EncodeNamedPipeFullName
        {
            get { return String.Format(@"\\.\pipe\{0}", EncodeNamedPipeName); }
        }

        public static bool UseAviSynthMT
        {
            get { return Settings.Default.UseAviSynthMT; }
            set { Settings.Default.UseAviSynthMT = value; }
        }

        public static bool UseHQDeinterlace
        {
            get { return Settings.Default.UseHQDeinterlace; }
            set { Settings.Default.UseHQDeinterlace = value; }
        }

        public static bool EnableSSIF
        {
            get { return Settings.Default.EnableSSIF; }
            set { Settings.Default.EnableSSIF = value; }
        }

        public static bool FilterLoopingPlaylists
        {
            get { return Settings.Default.FilterLoopingPlaylists; }
            set { Settings.Default.FilterLoopingPlaylists = value; }
        }

        public static bool FilterShortPlaylists
        {
            get { return Settings.Default.FilterShortPlaylists; }
        }

        public static int FilterShortPlaylistsValue
        {
            get { return Settings.Default.FilterShortPlaylistsValue; }
        }

        public static bool KeepStreamOrder
        {
            get { return Settings.Default.KeepStreamOrder; }
        }

        public static string ToolsPath
        {
            get
            {
                string tPath = Settings.Default.ToolsPath;
                if (String.IsNullOrEmpty(tPath))
                    tPath = Path.Combine(CommonAppSettingsPath, "codecs");
                return tPath;
            }

            set { Settings.Default.ToolsPath = value; }
        }

        public static string JavaInstallPath
        {
            get { return Settings.Default.JavaInstallPath; }
            set { Settings.Default.JavaInstallPath = value; }
        }

        public static bool JavaInstalled
        {
            get { return !String.IsNullOrEmpty(JavaInstallPath); }
        }

        public static string OutputLocation
        {
            get { return Settings.Default.OutputLocation; }
            set { Settings.Default.OutputLocation = value; }
        }

        public static string DemuxLocation
        {
            get { return Settings.Default.DemuxLocation; }
            set { Settings.Default.DemuxLocation = value; }
        }

        public static string Lastx264Ver
        {
            get { return Settings.Default.Lastx264Ver; }
            set { Settings.Default.Lastx264Ver = value; }
        }

        public static bool X264Installed
        {
            get { return !String.IsNullOrEmpty(Lastx264Ver); }
        }

        public static string Lastx26464Ver
        {
            get { return Settings.Default.Lastx264_64Ver; }
            set { Settings.Default.Lastx264_64Ver = value; }
        }

        public static bool X26464Installed
        {
            get { return !String.IsNullOrEmpty(Lastx26464Ver); }
        }

        public static string LastffmpegVer
        {
            get { return Settings.Default.LastffmpegVer; }
            set { Settings.Default.LastffmpegVer = value; }
        }

        public static bool FfmpegInstalled
        {
            get { return !String.IsNullOrEmpty(LastffmpegVer); }
        }

        public static string Lastffmpeg64Ver
        {
            get { return Settings.Default.Lastffmpeg_64Ver; }
            set { Settings.Default.Lastffmpeg_64Ver = value; }
        }

        public static bool Ffmpeg64Installed
        {
            get { return !String.IsNullOrEmpty(Lastffmpeg64Ver); }
        }

        public static string Lasteac3ToVer
        {
            get { return Settings.Default.Lasteac3toVer; }
            set { Settings.Default.Lasteac3toVer = value; }
        }

        public static bool Eac3ToInstalled
        {
            get { return !String.IsNullOrEmpty(Lasteac3ToVer); }
        }

        public static string LastHcEncVer
        {
            get { return Settings.Default.LastHcEncVer; }
            set { Settings.Default.LastHcEncVer = value; }
        }

        public static bool HcEncInstalled
        {
            get { return !String.IsNullOrEmpty(LastHcEncVer); }
        }

        public static string LastlsdvdVer
        {
            get { return Settings.Default.LastlsdvdVer; }
            set { Settings.Default.LastlsdvdVer = value; }
        }

        public static bool LsDvdInstalled
        {
            get { return !String.IsNullOrEmpty(LastlsdvdVer); }
        }

        public static string LastMKVMergeVer
        {
            get { return Settings.Default.LastMKVMergeVer; }
            set { Settings.Default.LastMKVMergeVer = value; }
        }

        public static bool MKVMergeInstalled
        {
            get { return !String.IsNullOrEmpty(LastMKVMergeVer); }
        }

        public static string LastMplayerVer
        {
            get { return Settings.Default.LastMplayerVer; }
            set { Settings.Default.LastMplayerVer = value; }
        }

        public static bool MplayerInstalled
        {
            get { return !String.IsNullOrEmpty(LastMplayerVer); }
        }

        public static string LastTSMuxerVer
        {
            get { return Settings.Default.LastTSMuxerVer; }
            set { Settings.Default.LastTSMuxerVer = value; }
        }

        public static bool TsMuxerInstalled
        {
            get { return !String.IsNullOrEmpty(LastTSMuxerVer); }
        }

        public static string LastAviSynthVer
        {
            get { return Settings.Default.LastAviSynthVer; }
            set { Settings.Default.LastAviSynthVer = value; }
        }

        public static bool AviSynthInstalled
        {
            get { return !String.IsNullOrEmpty(LastAviSynthVer); }
        }

        public static string LastAviSynthPluginsVer
        {
            get { return Settings.Default.LastAviSynthPluginsVer; }
            set { Settings.Default.LastAviSynthPluginsVer = value; }
        }

        public static string LastBDSup2SubVer
        {
            get { return Settings.Default.LastBDSup2SubVer; }
            set { Settings.Default.LastBDSup2SubVer = value; }
        }

        public static bool BDSup2SubInstalled
        {
            get { return !String.IsNullOrEmpty(LastBDSup2SubVer); }
        }

        public static string LastMp4BoxVer
        {
            get { return Settings.Default.LastMp4BoxVer; }
            set { Settings.Default.LastMp4BoxVer = value; }
        }

        public static bool MP4BoxInstalled
        {
            get { return !String.IsNullOrEmpty(LastMp4BoxVer); }
        }

        public static string LastMJPEGToolsVer
        {
            get { return Settings.Default.LastMJPEGtoolsVer; }
            set { Settings.Default.LastMJPEGtoolsVer = value; }
        }

        public static bool MjpegToolsInstalled
        {
            get { return !String.IsNullOrEmpty(LastMJPEGToolsVer); }
        }

        public static string LastDVDAuthorVer
        {
            get { return Settings.Default.LastDVDAuthorVer; }
            set { Settings.Default.LastDVDAuthorVer = value; }
        }

        public static bool DVDAuthorInstalled
        {
            get { return !String.IsNullOrEmpty(LastDVDAuthorVer); }
        }

        public static string LastOggEncVer
        {
            get { return Settings.Default.LastOggEncVer; }
            set { Settings.Default.LastOggEncVer = value; }
        }

        public static bool OggEncInstalled
        {
            get { return !String.IsNullOrEmpty(LastOggEncVer); }
        }

        public static string LastNeroAacEncVer
        {
            get { return Settings.Default.LastNeroAacEncVer; }
            set { Settings.Default.LastNeroAacEncVer = value; }
        }

        public static bool NeroAacEncInstalled
        {
            get { return !String.IsNullOrEmpty(LastNeroAacEncVer); }
        }

        public static string LastLameVer
        {
            get { return Settings.Default.LastLameVer; }
            set { Settings.Default.LastLameVer = value; }
        }

        public static bool LameInstalled
        {
            get { return !String.IsNullOrEmpty(LastLameVer); }
        }

        public static string LastVpxEncVer
        {
            get { return Settings.Default.LastVpxEncVer; }
            set { Settings.Default.LastVpxEncVer = value; }
        }

        public static bool VpxEncInstalled
        {
            get { return !String.IsNullOrEmpty(LastVpxEncVer); }
        }

        public static bool FirstStart
        {
            get { return Settings.Default.FirstStart; }
            set { Settings.Default.FirstStart = value; }
        }

        public static bool TSMuxeRUseAsyncIO
        {
            get { return Settings.Default.tsMuxeRUseAsyncIO; }
        }

        public static bool TSMuxeRBlurayAudioPES
        {
            get { return Settings.Default.tsMuxeRBlurayAudioPES; }
        }

        public static int TSMuxerSubtitleAdditionalBorder
        {
            get { return Settings.Default.tsMuxerSubtitleAdditionalBorder; }
        }

        public static int TSMuxeRBottomOffset
        {
            get { return Settings.Default.tsMuxeRBottomOffset; }
        }

        public static FontFamily TSMuxeRSubtitleFont
        {
            get { return Settings.Default.tsMuxeRSubtitleFont; }
            set { Settings.Default.tsMuxeRSubtitleFont = value; }
        }

        public static Color TSMuxeRSubtitleColor
        {
            get { return Settings.Default.tsMuxeRSubtitleColor; }
            set { Settings.Default.tsMuxeRSubtitleColor = value; }
        }

        public static int TSMuxeRSubtitleFontSize
        {
            get { return Settings.Default.tsMuxeRSubtitleFontSize; }
        }

        public static bool TSMuxeRVideoTimingInfo
        {
            get { return Settings.Default.tsMuxeRVideoTimingInfo; }
        }

        public static bool TSMuxeRAddVideoPPS
        {
            get { return Settings.Default.tsMuxeRAddVideoPPS; }
        }

        public static bool DeleteCompletedJobs
        {
            get { return Settings.Default.DeleteCompletedJobs;}
        }

        public static int ProcessPriority
        {
            get { return Settings.Default.ProcessPriority; }
            set { Settings.Default.ProcessPriority = value; }
        }

        public static bool DeleteTemporaryFiles
        {
            get { return Settings.Default.DeleteTemporaryFiles; }
            set { Settings.Default.DeleteTemporaryFiles = value; }
        }

        public static bool UseDebug
        {
            get { return Settings.Default.UseDebug; }
            set { Settings.Default.UseDebug = value; }
        }

        public static bool Use64BitEncoders
        {
            get { return Settings.Default.Use64bitEncoders; }
            set { Settings.Default.Use64bitEncoders = value; }
        }

        public static bool UseHardwareRendering
        {
            get { return Settings.Default.UseHardwareRendering; }
            set { Settings.Default.UseHardwareRendering = value; }
        }

        public static string UseLanguage
        {
            get { return Settings.Default.UseLanguage; }
            set { Settings.Default.UseLanguage = value; }
        }

        public static string LastSelectedProfile
        {
            get { return Settings.Default.LastSelectedProfile; }
            set { Settings.Default.LastSelectedProfile = value; }
        }

        public static string LastProfilesVer
        {
            get { return Settings.Default.LastProfilesVer; }
            set { Settings.Default.LastProfilesVer = value; }
        }

        public static string GetCompanyName()
        {
            string companyName = String.Empty;

            Assembly myAssembly = Assembly.GetExecutingAssembly();
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

            Assembly myAssembly = Assembly.GetExecutingAssembly();
            object[] attributes = myAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);

            if (attributes.Length > 0)
            {
                AssemblyProductAttribute apa = attributes[0] as AssemblyProductAttribute;

                if (apa != null)
                    productName = apa.Product;
            }
            return productName;
        }

        public static bool UpdateVersions
        {
            get { return Settings.Default.UpdateVersions; }
            set { Settings.Default.UpdateVersions = value; }
        }

        public static int UpdateFrequency
        {
            get { return Settings.Default.UpdateFrequency; }
            set { Settings.Default.UpdateFrequency = value; }
        }

        public static DateTime LastUpdateRun
        {
            get { return Settings.Default.LastUpdateRun; }
            set { Settings.Default.LastUpdateRun = value; }
        }

        public static bool ShowChangeLog
        {
            get { return Settings.Default.ShowChangeLog; }
        }

        public static bool CreateXbmcInfoFile
        {
            get { return Settings.Default.CreateXbmcInfoFile; }
        }

        public static string MovieDBLastLanguage
        {
            get { return Settings.Default.MovieDBLastLanguage; }
            set { Settings.Default.MovieDBLastLanguage = value; }
        }

        public static string MovieDBLastRatingCountry
        {
            get { return Settings.Default.MovieDBLastRatingCountry; }
            set { Settings.Default.MovieDBLastRatingCountry = value; }
        }

        public static string MovieDBLastFallbackLanguage
        {
            get { return Settings.Default.MovieDBLastFallbackLanguage; }
            set { Settings.Default.MovieDBLastFallbackLanguage = value; }
        }

        public static string MovieDBLastFallbackRatingCountry
        {
            get { return Settings.Default.MovieDBLastFallbackRatingCountry; }
            set { Settings.Default.MovieDBLastFallbackRatingCountry = value; }
        }

        public static string MovieDBPreferredCertPrefix
        {
            get { return Settings.Default.MovieDBPreferredCertPrefix; }
            set { Settings.Default.MovieDBPreferredCertPrefix = value; }
        }

        public static string MovieDBFallbackCertPrefix
        {
            get { return Settings.Default.MovieDBFallbackCertPrefix; }
            set { Settings.Default.MovieDBFallbackCertPrefix = value; }
        }

        public static int MovieDBRatingSource
        {
            get { return Settings.Default.MovieDBRatingSource; }
            set { Settings.Default.MovieDBRatingSource = value; }
        }

        public static string TvDBCachePath
        {
            get { return Settings.Default.TvDBCachePath; }
            set { Settings.Default.TvDBCachePath = value; }
        }

        public static string TvDBParseString
        {
            get { return Settings.Default.TvDBParseString; }
            set { Settings.Default.TvDBParseString = value; }
        }

        public static void InitTvDBCache()
        {
            if (!String.IsNullOrEmpty(TvDBCachePath)) return;

            TvDBCachePath = Path.Combine(CommonAppSettingsPath, "TvDBCache");
            if (!Directory.Exists(TvDBCachePath))
                Directory.CreateDirectory(TvDBCachePath, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));
        }

        public static string TvDBPreferredLanguage
        {
            get { return Settings.Default.TvDBPreferredLanguage; }
            set { Settings.Default.TvDBPreferredLanguage = value; }
        }

        public static string TvDBFallbackLanguage
        {
            get { return Settings.Default.TvDBFallbackLanguage; }
            set { Settings.Default.TvDBFallbackLanguage = value; }
        }

        public static int LastSelectedSource
        {
            get { return Settings.Default.LastSelectedSource; }
            set { Settings.Default.LastSelectedSource = value; }
        }

        public static bool UseFfmpegScaling
        {
            get { return Settings.Default.UseFfmpegScaling; }
            set { Settings.Default.UseFfmpegScaling = value; }
        }

        public static string AppPath
        {
            get
            {
                try
                {
                    return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return String.Empty;
                }
            }
        }

        public static string AppSettingsPath
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

        public static string CommonAppSettingsPath
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

        public static string TempPath
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

        public static CultureInfo CInfo
        {
            get { return App.Cinfo; }
        }

        public static object GetPreviousVersion(string propertyName)
        {
            return Settings.Default.GetPreviousVersion(propertyName);
        }

        public static void Upgrade()
        {
            Settings.Default.Upgrade();
            SaveSettings();
        }

        public static void SaveSettings()
        {
            try
            {
                Settings.Default.Save();
                BDInfoSettings.EnableSSIF = EnableSSIF;
                BDInfoSettings.FilterLoopingPlaylists = FilterLoopingPlaylists;
                BDInfoSettings.FilterShortPlaylists = FilterShortPlaylists;
                BDInfoSettings.FilterShortPlaylistsValue = FilterShortPlaylistsValue;
                BDInfoSettings.KeepStreamOrder = KeepStreamOrder;
                BDInfoSettings.SaveSettings();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public static ProcessPriorityClass GetProcessPriority()
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

        public static ThreadPriorityLevel GetThreadPriority()
        {
            switch (ProcessPriority)
            {
                case 0:
                    return ThreadPriorityLevel.TimeCritical;
                case 1: 
                    return ThreadPriorityLevel.Highest;
                case 2:
                    return ThreadPriorityLevel.AboveNormal;
                default:
                    return ThreadPriorityLevel.Normal;
                case 4:
                    return ThreadPriorityLevel.BelowNormal;
                case 5:
                    return ThreadPriorityLevel.Idle;
            }
        }

        /// <summary>
        /// returns the Version object containing AssemblyVersion of this application
        /// </summary>
        /// <returns></returns>
        public static Version GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static DateTime GetAppBuildDate()
        {
            DateTime result = new DateTime(2011, 01, 13);
            Version appVersion = GetAppVersion();
            result = result.AddDays(appVersion.Build);
            result = result.Add(TimeSpan.ParseExact(appVersion.Revision.ToString("0000"), "hhmm", CInfo));

            return result;
        }

        public static Version UpdaterVersion
        {
            get
            {
                Version result = new Version(0,0,0,0);
                try
                {
                    result = Version.Parse(Settings.Default.UpdaterVersion);
                }
                catch (Exception ex)
                { 
                    Log.Error(ex);
                }

                return result;
            }
            set
            {
                Settings.Default.UpdaterVersion = String.Format("{0:0}.{1:0}.{2:0}.{3:0}", value.Major,
                                                                           value.Minor,
                                                                           value.Build, value.Revision);
            }
        }

        public static string UpdaterPath
        {
            get { return Path.Combine(CommonAppSettingsPath, "Updater"); }
        }

        public static string AvsPluginsPath
        {
            get { return Path.Combine(AppPath, "AvsPlugins"); }
        }


        internal static void Reset()
        {
            Settings.Default.Reset();
            FirstStart = false;
            SaveSettings();
        }

        internal static void Reload()
        {
            Settings.Default.Reload();
        }
    }
}
