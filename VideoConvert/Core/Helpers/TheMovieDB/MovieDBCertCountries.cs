using System.Collections.Generic;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
    public static class MovieDBCertCountries
    {
        public static List<MovieDBCertCountry> CountryList { get { return GenerateCountryList(); } }

        private static List<MovieDBCertCountry> GenerateCountryList()
        {
            List<MovieDBCertCountry> result = new List<MovieDBCertCountry>
                {
                    new MovieDBCertCountry {CountryName = "au", Prefix = "AU-"},
                    new MovieDBCertCountry {CountryName = "bg", Prefix = "BG-"},
                    new MovieDBCertCountry {CountryName = "cs", Prefix = "CS-"},
                    new MovieDBCertCountry {CountryName = "da", Prefix = "DA-"},
                    new MovieDBCertCountry {CountryName = "de", Prefix = "DE-"},
                    new MovieDBCertCountry {CountryName = "el", Prefix = "EL-"},
                    new MovieDBCertCountry {CountryName = "es", Prefix = "ES-"},
                    new MovieDBCertCountry {CountryName = "fi", Prefix = "FI-"},
                    new MovieDBCertCountry {CountryName = "fr", Prefix = "FR-"},
                    new MovieDBCertCountry {CountryName = "gb", Prefix = "GB-"},
                    new MovieDBCertCountry {CountryName = "he", Prefix = "HE-"},
                    new MovieDBCertCountry {CountryName = "hr", Prefix = "HR-"},
                    new MovieDBCertCountry {CountryName = "hu", Prefix = "HU-"},
                    new MovieDBCertCountry {CountryName = "it", Prefix = "IT-"},
                    new MovieDBCertCountry {CountryName = "ja", Prefix = "JA-"},
                    new MovieDBCertCountry {CountryName = "ko", Prefix = "KO-"},
                    new MovieDBCertCountry {CountryName = "nl", Prefix = "NL-"},
                    new MovieDBCertCountry {CountryName = "no", Prefix = "NO-"},
                    new MovieDBCertCountry {CountryName = "pl", Prefix = "PL-"},
                    new MovieDBCertCountry {CountryName = "pt", Prefix = "PT-"},
                    new MovieDBCertCountry {CountryName = "ru", Prefix = "RU-"},
                    new MovieDBCertCountry {CountryName = "sl", Prefix = "SL-"},
                    new MovieDBCertCountry {CountryName = "sv", Prefix = "SV-"},
                    new MovieDBCertCountry {CountryName = "th", Prefix = "TH-"},
                    new MovieDBCertCountry {CountryName = "tr", Prefix = "TR-"},
                    new MovieDBCertCountry {CountryName = "us", Prefix = "US-"},
                    new MovieDBCertCountry {CountryName = "zh", Prefix = "ZH-"}
                };
            return result;
        }
    }
}
