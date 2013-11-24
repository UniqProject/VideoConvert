// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubtitleStyle.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Font stryle for subtitle captions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Subtitles
{
    using System.Drawing;

    /// <summary>
    /// Font stryle for subtitle captions
    /// </summary>
    public class SubtitleStyle
    {
        /// <summary>
        /// Font Name
        /// </summary>
        public string FontName;

        /// <summary>
        /// Font size
        /// </summary>
        public int FontSize;

        /// <summary>
        /// Foreground color
        /// </summary>
        public Color PrimaryColor;

        /// <summary>
        /// Foreground color for special captions
        /// </summary>
        public Color SecondaryColor;

        /// <summary>
        /// Border/outline color
        /// </summary>
        public Color OutlineColor;

        /// <summary>
        /// Background color
        /// </summary>
        public Color BackColor;

        /// <summary>
        /// Font style bold
        /// </summary>
        public bool Bold;

        /// <summary>
        /// Font style italic
        /// </summary>
        public bool Italic;

        /// <summary>
        /// Font style underlined
        /// </summary>
        public bool Underline;

        /// <summary>
        /// Font style strike-through
        /// </summary>
        public bool StrikeThrough;

        /// <summary>
        /// Border style
        /// </summary>
        public int BorderStyle;

        /// <summary>
        /// Outline / border width
        /// </summary>
        public int Outline;

        /// <summary>
        /// Shadow width
        /// </summary>
        public int Shadow;

        /// <summary>
        /// Caption alignment
        /// </summary>
        public int Alignment;

        /// <summary>
        /// Left margin
        /// </summary>
        public int MarginL;

        /// <summary>
        /// Right margin
        /// </summary>
        public int MarginR;

        /// <summary>
        /// Vertical margin
        /// </summary>
        public int MarginV;

        /// <summary>
        /// Alpha level
        /// </summary>
        public int AlphaLevel;

        /// <summary>
        /// Encoding
        /// </summary>
        public string Encoding;

        /// <summary>
        /// Default constructor
        /// </summary>
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