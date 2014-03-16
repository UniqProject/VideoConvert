// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBImageInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   TheMovieDB image
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Xml.Serialization;

    /// <summary>
    /// TheMovieDB image
    /// </summary>
    public class MovieDbImageInfo
    {
        /// <summary>
        /// image title
        /// </summary>
        [XmlIgnore]
        public string Title { get; set; }

        /// <summary>
        /// Url to Preview image
        /// </summary>
        [XmlAttribute("preview")]
        public string UrlPreview { get; set; }

        /// <summary>
        /// Url to Original image
        /// </summary>
        [XmlText]
        public string UrlOriginal { get; set; }

        /// <summary>
        /// Serializer property
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeUrlPreview()
        {
            return !(string.IsNullOrEmpty(UrlPreview));
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MovieDbImageInfo()
        {
            Title = string.Empty;
            UrlPreview = string.Empty;
            UrlOriginal = string.Empty;
        }
    }
}