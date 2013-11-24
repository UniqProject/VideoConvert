// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImdbApiVersion.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   IMDB API version
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.IMDB
{
    using System.Xml.Serialization;

    /// <summary>
    /// IMDB API version
    /// </summary>
    [XmlRoot("version")]
    public class ImdbApiVersion
    {
        /// <summary>
        /// API version
        /// </summary>
        [XmlElement("api")]
        public string Version { get; set; }

        /// <summary>
        /// Date of last Database change
        /// </summary>
        [XmlElement("database")]
        public string DatabaseDate { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ImdbApiVersion()
        {
            Version = string.Empty;
            DatabaseDate = string.Empty;
        }
    }
}