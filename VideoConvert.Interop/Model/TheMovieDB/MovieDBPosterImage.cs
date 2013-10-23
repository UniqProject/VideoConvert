// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBPosterImage.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Xml.Serialization;

    public class MovieDBPosterImage : MovieDBImageInfo
    {
        [XmlAttribute("aspect")]
        public string Aspect { get; set; }
        
        public MovieDBPosterImage()
        {
            Aspect = "poster";
        }
    }
}