// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DBTvShowSeason.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   TV-Show season
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Collections.Generic;

    /// <summary>
    /// TV-Show season
    /// </summary>
    public class DBTvShowSeason
    {
        /// <summary>
        /// Season Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Season number
        /// </summary>
        public int SeasonNumber { get; set; }

        /// <summary>
        /// Show specials
        /// </summary>
        public bool IsSpecial { get; set; }

        /// <summary>
        /// List of episodes
        /// </summary>
        public List<DBTvShowEpisode> Episodes { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DBTvShowSeason()
        {
            Title = string.Empty;
            SeasonNumber = 0;
            IsSpecial = false;
            Episodes = new List<DBTvShowEpisode>();
        }
    }
}