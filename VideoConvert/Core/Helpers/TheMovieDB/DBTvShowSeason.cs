using System.Collections.Generic;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
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