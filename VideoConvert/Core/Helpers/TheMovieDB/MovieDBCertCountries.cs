//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

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
