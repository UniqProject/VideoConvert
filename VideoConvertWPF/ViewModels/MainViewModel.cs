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
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using Caliburn.Micro;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using Newtonsoft.Json;
    using UpdateCore;
    using VideoConvert.AppServices.Services;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Utilities;
    using VideoConvertWPF.ViewModels.Interfaces;
    using ILog = log4net.ILog;
    using LogManager = log4net.LogManager;

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
                NotifyOfPropertyChange(()=>SelectedItem);
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

        public void Shutdown()
        {
            var jSer = new JsonSerializer();
            using (var sWriter = new StreamWriter(Path.Combine(_configService.AppSettingsPath, "JobQueue.json")))
            {
                var writer = new JsonTextWriter(sWriter);
                jSer.Serialize(writer, JobCollection);
                writer.Flush();
            }
        }

        public override void OnLoad()
        {
            base.OnLoad();
            
            JobCollection = new ObservableCollection<EncodeInfo>();
            JobCollection.CollectionChanged += JobCollectionChanged;

            try
            {
                var jSer = new JsonSerializer();
                using (
                    var sReader =
                        new StreamReader(Path.Combine(_configService.AppSettingsPath, "JobQueue.json")))
                {
                    JsonReader reader = new JsonTextReader(sReader);
                    var importList = jSer.Deserialize<List<EncodeInfo>>(reader);
                    foreach (var encodeInfo in importList)
                    {
                        AddJob(encodeInfo);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            

            CheckUpdate();

            Title = "My Title";
        }

        public void CheckUpdate()
        {
            var updateTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 1)};
            updateTimer.Tick += UpdateTimerTick;
            updateTimer.Start();
        }

        private void UpdateTimerTick(object sender, EventArgs e)
        {
            ((DispatcherTimer)sender).Stop();

            if (_updateCheckDone) return;

            if (_configService.FirstStart)
            {
                _shellViewModel.DisplayWindow(ShellWin.OptionsView);
                _configService.FirstStart = false;
                return;
            }
            if (_configService.ShowChangeLog && !_changeLogViewed && File.Exists(Path.Combine(_configService.AppPath, "updated")))
            {
                _shellViewModel.DisplayWindow(ShellWin.ChangelogView);
                _changeLogViewed = true;
                return;
            }
            if (_configService.ReloadToolVersions)
            {
                // TODO: Make Tool version reloading work
            }
            else
                RunUpdateWorker();

            _updateCheckDone = true;
        }

        private void RunUpdateWorker()
        {
            var checkUpdate = new BackgroundWorker
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
            var needUpdate = false;
            var needCheck = false;

            switch (_configService.UpdateFrequency)
            {
                case 0:
                    needCheck = true;
                    break;
                case 1:
                    if (_configService.LastUpdateRun.AddDays(1) < DateTime.Now)
                        needCheck = true;
                    break;
                case 2:
                    if (_configService.LastUpdateRun.AddDays(7) < DateTime.Now)
                        needCheck = true;
                    break;
            }

            if (needCheck && System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                StatusLabel = "Checking for updates ...";
                ShowStatusWindow = true;

                _processingService.GetUpdaterVersion();
                _processingService.GetAviSynthPluginsVer();

                var downloader = new WebClient { UseDefaultCredentials = true };
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

                using (var updateFile = Updater.LoadUpdateFileFromStream(onlineUpdateFile))
                {
                    if (updateFile.Core.PackageVersion.CompareTo(AppConfigService.GetAppVersion()) > 0)
                        needUpdate = true;

                    if (updateFile.Updater.PackageVersion.CompareTo(_configService.UpdaterVersion) > 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.AviSynthPlugins.PackageVersion, _configService.LastAviSynthPluginsVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.Profiles.PackageVersion, _configService.LastProfilesVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.X264.PackageVersion, _configService.Lastx264Ver) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.X26464.PackageVersion, _configService.Lastx26464Ver) != 0 &&
                        Environment.Is64BitOperatingSystem)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.FFMPEG.PackageVersion, _configService.LastffmpegVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.FFMPEG64.PackageVersion, _configService.Lastffmpeg64Ver) != 0 &&
                        Environment.Is64BitOperatingSystem)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.Eac3To.PackageVersion, _configService.Lasteac3ToVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.LsDvd.PackageVersion, _configService.LastlsdvdVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.MKVToolnix.PackageVersion, _configService.LastMKVMergeVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.Mplayer.PackageVersion, _configService.LastMplayerVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.TSMuxeR.PackageVersion, _configService.LastTSMuxerVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.MjpegTools.PackageVersion, _configService.LastMJPEGToolsVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.DVDAuthor.PackageVersion, _configService.LastDVDAuthorVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.MP4Box.PackageVersion, _configService.LastMp4BoxVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.HcEnc.PackageVersion, _configService.LastHcEncVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.OggEnc.PackageVersion, _configService.LastOggEncVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.OggEncLancer.PackageVersion, _configService.LastOggEncLancerVer) != 0
                        && _configService.UseOptimizedEncoders)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.Lame.PackageVersion, _configService.LastLameVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.VpxEnc.PackageVersion, _configService.LastVpxEncVer) != 0)
                        needUpdate = true;

                    if (string.CompareOrdinal(updateFile.BDSup2Sub.PackageVersion, _configService.LastBDSup2SubVer) != 0 &&
                        _configService.JavaInstalled)
                        needUpdate = true;
                }

                Thread.Sleep(2000);

                StatusLabel = "Ready";
                ShowStatusWindow = false;

                _configService.LastUpdateRun = DateTime.Now;
            }

            e.Result = needUpdate;
        }

        private void CheckUpdateRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateAvail = (bool) e.Result;
        }

        private void JobCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            JobCount = JobCollection.Count;
            NotifyOfPropertyChange(()=> JobCount);
        }

        public MainViewModel(IShellViewModel shellViewModel, IWindowManager windowManager, IAppConfigService config,
                             IProcessingService processingService)
        {
            _shellViewModel = shellViewModel;
            WindowManager = windowManager;
            _configService = config;
            _processingService = processingService;
        }

        #region ToolBar actions
        
        public void AddFiles()
        {
            var fileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = true,
                ShowPlacesList = true,
            };

            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            foreach (var fileName in fileDialog.FileNames)
                CreateJob(fileName);
        }

        public void AddFolder()
        {
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

            if (!string.IsNullOrEmpty(_lastDir))
                folderBrowser.InitialDirectory = _lastDir;

            if (folderBrowser.ShowDialog() != CommonFileDialogResult.Ok) return;

            CreateJob(folderBrowser.FileName);
            _lastDir = folderBrowser.FileName;
        }

        public void RemoveItem()
        {
            if (_selectedItem == null) return;

            JobCollection.Remove(_selectedItem);
            NotifyOfPropertyChange(()=>JobCollection);
            SelectedItem = null;
        }

        public void ClearList()
        {
            JobCollection.Clear();
            NotifyOfPropertyChange(()=>JobCollection);
        }

        public void StartEncode()
        {
            _shellViewModel.DisplayWindow(ShellWin.EncodeView, JobCollection);
        }

        public void ShowSettings()
        {
            _shellViewModel.DisplayWindow(ShellWin.OptionsView);
        }

        public void ShowUpdate()
        {
            StatusLabel = "Checking for Updates...";
            ShowStatusWindow = true;
        }

        #endregion
        

        private void CreateJob(string fileName)
        {
            var inJob = new EncodeInfo { InputFile = fileName, Input = _processingService.DetectInputType(fileName) };

            if ((string.IsNullOrEmpty(inJob.InputFile)) || (inJob.Input == InputType.InputUndefined)) return;

            var streamSelect = new StreamSelectViewModel(_configService, _shellViewModel,
                WindowManager, _processingService)
            {
                JobInfo = inJob,
            };

            if (WindowManager.ShowDialog(streamSelect, settings: new Dictionary<string, object>
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
                AddJob(inJob);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            
        }

        private void AddJob(EncodeInfo inJob)
        {
            try
            {
                JobCollection.Add(inJob);
                NotifyOfPropertyChange(() => JobCollection);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void SetOutput(EncodeInfo input)
        {
            var cleanJobName = input.JobName;
            var invalidChars = Path.GetInvalidFileNameChars();

            cleanJobName = invalidChars.Aggregate(cleanJobName, (current, invalidChar) => current.Replace(invalidChar.ToString(CultureInfo.InvariantCulture), ""));

            input.BaseName = cleanJobName;
            input.OutputFile = Path.Combine(_configService.OutputLocation, input.BaseName);

            var inputFilePath = Path.GetDirectoryName(input.InputFile);

            if (string.IsNullOrEmpty(inputFilePath))
                inputFilePath = input.InputFile;

            if (input.InputFile != null)
            {
                var inFile = Path.Combine(inputFilePath, Path.GetFileNameWithoutExtension(input.InputFile));

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

            Log.Info($"Add Input File: {input.InputFile}");
            Log.Info($"Input Format {input.Input}");
            Log.Info($"Output File: {input.OutputFile}");
            Log.Info($"Output Format {input.EncodingProfile.OutFormatStr}");
            Log.Info("Job Details");
            Log.Info(Environment.NewLine + input);
        }

        private void SetInOutTemp(EncodeInfo inJob)
        {
            var asciiFile = FileSystemHelper.GetAsciiFileName(inJob.InputFile);
            if (string.CompareOrdinal(inJob.InputFile, asciiFile) != 0)
                inJob.TempInput = FileSystemHelper.CreateTempFile(_configService.DemuxLocation, Path.GetExtension(inJob.InputFile));

            asciiFile = FileSystemHelper.GetAsciiFileName(inJob.OutputFile);

            if (string.CompareOrdinal(inJob.OutputFile, asciiFile) == 0) return;

            string fExt;
            if ((inJob.EncodingProfile.OutFormat == OutputType.OutputAvchd) ||
                (inJob.EncodingProfile.OutFormat == OutputType.OutputBluRay) ||
                (inJob.EncodingProfile.OutFormat == OutputType.OutputDvd))
                fExt = string.Empty;
            else
                fExt = Path.GetExtension(inJob.OutputFile);

            inJob.TempOutput = FileSystemHelper.CreateTempFile(_configService.DemuxLocation,
                string.IsNullOrEmpty(inJob.TempInput) ? asciiFile : inJob.TempInput, fExt);
        }
    }
}