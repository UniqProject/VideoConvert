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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using BDInfo;
using CheckBoxTreeViewLibrary;
using System.Xml;
using System.Text;
using System.Runtime.InteropServices;
using VideoConvert.Core;
using VideoConvert.Core.Encoder;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Media;
using VideoConvert.Core.Profiles;
using VideoConvert.Windows.TheMovieDB;
using log4net;

namespace VideoConvert.Windows
{
    /// <summary>
    /// Interaktionslogik für SingleFileSelect.xaml
    /// </summary>
    public partial class StreamSelect
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StreamSelect));

        ProfilesHandler _profiles;

        public EncodeInfo JobInfo { get; set; }

        readonly List<TreeNode> _tree = new List<TreeNode>();
        public List<TreeNode> Tree { get { return _tree; } }

        private int _treeNodeID;
        private BDROM _bdInfo;
        private int _defaultSelection;

        private MovieEntry _resultMovieData;
        private EpisodeEntry _resultEpisodeData;

        public StreamSelect()
        {
            InitializeComponent();
            _defaultSelection = 0;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            DisableSubtitleOptions();
            DisableAudioOptions();
            LoadStreams();
        }

        private void LoadStreams()
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

        #region background profile loading

        void ProfilesWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((_profiles.ProfileList != null) && (_profiles.ProfileList.Count > 0))
            {
                EncodingProfile.ItemsSource = from p in _profiles.FilteredList
                                              where p.Type == ProfileType.QuickSelect
                                              select p;
                if (string.IsNullOrEmpty(AppSettings.LastSelectedProfile))
                    EncodingProfile.SelectedIndex = 0;
                else
                    EncodingProfile.SelectedValue = AppSettings.LastSelectedProfile;
            }
            ((BackgroundWorker) sender).Dispose();
        }

        void ProfilesWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            _profiles = new ProfilesHandler();
        }
        #endregion

        #region background stream info loading

        void BgWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (JobInfo.Input == InputType.InputBluRay)
            {
                FileTitle.Text = _bdInfo.VolumeLabel;
            }
            else if (JobInfo.Input != InputType.InputDvd)
            {
                FileTitle.Text = JobInfo.MediaInfo.General.Title.Length > 0
                                     ? JobInfo.MediaInfo.General.Title
                                     : JobInfo.MediaInfo.General.FileName;
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(JobInfo.InputFile);
                FileTitle.Text = GetVolumeLabel(dir);

                if (FileTitle.Text.Length == 0)
                {
                    FileTitle.Text = Path.GetFileName(JobInfo.InputFile);
                }
            }

            SelectedTitle.ItemsSource = Tree;

            if (_defaultSelection > -1)
                SelectedTitle.SelectedIndex = _defaultSelection;

            ((BackgroundWorker) sender).Dispose();
        }

        void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            switch (JobInfo.Input)
            {
                case InputType.InputUndefined:
                    DialogResult = false;
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

        private void GetFileInfo()
        {
            MediaInfoContainer mi = new MediaInfoContainer();
            try
            {
                mi = Processing.GetMediaInfo(JobInfo.InputFile);
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex);
            }
            
            JobInfo.MediaInfo = mi;

            string fileTitleFormat = Processing.GetResourceString("streamselect_single_file_general");
            string fileAudioFormat = Processing.GetResourceString("streamselect_single_file_audio");
            string fileVideoFormat = Processing.GetResourceString("streamselect_single_file_video");
            string strChapters = Processing.GetResourceString("streamselect_chapters");
            string strVideo = Processing.GetResourceString("streamselect_video");
            string strAudio = Processing.GetResourceString("streamselect_audio");
            string strSubtitles = Processing.GetResourceString("streamselect_subtitles");

            string containerFormat = mi.General.Format;
            string duration = mi.General.DurationTime.ToString("H:mm:ss.fff");
            string shortFileName = mi.General.FileName + "." + mi.General.FileExtension;

            string treeRoot = string.Format(fileTitleFormat, shortFileName, containerFormat, duration);

            TreeNode root = new TreeNode
                {
                    ID = _treeNodeID++,
                    Name = treeRoot,
                    Data = JobInfo.InputFile,
                    IsChecked = true,
                    IsExpanded = true,
                    Children = new List<TreeNode>()
                };
            _tree.Add(root);

            TreeNode chaptersTree = CreateNode(root, strChapters, null);
            TreeNode videoTree = CreateNode(root, strVideo, null);
            TreeNode audioTree = CreateNode(root, strAudio, null);
            TreeNode subTree = CreateNode(root, strSubtitles, null);

            if (mi.Chapters.Count > 0)
            {
                string chaptersTitle = string.Format("{0:0} {1}", mi.Chapters.Count, strChapters);

                CreateNode(chaptersTree, chaptersTitle, mi.Chapters);
            }
            else
                chaptersTree.IsChecked = false;

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
                Single.TryParse(clip.DisplayAspectRatio, NumberStyles.Number, AppSettings.CInfo, out vid.AspectRatio);
                vid.FrameRateEnumerator = clip.FrameRateEnumerator;
                vid.FrameRateDenominator = clip.FrameRateDenominator;

                CreateNode(videoTree, videoStreamTitle, vid);
            }

            videoTree.IsChecked = videoTree.Children.Count > 0;

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
                                                 audio.SamplingRate, audio.BitDepth, audio.BitRate/1000);

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

                CreateNode(audioTree, audioStreamTitle, aud);
            }

            audioTree.IsChecked = audioTree.Children.Count > 0;

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

                CreateNode(subTree, subStreamTitle, subInfo);
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

                CreateNode(subTree, subStreamTitle, subInfo);
            }

            subTree.IsChecked = subTree.Children.Count > 0;

        }

        private void GetBDInfo()
        {
            string strChapters = Processing.GetResourceString("streamselect_chapters");
            string strVideo = Processing.GetResourceString("streamselect_video");
            string strAudio = Processing.GetResourceString("streamselect_audio");
            string strSubtitles = Processing.GetResourceString("streamselect_subtitles");

            string bdTitleFormat = Processing.GetResourceString("streamselect_bd_general");
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

                TreeNode root = new TreeNode
                    {
                        ID = _treeNodeID++,
                        Name = treeRoot,
                        Data = treeData,
                        Children = new List<TreeNode>(),
                        IsChecked = true,
                    };
                root.IsExpanded = root.IsChecked;
                _tree.Add(root);

                TreeNode chaptersTree = CreateNode(root, strChapters, null);
                TreeNode videoTree = CreateNode(root, strVideo, null);
                TreeNode audioTree = CreateNode(root, strAudio, null);
                TreeNode subTree = CreateNode(root, strSubtitles, null);

                List<TimeSpan> streamChapters = new List<TimeSpan>();
                if (item.Chapters.Count > 1)
                {
                    streamIndex++;

                    streamChapters.AddRange(item.Chapters.Select(TimeSpan.FromSeconds));

                    string chaptersFormat = string.Format("{0:0} {1}", streamChapters.Count, strChapters);

                    CreateNode(chaptersTree, chaptersFormat, streamChapters);
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
                    string videoStreamFormat =  string.Format("{3:g}: {0} ({1}), {2}", videoCodec, videoCodecShort,
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
                                        FPS = (float) clip.FrameRateEnumerator/clip.FrameRateDenominator,
                                        PicSize = (VideoFormat) clip.VideoFormat,
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
                                               AppSettings.CInfo, out vid.DemuxPlayList);

                                foreach (TSStreamClip streamClip in item.StreamClips)
                                    vid.DemuxStreamNames.Add(streamClip.StreamFile.FileInfo.FullName);

                                float mod;
                                switch (clip.AspectRatio)
                                {
                                    case TSAspectRatio.ASPECT_16_9:
                                        mod = (float) 1.777778;
                                        break;
                                    default:
                                        mod = (float) 1.333333;
                                        break;
                                }
                                vid.Width = (int) (vid.Height*mod);
                                vid.AspectRatio = mod;

                                CreateNode(videoTree, videoStreamFormat, vid);
                            }
                            break;
                        case TSStreamType.MVC_VIDEO:
                            {
                                StereoVideoInfo vid = new StereoVideoInfo
                                    {
                                        RightStreamId = streamIndex,
                                        LeftStreamId = leftVideoStreamID
                                    };
                                CreateNode(videoTree, videoStreamFormat, vid);
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

                    CreateNode(audioTree, audioStreamFormat, aud);
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

                    CreateNode(subTree, subStreamFormat, subInfo);
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

                    CreateNode(subTree, subStreamFormat, subInfo);
                }
                playlistIndex++;
            }
            _defaultSelection = longestClip - 1;
        }

        private int GetLongestBDPlaylist()
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

        private void GetDVDTitleList()
        {
            string strChapters = Processing.GetResourceString("streamselect_chapters");
            string strVideo = Processing.GetResourceString("streamselect_video");
            string strAudio = Processing.GetResourceString("streamselect_audio");
            string strSubtitles = Processing.GetResourceString("streamselect_subtitles");

            string dvdTitleFormat = Processing.GetResourceString("streamselect_dvd_general");
            string dvdAudioFormat = Processing.GetResourceString("streamselect_dvd_audio");
            string dvdSubFormat = Processing.GetResourceString("streamselect_dvd_subtitle");

            const string dvdVideoStreamFormat = "MPEG-2 {1:g}x{2:g} {4}{5} {0} {3:f} fps";

            LsDvd lsdvd = new LsDvd();
            string lines = lsdvd.GetDvdInfo(JobInfo.InputFile);

            XmlDocument dvdInfo = new XmlDocument();
            dvdInfo.LoadXml(lines);

            if (dvdInfo.DocumentElement == null) return;

            XmlNodeList tracks = dvdInfo.DocumentElement.SelectNodes("//track");
            XmlNode longestTrack = dvdInfo.DocumentElement.SelectSingleNode("//longest_track");

            if (longestTrack == null) return;
            if (tracks == null) return;

            int longest = Convert.ToInt32(longestTrack.InnerText);

            foreach (XmlNode track in tracks)
            {
                int videoId = 0;
                float fps = 0;
                string videoFormat = string.Empty;
                string aspect = string.Empty;
                int width = 0;
                int height = 0;
                string letterboxed = string.Empty;
                int vtsID = 0;
                float lengthTemp = 0;

                XmlNode tempNode = track.SelectSingleNode("ix");
                if (tempNode != null) 
                    videoId = Convert.ToInt32(tempNode.InnerText);

                tempNode = track.SelectSingleNode("fps");
                if (tempNode != null) 
                    fps = Convert.ToSingle(tempNode.InnerText, AppSettings.CInfo);

                tempNode = track.SelectSingleNode("format");
                if (tempNode != null) 
                    videoFormat = tempNode.InnerText;

                tempNode = track.SelectSingleNode("aspect");
                if (tempNode != null) 
                    aspect = tempNode.InnerText;

                tempNode = track.SelectSingleNode("width");
                if (tempNode != null) 
                    width = Convert.ToInt32(tempNode.InnerText);

                tempNode = track.SelectSingleNode("height");
                if (tempNode != null) 
                    height = Convert.ToInt32(tempNode.InnerText);

                tempNode = track.SelectSingleNode("df");
                if (tempNode != null)
                {
                    string df = tempNode.InnerText;
                    letterboxed = df == "?" ? string.Empty : " (" + df + ")";
                }

                tempNode = track.SelectSingleNode("vts");
                if (tempNode != null)
                    vtsID = Convert.ToInt32(tempNode.InnerText);

                tempNode = track.SelectSingleNode("length");
                if (tempNode != null)
                    lengthTemp = Convert.ToSingle(tempNode.InnerText, AppSettings.CInfo);

                TimeSpan length = new TimeSpan(0, 0, 0, (int) Math.Truncate(lengthTemp),
                                               (int) ((lengthTemp - Math.Truncate(lengthTemp))*1000));

                XmlNodeList audioTracks = track.SelectNodes("audio");
                XmlNodeList chapters = track.SelectNodes("chapter");
                XmlNodeList subtitles = track.SelectNodes("subp");

                List<TimeSpan> tempChapters = new List<TimeSpan>();

                DateTime duration = new DateTime();
                duration = duration.AddSeconds(length.TotalSeconds);

                string treeRoot = string.Format(dvdTitleFormat, videoId, duration.ToString("H:mm:ss.fff"));

                Dictionary<string, object> treeData = new Dictionary<string, object>
                                                          {{"Name", JobInfo.InputFile}, {"TrackID", videoId}};

                TreeNode root = new TreeNode
                    {
                        ID = _treeNodeID++,
                        Name = treeRoot,
                        Data = treeData,
                        Children = new List<TreeNode>(),
                        IsChecked = true,
                    };
                root.IsExpanded = root.IsChecked;
                _tree.Add(root);

                TreeNode chaptersTree = CreateNode(root, strChapters, null);
                TreeNode videoTree = CreateNode(root, strVideo, null);
                TreeNode audioTree = CreateNode(root, strAudio, null);
                TreeNode subTree = CreateNode(root, strSubtitles, null);

                if (chapters != null && chapters.Count > 0)
                {
                    TimeSpan chapLength = new TimeSpan(0, 0, 0, 0, 0);
                    tempChapters.Add(chapLength);
                }
                if (chapters != null)
                    tempChapters.AddRange(
                        chapters.Cast<XmlNode>().Select(
                            chapterTemp =>
                                {
                                    XmlNode node = chapterTemp.SelectSingleNode("length");
                                    return node != null ? Convert.ToSingle(node.InnerText, AppSettings.CInfo) : 0;
                                }).Select(
                                    chapterLengthTemp =>
                                    new TimeSpan(0, 0, 0, (int) Math.Truncate(chapterLengthTemp),
                                                 (int) ((chapterLengthTemp - Math.Truncate(chapterLengthTemp))*1000))));

                if (tempChapters.Count > 0)
                {
                    string chaptersFormat = string.Format("{0:0} {1}", tempChapters.Count, strChapters);
                    CreateNode(chaptersTree, chaptersFormat, tempChapters);
                }

                string videoStream = string.Format(dvdVideoStreamFormat, videoFormat, width, height, fps, aspect,
                                                   letterboxed);

                VideoInfo vid = new VideoInfo
                    {
                        VtsId = vtsID,
                        TrackId = videoId,
                        StreamId = 1,
                        FPS = fps,
                        Interlaced = true,
                        Format = "MPEG-2",
                        FrameCount = 0,
                        Width = width,
                        Height = height,
                        Encoded = false,
                        IsRawStream = false,
                        DemuxStreamNames = new List<string>(),
                        StreamSize = 0,
                        Length = lengthTemp
                    };

                if (aspect != "4/3")
                    Single.TryParse(aspect, NumberStyles.Number, AppSettings.CInfo, out vid.AspectRatio);
                else
                    vid.AspectRatio = 4f / 3f;

                Processing.GetFPSNumDenom(fps, out vid.FrameRateEnumerator, out vid.FrameRateDenominator);

                CreateNode(videoTree, videoStream, vid);

                int audioTracksCount = 0;

                if (audioTracks != null)
                {
                    audioTracksCount = audioTracks.Count;
                    foreach (XmlNode audio in audioTracks)
                    {
                        int audioID = 0;
                        string langCode = string.Empty;
                        string language = string.Empty;
                        string format = string.Empty;
                        int frequency = 0;
                        string quantization = string.Empty;
                        int channels = 0;
                        string content = string.Empty;
                        int streamID = 0;

                        tempNode = audio.SelectSingleNode("ix");
                        if (tempNode != null)
                            audioID = Convert.ToInt32(tempNode.InnerText);

                        tempNode = audio.SelectSingleNode("langcode");
                        if (tempNode != null)
                            langCode = tempNode.InnerText;

                        tempNode = audio.SelectSingleNode("language");
                        if (tempNode != null)
                            language = tempNode.InnerText;

                        tempNode = audio.SelectSingleNode("format");
                        if (tempNode != null)
                            format = tempNode.InnerText;

                        tempNode = audio.SelectSingleNode("frequency");
                        if (tempNode != null)
                            frequency = Convert.ToInt32(tempNode.InnerText);

                        tempNode = audio.SelectSingleNode("quantization");
                        if (tempNode != null)
                            quantization = tempNode.InnerText;

                        tempNode = audio.SelectSingleNode("channels");
                        if (tempNode != null)
                            channels = Convert.ToInt32(tempNode.InnerText);

                        tempNode = audio.SelectSingleNode("content");
                        if (tempNode != null)
                            content = tempNode.InnerText;

                        tempNode = audio.SelectSingleNode("streamid");
                        if (tempNode != null)
                            streamID = Int32.Parse(tempNode.InnerText.Replace("0x", string.Empty),
                                                   NumberStyles.HexNumber);

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
                }

                if (subtitles == null) continue;
                foreach (XmlNode subtitle in subtitles)
                {
                    int subID = 0;
                    string langCode = string.Empty;
                    string language = string.Empty;
                    string content = string.Empty;
                    int streamID = 0;

                    tempNode = subtitle.SelectSingleNode("ix");
                    if (tempNode != null) 
                        subID = Convert.ToInt32(tempNode.InnerText);

                    tempNode = subtitle.SelectSingleNode("langcode");
                    if (tempNode != null) 
                        langCode = tempNode.InnerText;

                    tempNode = subtitle.SelectSingleNode("language");
                    if (tempNode != null) 
                        language = tempNode.InnerText;

                    tempNode = subtitle.SelectSingleNode("content");
                    if (tempNode != null) 
                        content = tempNode.InnerText;

                    tempNode = subtitle.SelectSingleNode("streamid");
                    if (tempNode != null)
                        streamID = Int32.Parse(tempNode.InnerText.Replace("0x", string.Empty),
                                               NumberStyles.HexNumber);

                    string subtitleStream = string.Format(dvdSubFormat, subID, streamID, langCode, language, content);

                    SubtitleInfo subInfo = new SubtitleInfo
                        {
                            Id = subID + audioTracksCount,
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
            _defaultSelection = longest - 1;
        }

        private string GetVolumeLabel(FileSystemInfo dir)
        {
            uint serialNumber = 0;
            uint maxLength = 0;
            uint volumeFlags = new uint();
            StringBuilder volumeLabel = new StringBuilder(256);
            StringBuilder fileSystemName = new StringBuilder(256);
            string label = string.Empty;

            try
            {
                GetVolumeInformation(
                    dir.Name,
                    volumeLabel,
                    (uint) volumeLabel.Capacity,
                    ref serialNumber,
                    ref maxLength,
                    ref volumeFlags,
                    fileSystemName,
                    (uint) fileSystemName.Capacity);

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

        private TreeNode CreateNode(TreeNode tParent, string tName, object data)
        {
            TreeNode subNode = new TreeNode
                {
                    ID = _treeNodeID++,
                    Name = tName,
                    Data = data,
                    IsChecked = tParent.IsChecked,
                    IsExpanded = tParent.IsExpanded,
                    Parent = tParent,
                    Children = new List<TreeNode>()
                };
            tParent.Children.Add(subNode);
            return subNode;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            if (SelectedTitle.SelectedIndex < 0) return;
            if (EncodingProfile.SelectedIndex < 0) return;

            JobInfo.JobName = FileTitle.Text;

            TreeNode selected = (TreeNode)SelectedTitle.SelectedItem;

            IEnumerable<TreeNode> sortedList = GetCheckedItems(selected);

            bool videoSet = false;

            foreach (TreeNode item in sortedList)
            {
                if (item.Data == null) continue;

                Type dataType = item.Data.GetType();

                if (dataType == typeof (string))
                    JobInfo.InputFile = (string) item.Data;
                else if (dataType == typeof (VideoInfo))
                {
                    if (!videoSet)
                    {
                        JobInfo.VideoStream = (VideoInfo) item.Data;
                        videoSet = true;
                    }
                }
                else if (dataType == typeof (StereoVideoInfo))
                    JobInfo.StereoVideoStream = (StereoVideoInfo) item.Data;
                else if (dataType == typeof (AudioInfo))
                    JobInfo.AudioStreams.Add((AudioInfo) item.Data);
                else if (dataType == typeof (SubtitleInfo))
                {
                    SubtitleInfo sub = (SubtitleInfo) item.Data;
                    bool isBD = JobInfo.Input == InputType.InputBluRay || JobInfo.Input == InputType.InputAvchd ||
                                JobInfo.Input == InputType.InputHddvd;
                    if ((sub.Format == "PGS" || sub.Format == "VobSub") && ((isBD && _bdInfo != null && !_bdInfo.Is3D) || !isBD))
                        // don't extract subtitles on 3d blurays, because eac3to can't handle them
                        JobInfo.SubtitleStreams.Add(sub);
                }
                else if (dataType == typeof (List<TimeSpan>))
                    JobInfo.Chapters.AddRange((List<TimeSpan>) item.Data);
                else if (dataType == typeof (Dictionary<string, object>))
                {
                    object itemData;
                    (item.Data as Dictionary<string, object>).TryGetValue("Name", out itemData);
                    if (itemData != null)
                        JobInfo.InputFile = (string) itemData;
                    (item.Data as Dictionary<string, object>).TryGetValue("PlaylistIndex", out itemData);
                    if (itemData != null)
                        JobInfo.StreamId = (int) itemData;
                    (item.Data as Dictionary<string, object>).TryGetValue("TrackID", out itemData);
                    if (itemData != null)
                        JobInfo.TrackId = (int) itemData;
                }
            }
            JobInfo.StreamId = -1;

            if (_profiles != null)
            {
                QuickSelectProfile encProfile = (QuickSelectProfile)EncodingProfile.SelectedItem;
                EncoderProfile videoEncoder = GetProfile(encProfile.VideoProfile, encProfile.VideoProfileType);
                EncoderProfile audioEncoder = GetProfile(encProfile.AudioProfile, encProfile.AudioProfileType);
                JobInfo.AudioProfile = audioEncoder;
                JobInfo.VideoProfile = videoEncoder;
                JobInfo.EncodingProfile = encProfile;
            }

            AppSettings.LastSelectedProfile = (string)EncodingProfile.SelectedValue;
            AppSettings.SaveSettings();

            if (AppSettings.CreateXbmcInfoFile)
            {
                if (_resultMovieData != null)
                    JobInfo.MovieInfo = _resultMovieData;
                else if (_resultEpisodeData != null)
                    JobInfo.EpisodeInfo = _resultEpisodeData;
            }

            _bdInfo = null;
            DialogResult = true;
        }

        private EncoderProfile GetProfile(string pName, ProfileType pType)
        {
            return _profiles.ProfileList.Find(ep => (ep.Name == pName) && (ep.Type == pType));
        }

        private static IEnumerable<TreeNode> GetCheckedItems(TreeNode tree)
        {
            List<TreeNode> items = new List<TreeNode>();

            if (tree.IsChecked)
                items.Add(tree);

            if (tree.Children.Count > 0)
                foreach (TreeNode child in tree.Children)
                    items.AddRange(GetCheckedItems(child));

            return items;
        }

        private void CheckItemChanged(object sender, RoutedEventArgs e)
        {
            if (e.Source == null) return;

            CheckBoxTreeViewItem selectedNode = e.Source as CheckBoxTreeViewItem;
            if (selectedNode != null)
            {
                TreeNode item = (TreeNode)selectedNode.Header;

                if (String.CompareOrdinal(e.RoutedEvent.Name, "Unchecked") == 0)
                {
                    if (item != null)
                    {
                        UnCheckItems(item);
                        selectedNode.Items.Refresh();
                    }
                }
                else if (String.CompareOrdinal(e.RoutedEvent.Name, "Checked") == 0)
                {
                    if (item != null)
                    {
                        CheckRootItem(item);
                        TitleInfo.Items.Refresh();
                    }
                }
            }
        }

        private void CheckRootItem(TreeNode item)
        {
            
            TreeNode iParent = item.Parent;
            TreeNode topParent = null;
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

        private void UnCheckItems(TreeNode node)
        {
            foreach (TreeNode childNode in node.Children)
            {
                foreach (TreeNode subChildNode in childNode.Children)
                {
                    subChildNode.IsChecked = false;
                    subChildNode.IsExpanded = false;
                }
                childNode.IsChecked = false;
                childNode.IsExpanded = false;
            }
            node.IsChecked = false;
            node.IsExpanded = false;
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

        private void SelectedTitleSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            TitleInfo.ItemsSource = ((TreeNode) SelectedTitle.SelectedItem).Children;
        }

        private void TitleInfoSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeNode selectedNode = TitleInfo.SelectedItem as TreeNode;

            if (selectedNode == null) return;
            if (selectedNode.Data == null)
            {
                DisableSubtitleOptions();
                DisableAudioOptions();
                return;
            }

            Type typeData = selectedNode.Data.GetType();
            if (typeData == typeof (SubtitleInfo))
            {
                EnableSubtitleOptions();
                DisableAudioOptions();

                SubtitleInfo subtitleInfo = (SubtitleInfo) selectedNode.Data;

                KeepOnlyForcedCaptions.IsChecked = subtitleInfo.KeepOnlyForcedCaptions;
                HardSubIntoVideo.IsChecked = subtitleInfo.HardSubIntoVideo;
                SubMKVDefault.IsChecked = subtitleInfo.MkvDefault;
            }
            else if (typeData == typeof(AudioInfo))
            {
                EnableAudioOptions();
                DisableSubtitleOptions();
                AudioInfo audioInfo = (AudioInfo) selectedNode.Data;

                AudioMKVDefault.IsChecked = audioInfo.MkvDefault;
            }
            else
            {
                DisableSubtitleOptions();
                DisableAudioOptions();
            }
        }

        private void EnableSubtitleOptions()
        {
            SubtitleOptionGroup.IsEnabled = true;
            SubtitleOptionGroup.Visibility = Visibility.Visible;
        }

        private void DisableSubtitleOptions()
        {
            SubtitleOptionGroup.IsEnabled = false;
            SubtitleOptionGroup.Visibility = Visibility.Hidden;
        }

        private void EnableAudioOptions()
        {
            AudioOptionGroup.IsEnabled = true;
            AudioOptionGroup.Visibility = Visibility.Visible;
        }

        private void DisableAudioOptions()
        {
            AudioOptionGroup.IsEnabled = false;
            AudioOptionGroup.Visibility = Visibility.Hidden;
        }

        private void KeepOnlyForcedCaptionsChecked(object sender, RoutedEventArgs e)
        {
            TreeNode selectedNode = TitleInfo.SelectedItem as TreeNode;

            if (selectedNode == null) return;
            if (selectedNode.Data == null) return;
            SubtitleInfo subtitleInfo = (SubtitleInfo)selectedNode.Data;

            subtitleInfo.KeepOnlyForcedCaptions = KeepOnlyForcedCaptions.IsChecked.GetValueOrDefault();
        }

        private void HardSubIntoVideoChecked(object sender, RoutedEventArgs e)
        {
            TreeNode selectedNode = TitleInfo.SelectedItem as TreeNode;

            if (selectedNode == null) return;
            if (selectedNode.Data == null) return;
            SubtitleInfo subtitleInfo = (SubtitleInfo)selectedNode.Data;

            subtitleInfo.HardSubIntoVideo = HardSubIntoVideo.IsChecked.GetValueOrDefault();
        }

        private void SubMKVDefaultChecked(object sender, RoutedEventArgs e)
        {
            TreeNode selectedNode = TitleInfo.SelectedItem as TreeNode;

            if (selectedNode == null) return;
            if (selectedNode.Data == null) return;
            SubtitleInfo subtitleInfo = (SubtitleInfo)selectedNode.Data;

            subtitleInfo.MkvDefault = SubMKVDefault.IsChecked.GetValueOrDefault();
        }

        private void AudioMKVDefaultChecked(object sender, RoutedEventArgs e)
        {
            TreeNode selectedNode = TitleInfo.SelectedItem as TreeNode;

            if (selectedNode == null) return;
            if (selectedNode.Data == null) return;
            AudioInfo audioInfo = (AudioInfo)selectedNode.Data;

            audioInfo.MkvDefault = AudioMKVDefault.IsChecked.GetValueOrDefault();
        }

        private void XbmcMediaInfo_Click(object sender, RoutedEventArgs e)
        {
            using (DBInfoWindow dbInfoWindow = new DBInfoWindow {SearchString = FileTitle.Text, Owner = this})
            {
                if (dbInfoWindow.ShowDialog() != true) return;

                if (dbInfoWindow.ResultMovieData != null || dbInfoWindow.ResultEpisodeData != null)
                    FileTitle.Text = dbInfoWindow.SearchString;

                if (dbInfoWindow.ResultMovieData != null)
                {
                    _resultMovieData = dbInfoWindow.ResultMovieData;
                    return;
                }

                if (dbInfoWindow.ResultEpisodeData != null)
                    _resultEpisodeData = dbInfoWindow.ResultEpisodeData;
            }
        }
    }
}
