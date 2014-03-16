// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBCast.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Movie cast entry
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Xml.Serialization;

    /// <summary>
    /// Movie cast entry
    /// </summary>
    public class MovieDbCast
    {
        /// <summary>
        /// Name
        /// </summary>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Role
        /// </summary>
        [XmlElement("role")]
        public string Role { get; set; }

        /// <summary>
        /// Image
        /// </summary>
        [XmlElement("thumb")]
        public string Thumbnail { get; set; }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public MovieDbCast()
        {
            Thumbnail = string.Empty;
            Name = string.Empty;
            Role = string.Empty;
        }
    }
}