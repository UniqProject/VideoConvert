// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBCertCountry.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Certification Country for TheMovieDB Lib
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    /// <summary>
    /// Certification Country for TheMovieDB Lib
    /// </summary>
    public class MovieDbCertCountry
    {
        /// <summary>
        /// Full name
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Rating prefix
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MovieDbCertCountry ()
        {
            CountryName = string.Empty;
            Prefix = string.Empty;
        }
    }
}