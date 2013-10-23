// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBCast.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Xml.Serialization;

    public class MovieDBCast
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("role")]
        public string Role { get; set; }

        [XmlElement("thumb")]
        public string Thumbnail { get; set; }
        
        public MovieDBCast()
        {
            Thumbnail = string.Empty;
            Name = string.Empty;
            Role = string.Empty;
        }
    }
}