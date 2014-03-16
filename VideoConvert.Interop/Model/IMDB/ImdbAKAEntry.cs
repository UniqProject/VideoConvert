// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImdbAKAEntry.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   AKA (also known as) movie entry for IMDB
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.IMDB
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// AKA (also known as) movie entry for IMDB
    /// </summary>
    public class ImdbAkaEntry
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
        /// Title
        /// </summary>
        [XmlElement("title")]
        public string Title { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ImdbAkaEntry()
        {
            Remarks = new List<string>();
            Country = string.Empty;
            Title = string.Empty;
        }
    }
}