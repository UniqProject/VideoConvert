// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VideoInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Video stream information container
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;

    /// <summary>
    /// Video stream information container
    /// </summary>
    public class VideoInfo:IFormattable
    {
        /// <summary>
        /// Framerate of the Video stream
        /// </summary>
        public float Fps;

        /// <summary>
        /// encoding format
        /// </summary>
        public string Format;

        /// <summary>
        /// Format Profile
        /// </summary>
        public string FormatProfile;

        /// <summary>
        /// stream id in container
        /// </summary>
        public int StreamId;

        /// <summary>
        /// id of the video title set (only used for dvd)
        /// </summary>
        public int VtsId;

        /// <summary>
        /// Track ID
        /// </summary>
        public int TrackId;

        /// <summary>
        /// Stream kind ID
        /// </summary>
        public int StreamKindID;

        /// <summary>
        /// Temp file name
        /// </summary>
        public string TempFile;

        /// <summary>
        /// Interlaced stream
        /// </summary>
        public bool Interlaced;

        /// <summary>
        /// Stream resolution
        /// </summary>
        public VideoFormat PicSize;

        /// <summary>
        /// Stream ID used while demuxing
        /// </summary>
        public int DemuxStreamId;

        /// <summary>
        /// Stream names used while demuxing
        /// </summary>
        public List<string> DemuxStreamNames;

        /// <summary>
        /// BD Playlist used while demuxing
        /// </summary>
        public int DemuxPlayList;

        /// <summary>
        /// Video width
        /// </summary>
        public int Width;

        /// <summary>
        /// Video height
        /// </summary>
        public int Height;

        /// <summary>
        /// Video stream frame count
        /// </summary>
        public long FrameCount;

        /// <summary>
        /// Encoded stream
        /// </summary>
        public bool Encoded;

        /// <summary>
        /// Raw stream
        /// </summary>
        public bool IsRawStream;

        /// <summary>
        /// Stream size in bytes
        /// </summary>
        public ulong StreamSize;

        /// <summary>
        /// Stream duration in seconds
        /// </summary>
        public double Length;

        /// <summary>
        /// Video aspect ratio
        /// </summary>
        public float AspectRatio;

        /// <summary>
        /// Video bitrate
        /// </summary>
        public long Bitrate;

        /// <summary>
        /// Crop rectangle
        /// </summary>
        public Rectangle CropRect;

        /// <summary>
        /// Framerate Enumerator
        /// </summary>
        public int FrameRateEnumerator;

        /// <summary>
        /// Framerate Denominator
        /// </summary>
        public int FrameRateDenominator;

        /// <summary>
        /// Frame mode
        /// </summary>
        public string FrameMode;

        /// <summary>
        /// Default constructor
        /// </summary>
        public VideoInfo()
        {
            Fps = float.NaN;
            Format = string.Empty;
            FormatProfile = string.Empty;
            StreamId = int.MinValue;
            VtsId = int.MinValue;
            TrackId = int.MinValue;
            StreamKindID = int.MinValue;
            TempFile = string.Empty;
            Interlaced = false;
            PicSize = VideoFormat.Unknown;
            DemuxStreamId = int.MinValue;
            DemuxStreamNames = new List<string>();
            DemuxPlayList = int.MinValue;
            Width = int.MinValue;
            Height = int.MinValue;
            FrameCount = long.MinValue;
            Encoded = false;
            IsRawStream = false;
            StreamSize = ulong.MinValue;
            Length = double.NaN;
            AspectRatio = float.NaN;
            Bitrate = long.MinValue;
            CropRect = new Rectangle();
            FrameRateEnumerator = int.MinValue;
            FrameRateDenominator = int.MinValue;
            FrameMode = string.Empty;
        }

        /// <summary>
        /// Gibt einen <see cref="T:System.String"/> zurück, der das aktuelle <see cref="T:System.Object"/> darstellt.
        /// </summary>
        /// <returns>
        /// Ein <see cref="T:System.String"/>, der das aktuelle <see cref="T:System.Object"/> darstellt.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>
        /// Formatiert den Wert der aktuellen Instanz unter Verwendung des angegebenen Formats.
        /// </summary>
        /// <returns>
        /// Der Wert der aktuellen Instanz im angegebenen Format.
        /// </returns>
        /// <param name="format">Das zu verwendende Format.– oder – Ein NULL-Verweis (Nothing in Visual Basic),
        ///  wenn das für den Typ der <see cref="T:System.IFormattable"/> -Implementierung definierte Standardformat verwendet werden soll. </param>
        /// <param name="formatProvider">Der zum Formatieren des Werts zu verwendende Anbieter.– oder – Ein NULL-Verweis (Nothing in Visual Basic),
        ///  wenn die Informationen über numerische Formate dem aktuellen Gebietsschema des Betriebssystems entnommen werden sollen. </param>
        /// <filterpriority>2</filterpriority>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            var result = string.Empty;

            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.FPS:              {0:g} {1:s}", Fps,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.Format:           {0:s} {1:s}", Format,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.FormatProfile:    {0:s} {1:s}", FormatProfile,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.StreamID:         {0:g} {1:s}", StreamId,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.VTS_ID:           {0:g} {1:s}", VtsId,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.TrackID:          {0:g} {1:s}", TrackId,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.TempFile:         {0:s} {1:s}", TempFile,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.Interlaced:       {0:s} {1:s}",
                                    Interlaced.ToString(CultureInfo.InvariantCulture), Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.PicSize:          {0:s} {1:s}", PicSize.ToString("F"),
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.DemuxStreamID:    {0:g} {1:s}", DemuxStreamId,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.DemuxStreamNames: {0:s} {1:s}",
                                    string.Join(",", DemuxStreamNames.ToArray()), Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.DemuxPlayList:    {0:g} {1:s}", DemuxPlayList,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.Width:            {0:g} {1:s}", Width,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.Height:           {0:g} {1:s}", Height,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.FrameCount:       {0:g} {1:s}", FrameCount,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.encoded:          {0:s} {1:s}",
                                    Encoded.ToString(CultureInfo.InvariantCulture), Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.isRawStream:      {0:s} {1:s}",
                                    IsRawStream.ToString(CultureInfo.InvariantCulture), Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.StreamSize:       {0:g} {1:s}", StreamSize,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.Length:           {0:g} {1:s}", Length,
                                    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "VideoInfo.AspectRatio:      {0:g} {1:s}", AspectRatio,
                                    Environment.NewLine);
            return result;
        }
    }
}