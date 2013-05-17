using System.Collections.Generic;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
    public class MovieDBCastList
    {
        public List<MovieDBCast> Casts { get; set; }
 
        public MovieDBCastList()
        {
            Casts = new List<MovieDBCast>();
        }
    }
}
