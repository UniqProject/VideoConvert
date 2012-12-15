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
            public int Count;
            public int Status;
            public int StreamCount;
            public string StreamKind;
            public int StreamKindID;
            public int StreamKindPos;
            public int ID;
            public string UniqueID;
            public string MenuID;
            public int GeneralCount;
            public int VideoCount;
            public int AudioCount;
            public int TextCount;
            public int ChaptersCount;
            public int ImageCount;
            public int MenuCount;
            public string CompleteName;
            public string FolderName;
            public string FileName;
            public string FileExtension;
            public string Format;
            public string FormatInfo;
            public string FormatUrl;
            public string FormatExtensions;
            public string FormatCommercial;
            public string FormatCommercialIfAny;
            public string FormatVersion;
            public string FormatProfile;
            public string FormatSettings;
            public string InternetMediaType;
            public string Interleaved;
            public Int64 FileSize;
            public string FileSizeMeasured;
            public Int64 Duration;
            public DateTime DurationTime;
            public string OverallBitRateMode;
            public int OverallBitRate;
            public int OverallBitRateMin;
            public int OverallBitRateNom;
            public int OverallBitRateMax;
            public int Delay;
            public Int64 StreamSize;
            public float StreamSizeProportion;
            public string Title;
            public string TitleMore;
            public string TitleUrl;
            public string Domain;
            public string Collection;
            public int Season;
            public int SeasonPosition;
            public int SeasonPositionTotal;
            public string Movie;
            public string MovieMore;
            public string MovieCountry;
            public string MovieUrl;
            public string Album;
            public string AlbumMore;
            public int AlbumSort;
            public string Comic;
            public string ComicMore;
            public int ComicPositionTotal;
            public string Part;
            public int PartPosition;
            public int PartPositionTotal;
            public string Track;
            public string TrackMore;
            public string TrackUrl;
            public int TrackSort;
            public int TrackPosition;
            public int TrackPositionTotal;
            public string Chapter;
            public string SubTrack;
            public string OriginalAlbum;
            public string OriginalMovie;
            public string OriginalPart;
            public string OriginalTrack;
            public string Performer;
            public int PerformerSort;
            public string PerformerUrl;
            public string OriginalPerformer;
            public string Accompaniment;
            public string Composer;
            public string ComposerNationality;
            public string Arranger;
            public string Lyricist;
            public string OriginalLyricist;
            public string Conductor;
            public string Director;
            public string AssistantDirector;
            public string DirectorOfPhotography;
            public string SoundEngineer;
            public string ArtDirector;
            public string ProductionDesigner;
            public string Choregrapher;
            public string CostumeDesigner;
            public string Actor;
            public string ActorCharacter;
            public string WrittenBy;
            public string ScreenplayBy;
            public string EditedBy;
            public string CommisionedBy;
            public string Producer;
            public string CoProducer;
            public string ExecutiveProducer;
            public string MusicBy;
            public string DistributedBy;
            public string MasteredBy;
            public string EncodedBy;
            public string RemixedBy;
            public string ProductionStudio;
            public string ThanksTo;
            public string Publisher;
            public string PublisherUrl;
            public string Label;
            public string Genre;
            public string Mood;
            public string ContentType;
            public string Subject;
            public string Description;
            public string Keywords;
            public string Summary;
            public string Synopsis;
            public string Period;
            public string LawRating;
            public string LawRatingReason;
            public string ICRA;
            public string ReleasedDate;
            public string OriginalReleasedDate;
            public string RecordedDate;
            public string EncodedDate;
            public string TaggedDate;
            public string WrittenDate;
            public string MasteredDate;
            public DateTime FileCreatedDate;
            public DateTime FileModifiedDate;
            public string RecordedLocation;
            public string WrittenLocation;
            public string ArchivalLocation;
            public string EncodedApplication;
            public string EncodedApplicationUrl;
            public string EncodedLibrary;
            public string EncodedLibraryName;
            public string EncodedLibraryVersion;
            public string EncodedLibraryDate;
            public string EncodedLibrarySettings;
            public string Cropped;
            public string Dimensions;
            public float DotsPerInch;
            public string Lightness;
            public string OriginalSourceMedium;
            public string OriginalSourceForm;
            public string TaggedApplication;
            public string BPM;
            public string ISRC;
            public string ISBN;
            public string BarCode;
            public string LCCN;
            public string CatalogNumber;
            public string LabelCode;
            public string Owner;
            public string Copyright;
            public string CopyrightUrl;
            public string ProducerCopyright;
            public string TermsOfUse;
            public string CoverDescription;
            public string CoverType;
            public string CoverMime;
            public string CoverData;
            public string Lyrics;
            public string Comment;
            public float Rating;
        }

        

        public struct VideoStreamInfo
        {
            public int Count;
            public int Status;
            public int StreamCount;
            public string StreamKind;
            public int StreamKindID;
            public int StreamKindPos;
            public int ID;
            public string UniqueID;
            public string MenuID;
            public string Format;
            public string FormatInfo;
            public string FormatUrl;
            public string FormatCommercial;
            public string FormatCommercialIfAny;
            public string FormatVersion;
            public string FormatProfile;
            public string FormatSettings;
            public string MultiViewBaseProfile;
            public string MultiViewCount;
            public string InternetMediaType;
            public string MuxingMode;
            public string CodecID;
            public string CodecIDInfo;
            public string CodecIDHint;
            public string CodecIDUrl;
            public string CodecIDDescription;
            public Int64 Duration;
            public DateTime DurationTime;
            public string BitRateMode;
            public int BitRate;
            public int BitRateMin;
            public int BitRateNom;
            public int BitRateMax;
            public int Width;
            public int WidthOriginal;
            public int Height;
            public int HeightOriginal;
            public string PixelAspectRatio;
            public string PixelAspectRatioOriginal;
            public string DisplayAspectRatio;
            public string DisplayAspectRatioOriginal;
            public string Rotation;
            public string FrameRateMode;
            public string FrameRateModeOriginal;
            public float FrameRate;
            public int FrameRateEnumerator;
            public int FrameRateDenominator;
            public float FrameRateOriginal;
            public float FrameRateMin;
            public float FrameRateNom;
            public float FrameRateMax;
            public Int64 FrameCount;
            public string Standard;
            public string ColorSpace;
            public string ChromaSubsampling;
            public int BitDepth;
            public string ScanType;
            public string ScanOrder;
            public string BitsPixelPerFrame;
            public int Delay;
            public int DelayOriginal;
            public ulong StreamSize;
            public float StreamSizeProportion;
            public string Alignment;
            public string Title;
            public string EncodedApplication;
            public string EncodedApplicationUrl;
            public string EncodedLibrary;
            public string EncodedLibraryName;
            public string EncodedLibraryVersion;
            public string EncodedLibraryDate;
            public string EncodedLibrarySettings;
            public string Language;
            public string LanguageFull;
            public string LanguageIso6391;
            public string LanguageIso6392;
            public string LanguageMore;
            public string EncodedDate;
            public string TaggedDate;
            public string Encryption;
            public string BufferSize;
            public VideoFormat VideoSize;
        }

        public struct AudioStreamInfo
        {
            public int Count;
            public int Status;
            public int StreamCount;
            public string StreamKind;
            public int StreamKindID;
            public int StreamKindPos;
            public int ID;
            public string UniqueID;
            public string MenuID;
            public string Format;
            public string FormatInfo;
            public string FormatUrl;
            public string FormatCommercial;
            public string FormatCommercialIfAny;
            public string FormatVersion;
            public string FormatProfile;
            public string FormatSettings;
            public string InternetMediaType;
            public string MuxingMode;
            public string MuxingModeMore;
            public string CodecID;
            public string CodecIDInfo;
            public string CodecIDHint;
            public string CodecIDUrl;
            public string CodecIDDescription;
            public Int64 Duration;
            public DateTime DurationTime;
            public string BitRateMode;
            public int BitRate;
            public int BitRateMin;
            public int BitRateNom;
            public int BitRateMax;
            public int Channels;
            public string ChannelsString;
            public string ChannelPositions;
            public string ChannelPositionsString;
            public int SamplingRate;
            public int SamplingCount;
            public Int64 FrameCount;
            public int BitDepth;
            public string CompressionRate;
            public string CompressionMode;
            public int Delay;
            public int DelayOriginal;
            public int VideoDelay;
            public int Video0Delay;
            public float ReplayGain;
            public float ReplayGainPeak;
            public ulong StreamSize;
            public float StreamSizeProportion;
            public string Alignment;
            public int InterleaveVideoFrames;
            public int InterleaveDuration;
            public int InterleavePreload;
            public string Title;
            public string EncodedLibrary;
            public string EncodedLibraryName;
            public string EncodedLibraryVersion;
            public string EncodedLibraryDate;
            public string EncodedLibrarySettings;
            public string Language;
            public string LanguageFull;
            public string LanguageIso6391;
            public string LanguageIso6392;
            public string LanguageMore;
            public string EncodedDate;
            public string TaggedDate;
            public string Encryption;
        }

        public struct ChaptersInfo
        {
            public int Count;
            public int Status;
            public int StreamCount;
            public string StreamKind;
            public int StreamKindID;
            public int StreamKindPos;
            public int ID;
            public string UniqueID;
            public string MenuID;
            public string Format;
            public string FormatInfo;
            public string FormatUrl;
            public int Total;
            public string Title;
            public string Language;
            public string LanguageFull;
        }

        public struct ImageStreamInfo
        {
            public int Count;
            public int Status;
            public int StreamCount;
            public string StreamKind;
            public int StreamKindID;
            public int StreamKindPos;
            public int ID;
            public string UniqueID;
            public string MenuID;
            public string Title;
            public string Format;
            public string FormatInfo;
            public string FormatUrl;
            public string FormatCommercial;
            public string FormatCommercialIfAny;
            public string FormatVersion;
            public string FormatProfile;
            public string FormatSettings;
            public string InternetMediaType;
            public string CodecID;
            public string CodecIDInfo;
            public string CodecIDHint;
            public string CodecIDUrl;
            public string CodecIDDescription;
            public int Width;
            public int Height;
            public string ColorSpace;
            public string ChromaSubsampling;
            public int BitDepth;
            public string BitDepthStr;
            public string CompressionMode;
            public string CompressionRate;
            public ulong StreamSize;
            public float StreamSizeProportion;
            public string EncodedLibrary;
            public string EncodedLibraryName;
            public string EncodedLibraryVersion;
            public string EncodedLibraryDate;
            public string EncodedLibrarySettings;
            public string Language;
            public string LanguageFull;
            public string LanguageIso6391;
            public string LanguageIso6392;
            public string EncodedDate;
            public string TaggedDate;
            public string Encryption;
        }

        public struct TextStreamInfo
        {
            public int Count;
            public int Status;
            public int StreamCount;
            public string StreamKind;
            public int StreamKindID;
            public int StreamKindPos;
            public int ID;
            public string UniqueID;
            public string MenuID;
            public string Format;
            public string FormatInfo;
            public string FormatUrl;
            public string FormatCommercial;
            public string FormatCommercialIfAny;
            public string FormatVersion;
            public string FormatProfile;
            public string FormatSettings;
            public string InternetMediaType;
            public string MuxingMode;
            public string CodecID;
            public string CodecIDInfo;
            public string CodecIDHint;
            public string CodecIDUrl;
            public string CodecIDDescription;
            public Int64 Duration;
            public string BitRateMode;
            public int BitRate;
            public int BitRateMin;
            public int BitRateNom;
            public int BitRateMax;
            public int Width;
            public int Height;
            public string FrameRateMode;
            public float FrameRate;
            public float FrameRateOriginal;
            public float FrameRateMin;
            public float FrameRateNom;
            public float FrameRateMax;
            public Int64 FrameCount;
            public string ColorSpace;
            public string ChromaSubsampling;
            public int BitDepth;
            public string BitDepthStr;
            public string CompressionMode;
            public string CompressionRate;
            public int Delay;
            public int DelayOriginal;
            public int VideoDelay;
            public ulong StreamSize;
            public float StreamSizeProportion;
            public string Title;
            public string EncodedLibrary;
            public string EncodedLibraryName;
            public string EncodedLibraryVersion;
            public string EncodedLibraryDate;
            public string EncodedLibrarySettings;
            public string Language;
            public string LanguageFull;
            public string LanguageIso6391;
            public string LanguageIso6392;
            public string LanguageMore;
            public string EncodedDate;
            public string TaggedDate;
            public string Encryption;
        }

        public struct MenuStreamInfo
        {
            public int Count;
            public int Status;
            public int StreamCount;
            public string StreamKind;
            public int StreamKindID;
            public int StreamKindPos;
            public int ID;
            public string UniqueID;
            public string MenuID;
            public string Format;
            public string FormatInfo;
            public string FormatUrl;
            public string FormatCommercial;
            public string FormatCommercialIfAny;
            public string FormatVersion;
            public string FormatProfile;
            public string FormatSettings;
            public string CodecID;
            public string CodecIDInfo;
            public string CodecIDHint;
            public string CodecIDUrl;
            public string CodecIDDescription;
            public Int64 Duration;
            public int Delay;
            public string DelaySettings;
            public string DelaySource;
            public string ListStreamKind;
            public string ListStreamPos;
            public string List;
            public string Title;
            public string Language;
            public string LanguageFull;
            public string LanguageIso6391;
            public string LanguageIso6392;
            public string LanguageMore;
            public int ChaptersPosBegin;
            public int ChaptersPosEnd;
        }

        public int VideoStreams;
        public int AudioStreams;
        public int ChapterCount;
        public int ImageStreams;
        public int TextStreams;
        public int MenuCount;

        public GeneralInfo General;
        public List<VideoStreamInfo> Video;
        public List<AudioStreamInfo> Audio;
        public List<ChaptersInfo> Chapters;
        public List<ImageStreamInfo> Image;
        public List<TextStreamInfo> Text;
        public List<MenuStreamInfo> Menu;

        public List<TimeSpan> MyChapters;


        public MediaInfoContainer()
        {
            VideoStreams = 0;
            AudioStreams = 0;
            Video = new List<VideoStreamInfo>();
            Audio = new List<AudioStreamInfo>();
            Chapters = new List<ChaptersInfo>();
            Image = new List<ImageStreamInfo>();
            Text = new List<TextStreamInfo>();
            Menu = new List<MenuStreamInfo>();
            MyChapters = new List<TimeSpan>();
        }

        public void GetMediaInfo(string fileName)
        {
            const NumberStyles numStyle = NumberStyles.Number;

            MediaInfo mi = new MediaInfo();
            mi.Open(fileName);

            mi.Option("Complete_Get");

            VideoStreams = mi.CountGet(StreamKind.Video);
            AudioStreams = mi.CountGet(StreamKind.Audio);
            ChapterCount = mi.CountGet(StreamKind.Chapters);
            ImageStreams = mi.CountGet(StreamKind.Image);
            TextStreams = mi.CountGet(StreamKind.Text);
            MenuCount = mi.CountGet(StreamKind.Menu);

        #region Get General Info
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Count"), numStyle, AppSettings.CInfo, out General.Count);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Status"), numStyle, AppSettings.CInfo, out General.Status);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "StreamCount"), numStyle, AppSettings.CInfo, out General.StreamCount);
            General.StreamKind          = mi.Get(StreamKind.General, 0, "StreamKind");
            Int32.TryParse(mi.Get(StreamKind.General, 0, "StreamKindID"), numStyle, AppSettings.CInfo, out General.StreamKindID);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "StreamKindPos"), numStyle, AppSettings.CInfo, out General.StreamKindPos);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "ID"), numStyle, AppSettings.CInfo, out General.ID);
            General.UniqueID            = mi.Get(StreamKind.General, 0, "UniqueID");
            General.MenuID              = mi.Get(StreamKind.General, 0, "MenuID");
            Int32.TryParse(mi.Get(StreamKind.General, 0, "GeneralCount"), numStyle, AppSettings.CInfo, out General.GeneralCount);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "VideoCount"), numStyle, AppSettings.CInfo, out General.VideoCount);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "AudioCount"), numStyle, AppSettings.CInfo, out General.AudioCount);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "TextCount"), numStyle, AppSettings.CInfo, out General.TextCount);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "ChaptersCount"), numStyle, AppSettings.CInfo, out General.ChaptersCount);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "ImageCount"), numStyle, AppSettings.CInfo, out General.ImageCount);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "MenuCount"), numStyle, AppSettings.CInfo, out General.MenuCount);
            General.CompleteName        = mi.Get(StreamKind.General, 0, "CompleteName");
            General.FolderName          = mi.Get(StreamKind.General, 0, "FolderName");
            General.FileName            = mi.Get(StreamKind.General, 0, "FileName");
            General.FileExtension       = mi.Get(StreamKind.General, 0, "FileExtension");
            General.Format              = mi.Get(StreamKind.General, 0, "Format");
            General.FormatInfo          = mi.Get(StreamKind.General, 0, "Format/Info");
            General.FormatUrl           = mi.Get(StreamKind.General, 0, "Format/Url");
            General.FormatExtensions    = mi.Get(StreamKind.General, 0, "Format/Extensions");
            General.FormatCommercial    = mi.Get(StreamKind.General, 0, "Format/Commercial");
            General.FormatCommercialIfAny = mi.Get(StreamKind.General, 0, "Format/Commercial_IfAny");
            General.FormatVersion       = mi.Get(StreamKind.General, 0, "Format_Version");
            General.FormatProfile       = mi.Get(StreamKind.General, 0, "Format_Profile");
            General.FormatSettings      = mi.Get(StreamKind.General, 0, "Format_Settings");
            General.InternetMediaType   = mi.Get(StreamKind.General, 0, "InternetMediaType");
            General.Interleaved         = mi.Get(StreamKind.General, 0, "Interleaved");
            Int64.TryParse(mi.Get(StreamKind.General, 0, "FileSize"), numStyle, AppSettings.CInfo, out General.FileSize);
            General.FileSizeMeasured    = mi.Get(StreamKind.General, 0, "FileSize/String2");
            Int64.TryParse(mi.Get(StreamKind.General, 0, "Duration"), numStyle, AppSettings.CInfo, out General.Duration);
            DateTime.TryParse(mi.Get(StreamKind.General, 0, "Duration/String3"), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out General.DurationTime);
            General.OverallBitRateMode  = mi.Get(StreamKind.General, 0, "OverallBitRate_Mode");
            Int32.TryParse(mi.Get(StreamKind.General, 0, "OverallBitRate"), numStyle, AppSettings.CInfo, out General.OverallBitRate);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "OverallBitRate_Minimum"), numStyle, AppSettings.CInfo, out General.OverallBitRateMin);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "OverallBitRate_Nominal"), numStyle, AppSettings.CInfo, out General.OverallBitRateNom);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "OverallBitRate_Maximum"), numStyle, AppSettings.CInfo, out General.OverallBitRateMax);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Delay"), numStyle, AppSettings.CInfo, out General.Delay);
            Int64.TryParse(mi.Get(StreamKind.General, 0, "StreamSize"), numStyle, AppSettings.CInfo, out General.StreamSize);
            Single.TryParse(mi.Get(StreamKind.General, 0, "StreamSize_Proportion"), numStyle, AppSettings.CInfo, out General.StreamSizeProportion);
            General.Title               = mi.Get(StreamKind.General, 0, "Title");
            General.TitleMore           = mi.Get(StreamKind.General, 0, "Title/More");
            General.TitleUrl            = mi.Get(StreamKind.General, 0, "Title/Url");
            General.Collection          = mi.Get(StreamKind.General, 0, "Collection");
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Season"), numStyle, AppSettings.CInfo, out General.Season);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Season_Position"), numStyle, AppSettings.CInfo, out General.SeasonPosition);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Season_Position_Total"), numStyle, AppSettings.CInfo, out General.SeasonPositionTotal);
            General.Movie               = mi.Get(StreamKind.General, 0, "Movie");
            General.MovieMore           = mi.Get(StreamKind.General, 0, "Movie/More");
            General.MovieCountry        = mi.Get(StreamKind.General, 0, "Movie/Country");
            General.MovieUrl            = mi.Get(StreamKind.General, 0, "Movie/Url");
            General.Album               = mi.Get(StreamKind.General, 0, "Album");
            General.AlbumMore           = mi.Get(StreamKind.General, 0, "Album/More");
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Album/Sort"), numStyle, AppSettings.CInfo, out General.AlbumSort);
            General.Comic               = mi.Get(StreamKind.General, 0, "Comic");
            General.ComicMore           = mi.Get(StreamKind.General, 0, "Comic/More");
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Comic/Position_Total"), numStyle, AppSettings.CInfo, out General.ComicPositionTotal);
            General.Part                = mi.Get(StreamKind.General, 0, "Part");
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Part/Position"), numStyle, AppSettings.CInfo, out General.PartPosition);
            General.Track               = mi.Get(StreamKind.General, 0, "Track");
            General.TrackMore           = mi.Get(StreamKind.General, 0, "Track/More");
            General.TrackUrl            = mi.Get(StreamKind.General, 0, "Track/Url");
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Track/Sort"), numStyle, AppSettings.CInfo, out General.TrackSort);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Track/Position"), numStyle, AppSettings.CInfo, out General.TrackPosition);
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Track/Position_Total"), numStyle, AppSettings.CInfo, out General.TrackPositionTotal);
            General.Chapter             = mi.Get(StreamKind.General, 0, "Chapter");
            General.SubTrack            = mi.Get(StreamKind.General, 0, "SubTrack");
            General.OriginalAlbum       = mi.Get(StreamKind.General, 0, "Original/Album");
            General.OriginalMovie       = mi.Get(StreamKind.General, 0, "Original/Movie");
            General.OriginalPart        = mi.Get(StreamKind.General, 0, "Original/Part");
            General.OriginalTrack       = mi.Get(StreamKind.General, 0, "Original/Track");
            General.Performer           = mi.Get(StreamKind.General, 0, "Performer");
            Int32.TryParse(mi.Get(StreamKind.General, 0, "Performer/Sort"), numStyle, AppSettings.CInfo, out General.PerformerSort);
            General.PerformerUrl        = mi.Get(StreamKind.General, 0, "Performer/Url");
            General.OriginalPerformer   = mi.Get(StreamKind.General, 0, "Original/Performer");
            General.Accompaniment       = mi.Get(StreamKind.General, 0, "Accompaniment");
            General.Composer            = mi.Get(StreamKind.General, 0, "Composer");
            General.ComposerNationality = mi.Get(StreamKind.General, 0, "Composer/Nationality");
            General.Arranger            = mi.Get(StreamKind.General, 0, "Arranger");
            General.Lyricist            = mi.Get(StreamKind.General, 0, "Lyricist");
            General.OriginalLyricist    = mi.Get(StreamKind.General, 0, "Original/Lyricist");
            General.Conductor           = mi.Get(StreamKind.General, 0, "Conductor");
            General.Director            = mi.Get(StreamKind.General, 0, "Director");
            General.AssistantDirector   = mi.Get(StreamKind.General, 0, "AssistantDirector");
            General.DirectorOfPhotography = mi.Get(StreamKind.General, 0, "DirectorOfPhotography");
            General.SoundEngineer       = mi.Get(StreamKind.General, 0, "SoundEngineer");
            General.ArtDirector         = mi.Get(StreamKind.General, 0, "ArtDirector");
            General.ProductionDesigner  = mi.Get(StreamKind.General, 0, "ProductionDesigner");
            General.Choregrapher        = mi.Get(StreamKind.General, 0, "Ghoregrapher");
            General.CostumeDesigner     = mi.Get(StreamKind.General, 0, "CostumeDesigner");
            General.Actor               = mi.Get(StreamKind.General, 0, "Actor");
            General.ActorCharacter      = mi.Get(StreamKind.General, 0, "Actor_Character");
            General.WrittenBy           = mi.Get(StreamKind.General, 0, "WrittenBy");
            General.ScreenplayBy        = mi.Get(StreamKind.General, 0, "ScreenplayBy");
            General.EditedBy            = mi.Get(StreamKind.General, 0, "EditedBy");
            General.CommisionedBy       = mi.Get(StreamKind.General, 0, "CommisionedBy");
            General.Producer            = mi.Get(StreamKind.General, 0, "Producer");
            General.CoProducer          = mi.Get(StreamKind.General, 0, "CoProducer");
            General.ExecutiveProducer   = mi.Get(StreamKind.General, 0, "ExecutiveProducer");
            General.MusicBy             = mi.Get(StreamKind.General, 0, "MusicBy"); 
            General.DistributedBy       = mi.Get(StreamKind.General, 0, "DistributedBy");
            General.MasteredBy          = mi.Get(StreamKind.General, 0, "MasteredBy");
            General.EncodedBy           = mi.Get(StreamKind.General, 0, "EncodedBy");
            General.RemixedBy           = mi.Get(StreamKind.General, 0, "RemixedBy");
            General.ProductionStudio    = mi.Get(StreamKind.General, 0, "ProductionStudio");
            General.ThanksTo            = mi.Get(StreamKind.General, 0, "ThanksTo");
            General.Publisher           = mi.Get(StreamKind.General, 0, "Publisher");
            General.PublisherUrl        = mi.Get(StreamKind.General, 0, "Publisher/URL");
            General.Label               = mi.Get(StreamKind.General, 0, "Label");
            General.Genre               = mi.Get(StreamKind.General, 0, "Genre");
            General.Mood                = mi.Get(StreamKind.General, 0, "Mood");
            General.ContentType         = mi.Get(StreamKind.General, 0, "ContentType");
            General.Subject             = mi.Get(StreamKind.General, 0, "Subject");
            General.Description         = mi.Get(StreamKind.General, 0, "Description");
            General.Keywords            = mi.Get(StreamKind.General, 0, "Keywords");
            General.Summary             = mi.Get(StreamKind.General, 0, "Summary");
            General.Synopsis            = mi.Get(StreamKind.General, 0, "Synopsis");
            General.Period              = mi.Get(StreamKind.General, 0, "Period");
            General.LawRating           = mi.Get(StreamKind.General, 0, "LawRating");
            General.LawRatingReason     = mi.Get(StreamKind.General, 0, "LawRating_Reason");
            General.ICRA                = mi.Get(StreamKind.General, 0, "ICRA");
            General.ReleasedDate        = mi.Get(StreamKind.General, 0, "Released_Date");
            General.OriginalReleasedDate = mi.Get(StreamKind.General, 0, "Original/released_Date");
            General.RecordedDate        = mi.Get(StreamKind.General, 0, "Recorded_Date");
            General.EncodedDate         = mi.Get(StreamKind.General, 0, "Encoded_Date");
            General.TaggedDate          = mi.Get(StreamKind.General, 0, "Tagged_Date");
            General.WrittenDate         = mi.Get(StreamKind.General, 0, "Written_Date");
            General.MasteredDate        = mi.Get(StreamKind.General, 0, "Mastered_Date");
            DateTime.TryParse(mi.Get(StreamKind.General, 0, "File_Created_Date_Local"), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out General.FileCreatedDate);
            DateTime.TryParse(mi.Get(StreamKind.General, 0, "File_Modified_Date_Local"), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out General.FileModifiedDate);
            General.RecordedLocation    = mi.Get(StreamKind.General, 0, "Recorded_Location");
            General.WrittenLocation     = mi.Get(StreamKind.General, 0, "Written_Location");
            General.ArchivalLocation    = mi.Get(StreamKind.General, 0, "Archival_Location");
            General.EncodedApplication  = mi.Get(StreamKind.General, 0, "Encoded_Application");
            General.EncodedApplicationUrl = mi.Get(StreamKind.General, 0, "Encoded_Application/Url");
            General.EncodedLibrary      = mi.Get(StreamKind.General, 0, "Encoded_Library");
            General.EncodedLibraryName  = mi.Get(StreamKind.General, 0, "Encoded_Library/Name");
            General.EncodedLibraryVersion = mi.Get(StreamKind.General, 0, "Encoded_Library/Version");
            General.EncodedLibraryDate  = mi.Get(StreamKind.General, 0, "Encoded_Library/Date");
            General.EncodedLibrarySettings = mi.Get(StreamKind.General, 0, "Encoded_Library_Settings");
            General.Cropped             = mi.Get(StreamKind.General, 0, "Cropped");
            General.Dimensions          = mi.Get(StreamKind.General, 0, "Dimensions");
            Single.TryParse(mi.Get(StreamKind.General, 0, "DotsPerInch"), numStyle, AppSettings.CInfo, out General.DotsPerInch);
            General.Lightness           = mi.Get(StreamKind.General, 0, "Lightness");
            General.OriginalSourceMedium = mi.Get(StreamKind.General, 0, "OriginalSourceMedium");
            General.OriginalSourceForm  = mi.Get(StreamKind.General, 0, "OriginalSourceForm");
            General.TaggedApplication   = mi.Get(StreamKind.General, 0, "Tagged_Application");
            General.BPM                 = mi.Get(StreamKind.General, 0, "BPM");
            General.ISRC                = mi.Get(StreamKind.General, 0, "ISRC");
            General.ISBN                = mi.Get(StreamKind.General, 0, "ISBN");
            General.BarCode             = mi.Get(StreamKind.General, 0, "BarCode");
            General.LCCN                = mi.Get(StreamKind.General, 0, "LCCN");
            General.CatalogNumber       = mi.Get(StreamKind.General, 0, "CatalogNumber");
            General.LabelCode           = mi.Get(StreamKind.General, 0, "LabelCode");
            General.Owner               = mi.Get(StreamKind.General, 0, "Owner");
            General.Copyright           = mi.Get(StreamKind.General, 0, "Copyright");
            General.CopyrightUrl        = mi.Get(StreamKind.General, 0, "Copyright/Url");
            General.ProducerCopyright   = mi.Get(StreamKind.General, 0, "Producer_Copyright");
            General.TermsOfUse          = mi.Get(StreamKind.General, 0, "TermsOfUse");
            General.CoverDescription    = mi.Get(StreamKind.General, 0, "Cover_Description");
            General.CoverType           = mi.Get(StreamKind.General, 0, "Cover_Type");
            General.CoverMime           = mi.Get(StreamKind.General, 0, "Cover_Mime");
            General.CoverData           = mi.Get(StreamKind.General, 0, "CoverData");
            General.Lyrics              = mi.Get(StreamKind.General, 0, "Lyrics");
            General.Comment             = mi.Get(StreamKind.General, 0, "Comment");
            Single.TryParse(mi.Get(StreamKind.General, 0, "Rating"), numStyle, AppSettings.CInfo, out General.Rating);
        #endregion

        #region Get Video Info

            for (int i = 0; i < VideoStreams; i++)
            {
                VideoStreamInfo videoStream = new VideoStreamInfo();

                Int32.TryParse(mi.Get(StreamKind.Video, i, "Count"), numStyle, AppSettings.CInfo, out videoStream.Count);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "Status"), numStyle, AppSettings.CInfo, out videoStream.Status);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "StreamCount"), numStyle, AppSettings.CInfo, out videoStream.StreamCount);
                videoStream.StreamKind          = mi.Get(StreamKind.Video, i, "StreamKind");
                Int32.TryParse(mi.Get(StreamKind.Video, i, "StreamKindID"), numStyle, AppSettings.CInfo, out videoStream.StreamKindID);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "StreamKindPos"), numStyle, AppSettings.CInfo, out videoStream.StreamKindPos);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "ID"), numStyle, AppSettings.CInfo, out videoStream.ID);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "ID"), numStyle, AppSettings.CInfo, out videoStream.ID);
                videoStream.UniqueID            = mi.Get(StreamKind.Video, i, "UniqueID");
                videoStream.MenuID              = mi.Get(StreamKind.Video, i, "MenuID");
                videoStream.Format              = mi.Get(StreamKind.Video, i, "Format");
                videoStream.FormatInfo          = mi.Get(StreamKind.Video, i, "Format/Info");
                videoStream.FormatUrl           = mi.Get(StreamKind.Video, i, "Format/Url");
                videoStream.FormatCommercial    = mi.Get(StreamKind.Video, i, "Format/Commercial");
                videoStream.FormatCommercialIfAny = mi.Get(StreamKind.Video, i, "Format/Commercial_IfAny");
                videoStream.FormatVersion       = mi.Get(StreamKind.Video, i, "Format_Version");
                videoStream.FormatProfile       = mi.Get(StreamKind.Video, i, "Format_Profile");
                videoStream.FormatSettings      = mi.Get(StreamKind.Video, i, "Format_Settings");
                videoStream.MultiViewBaseProfile = mi.Get(StreamKind.Video, i, "MultiView_BaseProfile");
                videoStream.MultiViewCount      = mi.Get(StreamKind.Video, i, "MultiView_Count");
                videoStream.InternetMediaType   = mi.Get(StreamKind.Video, i, "InternetMediaType");
                videoStream.MuxingMode          = mi.Get(StreamKind.Video, i, "MuxingMode");
                videoStream.CodecID             = mi.Get(StreamKind.Video, i, "CodecID");
                videoStream.CodecIDInfo         = mi.Get(StreamKind.Video, i, "CodecID/Info");
                videoStream.CodecIDHint         = mi.Get(StreamKind.Video, i, "CodecID/Hint");
                videoStream.CodecIDUrl          = mi.Get(StreamKind.Video, i, "CodecID/Url");
                videoStream.CodecIDDescription  = mi.Get(StreamKind.Video, i, "CodecID_Description");
                Int64.TryParse(mi.Get(StreamKind.Video, i, "Duration"), numStyle, AppSettings.CInfo, out videoStream.Duration);
                DateTime.TryParse(mi.Get(StreamKind.Video, i, "Duration/String3"), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out videoStream.DurationTime);
                videoStream.BitRateMode         = mi.Get(StreamKind.Video, i, "BitRate_Mode");
                Int32.TryParse(mi.Get(StreamKind.Video, i, "BitRate"), numStyle, AppSettings.CInfo, out videoStream.BitRate);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "BitRate_Minimum"), numStyle, AppSettings.CInfo, out videoStream.BitRateMin);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "BitRate_Nominal"), numStyle, AppSettings.CInfo, out videoStream.BitRateNom);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "BitRate_Maximum"), numStyle, AppSettings.CInfo, out videoStream.BitRateMax);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "Width"), numStyle, AppSettings.CInfo, out videoStream.Width);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "Width_Original"), numStyle, AppSettings.CInfo, out videoStream.WidthOriginal);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "Height"), numStyle, AppSettings.CInfo, out videoStream.Height);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "Height_Original"), numStyle, AppSettings.CInfo, out videoStream.HeightOriginal);
                videoStream.PixelAspectRatio    = mi.Get(StreamKind.Video, i, "PixelAspectRatio");
                videoStream.PixelAspectRatioOriginal = mi.Get(StreamKind.Video, i, "PixelAspectRatio_Original");
                videoStream.DisplayAspectRatio  = mi.Get(StreamKind.Video, i, "DisplayAspectRatio");
                videoStream.DisplayAspectRatioOriginal = mi.Get(StreamKind.Video, i, "DisplayAspectRatio_Original");
                videoStream.Rotation            = mi.Get(StreamKind.Video, i, "Rotation");
                videoStream.FrameRateMode       = mi.Get(StreamKind.Video, i, "FrameRate_Mode");
                videoStream.FrameRateModeOriginal = mi.Get(StreamKind.Video, i, "FrameRate_Mode_Original");
                Single.TryParse(mi.Get(StreamKind.Video, i, "FrameRate"), numStyle, AppSettings.CInfo, out videoStream.FrameRate);

                Processing.GetFPSNumDenom(videoStream.FrameRate, out videoStream.FrameRateEnumerator, out videoStream.FrameRateDenominator);

                Single.TryParse(mi.Get(StreamKind.Video, i, "FrameRate_Original"), numStyle, AppSettings.CInfo, out videoStream.FrameRateOriginal);
                Single.TryParse(mi.Get(StreamKind.Video, i, "FrameRate_Minimum"), numStyle, AppSettings.CInfo, out videoStream.FrameRateMin);
                Single.TryParse(mi.Get(StreamKind.Video, i, "FrameRate_Nominal"), numStyle, AppSettings.CInfo, out videoStream.FrameRateNom);
                Single.TryParse(mi.Get(StreamKind.Video, i, "FrameRate_Maximum"), numStyle, AppSettings.CInfo, out videoStream.FrameRateMax);
                Int64.TryParse(mi.Get(StreamKind.Video, i, "FrameCount"), numStyle, AppSettings.CInfo, out videoStream.FrameCount);
                videoStream.Standard            = mi.Get(StreamKind.Video, i, "Standard");
                videoStream.ColorSpace          = mi.Get(StreamKind.Video, i, "ColorSpace");
                videoStream.ChromaSubsampling   = mi.Get(StreamKind.Video, i, "ChromaSubsampling");
                Int32.TryParse(mi.Get(StreamKind.Video, i, "BitDepth"), numStyle, AppSettings.CInfo, out videoStream.BitDepth);
                videoStream.ScanType            = mi.Get(StreamKind.Video, i, "ScanType");
                videoStream.ScanOrder           = mi.Get(StreamKind.Video, i, "ScanOrder");
                videoStream.BitsPixelPerFrame   = mi.Get(StreamKind.Video, i, "Bits-(Pixel*Frame)");
                Int32.TryParse(mi.Get(StreamKind.Video, i, "Delay"), numStyle, AppSettings.CInfo, out videoStream.Delay);
                Int32.TryParse(mi.Get(StreamKind.Video, i, "Delay_Original"), numStyle, AppSettings.CInfo, out videoStream.DelayOriginal);
                UInt64.TryParse(mi.Get(StreamKind.Video, i, "StreamSize"), numStyle, AppSettings.CInfo, out videoStream.StreamSize);
                Single.TryParse(mi.Get(StreamKind.Video, i, "StreamSize_Proportion"), numStyle, AppSettings.CInfo, out videoStream.StreamSizeProportion);
                videoStream.Alignment           = mi.Get(StreamKind.Video, i, "Alignment");
                videoStream.Title               = mi.Get(StreamKind.Video, i, "Title");
                videoStream.EncodedApplication  = mi.Get(StreamKind.Video, i, "Encoded_Application");
                videoStream.EncodedApplicationUrl = mi.Get(StreamKind.Video, i, "Encoded_Application/Url");
                videoStream.EncodedLibrary      = mi.Get(StreamKind.Video, i, "Encoded_Library");
                videoStream.EncodedLibraryName  = mi.Get(StreamKind.Video, i, "Encoded_Library/Name");
                videoStream.EncodedLibraryVersion = mi.Get(StreamKind.Video, i, "Encoded_Library/Version");
                videoStream.EncodedLibraryDate  = mi.Get(StreamKind.Video, i, "Encoded_Library/Date");
                videoStream.EncodedLibrarySettings = mi.Get(StreamKind.Video, i, "Encoded_Library_Settings");
                videoStream.Language            = mi.Get(StreamKind.Video, i, "Language");
                videoStream.LanguageFull        = mi.Get(StreamKind.Video, i, "Language/String1");
                videoStream.LanguageIso6391    = mi.Get(StreamKind.Video, i, "Language/String2");
                videoStream.LanguageIso6392    = mi.Get(StreamKind.Video, i, "Language/String3");
                videoStream.LanguageMore        = mi.Get(StreamKind.Video, i, "Language_More");
                videoStream.EncodedDate         = mi.Get(StreamKind.Video, i, "Encoded_Date");
                videoStream.TaggedDate          = mi.Get(StreamKind.Video, i, "Tagged_Date");
                videoStream.Encryption          = mi.Get(StreamKind.Video, i, "Encryption");
                videoStream.BufferSize          = mi.Get(StreamKind.Video, i, "BufferSize");

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

            for (int i = 0; i < AudioStreams; i++)
            {
                AudioStreamInfo audioStream = new AudioStreamInfo();

                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Count"), numStyle, AppSettings.CInfo, out audioStream.Count);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Status"), numStyle, AppSettings.CInfo, out audioStream.Status);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "StreamCount"), numStyle, AppSettings.CInfo, out audioStream.StreamCount);
                audioStream.StreamKind          = mi.Get(StreamKind.Audio, i, "StreamKind");
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "StreamKindID"), numStyle, AppSettings.CInfo, out audioStream.StreamKindID);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "StreamKindPos"), numStyle, AppSettings.CInfo, out audioStream.StreamKindPos);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "ID"), numStyle, AppSettings.CInfo, out audioStream.ID);
                audioStream.UniqueID            = mi.Get(StreamKind.Audio, i, "UniqueID");
                audioStream.MenuID              = mi.Get(StreamKind.Audio, i, "MenuID");
                audioStream.Format              = mi.Get(StreamKind.Audio, i, "Format");
                audioStream.FormatInfo          = mi.Get(StreamKind.Audio, i, "Format/Info");
                audioStream.FormatUrl           = mi.Get(StreamKind.Audio, i, "Format/Url");
                audioStream.FormatCommercial    = mi.Get(StreamKind.Audio, i, "Format/Commercial");
                audioStream.FormatCommercialIfAny = mi.Get(StreamKind.Audio, i, "Format/Commercial_IfAny");
                audioStream.FormatVersion       = mi.Get(StreamKind.Audio, i, "Format_Version");
                audioStream.FormatProfile       = mi.Get(StreamKind.Audio, i, "Format_Profile");
                audioStream.FormatSettings      = mi.Get(StreamKind.Audio, i, "Format_Settings");
                audioStream.InternetMediaType   = mi.Get(StreamKind.Audio, i, "InternetMediaType");
                audioStream.MuxingMode          = mi.Get(StreamKind.Audio, i, "MuxingMode");
                audioStream.MuxingModeMore      = mi.Get(StreamKind.Audio, i, "MuxingMode_MoreInfo");
                audioStream.CodecID             = mi.Get(StreamKind.Audio, i, "CodecID");
                audioStream.CodecIDInfo         = mi.Get(StreamKind.Audio, i, "CodecID/Info");
                audioStream.CodecIDHint         = mi.Get(StreamKind.Audio, i, "CodecID/Hint");
                audioStream.CodecIDUrl          = mi.Get(StreamKind.Audio, i, "CodecID/Url");
                audioStream.CodecIDDescription  = mi.Get(StreamKind.Audio, i, "CodecID_Description");
                Int64.TryParse(mi.Get(StreamKind.Audio, i, "Duration"), numStyle, AppSettings.CInfo, out audioStream.Duration);
                DateTime.TryParse(mi.Get(StreamKind.Audio, i, "Duration/String3"), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out audioStream.DurationTime);
                audioStream.BitRateMode         = mi.Get(StreamKind.Audio, i, "BitRate_Mode");
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "BitRate"), numStyle, AppSettings.CInfo, out audioStream.BitRate);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "BitRate_Minimum"), numStyle, AppSettings.CInfo, out audioStream.BitRateMin);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "BitRate_Nominal"), numStyle, AppSettings.CInfo, out audioStream.BitRateNom);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "BitRate_Maximum"), numStyle, AppSettings.CInfo, out audioStream.BitRateMax);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Channel(s)"), numStyle, AppSettings.CInfo, out audioStream.Channels);
                audioStream.ChannelsString      = mi.Get(StreamKind.Audio, i, "Channel(s)/String");
                audioStream.ChannelPositionsString    = mi.Get(StreamKind.Audio, i, "ChannelPositions/String2");
                audioStream.ChannelPositions    = mi.Get(StreamKind.Audio, i, "ChannelPositions");
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "SamplingRate"), numStyle, AppSettings.CInfo, out audioStream.SamplingRate);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "SamplingCount"), numStyle, AppSettings.CInfo, out audioStream.SamplingCount);
                Int64.TryParse(mi.Get(StreamKind.Audio, i, "FrameCount"), numStyle, AppSettings.CInfo, out audioStream.FrameCount);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "BitDepth"), numStyle, AppSettings.CInfo, out audioStream.BitDepth);
                audioStream.CompressionRate     = mi.Get(StreamKind.Audio, i, "Compression_Ratio");
                audioStream.CompressionMode     = mi.Get(StreamKind.Audio, i, "Compression_Mode");
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Delay"), numStyle, AppSettings.CInfo, out audioStream.Delay);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Delay_Original"), numStyle, AppSettings.CInfo, out audioStream.DelayOriginal);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Video_Delay"), numStyle, AppSettings.CInfo, out audioStream.VideoDelay);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Video0_Delay"), numStyle, AppSettings.CInfo, out audioStream.Video0Delay);
                Single.TryParse(mi.Get(StreamKind.Audio, i, "ReplayGain_Gain"), numStyle, AppSettings.CInfo, out audioStream.ReplayGain);
                Single.TryParse(mi.Get(StreamKind.Audio, i, "ReplayGain_Peak"), numStyle, AppSettings.CInfo, out audioStream.ReplayGainPeak);
                UInt64.TryParse(mi.Get(StreamKind.Audio, i, "StreamSize"), numStyle, AppSettings.CInfo, out audioStream.StreamSize);
                Single.TryParse(mi.Get(StreamKind.Audio, i, "StreamSize_Proportion"), numStyle, AppSettings.CInfo, out audioStream.StreamSizeProportion);
                audioStream.Alignment           = mi.Get(StreamKind.Audio, i, "Alignment");
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Interleave_VideoFrames"), numStyle, AppSettings.CInfo, out audioStream.InterleaveVideoFrames);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Interleave_Duration"), numStyle, AppSettings.CInfo, out audioStream.InterleaveDuration);
                Int32.TryParse(mi.Get(StreamKind.Audio, i, "Interleave_Preload"), numStyle, AppSettings.CInfo, out audioStream.InterleavePreload);
                audioStream.Title               = mi.Get(StreamKind.Audio, i, "Title");
                audioStream.EncodedLibrary      = mi.Get(StreamKind.Audio, i, "Encoded_Library");
                audioStream.EncodedLibraryName  = mi.Get(StreamKind.Audio, i, "Encoded_Library/Name");
                audioStream.EncodedLibraryVersion = mi.Get(StreamKind.Audio, i, "Encoded_Library/Version");
                audioStream.EncodedLibraryDate  = mi.Get(StreamKind.Audio, i, "Encoded_Library/Date");
                audioStream.EncodedLibrarySettings = mi.Get(StreamKind.Audio, i, "Encoded_Library_Settings");
                audioStream.Language            = mi.Get(StreamKind.Audio, i, "Language");
                audioStream.LanguageFull        = mi.Get(StreamKind.Audio, i, "Language/String1");
                audioStream.LanguageIso6391    = mi.Get(StreamKind.Audio, i, "Language/String2");
                audioStream.LanguageIso6392    = mi.Get(StreamKind.Audio, i, "Language/String3");
                audioStream.LanguageMore        = mi.Get(StreamKind.Audio, i, "Language_More");
                audioStream.EncodedDate         = mi.Get(StreamKind.Audio, i, "Encoded_Date");
                audioStream.TaggedDate          = mi.Get(StreamKind.Audio, i, "Tagged_Date");
                audioStream.Encryption          = mi.Get(StreamKind.Audio, i, "Encryption");

                Audio.Add(audioStream);
            }
        #endregion

        #region Get Chapters Info

            for (int i = 0; i < ChapterCount; i++)
            {
                ChaptersInfo chapter = new ChaptersInfo();

                Int32.TryParse(mi.Get(StreamKind.Chapters, i, "Count"), numStyle, AppSettings.CInfo, out chapter.Count);
                Int32.TryParse(mi.Get(StreamKind.Chapters, i, "Status"), numStyle, AppSettings.CInfo, out chapter.Status);
                Int32.TryParse(mi.Get(StreamKind.Chapters, i, "StreamCount"), numStyle, AppSettings.CInfo, out chapter.StreamCount);
                chapter.StreamKind              = mi.Get(StreamKind.Chapters, i, "StreamKind");
                Int32.TryParse(mi.Get(StreamKind.Chapters, i, "StreamKindID"), numStyle, AppSettings.CInfo, out chapter.StreamKindID);
                Int32.TryParse(mi.Get(StreamKind.Chapters, i, "StreamKindPos"), numStyle, AppSettings.CInfo, out chapter.StreamKindPos);
                Int32.TryParse(mi.Get(StreamKind.Chapters, i, "ID"), numStyle, AppSettings.CInfo, out chapter.ID);
                chapter.UniqueID                = mi.Get(StreamKind.Chapters, i, "UniqueID");
                chapter.MenuID                  = mi.Get(StreamKind.Chapters, i, "MenuID");
                chapter.Format                  = mi.Get(StreamKind.Chapters, i, "Format");
                chapter.FormatInfo              = mi.Get(StreamKind.Chapters, i, "Format/Info");
                chapter.FormatUrl               = mi.Get(StreamKind.Chapters, i, "Format/Url");
                Int32.TryParse(mi.Get(StreamKind.Chapters, i, "Total"), numStyle, AppSettings.CInfo, out chapter.Total);
                chapter.Title                   = mi.Get(StreamKind.Chapters, i, "Title");
                chapter.Language                = mi.Get(StreamKind.Chapters, i, "Language");
                chapter.LanguageFull            = mi.Get(StreamKind.Chapters, i, "Language/String");

                Chapters.Add(chapter);
            }
        #endregion

        #region Get Image Info

            for (int i = 0; i < ImageStreams; i++)
            {
                ImageStreamInfo imageStream = new ImageStreamInfo();

                Int32.TryParse(mi.Get(StreamKind.Image, i, "Count"), numStyle, AppSettings.CInfo, out imageStream.Count);
                Int32.TryParse(mi.Get(StreamKind.Image, i, "Status"), numStyle, AppSettings.CInfo, out imageStream.Status);
                Int32.TryParse(mi.Get(StreamKind.Image, i, "StreamCount"), numStyle, AppSettings.CInfo, out imageStream.StreamCount);
                imageStream.StreamKind              = mi.Get(StreamKind.Image, i, "StreamKind");
                Int32.TryParse(mi.Get(StreamKind.Image, i, "StreamKindID"), numStyle, AppSettings.CInfo, out imageStream.StreamKindID);
                Int32.TryParse(mi.Get(StreamKind.Image, i, "StreamKindPos"), numStyle, AppSettings.CInfo, out imageStream.StreamKindPos);
                Int32.TryParse(mi.Get(StreamKind.Image, i, "ID"), numStyle, AppSettings.CInfo, out imageStream.ID);
                imageStream.UniqueID                = mi.Get(StreamKind.Image, i, "UniqueID");
                imageStream.MenuID                  = mi.Get(StreamKind.Image, i, "MenuID");
                imageStream.Title                   = mi.Get(StreamKind.Image, i, "Title");
                imageStream.Format                  = mi.Get(StreamKind.Image, i, "Format");
                imageStream.FormatInfo              = mi.Get(StreamKind.Image, i, "Format/Info");
                imageStream.FormatUrl               = mi.Get(StreamKind.Image, i, "Format/Url");
                imageStream.FormatCommercial        = mi.Get(StreamKind.Image, i, "Format/Commercial");
                imageStream.FormatCommercialIfAny   = mi.Get(StreamKind.Image, i, "Format/Commercial_IfAny");
                imageStream.FormatVersion           = mi.Get(StreamKind.Image, i, "Format_Version");
                imageStream.FormatProfile           = mi.Get(StreamKind.Image, i, "Format_Profile");
                imageStream.FormatSettings          = mi.Get(StreamKind.Image, i, "Format_Settings");
                imageStream.InternetMediaType       = mi.Get(StreamKind.Image, i, "InternetMediaType");
                imageStream.CodecID                 = mi.Get(StreamKind.Image, i, "CodecID");
                imageStream.CodecIDInfo             = mi.Get(StreamKind.Image, i, "CodecID/Info");
                imageStream.CodecIDHint             = mi.Get(StreamKind.Image, i, "CodecID/Hint");
                imageStream.CodecIDUrl              = mi.Get(StreamKind.Image, i, "CodecID/Url");
                imageStream.CodecIDDescription      = mi.Get(StreamKind.Image, i, "CodecID_Description");
                Int32.TryParse(mi.Get(StreamKind.Image, i, "Width"), numStyle, AppSettings.CInfo, out imageStream.Width);
                Int32.TryParse(mi.Get(StreamKind.Image, i, "Height"), numStyle, AppSettings.CInfo, out imageStream.Height);
                imageStream.ColorSpace              = mi.Get(StreamKind.Image, i, "ColorSpace");
                imageStream.ChromaSubsampling       = mi.Get(StreamKind.Image, i, "ChromaSubsampling");
                Int32.TryParse(mi.Get(StreamKind.Image, i, "BitDepth"), numStyle, AppSettings.CInfo, out imageStream.BitDepth);
                imageStream.BitDepthStr             = mi.Get(StreamKind.Image, i, "BitDepth/String");
                imageStream.CompressionMode         = mi.Get(StreamKind.Image, i, "Compression_Mode/String");
                imageStream.CompressionRate         = mi.Get(StreamKind.Image, i, "Compression_Ratio");
                UInt64.TryParse(mi.Get(StreamKind.Image, i, "StreamSize"), numStyle, AppSettings.CInfo, out imageStream.StreamSize);
                Single.TryParse(mi.Get(StreamKind.Image, i, "StreamSize_Proportion"), numStyle, AppSettings.CInfo, out imageStream.StreamSizeProportion);
                imageStream.EncodedLibrary          = mi.Get(StreamKind.Image, i, "Encoded_Library");
                imageStream.EncodedLibraryName      = mi.Get(StreamKind.Image, i, "Encoded_Library/Name");
                imageStream.EncodedLibraryVersion   = mi.Get(StreamKind.Image, i, "Encoded_Library/Version");
                imageStream.EncodedLibraryDate      = mi.Get(StreamKind.Image, i, "Encoded_Library/Date");
                imageStream.EncodedLibrarySettings  = mi.Get(StreamKind.Image, i, "Encoded_Library_Settings");
                imageStream.Language                = mi.Get(StreamKind.Image, i, "Language");
                imageStream.LanguageFull            = mi.Get(StreamKind.Image, i, "Language/String1");
                imageStream.LanguageIso6391        = mi.Get(StreamKind.Image, i, "Language/String2");
                imageStream.LanguageIso6392        = mi.Get(StreamKind.Image, i, "Language/String3");
                imageStream.EncodedDate             = mi.Get(StreamKind.Image, i, "Encoded_Date");
                imageStream.TaggedDate              = mi.Get(StreamKind.Image, i, "Tagged_Date");
                imageStream.Encryption              = mi.Get(StreamKind.Image, i, "Encryption");

                Image.Add(imageStream);
            }
        #endregion

        #region Get Text Info

            for (int i = 0; i < TextStreams; i++)
            {
                TextStreamInfo textStream = new TextStreamInfo();

                Int32.TryParse(mi.Get(StreamKind.Text, i, "Count"), numStyle, AppSettings.CInfo, out textStream.Count);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "Status"), numStyle, AppSettings.CInfo, out textStream.Status);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "StreamCount"), numStyle, AppSettings.CInfo, out textStream.StreamCount);
                textStream.StreamKind               = mi.Get(StreamKind.Text, i, "StreamKind");
                Int32.TryParse(mi.Get(StreamKind.Text, i, "StreamKindID"), numStyle, AppSettings.CInfo, out textStream.StreamKindID);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "StreamKindPos"), numStyle, AppSettings.CInfo, out textStream.StreamKindPos);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "ID"), numStyle, AppSettings.CInfo, out textStream.ID);
                textStream.UniqueID                 = mi.Get(StreamKind.Text, i, "UniqueID");
                textStream.MenuID                   = mi.Get(StreamKind.Text, i, "MenuID");
                textStream.Format                   = mi.Get(StreamKind.Text, i, "Format");
                textStream.FormatInfo               = mi.Get(StreamKind.Text, i, "Format/Info");
                textStream.FormatUrl                = mi.Get(StreamKind.Text, i, "Format/Url");
                textStream.FormatCommercial         = mi.Get(StreamKind.Text, i, "Format/Commercial");
                textStream.FormatCommercialIfAny    = mi.Get(StreamKind.Text, i, "Format/Commercial_IfAny");
                textStream.FormatVersion            = mi.Get(StreamKind.Text, i, "Format_Version");
                textStream.FormatProfile            = mi.Get(StreamKind.Text, i, "Format_Profile");
                textStream.FormatSettings           = mi.Get(StreamKind.Text, i, "Format_Settings");
                textStream.InternetMediaType        = mi.Get(StreamKind.Text, i, "InternetMediaType");
                textStream.MuxingMode               = mi.Get(StreamKind.Text, i, "MuxingMode");
                textStream.CodecID                  = mi.Get(StreamKind.Text, i, "CodecID");
                textStream.CodecIDInfo              = mi.Get(StreamKind.Text, i, "CodecID/Info");
                textStream.CodecIDHint              = mi.Get(StreamKind.Text, i, "CodecID/Hint");
                textStream.CodecIDUrl               = mi.Get(StreamKind.Text, i, "CodecID/Url");
                textStream.CodecIDDescription       = mi.Get(StreamKind.Text, i, "CodecID_Description");
                Int64.TryParse(mi.Get(StreamKind.Text, i, "Duration"), numStyle, AppSettings.CInfo, out textStream.Duration);
                textStream.BitRateMode              = mi.Get(StreamKind.Text, i, "BitRate_Mode");
                Int32.TryParse(mi.Get(StreamKind.Text, i, "BitRate"), numStyle, AppSettings.CInfo, out textStream.BitRate);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "BitRate_Minimum"), numStyle, AppSettings.CInfo, out textStream.BitRateMin);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "BitRate_Nominal"), numStyle, AppSettings.CInfo, out textStream.BitRateNom);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "BitRate_Maximum"), numStyle, AppSettings.CInfo, out textStream.BitRateMax);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "Width"), numStyle, AppSettings.CInfo, out textStream.Width);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "Height"), numStyle, AppSettings.CInfo, out textStream.Height);
                textStream.FrameRateMode            = mi.Get(StreamKind.Text, i, "FrameRate_Mode");
                Single.TryParse(mi.Get(StreamKind.Text, i, "FrameRate"), numStyle, AppSettings.CInfo, out textStream.FrameRate);
                Single.TryParse(mi.Get(StreamKind.Text, i, "FrameRate_Original"), numStyle, AppSettings.CInfo, out textStream.FrameRateOriginal);
                Single.TryParse(mi.Get(StreamKind.Text, i, "FrameRate_Minimum"), numStyle, AppSettings.CInfo, out textStream.FrameRateMin);
                Single.TryParse(mi.Get(StreamKind.Text, i, "FrameRate_Nominal"), numStyle, AppSettings.CInfo, out textStream.FrameRateNom);
                Single.TryParse(mi.Get(StreamKind.Text, i, "FrameRate_Maximum"), numStyle, AppSettings.CInfo, out textStream.FrameRateMax);
                Int64.TryParse(mi.Get(StreamKind.Text, i, "FrameCount"), numStyle, AppSettings.CInfo, out textStream.FrameCount);
                textStream.ColorSpace               = mi.Get(StreamKind.Text, i, "ColorSpace");
                textStream.ChromaSubsampling        = mi.Get(StreamKind.Text, i, "ChromaSubsampling");
                Int32.TryParse(mi.Get(StreamKind.Text, i, "BitDepth"), numStyle, AppSettings.CInfo, out textStream.BitDepth);
                textStream.CompressionMode          = mi.Get(StreamKind.Text, i, "Compression_Mode/String");
                textStream.CompressionRate          = mi.Get(StreamKind.Text, i, "Compression_Ratio");
                Int32.TryParse(mi.Get(StreamKind.Text, i, "Delay"), numStyle, AppSettings.CInfo, out textStream.Delay);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "Delay_Original"), numStyle, AppSettings.CInfo, out textStream.DelayOriginal);
                Int32.TryParse(mi.Get(StreamKind.Text, i, "Video_Delay"), numStyle, AppSettings.CInfo, out textStream.VideoDelay);
                UInt64.TryParse(mi.Get(StreamKind.Text, i, "StreamSize"), numStyle, AppSettings.CInfo, out textStream.StreamSize);
                Single.TryParse(mi.Get(StreamKind.Text, i, "StreamSize_Proportion"), numStyle, AppSettings.CInfo, out textStream.StreamSizeProportion);
                textStream.Title                    = mi.Get(StreamKind.Text, i, "Title");
                textStream.EncodedLibrary           = mi.Get(StreamKind.Text, i, "Encoded_Library");
                textStream.EncodedLibraryName       = mi.Get(StreamKind.Text, i, "Encoded_Library/Name");
                textStream.EncodedLibraryVersion    = mi.Get(StreamKind.Text, i, "Encoded_Library/Version");
                textStream.EncodedLibraryDate       = mi.Get(StreamKind.Text, i, "Encoded_Library/Date");
                textStream.EncodedLibrarySettings   = mi.Get(StreamKind.Text, i, "Encoded_Library_Settings");
                textStream.Language                 = mi.Get(StreamKind.Text, i, "Language");
                textStream.LanguageFull             = mi.Get(StreamKind.Text, i, "Language/String1");
                textStream.LanguageIso6391         = mi.Get(StreamKind.Text, i, "Language/String2");
                textStream.LanguageIso6392         = mi.Get(StreamKind.Text, i, "Language/String3");
                textStream.LanguageMore             = mi.Get(StreamKind.Text, i, "Language_More");
                textStream.EncodedDate              = mi.Get(StreamKind.Text, i, "Encoded_Date");
                textStream.TaggedDate               = mi.Get(StreamKind.Text, i, "Tagged_Date");
                textStream.Encryption               = mi.Get(StreamKind.Text, i, "Encryption");

                Text.Add(textStream);
            }
        #endregion

        #region Get Menu Info

            for (int i = 0; i < MenuCount; i++)
            {
                MenuStreamInfo menuStream = new MenuStreamInfo();

                Int32.TryParse(mi.Get(StreamKind.Menu, i, "Count"), numStyle, AppSettings.CInfo, out menuStream.Count);
                Int32.TryParse(mi.Get(StreamKind.Menu, i, "Status"), numStyle, AppSettings.CInfo, out menuStream.Status);
                Int32.TryParse(mi.Get(StreamKind.Menu, i, "StreamCount"), numStyle, AppSettings.CInfo, out menuStream.StreamCount);
                menuStream.StreamKind               = mi.Get(StreamKind.Menu, i, "StreamKind");
                Int32.TryParse(mi.Get(StreamKind.Menu, i, "StreamKindID"), numStyle, AppSettings.CInfo, out menuStream.StreamKindID);
                Int32.TryParse(mi.Get(StreamKind.Menu, i, "StreamKindPos"), numStyle, AppSettings.CInfo, out menuStream.StreamKindPos);
                Int32.TryParse(mi.Get(StreamKind.Menu, i, "ID"), numStyle, AppSettings.CInfo, out menuStream.ID);
                menuStream.UniqueID                 = mi.Get(StreamKind.Menu, i, "UniqueID");
                menuStream.MenuID                   = mi.Get(StreamKind.Menu, i, "MenuID");
                menuStream.Format                   = mi.Get(StreamKind.Menu, i, "Format");
                menuStream.FormatInfo               = mi.Get(StreamKind.Menu, i, "Format/Info");
                menuStream.FormatUrl                = mi.Get(StreamKind.Menu, i, "Format/Url");
                menuStream.FormatCommercial         = mi.Get(StreamKind.Menu, i, "Format/Commercial");
                menuStream.FormatCommercialIfAny    = mi.Get(StreamKind.Menu, i, "Format/Commercial_IfAny");
                menuStream.FormatVersion            = mi.Get(StreamKind.Menu, i, "Format_Version");
                menuStream.FormatProfile            = mi.Get(StreamKind.Menu, i, "Format_Profile");
                menuStream.FormatSettings           = mi.Get(StreamKind.Menu, i, "Format_Settings");
                menuStream.CodecID                  = mi.Get(StreamKind.Menu, i, "CodecID");
                menuStream.CodecIDInfo              = mi.Get(StreamKind.Menu, i, "CodecID/Info");
                menuStream.CodecIDHint              = mi.Get(StreamKind.Menu, i, "CodecID/Hint");
                menuStream.CodecIDUrl               = mi.Get(StreamKind.Menu, i, "CodecID/Url");
                menuStream.CodecIDDescription       = mi.Get(StreamKind.Menu, i, "CodecID_Description");
                Int64.TryParse(mi.Get(StreamKind.Menu, i, "Duration"), numStyle, AppSettings.CInfo, out menuStream.Duration);
                Int32.TryParse(mi.Get(StreamKind.Menu, i, "Delay"), numStyle, AppSettings.CInfo, out menuStream.Delay);
                menuStream.DelaySettings            = mi.Get(StreamKind.Menu, i, "Delay_Settings");
                menuStream.DelaySource              = mi.Get(StreamKind.Menu, i, "Delay_Source");
                menuStream.ListStreamKind           = mi.Get(StreamKind.Menu, i, "List_StreamKind");
                menuStream.ListStreamPos            = mi.Get(StreamKind.Menu, i, "List_StreamPos");
                menuStream.List                     = mi.Get(StreamKind.Menu, i, "List");
                menuStream.Title                    = mi.Get(StreamKind.Menu, i, "Title");
                menuStream.Language                 = mi.Get(StreamKind.Menu, i, "Language");
                menuStream.LanguageFull             = mi.Get(StreamKind.Menu, i, "Language/String1");
                menuStream.LanguageIso6391         = mi.Get(StreamKind.Menu, i, "Language/String2");
                menuStream.LanguageIso6392         = mi.Get(StreamKind.Menu, i, "Language/String3");
                menuStream.LanguageMore             = mi.Get(StreamKind.Menu, i, "Language_More");
                Int32.TryParse(mi.Get(StreamKind.Menu, i, "Chapters_Pos_Begin"), numStyle, AppSettings.CInfo, out menuStream.ChaptersPosBegin);
                Int32.TryParse(mi.Get(StreamKind.Menu, i, "Chapters_Pos_End"), numStyle, AppSettings.CInfo, out menuStream.ChaptersPosEnd);

                for (int j = menuStream.ChaptersPosBegin; j < menuStream.ChaptersPosEnd; j++)
                {
                    DateTime tempTime;
                    DateTime.TryParse(mi.Get(StreamKind.Menu, i, j, InfoKind.Name), AppSettings.CInfo, DateTimeStyles.AssumeLocal, out tempTime);
                    MyChapters.Add(tempTime.TimeOfDay);
                }

                

                Menu.Add(menuStream);
            }
        #endregion

            mi.Close();
        }
    }
}