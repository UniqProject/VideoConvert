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
using System.Drawing;

namespace VideoConvert.Core.Subtitles
{
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
            Style.FontName = AppSettings.TSMuxeRSubtitleFont.Source;
            Style.FontSize = AppSettings.TSMuxeRSubtitleFontSize;
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
