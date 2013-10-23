// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Threading;
    using Caliburn.Micro;
    using Interfaces;
    using UpdateCore;
    using VideoConvert.AppServices.Services;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Utilities;
    using ILog = log4net.ILog;
    using LogManager = log4net.LogManager;
    using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private readonly IShellViewModel _shellViewModel;

        private static readonly ILog Log = LogManager.GetLogger(typeof(MainViewModel));

        private static string _lastDir;
        private EncodeInfo _selectedItem;
        private string _statusLabel;
        private bool _showStatusWindow;
        private bool _changeLogViewed;
        private bool _updateCheckDone;

        public int JobCount { get; set; }
        public ObservableCollection<EncodeInfo> JobCollection { get; set; }

        private readonly IAppConfigService _configService;
        private readonly IProcessingService _processingService;

        public EncodeInfo SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                this.NotifyOfPropertyChange(()=>this.SelectedItem);
            }
        }

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

        public void Shutdown()
        {
            
        }

        public override void OnLoad()
        {
            base.OnLoad();
            
            JobCollection = new ObservableCollection<EncodeInfo>();
            JobCollection.CollectionChanged += JobCollectionChanged;

            CheckUpdate();

            this.Title = "My Title";
        }

        public void CheckUpdate()
        {
            DispatcherTimer updateTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 1)};
            updateTimer.Tick += UpdateTimerTick;
            updateTimer.Start();
        }

        private void UpdateTimerTick(object sender, EventArgs e)
        {
            ((DispatcherTimer)sender).Stop();

            if (_updateCheckDone) return;

            if (this._configService.FirstStart)
            {
                this._shellViewModel.DisplayWindow(ShellWin.OptionsView);
                this._configService.FirstStart = false;
                return;
            }
            if (this._configService.ShowChangeLog && !_changeLogViewed && File.Exists(Path.Combine(_configService.AppPath, "updated")))
            {
                this._shellViewModel.DisplayWindow(ShellWin.ChangelogView);
                _changeLogViewed = true;
                return;
            }
            if (this._configService.ReloadToolVersions)
            {
                // TODO: Make Tool version reloading work
            }
            else
                RunUpdateWorker();

            _updateCheckDone = true;
        }

        private void RunUpdateWorker()
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

        private void CheckUpdateDoWork(object sender, DoWorkEventArgs e)
        {
            bool needUpdate = false;
            bool needCheck = false;

            switch (this._configService.UpdateFrequency)
            {
                case 0:
                    needCheck = true;
                    break;
                case 1:
                    if (this._configService.LastUpdateRun.AddDays(1) < DateTime.Now)
                        needCheck = true;
                    break;
                case 2:
                    if (this._configService.LastUpdateRun.AddDays(7) < DateTime.Now)
                        needCheck = true;
                    break;
            }

            if (needCheck && System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                this.StatusLabel = "Checking for updates ...";
                this.ShowStatusWindow = true;

                _processingService.GetUpdaterVersion();
                _processingService.GetAviSynthPluginsVer();

                WebClient downloader = new WebClient { UseDefaultCredentials = true };
                Stream onlineUpdateFile;
                try
                {
                    onlineUpdateFile = downloader.OpenRead(new Uri("http://www.jt-soft.de/videoconvert/updatefile_7z.xml"));
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
                    if (updateFile.Core.PackageVersion.CompareTo(AppConfigService.GetAppVersion()) > 0)
                        needUpdate = true;

                    if (updateFile.Updater.PackageVersion.CompareTo(_configService.UpdaterVersion) > 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.AviSynthPlugins.PackageVersion, this._configService.LastAviSynthPluginsVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.Profiles.PackageVersion, this._configService.LastProfilesVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.X264.PackageVersion, this._configService.Lastx264Ver) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.X26464.PackageVersion, this._configService.Lastx26464Ver) != 0 &&
                        Environment.Is64BitOperatingSystem)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.FFMPEG.PackageVersion, this._configService.LastffmpegVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.FFMPEG64.PackageVersion, this._configService.Lastffmpeg64Ver) != 0 &&
                        Environment.Is64BitOperatingSystem)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.Eac3To.PackageVersion, this._configService.Lasteac3ToVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.LsDvd.PackageVersion, this._configService.LastlsdvdVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.MKVToolnix.PackageVersion, this._configService.LastMKVMergeVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.Mplayer.PackageVersion, this._configService.LastMplayerVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.TSMuxeR.PackageVersion, this._configService.LastTSMuxerVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.MjpegTools.PackageVersion, this._configService.LastMJPEGToolsVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.DVDAuthor.PackageVersion, this._configService.LastDVDAuthorVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.MP4Box.PackageVersion, this._configService.LastMp4BoxVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.HcEnc.PackageVersion, this._configService.LastHcEncVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.OggEnc.PackageVersion, this._configService.LastOggEncVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.OggEncLancer.PackageVersion, this._configService.LastOggEncLancerVer) != 0
                        && this._configService.UseOptimizedEncoders)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.Lame.PackageVersion, this._configService.LastLameVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.VpxEnc.PackageVersion, this._configService.LastVpxEncVer) != 0)
                        needUpdate = true;

                    if (String.CompareOrdinal(updateFile.BDSup2Sub.PackageVersion, this._configService.LastBDSup2SubVer) != 0 &&
                        this._configService.JavaInstalled)
                        needUpdate = true;
                }

                Thread.Sleep(2000);

                this.StatusLabel = "Ready";
                this.ShowStatusWindow = false;

                this._configService.LastUpdateRun = DateTime.Now;
            }

            e.Result = needUpdate;
        }

        private void CheckUpdateRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.UpdateAvail = (bool) e.Result;
        }

        private void JobCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            JobCount = JobCollection.Count;
            this.NotifyOfPropertyChange(()=> this.JobCount);
        }

        public MainViewModel(IShellViewModel shellViewModel, IWindowManager windowManager, IAppConfigService config,
                             IProcessingService processingService)
        {
            this._shellViewModel = shellViewModel;
            this.WindowManager = windowManager;
            this._configService = config;
            _processingService = processingService;
        }

        #region ToolBar actions
        
        public void AddFiles()
        {
            OpenFileDialog fileDialog = new OpenFileDialog { DereferenceLinks = false, Multiselect = true, ValidateNames = true };
            if (fileDialog.ShowDialog() != true) return;
            foreach (string fileName in fileDialog.FileNames)
                CreateJob(fileName);
        }

        public void AddFolder()
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(_lastDir))
                folderBrowser.SelectedPath = _lastDir;

            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                CreateJob(folderBrowser.SelectedPath);
                _lastDir = folderBrowser.SelectedPath;
            }
        }

        public void RemoveItem()
        {
            if (_selectedItem != null)
            {
                JobCollection.Remove(_selectedItem);
                this.NotifyOfPropertyChange(()=>this.JobCollection);
                SelectedItem = null;
            }
        }

        public void ClearList()
        {
            JobCollection.Clear();
            this.NotifyOfPropertyChange(()=>JobCollection);
        }

        public void StartEncode()
        {
            
        }

        public void ShowSettings()
        {
            _shellViewModel.DisplayWindow(ShellWin.OptionsView);
        }

        public void ShowUpdate()
        {
            this.StatusLabel = "Checking for Updates...";
            this.ShowStatusWindow = true;
        }

        #endregion
        

        private void CreateJob(string fileName)
        {
            EncodeInfo inJob = new EncodeInfo { InputFile = fileName, Input = _processingService.DetectInputType(fileName) };

            if ((string.IsNullOrEmpty(inJob.InputFile)) || (inJob.Input == InputType.InputUndefined)) return;

            StreamSelectViewModel streamSelect = new StreamSelectViewModel(this._configService, _shellViewModel,
                WindowManager, _processingService)
            {
                JobInfo = inJob,
            };

            if (this.WindowManager.ShowDialog(streamSelect, settings: new Dictionary<string, object>
                                                            {
                                                                {"ShowInTaskbar", false},
                                                                {"ResizeMode", ResizeMode.CanMinimize},
                                                                {"Title", "Select Streams"}
                                                            }) != true) return;
            try
            {
                _processingService.CheckSubtitles(inJob);
                _processingService.CheckStreamLimit(inJob);
                SetOutput(inJob);
                SetInOutTemp(inJob);
                JobCollection.Add(inJob);
                this.NotifyOfPropertyChange(()=>this.JobCollection);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            
        }

        private void SetOutput(EncodeInfo input)
        {
            string cleanJobName = input.JobName;
            char[] invalidChars = Path.GetInvalidFileNameChars();

            cleanJobName = invalidChars.Aggregate(cleanJobName, (current, invalidChar) => current.Replace(invalidChar.ToString(CultureInfo.InvariantCulture), ""));

            input.BaseName = cleanJobName;
            input.OutputFile = Path.Combine(this._configService.OutputLocation, input.BaseName);

            string inputFilePath = Path.GetDirectoryName(input.InputFile);

            if (string.IsNullOrEmpty(inputFilePath))
                inputFilePath = input.InputFile;

            if (input.InputFile != null)
            {
                string inFile = Path.Combine(inputFilePath, Path.GetFileNameWithoutExtension(input.InputFile));

                if (inFile == input.OutputFile)
                {
                    input.OutputFile += ".new";
                }
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

            Log.InfoFormat("Add Input File: {0}", input.InputFile);
            Log.InfoFormat("Input Format {0}", input.Input);
            Log.InfoFormat("Output File: {0}", input.OutputFile);
            Log.InfoFormat("Output Format {0}", input.EncodingProfile.OutFormatStr);
            Log.Info("Job Details");
            Log.Info(Environment.NewLine + input);
        }

        private void SetInOutTemp(EncodeInfo inJob)
        {
            string asciiFile = FileSystemHelper.GetAsciiFileName(inJob.InputFile);
            if (string.CompareOrdinal(inJob.InputFile, asciiFile) != 0)
                inJob.TempInput = FileSystemHelper.CreateTempFile(this._configService.DemuxLocation, Path.GetExtension(inJob.InputFile));

            asciiFile = FileSystemHelper.GetAsciiFileName(inJob.OutputFile);
            if (string.CompareOrdinal(inJob.OutputFile, asciiFile) != 0)
            {
                string fExt;
                if ((inJob.EncodingProfile.OutFormat == OutputType.OutputAvchd) ||
                    (inJob.EncodingProfile.OutFormat == OutputType.OutputBluRay) ||
                    (inJob.EncodingProfile.OutFormat == OutputType.OutputDvd))
                    fExt = string.Empty;
                else
                    fExt = Path.GetExtension(inJob.OutputFile);

                inJob.TempOutput = FileSystemHelper.CreateTempFile(this._configService.DemuxLocation,
                    string.IsNullOrEmpty(inJob.TempInput) ? asciiFile : inJob.TempInput, fExt);
            }
        }
    }
}