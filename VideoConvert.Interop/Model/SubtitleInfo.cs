// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubtitleInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Subtitle Stream properties
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System;

    /// <summary>
    /// contains information for a subtitle stream
    /// </summary>
    public class SubtitleInfo : IFormattable
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id;

        /// <summary>
        /// Stream ID
        /// </summary>
        public int StreamId;

        /// <summary>
        /// Stream Format
        /// </summary>
        public string Format;

        /// <summary>
        /// Language Code
        /// </summary>
        public string LangCode;

        /// <summary>
        /// Temp file name
        /// </summary>
        public string TempFile;

        /// <summary>
        /// Stream kind ID
        /// </summary>
        public int StreamKindId;

        /// <summary>
        /// Strean Delay
        /// </summary>
        public int Delay;

        /// <summary>
        /// Stream ID used for Demuxing
        /// </summary>
        public int DemuxStreamId;

        /// <summary>
        /// Stream size in bytes
        /// </summary>
        public ulong StreamSize;

        /// <summary>
        /// Keep only forced captions during processing
        /// </summary>
        public bool KeepOnlyForcedCaptions;

        /// <summary>
        /// Hardsub into video during encoding process
        /// </summary>
        public bool HardSubIntoVideo;

        /// <summary>
        /// Make stream default
        /// </summary>
        public bool MkvDefault;

        /// <summary>
        /// Stream has raw format
        /// </summary>
        public bool RawStream;

        /// <summary>
        /// Stream needs conversion
        /// </summary>
        public bool NeedConversion;

        /// <summary>
        /// Is supported format
        /// </summary>
        public bool FormatSupported;

        /// <summary>
        /// Default constuctor
        /// </summary>
        public SubtitleInfo()
        {
            Id = int.MinValue;
            StreamId = int.MinValue;
            Format = string.Empty;
            LangCode = string.Empty;
            TempFile = string.Empty;
            StreamKindId = int.MinValue;
            Delay = int.MinValue;
            DemuxStreamId = int.MinValue;
            StreamSize = ulong.MinValue;
            KeepOnlyForcedCaptions = false;
            HardSubIntoVideo = false;
            MkvDefault = false;
            RawStream = false;
            NeedConversion = false;
            FormatSupported = true;
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

            result += $"SubtitleInfo.ID:              {Id:0} {Environment.NewLine}";
            result += $"SubtitleInfo.StreamID:        {StreamId:0} {Environment.NewLine}";
            result += $"SubtitleInfo.Format:          {Format} {Environment.NewLine}";
            result += $"SubtitleInfo.LangCode:        {LangCode} {Environment.NewLine}";
            result += $"SubtitleInfo.TempFile:        {TempFile} {Environment.NewLine}";
            result += $"SubtitleInfo.StreamKindID:    {StreamKindId:0} {Environment.NewLine}";
            result += $"SubtitleInfo.Delay:           {Delay:0} {Environment.NewLine}";
            result += $"SubtitleInfo.DemuxStreamID:   {DemuxStreamId:0} {Environment.NewLine}";
            result += $"SubtitleInfo.StreamSize:      {StreamSize:0} {Environment.NewLine}";
            
            return result;
        }
    }
}