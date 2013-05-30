using System;
using System.Collections.Generic;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
    public class DBTvShowEpisode
    {
        public string EpisodeTitle { get; set; }
        public int EpisodeNumber { get; set; }
        public int AbsoluteEpisodeNumber { get; set; }
        public double DvdEpisodeNumber { get; set; }
        public double CombinedEpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public bool IsSpecial { get; set; }
        public string ImdbId { get; set; }
        public DateTime FirstAired { get; set; }
        public double Rating { get; set; }
        public int Runtime { get; set; }
        public List<string> Writers { get; set; }
        public string WritersString
        {
            get { return Writers != null ? string.Join(" / ", Writers) : string.Empty; }
        }

        public List<string> Directors { get; set; }
        public string DirectorsString
        {
            get { return Directors != null ? string.Join(" / ", Directors) : string.Empty; }
        }

        public List<string> GuestStars { get; set; }
        public string GuestStarsString
        {
            get { return GuestStars != null ? string.Join(" / ", GuestStars) : string.Empty; }
        }

        public string Plot { get; set; }
        public string EpisodeImageUrl { get; set; }
    }
}