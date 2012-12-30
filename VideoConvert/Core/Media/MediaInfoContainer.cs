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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace VideoConvert.Core.Media
{
    public class MediaInfoContainer : CollectionBase
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
            Video = new List<VideoStreamInfo>();
            Audio = new List<AudioStreamInfo>();
            Image = new List<ImageStreamInfo>();
            Text = new List<TextStreamInfo>();
            Chapters = new List<TimeSpan>();
        }

        public void GetMediaInfo(string fileName)
        {
            const NumberStyles numStyle = NumberStyles.Number;

            MediaInfo mi = new MediaInfo();
            mi.Open(fileName);

            mi.Option("Complete_Get");

            _videoStreams = mi.CountGet(StreamKind.Video);
            _audioStreams = mi.CountGet(StreamKind.Audio);
            _imageStreams = mi.CountGet(StreamKind.Image);
            _textStreams = mi.CountGet(StreamKind.Text);
            _menuCount = mi.CountGet(StreamKind.Menu);

        #region Get General Info
            General.CompleteName        = mi.Get(StreamKind.General, 0, "CompleteName");
            General.FileName            = mi.Get(StreamKind.General, 0, "FileName");
            General.FileExtension       = mi.Get(StreamKind.General, 0, "FileExtension");
            General.Format              = mi.Get(StreamKind.General, 0, "Format");
            General.FormatExtensions    = mi.Get(StreamKind.General, 0, "Format/Extensions");
            General.InternetMediaType   = mi.Get(StreamKind.General, 0, "InternetMediaType");
            DateTime.TryParse(mi.Get(StreamKind.General, 0, "Duration/String3"), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out General.DurationTime);
            General.Title               = mi.Get(StreamKind.General, 0, "Title");
            General.EncodedApplication  = mi.Get(StreamKind.General, 0, "Encoded_Application");
            General.EncodedApplicationUrl = mi.Get(StreamKind.General, 0, "Encoded_Application/Url");
            General.EncodedLibrary      = mi.Get(StreamKind.General, 0, "Encoded_Library");
            General.EncodedLibraryName  = mi.Get(StreamKind.General, 0, "Encoded_Library/Name");
            General.EncodedLibraryVersion = mi.Get(StreamKind.General, 0, "Encoded_Library/Version");
            General.EncodedLibraryDate  = mi.Get(StreamKind.General, 0, "Encoded_Library/Date");
            General.EncodedLibrarySettings = mi.Get(StreamKind.General, 0, "Encoded_Library_Settings");
        #endregion

        #region Get Video Info

            for (int i = 0; i < _videoStreams; i++)
            {
                VideoStreamInfo videoStream = new VideoStreamInfo();

                Int32.TryParse(mi.Get(StreamKind.Video, i, "StreamKindID"), numStyle, AppSettings.CInfo, out videoStream.StreamKindID);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "StreamKindPos"), numStyle, AppSettings.CInfo, out videoStream.StreamKindPos);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "ID"), numStyle, AppSettings.CInfo, out videoStream.ID);
                videoStream.Format              = mi.Get(StreamKind.Video, i, "Format");
                videoStream.FormatInfo          = mi.Get(StreamKind.Video, i, "Format/Info");
                videoStream.FormatVersion       = mi.Get(StreamKind.Video, i, "Format_Version");
                videoStream.FormatProfile       = mi.Get(StreamKind.Video, i, "Format_Profile");
                videoStream.MultiViewBaseProfile = mi.Get(StreamKind.Video, i, "MultiView_BaseProfile");
                videoStream.MultiViewCount      = mi.Get(StreamKind.Video, i, "MultiView_Count");
                videoStream.InternetMediaType   = mi.Get(StreamKind.Video, i, "InternetMediaType");
                videoStream.CodecID             = mi.Get(StreamKind.Video, i, "CodecID");
                videoStream.CodecIDInfo         = mi.Get(StreamKind.Video, i, "CodecID/Info");
                videoStream.CodecIDUrl          = mi.Get(StreamKind.Video, i, "CodecID/Url");
                videoStream.CodecIDDescription  = mi.Get(StreamKind.Video, i, "CodecID_Description");
                DateTime.TryParse(mi.Get(StreamKind.Video, i, "Duration/String3"), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out videoStream.DurationTime);
                videoStream.BitRateMode         = mi.Get(StreamKind.Video, i, "BitRate_Mode");
                Int32.TryParse(mi.Get(StreamKind.Video, i, "BitRate"), numStyle, AppSettings.CInfo, out videoStream.BitRate);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "BitRate_Minimum"), numStyle, AppSettings.CInfo, out videoStream.BitRateMin);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "BitRate_Nominal"), numStyle, AppSettings.CInfo, out videoStream.BitRateNom);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "BitRate_Maximum"), numStyle, AppSettings.CInfo, out videoStream.BitRateMax);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "Width"), numStyle, AppSettings.CInfo, out videoStream.Width);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "Height"), numStyle, AppSettings.CInfo, out videoStream.Height);
                videoStream.PixelAspectRatio    = mi.Get(StreamKind.Video, i, "PixelAspectRatio");
                videoStream.DisplayAspectRatio  = mi.Get(StreamKind.Video, i, "DisplayAspectRatio");
                videoStream.FrameRateMode       = mi.Get(StreamKind.Video, i, "FrameRate_Mode");

                Single.TryParse(mi.Get(StreamKind.Video, i, "FrameRate"), numStyle, AppSettings.CInfo, out videoStream.FrameRate);
                Processing.GetFPSNumDenom(videoStream.FrameRate, out videoStream.FrameRateEnumerator, out videoStream.FrameRateDenominator);

                Single.TryParse(mi.Get(StreamKind.Video, i, "FrameRate_Minimum"), numStyle, AppSettings.CInfo, out videoStream.FrameRateMin);
                Single.TryParse(mi.Get(StreamKind.Video, i, "FrameRate_Nominal"), numStyle, AppSettings.CInfo, out videoStream.FrameRateNom);
                Single.TryParse(mi.Get(StreamKind.Video, i, "FrameRate_Maximum"), numStyle, AppSettings.CInfo, out videoStream.FrameRateMax);
                Int64.TryParse(mi.Get(StreamKind.Video, i, "FrameCount"), numStyle, AppSettings.CInfo, out videoStream.FrameCount);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "BitDepth"), numStyle, AppSettings.CInfo, out videoStream.BitDepth);
                videoStream.ScanType            = mi.Get(StreamKind.Video, i, "ScanType");
                videoStream.ScanOrder           = mi.Get(StreamKind.Video, i, "ScanOrder");
                UInt64.TryParse(mi.Get(StreamKind.Video, i, "StreamSize"), numStyle, AppSettings.CInfo, out videoStream.StreamSize);
                videoStream.EncodedApplication  = mi.Get(StreamKind.Video, i, "Encoded_Application");
                videoStream.EncodedApplicationUrl = mi.Get(StreamKind.Video, i, "Encoded_Application/Url");
                videoStream.EncodedLibrary      = mi.Get(StreamKind.Video, i, "Encoded_Library");
                videoStream.EncodedLibraryName  = mi.Get(StreamKind.Video, i, "Encoded_Library/Name");
                videoStream.EncodedLibraryVersion = mi.Get(StreamKind.Video, i, "Encoded_Library/Version");
                videoStream.EncodedLibraryDate  = mi.Get(StreamKind.Video, i, "Encoded_Library/Date");
                videoStream.EncodedLibrarySettings = mi.Get(StreamKind.Video, i, "Encoded_Library_Settings");

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

                Int32.TryParse(mi.Get(StreamKind.Audio, i, "StreamKindID"), numStyle, AppSettings.CInfo, out audioStream.StreamKindID);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "StreamKindPos"), numStyle, AppSettings.CInfo, out audioStream.StreamKindPos);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "ID"), numStyle, AppSettings.CInfo, out audioStream.ID);
                audioStream.Format              = mi.Get(StreamKind.Audio, i, "Format");
                audioStream.FormatInfo          = mi.Get(StreamKind.Audio, i, "Format/Info");
                audioStream.FormatVersion       = mi.Get(StreamKind.Audio, i, "Format_Version");
                audioStream.FormatProfile       = mi.Get(StreamKind.Audio, i, "Format_Profile");
                audioStream.CodecID             = mi.Get(StreamKind.Audio, i, "CodecID");
                audioStream.CodecIDInfo         = mi.Get(StreamKind.Audio, i, "CodecID/Info");
                audioStream.CodecIDUrl          = mi.Get(StreamKind.Audio, i, "CodecID/Url");
                audioStream.CodecIDDescription  = mi.Get(StreamKind.Audio, i, "CodecID_Description");
                Int64.TryParse(mi.Get(StreamKind.Audio, i, "Duration"), numStyle, AppSettings.CInfo, out audioStream.Duration);
                audioStream.BitRateMode         = mi.Get(StreamKind.Audio, i, "BitRate_Mode");
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "BitRate"), numStyle, AppSettings.CInfo, out audioStream.BitRate);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "BitRate_Minimum"), numStyle, AppSettings.CInfo, out audioStream.BitRateMin);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "BitRate_Nominal"), numStyle, AppSettings.CInfo, out audioStream.BitRateNom);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "BitRate_Maximum"), numStyle, AppSettings.CInfo, out audioStream.BitRateMax);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Channel(s)"), numStyle, AppSettings.CInfo, out audioStream.Channels);
                audioStream.ChannelsString      = mi.Get(StreamKind.Audio, i, "Channel(s)/String");
                audioStream.ChannelPositions    = mi.Get(StreamKind.Audio, i, "ChannelPositions");
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "SamplingRate"), numStyle, AppSettings.CInfo, out audioStream.SamplingRate);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "BitDepth"), numStyle, AppSettings.CInfo, out audioStream.BitDepth);
                audioStream.CompressionMode     = mi.Get(StreamKind.Audio, i, "Compression_Mode");
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Delay"), numStyle, AppSettings.CInfo, out audioStream.Delay);
                UInt64.TryParse(mi.Get(StreamKind.Audio, i, "StreamSize"), numStyle, AppSettings.CInfo, out audioStream.StreamSize);
                audioStream.EncodedLibrary      = mi.Get(StreamKind.Audio, i, "Encoded_Library");
                audioStream.EncodedLibraryName  = mi.Get(StreamKind.Audio, i, "Encoded_Library/Name");
                audioStream.EncodedLibraryVersion = mi.Get(StreamKind.Audio, i, "Encoded_Library/Version");
                audioStream.EncodedLibraryDate  = mi.Get(StreamKind.Audio, i, "Encoded_Library/Date");
                audioStream.EncodedLibrarySettings = mi.Get(StreamKind.Audio, i, "Encoded_Library_Settings");
                audioStream.LanguageFull        = mi.Get(StreamKind.Audio, i, "Language/String1");
                audioStream.LanguageIso6391     = mi.Get(StreamKind.Audio, i, "Language/String2");
                audioStream.LanguageIso6392     = mi.Get(StreamKind.Audio, i, "Language/String3");

                Audio.Add(audioStream);
            }
        #endregion

        #region Get Image Info
            for (int i = 0; i < _imageStreams; i++)
            {
                ImageStreamInfo imageStream = new ImageStreamInfo();

                Int32.TryParse(mi.Get(StreamKind.Image, i, "StreamKindID"), numStyle, AppSettings.CInfo, out imageStream.StreamKindID);
                Int32.TryParse(mi.Get(StreamKind.Image, i, "ID"), numStyle, AppSettings.CInfo, out imageStream.ID);
                imageStream.Format                  = mi.Get(StreamKind.Image, i, "Format");
                imageStream.CodecIDInfo             = mi.Get(StreamKind.Image, i, "CodecID/Info");
                UInt64.TryParse(mi.Get(StreamKind.Image, i, "StreamSize"), numStyle, AppSettings.CInfo, out imageStream.StreamSize);
                imageStream.LanguageFull            = mi.Get(StreamKind.Image, i, "Language/String1");
                imageStream.LanguageIso6392         = mi.Get(StreamKind.Image, i, "Language/String3");

                Image.Add(imageStream);
            }
        #endregion

        #region Get Text Info
            for (int i = 0; i < _textStreams; i++)
            {
                TextStreamInfo textStream = new TextStreamInfo();

                Int32.TryParse(mi.Get(StreamKind.Text, i, "StreamKindID"), numStyle, AppSettings.CInfo, out textStream.StreamKindID);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "ID"), numStyle, AppSettings.CInfo, out textStream.ID);
                textStream.Format                   = mi.Get(StreamKind.Text, i, "Format");
                textStream.CodecIDInfo              = mi.Get(StreamKind.Text, i, "CodecID/Info");
                Int32.TryParse(mi.Get(StreamKind.Text, i, "Delay"), numStyle, AppSettings.CInfo, out textStream.Delay);
                UInt64.TryParse(mi.Get(StreamKind.Text, i, "StreamSize"), numStyle, AppSettings.CInfo, out textStream.StreamSize);
                textStream.LanguageFull             = mi.Get(StreamKind.Text, i, "Language/String1");
                textStream.LanguageIso6392          = mi.Get(StreamKind.Text, i, "Language/String3");

                Text.Add(textStream);
            }
        #endregion

        #region Get Menu Info
            for (int i = 0; i < _menuCount; i++)
            {
                MenuStreamInfo menuStream = new MenuStreamInfo();
                
                Int32.TryParse(mi.Get(StreamKind.Menu, i, "Chapters_Pos_Begin"), numStyle, AppSettings.CInfo, out menuStream.ChaptersPosBegin);
                Int32.TryParse(mi.Get(StreamKind.Menu, i, "Chapters_Pos_End"), numStyle, AppSettings.CInfo, out menuStream.ChaptersPosEnd);

                for (int j = menuStream.ChaptersPosBegin; j < menuStream.ChaptersPosEnd; j++)
                {
                    DateTime tempTime;
                    DateTime.TryParse(mi.Get(StreamKind.Menu, i, j, InfoKind.Name), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out tempTime);
                    Chapters.Add(tempTime.TimeOfDay);
                }
            }
        #endregion

            mi.Close();
        }
    }
}