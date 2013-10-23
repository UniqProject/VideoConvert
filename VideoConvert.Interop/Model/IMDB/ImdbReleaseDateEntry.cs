// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImdbReleaseDateEntry.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.IMDB
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class ImdbReleaseDateEntry
    {
        [XmlArray("remarks")]
        [XmlArrayItem("item")]
        public List<string> Remarks { get; set; }

        [XmlElement("country")]
        public string Country { get; set; }

        [XmlElement("year")]
        public int Year { get; set; }

        [XmlElement("month")]
        public int Month { get; set; }

        [XmlElement("day")]
        public int Day { get; set; }

        [XmlIgnore]
        public DateTime Date { get { return new DateTime(Year, Month, Day); } }

        public ImdbReleaseDateEntry()
        {
            Remarks = new List<string>();
            Country = string.Empty;
            Year = 0;
            Month = 0;
            Day = 0;
        }
    }
}