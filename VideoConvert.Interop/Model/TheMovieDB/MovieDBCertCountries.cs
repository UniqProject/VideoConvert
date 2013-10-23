// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBCertCountries.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Collections.Generic;

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
