// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImdbReleaseDateEntry.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   IMDB Release Date entry
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.IMDB
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// IMDB Release Date entry
    /// </summary>
    public class ImdbReleaseDateEntry
    {
        /// <summary>
        /// Remarks
        /// </summary>
        [XmlArray("remarks")]
        [XmlArrayItem("item")]
        public List<string> Remarks { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        [XmlElement("country")]
        public string Country { get; set; }

        /// <summary>
        /// Release year
        /// </summary>
        [XmlElement("year")]
        public int Year { get; set; }

        /// <summary>
        /// Release month
        /// </summary>
        [XmlElement("month")]
        public int Month { get; set; }

        /// <summary>
        /// Release day
        /// </summary>
        [XmlElement("day")]
        public int Day { get; set; }

        /// <summary>
        /// Release Date
        /// </summary>
        [XmlIgnore]
        public DateTime Date => new DateTime(Year, Month, Day);

        /// <summary>
        /// Default constructor
        /// </summary>
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