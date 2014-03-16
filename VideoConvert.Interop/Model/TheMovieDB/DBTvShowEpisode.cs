// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DBTvShowEpisode.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   TV Show Episode
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// TV Show Episode
    /// </summary>
    public class DbTvShowEpisode
    {
        /// <summary>
        /// Episode Title
        /// </summary>
        public string EpisodeTitle { get; set; }

        /// <summary>
        /// Episode number in season
        /// </summary>
        public int EpisodeNumber { get; set; }

        /// <summary>
        /// Absolute episode number
        /// </summary>
        public int AbsoluteEpisodeNumber { get; set; }

        /// <summary>
        /// Episode number in DVD ordering
        /// </summary>
        public double DvdEpisodeNumber { get; set; }

        /// <summary>
        /// Combined episode number
        /// </summary>
        public double CombinedEpisodeNumber { get; set; }

        /// <summary>
        /// Season number
        /// </summary>
        public int SeasonNumber { get; set; }

        /// <summary>
        /// Is episode special
        /// </summary>
        public bool IsSpecial { get; set; }

        /// <summary>
        /// IMDB id
        /// </summary>
        public string ImdbId { get; set; }

        /// <summary>
        /// First aired date
        /// </summary>
        public DateTime FirstAired { get; set; }

        /// <summary>
        /// User rating
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// Episode length
        /// </summary>
        public int Runtime { get; set; }

        /// <summary>
        /// Writers
        /// </summary>
        public List<string> Writers { get; set; }

        /// <summary>
        /// Concatenated list of writers
        /// </summary>
        public string WritersString
        {
            get { return Writers != null ? string.Join(" / ", Writers) : string.Empty; }
        }

        /// <summary>
        /// Directors
        /// </summary>
        public List<string> Directors { get; set; }

        /// <summary>
        /// Concatenated list of directors
        /// </summary>
        public string DirectorsString
        {
            get { return Directors != null ? string.Join(" / ", Directors) : string.Empty; }
        }

        /// <summary>
        /// Guest stars
        /// </summary>
        public List<string> GuestStars { get; set; }

        /// <summary>
        /// Concatenated list of guest stars
        /// </summary>
        public string GuestStarsString
        {
            get { return GuestStars != null ? string.Join(" / ", GuestStars) : string.Empty; }
        }

        /// <summary>
        /// Episode plot
        /// </summary>
        public string Plot { get; set; }

        /// <summary>
        /// Episode image url
        /// </summary>
        public string EpisodeImageUrl { get; set; }
    }
}