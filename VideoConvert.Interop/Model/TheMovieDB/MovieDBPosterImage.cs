// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBPosterImage.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   TheMovieDB poster image
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Xml.Serialization;

    /// <summary>
    /// TheMovieDB poster image
    /// </summary>
    public class MovieDBPosterImage : MovieDBImageInfo
    {
        /// <summary>
        /// image aspect
        /// </summary>
        [XmlAttribute("aspect")]
        public string Aspect { get; set; }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public MovieDBPosterImage()
        {
            Aspect = "poster";
        }
    }
}