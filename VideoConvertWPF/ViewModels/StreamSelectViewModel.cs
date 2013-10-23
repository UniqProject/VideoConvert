// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamSelectViewModel.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using BDInfo;
    using Caliburn.Micro;
    using Interfaces;
    using SharpDvdInfo;
    using SharpDvdInfo.DvdTypes;
    using SharpDvdInfo.Model;
    using VideoConvert.AppServices.Model.Profiles;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Model.MediaInfo;
    using VideoConvert.Interop.Model.Profiles;
    using VideoConvert.Interop.Utilities;
    using ILog = log4net.ILog;
    using LogManager = log4net.LogManager;
    using StreamTreeNode = VideoConvert.Interop.Model.StreamTreeNode;

    public class StreamSelectViewModel : ViewModelBase, IStreamSelectViewModel
    {
        private readonly IShellViewModel _shellViewModel;
        private static readonly ILog Log = LogManager.GetLogger(typeof(StreamSelectViewModel));

        #region private properties

        private EncodeInfo _jobInfo;
        private List<EncoderProfile> _profiles;
        private List<StreamTreeNode> _tree;

        private BDROM _bdInfo;
        private int _treeNodeID;
        private int _defaultSelection;
        private MovieEntry _resultMovieData;
        private EpisodeEntry _resultEpisodeData;
        private int _selectedIndex;
        private StreamTreeNode _selectedNode;
        private StreamTreeNode _selectedTitleInfo;
        private EncoderProfile _selectedProfile;
        private string _selectedProfileName;

        private readonly IAppConfigService _configService;
        private readonly IProcessingService _processingService;

        #endregion

        #region public properties

        public bool? DialogResult { get; set; }

        public EncodeInfo JobInfo
        {
            get
            {
                return _jobInfo;
            }
            set
            {
                _jobInfo = value;
                this.NotifyOfPropertyChange(()=>this.JobInfo);
            }
        }

        public string JobTitle
        {
            get
            {
                return JobInfo.JobName; 
            }
            set
            {
                JobInfo.JobName = value;
                this.NotifyOfPropertyChange(()=>JobTitle);
            }
        }

        public List<EncoderProfile> Profiles
        {
            get
            {
                return _profiles;
            }
            set
            {
                _profiles = value;
                this.NotifyOfPropertyChange(()=>this.Profiles);
            }
        }

        public EncoderProfile SelectedProfile
        {
            get
            {
                return _selectedProfile;
            }
            set
            {
                _selectedProfile = value;
                this.NotifyOfPropertyChange(()=>this.SelectedProfile);
            }
        }

        public string SelectedProfileName
        {
            get
            {
                return this._configService.LastSelectedProfile; 
            }
            set { _selectedProfileName = value; }
        }

        public List<StreamTreeNode> Tree
        {
            get
            {
                return _tree;
            }
            set
            {
                _tree = value;
                this.NotifyOfPropertyChange(()=>this.Tree);
            }
        }

        public StreamTreeNode SelectedTitleInfo
        {
            get
            {
                return _selectedTitleInfo;
            }
            set
            {
                _selectedTitleInfo = value;
                this.NotifyOfPropertyChange(()=>this.SelectedTitleInfo);
            }
        }

        public BDROM BdRom
        {
            get
            {
                return _bdInfo;
            }
            set
            {
                _bdInfo = value;
                this.NotifyOfPropertyChange(()=>this.BdRom);
            }
        }

        public int TreeNodeID
        {
            get
            {
                return _treeNodeID;
            }
            set
            {
                _treeNodeID = value;
                this.NotifyOfPropertyChange(()=>this.TreeNodeID);
            }
        }

        public int DefaultSelection
        {
            get
            {
                return _defaultSelection;
            }
            set
            {
                _defaultSelection = value;
                this.NotifyOfPropertyChange(()=>this.DefaultSelection);
            }
        }

        public MovieEntry ResultMovieData
        {
            get
            {
                return _resultMovieData;
            }
            set
            {
                _resultMovieData = value;
                this.NotifyOfPropertyChange(()=>this.ResultMovieData);
            }
        }

        public EpisodeEntry ResultEpisodeData
        {
            get
            {
                return _resultEpisodeData;
            }
            set
            {
                _resultEpisodeData = value;
                this.NotifyOfPropertyChange(()=>this.ResultEpisodeData);
            }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                this.NotifyOfPropertyChange(()=>this.SelectedIndex);
            }
        }

        public StreamTreeNode SelectedNode
        {
            get
            {
                return _selectedNode;
            }
            set
            {
                _selectedNode = value;
                this.NotifyOfPropertyChange(() => this.SelectedNode);
                this.NotifyOfPropertyChange(() => this.SelectedNodeData);
                this.NotifyOfPropertyChange(() => this.MatroskaDefault);
                this.NotifyOfPropertyChange(() => this.HardcodeIntoVideo);
                this.NotifyOfPropertyChange(() => this.KeepOnlyForced);
            }
        }

        public object SelectedNodeData
        {
            get
            {
                if (_selectedNode != null)
                    return _selectedNode.Data;
                return null;
            }
        }

        public bool MatroskaDefault
        {
            get
            {
                if (_selectedNode != null)
                    return _selectedNode.MatroskaDefault;
                return false;
            }
            set
            {
                if (_selectedNode != null)
                {
                    _selectedNode.MatroskaDefault = value;
                    this.NotifyOfPropertyChange(()=>MatroskaDefault);
                }
            }
        }

        public bool HardcodeIntoVideo
        {
            get
            {
                if (_selectedNode != null)
                    return _selectedNode.HardcodeIntoVideo;
                return false;
            }
            set
            {
                if (_selectedNode != null)
                {
                    _selectedNode.HardcodeIntoVideo = value;
                    this.NotifyOfPropertyChange(() => HardcodeIntoVideo);
                }
            }
        }

        public bool KeepOnlyForced
        {
            get
            {
                if (_selectedNode != null)
                    return _selectedNode.KeepOnlyForced;
                return false;
            }
            set
            {
                if (_selectedNode != null)
                {
                    _selectedNode.KeepOnlyForced = value;
                    this.NotifyOfPropertyChange(() => KeepOnlyForced);
                }
            }
        }

        #endregion

        #region constructors

        public StreamSelectViewModel(IAppConfigService config, IShellViewModel shellViewModel,
            IWindowManager windowManager, IProcessingService processing)
        {
            this._shellViewModel = shellViewModel;
            this.WindowManager = windowManager;
            this._configService = config;
            this._processingService = processing;
        }

        public override void OnLoad()
        {
            base.OnLoad();
            Tree = new List<StreamTreeNode>();
            Profiles = new List<EncoderProfile>();
            DefaultSelection = 0;
            LoadStreams();
        }

        public void ShowAbout()
        {
            
        }

        #endregion

        #region background stream info loading

        private void BgWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (JobInfo.Input == InputType.InputBluRay)
            {
                JobTitle = _bdInfo.VolumeLabel;
            }
            else if (JobInfo.Input != InputType.InputDvd)
            {
                JobTitle = JobInfo.MediaInfo.General.Title.Length > 0
                                     ? JobInfo.MediaInfo.General.Title
                                     : JobInfo.MediaInfo.General.FileName;
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(JobInfo.InputFile);
                JobTitle = GetVolumeLabel(dir);

                if (string.IsNullOrEmpty(JobTitle))
                {
                    JobTitle = Path.GetFileName(JobInfo.InputFile);
                }
            }

            this.NotifyOfPropertyChange(() => JobInfo);
            this.NotifyOfPropertyChange(() => Tree);
            SelectedIndex = DefaultSelection;
        }

        private void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            switch (JobInfo.Input)
            {
                case InputType.InputUndefined:
                    this.DialogResult = false;
                    return;
                case InputType.InputBluRay:
                case InputType.InputAvchd:
                case InputType.InputHddvd:
                    GetBDInfo();
                    break;
                case InputType.InputDvd:
                    GetDVDTitleList();
                    break;
                default:
                    GetFileInfo();
                    break;
            }
        }

        #endregion

        #region background profile loading

        private void ProfilesWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.NotifyOfPropertyChange(()=>this.Profiles);
            this.NotifyOfPropertyChange(()=> this.SelectedProfileName);
        }

        private void ProfilesWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            ProfilesHandler profHandler = new ProfilesHandler(this._configService);
            _profiles = profHandler.FilteredList.Where(p => p.Type == ProfileType.QuickSelect).ToList();
        }

        #endregion

        #region control and property events

        private void TreeNodePropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            StreamTreeNode myStreamTree = sender as StreamTreeNode;
            if (myStreamTree == null) return;
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "IsChecked":
                    if (myStreamTree.Children != null && myStreamTree.Children.Count > 0)
                    {
                        CheckSubItems(myStreamTree);
                    }

                    this.NotifyOfPropertyChange(() => Tree);

                    break;

                case "MatroskaDefault":
                    if (myStreamTree.Data.GetType().Name == "SubtitleInfo")
                        ((SubtitleInfo)myStreamTree.Data).MkvDefault = myStreamTree.MatroskaDefault;
                    else if (myStreamTree.Data.GetType().Name == "AudioInfo")
                        ((AudioInfo)myStreamTree.Data).MkvDefault = myStreamTree.MatroskaDefault;

                    this.NotifyOfPropertyChange(() => this.MatroskaDefault);

                    break;

                case "HardcodeIntoVideo":
                    if (myStreamTree.Data.GetType().Name == "SubtitleInfo")
                        ((SubtitleInfo)myStreamTree.Data).HardSubIntoVideo = myStreamTree.HardcodeIntoVideo;

                    this.NotifyOfPropertyChange(() => this.HardcodeIntoVideo);

                    break;

                case "KeepOnlyForced":
                    if (myStreamTree.Data.GetType().Name == "SubtitleInfo")
                        ((SubtitleInfo)myStreamTree.Data).KeepOnlyForcedCaptions = myStreamTree.KeepOnlyForced;

                    this.NotifyOfPropertyChange(() => this.KeepOnlyForced);

                    break;
            }
        }

        public void SetSelectedItem(StreamTreeNode myNode)
        {
            SelectedNode = myNode;
            this.NotifyOfPropertyChange(() => this.SelectedNode);
            this.NotifyOfPropertyChange(() => this.SelectedNodeData);
        }

        public void ClickOK()
        {
            if (SelectedTitleInfo == null) return;
            if (SelectedProfile == null) return;
            if (string.IsNullOrEmpty(JobTitle)) return;

            IEnumerable<StreamTreeNode> sortedList = GetCheckedItems(SelectedTitleInfo);

            bool videoSet = false;

            foreach (StreamTreeNode item in sortedList)
            {
                if (item.Data == null) continue;

                Type dataType = item.Data.GetType();

                if (dataType == typeof(string))
                    JobInfo.InputFile = (string)item.Data;
                else if (dataType == typeof(VideoInfo))
                {
                    if (!videoSet)
                    {
                        JobInfo.VideoStream = (VideoInfo)item.Data;
                        videoSet = true;
                    }
                }
                else if (dataType == typeof(StereoVideoInfo))
                    JobInfo.StereoVideoStream = (StereoVideoInfo)item.Data;
                else if (dataType == typeof(AudioInfo))
                    JobInfo.AudioStreams.Add((AudioInfo)item.Data);
                else if (dataType == typeof(SubtitleInfo))
                {
                    SubtitleInfo sub = (SubtitleInfo)item.Data;
                    bool isBD = JobInfo.Input == InputType.InputBluRay || JobInfo.Input == InputType.InputAvchd ||
                                JobInfo.Input == InputType.InputHddvd;
                    if ((sub.Format == "PGS" || sub.Format == "VobSub" || sub.Format == "UTF-8" || sub.Format == "ASS" || sub.Format == "SSA") && ((isBD && _bdInfo != null && !_bdInfo.Is3D) || !isBD))
                        // don't extract subtitles on 3d blurays, because eac3to can't handle them
                        JobInfo.SubtitleStreams.Add(sub);
                }
                else if (dataType == typeof(List<TimeSpan>))
                    JobInfo.Chapters.AddRange((List<TimeSpan>)item.Data);
                else if (dataType == typeof(Dictionary<string, object>))
                {
                    object itemData;
                    (item.Data as Dictionary<string, object>).TryGetValue("Name", out itemData);
                    if (itemData != null)
                        JobInfo.InputFile = (string)itemData;
                    (item.Data as Dictionary<string, object>).TryGetValue("PlaylistIndex", out itemData);
                    if (itemData != null)
                        JobInfo.StreamId = (int)itemData;
                    (item.Data as Dictionary<string, object>).TryGetValue("TrackID", out itemData);
                    if (itemData != null)
                        JobInfo.TrackId = (int)itemData;
                }
            }
            JobInfo.StreamId = -1;

            if (Profiles != null && SelectedProfile != null)
            {
                JobInfo.EncodingProfile = (QuickSelectProfile)SelectedProfile;
                JobInfo.AudioProfile = GetProfile(JobInfo.EncodingProfile.AudioProfile,
                    JobInfo.EncodingProfile.AudioProfileType);
                JobInfo.VideoProfile = GetProfile(JobInfo.EncodingProfile.VideoProfile,
                    JobInfo.EncodingProfile.VideoProfileType);
            }

            this._configService.LastSelectedProfile = SelectedProfile.Name;

            if (this._configService.CreateXbmcInfoFile)
            {
                if (_resultMovieData != null)
                    JobInfo.MovieInfo = ResultMovieData;
                else if (_resultEpisodeData != null)
                    JobInfo.EpisodeInfo = ResultEpisodeData;
            }

            _bdInfo = null;
            this.TryClose(true);
        }

        #endregion

        #region model logic

        public void LoadStreams()
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += BgWorkerDoWork;
            bgWorker.RunWorkerCompleted += BgWorkerRunWorkerCompleted;
            bgWorker.RunWorkerAsync();

            BackgroundWorker profilesWorker = new BackgroundWorker();
            profilesWorker.DoWork += ProfilesWorkerDoWork;
            profilesWorker.RunWorkerCompleted += ProfilesWorkerRunWorkerCompleted;
            profilesWorker.RunWorkerAsync();
        }

        public void GetFileInfo()
        {
            MediaInfoContainer mi = new MediaInfoContainer();
            try
            {
                mi = GenHelper.GetMediaInfo(JobInfo.InputFile);
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex);
            }

            JobInfo.MediaInfo = mi;

            const string fileTitleFormat = "{0} / {1} / Length: {2}";
            const string fileAudioFormat = "{0:g} Channels ({1}) / {2:g}Hz / {3:g} bit / {4:g} kbit/s";
            const string fileVideoFormat = "{0:d}x{1:d} {2} / Profile: {3} / {4:0.000}fps";
            const string strChapters = "Chapters";
            const string strVideo = "Video";
            const string strAudio = "Audio";
            const string strSubtitles = "Subtitles";

            string containerFormat = mi.General.Format;
            string duration = mi.General.DurationTime.ToString("H:mm:ss.fff");
            string shortFileName = mi.General.FileName + "." + mi.General.FileExtension;

            string treeRoot = string.Format(fileTitleFormat, shortFileName, containerFormat, duration);

            StreamTreeNode root = new StreamTreeNode
            {
                ID = _treeNodeID++,
                Name = treeRoot,
                Data = JobInfo.InputFile,
                IsChecked = true,
                IsExpanded = true,
                Children = new List<StreamTreeNode>()
            };
            root.PropertyChanged += TreeNodePropertyChanged;
            _tree.Add(root);

            StreamTreeNode chaptersStreamTree = CreateNode(root, strChapters, null);
            StreamTreeNode videoStreamTree = CreateNode(root, strVideo, null);
            StreamTreeNode audioStreamTree = CreateNode(root, strAudio, null);
            StreamTreeNode subStreamTree = CreateNode(root, strSubtitles, null);

            if (mi.Chapters.Count > 0)
            {
                string chaptersTitle = string.Format("{0:0} {1}", mi.Chapters.Count, strChapters);

                CreateNode(chaptersStreamTree, chaptersTitle, mi.Chapters);
            }
            else
                chaptersStreamTree.IsChecked = false;

            int streamIndex = 0;

            foreach (MediaInfoContainer.VideoStreamInfo clip in mi.Video)
            {
                streamIndex++;
                int videoPid = clip.ID;
                string videoCodec = clip.FormatInfo;
                string videoCodecShort = clip.Format;
                string videoDesc = string.Format(fileVideoFormat, clip.Width, clip.Height, clip.ScanType,
                                                 clip.FormatProfile, clip.FrameRate);

                string videoStreamTitle = string.Format("{3:g}: {0} ({1}), {2}", videoCodec, videoCodecShort, videoDesc,
                                                        streamIndex);

                VideoInfo vid = new VideoInfo();
                if (JobInfo.Input == InputType.InputAvi)
                    vid.StreamId = 0;
                else
                    vid.StreamId = videoPid == 0 ? streamIndex : videoPid;
                vid.StreamKindID = clip.StreamKindID;
                vid.FPS = clip.FrameRate;
                vid.PicSize = clip.VideoSize;
                vid.Interlaced = clip.ScanType == "Interlaced";
                vid.Format = clip.Format;
                vid.FormatProfile = clip.FormatProfile;
                vid.Height = clip.Height;
                vid.Width = clip.Width;
                vid.FrameCount = clip.FrameCount;
                vid.StreamSize = clip.StreamSize;
                vid.Length = mi.General.DurationTime.TimeOfDay.TotalSeconds;
                Single.TryParse(clip.DisplayAspectRatio, NumberStyles.Number, this._configService.CInfo, out vid.AspectRatio);
                vid.FrameRateEnumerator = clip.FrameRateEnumerator;
                vid.FrameRateDenominator = clip.FrameRateDenominator;
                vid.FrameMode = clip.FormatFrameMode;

                CreateNode(videoStreamTree, videoStreamTitle, vid);
            }

            videoStreamTree.IsChecked = videoStreamTree.Children.Count > 0;

            foreach (MediaInfoContainer.AudioStreamInfo audio in mi.Audio)
            {
                streamIndex++;
                int audioPid = audio.ID;
                string audioCodec = audio.FormatInfo;
                string audioCodecShort = audio.Format;
                string audioLangCode = audio.LanguageIso6392;
                string audioLanguage = audio.LanguageFull;
                int audioStreamKindID = audio.StreamKindID;

                string audioDesc = string.Format(fileAudioFormat, audio.Channels, audio.ChannelPositions,
                                                 audio.SamplingRate, audio.BitDepth, audio.BitRate / 1000);

                string audioStreamTitle = string.Format("{5:g}: {0} ({1}) / {2} ({3}) / {4}", audioCodec,
                                                        audioCodecShort, audioLangCode, audioLanguage, audioDesc,
                                                        streamIndex);

                if (JobInfo.Input == InputType.InputAvi)
                    audioPid += 1;
                else
                    audioPid = audioPid == 0 ? streamIndex : audioPid;

                AudioInfo aud = new AudioInfo
                {
                    Id = audioPid,
                    Format = audioCodecShort,
                    FormatProfile = audio.FormatProfile,
                    StreamId = streamIndex,
                    LangCode = audioLangCode,
                    OriginalId = audioPid,
                    StreamKindId = audioStreamKindID,
                    Delay = audio.Delay,
                    Bitrate = audio.BitRate,
                    SampleRate = audio.SamplingRate,
                    ChannelCount = audio.Channels,
                    BitDepth = audio.BitDepth,
                    ShortLang = audio.LanguageIso6391,
                    StreamSize = audio.StreamSize,
                    Length = mi.General.DurationTime.TimeOfDay.TotalSeconds,
                    IsHdStream = audio.CompressionMode == "Lossless"
                };

                CreateNode(audioStreamTree, audioStreamTitle, aud);
            }

            audioStreamTree.IsChecked = audioStreamTree.Children.Count > 0;

            foreach (MediaInfoContainer.TextStreamInfo sub in mi.Text)
            {
                streamIndex++;
                string subCodec = sub.CodecIDInfo;
                string subCodecShort = sub.Format;
                string subLangCode = sub.LanguageIso6392;
                string subLanguage = sub.LanguageFull;
                int subStreamKindID = sub.StreamKindID;

                string subStreamTitle = string.Format("{4:g}: {0} ({1}) / {2} ({3})", subCodec, subCodecShort,
                                                      subLangCode, subLanguage, streamIndex);

                SubtitleInfo subInfo = new SubtitleInfo
                {
                    Id = sub.ID,
                    StreamId = streamIndex,
                    LangCode = subLangCode,
                    Format = subCodecShort,
                    StreamKindId = subStreamKindID,
                    Delay = sub.Delay,
                    StreamSize = sub.StreamSize
                };

                CreateNode(subStreamTree, subStreamTitle, subInfo);
            }

            foreach (MediaInfoContainer.ImageStreamInfo sub in mi.Image)
            {
                streamIndex++;
                string subCodec = sub.CodecIDInfo;
                string subCodecShort = sub.Format;
                string subLangCode = sub.LanguageIso6392;
                string subLanguage = sub.LanguageFull;
                int subStreamKindID = sub.StreamKindID;

                string subStreamTitle = string.Format("{4:g}: {0} ({1}) / {2} ({3})", subCodec, subCodecShort,
                                                      subLangCode, subLanguage, streamIndex);
                SubtitleInfo subInfo = new SubtitleInfo
                {
                    Id = sub.ID,
                    StreamId = streamIndex,
                    LangCode = subLangCode,
                    Format = subCodecShort,
                    StreamKindId = subStreamKindID,
                    Delay = 0,
                    StreamSize = sub.StreamSize
                };

                CreateNode(subStreamTree, subStreamTitle, subInfo);
            }

            subStreamTree.IsChecked = subStreamTree.Children.Count > 0;

            this.NotifyOfPropertyChange(() => this.Tree);
        }

        public void GetBDInfo()
        {
            const string strChapters = "Chapters";    //ProcessingService.GetResourceString("streamselect_chapters");
            const string strVideo = "Video";    //ProcessingService.GetResourceString("streamselect_video");
            const string strAudio = "Audio";    //ProcessingService.GetResourceString("streamselect_audio");
            const string strSubtitles = "Subtitles";    //ProcessingService.GetResourceString("streamselect_subtitles");

            const string bdTitleFormat = "Title: {0:g} ({1}), Length: {2}";    //ProcessingService.GetResourceString("streamselect_bd_general");
            const string bdAudioFormat = "{5:g}: {0} ({1}) / {2} ({3}) / {4}";
            const string bdSubFormat = "{3:g}: {0} / {1} ({2}); {4}";

            _bdInfo = new BDROM(JobInfo.InputFile);
            _bdInfo.Scan();

            int longestClip = GetLongestBDPlaylist();

            int playlistIndex = 1;

            foreach (TSPlaylistFile item in _bdInfo.PlaylistFiles.Values)
            {
                if (!item.IsValid)
                {
                    playlistIndex++;
                    continue;
                }

                int streamIndex = 0;

                DateTime duration = new DateTime();

                duration = duration.AddSeconds(item.TotalLength);

                string treeRoot = string.Format(bdTitleFormat, playlistIndex, item.Name,
                                                duration.ToString("H:mm:ss.fff"));

                Dictionary<string, object> treeData = new Dictionary<string, object>
                    {
                        {
                            "Name",
                            Path.Combine(_bdInfo.DirectoryPLAYLIST.FullName,
                                         item.Name)
                        },
                        {"PlaylistIndex", playlistIndex}
                    };

                StreamTreeNode root = new StreamTreeNode
                {
                    ID = _treeNodeID++,
                    Name = treeRoot,
                    Data = treeData,
                    Children = new List<StreamTreeNode>(),
                    IsChecked = true,
                    IsExpanded = true
                };
                root.PropertyChanged += TreeNodePropertyChanged;
                _tree.Add(root);

                StreamTreeNode chaptersStreamTree = CreateNode(root, strChapters, null);
                StreamTreeNode videoStreamTree = CreateNode(root, strVideo, null);
                StreamTreeNode audioStreamTree = CreateNode(root, strAudio, null);
                StreamTreeNode subStreamTree = CreateNode(root, strSubtitles, null);

                List<TimeSpan> streamChapters = new List<TimeSpan>();
                if (item.Chapters.Count > 1)
                {
                    streamIndex++;

                    streamChapters.AddRange(item.Chapters.Select(TimeSpan.FromSeconds));

                    string chaptersFormat = string.Format("{0:0} {1}", streamChapters.Count, strChapters);

                    CreateNode(chaptersStreamTree, chaptersFormat, streamChapters);
                }

                string videoDescStereo = string.Empty;
                int leftVideoStreamID = -1;
                foreach (TSVideoStream clip in item.VideoStreams)
                {
                    streamIndex++;
                    string videoCodec = clip.CodecName;
                    string videoCodecShort = clip.CodecShortName;
                    string videoDesc = clip.Description;

                    if ((clip.StreamType == TSStreamType.AVC_VIDEO) && (item.VideoStreams.Count > 1)
                        && (item.VideoStreams[0].PID == clip.PID)
                        && (item.VideoStreams[item.VideoStreams.Count - 1].StreamType == TSStreamType.MVC_VIDEO))
                    {
                        videoDescStereo = videoDesc;
                        videoCodec += " (left eye)";
                        leftVideoStreamID = streamIndex;
                    }
                    if ((clip.StreamType == TSStreamType.MVC_VIDEO) && (item.VideoStreams.Count > 1)
                        && (item.VideoStreams[item.VideoStreams.Count - 1].PID == clip.PID)
                        && (item.VideoStreams[0].StreamType == TSStreamType.AVC_VIDEO))
                    {
                        videoDesc = videoDescStereo;
                        videoCodec = "MPEG-4 MVC Video (right eye)";
                    }
                    /* */
                    string videoStreamFormat = string.Format("{3:g}: {0} ({1}), {2}", videoCodec, videoCodecShort,
                                                                   videoDesc, streamIndex);
                    switch (clip.StreamType)
                    {
                        case TSStreamType.AVC_VIDEO:
                        case TSStreamType.MPEG2_VIDEO:
                        case TSStreamType.MPEG1_VIDEO:
                        case TSStreamType.VC1_VIDEO:
                            {
                                VideoInfo vid = new VideoInfo
                                {
                                    StreamId = streamIndex,
                                    TrackId = playlistIndex,
                                    FPS = (float)clip.FrameRateEnumerator / clip.FrameRateDenominator,
                                    PicSize = (VideoFormat)clip.VideoFormat,
                                    Interlaced = clip.IsInterlaced,
                                    Format = clip.CodecShortName,
                                    DemuxStreamId = clip.PID,
                                    FrameCount = 0,
                                    Encoded = false,
                                    IsRawStream = false,
                                    StreamSize = 0,
                                    Length = item.TotalLength,
                                    FrameRateEnumerator = clip.FrameRateEnumerator,
                                    FrameRateDenominator = clip.FrameRateDenominator,
                                    Height = clip.Height
                                };

                                Int32.TryParse(item.Name.Substring(0, item.Name.LastIndexOf('.')), NumberStyles.Number,
                                               this._configService.CInfo, out vid.DemuxPlayList);

                                foreach (TSStreamClip streamClip in item.StreamClips)
                                    vid.DemuxStreamNames.Add(streamClip.StreamFile.FileInfo.FullName);

                                float mod;
                                switch (clip.AspectRatio)
                                {
                                    case TSAspectRatio.ASPECT_16_9:
                                        mod = (float)1.777778;
                                        break;
                                    default:
                                        mod = (float)1.333333;
                                        break;
                                }
                                vid.Width = (int)(vid.Height * mod);
                                vid.AspectRatio = mod;

                                CreateNode(videoStreamTree, videoStreamFormat, vid);
                            }
                            break;
                        case TSStreamType.MVC_VIDEO:
                            {
                                StereoVideoInfo vid = new StereoVideoInfo
                                {
                                    RightStreamId = streamIndex,
                                    LeftStreamId = leftVideoStreamID
                                };
                                CreateNode(videoStreamTree, videoStreamFormat, vid);
                            }
                            break;
                    }
                }

                foreach (TSAudioStream audio in item.AudioStreams)
                {
                    streamIndex++;
                    string audioCodec = audio.CodecName;
                    string audioCodecShort = audio.CodecShortName;
                    string audioDesc = audio.Description;
                    string audioLangCode = audio.LanguageCode;
                    string audioLanguage = audio.LanguageName;

                    string audioStreamFormat = string.Format(bdAudioFormat, audioCodec, audioCodecShort, audioLangCode,
                                                             audioLanguage, audioDesc, streamIndex);

                    AudioInfo aud = new AudioInfo
                    {
                        Format = audioCodecShort,
                        FormatProfile = string.Empty,
                        Id = streamIndex,
                        StreamId = streamIndex,
                        LangCode = audioLangCode,
                        TempFile = string.Empty,
                        OriginalId = streamIndex,
                        Delay = 0,
                        Bitrate = audio.BitRate,
                        DemuxStreamId = audio.PID,
                        SampleRate = audio.SampleRate,
                        ChannelCount = audio.ChannelCount + audio.LFE,
                        BitDepth = audio.BitDepth,
                        ShortLang = audio.LanguageCode,
                        StreamSize = 0,
                        Length = item.TotalLength,
                        IsHdStream = audio.CoreStream != null
                    };

                    CreateNode(audioStreamTree, audioStreamFormat, aud);
                }

                foreach (TSTextStream sub in item.TextStreams)
                {
                    streamIndex++;
                    string subCodecShort = sub.CodecShortName;
                    string subDesc = sub.Description;
                    string subLangCode = sub.LanguageCode;
                    string subLanguage = sub.LanguageName;

                    string subStreamFormat = string.Format(bdSubFormat, subCodecShort, subLangCode, subLanguage,
                                                           streamIndex, subDesc);

                    SubtitleInfo subInfo = new SubtitleInfo
                    {
                        Id = streamIndex,
                        StreamId = streamIndex,
                        TempFile = string.Empty,
                        LangCode = subLangCode,
                        Format = subCodecShort,
                        Delay = 0,
                        DemuxStreamId = sub.PID,
                        StreamSize = 0
                    };

                    CreateNode(subStreamTree, subStreamFormat, subInfo);
                }

                foreach (TSGraphicsStream sub in item.GraphicsStreams)
                {
                    streamIndex++;
                    string subCodecShort = sub.CodecShortName;
                    string subDesc = sub.Description;
                    string subLangCode = sub.LanguageCode;
                    string subLanguage = sub.LanguageName;

                    string subStreamFormat = string.Format(bdSubFormat, subCodecShort, subLangCode, subLanguage,
                                                           streamIndex, subDesc);

                    SubtitleInfo subInfo = new SubtitleInfo
                    {
                        Id = streamIndex,
                        StreamId = streamIndex,
                        TempFile = string.Empty,
                        LangCode = subLangCode,
                        Format = subCodecShort,
                        DemuxStreamId = sub.PID,
                        StreamSize = 0
                    };

                    CreateNode(subStreamTree, subStreamFormat, subInfo);
                }
                playlistIndex++;
            }
            _defaultSelection = longestClip - 1;
        }

        public int GetLongestBDPlaylist()
        {
            int longest = 0;
            int longestClip = 0;

            int playlistIndex = 1;

            foreach (int clipLength in from item in _bdInfo.PlaylistFiles.Values where item.IsValid select (int)Math.Truncate(item.TotalLength))
            {
                if (clipLength > longest)
                {
                    longest = clipLength;
                    longestClip = playlistIndex;
                }
                playlistIndex++;
            }

            return longestClip;
        }

        public void GetDVDTitleList()
        {
            const string strChapters = "Chapters";
            const string strVideo = "Video";
            const string strAudio = "Audio";
            const string strSubtitles = "Subtitles";

            const string dvdTitleFormat = "Title: {0:g}, Length: {1}";
            const string dvdAudioFormat = "Track {0:g} ({1:g}), {2} ({3} - {4}), {5} {6:g} Channels {7:g} Hz, {8}";
            const string dvdSubFormat = "Track {0:g} ({1:g}), {2} ({3} - {4})";

            const string dvdVideoStreamFormat = "{0} {1} {2} ({3}) {4} {5:0.000} fps";

            DvdInfoContainer dvd = new DvdInfoContainer(JobInfo.InputFile);

            foreach (TitleInfo info in dvd.Titles)
            {
                int videoId = info.TitleNumber;
                float fps = info.VideoStream.Framerate;
                string videoFormat = info.VideoStream.VideoStandard.ToString();
                string codec = _processingService.StringValueOf(info.VideoStream.CodingMode);
                string aspect = _processingService.StringValueOf(info.VideoStream.AspectRatio);

                string resolution = _processingService.StringValueOf(info.VideoStream.VideoResolution);

                string[] resolutionArray = resolution.Split(new[] {"x"}, StringSplitOptions.RemoveEmptyEntries);
                int width = 0, height = 0;

                try
                {
                    int.TryParse(resolutionArray[0], out width);
                    int.TryParse(resolutionArray[1], out height);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }

                string letterboxed = _processingService.StringValueOf(info.VideoStream.DisplayFormat);
                int vtsID = info.TitleSetNumber;

                DateTime duration = DateTime.MinValue.Add(info.VideoStream.Runtime);
                string treeRoot = string.Format(dvdTitleFormat, videoId, duration.ToString("H:mm:ss.fff"));

                Dictionary<string, object> treeData = new Dictionary<string, object>
                {
                    {"Name", JobInfo.InputFile},
                    {"TrackID", videoId}
                };
                StreamTreeNode root = new StreamTreeNode
                {
                    ID = _treeNodeID++,
                    Name = treeRoot,
                    Data = treeData,
                    Children = new List<StreamTreeNode>(),
                    IsChecked = true,
                    IsExpanded = true
                };
                _tree.Add(root);

                StreamTreeNode chaptersTree = CreateNode(root, strChapters, null);
                StreamTreeNode videoTree = CreateNode(root, strVideo, null);
                StreamTreeNode audioTree = CreateNode(root, strAudio, null);
                StreamTreeNode subTree = CreateNode(root, strSubtitles, null);

                if (info.Chapters != null && info.Chapters.Count > 0)
                {
                    string chaptersFormat = string.Format("{0:0} {1}", info.Chapters.Count, strChapters);
                    CreateNode(chaptersTree, chaptersFormat, info.Chapters);
                }

                string videoStream = string.Format(dvdVideoStreamFormat, codec, resolution, aspect, letterboxed, videoFormat,
                                                   fps);
                VideoInfo vid = new VideoInfo
                {
                    VtsId = vtsID,
                    TrackId = videoId,
                    StreamId = 1,
                    FPS = fps,
                    Interlaced = true,
                    Format = codec,
                    FrameCount = 0,
                    Width = width,
                    Height = height,
                    Encoded = false,
                    IsRawStream = false,
                    DemuxStreamNames = new List<string>(),
                    StreamSize = 0,
                    Length = info.VideoStream.Runtime.TotalSeconds,
                    FrameRateEnumerator = info.VideoStream.FrameRateNumerator,
                    FrameRateDenominator = info.VideoStream.FrameRateDenominator,
                    AspectRatio = info.VideoStream.AspectRatio == DvdVideoAspectRatio.Aspect4By3 ? 4f / 3f : 16f / 9f
                };

                CreateNode(videoTree, videoStream, vid);

                foreach (AudioProperties stream in info.AudioStreams)
                {
                    int audioID = stream.StreamIndex;
                    string langCode = stream.Language.Code;
                    string language = stream.Language.Name;
                    string format = _processingService.StringValueOf(stream.CodingMode);
                    int frequency = stream.SampleRate;
                    string quantization = _processingService.StringValueOf(stream.Quantization);
                    int channels = stream.Channels;
                    string content = _processingService.StringValueOf(stream.Extension);
                    int streamID = stream.StreamId;

                    string audioStream = string.Format(dvdAudioFormat, audioID, streamID, langCode, language,
                                                           content, format, channels, frequency, quantization);

                    AudioInfo aud = new AudioInfo
                    {
                        Format = format,
                        FormatProfile = string.Empty,
                        Id = audioID,
                        StreamId = streamID,
                        LangCode = langCode,
                        TempFile = string.Empty,
                        OriginalId = audioID,
                        Delay = 0,
                        Bitrate = 0,
                        SampleRate = frequency,
                        ChannelCount = channels,
                        ShortLang = langCode,
                        StreamSize = 0,
                        IsHdStream = false
                    };

                    CreateNode(audioTree, audioStream, aud);
                }

                foreach (SubpictureProperties stream in info.SubtitleStreams)
                {
                    int subID = stream.StreamIndex;
                    string langCode = stream.Language.Code;
                    string language = stream.Language.Name;
                    string content = _processingService.StringValueOf(stream.Extension);
                    int streamID = stream.StreamId;

                    string subtitleStream = string.Format(dvdSubFormat, subID, streamID, langCode, language, content);

                    SubtitleInfo subInfo = new SubtitleInfo
                    {
                        Id = subID + info.AudioStreams.Count,
                        StreamId = streamID,
                        TempFile = string.Empty,
                        LangCode = langCode,
                        Format = "VobSub",
                        Delay = 0,
                        StreamSize = 0
                    };

                    CreateNode(subTree, subtitleStream, subInfo);
                }

            }

            int longestTrack =
                dvd.Titles.Single(
                    info => info.VideoStream.Runtime == dvd.Titles.Max(infoLocl => infoLocl.VideoStream.Runtime))
                    .TitleNumber;

            _defaultSelection = longestTrack - 1;
        }

        public string GetVolumeLabel(FileSystemInfo dir)
        {
            uint serialNumber = 0;
            uint maxLength = 0;
            uint volumeFlags = new uint();
            StringBuilder volumeLabel = new StringBuilder(256);
            StringBuilder fileSystemName = new StringBuilder(256);
            string label = string.Empty;

            try
            {
                GetVolumeInformation(dir.Name, volumeLabel, (uint) volumeLabel.Capacity, ref serialNumber, ref maxLength,
                    ref volumeFlags, fileSystemName, (uint) fileSystemName.Capacity);

                label = volumeLabel.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            if (label.Length == 0)
                label = dir.Name;

            return label;
        }

        public StreamTreeNode CreateNode(StreamTreeNode tParent, string tName, object data)
        {
            StreamTreeNode subNode = new StreamTreeNode
            {
                ID = _treeNodeID++,
                Name = tName,
                Data = data,
                IsChecked = tParent.IsChecked,
                IsExpanded = tParent.IsExpanded,
                Parent = tParent,
                Children = new List<StreamTreeNode>()
            };
            subNode.PropertyChanged += TreeNodePropertyChanged;
            tParent.Children.Add(subNode);
           
            return subNode;
        }

        public EncoderProfile GetProfile(string pName, ProfileType pType)
        {
            ProfilesHandler profHandler = new ProfilesHandler(this._configService);
            return profHandler.FilteredList.Find(ep => (ep.Name == pName) && (ep.Type == pType));
        }

        public IEnumerable<StreamTreeNode> GetCheckedItems(StreamTreeNode streamTree)
        {
            List<StreamTreeNode> items = new List<StreamTreeNode>();

            if (streamTree.IsChecked)
                items.Add(streamTree);

            if (streamTree.Children.Count > 0)
                foreach (StreamTreeNode child in streamTree.Children)
                    items.AddRange(GetCheckedItems(child));

            return items;
        }

        public void CheckRootItem(StreamTreeNode item)
        {
            StreamTreeNode iParent = item.Parent;
            StreamTreeNode topParent = null;
            if (iParent != null)
            {
                item.IsChecked = true;
                item.IsExpanded = true;

                iParent.IsChecked = true;
                iParent.IsExpanded = true;

                topParent = iParent.Parent;
            }

            if (topParent != null)
            {
                topParent.IsExpanded = true;
                topParent.IsChecked = true;
            }
        }

        public void CheckSubItems(StreamTreeNode node)
        {
            foreach (StreamTreeNode childNode in node.Children)
            {
                childNode.IsChecked = node.IsChecked;
                childNode.IsExpanded = node.IsChecked;
            }
            node.IsExpanded = node.IsChecked;
        }

        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(
            string pathName,
            StringBuilder volumeNameBuffer,
            uint volumeNameSize,
            ref uint volumeSerialNumber,
            ref uint maximumComponentLength,
            ref uint fileSystemFlags,
            StringBuilder fileSystemNameBuffer,
            uint fileSystemNameSize);

        #endregion
    }
}