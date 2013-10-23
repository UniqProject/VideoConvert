﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaInfoContainer.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.MediaInfo
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Utilities;

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
            public string FormatFrameMode;
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

        private static int _videoStreams;
        private static int _audioStreams;
        private static int _imageStreams;
        private static int _textStreams;
        private static int _menuCount;

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

        public static MediaInfoContainer GetMediaInfo(string fileName)
        {
            MediaInfoContainer result = new MediaInfoContainer();
            const NumberStyles numStyle = NumberStyles.Number;

            MediaInfo resultInfo = new MediaInfo();
            resultInfo.Option("Internet", "No");

            resultInfo.Open(fileName);
            
            _videoStreams = resultInfo.Count_Get(StreamKind.Video);
            _audioStreams = resultInfo.Count_Get(StreamKind.Audio);
            _imageStreams = resultInfo.Count_Get(StreamKind.Image);
            _textStreams = resultInfo.Count_Get(StreamKind.Text);
            _menuCount = resultInfo.Count_Get(StreamKind.Menu);

        #region Get General Info
            result.General.CompleteName = resultInfo.Get(StreamKind.General, 0, "CompleteName");
            result.General.FileName = resultInfo.Get(StreamKind.General, 0, "FileName");
            result.General.FileExtension = resultInfo.Get(StreamKind.General, 0, "FileExtension");
            result.General.Format = resultInfo.Get(StreamKind.General, 0, "Format");
            result.General.FormatExtensions = resultInfo.Get(StreamKind.General, 0, "Format/Extensions");
            result.General.InternetMediaType = resultInfo.Get(StreamKind.General, 0, "InternetMediaType");
            DateTime.TryParse(resultInfo.Get(StreamKind.General, 0, "Duration/String3"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result.General.DurationTime);
            result.General.Title = resultInfo.Get(StreamKind.General, 0, "Title");
            result.General.EncodedApplication = resultInfo.Get(StreamKind.General, 0, "Encoded_Application");
            result.General.EncodedApplicationUrl = resultInfo.Get(StreamKind.General, 0, "Encoded_Application/Url");
            result.General.EncodedLibrary = resultInfo.Get(StreamKind.General, 0, "Encoded_Library");
            result.General.EncodedLibraryName = resultInfo.Get(StreamKind.General, 0, "Encoded_Library/Name");
            result.General.EncodedLibraryVersion = resultInfo.Get(StreamKind.General, 0, "Encoded_Library/Version");
            result.General.EncodedLibraryDate = resultInfo.Get(StreamKind.General, 0, "Encoded_Library/Date");
            result.General.EncodedLibrarySettings = resultInfo.Get(StreamKind.General, 0, "Encoded_Library_Settings");
        #endregion

        #region Get Video Info

            for (int i = 0; i < _videoStreams; i++)
            {
                VideoStreamInfo videoStream = new VideoStreamInfo();

                Int32.TryParse(resultInfo.Get(StreamKind.Video, i, "StreamKindID"), numStyle, CultureInfo.InvariantCulture, out videoStream.StreamKindID);
                Int32.TryParse(resultInfo.Get(StreamKind.Video, i, "StreamKindPos"), numStyle, CultureInfo.InvariantCulture, out videoStream.StreamKindPos);
                Int32.TryParse(resultInfo.Get(StreamKind.Video, i, "ID"), numStyle, CultureInfo.InvariantCulture, out videoStream.ID);
                videoStream.Format              = resultInfo.Get(StreamKind.Video, i, "Format");
                videoStream.FormatInfo          = resultInfo.Get(StreamKind.Video, i, "Format/Info");
                videoStream.FormatVersion       = resultInfo.Get(StreamKind.Video, i, "Format_Version");
                videoStream.FormatProfile       = resultInfo.Get(StreamKind.Video, i, "Format_Profile");
                videoStream.FormatFrameMode     = resultInfo.Get(StreamKind.Video, i, "Format_Settings_FrameMode");
                videoStream.MultiViewBaseProfile = resultInfo.Get(StreamKind.Video, i, "MultiView_BaseProfile");
                videoStream.MultiViewCount      = resultInfo.Get(StreamKind.Video, i, "MultiView_Count");
                videoStream.InternetMediaType   = resultInfo.Get(StreamKind.Video, i, "InternetMediaType");
                videoStream.CodecID             = resultInfo.Get(StreamKind.Video, i, "CodecID");
                videoStream.CodecIDInfo         = resultInfo.Get(StreamKind.Video, i, "CodecID/Info");
                videoStream.CodecIDUrl          = resultInfo.Get(StreamKind.Video, i, "CodecID/Url");
                videoStream.CodecIDDescription  = resultInfo.Get(StreamKind.Video, i, "CodecID_Description");
                DateTime.TryParse(resultInfo.Get(StreamKind.Video, i, "Duration/String3"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out videoStream.DurationTime);
                videoStream.BitRateMode         = resultInfo.Get(StreamKind.Video, i, "BitRate_Mode");
                Int32.TryParse(resultInfo.Get(StreamKind.Video, i, "BitRate"), numStyle, CultureInfo.InvariantCulture, out videoStream.BitRate);
                Int32.TryParse(resultInfo.Get(StreamKind.Video, i, "BitRate_Minimum"), numStyle, CultureInfo.InvariantCulture, out videoStream.BitRateMin);
                Int32.TryParse(resultInfo.Get(StreamKind.Video, i, "BitRate_Nominal"), numStyle, CultureInfo.InvariantCulture, out videoStream.BitRateNom);
                Int32.TryParse(resultInfo.Get(StreamKind.Video, i, "BitRate_Maximum"), numStyle, CultureInfo.InvariantCulture, out videoStream.BitRateMax);
                Int32.TryParse(resultInfo.Get(StreamKind.Video, i, "Width"), numStyle, CultureInfo.InvariantCulture, out videoStream.Width);
                Int32.TryParse(resultInfo.Get(StreamKind.Video, i, "Height"), numStyle, CultureInfo.InvariantCulture, out videoStream.Height);
                videoStream.PixelAspectRatio    = resultInfo.Get(StreamKind.Video, i, "PixelAspectRatio");
                videoStream.DisplayAspectRatio  = resultInfo.Get(StreamKind.Video, i, "DisplayAspectRatio");
                videoStream.FrameRateMode       = resultInfo.Get(StreamKind.Video, i, "FrameRate_Mode");

                Single.TryParse(resultInfo.Get(StreamKind.Video, i, "FrameRate"), numStyle, CultureInfo.InvariantCulture, out videoStream.FrameRate);
                VideoHelper.GetFPSNumDenom(videoStream.FrameRate, out videoStream.FrameRateEnumerator, out videoStream.FrameRateDenominator);

                Single.TryParse(resultInfo.Get(StreamKind.Video, i, "FrameRate_Minimum"), numStyle, CultureInfo.InvariantCulture, out videoStream.FrameRateMin);
                Single.TryParse(resultInfo.Get(StreamKind.Video, i, "FrameRate_Nominal"), numStyle, CultureInfo.InvariantCulture, out videoStream.FrameRateNom);
                Single.TryParse(resultInfo.Get(StreamKind.Video, i, "FrameRate_Maximum"), numStyle, CultureInfo.InvariantCulture, out videoStream.FrameRateMax);
                Int64.TryParse(resultInfo.Get(StreamKind.Video, i, "FrameCount"), numStyle, CultureInfo.InvariantCulture, out videoStream.FrameCount);
                Int32.TryParse(resultInfo.Get(StreamKind.Video, i, "BitDepth"), numStyle, CultureInfo.InvariantCulture, out videoStream.BitDepth);
                videoStream.ScanType            = resultInfo.Get(StreamKind.Video, i, "ScanType");
                videoStream.ScanOrder           = resultInfo.Get(StreamKind.Video, i, "ScanOrder");
                UInt64.TryParse(resultInfo.Get(StreamKind.Video, i, "StreamSize"), numStyle, CultureInfo.InvariantCulture, out videoStream.StreamSize);
                videoStream.EncodedApplication  = resultInfo.Get(StreamKind.Video, i, "Encoded_Application");
                videoStream.EncodedApplicationUrl = resultInfo.Get(StreamKind.Video, i, "Encoded_Application/Url");
                videoStream.EncodedLibrary      = resultInfo.Get(StreamKind.Video, i, "Encoded_Library");
                videoStream.EncodedLibraryName  = resultInfo.Get(StreamKind.Video, i, "Encoded_Library/Name");
                videoStream.EncodedLibraryVersion = resultInfo.Get(StreamKind.Video, i, "Encoded_Library/Version");
                videoStream.EncodedLibraryDate  = resultInfo.Get(StreamKind.Video, i, "Encoded_Library/Date");
                videoStream.EncodedLibrarySettings = resultInfo.Get(StreamKind.Video, i, "Encoded_Library_Settings");

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

                result.Video.Add(videoStream);
            }
        #endregion

        #region Get Audio Info
            for (int i = 0; i < _audioStreams; i++)
            {
                AudioStreamInfo audioStream = new AudioStreamInfo();

                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "StreamKindID"), numStyle, CultureInfo.InvariantCulture, out audioStream.StreamKindID);
                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "StreamKindPos"), numStyle, CultureInfo.InvariantCulture, out audioStream.StreamKindPos);
                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "ID"), numStyle, CultureInfo.InvariantCulture, out audioStream.ID);
                audioStream.Format              = resultInfo.Get(StreamKind.Audio, i, "Format");
                audioStream.FormatInfo          = resultInfo.Get(StreamKind.Audio, i, "Format/Info");
                audioStream.FormatVersion       = resultInfo.Get(StreamKind.Audio, i, "Format_Version");
                audioStream.FormatProfile       = resultInfo.Get(StreamKind.Audio, i, "Format_Profile");
                audioStream.CodecID             = resultInfo.Get(StreamKind.Audio, i, "CodecID");
                audioStream.CodecIDInfo         = resultInfo.Get(StreamKind.Audio, i, "CodecID/Info");
                audioStream.CodecIDUrl          = resultInfo.Get(StreamKind.Audio, i, "CodecID/Url");
                audioStream.CodecIDDescription  = resultInfo.Get(StreamKind.Audio, i, "CodecID_Description");
                Int64.TryParse(resultInfo.Get(StreamKind.Audio, i, "Duration"), numStyle, CultureInfo.InvariantCulture, out audioStream.Duration);
                audioStream.BitRateMode         = resultInfo.Get(StreamKind.Audio, i, "BitRate_Mode");
                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "BitRate"), numStyle, CultureInfo.InvariantCulture, out audioStream.BitRate);
                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "BitRate_Minimum"), numStyle, CultureInfo.InvariantCulture, out audioStream.BitRateMin);
                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "BitRate_Nominal"), numStyle, CultureInfo.InvariantCulture, out audioStream.BitRateNom);
                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "BitRate_Maximum"), numStyle, CultureInfo.InvariantCulture, out audioStream.BitRateMax);
                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "Channel(s)"), numStyle, CultureInfo.InvariantCulture, out audioStream.Channels);
                audioStream.ChannelsString      = resultInfo.Get(StreamKind.Audio, i, "Channel(s)/String");
                audioStream.ChannelPositions    = resultInfo.Get(StreamKind.Audio, i, "ChannelPositions");
                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "SamplingRate"), numStyle, CultureInfo.InvariantCulture, out audioStream.SamplingRate);
                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "BitDepth"), numStyle, CultureInfo.InvariantCulture, out audioStream.BitDepth);
                audioStream.CompressionMode     = resultInfo.Get(StreamKind.Audio, i, "Compression_Mode");
                Int32.TryParse(resultInfo.Get(StreamKind.Audio, i, "Delay"), numStyle, CultureInfo.InvariantCulture, out audioStream.Delay);
                UInt64.TryParse(resultInfo.Get(StreamKind.Audio, i, "StreamSize"), numStyle, CultureInfo.InvariantCulture, out audioStream.StreamSize);
                audioStream.EncodedLibrary      = resultInfo.Get(StreamKind.Audio, i, "Encoded_Library");
                audioStream.EncodedLibraryName  = resultInfo.Get(StreamKind.Audio, i, "Encoded_Library/Name");
                audioStream.EncodedLibraryVersion = resultInfo.Get(StreamKind.Audio, i, "Encoded_Library/Version");
                audioStream.EncodedLibraryDate  = resultInfo.Get(StreamKind.Audio, i, "Encoded_Library/Date");
                audioStream.EncodedLibrarySettings = resultInfo.Get(StreamKind.Audio, i, "Encoded_Library_Settings");
                audioStream.LanguageFull        = resultInfo.Get(StreamKind.Audio, i, "Language/String1");
                audioStream.LanguageIso6391     = resultInfo.Get(StreamKind.Audio, i, "Language/String2");
                audioStream.LanguageIso6392     = resultInfo.Get(StreamKind.Audio, i, "Language/String3");

                result.Audio.Add(audioStream);
            }
        #endregion

        #region Get Image Info
            for (int i = 0; i < _imageStreams; i++)
            {
                ImageStreamInfo imageStream = new ImageStreamInfo();

                Int32.TryParse(resultInfo.Get(StreamKind.Image, i, "StreamKindID"), numStyle, CultureInfo.InvariantCulture, out imageStream.StreamKindID);
                Int32.TryParse(resultInfo.Get(StreamKind.Image, i, "ID"), numStyle, CultureInfo.InvariantCulture, out imageStream.ID);
                imageStream.Format                  = resultInfo.Get(StreamKind.Image, i, "Format");
                imageStream.CodecIDInfo             = resultInfo.Get(StreamKind.Image, i, "CodecID/Info");
                UInt64.TryParse(resultInfo.Get(StreamKind.Image, i, "StreamSize"), numStyle, CultureInfo.InvariantCulture, out imageStream.StreamSize);
                imageStream.LanguageFull            = resultInfo.Get(StreamKind.Image, i, "Language/String1");
                imageStream.LanguageIso6392         = resultInfo.Get(StreamKind.Image, i, "Language/String3");

                result.Image.Add(imageStream);
            }
        #endregion

        #region Get Text Info
            for (int i = 0; i < _textStreams; i++)
            {
                TextStreamInfo textStream = new TextStreamInfo();

                Int32.TryParse(resultInfo.Get(StreamKind.Text, i, "StreamKindID"), numStyle, CultureInfo.InvariantCulture, out textStream.StreamKindID);
                Int32.TryParse(resultInfo.Get(StreamKind.Text, i, "ID"), numStyle, CultureInfo.InvariantCulture, out textStream.ID);
                textStream.Format                   = resultInfo.Get(StreamKind.Text, i, "Format");
                textStream.CodecIDInfo              = resultInfo.Get(StreamKind.Text, i, "CodecID/Info");
                Int32.TryParse(resultInfo.Get(StreamKind.Text, i, "Delay"), numStyle, CultureInfo.InvariantCulture, out textStream.Delay);
                UInt64.TryParse(resultInfo.Get(StreamKind.Text, i, "StreamSize"), numStyle, CultureInfo.InvariantCulture, out textStream.StreamSize);
                textStream.LanguageFull             = resultInfo.Get(StreamKind.Text, i, "Language/String1");
                textStream.LanguageIso6392          = resultInfo.Get(StreamKind.Text, i, "Language/String3");

                result.Text.Add(textStream);
            }
        #endregion

        #region Get Menu Info
            for (int i = 0; i < _menuCount; i++)
            {
                MenuStreamInfo menuStream = new MenuStreamInfo();
                
                Int32.TryParse(resultInfo.Get(StreamKind.Menu, i, "Chapters_Pos_Begin"), numStyle, CultureInfo.InvariantCulture, out menuStream.ChaptersPosBegin);
                Int32.TryParse(resultInfo.Get(StreamKind.Menu, i, "Chapters_Pos_End"), numStyle, CultureInfo.InvariantCulture, out menuStream.ChaptersPosEnd);

                for (int j = menuStream.ChaptersPosBegin; j < menuStream.ChaptersPosEnd; j++)
                {
                    DateTime tempTime;
                    DateTime.TryParse(resultInfo.Get(StreamKind.Menu, i, j, InfoKind.Name), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out tempTime);
                    result.Chapters.Add(tempTime.TimeOfDay);
                }
            }
        #endregion

            resultInfo.Option("Complete");
            resultInfo.Close();

            return result;
        }

        public void Dispose()
        {
            _videoStreams = 0;
            _audioStreams = 0;
            Video = null;
            Audio = null;
            Image = null;
            Text = null;
            Chapters = null;
            General = new GeneralInfo();
        }
    }
}