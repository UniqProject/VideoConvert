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
using System.Drawing;

namespace VideoConvert.Core
{
    /// <summary>
    /// contains information for a video stream
    /// </summary>
    public class VideoInfo:IFormattable
    {
        /// <summary>
        /// Framerate of the Video stream
        /// </summary>
        public float FPS;
        /// <summary>
        /// encoding format
        /// </summary>
        public string Format;
        public string FormatProfile;
        /// <summary>
        /// stream id in container
        /// </summary>
        public int StreamId;
        /// <summary>
        /// id of the video title set (only used for dvd)
        /// </summary>
        public int VtsId;

        public int TrackId;
        public int StreamKindID;
        public string TempFile;
        public bool Interlaced;
        public VideoFormat PicSize;
        public int DemuxStreamId;
        public List<string> DemuxStreamNames;
        public int DemuxPlayList;
        public int Width;
        public int Height;
        public long FrameCount;
        public bool Encoded;
        public bool IsRawStream;
        public ulong StreamSize;
        public double Length;
        public float AspectRatio;
        public long Bitrate;
        public Rectangle CropRect;
        public int FrameRateEnumerator;
        public int FrameRateDenominator;
        public string FrameMode;

        public VideoInfo()
        {
            FPS = float.NaN;
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
            string result = string.Empty;

            result += string.Format(AppSettings.CInfo, "VideoInfo.FPS:              {0:g} {1:s}", FPS,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.Format:           {0:s} {1:s}", Format,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.FormatProfile:    {0:s} {1:s}", FormatProfile,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.StreamID:         {0:g} {1:s}", StreamId,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.VTS_ID:           {0:g} {1:s}", VtsId,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.TrackID:          {0:g} {1:s}", TrackId,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.TempFile:         {0:s} {1:s}", TempFile,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.Interlaced:       {0:s} {1:s}",
                                    Interlaced.ToString(AppSettings.CInfo), Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.PicSize:          {0:s} {1:s}", PicSize.ToString("F"),
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.DemuxStreamID:    {0:g} {1:s}", DemuxStreamId,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.DemuxStreamNames: {0:s} {1:s}",
                                    string.Join(",", DemuxStreamNames.ToArray()), Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.DemuxPlayList:    {0:g} {1:s}", DemuxPlayList,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.Width:            {0:g} {1:s}", Width,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.Height:           {0:g} {1:s}", Height,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.FrameCount:       {0:g} {1:s}", FrameCount,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.encoded:          {0:s} {1:s}",
                                    Encoded.ToString(AppSettings.CInfo), Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.isRawStream:      {0:s} {1:s}",
                                    IsRawStream.ToString(AppSettings.CInfo), Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.StreamSize:       {0:g} {1:s}", StreamSize,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.Length:           {0:g} {1:s}", Length,
                                    Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "VideoInfo.AspectRatio:      {0:g} {1:s}", AspectRatio,
                                    Environment.NewLine);
            return result;
        }
    }
}