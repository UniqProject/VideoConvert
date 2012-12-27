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
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using VideoConvert.Core;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Media;
using VideoConvert.Core.Profiles;
using VideoConvert.Core.Video.x264;
using Xceed.Wpf.Toolkit;
using log4net;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using GroupBox = System.Windows.Controls.GroupBox;
using RadioButton = System.Windows.Controls.RadioButton;
using TabControl = System.Windows.Controls.TabControl;
using TextBox = System.Windows.Controls.TextBox;

namespace VideoConvert.Windows
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SettingsWindow));

        private string _encPath;
        private string _javaPath;
        private ProfilesHandler _profiles = new ProfilesHandler();

        private QuickSelectProfile _selectedQuickProfile;

        private EncoderProfile _selectedVideoProfile;
        private EncoderProfile _selectedAudioProfile;

        private readonly List<TargetSize> _sizeList = TargetSize.GenerateList();
        private readonly List<SizeModificator> _sizeMod = SizeModificator.GenerateList();

        private X264Device _actualDevice;

        private string _inputPrompt;

        public ObservableCollection<SupportedLanguage> Langs
        {
            get { return new ObservableCollection<SupportedLanguage>(SupportedLanguage.GetSupportedLanguages()); }
        }

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            #region get BDInfo version
            try
            {
                Assembly ass = Assembly.GetAssembly(typeof(BDInfo.TSCodecAC3));
                AssemblyName asname = ass.GetName();
                Version ver = asname.Version;
                BDInfoVer.Content = string.Format("{0:g}.{1:g}.{2:g}", ver.Major, ver.Minor, ver.Build);
            }
// ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
            {
            }
            #endregion

            #region get MediaInfo version
            try
            {
                MediaInfo mi = new MediaInfo();
                string miVer = mi.Option("Info_Version");
                MediaInfoVer.Content = miVer;
                mi.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            #endregion

            List<X264Device> x264DeviceList = X264Device.CreateDeviceList();
            X264TuneDevice.ItemsSource = x264DeviceList;

            // load available fonts in the background, to decrease window load times
            BackgroundWorker fontLoader = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

            fontLoader.DoWork += (bwSender, args) =>
                {
                    BackgroundWorker bw = (BackgroundWorker) bwSender;
                    foreach (FontFamily item in Fonts.SystemFontFamilies)
                        bw.ReportProgress(-1, item);
                };
            fontLoader.ProgressChanged += (o, args) =>
                {
                    FontFamily font = (FontFamily) args.UserState;
                    tsMuxeRFontSwitcher.Items.Add(font);
                };
            fontLoader.RunWorkerCompleted +=
                (o, args) => { tsMuxeRFontSwitcher.SelectedItem = AppSettings.TSMuxeRSubtitleFont; };

            fontLoader.RunWorkerAsync();
            
            tsMuxeRFontColorPicker.SelectedColor = AppSettings.TSMuxeRSubtitleColor;

            ReloadQuickProfileList();
            ReloadVideoProfiles();
            ReloadAudioProfiles();
            LoadStaticValues();

            VideoEncoder.SelectedIndex = 1; //select x264 as default video encoder
            AudioEncoder.SelectedIndex = 1; //select ac3 as default audio encoder

            QuickSelectTargetSize.ItemsSource = _sizeList;
            QuickSelectTargetSizeMod.ItemsSource = _sizeMod;

            _inputPrompt = Processing.GetResourceString("profile_input_prompt");
        }

        private void LoadStaticValues()
        {
            Array enumValues = Enum.GetValues(typeof(OutputType));
            QuickSelectOutputFormat.Items.Clear();
            foreach (OutputType item in enumValues)
                QuickSelectOutputFormat.Items.Add(Processing.StringValueOf(item));
        }

        private void ReloadQuickProfileList()
        {
            int selectedProfile = ProfilesSelection.SelectedIndex;

            if (_profiles.ProfileList.Count == 1)
                _profiles = new ProfilesHandler();

            ProfilesSelection.ItemsSource = from p in _profiles.ProfileList
                                            where p.Type == ProfileType.QuickSelect
                                            select p;
            if (!IsLoaded) return;

            ProfilesSelection.SelectedIndex = selectedProfile;
        }

        private void ReloadVideoProfiles()
        {
            int selectedProfile = QuickProfileVideoSelect.SelectedIndex;
            switch (QuickSelectOutputFormat.SelectedIndex)
            {
                case 0:
                    QuickProfileVideoSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where
                                                              p.Type == ProfileType.Copy || p.Type == ProfileType.X264 ||
                                                              p.Type == ProfileType.XVid || p.Type == ProfileType.HcEnc ||
                                                              p.Type == ProfileType.VP8
                                                          select p;
                    break;
                case 1:
                    QuickProfileVideoSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where
                                                              p.Type == ProfileType.X264 || p.Type == ProfileType.XVid ||
                                                              p.Type == ProfileType.HcEnc
                                                          select p;
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                    QuickProfileVideoSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where
                                                              p.Type == ProfileType.X264 || p.Type == ProfileType.HcEnc
                                                          select p;
                    break;
                case 6:
                    QuickProfileVideoSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where
                                                              p.Type == ProfileType.HcEnc
                                                          select p;
                    break;
                case 7:
                    QuickProfileVideoSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where
                                                              p.Type == ProfileType.VP8
                                                          select p;
                    break;
            }

            if (IsLoaded)
                QuickProfileVideoSelect.SelectedIndex = selectedProfile;

            selectedProfile = VideoProfileSelect.SelectedIndex;
            switch (VideoEncoder.SelectedIndex)
            {
                case 0:
                    VideoProfileSelect.ItemsSource = from p in _profiles.ProfileList
                                                     where p.Type == ProfileType.Copy
                                                     select p;
                    break;
                case 1:
                    VideoProfileSelect.ItemsSource = from p in _profiles.ProfileList
                                                     where p.Type == ProfileType.X264
                                                     select p;
                    break;
                case 2:
                    VideoProfileSelect.ItemsSource = from p in _profiles.ProfileList
                                                     where p.Type == ProfileType.XVid
                                                     select p;
                    break;
                case 3:
                    VideoProfileSelect.ItemsSource = from p in _profiles.ProfileList
                                                     where p.Type == ProfileType.HcEnc
                                                     select p;
                    break;
                case 4:
                    VideoProfileSelect.ItemsSource = from p in _profiles.ProfileList
                                                     where p.Type == ProfileType.VP8
                                                     select p;
                    break;
            }
            if (!IsLoaded) return;
            if (VideoProfileSelect.Items.Count <= 0) return;

            selectedProfile = selectedProfile == -1 ? 0 : selectedProfile;
            VideoProfileSelect.SelectedIndex = selectedProfile > VideoProfileSelect.Items.Count ? 0 : selectedProfile;
        }

        private void ReloadAudioProfiles()
        {
            string selectedAudioProfile = QuickProfileAudioSelect.SelectedValue as string;

            switch (QuickSelectOutputFormat.SelectedIndex)
            {
                case 0:     // matroska
                    QuickProfileAudioSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where
                                                              p.Type == ProfileType.AC3 || p.Type == ProfileType.Copy ||
                                                              p.Type == ProfileType.OGG || p.Type == ProfileType.MP3 ||
                                                              p.Type == ProfileType.AAC
                                                          select p;
                    break;
                case 1:     // mp4
                    QuickProfileAudioSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where p.Type == ProfileType.MP3 || p.Type == ProfileType.AAC
                                                          select p;
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                    QuickProfileAudioSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where
                                                              p.Type == ProfileType.AC3 || p.Type == ProfileType.Copy
                                                          select p;
                    break;
                case 6:
                    QuickProfileAudioSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where
                                                              p.Type == ProfileType.AC3
                                                          select p;
                    break;
                case 7:
                case 8:
                    QuickProfileAudioSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where p.Type == ProfileType.OGG
                                                          select p;
                    break;
                default:
                    QuickProfileAudioSelect.ItemsSource = from p in _profiles.ProfileList
                                                          where p.Type == ProfileType.AC3 || p.Type == ProfileType.Copy
                                                          select p;
                    break;
            }

            if (IsLoaded)
                QuickProfileAudioSelect.SelectedValue = selectedAudioProfile;

            int selectedProfile = AudioProfileSelect.SelectedIndex;
            switch (AudioEncoder.SelectedIndex)
            {
                case 0:
                    AudioProfileSelect.ItemsSource = from p in _profiles.ProfileList
                                                     where p.Type == ProfileType.Copy
                                                     select p;
                    break;
                case 1:
                    AudioProfileSelect.ItemsSource = from p in _profiles.ProfileList
                                                     where p.Type == ProfileType.AC3
                                                     select p;
                    break;
                case 2:
                    AudioProfileSelect.ItemsSource = from p in _profiles.ProfileList
                                                     where p.Type == ProfileType.OGG
                                                     select p;
                    break;
                case 3:
                    AudioProfileSelect.ItemsSource = from p in _profiles.ProfileList
                                                     where p.Type == ProfileType.MP3
                                                     select p;
                    break;
                case 4:
                    AudioProfileSelect.ItemsSource = from p in _profiles.ProfileList
                                                     where p.Type == ProfileType.AAC
                                                     select p;
                    break;
            }

            if (!IsLoaded) return;
            if (AudioProfileSelect.Items.Count <= 0) return;

            selectedProfile = selectedProfile == -1 ? 0 : selectedProfile;
            AudioProfileSelect.SelectedIndex = selectedProfile > AudioProfileSelect.Items.Count ? 0 : selectedProfile;
        }

        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            
            if (SetDebug.IsChecked.GetValueOrDefault() != AppSettings.UseDebug)
            {
                AppSettings.UseDebug = SetDebug.IsChecked.GetValueOrDefault();
                App.ReconfigureLogger();
            }

            if (UseHardwareRendering.IsChecked.GetValueOrDefault() != AppSettings.UseHardwareRendering)
            {
                AppSettings.UseHardwareRendering = UseHardwareRendering.IsChecked.GetValueOrDefault();
                App.ReconfigureRenderMode();
            }
            
            
            ExplicitSettingsUpdate();
            Log.Error("saved");
            
            DialogResult = true;
        }

        private void ClearLogFileClick(object sender, RoutedEventArgs e)
        {
            App.ClearLogFile();
        }

        private void CreateReportFileClick(object sender, RoutedEventArgs e)
        {
            string logFile = Path.Combine(AppSettings.AppSettingsPath, "ErrorLog.txt");
            string targetFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                                             "VideoConvert ErrorLog.txt");

            using (FileStream reader = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                              writer = new FileStream(targetFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                reader.CopyTo(writer);
            }
        }

        private void ExplicitSettingsUpdate()
        {
            BindingExpression be;

            Properties.Settings.Default.FirstStart = false;
            // update settings of all RadioButtons
            IEnumerable<RadioButton> radioButtons = GetAllComponentsOfType<RadioButton>(this);
            foreach (RadioButton button in radioButtons)
            {
                be = button.GetBindingExpression(ToggleButton.IsCheckedProperty);
                if (be != null)
                    be.UpdateSource();
            }

            // update settings of all checkboxes
            IEnumerable<CheckBox> checkBoxes = GetAllComponentsOfType<CheckBox>(this);
            foreach (CheckBox box in checkBoxes)
            {
                be = box.GetBindingExpression(ToggleButton.IsCheckedProperty);
                if (be != null)
                    be.UpdateSource();
            }

            // update settings of all textboxes
            IEnumerable<TextBox> textBoxes = GetAllComponentsOfType<TextBox>(this);
            foreach (TextBox box in textBoxes)
            {
                be = box.GetBindingExpression(TextBox.TextProperty);
                if (be != null)
                    be.UpdateSource();
            }

            // update settings of allt integerupdown boxes
            IEnumerable<IntegerUpDown> intBoxes = GetAllComponentsOfType<IntegerUpDown>(MainGrid);
            foreach (IntegerUpDown box in intBoxes)
            {
                be = box.GetBindingExpression(IntegerUpDown.ValueProperty);
                if (be != null)
                    be.UpdateSource();
            }

            // update settings of all doubleupdown boxes
            IEnumerable<DoubleUpDown> doubleBoxes = GetAllComponentsOfType<DoubleUpDown>(MainGrid);
            foreach (DoubleUpDown box in doubleBoxes)
            {
                be = box.GetBindingExpression(DoubleUpDown.ValueProperty);
                if (be != null)
                    be.UpdateSource();
            }

            // update settings of all sliders
            IEnumerable<Slider> sliders = GetAllComponentsOfType<Slider>(MainGrid);
            foreach (Slider box in sliders)
            {
                be = box.GetBindingExpression(RangeBase.ValueProperty);
                if (be != null)
                    be.UpdateSource();
            }

            AppSettings.TSMuxeRSubtitleFont = (FontFamily)tsMuxeRFontSwitcher.SelectedItem;
            AppSettings.TSMuxeRSubtitleColor = tsMuxeRFontColorPicker.SelectedColor;
            AppSettings.UseLanguage = LanguageSelect.SelectedValue as string;
            AppSettings.ProcessPriority = ProcessPriority.SelectedIndex;
            AppSettings.UpdateFrequency = AutoUpdateFrequency.SelectedIndex;
        }

        /// <summary>
        /// Returns a list of requested controls from a container (including sub-containers)
        /// </summary>
        /// <typeparam name="T">Requested Type</typeparam>
        /// <param name="inElement">Container</param>
        /// <returns>List of Controls with Requested Type</returns>
        private static IEnumerable<T> GetAllComponentsOfType<T>(FrameworkElement inElement)
        {
            IEnumerable<T> result = LogicalTreeHelper.GetChildren(inElement).OfType<T>();

            IEnumerable<FrameworkElement> elements = LogicalTreeHelper.GetChildren(inElement).OfType<FrameworkElement>();
            return elements.Where(element =>
                                  (element.GetType() == typeof (Grid))
                                  || (element.GetType() == typeof (TabControl))
                                  || (element.GetType() == typeof (TabItem))
                                  || (element.GetType() == typeof (GroupBox)))
                .Aggregate(result, (current, element) => current.Concat(GetAllComponentsOfType<T>(element)));
        }

        private static string GetFolder()
        {
            string result = string.Empty;

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                result = folderBrowser.SelectedPath;

            return result;
        }

        private static string GetFilePath(string file = "")
        {
            string result = string.Empty;

            OpenFileDialog fileDialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(file))
            {
                fileDialog.Filter = string.Format("{0}|{0}", file);
            }
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                result = fileDialog.FileName;

            return result;
        }

        private void SelectDemuxLocationClick(object sender, RoutedEventArgs e)
        {
            string folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
            {
                DemuxLocation.Text = folder;
            }
        }

        private void BrowseOutputLocationClick(object sender, RoutedEventArgs e)
        {
            string folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
            {
                OutputLocation.Text = folder;
            }
        }

        private void SelectEncoderPathClick(object sender, RoutedEventArgs e)
        {
            string folder = GetFolder();
            if (!string.IsNullOrEmpty(folder))
            {
                EncoderPath.Text = folder;
            }
        }

        private void ReloadEncodersClick(object sender, RoutedEventArgs e)
        {
            _encPath = EncoderPath.Text;
            _javaPath = JavaPath.Text;

            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += BwDoWork;
            bw.RunWorkerCompleted += BwRunWorkerCompleted;
            bw.RunWorkerAsync();
            ReloadEncoders.IsEnabled = false;
            AppVersionsExpander.IsExpanded = true;
            
        }

        void BwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ReloadEncoders.IsEnabled = true;
            _encPath = string.Empty;
        }

        void BwDoWork(object sender, DoWorkEventArgs e)
        {
            if (!string.IsNullOrEmpty(_encPath))
            {
                Processing.GetAppVersions(_encPath, _javaPath);
            }
        }

        private void ProfilesSelectionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedQuickProfile = ProfilesSelection.SelectedItem as QuickSelectProfile;
            LoadQuickProfile();
        }

        private void LoadQuickProfile()
        {
            if (_selectedQuickProfile == null) return;
            QuickSelectOutputFormat.SelectedIndex = (int) _selectedQuickProfile.OutFormat;

            QuickProfileVideoSelect.SelectedIndex = -1;     // force updating profile type
            QuickProfileVideoSelect.SelectedValue = _selectedQuickProfile.VideoProfile;
            
            QuickProfileAudioSelect.SelectedIndex = -1;     // force updating profile type
            QuickProfileAudioSelect.SelectedValue = _selectedQuickProfile.AudioProfile;
            
            QuickSelectSystemType.SelectedIndex = _selectedQuickProfile.SystemType;
            QuickSelectOutputResolution.Value = _selectedQuickProfile.TargetWidth;
            QuickSelectKeepInputResolution.IsChecked = _selectedQuickProfile.KeepInputResolution;

            QuickSelectDeinterlaceVideo.IsChecked = _selectedQuickProfile.Deinterlace;
            QuickSelectAutoCropVideo.IsChecked = _selectedQuickProfile.AutoCropResize;
                
            TargetSize tSize = _sizeList.Find(ts => ts.Size == _selectedQuickProfile.TargetFileSize);

            int sizeID = tSize != null ? tSize.ID : 12;

            QuickSelectTargetSize.SelectedIndex = sizeID;
            if (sizeID == 12)
            {
                SetCustomSize(_selectedQuickProfile.TargetFileSize);
            }

            QuickSelectStereoscopyType.SelectedIndex = (int) _selectedQuickProfile.StereoType;
        }

        private void UpdateQuickProfileButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedQuickProfile == null) return;

            _selectedQuickProfile.AudioProfile = ((EncoderProfile) QuickProfileAudioSelect.SelectedItem).Name;
            _selectedQuickProfile.AudioProfileType = ((EncoderProfile) QuickProfileAudioSelect.SelectedItem).Type;
            _selectedQuickProfile.VideoProfile = ((EncoderProfile) QuickProfileVideoSelect.SelectedItem).Name;
            _selectedQuickProfile.VideoProfileType = ((EncoderProfile) QuickProfileVideoSelect.SelectedItem).Type;

            _selectedQuickProfile.AutoCropResize = QuickSelectAutoCropVideo.IsChecked.GetValueOrDefault();
            _selectedQuickProfile.Deinterlace = QuickSelectDeinterlaceVideo.IsChecked.GetValueOrDefault();
            _selectedQuickProfile.KeepInputResolution = QuickSelectKeepInputResolution.IsChecked.GetValueOrDefault();
            _selectedQuickProfile.OutFormat = (OutputType)QuickSelectOutputFormat.SelectedIndex;
            _selectedQuickProfile.StereoType = (StereoEncoding) QuickSelectStereoscopyType.SelectedIndex;
            _selectedQuickProfile.SystemType = QuickSelectSystemType.SelectedIndex;

            if (QuickSelectTargetSize.SelectedIndex < 12)
                _selectedQuickProfile.TargetFileSize = ((TargetSize)QuickSelectTargetSize.SelectedItem).Size;
            else
            {
                SizeModificator mod = (SizeModificator) QuickSelectTargetSizeMod.SelectedItem;
                double targetSize = QuickSelectTargetSizeValue.Value.GetValueOrDefault();
                targetSize = targetSize*mod.Mod;
                _selectedQuickProfile.TargetFileSize = (ulong) Math.Floor(targetSize);
            }

            _selectedQuickProfile.TargetWidth = QuickSelectOutputResolution.Value.GetValueOrDefault();
            _profiles.TriggerUpdate();

        }

        private void NewQuickProfileButtonClick(object sender, RoutedEventArgs e)
        {
            string newProfile = InputBox.Show(_inputPrompt);
            if (String.IsNullOrEmpty(newProfile)) return;

            QuickSelectProfile profile = new QuickSelectProfile { Name = newProfile };

            if (_profiles.AddProfile(profile))
                ReloadQuickProfileList();
        }

        private void DeleteQuickProfileButtonClick(object sender, RoutedEventArgs e)
        {
            QuickSelectProfile selectedProfile = ProfilesSelection.SelectedItem as QuickSelectProfile;
            if (selectedProfile == null) return;

            if (_profiles.RemoveProfile(selectedProfile))
                ReloadQuickProfileList();
        }

        private void VideoEncoderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)  // prevent throwing an exception if this event is fired upon window creation
                ReloadVideoProfiles();
        }

        private void VideoProfileSelectSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (VideoEncoder.SelectedIndex)
            {
                case 1:
                    Loadx264Profile();
                    break;
                case 3:
                    LoadHcEncProfile();
                    break;
                case 4:
                    LoadVP8Profile();
                    break;
            }
        }

        private void LoadVP8Profile()
        {
            _selectedVideoProfile = VideoProfileSelect.SelectedItem as VP8Profile;
            if (_selectedVideoProfile == null) return;

            VP8EncodingMode.SelectedIndex = ((VP8Profile) _selectedVideoProfile).EncodingMode;
            VP8BitrateValue.Value = ((VP8Profile) _selectedVideoProfile).Bitrate;
            VP8BitrateMode.SelectedIndex = ((VP8Profile) _selectedVideoProfile).BitrateMode;

            VP8ProfileVal.SelectedIndex = ((VP8Profile) _selectedVideoProfile).Profile;
            VP8BasicSpeedControl.SelectedIndex = ((VP8Profile) _selectedVideoProfile).SpeedControl;
            VP8CPUUtilizationModifier.Value = ((VP8Profile) _selectedVideoProfile).CPUModifier;
            VP8DeadlinePerFrameValue.Value = ((VP8Profile) _selectedVideoProfile).DeadlinePerFrame;
            VP8TokenPartitioning.SelectedIndex = ((VP8Profile) _selectedVideoProfile).TokenPart;

            VP8NoiseFiltering.Value = ((VP8Profile) _selectedVideoProfile).NoiseFiltering;
            VP8Sharpness.Value = ((VP8Profile) _selectedVideoProfile).Sharpness;
            VP8Threads.Value = ((VP8Profile) _selectedVideoProfile).Threads;
            VP8StaticThreshold.Value = ((VP8Profile) _selectedVideoProfile).StaticThreshold;
            VP8UseErrorResilience.IsChecked = ((VP8Profile) _selectedVideoProfile).UseErrorResilience;

            VP8ArnrMaxFrameCount.Value = ((VP8Profile) _selectedVideoProfile).ArnrMaxFrames;
            VP8ArnrMaxStrength.Value = ((VP8Profile) _selectedVideoProfile).ArnrStrength;

            VP8GopSizeMax.Value = ((VP8Profile) _selectedVideoProfile).GopMax;
            VP8GopSizeMin.Value = ((VP8Profile) _selectedVideoProfile).GopMin;
            VP8MaxFramesLag.Value = ((VP8Profile) _selectedVideoProfile).MaxFramesLag;
            VP8FrameDrop.Value = ((VP8Profile) _selectedVideoProfile).FrameDrop;
            VP8UseSpatialResampling.IsChecked = ((VP8Profile) _selectedVideoProfile).UseSpatialResampling;
            VP8DownScalingThreshold.Value = ((VP8Profile) _selectedVideoProfile).DownscaleThreshold;
            VP8UpScalingThreshold.Value = ((VP8Profile) _selectedVideoProfile).UpscaleThreshold;
            VP8UseArnr.IsChecked = ((VP8Profile) _selectedVideoProfile).UseArnrFrameDecision;

            VP8InitialBufferSize.Value = ((VP8Profile) _selectedVideoProfile).InitialBufferSize;
            VP8OptimalBufferSize.Value = ((VP8Profile) _selectedVideoProfile).OptimalBufferSize;
            VP8BufferSize.Value = ((VP8Profile) _selectedVideoProfile).BufferSize;
            VP8UndershootDatarate.Value = ((VP8Profile) _selectedVideoProfile).UndershootDataRate;

            VP8QuantizerMin.Value = ((VP8Profile) _selectedVideoProfile).QuantizerMin;
            VP8QuantizerMax.Value = ((VP8Profile) _selectedVideoProfile).QuantizerMax;
            VP8FrameAdjust.Value = ((VP8Profile) _selectedVideoProfile).BiasFrameAdjust;
            VP8SectionMin.Value = ((VP8Profile) _selectedVideoProfile).SectionMin;
            VP8SectionMax.Value = ((VP8Profile) _selectedVideoProfile).SectionMax;
        }

        private void Loadx264Profile()
        {
            _selectedVideoProfile = VideoProfileSelect.SelectedItem as X264Profile;
            if (_selectedVideoProfile == null) return;

            X264EncodingMode.SelectedIndex = ((X264Profile)_selectedVideoProfile).EncodingMode;

            X264Quantizer.Value = ((X264Profile)_selectedVideoProfile).QuantizerSetting;
            X264Quality.Value = ((X264Profile)_selectedVideoProfile).QualitySetting;

            X264VBRSetting.Value = ((X264Profile)_selectedVideoProfile).VBRSetting;
            X264Tuning.SelectedIndex = ((X264Profile)_selectedVideoProfile).Tuning;
            X264AVCProfile.SelectedIndex = ((X264Profile)_selectedVideoProfile).AVCProfile;
            X264AVCPreset.SelectedIndex = ((X264Profile)_selectedVideoProfile).Preset;
            X264AVCLevel.SelectedIndex = ((X264Profile)_selectedVideoProfile).AVCLevel;
            X264TuneDevice.SelectedIndex = ((X264Profile)_selectedVideoProfile).TuneDevice;
        }

        private void NewVideoProfileButtonClick(object sender, RoutedEventArgs e)
        {
            string defaultResponse = GetDefaultResponseVideo();
            string newProfile = InputBox.Show(_inputPrompt, defaultResponse);
            if (String.IsNullOrEmpty(newProfile) || newProfile.Equals(defaultResponse)) return;

            EncoderProfile profile = new EncoderProfile();
            switch (VideoEncoder.SelectedIndex)
            {
                case 1:
                    profile = new X264Profile();
                    break;
                case 3:
                    profile = new HcEncProfile();
                    break;
                case 4:
                    profile = new VP8Profile();
                    break;
            }

            if (profile.Type == ProfileType.None) return;

            profile.Name = newProfile;

            if (_profiles.AddProfile(profile))
                ReloadVideoProfiles();
        }

        private string GetDefaultResponseVideo()
        {
            switch (VideoEncoder.SelectedIndex)
            {
                case 1:
                    return "x264: ";
                case 2:
                    return "Xvid: ";
                case 3:
                    return "MPEG-2: ";
                case 4:
                    return "VP8: ";
                default:
                    return string.Empty;
            }
        }

        private void UpdateVideoProfileButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedVideoProfile == null) return;

            switch (VideoEncoder.SelectedIndex)
            {
                case 3:
                    ((HcEncProfile) _selectedVideoProfile).Bitrate = HcEncBitrate.Value.GetValueOrDefault();
                    ((HcEncProfile) _selectedVideoProfile).Profile = HcEncProfile.SelectedIndex;
                    ((HcEncProfile) _selectedVideoProfile).DCPrecision = HcEncDCPrecision.SelectedIndex;
                    ((HcEncProfile) _selectedVideoProfile).Interlacing = HcEncInterlacing.SelectedIndex;
                    ((HcEncProfile) _selectedVideoProfile).FieldOrder = HcEncInterlaceFieldOrder.SelectedIndex;
                    ((HcEncProfile) _selectedVideoProfile).ChromaDownsampling = HcEncChromaDownsampling.SelectedIndex;
                    ((HcEncProfile)_selectedVideoProfile).ClosedGops = HcEncClosedGOP.IsChecked.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).SceneChange = HcEncSceneChange.IsChecked.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).AutoGOP = HcEncAutoGOP.IsChecked.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).GopLength = HcEncGopLength.Value.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).BFrames = HcEncBFrames.Value.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).LuminanceGain = HcEncLumGain.Value.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).AQ = HcEncAQ.Value.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).SMP = HcEncSMP.IsChecked.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).VBVCheck = hcEncVBVCheck.IsChecked.GetValueOrDefault();
                    ((HcEncProfile) _selectedVideoProfile).Matrix = hcEncBuiltinMatrix.SelectedIndex;
                    ((HcEncProfile) _selectedVideoProfile).IntraVLC = hcEncIntraVLC.SelectedIndex;
                    ((HcEncProfile) _selectedVideoProfile).Colorimetry = hcEncColorimetry.SelectedIndex;
                    ((HcEncProfile) _selectedVideoProfile).MPGLevel = hcEncMPGLevel.SelectedIndex;
                    ((HcEncProfile)_selectedVideoProfile).VBRBias = hcEncVBRBias.Value.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).LastIFrame = hcEncLastIFrame.IsChecked.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).SeqEndCode = hcEncSeqEndCode.IsChecked.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).Allow3BFrames = hcEncAllow3Frames.IsChecked.GetValueOrDefault();
                    ((HcEncProfile)_selectedVideoProfile).UseLosslessFile = hcEncUseLosslessFile.IsChecked.GetValueOrDefault();
                    break;
                case 4:
                    ((VP8Profile) _selectedVideoProfile).EncodingMode = VP8EncodingMode.SelectedIndex;
                    ((VP8Profile) _selectedVideoProfile).Bitrate = VP8BitrateValue.Value.GetValueOrDefault();
                    ((VP8Profile) _selectedVideoProfile).BitrateMode = VP8BitrateMode.SelectedIndex;

                    ((VP8Profile) _selectedVideoProfile).Profile = VP8ProfileVal.SelectedIndex;
                    ((VP8Profile) _selectedVideoProfile).SpeedControl = VP8BasicSpeedControl.SelectedIndex;
                    ((VP8Profile) _selectedVideoProfile).CPUModifier = VP8CPUUtilizationModifier.Value.GetValueOrDefault();
                    ((VP8Profile) _selectedVideoProfile).DeadlinePerFrame = VP8DeadlinePerFrameValue.Value.GetValueOrDefault();
                    ((VP8Profile) _selectedVideoProfile).TokenPart = VP8TokenPartitioning.SelectedIndex;

                    ((VP8Profile)_selectedVideoProfile).NoiseFiltering = VP8NoiseFiltering.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).Sharpness = VP8Sharpness.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).Threads = VP8Threads.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).StaticThreshold = VP8StaticThreshold.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).UseErrorResilience = VP8UseErrorResilience.IsChecked.GetValueOrDefault();

                    ((VP8Profile)_selectedVideoProfile).ArnrMaxFrames = VP8ArnrMaxFrameCount.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).ArnrStrength = VP8ArnrMaxStrength.Value.GetValueOrDefault();

                    ((VP8Profile)_selectedVideoProfile).GopMax = VP8GopSizeMax.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).GopMin = VP8GopSizeMin.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).MaxFramesLag = VP8MaxFramesLag.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).FrameDrop = VP8FrameDrop.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).UseSpatialResampling = VP8UseSpatialResampling.IsChecked.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).DownscaleThreshold = VP8DownScalingThreshold.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).UpscaleThreshold = VP8UpScalingThreshold.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).UseArnrFrameDecision = VP8UseArnr.IsChecked.GetValueOrDefault();

                    ((VP8Profile)_selectedVideoProfile).InitialBufferSize = VP8InitialBufferSize.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).OptimalBufferSize = VP8OptimalBufferSize.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).BufferSize = VP8BufferSize.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).UndershootDataRate = VP8UndershootDatarate.Value.GetValueOrDefault();

                    ((VP8Profile)_selectedVideoProfile).QuantizerMin = VP8QuantizerMin.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).QuantizerMax = VP8QuantizerMax.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).BiasFrameAdjust = VP8FrameAdjust.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).SectionMin = VP8SectionMin.Value.GetValueOrDefault();
                    ((VP8Profile)_selectedVideoProfile).SectionMax = VP8SectionMax.Value.GetValueOrDefault();
                    break;
            }

            _profiles.TriggerUpdate();
            ReloadVideoProfiles();
        }

        private void DeleteVideoProfileButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedVideoProfile == null) return;

            if (_profiles.RemoveProfile(_selectedVideoProfile))
                ReloadVideoProfiles();
        }

        #region x264 Profiles

        private void X264AdvancedSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            X264AdvancedSettings advancedSettings = new X264AdvancedSettings { Owner = this };
            advancedSettings.SetProfile((X264Profile)_selectedVideoProfile);
            if (advancedSettings.ShowDialog() == true)
                _selectedVideoProfile = advancedSettings.Profile;
        }

        private void X264TuningSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedVideoProfile == null) return;

            ((X264Profile)_selectedVideoProfile).Tuning = X264Tuning.SelectedIndex;
        }

        private void X264TuneDeviceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedVideoProfile == null) return;

            _actualDevice = (X264Device)X264TuneDevice.SelectedItem;

            ((X264Profile)_selectedVideoProfile).TuneDevice = X264TuneDevice.SelectedIndex;
            if (_actualDevice.BFrames > -1 && ((X264Profile)_selectedVideoProfile).NumBFrames > _actualDevice.BFrames)
                ((X264Profile) _selectedVideoProfile).NumBFrames = _actualDevice.BFrames;

            ((X264Profile)_selectedVideoProfile).UseBluRayCompatibility = _actualDevice.BluRay;

            if (_actualDevice.BluRay && ((X264Profile)_selectedVideoProfile).BPyramid > 1)
                ((X264Profile)_selectedVideoProfile).BPyramid = 1;

            if (_actualDevice.BPyramid > -1 && ((X264Profile)_selectedVideoProfile).BPyramid != _actualDevice.BPyramid)
                ((X264Profile)_selectedVideoProfile).BPyramid = _actualDevice.BPyramid;

            if (_actualDevice.MaxGOP > -1 && ((X264Profile)_selectedVideoProfile).MaxGopSize > _actualDevice.MaxGOP)
                ((X264Profile)_selectedVideoProfile).MaxGopSize = _actualDevice.MaxGOP;

            if (_actualDevice.Level > -1 && ((X264Profile)_selectedVideoProfile).AVCLevel > _actualDevice.Level)
            {
                ((X264Profile)_selectedVideoProfile).AVCLevel = _actualDevice.Level;
                X264AVCLevel.SelectedIndex = _actualDevice.Level;
            }

            if (_actualDevice.Profile > -1 && ((X264Profile)_selectedVideoProfile).AVCProfile > _actualDevice.Profile)
            {
                ((X264Profile)_selectedVideoProfile).AVCProfile = _actualDevice.Profile;
                X264AVCProfile.SelectedIndex = _actualDevice.Profile;
            }

            if (_actualDevice.ReferenceFrames > -1 && ((X264Profile)_selectedVideoProfile).NumRefFrames > _actualDevice.ReferenceFrames)
                ((X264Profile)_selectedVideoProfile).NumRefFrames = _actualDevice.ReferenceFrames;

            int tempref = X264Settings.GetMaxRefForLevel(((X264Profile)_selectedVideoProfile).AVCLevel, 1920, 1080);
            if (tempref > 0 && tempref < ((X264Profile)_selectedVideoProfile).NumRefFrames)
                ((X264Profile)_selectedVideoProfile).NumRefFrames = tempref;

            if (_actualDevice.VBVBufsize > -1 &&
                (((X264Profile)_selectedVideoProfile).VBVBufSize > _actualDevice.VBVBufsize ||
                 ((X264Profile)_selectedVideoProfile).VBVBufSize == 0))
            {
                ((X264Profile) _selectedVideoProfile).VBVBufSize = _actualDevice.VBVBufsize;
            }

            if (_actualDevice.VBVMaxrate > -1 &&
                (((X264Profile)_selectedVideoProfile).VBVMaxRate > _actualDevice.VBVMaxrate ||
                 ((X264Profile)_selectedVideoProfile).VBVMaxRate == 0))
            {
                ((X264Profile) _selectedVideoProfile).VBVMaxRate = _actualDevice.VBVMaxrate;
            }
            if ((_actualDevice.ID == 3) && (((X264Profile)_selectedVideoProfile).NumSlices != 4))
                ((X264Profile)_selectedVideoProfile).NumSlices = 4;

            if (_actualDevice.BluRay && ((X264Profile)_selectedVideoProfile).UseAccessUnitDelimiters == false)
                ((X264Profile)_selectedVideoProfile).UseAccessUnitDelimiters = true;

            if (_actualDevice.BluRay && ((X264Profile)_selectedVideoProfile).UseBluRayCompatibility == false)
                ((X264Profile)_selectedVideoProfile).UseBluRayCompatibility = true;

            if (_actualDevice.BluRay && ((X264Profile)_selectedVideoProfile).PFrameWeightedPrediction > 1)
                ((X264Profile)_selectedVideoProfile).PFrameWeightedPrediction = 1;

            if (_actualDevice.BluRay && ((X264Profile)_selectedVideoProfile).HRDInfo < 1)
                ((X264Profile)_selectedVideoProfile).HRDInfo = 1;
        }

        private void X264AvcProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedVideoProfile == null) return;

            if (_actualDevice != null)
            {
                if (_actualDevice.Profile > -1 && X264AVCProfile.SelectedIndex <= _actualDevice.Profile)
                {
                    ((X264Profile)_selectedVideoProfile).AVCProfile = X264AVCProfile.SelectedIndex;
                }
                else
                {
                    ((X264Profile)_selectedVideoProfile).AVCProfile = X264AVCProfile.SelectedIndex;
                }
            }
            else
            {
                ((X264Profile)_selectedVideoProfile).AVCProfile = X264AVCProfile.SelectedIndex;
            }
            switch (((X264Profile) _selectedVideoProfile).AVCProfile)
            {
                case 0:
                    ((X264Profile) _selectedVideoProfile).MacroBlocksPartitionsAdaptiveDCT = false;
                    ((X264Profile) _selectedVideoProfile).NumBFrames = 0;
                    ((X264Profile) _selectedVideoProfile).UseCabac = false;
                    ((X264Profile) _selectedVideoProfile).QuantizerMatrix = 0;
                    ((X264Profile) _selectedVideoProfile).PFrameWeightedPrediction = 0;
                    ((X264Profile) _selectedVideoProfile).InterlaceMode = 1;
                    break;
                case 1:
                    ((X264Profile) _selectedVideoProfile).MacroBlocksPartitionsAdaptiveDCT = false;
                    ((X264Profile) _selectedVideoProfile).QuantizerMatrix = 0;
                    break;
            }
        }

        private void X264AvcLevelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedVideoProfile == null) return;

            if (_actualDevice != null)
            {
                if (_actualDevice.Level > -1 && X264AVCLevel.SelectedIndex <= _actualDevice.Level)
                {
                    ((X264Profile)_selectedVideoProfile).AVCLevel = X264AVCLevel.SelectedIndex;
                }
                else
                    X264AVCLevel.SelectedIndex = _actualDevice.Level;
            }
            else
            {
                ((X264Profile)_selectedVideoProfile).AVCLevel = X264AVCLevel.SelectedIndex;
            }
        }

        private void X264AvcPresetSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedVideoProfile == null) return;

            ((X264Profile)_selectedVideoProfile).Preset = X264AVCPreset.SelectedIndex;
        }

        private void X264EncodingModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                X264VBREncodingDescLabel.Visibility = X264EncodingMode.SelectedIndex == 1 ||
                                                      X264EncodingMode.SelectedIndex == 4
                                                          ? Visibility.Collapsed
                                                          : Visibility.Visible;
                X264QuantizerLabel.Visibility = X264EncodingMode.SelectedIndex == 1
                                                    ? Visibility.Visible
                                                    : Visibility.Collapsed;
                X264QualityLabel.Visibility = X264EncodingMode.SelectedIndex == 4
                                                  ? Visibility.Visible
                                                  : Visibility.Collapsed;
            }
            else
            {
                X264VBREncodingDescLabel.Visibility = Visibility.Collapsed;
                X264QuantizerLabel.Visibility = Visibility.Collapsed;
                X264QualityLabel.Visibility = Visibility.Collapsed;
            }

            if (_selectedVideoProfile == null) return;

            ((X264Profile)_selectedVideoProfile).EncodingMode = X264EncodingMode.SelectedIndex;
        }

        private void X264QualityValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_selectedVideoProfile == null) return;

            string source = ((IntegerUpDown)sender).Name;
            if (String.CompareOrdinal(source, "x264Quantizer") == 0)
                ((X264Profile)_selectedVideoProfile).QuantizerSetting = X264Quantizer.Value.GetValueOrDefault();
            else if (String.CompareOrdinal(source, "x264Quality") == 0)
                ((X264Profile)_selectedVideoProfile).QualitySetting = X264Quality.Value.GetValueOrDefault();
            else if (String.CompareOrdinal(source, "x264VBRSetting") == 0)
                ((X264Profile)_selectedVideoProfile).VBRSetting = X264VBRSetting.Value.GetValueOrDefault();
        }
        #endregion

        private void AudioEncoderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                ReloadAudioProfiles();
        }

        private void AudioProfileSelectSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (AudioEncoder.SelectedIndex)
            {
                case 1:
                    LoadAC3Profile();
                    break;
                case 2:
                    LoadOggProfile();
                    break;
                case 3:
                    LoadMP3Profile();
                    break;
                case 4:
                    LoadAACProfile();
                    break;
            }
        }

        private void LoadAC3Profile()
        {
            _selectedAudioProfile = (AC3Profile)AudioProfileSelect.SelectedItem;

            if (_selectedAudioProfile == null) return;

            AC3ApplyDynamicRange.IsChecked = ((AC3Profile)_selectedAudioProfile).ApplyDynamicRangeCompression;

            AC3OutputChannels.SelectedIndex = ((AC3Profile)_selectedAudioProfile).OutputChannels;
            AC3SampleRate.SelectedIndex = ((AC3Profile)_selectedAudioProfile).SampleRate;

            AC3Bitrate.SelectedIndex = ((AC3Profile)_selectedAudioProfile).Bitrate;
        }

        private void LoadOggProfile()
        {
            _selectedAudioProfile = (OggProfile)AudioProfileSelect.SelectedItem;

            if (_selectedAudioProfile == null) return;

            OggOutputChannels.SelectedIndex = ((OggProfile)_selectedAudioProfile).OutputChannels;
            OggSampleRate.SelectedIndex = ((OggProfile)_selectedAudioProfile).SampleRate;

            OggEncodingBitrate.Value = ((OggProfile)_selectedAudioProfile).Bitrate;
            OggEncodingQuality.Value = (decimal?) ((OggProfile) _selectedAudioProfile).Quality;
            OggEncodingMode.SelectedIndex = ((OggProfile) _selectedAudioProfile).EncodingMode;
        }

        private void LoadMP3Profile()
        {
            _selectedAudioProfile = (MP3Profile)AudioProfileSelect.SelectedItem;

            if (_selectedAudioProfile == null) return;

            MP3OutputChannels.SelectedIndex = ((MP3Profile)_selectedAudioProfile).OutputChannels;
            MP3SampleRate.SelectedIndex = ((MP3Profile)_selectedAudioProfile).SampleRate;

            MP3EncodingBitrate.Value = ((MP3Profile)_selectedAudioProfile).Bitrate;
            MP3EncodingQuality.Value = ((MP3Profile)_selectedAudioProfile).Quality;
            MP3EncodingMode.SelectedIndex = ((MP3Profile)_selectedAudioProfile).EncodingMode;
            MP3EncodingPreset.SelectedValue = ((MP3Profile) _selectedAudioProfile).Preset;
        }

        private void LoadAACProfile()
        {
            _selectedAudioProfile = (AACProfile)AudioProfileSelect.SelectedItem;

            if (_selectedAudioProfile == null) return;

            AACOutputChannels.SelectedIndex = ((AACProfile)_selectedAudioProfile).OutputChannels;
            AACSampleRate.SelectedIndex = ((AACProfile)_selectedAudioProfile).SampleRate;

            AACEncodingBitrate.Value = ((AACProfile)_selectedAudioProfile).Bitrate;
            AACEncodingQuality.Value = (decimal?)((AACProfile)_selectedAudioProfile).Quality;
            AACEncodingMode.SelectedIndex = ((AACProfile)_selectedAudioProfile).EncodingMode;
        }

        private string GetDefaultResponseAudio()
        {
            switch (AudioEncoder.SelectedIndex)
            {
                case 1:
                    return "AC3: ";
                case 2:
                    return "OGG: ";
                case 3:
                    return "MP3: ";
                case 4:
                    return "AAC: ";
                default:
                    return string.Empty;
            }
        }

        private void NewAudioProfileButtonClick(object sender, RoutedEventArgs e)
        {
            string defaultResponse = GetDefaultResponseAudio();
            string newProfile = InputBox.Show(_inputPrompt, defaultResponse);

            if (String.IsNullOrEmpty(newProfile) || newProfile.Equals(defaultResponse)) return;

            EncoderProfile profile = new EncoderProfile();
            switch (AudioEncoder.SelectedIndex)
            {
                case 1:
                    profile = new AC3Profile();
                    break;
                case 2:
                    profile = new OggProfile();
                    break;
                case 3:
                    profile = new MP3Profile();
                    break;
                case 4:
                    profile = new AACProfile();
                    break;
            }

            if (profile.Type == ProfileType.None) return;

            profile.Name = newProfile;

            if (_profiles.AddProfile(profile))
                ReloadAudioProfiles();
        }

        private void UpdateAudioProfileButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedAudioProfile == null) return;

            switch (AudioEncoder.SelectedIndex)
            {
                case 1:
                    {
                        ((AC3Profile) _selectedAudioProfile).ApplyDynamicRangeCompression =
                            AC3ApplyDynamicRange.IsChecked.GetValueOrDefault();

                        ((AC3Profile) _selectedAudioProfile).OutputChannels = AC3OutputChannels.SelectedIndex;
                        ((AC3Profile) _selectedAudioProfile).SampleRate = AC3SampleRate.SelectedIndex;

                        ((AC3Profile) _selectedAudioProfile).Bitrate = AC3Bitrate.SelectedIndex;
                    }
                    break;
                case 2:
                    {
                        ((OggProfile) _selectedAudioProfile).OutputChannels = OggOutputChannels.SelectedIndex;
                        ((OggProfile) _selectedAudioProfile).SampleRate = OggSampleRate.SelectedIndex;

                        ((OggProfile) _selectedAudioProfile).Bitrate = OggEncodingBitrate.Value.GetValueOrDefault();
                        ((OggProfile) _selectedAudioProfile).Quality =
                            (float) OggEncodingQuality.Value.GetValueOrDefault();
                        ((OggProfile) _selectedAudioProfile).EncodingMode = OggEncodingMode.SelectedIndex;
                    }
                    break;
                case 3:
                    {
                        ((MP3Profile) _selectedAudioProfile).OutputChannels = MP3OutputChannels.SelectedIndex;
                        ((MP3Profile) _selectedAudioProfile).SampleRate = MP3SampleRate.SelectedIndex;

                        ((MP3Profile) _selectedAudioProfile).Bitrate = MP3EncodingBitrate.Value.GetValueOrDefault();
                        ((MP3Profile) _selectedAudioProfile).Quality = MP3EncodingQuality.Value.GetValueOrDefault();
                        ((MP3Profile) _selectedAudioProfile).EncodingMode = MP3EncodingMode.SelectedIndex;
                        ((MP3Profile) _selectedAudioProfile).Preset = MP3EncodingPreset.SelectedValue as string;
                    }
                    break;
                case 4:
                    {
                        ((AACProfile) _selectedAudioProfile).OutputChannels = AACOutputChannels.SelectedIndex;
                        ((AACProfile) _selectedAudioProfile).SampleRate = AACSampleRate.SelectedIndex;

                        ((AACProfile) _selectedAudioProfile).Bitrate = AACEncodingBitrate.Value.GetValueOrDefault();
                        ((AACProfile) _selectedAudioProfile).Quality =
                            (float) AACEncodingQuality.Value.GetValueOrDefault();
                        ((AACProfile) _selectedAudioProfile).EncodingMode = AACEncodingMode.SelectedIndex;
                    }
                    break;
            }

            _profiles.TriggerUpdate();
            ReloadAudioProfiles();
        }

        private void DeleteAudioProfileButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedAudioProfile == null) return;

            if (_profiles.RemoveProfile(_selectedAudioProfile))
                ReloadAudioProfiles();
        }

        private void QuickSelectOutputFormatSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadAudioProfiles();
            ReloadVideoProfiles();
        }

        private void QuickSelectKeepInputResolutionChecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            if (QuickSelectKeepInputResolution.IsChecked.GetValueOrDefault())
            {
                QuickSelectAutoCropVideo.IsChecked = false;
                QuickSelectAutoCropVideo.IsEnabled = false;
                QuickSelectOutputResolution.IsEnabled = false;
                QuickSelectOutputResolutionLabel.IsEnabled = false;
            }
            else
            {
                QuickSelectAutoCropVideo.IsEnabled = true;
                QuickSelectOutputResolution.IsEnabled = true;
                QuickSelectOutputResolutionLabel.IsEnabled = true;
            }
        }

        private void QuickSelectTargetSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            QuickSelectTargetSizeValue.IsEnabled = QuickSelectTargetSize.SelectedIndex == 12;

            if (_selectedQuickProfile == null) return;

            if (QuickSelectTargetSize.SelectedIndex < 12)
                _selectedQuickProfile.TargetFileSize = ((TargetSize)QuickSelectTargetSize.SelectedItem).Size;
            else
                SetCustomSize(_selectedQuickProfile.TargetFileSize);
        }

        private void SetCustomSize(ulong tSize)
        {
            double temp = tSize;
            int index = 0;

            if (tSize > 0UL)
            {
                if (tSize > 999)
                {
                    foreach (SizeModificator mod in _sizeMod)
                    {
                        temp = tSize / (double)mod.Mod;
                        if (temp >= 1D)
                        {
                            index = mod.ID;
                        }
                        else
                            break;
                        if (temp < 1000D)
                            break;
                    }
                }
            }
            else
                temp = 1D;

            QuickSelectTargetSizeMod.SelectedIndex = index;
            QuickSelectTargetSizeValue.Value = temp;
        }

        private void LoadHcEncProfile()
        {
            _selectedVideoProfile = VideoProfileSelect.SelectedItem as HcEncProfile;

            if (_selectedVideoProfile == null || _selectedVideoProfile.Type != ProfileType.HcEnc) return;

            HcEncBitrate.Value = ((HcEncProfile)_selectedVideoProfile).Bitrate;

            HcEncProfile.SelectedIndex = ((HcEncProfile)_selectedVideoProfile).Profile;
            HcEncDCPrecision.SelectedIndex = ((HcEncProfile)_selectedVideoProfile).DCPrecision;
            HcEncInterlacing.SelectedIndex = ((HcEncProfile)_selectedVideoProfile).Interlacing;
            HcEncInterlaceFieldOrder.SelectedIndex = ((HcEncProfile)_selectedVideoProfile).FieldOrder;
            HcEncChromaDownsampling.SelectedIndex = ((HcEncProfile)_selectedVideoProfile).ChromaDownsampling;
            HcEncClosedGOP.IsChecked = ((HcEncProfile)_selectedVideoProfile).ClosedGops;
            HcEncSceneChange.IsChecked = ((HcEncProfile)_selectedVideoProfile).SceneChange;
            HcEncAutoGOP.IsChecked = ((HcEncProfile)_selectedVideoProfile).AutoGOP;
            HcEncGopLength.Value = ((HcEncProfile)_selectedVideoProfile).GopLength;
            HcEncBFrames.Value = ((HcEncProfile)_selectedVideoProfile).BFrames;
            HcEncLumGain.Value = ((HcEncProfile)_selectedVideoProfile).LuminanceGain;
            HcEncAQ.Value = ((HcEncProfile)_selectedVideoProfile).AQ;
            HcEncSMP.IsChecked = ((HcEncProfile)_selectedVideoProfile).SMP;
            hcEncVBVCheck.IsChecked = ((HcEncProfile)_selectedVideoProfile).VBVCheck;
            hcEncBuiltinMatrix.SelectedIndex = ((HcEncProfile)_selectedVideoProfile).Matrix;
            hcEncIntraVLC.SelectedIndex = ((HcEncProfile)_selectedVideoProfile).IntraVLC;
            hcEncColorimetry.SelectedIndex = ((HcEncProfile)_selectedVideoProfile).Colorimetry;
            hcEncMPGLevel.SelectedIndex = ((HcEncProfile)_selectedVideoProfile).MPGLevel;
            hcEncVBRBias.Value = ((HcEncProfile)_selectedVideoProfile).VBRBias;
            hcEncLastIFrame.IsChecked = ((HcEncProfile)_selectedVideoProfile).LastIFrame;
            hcEncSeqEndCode.IsChecked = ((HcEncProfile)_selectedVideoProfile).SeqEndCode;
            hcEncAllow3Frames.IsChecked = ((HcEncProfile)_selectedVideoProfile).Allow3BFrames;
            hcEncUseLosslessFile.IsChecked = ((HcEncProfile)_selectedVideoProfile).UseLosslessFile;
        }

        private void JavaPathSelectionClick(object sender, RoutedEventArgs e)
        {
            string folder = GetFilePath("java.exe");
            if (!string.IsNullOrEmpty(folder))
            {
                JavaPath.Text = folder;
            }
        }

        private void ResetSettingsClick(object sender, RoutedEventArgs e)
        {
            AppSettings.Reset();
        }

        private void OggEncodingModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                OggEncodingBitrateLabel.Visibility = OggEncodingMode.SelectedIndex == 0 ||
                                                     OggEncodingMode.SelectedIndex == 1
                                                         ? Visibility.Visible
                                                         : Visibility.Collapsed;
                OggEncodingQualityLabel.Visibility = OggEncodingMode.SelectedIndex == 2
                                                         ? Visibility.Visible
                                                         : Visibility.Collapsed;
            }
            else
            {
                OggEncodingBitrateLabel.Visibility = Visibility.Collapsed;
                OggEncodingQualityLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void AACEncodingModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                AACEncodingBitrateLabel.Visibility = AACEncodingMode.SelectedIndex == 0 ||
                                                     AACEncodingMode.SelectedIndex == 1
                                                         ? Visibility.Visible
                                                         : Visibility.Collapsed;
                AACEncodingQualityLabel.Visibility = AACEncodingMode.SelectedIndex == 2
                                                         ? Visibility.Visible
                                                         : Visibility.Collapsed;
            }
            else
            {
                AACEncodingBitrateLabel.Visibility = Visibility.Collapsed;
                AACEncodingQualityLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void MP3EncodingModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                MP3EncodingBitrateLabel.Visibility = MP3EncodingMode.SelectedIndex == 0 ||
                                                     MP3EncodingMode.SelectedIndex == 1
                                                         ? Visibility.Visible
                                                         : Visibility.Collapsed;
                MP3EncodingQualityLabel.Visibility = MP3EncodingMode.SelectedIndex == 2
                                                         ? Visibility.Visible
                                                         : Visibility.Collapsed;
                MP3EncodingPresetLabel.Visibility = MP3EncodingMode.SelectedIndex == 3
                                                        ? Visibility.Visible
                                                        : Visibility.Collapsed;
            }
            else
            {
                MP3EncodingBitrateLabel.Visibility = Visibility.Collapsed;
                MP3EncodingQualityLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (_profiles != null)
                _profiles.Destroy();
        }

        private void NeroAacEncGetLinkMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start((string) NeroAacEncGetLink.Content);
        }

        private void VP8BasicSpeedControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (VP8BasicSpeedControl.SelectedIndex)
            {
                case 0:
                    VP8CPUUtilizationModifier.Maximum = 15;
                    VP8CPUUtilizationModifier.Value = 12;
                    VP8CPUUtilizationModifier.IsEnabled = true;
                    break;
                case 1:
                    VP8CPUUtilizationModifier.Maximum = 5;
                    VP8CPUUtilizationModifier.Value = 3;
                    VP8CPUUtilizationModifier.IsEnabled = true;
                    break;
                case 2:
                    VP8CPUUtilizationModifier.Maximum = 1;
                    VP8CPUUtilizationModifier.Value = 0;
                    VP8CPUUtilizationModifier.IsEnabled = false;
                    break;
            }
        }
    }
}
