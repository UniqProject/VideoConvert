// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubtitleStyle.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Subtitles
{
    using System.Drawing;

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