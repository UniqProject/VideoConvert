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

            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.ID:              {0:g} {1:s}", Id, Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.StreamID:        {0:g} {1:s}", StreamId, Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.Format:          {0:s} {1:s}", Format, Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.FormatProfile:   {0:s} {1:s}", FormatProfile, Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.LangCode:        {0:s} {1:s}", LangCode,      Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.ShortLang:       {0:s} {1:s}", ShortLang,     Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.TempFile:        {0:s} {1:s}", TempFile,      Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.OriginalID:      {0:g} {1:s}", OriginalId,    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.StreamKindID:    {0:g} {1:s}", StreamKindId,  Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.Delay:           {0:g} {1:s}", Delay,         Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.Bitrate:         {0:g} {1:s}", Bitrate,       Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.DemuxStreamID:   {0:g} {1:s}", DemuxStreamId, Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.SampleRate:      {0:g} {1:s}", SampleRate,    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.ChannelCount:    {0:g} {1:s}", ChannelCount,  Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.StreamSize:      {0:g} {1:s}", StreamSize,    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.Length:          {0:g} {1:s}", Length,        Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.IsHDStream:      {0:g} {1:s}", IsHdStream,    Environment.NewLine);
            result += string.Format(CultureInfo.InvariantCulture, "AudioInfo.MkvDefault:      {0:g} {1:s}", MkvDefault,    Environment.NewLine);

            return result;

        }
    }
}