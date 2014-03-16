// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBSeasonBannerImage.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Banner image for TV-Show season
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Xml.Serialization;

    /// <summary>
    /// Banner image for TV-Show season
    /// </summary>
    public class MovieDbSeasonBannerImage : MovieDbBannerImage
    {
        /// <summary>
        /// image type
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// Show season
        /// </summary>
        [XmlAttribute("season")]
        public int Season { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MovieDbSeasonBannerImage()
        {
            Type = "season";
            Season = -1;
        }
    }
}