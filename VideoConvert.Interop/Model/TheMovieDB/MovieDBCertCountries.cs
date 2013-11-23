// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBCertCountries.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   List of Certification countries supported by TheMovieDB Lib
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System.Collections.Generic;

    /// <summary>
    /// List of Certification countries supported by TheMovieDB Lib
    /// </summary>
    public static class MovieDbCertCountries
    {
        /// <summary>
        /// List of Certification countries supported by TheMovieDB Lib
        /// </summary>
        public static List<MovieDbCertCountry> CountryList { get { return GenerateCountryList(); } }

        /// <summary>
        /// Generates a List of Certification countries supported by TheMovieDB Lib
        /// </summary>
        /// <returns></returns>
        private static List<MovieDbCertCountry> GenerateCountryList()
        {
            var result = new List<MovieDbCertCountry>
                {
                    new MovieDbCertCountry {CountryName = "au", Prefix = "AU-"},
                    new MovieDbCertCountry {CountryName = "bg", Prefix = "BG-"},
                    new MovieDbCertCountry {CountryName = "cs", Prefix = "CS-"},
                    new MovieDbCertCountry {CountryName = "da", Prefix = "DA-"},
                    new MovieDbCertCountry {CountryName = "de", Prefix = "DE-"},
                    new MovieDbCertCountry {CountryName = "el", Prefix = "EL-"},
                    new MovieDbCertCountry {CountryName = "es", Prefix = "ES-"},
                    new MovieDbCertCountry {CountryName = "fi", Prefix = "FI-"},
                    new MovieDbCertCountry {CountryName = "fr", Prefix = "FR-"},
                    new MovieDbCertCountry {CountryName = "gb", Prefix = "GB-"},
                    new MovieDbCertCountry {CountryName = "he", Prefix = "HE-"},
                    new MovieDbCertCountry {CountryName = "hr", Prefix = "HR-"},
                    new MovieDbCertCountry {CountryName = "hu", Prefix = "HU-"},
                    new MovieDbCertCountry {CountryName = "it", Prefix = "IT-"},
                    new MovieDbCertCountry {CountryName = "ja", Prefix = "JA-"},
                    new MovieDbCertCountry {CountryName = "ko", Prefix = "KO-"},
                    new MovieDbCertCountry {CountryName = "nl", Prefix = "NL-"},
                    new MovieDbCertCountry {CountryName = "no", Prefix = "NO-"},
                    new MovieDbCertCountry {CountryName = "pl", Prefix = "PL-"},
                    new MovieDbCertCountry {CountryName = "pt", Prefix = "PT-"},
                    new MovieDbCertCountry {CountryName = "ru", Prefix = "RU-"},
                    new MovieDbCertCountry {CountryName = "sl", Prefix = "SL-"},
                    new MovieDbCertCountry {CountryName = "sv", Prefix = "SV-"},
                    new MovieDbCertCountry {CountryName = "th", Prefix = "TH-"},
                    new MovieDbCertCountry {CountryName = "tr", Prefix = "TR-"},
                    new MovieDbCertCountry {CountryName = "us", Prefix = "US-"},
                    new MovieDbCertCountry {CountryName = "zh", Prefix = "ZH-"}
                };
            return result;
        }
    }
}
