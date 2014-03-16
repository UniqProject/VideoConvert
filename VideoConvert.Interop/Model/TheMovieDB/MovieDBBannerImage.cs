// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBBannerImage.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Banner image for TheMovieDB
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    /// <summary>
    /// Banner image for TheMovieDB
    /// </summary>
    public class MovieDbBannerImage : MovieDbPosterImage
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MovieDbBannerImage()
        {
            Aspect = "banner";
        }
    }
}