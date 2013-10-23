// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImdbAKAEntry.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.IMDB
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class ImdbAKAEntry
    {
        [XmlArray("remarks")]
        [XmlArrayItem("item")]
        public List<string> Remarks { get; set; }

        [XmlElement("country")]
        public string Country { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        public ImdbAKAEntry()
        {
            Remarks = new List<string>();
            Country = string.Empty;
            Title = string.Empty;
        }
    }
}