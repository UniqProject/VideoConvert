// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Audio stream information container
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Audio stream information container
    /// </summary>
    public class AudioInfo:IFormattable
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
        /// Stream format
        /// </summary>
        public string Format;

        /// <summary>
        /// Stream format profile
        /// </summary>
        public string FormatProfile;

        /// <summary>
        /// Language code
        /// </summary>
        public string LangCode;

        /// <summary>
        /// Language
        /// </summary>
        public string ShortLang;

        /// <summary>
        /// Temp file name
        /// </summary>
        public string TempFile;

        /// <summary>
        /// Original stream ID
        /// </summary>
        public int OriginalId;

        /// <summary>
        /// Stream kind ID
        /// </summary>
        public int StreamKindId;

        /// <summary>
        /// Stream delay
        /// </summary>
        public int Delay;

        /// <summary>
        /// Stream bitrate
        /// </summary>
        public long Bitrate;

        /// <summary>
        /// Stream ID used while demuxing
        /// </summary>
        public int DemuxStreamId;

        /// <summary>
        /// Stream Sample Rate
        /// </summary>
        public int SampleRate;

        /// <summary>
        /// Stream Channel count
        /// </summary>
        public int ChannelCount;

        /// <summary>
        /// Stream Bit depth
        /// </summary>
        public int BitDepth;

        /// <summary>
        /// Stream size in bytes
        /// </summary>
        public ulong StreamSize;

        /// <summary>
        /// Stream duration in seconds
        /// </summary>
        public double Length;

        /// <summary>
        /// HD Audio (TrueHD / DTS-HR / DTS-MA)
        /// </summary>
        public bool IsHdStream;

        /// <summary>
        /// Default stream for MKV output
        /// </summary>
        public bool MkvDefault;

        /// <summary>
        /// Default constructor
        /// </summary>
        public AudioInfo()
        {
            Id = int.MinValue;
            StreamId = int.MinValue;
            Format = string.Empty;
            FormatProfile = string.Empty;
            LangCode = string.Empty;
            ShortLang = string.Empty;
            TempFile = string.Empty;
            OriginalId = int.MinValue;
            StreamKindId = int.MinValue;
            Delay = int.MinValue;
            Bitrate = long.MinValue;
            DemuxStreamId = int.MinValue;
            SampleRate = int.MinValue;
            ChannelCount = int.MinValue;
            BitDepth = int.MinValue;
            StreamSize = ulong.MinValue;
            Length = double.NaN;
            IsHdStream = false;
            MkvDefault = false;
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
        /// wenn das für den Typ der <see cref="T:System.IFormattable"/> -Implementierung definierte Standardformat verwendet werden soll. </param>
        /// <param name="formatProvider">Der zum Formatieren des Werts zu verwendende Anbieter.– oder – Ein NULL-Verweis (Nothing in Visual Basic),
        ///  wenn die Informationen über numerische Formate dem aktuellen Gebietsschema des Betriebssystems entnommen werden sollen. </param>
        /// <filterpriority>2</filterpriority>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            var result = string.Empty;

            result += $"AudioInfo.ID:              {Id:0} {Environment.NewLine}";
            result += $"AudioInfo.StreamID:        {StreamId:0} {Environment.NewLine}";
            result += $"AudioInfo.Format:          {Format} {Environment.NewLine}";
            result += $"AudioInfo.FormatProfile:   {FormatProfile} {Environment.NewLine}";
            result += $"AudioInfo.LangCode:        {LangCode} {Environment.NewLine}";
            result += $"AudioInfo.ShortLang:       {ShortLang} {Environment.NewLine}";
            result += $"AudioInfo.TempFile:        {TempFile} {Environment.NewLine}";
            result += $"AudioInfo.OriginalID:      {OriginalId:0} {Environment.NewLine}";
            result += $"AudioInfo.StreamKindID:    {StreamKindId:0} {Environment.NewLine}";
            result += $"AudioInfo.Delay:           {Delay:0} {Environment.NewLine}";
            result += $"AudioInfo.Bitrate:         {Bitrate:0} {Environment.NewLine}";
            result += $"AudioInfo.DemuxStreamID:   {DemuxStreamId:0} {Environment.NewLine}";
            result += $"AudioInfo.SampleRate:      {SampleRate:0} {Environment.NewLine}";
            result += $"AudioInfo.ChannelCount:    {ChannelCount:0} {Environment.NewLine}";
            result += $"AudioInfo.StreamSize:      {StreamSize:0} {Environment.NewLine}";
            result += $"AudioInfo.Length:          {Length:0.###} {Environment.NewLine}".ToString(CultureInfo.InvariantCulture);
            result += $"AudioInfo.IsHDStream:      {IsHdStream} {Environment.NewLine}";
            result += $"AudioInfo.MkvDefault:      {MkvDefault} {Environment.NewLine}";

            return result;

        }
    }
}