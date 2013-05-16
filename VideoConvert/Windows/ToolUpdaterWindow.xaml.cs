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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using UpdateCore;
using VideoConvert.Core;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;

namespace VideoConvert.Windows
{
    /// <summary>
    /// Interaktionslogik für ToolUpdaterWindow.xaml
    /// </summary>
    public partial class ToolUpdaterWindow
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ToolUpdaterWindow));
        public ObservableCollection<ToolVersions> ToolCollection { get; private set; }

        public string MainAppUpdateFile { get; private set; }

        private readonly List<ToolVersions> _tempToolCollection = new List<ToolVersions>();
        private bool _mainAppUpdate;

        private readonly List<PackageInfo> _packages = new List<PackageInfo>();

        public ToolUpdaterWindow()
        {
            ToolCollection = new ObservableCollection<ToolVersions>();
            MainAppUpdateFile = string.Empty;
            InitializeComponent();
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AppSettings.ToolsPath))
            {
                string msg = Processing.GetResourceString("update_error_encpath");
                Xceed.Wpf.Toolkit.MessageBox.Show(msg, "Error",
                                                  MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                BackgroundWorker checkUpdate = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                checkUpdate.RunWorkerCompleted += CheckUpdateRunWorkerCompleted;
                checkUpdate.DoWork += CheckUpdateDoWork;
                checkUpdate.RunWorkerAsync();
            }
        }

        void CheckUpdateDoWork(object sender, DoWorkEventArgs e)
        {
            const string serverPath = "http://www.jt-soft.de/videoconvert/";
            const string serverPathTools = "http://www.jt-soft.de/videoconvert/tools/";
            string tempPath = AppSettings.TempPath;

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                WebClient downloader = new WebClient { UseDefaultCredentials = true };
                Stream onlineUpdateFile;
                try
                {
                    onlineUpdateFile = downloader.OpenRead(new Uri("http://www.jt-soft.de/videoconvert/updatefile.xml"));
                }
                catch (WebException exception)
                {
                    Log.Error(exception);
                    e.Result = false;
                    return;
                }

                if (onlineUpdateFile == null)
                {
                    e.Result = false;
                    return;
                }

                using (UpdateFileInfo updateFile = Updater.LoadUpdateFileFromStream(onlineUpdateFile))
                {
                    if (updateFile.Core.PackageVersion.CompareTo(AppSettings.GetAppVersion()) > 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "Core",
                                LocalVersion = AppSettings.GetAppVersion().ToString(4),
                                ServerVersion = updateFile.Core.PackageVersionStr,
                                FileName =
                                    Path.Combine(tempPath, updateFile.Core.PackageName),
                                DownloadUri = serverPath + updateFile.Core.PackageName,
                                DownloadType = AppType.MainApp,
                                Destination = AppSettings.AppPath
                            });

                    if (updateFile.Updater.PackageVersion.CompareTo(AppSettings.UpdaterVersion) > 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "Updater (UAC Compatible)",
                                LocalVersion = AppSettings.UpdaterVersion.ToString(4),
                                ServerVersion = updateFile.Updater.PackageVersionStr,
                                FileName =
                                    Path.Combine(tempPath, updateFile.Updater.PackageName),
                                DownloadUri = serverPath + updateFile.Updater.PackageName,
                                DownloadType = AppType.Updater,
                                Destination = AppSettings.UpdaterPath
                            });

                    if (
                        String.CompareOrdinal(updateFile.AviSynthPlugins.PackageVersion,
                                              AppSettings.LastAviSynthPluginsVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "AviSynth Plugins",
                                LocalVersion = AppSettings.LastAviSynthPluginsVer,
                                ServerVersion = updateFile.AviSynthPlugins.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.AviSynthPlugins.PackageName),
                                DownloadUri = serverPath + updateFile.AviSynthPlugins.PackageName,
                                DownloadType = AppType.AviSynthPlugins,
                                Destination = AppSettings.AvsPluginsPath
                            });

                    if (String.CompareOrdinal(updateFile.Profiles.PackageVersion, AppSettings.LastProfilesVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "Profiles",
                                LocalVersion = AppSettings.LastProfilesVer,
                                ServerVersion = updateFile.Profiles.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.Profiles.PackageName),
                                DownloadUri = serverPath + updateFile.Profiles.PackageName,
                                DownloadType = AppType.Profiles,
                                Destination = AppSettings.CommonAppSettingsPath
                            });

                    if (String.CompareOrdinal(updateFile.X264.PackageVersion, AppSettings.Lastx264Ver) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "x264",
                                LocalVersion = AppSettings.Lastx264Ver,
                                ServerVersion = updateFile.X264.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.X264.PackageName),
                                DownloadUri = serverPathTools + updateFile.X264.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.X26464.PackageVersion, AppSettings.Lastx26464Ver) != 0 &&
                        Environment.Is64BitOperatingSystem)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "x264 (64 bit)",
                                LocalVersion = AppSettings.Lastx26464Ver,
                                ServerVersion = updateFile.X26464.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.X26464.PackageName),
                                DownloadUri = serverPathTools + updateFile.X26464.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.FFMPEG.PackageVersion, AppSettings.LastffmpegVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "ffmpeg",
                                LocalVersion = AppSettings.LastffmpegVer,
                                ServerVersion = updateFile.FFMPEG.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.FFMPEG.PackageName),
                                DownloadUri = serverPathTools + updateFile.FFMPEG.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.FFMPEG64.PackageVersion, AppSettings.Lastffmpeg64Ver) != 0 &&
                        Environment.Is64BitOperatingSystem)
                        _tempToolCollection.Add(new ToolVersions
                        {
                            ToolName = "ffmpeg (64 bit)",
                            LocalVersion = AppSettings.Lastffmpeg64Ver,
                            ServerVersion = updateFile.FFMPEG64.PackageVersion,
                            FileName = Path.Combine(tempPath, updateFile.FFMPEG64.PackageName),
                            DownloadUri = serverPathTools + updateFile.FFMPEG64.PackageName,
                            DownloadType = AppType.Encoder,
                            Destination = AppSettings.ToolsPath
                        });

                    if (String.CompareOrdinal(updateFile.Eac3To.PackageVersion, AppSettings.Lasteac3ToVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "eac3to",
                                LocalVersion = AppSettings.Lasteac3ToVer,
                                ServerVersion = updateFile.Eac3To.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.Eac3To.PackageName),
                                DownloadUri = serverPathTools + updateFile.Eac3To.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.LsDvd.PackageVersion, AppSettings.LastlsdvdVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "lsdvd",
                                LocalVersion = AppSettings.LastlsdvdVer,
                                ServerVersion = updateFile.LsDvd.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.LsDvd.PackageName),
                                DownloadUri = serverPathTools + updateFile.LsDvd.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.MKVToolnix.PackageVersion, AppSettings.LastMKVMergeVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "MKVToolnix",
                                LocalVersion = AppSettings.LastMKVMergeVer,
                                ServerVersion = updateFile.MKVToolnix.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.MKVToolnix.PackageName),
                                DownloadUri = serverPathTools + updateFile.MKVToolnix.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.Mplayer.PackageVersion, AppSettings.LastMplayerVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "MPlayer",
                                LocalVersion = AppSettings.LastMplayerVer,
                                ServerVersion = updateFile.Mplayer.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.Mplayer.PackageName),
                                DownloadUri = serverPathTools + updateFile.Mplayer.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.TSMuxeR.PackageVersion, AppSettings.LastTSMuxerVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "TSMuxeR",
                                LocalVersion = AppSettings.LastTSMuxerVer,
                                ServerVersion = updateFile.TSMuxeR.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.TSMuxeR.PackageName),
                                DownloadUri = serverPathTools + updateFile.TSMuxeR.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.MjpegTools.PackageVersion, AppSettings.LastMJPEGToolsVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "mjpegtools",
                                LocalVersion = AppSettings.LastMJPEGToolsVer,
                                ServerVersion = updateFile.MjpegTools.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.MjpegTools.PackageName),
                                DownloadUri = serverPathTools + updateFile.MjpegTools.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.DVDAuthor.PackageVersion, AppSettings.LastDVDAuthorVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "DVDAuthor",
                                LocalVersion = AppSettings.LastDVDAuthorVer,
                                ServerVersion = updateFile.DVDAuthor.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.DVDAuthor.PackageName),
                                DownloadUri = serverPathTools + updateFile.DVDAuthor.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.MP4Box.PackageVersion, AppSettings.LastMp4BoxVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "MP4Box",
                                LocalVersion = AppSettings.LastMp4BoxVer,
                                ServerVersion = updateFile.MP4Box.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.MP4Box.PackageName),
                                DownloadUri = serverPathTools + updateFile.MP4Box.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.HcEnc.PackageVersion, AppSettings.LastHcEncVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "HCEnc",
                                LocalVersion = AppSettings.LastHcEncVer,
                                ServerVersion = updateFile.HcEnc.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.HcEnc.PackageName),
                                DownloadUri = serverPathTools + updateFile.HcEnc.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.OggEnc.PackageVersion, AppSettings.LastOggEncVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "OggEnc2",
                                LocalVersion = AppSettings.LastOggEncVer,
                                ServerVersion = updateFile.OggEnc.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.OggEnc.PackageName),
                                DownloadUri = serverPathTools + updateFile.OggEnc.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.Lame.PackageVersion, AppSettings.LastLameVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "Lame",
                                LocalVersion = AppSettings.LastLameVer,
                                ServerVersion = updateFile.Lame.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.Lame.PackageName),
                                DownloadUri = serverPathTools + updateFile.Lame.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.VpxEnc.PackageVersion, AppSettings.LastVpxEncVer) != 0)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "VP8 Encoder",
                                LocalVersion = AppSettings.LastVpxEncVer,
                                ServerVersion = updateFile.VpxEnc.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.VpxEnc.PackageName),
                                DownloadUri = serverPathTools + updateFile.VpxEnc.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });

                    if (String.CompareOrdinal(updateFile.BDSup2Sub.PackageVersion, AppSettings.LastBDSup2SubVer) != 0 &&
                        AppSettings.JavaInstalled)
                        _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "bdsup2sub",
                                LocalVersion = AppSettings.LastBDSup2SubVer,
                                ServerVersion = updateFile.BDSup2Sub.PackageVersion,
                                FileName = Path.Combine(tempPath, updateFile.BDSup2Sub.PackageName),
                                DownloadUri = serverPathTools + updateFile.BDSup2Sub.PackageName,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });
                }
            }

            e.Result = true;
        }

        void CheckUpdateRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateToolVersions();
        }

        private void UpdateToolVersions()
        {
            ToolCollection.Clear();
            foreach (ToolVersions ver in _tempToolCollection)
                ToolCollection.Add(ver);
        }

        private void UpdateButtonClick(object sender, RoutedEventArgs e)
        {
            CloseButton.IsEnabled = false;
            UpdateButton.IsEnabled = false;
            BackgroundWorker updater = new BackgroundWorker
                                           {
                                               WorkerReportsProgress = true,
                                               WorkerSupportsCancellation = true
                                           };
            updater.RunWorkerCompleted += UpdaterRunWorkerCompleted;
            updater.ProgressChanged += UpdaterProgressChanged;
            updater.DoWork += UpdaterDoWork;
            updater.RunWorkerAsync();
        }

        void UpdaterProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -20)
            {
                ToolCollection.Remove((ToolVersions)e.UserState);
            }
            else
            {
                StatusBar.IsIndeterminate = e.ProgressPercentage < 0;
                StatusBar.Value = e.ProgressPercentage;

                StatusText.Content = e.UserState;
            }
        }

        void UpdaterDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("update_downloading_status");
            string progressFmt = Processing.GetResourceString("update_downloading_progress");
            string unzippingStatus = Processing.GetResourceString("update_unzipping");
            string reloadStatus = Processing.GetResourceString("update_reload_versions");
            string finishedStatus = Processing.GetResourceString("update_finished");
            string importProfiles = Processing.GetResourceString("update_import_profiles");

            string tempPath = AppSettings.TempPath;

            foreach (ToolVersions item in _tempToolCollection)
            {
                string showName = item.ToolName;
                bw.ReportProgress(-1, string.Format(status, showName, item.ServerVersion));

                string outFPath = Path.GetDirectoryName(item.FileName);
                if (!string.IsNullOrEmpty(outFPath) && !Directory.Exists(outFPath))
                    Directory.CreateDirectory(outFPath, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));
                tempPath = outFPath;

                WebClient downloader = new WebClient {UseDefaultCredentials = true};
                downloader.DownloadFileAsync(new Uri(item.DownloadUri), item.FileName);

                ToolVersions lItem = item;
                
                downloader.DownloadProgressChanged += (s, ev) =>
                {
                    string progress = string.Format(progressFmt, showName, lItem.ServerVersion, ev.ProgressPercentage);
                    bw.ReportProgress(ev.ProgressPercentage, progress);
                };

                while (downloader.IsBusy)
                    Thread.Sleep(200);

                switch (lItem.DownloadType)
                {
                    case AppType.Updater:
                        if (!string.IsNullOrEmpty(AppSettings.UpdaterPath) && !Directory.Exists(AppSettings.UpdaterPath))
                            Directory.CreateDirectory(AppSettings.UpdaterPath, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));
                        bw.ReportProgress(-1, unzippingStatus);
                        try
                        {
                            using (ZipFile zFile = new ZipFile(lItem.FileName))
                            {
                                foreach (ZipEntry entry in zFile)
                                {
                                    if (AppSettings.UpdaterPath == null) continue;

                                    string outPath = Path.Combine(AppSettings.UpdaterPath, entry.Name);

                                    if (entry.IsDirectory)
                                        Directory.CreateDirectory(outPath, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));
                                    else if (entry.IsFile)
                                    {
                                        using (Stream zStream = zFile.GetInputStream(entry),
                                                      outFile = new FileStream(outPath, FileMode.Create))
                                        {
                                            zStream.CopyTo(outFile);
                                        }
                                    }
                                }
                            }
                            File.Delete(item.FileName);
                        }
                        catch (Exception ex)
                        {
                            Log.ErrorFormat("Error reading \"{0:s}\" -> {1:s}", item.FileName, ex.Message);
                        }
                        break;
                    case AppType.Profiles:
                        try
                        {
                            bw.ReportProgress(-1, importProfiles);
                            List<EncoderProfile> importedProfiles = ProfilesHandler.ImportProfiles(lItem.FileName);
                            ProfilesHandler profiles = new ProfilesHandler();
                            foreach (EncoderProfile profile in profiles.ProfileList)
                            {
                                try
                                {
                                    EncoderProfile selProfile =
                                    importedProfiles.Single(
                                        encoderProfile =>
                                        encoderProfile.Name == profile.Name && encoderProfile.Type == profile.Type);
                                    importedProfiles.Remove(selProfile);
                                }
                                catch (Exception importException)
                                {
                                    Log.Error(importException);
                                }
                            }
                            foreach (EncoderProfile encoderProfile in importedProfiles)
                            {
                                profiles.AddProfile(encoderProfile);
                            }

                            profiles.Destroy();
                            AppSettings.LastProfilesVer = lItem.ServerVersion;
                        }
                        catch (Exception exProfiles)
                        {
                            Log.Error(exProfiles);
                        }
                        
                        break;
                    default:
                        {
                            _mainAppUpdate = true;
                            PackageInfo package = new PackageInfo
                                {
                                    PackageName = item.ToolName,
                                    PackageLocation = item.FileName,
                                    Version = item.ServerVersion,
                                    Destination = item.Destination,
                                    WriteVersion = item.DownloadType == AppType.AviSynthPlugins,
                                    ClearDirectory = item.DownloadType == AppType.MainApp,
                                    RecursiveClearDirectory = item.DownloadType == AppType.AviSynthPlugins
                                };
                            _packages.Add(package);
                        }
                        break;
                }
                bw.ReportProgress(-20, item);
            }

            if (_packages.Count > 0)
            {
                if (tempPath != null) MainAppUpdateFile = Path.Combine(tempPath, "update.xml");
                Updater.SaveUpdateList(MainAppUpdateFile, _packages);
            }
            else
            {
                bw.ReportProgress(-1, reloadStatus);
                Processing.GetAppVersions();
            }
            
            bw.ReportProgress(0, finishedStatus);
        }

        void UpdaterRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            AppSettings.LastUpdateRun = DateTime.Now;

            if (_mainAppUpdate)
            {
                string msg = Processing.GetResourceString("update_restart_needed");
                MessageBoxResult res = Xceed.Wpf.Toolkit.MessageBox.Show(msg,
                                                                      "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                if (res == MessageBoxResult.OK)
                {
                    Process updaterProcess = new Process();
                    ProcessStartInfo sInfo = new ProcessStartInfo
                                                 {
                                                     FileName =
                                                         Path.Combine(AppSettings.UpdaterPath, @"AppUpdater.exe"),
                                                     WorkingDirectory = AppSettings.UpdaterPath,
                                                     UseShellExecute = true,
                                                     Arguments =
                                                         string.Format(AppSettings.CInfo, "\"{0}\" \"{1}\"", MainAppUpdateFile,
                                                                       Assembly.GetExecutingAssembly().Location)
                                                 };
                    bool elevated = false;
                    try
                    {
                        elevated = Processing.IsProcessElevated();
                    }
                    catch (Exception)
                    {
                        Log.Error("Could not determine process elevation status");
                    }
                    Log.Info("Process created");

                    if (Environment.OSVersion.Version.Major >= 6 && !elevated)
                    {
                        sInfo.Verb = "runas";
                        Log.Info("Runas Set");
                    }
                    updaterProcess.StartInfo = sInfo;
                    Log.Info("before start");
                    try
                    {
                        updaterProcess.Start();
                    }
                    catch (Exception startEx)
                    {
                        Log.Error(startEx);
                    }
                    
                    Log.Info("after start");
                    AppSettings.UpdateVersions = true;
                    AppSettings.SaveSettings();
                    Application.Current.Shutdown();
                }

            }
            else
            {
                AppSettings.SaveSettings();
                if (Parent != null) ((Grid) Parent).Children.Clear();
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            if (Parent != null) ((Grid) Parent).Children.Clear();
        }
    }

    public class ToolVersions
    {
        public string ToolName { get; set; }
        public string LocalVersion { get; set; }
        public string ServerVersion { get; set; }
        public string DownloadUri { get; set; }
        public string FileName { get; set; }
        public string Destination { get; set; }
        public AppType DownloadType { get; set; }
    }

    public enum AppType { MainApp = 0, Updater, Encoder, AviSynthPlugins, Profiles }
}
