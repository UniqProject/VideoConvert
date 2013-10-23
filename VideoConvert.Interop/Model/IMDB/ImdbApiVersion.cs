// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImdbApiVersion.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.IMDB
{
    using System.Xml.Serialization;

    [XmlRoot("version")]
    public class ImdbApiVersion
    {
        [XmlElement("api")]
        public string Version { get; set; }

        [XmlElement("database")]
        public string DatabaseDate { get; set; }

        public ImdbApiVersion()
        {
            Version = string.Empty;
            DatabaseDate = string.Empty;
        }
    }
}