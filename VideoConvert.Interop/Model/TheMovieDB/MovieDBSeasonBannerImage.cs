// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBSeasonBannerImage.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Xml.Serialization;

    public class MovieDBSeasonBannerImage : MovieDBBannerImage
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("season")]
        public int Season { get; set; }

        public MovieDBSeasonBannerImage()
        {
            Type = "season";
            Season = -1;
        }
    }
}