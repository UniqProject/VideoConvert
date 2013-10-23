// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBLanguage.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    public class MovieDBLanguage
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public MovieDBLanguage()
        {
            Code = string.Empty;
            Name = string.Empty;
        }
    }
}