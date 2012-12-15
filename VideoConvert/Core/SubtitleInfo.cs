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

namespace VideoConvert.Core
{
    /// <summary>
    /// contains information for a subtitle stream
    /// </summary>
    public class SubtitleInfo:IFormattable
    {
        public int Id;
        public int StreamId;
        public string Format;
        public string LangCode;
        public string TempFile;
        public int StreamKindId;
        public int Delay;
        public int DemuxStreamId;
        public ulong StreamSize;
        public bool KeepOnlyForcedCaptions;
        public bool HardSubIntoVideo;
        public bool MkvDefault;

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

            result += string.Format(AppSettings.CInfo, "SubtitleInfo.ID:              {0:g} {1:s}", Id,             Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "SubtitleInfo.StreamID:        {0:g} {1:s}", StreamId,       Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "SubtitleInfo.Format:          {0:s} {1:s}", Format,         Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "SubtitleInfo.LangCode:        {0:s} {1:s}", LangCode,       Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "SubtitleInfo.TempFile:        {0:s} {1:s}", TempFile,       Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "SubtitleInfo.StreamKindID:    {0:g} {1:s}", StreamKindId,   Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "SubtitleInfo.Delay:           {0:g} {1:s}", Delay,          Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "SubtitleInfo.DemuxStreamID:   {0:g} {1:s}", DemuxStreamId,  Environment.NewLine);
            result += string.Format(AppSettings.CInfo, "SubtitleInfo.StreamSize:      {0:g} {1:s}", StreamSize,     Environment.NewLine);
            
            return result;
        }
    }
}