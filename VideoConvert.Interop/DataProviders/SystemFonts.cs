// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemFonts.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Installed system fonts
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.DataProviders
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;

    /// <summary>
    /// Installed system fonts
    /// </summary>
    public class SystemFonts
    {
        /// <summary>
        /// List of installed font families
        /// </summary>
        public static List<FontFamily> SystemFontFamilies
        {
            get
            {
                return Fonts.SystemFontFamilies.OrderBy(family => family.Source).ToList();
            }
        }
    }
}