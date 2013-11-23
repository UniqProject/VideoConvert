// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBLanguage.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Language definition for TheMovieDB Lib
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    /// <summary>
    /// Language definition for TheMovieDB Lib
    /// </summary>
    public class MovieDbLanguage
    {
        /// <summary>
        /// ISO-Abbreviation
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Full-length language name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MovieDbLanguage()
        {
            Code = string.Empty;
            Name = string.Empty;
        }
    }
}