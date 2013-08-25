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

using System.Drawing;

namespace VideoConvert.Core.Subtitles
{
    public class SubtitleStyle
    {
        public string FontName;
        public int FontSize;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public Color OutlineColor;
        public Color BackColor;
        public bool Bold;
        public bool Italic;
        public bool Underline;
        public bool StrikeThrough;
        public int BorderStyle;
        public int Outline;
        public int Shadow;
        public int Alignment;
        public int MarginL;
        public int MarginR;
        public int MarginV;
        public int AlphaLevel;
        public string Encoding;

        public SubtitleStyle()
        {
            FontName = string.Empty;
            FontSize = 0;
            PrimaryColor = Color.White;
            SecondaryColor = Color.WhiteSmoke;
            OutlineColor = Color.Black;
            BackColor = Color.Black;
            Bold = false;
            Italic = false;
            Underline = false;
            StrikeThrough = false;
            BorderStyle = 0;
            Outline = 0;
            Shadow = 0;
            Alignment = 0;
            MarginL = 0;
            MarginR = 0;
            MarginV = 0;
            AlphaLevel = 0;
            Encoding = string.Empty;
        }
    }
}