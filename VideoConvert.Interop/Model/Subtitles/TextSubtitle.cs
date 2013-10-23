// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextSubtitle.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Subtitles
{
    using System.Collections.Generic;
    using System.Drawing;

    public class TextSubtitle
    {
        public SubtitleStyle Style;
        public List<SubCaption> Captions;

        public TextSubtitle()
        {
            Style = new SubtitleStyle();
            Captions = new List<SubCaption>();
        }

        public void SetDefaultStyle()
        {
            Style.FontName = "Microsoft Sans Serif";
            Style.FontSize = 20;
            Style.PrimaryColor = Color.White;
            Style.SecondaryColor = Color.WhiteSmoke;
            Style.OutlineColor = Color.Black;
            Style.BackColor = Color.Black;
            Style.Bold = false;
            Style.Italic = false;
            Style.BorderStyle = 1;
            Style.Outline = 1;
            Style.Shadow = 2;
            Style.Alignment = 2;
            Style.MarginL = 10;
            Style.MarginR = 10;
            Style.MarginV = 10;
            Style.AlphaLevel = 0;
            Style.Encoding = "0";
        }
    }
}
