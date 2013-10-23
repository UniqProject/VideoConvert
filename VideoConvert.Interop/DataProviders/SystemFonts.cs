// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemFonts.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.DataProviders
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;

    public class SystemFonts
    {
        public static List<FontFamily> SystemFontFamilies
        {
            get
            {
                return Fonts.SystemFontFamilies.OrderBy(family => family.Source).ToList();
            }
        }
    }
}