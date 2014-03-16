// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBSeasonPosterImage.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Poster image for TV-Show Season
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Xml.Serialization;

    /// <summary>
    /// Poster image for TV-Show Season
    /// </summary>
    public class MovieDbSeasonPosterImage : MovieDbPosterImage
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
        public MovieDbSeasonPosterImage()
        {
            Type = "season";
            Season = -1;
        }
    }
}