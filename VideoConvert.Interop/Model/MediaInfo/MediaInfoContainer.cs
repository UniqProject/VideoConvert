// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaInfoContainer.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   MediaInfo file properties
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.MediaInfo
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// MediaInfo file properties
    /// </summary>
    public class MediaInfoContainer : IDisposable
    {
        /// <summary>
        /// General file properties
        /// </summary>
        public struct GeneralInfo
        {
            /// <summary>
            /// Complete file name
            /// </summary>
            public string CompleteName;

            /// <summary>
            /// File name only
            /// </summary>
            public string FileName;

            /// <summary>
            /// File extension only
            /// </summary>
            public string FileExtension;

            /// <summary>
            /// File format
            /// </summary>
            public string Format;

            /// <summary>
            /// Format extensions
            /// </summary>
            public string FormatExtensions;

            /// <summary>
            /// Internet mime type
            /// </summary>
            public string InternetMediaType;

            /// <summary>
            /// File duration
            /// </summary>
            public DateTime DurationTime;

            /// <summary>
            /// File title
            /// </summary>
            public string Title;

            /// <summary>
            /// Application used for encoding
            /// </summary>
            public string EncodedApplication;

            /// <summary>
            /// URL of application used for encoding
            /// </summary>
            public string EncodedApplicationUrl;

            /// <summary>
            /// Library used for encoding
            /// </summary>
            public string EncodedLibrary;

            /// <summary>
            /// Library name of encoding lib
            /// </summary>
            public string EncodedLibraryName;

            /// <summary>
            /// Version of encoding lib
            /// </summary>
            public string EncodedLibraryVersion;

            /// <summary>
            /// creation date of encoding lib
            /// </summary>
            public string EncodedLibraryDate;

            /// <summary>
            /// encoding lib settings
            /// </summary>
            public string EncodedLibrarySettings;
        }

        /// <summary>
        /// Video stream properties
        /// </summary>
        public struct VideoStreamInfo
        {
            /// <summary>
            /// Stream kind ID
            /// </summary>
            public int StreamKindID;

            /// <summary>
            /// Stream kind position
            /// </summary>
            public int StreamKindPos;

            /// <summary>
            /// Stream ID
            /// </summary>
            public int ID;

            /// <summary>
            /// Stream format
            /// </summary>
            public string Format;

            /// <summary>
            /// Stream format info
            /// </summary>
            public string FormatInfo;

            /// <summary>
            /// Stream format version
            /// </summary>
            public string FormatVersion;

            /// <summary>
            /// Stream format profile
            /// </summary>
            public string FormatProfile;

            /// <summary>
            /// Stream format frame mode
            /// </summary>
            public string FormatFrameMode;

            /// <summary>
            /// Stream Multiview base profile
            /// </summary>
            public string MultiViewBaseProfile;

            /// <summary>
            /// Stream Multiview count
            /// </summary>
            public string MultiViewCount;

            /// <summary>
            /// Stream MIME type
            /// </summary>
            public string InternetMediaType;

            /// <summary>
            /// Stream codec ID
            /// </summary>
            public string CodecID;

            /// <summary>
            /// Stream codec ID info
            /// </summary>
            public string CodecIDInfo;

            /// <summary>
            /// Stream codec ID URL
            /// </summary>
            public string CodecIDUrl;

            /// <summary>
            /// Stream codec ID description
            /// </summary>
            public string CodecIDDescription;

            /// <summary>
            /// Stream duration
            /// </summary>
            public DateTime DurationTime;

            /// <summary>
            /// Stream bitrate mode
            /// </summary>
            public string BitRateMode;

            /// <summary>
            /// Stream bitrate
            /// </summary>
            public int BitRate;

            /// <summary>
            /// Stream min bitrate
            /// </summary>
            public int BitRateMin;

            /// <summary>
            /// Stream nominal bitrate
            /// </summary>
            public int BitRateNom;

            /// <summary>
            /// Stream max bitrate
            /// </summary>
            public int BitRateMax;

            /// <summary>
            /// Stream width
            /// </summary>
            public int Width;

            /// <summary>
            /// Stream height
            /// </summary>
            public int Height;

            /// <summary>
            /// Stream pixel aspect ratio (PAR)
            /// </summary>
            public string PixelAspectRatio;

            /// <summary>
            /// Stream display aspect ratio (DAR)
            /// </summary>
            public string DisplayAspectRatio;

            /// <summary>
            /// Stream framerate mode
            /// </summary>
            public string FrameRateMode;

            /// <summary>
            /// Stream framerate
            /// </summary>
            public float FrameRate;

            /// <summary>
            /// Stream framerate enumerator
            /// </summary>
            public int FrameRateEnumerator;

            /// <summary>
            /// Stream framerate denominator
            /// </summary>
            public int FrameRateDenominator;

            /// <summary>
            /// Stream min framerate (VFR only)
            /// </summary>
            public float FrameRateMin;

            /// <summary>
            /// Stream nominal framerate
            /// </summary>
            public float FrameRateNom;

            /// <summary>
            /// Stream max framerate (VFR only)
            /// </summary>
            public float FrameRateMax;

            /// <summary>
            /// Stream frame count
            /// </summary>
            public long FrameCount;

            /// <summary>
            /// Stream bitdepth
            /// </summary>
            public int BitDepth;

            /// <summary>
            /// Stream interlacing scantype
            /// </summary>
            public string ScanType;

            /// <summary>
            /// Stream interlacing scan order
            /// </summary>
            public string ScanOrder;

            /// <summary>
            /// Stream size
            /// </summary>
            public ulong StreamSize;

            /// <summary>
            /// Encoding application
            /// </summary>
            public string EncodedApplication;

            /// <summary>
            /// URL of encoding application
            /// </summary>
            public string EncodedApplicationUrl;

            /// <summary>
            /// Encoding library
            /// </summary>
            public string EncodedLibrary;

            /// <summary>
            /// Name of encoding library
            /// </summary>
            public string EncodedLibraryName;

            /// <summary>
            /// Version of encoding library
            /// </summary>
            public string EncodedLibraryVersion;

            /// <summary>
            /// Creation date of encoding library
            /// </summary>
            public string EncodedLibraryDate;

            /// <summary>
            /// Encoding library settings
            /// </summary>
            public string EncodedLibrarySettings;

            /// <summary>
            /// Picture size
            /// </summary>
            public VideoFormat VideoSize;
        }

        /// <summary>
        /// Audio stream properties
        /// </summary>
        public struct AudioStreamInfo
        {
            /// <summary>
            /// Stream kind ID
            /// </summary>
            public int StreamKindID;

            /// <summary>
            /// Stream kind position
            /// </summary>
            public int StreamKindPos;

            /// <summary>
            /// Stream ID
            /// </summary>
            public int ID;

            /// <summary>
            /// Stream format
            /// </summary>
            public string Format;

            /// <summary>
            /// Stream format info
            /// </summary>
            public string FormatInfo;

            /// <summary>
            /// Stream Format version
            /// </summary>
            public string FormatVersion;

            /// <summary>
            /// Stream format profile
            /// </summary>
            public string FormatProfile;

            /// <summary>
            /// Stream Codec ID
            /// </summary>
            public string CodecID;

            /// <summary>
            /// Stream Codec ID info
            /// </summary>
            public string CodecIDInfo;

            /// <summary>
            /// Stream Codec ID Url
            /// </summary>
            public string CodecIDUrl;

            /// <summary>
            /// Stream Codec ID description
            /// </summary>
            public string CodecIDDescription;

            /// <summary>
            /// Stream duration
            /// </summary>
            public long Duration;

            /// <summary>
            /// Stream bitrate mode
            /// </summary>
            public string BitRateMode;

            /// <summary>
            /// Stream bitrate
            /// </summary>
            public int BitRate;

            /// <summary>
            /// Stream min bitrate
            /// </summary>
            public int BitRateMin;

            /// <summary>
            /// Stream nominal bitrate
            /// </summary>
            public int BitRateNom;

            /// <summary>
            /// Stream max bitrate
            /// </summary>
            public int BitRateMax;

            /// <summary>
            /// Stream channel count
            /// </summary>
            public int Channels;

            /// <summary>
            /// Stream channels
            /// </summary>
            public string ChannelsString;

            /// <summary>
            /// Stream channel positions
            /// </summary>
            public string ChannelPositions;

            /// <summary>
            /// Stream sampling rate
            /// </summary>
            public int SamplingRate;

            /// <summary>
            /// Stream bit depth
            /// </summary>
            public int BitDepth;

            /// <summary>
            /// Stream compression mode
            /// </summary>
            public string CompressionMode;

            /// <summary>
            /// Stream delay
            /// </summary>
            public int Delay;

            /// <summary>
            /// Stream size
            /// </summary>
            public ulong StreamSize;

            /// <summary>
            /// Encoding library
            /// </summary>
            public string EncodedLibrary;

            /// <summary>
            /// Name of encoding library
            /// </summary>
            public string EncodedLibraryName;

            /// <summary>
            /// Version of encoding library
            /// </summary>
            public string EncodedLibraryVersion;

            /// <summary>
            /// Creation date of encoding library
            /// </summary>
            public string EncodedLibraryDate;

            /// <summary>
            /// Encoding library settings
            /// </summary>
            public string EncodedLibrarySettings;

            /// <summary>
            /// Stream full language name
            /// </summary>
            public string LanguageFull;

            /// <summary>
            /// Stream language abbreviation (ISO 639-1)
            /// </summary>
            public string LanguageIso6391;

            /// <summary>
            /// Stream language abbreviation (ISO 639-2)
            /// </summary>
            public string LanguageIso6392;
        }

        /// <summary>
        /// Image subtitle stream properties
        /// </summary>
        public struct ImageStreamInfo
        {
            /// <summary>
            /// Stream kind ID
            /// </summary>
            public int StreamKindID;

            /// <summary>
            /// Stream ID
            /// </summary>
            public int ID;

            /// <summary>
            /// Stream format
            /// </summary>
            public string Format;

            /// <summary>
            /// Stream Codec ID info
            /// </summary>
            public string CodecIDInfo;

            /// <summary>
            /// Stream size
            /// </summary>
            public ulong StreamSize;

            /// <summary>
            /// Stream full language name
            /// </summary>
            public string LanguageFull;

            /// <summary>
            /// Stream language abbreviation (ISO 639-2)
            /// </summary>
            public string LanguageIso6392;
        }

        /// <summary>
        /// Text subtitle stream properties
        /// </summary>
        public struct TextStreamInfo
        {
            /// <summary>
            /// Stream kind ID
            /// </summary>
            public int StreamKindID;

            /// <summary>
            /// Stream ID
            /// </summary>
            public int ID;

            /// <summary>
            /// Stream format
            /// </summary>
            public string Format;

            /// <summary>
            /// Stream Codec ID info
            /// </summary>
            public string CodecIDInfo;

            /// <summary>
            /// Stream delay
            /// </summary>
            public int Delay;

            /// <summary>
            /// Stream size
            /// </summary>
            public ulong StreamSize;

            /// <summary>
            /// Stream full language name
            /// </summary>
            public string LanguageFull;

            /// <summary>
            /// Stream language abbreviation (ISO 639-2)
            /// </summary>
            public string LanguageIso6392;
        }

        /// <summary>
        /// Menu stream properties
        /// </summary>
        public struct MenuStreamInfo
        {
            /// <summary>
            /// Chapter start
            /// </summary>
            public int ChaptersPosBegin;

            /// <summary>
            /// Chapter end
            /// </summary>
            public int ChaptersPosEnd;
        }

        private static int _videoStreams;
        private static int _audioStreams;
        private static int _imageStreams;
        private static int _textStreams;
        private static int _menuCount;

        /// <summary>
        /// General properties
        /// </summary>
        public GeneralInfo General;

        /// <summary>
        /// Video streams
        /// </summary>
        public List<VideoStreamInfo> Video;

        /// <summary>
        /// Audio streams
        /// </summary>
        public List<AudioStreamInfo> Audio;

        /// <summary>
        /// Image subtitle streams
        /// </summary>
        public List<ImageStreamInfo> Image;

        /// <summary>
        /// Text subtitle streams
        /// </summary>
        public List<TextStreamInfo> Text;

        /// <summary>
        /// Chapters
        /// </summary>
        public List<TimeSpan> Chapters;


        /// <summary>
        /// Default constructor
        /// </summary>
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

        /// <summary>
        /// Read MediaInfo properties from given file
        /// </summary>
        /// <param name="fileName">File to check</param>
        /// <returns>File Properties</returns>
        public static MediaInfoContainer GetMediaInfo(string fileName)
        {
            var result = new MediaInfoContainer();
            const NumberStyles numStyle = NumberStyles.Number;

            var resultInfo = new MediaInfo();
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

            for (var i = 0; i < _videoStreams; i++)
            {
                var videoStream = new VideoStreamInfo();

                int.TryParse(resultInfo.Get(StreamKind.Video, i, "StreamKindID"), numStyle, CultureInfo.InvariantCulture, out videoStream.StreamKindID);
                int.TryParse(resultInfo.Get(StreamKind.Video, i, "StreamKindPos"), numStyle, CultureInfo.InvariantCulture, out videoStream.StreamKindPos);
                int.TryParse(resultInfo.Get(StreamKind.Video, i, "ID"), numStyle, CultureInfo.InvariantCulture, out videoStream.ID);
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
                int.TryParse(resultInfo.Get(StreamKind.Video, i, "BitRate"), numStyle, CultureInfo.InvariantCulture, out videoStream.BitRate);
                int.TryParse(resultInfo.Get(StreamKind.Video, i, "BitRate_Minimum"), numStyle, CultureInfo.InvariantCulture, out videoStream.BitRateMin);
                int.TryParse(resultInfo.Get(StreamKind.Video, i, "BitRate_Nominal"), numStyle, CultureInfo.InvariantCulture, out videoStream.BitRateNom);
                int.TryParse(resultInfo.Get(StreamKind.Video, i, "BitRate_Maximum"), numStyle, CultureInfo.InvariantCulture, out videoStream.BitRateMax);
                int.TryParse(resultInfo.Get(StreamKind.Video, i, "Width"), numStyle, CultureInfo.InvariantCulture, out videoStream.Width);
                int.TryParse(resultInfo.Get(StreamKind.Video, i, "Height"), numStyle, CultureInfo.InvariantCulture, out videoStream.Height);
                videoStream.PixelAspectRatio    = resultInfo.Get(StreamKind.Video, i, "PixelAspectRatio");
                videoStream.DisplayAspectRatio  = resultInfo.Get(StreamKind.Video, i, "DisplayAspectRatio");
                videoStream.FrameRateMode       = resultInfo.Get(StreamKind.Video, i, "FrameRate_Mode");

                float.TryParse(resultInfo.Get(StreamKind.Video, i, "FrameRate"), numStyle, CultureInfo.InvariantCulture, out videoStream.FrameRate);
                VideoHelper.GetFpsNumDenom(videoStream.FrameRate, out videoStream.FrameRateEnumerator, out videoStream.FrameRateDenominator);

                float.TryParse(resultInfo.Get(StreamKind.Video, i, "FrameRate_Minimum"), numStyle, CultureInfo.InvariantCulture, out videoStream.FrameRateMin);
                float.TryParse(resultInfo.Get(StreamKind.Video, i, "FrameRate_Nominal"), numStyle, CultureInfo.InvariantCulture, out videoStream.FrameRateNom);
                float.TryParse(resultInfo.Get(StreamKind.Video, i, "FrameRate_Maximum"), numStyle, CultureInfo.InvariantCulture, out videoStream.FrameRateMax);
                long.TryParse(resultInfo.Get(StreamKind.Video, i, "FrameCount"), numStyle, CultureInfo.InvariantCulture, out videoStream.FrameCount);
                int.TryParse(resultInfo.Get(StreamKind.Video, i, "BitDepth"), numStyle, CultureInfo.InvariantCulture, out videoStream.BitDepth);
                videoStream.ScanType            = resultInfo.Get(StreamKind.Video, i, "ScanType");
                videoStream.ScanOrder           = resultInfo.Get(StreamKind.Video, i, "ScanOrder");
                ulong.TryParse(resultInfo.Get(StreamKind.Video, i, "StreamSize"), numStyle, CultureInfo.InvariantCulture, out videoStream.StreamSize);
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
            for (var i = 0; i < _audioStreams; i++)
            {
                var audioStream = new AudioStreamInfo();

                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "StreamKindID"), numStyle, CultureInfo.InvariantCulture, out audioStream.StreamKindID);
                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "StreamKindPos"), numStyle, CultureInfo.InvariantCulture, out audioStream.StreamKindPos);
                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "ID"), numStyle, CultureInfo.InvariantCulture, out audioStream.ID);
                audioStream.Format              = resultInfo.Get(StreamKind.Audio, i, "Format");
                audioStream.FormatInfo          = resultInfo.Get(StreamKind.Audio, i, "Format/Info");
                audioStream.FormatVersion       = resultInfo.Get(StreamKind.Audio, i, "Format_Version");
                audioStream.FormatProfile       = resultInfo.Get(StreamKind.Audio, i, "Format_Profile");
                audioStream.CodecID             = resultInfo.Get(StreamKind.Audio, i, "CodecID");
                audioStream.CodecIDInfo         = resultInfo.Get(StreamKind.Audio, i, "CodecID/Info");
                audioStream.CodecIDUrl          = resultInfo.Get(StreamKind.Audio, i, "CodecID/Url");
                audioStream.CodecIDDescription  = resultInfo.Get(StreamKind.Audio, i, "CodecID_Description");
                long.TryParse(resultInfo.Get(StreamKind.Audio, i, "Duration"), numStyle, CultureInfo.InvariantCulture, out audioStream.Duration);
                audioStream.BitRateMode         = resultInfo.Get(StreamKind.Audio, i, "BitRate_Mode");
                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "BitRate"), numStyle, CultureInfo.InvariantCulture, out audioStream.BitRate);
                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "BitRate_Minimum"), numStyle, CultureInfo.InvariantCulture, out audioStream.BitRateMin);
                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "BitRate_Nominal"), numStyle, CultureInfo.InvariantCulture, out audioStream.BitRateNom);
                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "BitRate_Maximum"), numStyle, CultureInfo.InvariantCulture, out audioStream.BitRateMax);
                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "Channel(s)"), numStyle, CultureInfo.InvariantCulture, out audioStream.Channels);
                audioStream.ChannelsString      = resultInfo.Get(StreamKind.Audio, i, "Channel(s)/String");
                audioStream.ChannelPositions    = resultInfo.Get(StreamKind.Audio, i, "ChannelPositions");
                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "SamplingRate"), numStyle, CultureInfo.InvariantCulture, out audioStream.SamplingRate);
                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "BitDepth"), numStyle, CultureInfo.InvariantCulture, out audioStream.BitDepth);
                audioStream.CompressionMode     = resultInfo.Get(StreamKind.Audio, i, "Compression_Mode");
                int.TryParse(resultInfo.Get(StreamKind.Audio, i, "Delay"), numStyle, CultureInfo.InvariantCulture, out audioStream.Delay);
                ulong.TryParse(resultInfo.Get(StreamKind.Audio, i, "StreamSize"), numStyle, CultureInfo.InvariantCulture, out audioStream.StreamSize);
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
            for (var i = 0; i < _imageStreams; i++)
            {
                var imageStream = new ImageStreamInfo();

                int.TryParse(resultInfo.Get(StreamKind.Image, i, "StreamKindID"), numStyle, CultureInfo.InvariantCulture, out imageStream.StreamKindID);
                int.TryParse(resultInfo.Get(StreamKind.Image, i, "ID"), numStyle, CultureInfo.InvariantCulture, out imageStream.ID);
                imageStream.Format                  = resultInfo.Get(StreamKind.Image, i, "Format");
                imageStream.CodecIDInfo             = resultInfo.Get(StreamKind.Image, i, "CodecID/Info");
                ulong.TryParse(resultInfo.Get(StreamKind.Image, i, "StreamSize"), numStyle, CultureInfo.InvariantCulture, out imageStream.StreamSize);
                imageStream.LanguageFull            = resultInfo.Get(StreamKind.Image, i, "Language/String1");
                imageStream.LanguageIso6392         = resultInfo.Get(StreamKind.Image, i, "Language/String3");

                result.Image.Add(imageStream);
            }
        #endregion

        #region Get Text Info
            for (var i = 0; i < _textStreams; i++)
            {
                var textStream = new TextStreamInfo();

                int.TryParse(resultInfo.Get(StreamKind.Text, i, "StreamKindID"), numStyle, CultureInfo.InvariantCulture, out textStream.StreamKindID);
                int.TryParse(resultInfo.Get(StreamKind.Text, i, "ID"), numStyle, CultureInfo.InvariantCulture, out textStream.ID);
                textStream.Format                   = resultInfo.Get(StreamKind.Text, i, "Format");
                textStream.CodecIDInfo              = resultInfo.Get(StreamKind.Text, i, "CodecID/Info");
                int.TryParse(resultInfo.Get(StreamKind.Text, i, "Delay"), numStyle, CultureInfo.InvariantCulture, out textStream.Delay);
                ulong.TryParse(resultInfo.Get(StreamKind.Text, i, "StreamSize"), numStyle, CultureInfo.InvariantCulture, out textStream.StreamSize);
                textStream.LanguageFull             = resultInfo.Get(StreamKind.Text, i, "Language/String1");
                textStream.LanguageIso6392          = resultInfo.Get(StreamKind.Text, i, "Language/String3");

                result.Text.Add(textStream);
            }
        #endregion

        #region Get Menu Info
            for (var i = 0; i < _menuCount; i++)
            {
                var menuStream = new MenuStreamInfo();
                
                int.TryParse(resultInfo.Get(StreamKind.Menu, i, "Chapters_Pos_Begin"), numStyle, CultureInfo.InvariantCulture, out menuStream.ChaptersPosBegin);
                int.TryParse(resultInfo.Get(StreamKind.Menu, i, "Chapters_Pos_End"), numStyle, CultureInfo.InvariantCulture, out menuStream.ChaptersPosEnd);

                for (var j = menuStream.ChaptersPosBegin; j < menuStream.ChaptersPosEnd; j++)
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

        /// <summary>
        /// Free resources
        /// </summary>
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