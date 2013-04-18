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
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using VideoConvert.Core;
using log4net;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Xml;

namespace VideoConvert.Windows
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));

        private readonly ObservableCollection<EncodeInfo> _jobCollection = new ObservableCollection<EncodeInfo>();

        public ObservableCollection<EncodeInfo> JobCollection { get { return _jobCollection; } }

        public MainWindow()
        {
            InitializeComponent();
            _jobCollection.CollectionChanged += JobCollectionCollectionChanged;
        }

        void JobCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateEncodeButton();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            Version tempVer = AppSettings.GetAppVersion();

            AppVersionLabel.Content = string.Format("{0} v{1:g}.{2:g} (build @ {3})",
                                                    Title,
                                                    tempVer.Major,
                                                    tempVer.Minor,
                                                    AppSettings.GetAppBuildDate().ToString("dd.MM.yyyy HH:mm"));

            DispatcherTimer updateTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 1)};
            updateTimer.Tick += UpdateTimerTick;
            updateTimer.Start();
        }

        void UpdateTimerTick(object sender, EventArgs e)
        {
            ((DispatcherTimer) sender).Stop();

            bool reloadVersions = AppSettings.UpdateVersions;

            if (AppSettings.FirstStart)
            {
                // get application settings from previous version, if possible
                AppSettings.Upgrade();
                
                if (AppSettings.FirstStart)
                {
                    SettingsWindow settings = new SettingsWindow {Owner = this};
                    settings.ShowDialog();
                }
                else
                {
                    if (AppSettings.ShowChangeLog)
                    {
                        ChangelogViewer changeLog = new ChangelogViewer {Owner = this};
                        changeLog.ShowDialog();
                    }
                    string msg = Processing.GetResourceString("update_done");
                    Xceed.Wpf.Toolkit.MessageBox.Show(msg,
                                                      "Update",
                                                      MessageBoxButton.OK,
                                                      MessageBoxImage.Information);
                    reloadVersions = true;
                }
            }

            if (reloadVersions)
            {
                BackgroundWorker reloadV = new BackgroundWorker
                    {
                        WorkerReportsProgress = true,
                        WorkerSupportsCancellation = false
                    };

                reloadV.RunWorkerCompleted += (o, args) => RunUpdateWorker();
                reloadV.DoWork += (o, args) =>
                    {
                        BackgroundWorker bw = (BackgroundWorker) o;
                        bw.ReportProgress(-5);
                        Processing.GetAppVersions();
                        bw.ReportProgress(-6);

                    };
                reloadV.ProgressChanged += (o, args) =>
                    {
                        switch (args.ProgressPercentage)
                        {
                            case -5:
                                string msg = Processing.GetResourceString("check_versions");

                                StatusItem.Text = msg;
                                StatusItem.FontWeight = FontWeights.Bold;
                                StatusItemHeader.Background = Brushes.Yellow;
                                break;

                            case -6:
                                StatusItem.Text = string.Empty;
                                StatusItem.FontWeight = FontWeights.Normal;
                                StatusItemHeader.Background = Brushes.Transparent;
                                break;
                        }
                    };
                reloadV.RunWorkerAsync();
            }
            else
                RunUpdateWorker();
        }

        public void RunUpdateWorker()
        {
            BackgroundWorker checkUpdate = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
            checkUpdate.RunWorkerCompleted += CheckUpdateRunWorkerCompleted;
            checkUpdate.DoWork += CheckUpdateDoWork;
            checkUpdate.RunWorkerAsync();
        }

        public void CheckUpdateDoWork(object sender, DoWorkEventArgs e)
        {
            bool needUpdate = false;

            bool needCheck = false;

            switch (AppSettings.UpdateFrequency)
            {
                case 0:
                    needCheck = true;
                    break;
                case 1:
                    if (AppSettings.LastUpdateRun.AddDays(1) > DateTime.Now)
                        needCheck = true;
                    break;
                case 2:
                    if (AppSettings.LastUpdateRun.AddDays(7) > DateTime.Now)
                        needCheck = true;
                    break;
            }

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() && needCheck)
            {
                Processing.GetUpdaterVersion();
                Processing.GetAviSynthPluginsVer();

                WebClient downloader = new WebClient {UseDefaultCredentials = true};
                string versionFile = string.Empty;
                try
                {
                    versionFile = downloader.DownloadString(new Uri("http://www.jt-soft.de/videoconvert/update.xml"));
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                }
                if (string.IsNullOrEmpty(versionFile))
                {
                    e.Result = false;
                    return;
                }

                XmlDocument verFile = new XmlDocument();
                verFile.LoadXml(versionFile);

                // small check if we have the right structure
                if (verFile.SelectSingleNode("/videoconvert_updatefile") != null)      
                {
                    XmlNode appVersion = verFile.SelectSingleNode("//videoconvert");
                    if (appVersion != null)
                    {
                        if (appVersion.Attributes != null)
                        {
                            XmlAttribute verAttrib = appVersion.Attributes["version"];

                            Version verOnline = new Version(verAttrib.Value);
                            if (verOnline.CompareTo(AppSettings.GetAppVersion()) > 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//uac_updater");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            verOnline = new Version(verAttrib.Value);
                            if (verOnline.CompareTo(AppSettings.UpdaterVersion) > 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//avisynth_plugins");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastAviSynthPluginsVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//profiles");
                            if (appVersion != null)
                                if (appVersion.Attributes != null)
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastProfilesVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//x264");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.Lastx264Ver) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//x264_64");
                            if (appVersion != null)
                                if (appVersion.Attributes != null)
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.Lastx26464Ver) != 0 &&
                                Environment.Is64BitOperatingSystem)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//ffmpeg");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastffmpegVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//eac3to");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.Lasteac3ToVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//lsdvd");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastlsdvdVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//mkv_toolnix");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastMKVMergeVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//mplayer");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastMplayerVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//tsmuxer");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastTSMuxerVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//mjpegtools");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastMJPEGToolsVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//dvdauthor");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastDVDAuthorVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//mp4box");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastMp4BoxVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//hcenc");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastHcEncVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//oggenc");
                            if (appVersion != null)
                                if (appVersion.Attributes != null)
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastOggEncVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//lame");
                            if (appVersion != null)
                                if (appVersion.Attributes != null)
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastLameVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//vpxenc");
                            if (appVersion != null)
                                if (appVersion.Attributes != null)
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastVpxEncVer) != 0)
                                needUpdate = true;

                            appVersion = verFile.SelectSingleNode("//bdsup2sub");
                            if (appVersion != null)
                                if (appVersion.Attributes != null) 
                                    verAttrib = appVersion.Attributes["version"];
                            if (String.CompareOrdinal(verAttrib.Value, AppSettings.LastBDSup2SubVer) != 0 &&
                                AppSettings.JavaInstalled)
                                needUpdate = true;
                        }
                    }
                }
            }

            if (!needUpdate)
                AppSettings.LastUpdateRun = DateTime.Now;

            e.Result = needUpdate;
        }

        public void CheckUpdateRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!((bool) e.Result)) return;

            string msg = Processing.GetResourceString("update_available");

            StatusItem.Text = msg;
            StatusItem.FontWeight = FontWeights.Bold;
            StatusItemHeader.Background = Brushes.Green;
            StatusItemLink.Click += (s, ev) =>
                {
                    if (ViewControl.Children.Count != 0) return;

                    StatusItemHeader.Visibility = Visibility.Collapsed;
                    ToolUpdaterWindow updater = new ToolUpdaterWindow();
                    ViewControl.Children.Add(updater);
                };
        }

        private void EncodeBtnClick(object sender, RoutedEventArgs e)
        {
            if (ViewControl.Children.Count > 0) return;

            EncodingWindow encoder = new EncodingWindow {JobList = JobCollection, TaskBar = TaskbarItemInfo};
            ViewControl.Children.Add(encoder);
        }

        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow {Owner = this};
            settings.ShowDialog();
        }

        private void ClearListClick(object sender, RoutedEventArgs e)
        {
            _jobCollection.Clear();
        }

        private void MainFormSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((!e.HeightChanged) || (e.NewSize.Height <= e.PreviousSize.Height)) return;

            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowHeight = e.NewSize.Height;
            Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void AddFilesClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
                                            {DereferenceLinks = false, Multiselect = true, ValidateNames = true};
            if (fileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            foreach (string fileName in fileDialog.FileNames)
                CreateJob(fileName);
        }

        private void AddFolderClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateJob(folderBrowser.SelectedPath);
        }

        private void CreateJob(string fileName)
        {
            EncodeInfo inJob = new EncodeInfo {InputFile = fileName, Input = Processing.DetectInputType(fileName)};

            if ((string.IsNullOrEmpty(inJob.InputFile)) || (inJob.Input == InputType.InputUndefined)) return;

            StreamSelect streamSelection = new StreamSelect {JobInfo = inJob, Owner = this};
            bool? retValue = streamSelection.ShowDialog();
            if (retValue != true) return;

            inJob = SetOutput(inJob);
            inJob = SetInOutTemp(inJob);
            JobCollection.Add(inJob);
        }

        private static EncodeInfo SetInOutTemp(EncodeInfo inJob)
        {
            string asciiFile = Processing.GetAsciiFileName(inJob.InputFile);
            if (string.CompareOrdinal(inJob.InputFile, asciiFile) != 0)
                inJob.TempInput = Processing.CreateTempFile(Path.GetExtension(inJob.InputFile));

            asciiFile = Processing.GetAsciiFileName(inJob.OutputFile);
            if (string.CompareOrdinal(inJob.OutputFile, asciiFile) != 0)
            {
                string fExt;
                if ((inJob.EncodingProfile.OutFormat == OutputType.OutputAvchd) ||
                    (inJob.EncodingProfile.OutFormat == OutputType.OutputBluRay) ||
                    (inJob.EncodingProfile.OutFormat == OutputType.OutputDvd))
                    fExt = string.Empty;
                else
                    fExt = Path.GetExtension(inJob.OutputFile);

                inJob.TempOutput = Processing.CreateTempFile(string.IsNullOrEmpty(inJob.TempInput) ? asciiFile : inJob.TempInput, fExt);
            }

            return inJob;
        }

        /// <summary>
        /// sets output filename based on output type
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static EncodeInfo SetOutput(EncodeInfo input)
        {
            input.OutputFile = Path.Combine(AppSettings.OutputLocation, input.JobName);

            string inputFilePath = Path.GetDirectoryName(input.InputFile);

            if (string.IsNullOrEmpty(inputFilePath))
                inputFilePath = input.InputFile;

            string inFile = Path.Combine(inputFilePath,
// ReSharper disable AssignNullToNotNullAttribute
                                         Path.GetFileNameWithoutExtension(input.InputFile));
// ReSharper restore AssignNullToNotNullAttribute

            if (inFile == input.OutputFile)
            {
                input.OutputFile += ".new";
            }

            switch (input.EncodingProfile.OutFormat)
            {
                case OutputType.OutputMatroska:
                    input.OutputFile += ".mkv";
                    break;
                case OutputType.OutputWebM:
                    input.OutputFile += ".webm";
                    break;
                case OutputType.OutputMp4:
                    input.OutputFile += ".mp4";
                    break;
                case OutputType.OutputTs:
                    input.OutputFile += ".ts";
                    break;
                case OutputType.OutputM2Ts:
                    input.OutputFile += ".m2ts";
                    break;
                case OutputType.OutputBluRay:
                    break;
                case OutputType.OutputAvchd:
                    break;
                case OutputType.OutputDvd:
                    break;
            }

            Log.InfoFormat("Add Input File: {0:s}", input.InputFile);
            Log.InfoFormat("Input Format {0}", input.Input);
            Log.InfoFormat("Output File: {0:s}", input.OutputFile);
            Log.InfoFormat("Output Format {0}", input.EncodingProfile.OutFormatStr);
            Log.Info("Job Details");
            Log.Info(Environment.NewLine + input);
            return input;
        }

        private void ViewControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateEncodeButton();
            UpdateFileHandlingButtons();
        }

        private void UpdateFileHandlingButtons()
        {
            if (!IsLoaded) return;

            bool enable = ViewControl.Children.Count > 0;

            ShowSettingsBtn.IsEnabled = !enable;
            AddFilesBtn.IsEnabled = !enable;
            AddFolderBtn.IsEnabled = !enable;
            RemoveItemBtn.IsEnabled = !enable;
            ClearListBtn.IsEnabled = !enable;
        }

        private void UpdateEncodeButton()
        {
            if (!IsLoaded) return;

            if (ViewControl.Children.Count > 0)
                RunEncodeBtn.IsEnabled = false;
            else
            {
                RunEncodeBtn.IsEnabled = _jobCollection.Count > 0;
            }
        }

        private void RemoveEntryClick(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItems.Count > 0)
                _jobCollection.Remove(FileList.SelectedItem as EncodeInfo);
        }
    }
}
