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
using log4net;

namespace VideoConvert.Core
{
    public class AppSettings
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(AppSettings));

        public static string DecodeNamedPipeName
        {
            get { return string.Format("{0}_decodePipe", GetProductName()); }
        }

        public static string DecodeNamedPipeFullName
        {
            get { return string.Format(@"\\.\pipe\{0}", DecodeNamedPipeName); }
        }

        public static string EncodeNamedPipeName
        {
            get { return string.Format("{0}_encodePipe", GetProductName()); }
        }

        public static string EncodeNamedPipeFullName
        {
            get { return string.Format(@"\\.\pipe\{0}", EncodeNamedPipeName); }
        }

        public static bool UseAviSynthMT
        {
            get { return Properties.Settings.Default.UseAviSynthMT; }
            set { Properties.Settings.Default.UseAviSynthMT = value; }
        }

        public static bool UseHQDeinterlace
        {
            get { return Properties.Settings.Default.UseHQDeinterlace; }
            set { Properties.Settings.Default.UseHQDeinterlace = value; }
        }

        public static bool EnableSSIF
        {
            get { return Properties.Settings.Default.EnableSSIF; }
            set { Properties.Settings.Default.EnableSSIF = value; }
        }

        public static bool FilterLoopingPlaylists
        {
            get { return Properties.Settings.Default.FilterLoopingPlaylists; }
            set { Properties.Settings.Default.FilterLoopingPlaylists = value; }
        }

        public static bool FilterShortPlaylists
        {
            get { return Properties.Settings.Default.FilterShortPlaylists; }
        }

        public static int FilterShortPlaylistsValue
        {
            get { return Properties.Settings.Default.FilterShortPlaylistsValue; }
        }

        public static bool KeepStreamOrder
        {
            get { return Properties.Settings.Default.KeepStreamOrder; }
        }

        public static string ToolsPath
        {
            get
            {
                string tPath = Properties.Settings.Default.ToolsPath;
                if (string.IsNullOrEmpty(tPath))
                    tPath = Path.Combine(CommonAppSettingsPath, "codecs");
                return tPath;
            }

            set { Properties.Settings.Default.ToolsPath = value; }
        }

        public static string JavaInstallPath
        {
            get { return Properties.Settings.Default.JavaInstallPath; }
            set { Properties.Settings.Default.JavaInstallPath = value; }
        }

        public static bool JavaInstalled
        {
            get { return !string.IsNullOrEmpty(JavaInstallPath); }
        }

        public static string OutputLocation
        {
            get { return Properties.Settings.Default.OutputLocation; }
            set { Properties.Settings.Default.OutputLocation = value; }
        }

        public static string DemuxLocation
        {
            get { return Properties.Settings.Default.DemuxLocation; }
            set { Properties.Settings.Default.DemuxLocation = value; }
        }

        public static string Lastx264Ver
        {
            get { return Properties.Settings.Default.Lastx264Ver; }
            set { Properties.Settings.Default.Lastx264Ver = value; }
        }

        public static bool X264Installed
        {
            get { return !string.IsNullOrEmpty(Lastx264Ver); }
        }

        public static string Lastx26464Ver
        {
            get { return Properties.Settings.Default.Lastx264_64Ver; }
            set { Properties.Settings.Default.Lastx264_64Ver = value; }
        }

        public static bool X26464Installed
        {
            get { return !string.IsNullOrEmpty(Lastx26464Ver); }
        }

        public static string LastffmpegVer
        {
            get { return Properties.Settings.Default.LastffmpegVer; }
            set { Properties.Settings.Default.LastffmpegVer = value; }
        }

        public static bool FfmpegInstalled
        {
            get { return !string.IsNullOrEmpty(LastffmpegVer); }
        }

        public static string Lastffmpeg64Ver
        {
            get { return Properties.Settings.Default.Lastffmpeg_64Ver; }
            set { Properties.Settings.Default.Lastffmpeg_64Ver = value; }
        }

        public static bool Ffmpeg64Installed
        {
            get { return !string.IsNullOrEmpty(Lastffmpeg64Ver); }
        }

        public static string Lasteac3ToVer
        {
            get { return Properties.Settings.Default.Lasteac3toVer; }
            set { Properties.Settings.Default.Lasteac3toVer = value; }
        }

        public static bool Eac3ToInstalled
        {
            get { return !string.IsNullOrEmpty(Lasteac3ToVer); }
        }

        public static string LastHcEncVer
        {
            get { return Properties.Settings.Default.LastHcEncVer; }
            set { Properties.Settings.Default.LastHcEncVer = value; }
        }

        public static bool HcEncInstalled
        {
            get { return !string.IsNullOrEmpty(LastHcEncVer); }
        }

        public static string LastlsdvdVer
        {
            get { return Properties.Settings.Default.LastlsdvdVer; }
            set { Properties.Settings.Default.LastlsdvdVer = value; }
        }

        public static bool LsDvdInstalled
        {
            get { return !string.IsNullOrEmpty(LastlsdvdVer); }
        }

        public static string LastMKVMergeVer
        {
            get { return Properties.Settings.Default.LastMKVMergeVer; }
            set { Properties.Settings.Default.LastMKVMergeVer = value; }
        }

        public static bool MKVMergeInstalled
        {
            get { return !string.IsNullOrEmpty(LastMKVMergeVer); }
        }

        public static string LastMplayerVer
        {
            get { return Properties.Settings.Default.LastMplayerVer; }
            set { Properties.Settings.Default.LastMplayerVer = value; }
        }

        public static bool MplayerInstalled
        {
            get { return !string.IsNullOrEmpty(LastMplayerVer); }
        }

        public static string LastTSMuxerVer
        {
            get { return Properties.Settings.Default.LastTSMuxerVer; }
            set { Properties.Settings.Default.LastTSMuxerVer = value; }
        }

        public static bool TsMuxerInstalled
        {
            get { return !string.IsNullOrEmpty(LastTSMuxerVer); }
        }

        public static string LastAviSynthVer
        {
            get { return Properties.Settings.Default.LastAviSynthVer; }
            set { Properties.Settings.Default.LastAviSynthVer = value; }
        }

        public static bool AviSynthInstalled
        {
            get { return !string.IsNullOrEmpty(LastAviSynthVer); }
        }

        public static string LastAviSynthPluginsVer
        {
            get { return Properties.Settings.Default.LastAviSynthPluginsVer; }
            set { Properties.Settings.Default.LastAviSynthPluginsVer = value; }
        }

        public static string LastBDSup2SubVer
        {
            get { return Properties.Settings.Default.LastBDSup2SubVer; }
            set { Properties.Settings.Default.LastBDSup2SubVer = value; }
        }

        public static bool BDSup2SubInstalled
        {
            get { return !string.IsNullOrEmpty(LastBDSup2SubVer); }
        }

        public static string LastMp4BoxVer
        {
            get { return Properties.Settings.Default.LastMp4BoxVer; }
            set { Properties.Settings.Default.LastMp4BoxVer = value; }
        }

        public static bool MP4BoxInstalled
        {
            get { return !string.IsNullOrEmpty(LastMp4BoxVer); }
        }

        public static string LastMJPEGToolsVer
        {
            get { return Properties.Settings.Default.LastMJPEGtoolsVer; }
            set { Properties.Settings.Default.LastMJPEGtoolsVer = value; }
        }

        public static bool MjpegToolsInstalled
        {
            get { return !string.IsNullOrEmpty(LastMJPEGToolsVer); }
        }

        public static string LastDVDAuthorVer
        {
            get { return Properties.Settings.Default.LastDVDAuthorVer; }
            set { Properties.Settings.Default.LastDVDAuthorVer = value; }
        }

        public static bool DVDAuthorInstalled
        {
            get { return !string.IsNullOrEmpty(LastDVDAuthorVer); }
        }

        public static string LastOggEncVer
        {
            get { return Properties.Settings.Default.LastOggEncVer; }
            set { Properties.Settings.Default.LastOggEncVer = value; }
        }

        public static bool OggEncInstalled
        {
            get { return !string.IsNullOrEmpty(LastOggEncVer); }
        }

        public static string LastNeroAacEncVer
        {
            get { return Properties.Settings.Default.LastNeroAacEncVer; }
            set { Properties.Settings.Default.LastNeroAacEncVer = value; }
        }

        public static bool NeroAacEncInstalled
        {
            get { return !string.IsNullOrEmpty(LastNeroAacEncVer); }
        }

        public static string LastLameVer
        {
            get { return Properties.Settings.Default.LastLameVer; }
            set { Properties.Settings.Default.LastLameVer = value; }
        }

        public static bool LameInstalled
        {
            get { return !string.IsNullOrEmpty(LastLameVer); }
        }

        public static string LastVpxEncVer
        {
            get { return Properties.Settings.Default.LastVpxEncVer; }
            set { Properties.Settings.Default.LastVpxEncVer = value; }
        }

        public static bool VpxEncInstalled
        {
            get { return !string.IsNullOrEmpty(LastVpxEncVer); }
        }

        public static bool FirstStart
        {
            get { return Properties.Settings.Default.FirstStart; }
            set { Properties.Settings.Default.FirstStart = value; }
        }

        public static bool TSMuxeRUseAsyncIO
        {
            get { return Properties.Settings.Default.tsMuxeRUseAsyncIO; }
        }

        public static bool TSMuxeRBlurayAudioPES
        {
            get { return Properties.Settings.Default.tsMuxeRBlurayAudioPES; }
        }

        public static int TSMuxerSubtitleAdditionalBorder
        {
            get { return Properties.Settings.Default.tsMuxerSubtitleAdditionalBorder; }
        }

        public static int TSMuxeRBottomOffset
        {
            get { return Properties.Settings.Default.tsMuxeRBottomOffset; }
        }

        public static System.Windows.Media.FontFamily TSMuxeRSubtitleFont
        {
            get { return Properties.Settings.Default.tsMuxeRSubtitleFont; }
            set { Properties.Settings.Default.tsMuxeRSubtitleFont = value; }
        }

        public static System.Windows.Media.Color TSMuxeRSubtitleColor
        {
            get { return Properties.Settings.Default.tsMuxeRSubtitleColor; }
            set { Properties.Settings.Default.tsMuxeRSubtitleColor = value; }
        }

        public static int TSMuxeRSubtitleFontSize
        {
            get { return Properties.Settings.Default.tsMuxeRSubtitleFontSize; }
        }

        public static bool TSMuxeRVideoTimingInfo
        {
            get { return Properties.Settings.Default.tsMuxeRVideoTimingInfo; }
        }

        public static bool TSMuxeRAddVideoPPS
        {
            get { return Properties.Settings.Default.tsMuxeRAddVideoPPS; }
        }

        public static bool DeleteCompletedJobs
        {
            get { return Properties.Settings.Default.DeleteCompletedJobs;}
        }

        public static int ProcessPriority
        {
            get { return Properties.Settings.Default.ProcessPriority; }
            set { Properties.Settings.Default.ProcessPriority = value; }
        }

        public static bool DeleteTemporaryFiles
        {
            get { return Properties.Settings.Default.DeleteTemporaryFiles; }
            set { Properties.Settings.Default.DeleteTemporaryFiles = value; }
        }

        public static bool UseDebug
        {
            get { return Properties.Settings.Default.UseDebug; }
            set { Properties.Settings.Default.UseDebug = value; }
        }

        public static bool Use64BitEncoders
        {
            get { return Properties.Settings.Default.Use64bitEncoders; }
            set { Properties.Settings.Default.Use64bitEncoders = value; }
        }

        public static bool UseHardwareRendering
        {
            get { return Properties.Settings.Default.UseHardwareRendering; }
            set { Properties.Settings.Default.UseHardwareRendering = value; }
        }

        public static string UseLanguage
        {
            get { return Properties.Settings.Default.UseLanguage; }
            set { Properties.Settings.Default.UseLanguage = value; }
        }

        public static string LastSelectedProfile
        {
            get { return Properties.Settings.Default.LastSelectedProfile; }
            set { Properties.Settings.Default.LastSelectedProfile = value; }
        }

        public static string LastProfilesVer
        {
            get { return Properties.Settings.Default.LastProfilesVer; }
            set { Properties.Settings.Default.LastProfilesVer = value; }
        }

        public static string GetCompanyName()
        {
            string companyName = string.Empty;

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
            string productName = string.Empty;

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
            get { return Properties.Settings.Default.UpdateVersions; }
            set { Properties.Settings.Default.UpdateVersions = value; }
        }

        public static int UpdateFrequency
        {
            get { return Properties.Settings.Default.UpdateFrequency; }
            set { Properties.Settings.Default.UpdateFrequency = value; }
        }

        public static DateTime LastUpdateRun
        {
            get { return Properties.Settings.Default.LastUpdateRun; }
            set { Properties.Settings.Default.LastUpdateRun = value; }
        }

        public static bool ShowChangeLog
        {
            get { return Properties.Settings.Default.ShowChangeLog; }
        }

        public static bool CreateXbmcInfoFile
        {
            get { return Properties.Settings.Default.CreateXbmcInfoFile; }
        }

        public static string MovieDBLastLanguage
        {
            get { return Properties.Settings.Default.MovieDBLastLanguage; }
            set { Properties.Settings.Default.MovieDBLastLanguage = value; }
        }

        public static string MovieDBLastRatingCountry
        {
            get { return Properties.Settings.Default.MovieDBLastRatingCountry; }
            set { Properties.Settings.Default.MovieDBLastRatingCountry = value; }
        }

        public static string MovieDBLastFallbackLanguage
        {
            get { return Properties.Settings.Default.MovieDBLastFallbackLanguage; }
            set { Properties.Settings.Default.MovieDBLastFallbackLanguage = value; }
        }

        public static string MovieDBLastFallbackRatingCountry
        {
            get { return Properties.Settings.Default.MovieDBLastFallbackRatingCountry; }
            set { Properties.Settings.Default.MovieDBLastFallbackRatingCountry = value; }
        }

        public static string MovieDBPreferredCertPrefix
        {
            get { return Properties.Settings.Default.MovieDBPreferredCertPrefix; }
            set { Properties.Settings.Default.MovieDBPreferredCertPrefix = value; }
        }

        public static string MovieDBFallbackCertPrefix
        {
            get { return Properties.Settings.Default.MovieDBFallbackCertPrefix; }
            set { Properties.Settings.Default.MovieDBFallbackCertPrefix = value; }
        }

        public static int MovieDBRatingSource
        {
            get { return Properties.Settings.Default.MovieDBRatingSource; }
            set { Properties.Settings.Default.MovieDBRatingSource = value; }
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
                    return string.Empty;
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
                    return string.Empty;
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
                    return string.Empty;
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
                    return string.Empty;
                }
            }
        }

        public static CultureInfo CInfo
        {
            get { return App.Cinfo; }
        }

        public static object GetPreviousVersion(string propertyName)
        {
            return Properties.Settings.Default.GetPreviousVersion(propertyName);
        }

        public static void Upgrade()
        {
            Properties.Settings.Default.Upgrade();
            SaveSettings();
        }

        public static void SaveSettings()
        {
            try
            {
                Properties.Settings.Default.Save();
                BDInfo.BDInfoSettings.EnableSSIF = EnableSSIF;
                BDInfo.BDInfoSettings.FilterLoopingPlaylists = FilterLoopingPlaylists;
                BDInfo.BDInfoSettings.FilterShortPlaylists = FilterShortPlaylists;
                BDInfo.BDInfoSettings.FilterShortPlaylistsValue = FilterShortPlaylistsValue;
                BDInfo.BDInfoSettings.KeepStreamOrder = KeepStreamOrder;
                BDInfo.BDInfoSettings.SaveSettings();
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
                    result = Version.Parse(Properties.Settings.Default.UpdaterVersion);
                }
                catch (Exception ex)
                { 
                    Log.Error(ex);
                }

                return result;
            }
            set
            {
                Properties.Settings.Default.UpdaterVersion = string.Format("{0:0}.{1:0}.{2:0}.{3:0}", value.Major,
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
            Properties.Settings.Default.Reset();
            FirstStart = false;
            SaveSettings();
        }

        internal static void Reload()
        {
            Properties.Settings.Default.Reload();
        }
    }
}
