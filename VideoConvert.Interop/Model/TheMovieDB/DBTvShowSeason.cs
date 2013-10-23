// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DBTvShowSeason.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Collections.Generic;

    public class DBTvShowSeason
    {
        public string Title { get; set; }
        public int SeasonNumber { get; set; }
        public bool IsSpecial { get; set; }
        public List<DBTvShowEpisode> Episodes { get; set; }

        public DBTvShowSeason()
        {
            Title = string.Empty;
            SeasonNumber = 0;
            IsSpecial = false;
            Episodes = new List<DBTvShowEpisode>();
        }
    }
}