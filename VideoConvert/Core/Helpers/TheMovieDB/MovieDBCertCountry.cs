namespace VideoConvert.Core.Helpers.TheMovieDB
{
    public class MovieDBCertCountry
    {
        public string CountryName { get; set; }
        public string Prefix { get; set; }
        public MovieDBCertCountry ()
        {
            CountryName = string.Empty;
            Prefix = string.Empty;
        }
    }
}