// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBImageInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Xml.Serialization;

    public class MovieDBImageInfo
    {
        [XmlIgnore]
        public string Title { get; set; }

        [XmlAttribute("preview")]
        public string UrlPreview { get; set; }

        [XmlText]
        public string UrlOriginal { get; set; }

        public bool ShouldSerializeUrlPreview()
        {
            return !(string.IsNullOrEmpty(UrlPreview));
        }

        public MovieDBImageInfo()
        {
            Title = string.Empty;
            UrlPreview = string.Empty;
            UrlOriginal = string.Empty;
        }
    }
}