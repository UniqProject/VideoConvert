// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBCastList.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Collections.Generic;

    public class MovieDBCastList
    {
        public List<MovieDBCast> Casts { get; set; }
 
        public MovieDBCastList()
        {
            Casts = new List<MovieDBCast>();
        }
    }
}
