// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBCastList.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Movie Casts
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Collections.Generic;

    /// <summary>
    /// Movie Casts
    /// </summary>
    public class MovieDBCastList
    {
        /// <summary>
        /// Casts
        /// </summary>
        public List<MovieDBCast> Casts { get; set; }
 
        /// <summary>
        /// Default constructor
        /// </summary>
        public MovieDBCastList()
        {
            Casts = new List<MovieDBCast>();
        }
    }
}
