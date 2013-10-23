// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBCertCountry.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
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