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
using System.Globalization;

namespace VideoConvert.Core.Media
{
    public class MediaInfoContainer : IDisposable
    {
        public struct GeneralInfo
        {
            public string CompleteName;
            public string FileName;
            public string FileExtension;
            public string Format;
            public string FormatExtensions;
            public string InternetMediaType;
            public DateTime DurationTime;
            public string Title;
            public string EncodedApplication;
            public string EncodedApplicationUrl;
            public string EncodedLibrary;
            public string EncodedLibraryName;
            public string EncodedLibraryVersion;
            public string EncodedLibraryDate;
            public string EncodedLibrarySettings;
        }

        public struct VideoStreamInfo
        {
            public int StreamKindID;
            public int StreamKindPos;
            public int ID;
            public string Format;
            public string FormatInfo;
            public string FormatVersion;
            public string FormatProfile;
            public string MultiViewBaseProfile;
            public string MultiViewCount;
            public string InternetMediaType;
            public string CodecID;
            public string CodecIDInfo;
            public string CodecIDUrl;
            public string CodecIDDescription;
            public DateTime DurationTime;
            public string BitRateMode;
            public int BitRate;
            public int BitRateMin;
            public int BitRateNom;
            public int BitRateMax;
            public int Width;
            public int Height;
            public string PixelAspectRatio;
            public string DisplayAspectRatio;
            public string FrameRateMode;
            public float FrameRate;
            public int FrameRateEnumerator;
            public int FrameRateDenominator;
            public float FrameRateMin;
            public float FrameRateNom;
            public float FrameRateMax;
            public Int64 FrameCount;
            public int BitDepth;
            public string ScanType;
            public string ScanOrder;
            public ulong StreamSize;
            public string EncodedApplication;
            public string EncodedApplicationUrl;
            public string EncodedLibrary;
            public string EncodedLibraryName;
            public string EncodedLibraryVersion;
            public string EncodedLibraryDate;
            public string EncodedLibrarySettings;
            public VideoFormat VideoSize;
        }

        public struct AudioStreamInfo
        {
            public int StreamKindID;
            public int StreamKindPos;
            public int ID;
            public string Format;
            public string FormatInfo;
            public string FormatVersion;
            public string FormatProfile;
            public string CodecID;
            public string CodecIDInfo;
            public string CodecIDUrl;
            public string CodecIDDescription;
            public Int64 Duration;
            public string BitRateMode;
            public int BitRate;
            public int BitRateMin;
            public int BitRateNom;
            public int BitRateMax;
            public int Channels;
            public string ChannelsString;
            public string ChannelPositions;
            public int SamplingRate;
            public int BitDepth;
            public string CompressionMode;
            public int Delay;
            public ulong StreamSize;
            public string EncodedLibrary;
            public string EncodedLibraryName;
            public string EncodedLibraryVersion;
            public string EncodedLibraryDate;
            public string EncodedLibrarySettings;
            public string LanguageFull;
            public string LanguageIso6391;
            public string LanguageIso6392;
        }

        public struct ImageStreamInfo
        {
            public int StreamKindID;
            public int ID;
            public string Format;
            public string CodecIDInfo;
            public ulong StreamSize;
            public string LanguageFull;
            public string LanguageIso6392;
        }

        public struct TextStreamInfo
        {
            public int StreamKindID;
            public int ID;
            public string Format;
            public string CodecIDInfo;
            public int Delay;
            public ulong StreamSize;
            public string LanguageFull;
            public string LanguageIso6392;
        }

        public struct MenuStreamInfo
        {
            public int ChaptersPosBegin;
            public int ChaptersPosEnd;
        }

        private int _videoStreams;
        private int _audioStreams;
        private int _imageStreams;
        private int _textStreams;
        private int _menuCount;

        public GeneralInfo General;
        public List<VideoStreamInfo> Video;
        public List<AudioStreamInfo> Audio;
        public List<ImageStreamInfo> Image;
        public List<TextStreamInfo> Text;

        public List<TimeSpan> Chapters;


        public MediaInfoContainer()
        {
            _videoStreams = 0;
            _audioStreams = 0;
            General = new GeneralInfo();
            Video = new List<VideoStreamInfo>();
            Audio = new List<AudioStreamInfo>();
            Image = new List<ImageStreamInfo>();
            Text = new List<TextStreamInfo>();
            Chapters = new List<TimeSpan>();
        }

