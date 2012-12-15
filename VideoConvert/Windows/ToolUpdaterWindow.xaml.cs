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

using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls;
using System.ComponentModel;
using System.Net;
using System.Xml;
using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows;
using VideoConvert.Core;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;
using log4net;
using UpdateCore;
using System.Linq;

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

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                WebClient downloader = new WebClient {UseDefaultCredentials = true};
                string versionFile = downloader.DownloadString(new Uri("http://www.jt-soft.de/videoconvert/update.xml"));

                XmlDocument verFile = new XmlDocument();
                verFile.LoadXml(versionFile);

                XmlAttribute verAttrib = verFile.CreateAttribute("version");
                string tempPath = AppSettings.TempPath;

                if (verFile.SelectSingleNode("/videoconvert_updatefile") != null)      // small check if we have the right structure
                {
                    Version verOnline;
                    XmlNode appVersion = verFile.SelectSingleNode("//videoconvert");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null)
                            verAttrib = appVersion.Attributes["version"];
                        verOnline = new Version(verAttrib.Value);
                        if (verOnline.CompareTo(AppSettings.GetAppVersion()) > 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "Core",
                                                            LocalVersion = AppSettings.GetAppVersion().ToString(4),
                                                            ServerVersion = verOnline.ToString(4),
                                                            FileName =
                                                                Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPath + appVersion.InnerText,
                                                            DownloadType = AppType.MainApp,
                                                            Destination = AppSettings.AppPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//uac_updater");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null)
                            verAttrib = appVersion.Attributes["version"];
                        verOnline = new Version(verAttrib.Value);
                        if (verOnline.CompareTo(AppSettings.UpdaterVersion) > 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "Updater (UAC Compatible)",
                                                            LocalVersion = AppSettings.UpdaterVersion.ToString(4),
                                                            ServerVersion = verOnline.ToString(4),
                                                            FileName =
                                                                Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPath + appVersion.InnerText,
                                                            DownloadType = AppType.Updater,
                                                            Destination = AppSettings.UpdaterPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//avisynth_plugins");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastAviSynthPluginsVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "AviSynth Plugins",
                                                            LocalVersion = AppSettings.LastAviSynthPluginsVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPath + appVersion.InnerText,
                                                            DownloadType = AppType.AviSynthPlugins,
                                                            Destination = AppSettings.AvsPluginsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//profiles");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null)
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastProfilesVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "Profiles",
                                LocalVersion = AppSettings.LastProfilesVer,
                                ServerVersion = verAttrib.Value,
                                FileName = Path.Combine(tempPath, appVersion.InnerText),
                                DownloadUri = serverPath + appVersion.InnerText,
                                DownloadType = AppType.Profiles,
                                Destination = AppSettings.CommonAppSettingsPath
                            });
                    }

                    appVersion = verFile.SelectSingleNode("//x264");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.Lastx264Ver) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "x264",
                                                            LocalVersion = AppSettings.Lastx264Ver,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//ffmpeg");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastffmpegVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "ffmpeg",
                                                            LocalVersion = AppSettings.LastffmpegVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//eac3to");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.Lasteac3ToVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "eac3to",
                                                            LocalVersion = AppSettings.Lasteac3ToVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//lsdvd");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastlsdvdVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "lsdvd",
                                                            LocalVersion = AppSettings.LastlsdvdVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//mkv_toolnix");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastMKVMergeVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "MKVToolnix",
                                                            LocalVersion = AppSettings.LastMKVMergeVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//mplayer");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastMplayerVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "MPlayer",
                                                            LocalVersion = AppSettings.LastMplayerVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//tsmuxer");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastTSMuxerVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "TSMuxeR",
                                                            LocalVersion = AppSettings.LastTSMuxerVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//mjpegtools");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastMJPEGtoolsVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "mjpegtools",
                                                            LocalVersion = AppSettings.LastMJPEGtoolsVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//dvdauthor");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastDVDAuthorVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "DVDAuthor",
                                                            LocalVersion = AppSettings.LastDVDAuthorVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//mp4box");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastMp4BoxVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "MP4Box",
                                                            LocalVersion = AppSettings.LastMp4BoxVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//hcenc");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastHcEncVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "HCEnc",
                                                            LocalVersion = AppSettings.LastHcEncVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//oggenc");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null)
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastOggEncVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "OggEnc2",
                                                            LocalVersion = AppSettings.LastOggEncVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//lame");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null)
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastLameVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "Lame",
                                                            LocalVersion = AppSettings.LastLameVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }

                    appVersion = verFile.SelectSingleNode("//vpxenc");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null)
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastVpxEncVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                            {
                                ToolName = "VP8 Encoder",
                                LocalVersion = AppSettings.LastVpxEncVer,
                                ServerVersion = verAttrib.Value,
                                FileName = Path.Combine(tempPath, appVersion.InnerText),
                                DownloadUri = serverPathTools + appVersion.InnerText,
                                DownloadType = AppType.Encoder,
                                Destination = AppSettings.ToolsPath
                            });
                    }

                    appVersion = verFile.SelectSingleNode("//bdsup2sub");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null) 
                            verAttrib = appVersion.Attributes["version"];
                        if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastBDSup2SubVer) != 0)
                            _tempToolCollection.Add(new ToolVersions
                                                        {
                                                            ToolName = "bdsup2sub",
                                                            LocalVersion = AppSettings.LastBDSup2SubVer,
                                                            ServerVersion = verAttrib.Value,
                                                            FileName = Path.Combine(tempPath, appVersion.InnerText),
                                                            DownloadUri = serverPathTools + appVersion.InnerText,
                                                            DownloadType = AppType.Encoder,
                                                            Destination = AppSettings.ToolsPath
                                                        });
                    }
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
                                                          WriteVersion = item.DownloadType == AppType.AviSynthPlugins
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