        public void GetMediaInfo(string fileName)
        {
            const NumberStyles numStyle = NumberStyles.Number;

            if (Processing.mediaInfo == null)
            {
                Processing.mediaInfo = new MediaInfo();
                Processing.mediaInfo.Option("Internet", "No");
            }

            Processing.mediaInfo.Open(fileName);
            
            _videoStreams = Processing.mediaInfo.Count_Get(StreamKind.Video);
            _audioStreams = Processing.mediaInfo.Count_Get(StreamKind.Audio);
            _imageStreams = Processing.mediaInfo.Count_Get(StreamKind.Image);
            _textStreams = Processing.mediaInfo.Count_Get(StreamKind.Text);
            _menuCount = Processing.mediaInfo.Count_Get(StreamKind.Menu);

        #region Get General Info
            General.CompleteName        = Processing.mediaInfo.Get(StreamKind.General, 0, "CompleteName");
            General.FileName            = Processing.mediaInfo.Get(StreamKind.General, 0, "FileName");
            General.FileExtension       = Processing.mediaInfo.Get(StreamKind.General, 0, "FileExtension");
            General.Format              = Processing.mediaInfo.Get(StreamKind.General, 0, "Format");
            General.FormatExtensions    = Processing.mediaInfo.Get(StreamKind.General, 0, "Format/Extensions");
            General.InternetMediaType   = Processing.mediaInfo.Get(StreamKind.General, 0, "InternetMediaType");
            DateTime.TryParse(Processing.mediaInfo.Get(StreamKind.General, 0, "Duration/String3"), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out General.DurationTime);
            General.Title               = Processing.mediaInfo.Get(StreamKind.General, 0, "Title");
            General.EncodedApplication  = Processing.mediaInfo.Get(StreamKind.General, 0, "Encoded_Application");
            General.EncodedApplicationUrl = Processing.mediaInfo.Get(StreamKind.General, 0, "Encoded_Application/Url");
            General.EncodedLibrary      = Processing.mediaInfo.Get(StreamKind.General, 0, "Encoded_Library");
            General.EncodedLibraryName  = Processing.mediaInfo.Get(StreamKind.General, 0, "Encoded_Library/Name");
            General.EncodedLibraryVersion = Processing.mediaInfo.Get(StreamKind.General, 0, "Encoded_Library/Version");
            General.EncodedLibraryDate  = Processing.mediaInfo.Get(StreamKind.General, 0, "Encoded_Library/Date");
            General.EncodedLibrarySettings = Processing.mediaInfo.Get(StreamKind.General, 0, "Encoded_Library_Settings");
        #endregion

        #region Get Video Info

            for (int i = 0; i < _videoStreams; i++)
            {
                VideoStreamInfo videoStream = new VideoStreamInfo();

                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "StreamKindID"), numStyle, AppSettings.CInfo, out videoStream.StreamKindID);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "StreamKindPos"), numStyle, AppSettings.CInfo, out videoStream.StreamKindPos);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "ID"), numStyle, AppSettings.CInfo, out videoStream.ID);
                videoStream.Format              = Processing.mediaInfo.Get(StreamKind.Video, i, "Format");
                videoStream.FormatInfo          = Processing.mediaInfo.Get(StreamKind.Video, i, "Format/Info");
                videoStream.FormatVersion       = Processing.mediaInfo.Get(StreamKind.Video, i, "Format_Version");
                videoStream.FormatProfile       = Processing.mediaInfo.Get(StreamKind.Video, i, "Format_Profile");
                videoStream.MultiViewBaseProfile = Processing.mediaInfo.Get(StreamKind.Video, i, "MultiView_BaseProfile");
                videoStream.MultiViewCount      = Processing.mediaInfo.Get(StreamKind.Video, i, "MultiView_Count");
                videoStream.InternetMediaType   = Processing.mediaInfo.Get(StreamKind.Video, i, "InternetMediaType");
                videoStream.CodecID             = Processing.mediaInfo.Get(StreamKind.Video, i, "CodecID");
                videoStream.CodecIDInfo         = Processing.mediaInfo.Get(StreamKind.Video, i, "CodecID/Info");
                videoStream.CodecIDUrl          = Processing.mediaInfo.Get(StreamKind.Video, i, "CodecID/Url");
                videoStream.CodecIDDescription  = Processing.mediaInfo.Get(StreamKind.Video, i, "CodecID_Description");
                DateTime.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "Duration/String3"), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out videoStream.DurationTime);
                videoStream.BitRateMode         = Processing.mediaInfo.Get(StreamKind.Video, i, "BitRate_Mode");
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "BitRate"), numStyle, AppSettings.CInfo, out videoStream.BitRate);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "BitRate_Minimum"), numStyle, AppSettings.CInfo, out videoStream.BitRateMin);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "BitRate_Nominal"), numStyle, AppSettings.CInfo, out videoStream.BitRateNom);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "BitRate_Maximum"), numStyle, AppSettings.CInfo, out videoStream.BitRateMax);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "Width"), numStyle, AppSettings.CInfo, out videoStream.Width);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "Height"), numStyle, AppSettings.CInfo, out videoStream.Height);
                videoStream.PixelAspectRatio    = Processing.mediaInfo.Get(StreamKind.Video, i, "PixelAspectRatio");
                videoStream.DisplayAspectRatio  = Processing.mediaInfo.Get(StreamKind.Video, i, "DisplayAspectRatio");
                videoStream.FrameRateMode       = Processing.mediaInfo.Get(StreamKind.Video, i, "FrameRate_Mode");

                Single.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "FrameRate"), numStyle, AppSettings.CInfo, out videoStream.FrameRate);
                Processing.GetFPSNumDenom(videoStream.FrameRate, out videoStream.FrameRateEnumerator, out videoStream.FrameRateDenominator);

                Single.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "FrameRate_Minimum"), numStyle, AppSettings.CInfo, out videoStream.FrameRateMin);
                Single.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "FrameRate_Nominal"), numStyle, AppSettings.CInfo, out videoStream.FrameRateNom);
                Single.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "FrameRate_Maximum"), numStyle, AppSettings.CInfo, out videoStream.FrameRateMax);
                Int64.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "FrameCount"), numStyle, AppSettings.CInfo, out videoStream.FrameCount);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "BitDepth"), numStyle, AppSettings.CInfo, out videoStream.BitDepth);
                videoStream.ScanType            = Processing.mediaInfo.Get(StreamKind.Video, i, "ScanType");
                videoStream.ScanOrder           = Processing.mediaInfo.Get(StreamKind.Video, i, "ScanOrder");
                UInt64.TryParse(Processing.mediaInfo.Get(StreamKind.Video, i, "StreamSize"), numStyle, AppSettings.CInfo, out videoStream.StreamSize);
                videoStream.EncodedApplication  = Processing.mediaInfo.Get(StreamKind.Video, i, "Encoded_Application");
                videoStream.EncodedApplicationUrl = Processing.mediaInfo.Get(StreamKind.Video, i, "Encoded_Application/Url");
                videoStream.EncodedLibrary      = Processing.mediaInfo.Get(StreamKind.Video, i, "Encoded_Library");
                videoStream.EncodedLibraryName  = Processing.mediaInfo.Get(StreamKind.Video, i, "Encoded_Library/Name");
                videoStream.EncodedLibraryVersion = Processing.mediaInfo.Get(StreamKind.Video, i, "Encoded_Library/Version");
                videoStream.EncodedLibraryDate  = Processing.mediaInfo.Get(StreamKind.Video, i, "Encoded_Library/Date");
                videoStream.EncodedLibrarySettings = Processing.mediaInfo.Get(StreamKind.Video, i, "Encoded_Library_Settings");

                if (videoStream.Width > 1280)
                {
                    if ((videoStream.ScanType == "Progressive") || (videoStream.ScanType == ""))
                        videoStream.VideoSize = VideoFormat.Videoformat1080P;
                    else
                        videoStream.VideoSize = VideoFormat.Videoformat1080I;
                }
                else if (videoStream.Width > 720)
                {
                    videoStream.VideoSize = VideoFormat.Videoformat720P;
                }
                else if ((videoStream.Height > 480) && (videoStream.Height <= 576) && (videoStream.Width <= 720))
                {
                    if ((videoStream.ScanType == "Progressive") || (videoStream.ScanType == ""))
                        videoStream.VideoSize = VideoFormat.Videoformat576P;
                    else
                        videoStream.VideoSize = VideoFormat.Videoformat576I;
                }
                else
                {
                    if ((videoStream.ScanType == "Progressive") || (videoStream.ScanType == ""))
                        videoStream.VideoSize = VideoFormat.Videoformat480P;
                    else
                        videoStream.VideoSize = VideoFormat.Videoformat480I;
                }

                Video.Add(videoStream);
            }
        #endregion

        #region Get Audio Info
            for (int i = 0; i < _audioStreams; i++)
            {
                AudioStreamInfo audioStream = new AudioStreamInfo();

                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "StreamKindID"), numStyle, AppSettings.CInfo, out audioStream.StreamKindID);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "StreamKindPos"), numStyle, AppSettings.CInfo, out audioStream.StreamKindPos);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "ID"), numStyle, AppSettings.CInfo, out audioStream.ID);
                audioStream.Format              = Processing.mediaInfo.Get(StreamKind.Audio, i, "Format");
                audioStream.FormatInfo          = Processing.mediaInfo.Get(StreamKind.Audio, i, "Format/Info");
                audioStream.FormatVersion       = Processing.mediaInfo.Get(StreamKind.Audio, i, "Format_Version");
                audioStream.FormatProfile       = Processing.mediaInfo.Get(StreamKind.Audio, i, "Format_Profile");
                audioStream.CodecID             = Processing.mediaInfo.Get(StreamKind.Audio, i, "CodecID");
                audioStream.CodecIDInfo         = Processing.mediaInfo.Get(StreamKind.Audio, i, "CodecID/Info");
                audioStream.CodecIDUrl          = Processing.mediaInfo.Get(StreamKind.Audio, i, "CodecID/Url");
                audioStream.CodecIDDescription  = Processing.mediaInfo.Get(StreamKind.Audio, i, "CodecID_Description");
                Int64.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "Duration"), numStyle, AppSettings.CInfo, out audioStream.Duration);
                audioStream.BitRateMode         = Processing.mediaInfo.Get(StreamKind.Audio, i, "BitRate_Mode");
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "BitRate"), numStyle, AppSettings.CInfo, out audioStream.BitRate);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "BitRate_Minimum"), numStyle, AppSettings.CInfo, out audioStream.BitRateMin);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "BitRate_Nominal"), numStyle, AppSettings.CInfo, out audioStream.BitRateNom);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "BitRate_Maximum"), numStyle, AppSettings.CInfo, out audioStream.BitRateMax);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "Channel(s)"), numStyle, AppSettings.CInfo, out audioStream.Channels);
                audioStream.ChannelsString      = Processing.mediaInfo.Get(StreamKind.Audio, i, "Channel(s)/String");
                audioStream.ChannelPositions    = Processing.mediaInfo.Get(StreamKind.Audio, i, "ChannelPositions");
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "SamplingRate"), numStyle, AppSettings.CInfo, out audioStream.SamplingRate);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "BitDepth"), numStyle, AppSettings.CInfo, out audioStream.BitDepth);
                audioStream.CompressionMode     = Processing.mediaInfo.Get(StreamKind.Audio, i, "Compression_Mode");
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "Delay"), numStyle, AppSettings.CInfo, out audioStream.Delay);
                UInt64.TryParse(Processing.mediaInfo.Get(StreamKind.Audio, i, "StreamSize"), numStyle, AppSettings.CInfo, out audioStream.StreamSize);
                audioStream.EncodedLibrary      = Processing.mediaInfo.Get(StreamKind.Audio, i, "Encoded_Library");
                audioStream.EncodedLibraryName  = Processing.mediaInfo.Get(StreamKind.Audio, i, "Encoded_Library/Name");
                audioStream.EncodedLibraryVersion = Processing.mediaInfo.Get(StreamKind.Audio, i, "Encoded_Library/Version");
                audioStream.EncodedLibraryDate  = Processing.mediaInfo.Get(StreamKind.Audio, i, "Encoded_Library/Date");
                audioStream.EncodedLibrarySettings = Processing.mediaInfo.Get(StreamKind.Audio, i, "Encoded_Library_Settings");
                audioStream.LanguageFull        = Processing.mediaInfo.Get(StreamKind.Audio, i, "Language/String1");
                audioStream.LanguageIso6391     = Processing.mediaInfo.Get(StreamKind.Audio, i, "Language/String2");
                audioStream.LanguageIso6392     = Processing.mediaInfo.Get(StreamKind.Audio, i, "Language/String3");

                Audio.Add(audioStream);
            }
        #endregion

        #region Get Image Info
            for (int i = 0; i < _imageStreams; i++)
            {
                ImageStreamInfo imageStream = new ImageStreamInfo();

                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Image, i, "StreamKindID"), numStyle, AppSettings.CInfo, out imageStream.StreamKindID);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Image, i, "ID"), numStyle, AppSettings.CInfo, out imageStream.ID);
                imageStream.Format                  = Processing.mediaInfo.Get(StreamKind.Image, i, "Format");
                imageStream.CodecIDInfo             = Processing.mediaInfo.Get(StreamKind.Image, i, "CodecID/Info");
                UInt64.TryParse(Processing.mediaInfo.Get(StreamKind.Image, i, "StreamSize"), numStyle, AppSettings.CInfo, out imageStream.StreamSize);
                imageStream.LanguageFull            = Processing.mediaInfo.Get(StreamKind.Image, i, "Language/String1");
                imageStream.LanguageIso6392         = Processing.mediaInfo.Get(StreamKind.Image, i, "Language/String3");

                Image.Add(imageStream);
            }
        #endregion

        #region Get Text Info
            for (int i = 0; i < _textStreams; i++)
            {
                TextStreamInfo textStream = new TextStreamInfo();

                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Text, i, "StreamKindID"), numStyle, AppSettings.CInfo, out textStream.StreamKindID);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Text, i, "ID"), numStyle, AppSettings.CInfo, out textStream.ID);
                textStream.Format                   = Processing.mediaInfo.Get(StreamKind.Text, i, "Format");
                textStream.CodecIDInfo              = Processing.mediaInfo.Get(StreamKind.Text, i, "CodecID/Info");
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Text, i, "Delay"), numStyle, AppSettings.CInfo, out textStream.Delay);
                UInt64.TryParse(Processing.mediaInfo.Get(StreamKind.Text, i, "StreamSize"), numStyle, AppSettings.CInfo, out textStream.StreamSize);
                textStream.LanguageFull             = Processing.mediaInfo.Get(StreamKind.Text, i, "Language/String1");
                textStream.LanguageIso6392          = Processing.mediaInfo.Get(StreamKind.Text, i, "Language/String3");

                Text.Add(textStream);
            }
        #endregion

        #region Get Menu Info
            for (int i = 0; i < _menuCount; i++)
            {
                MenuStreamInfo menuStream = new MenuStreamInfo();
                
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Menu, i, "Chapters_Pos_Begin"), numStyle, AppSettings.CInfo, out menuStream.ChaptersPosBegin);
                Int32.TryParse(Processing.mediaInfo.Get(StreamKind.Menu, i, "Chapters_Pos_End"), numStyle, AppSettings.CInfo, out menuStream.ChaptersPosEnd);

                for (int j = menuStream.ChaptersPosBegin; j < menuStream.ChaptersPosEnd; j++)
                {
                    DateTime tempTime;
                    DateTime.TryParse(Processing.mediaInfo.Get(StreamKind.Menu, i, j, InfoKind.Name), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out tempTime);
                    Chapters.Add(tempTime.TimeOfDay);
                }
            }
        #endregion
            Processing.mediaInfo.Option("Complete");
            Processing.mediaInfo.Close();
        }

        public void Dispose()
        {
            _videoStreams = 0;
            _audioStreams = 0;
            Video = new List<VideoStreamInfo>();
            Audio = new List<AudioStreamInfo>();
            Image = new List<ImageStreamInfo>();
            Text = new List<TextStreamInfo>();
            Chapters = new List<TimeSpan>();
            General = new GeneralInfo();
        }
    }
}