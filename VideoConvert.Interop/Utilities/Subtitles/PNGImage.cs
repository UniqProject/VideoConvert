// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PNGImage.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   PNG (Portable network graphics) container
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities.Subtitles
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Model.Subtitles;

    /// <summary>
    /// PNG (Portable network graphics) container
    /// </summary>
    public class PngImage
    {
        /// <summary>
        /// Creates an PNG image for given Subtitle caption
        /// </summary>
        /// <param name="caption">Subtitle caption</param>
        /// <param name="style">Subtitle style</param>
        /// <param name="number">Caption number</param>
        /// <param name="videoWidth">Video width</param>
        /// <param name="videoHeight">Video height</param>
        /// <param name="baseFName">File base name</param>
        /// <returns>Generated PNG image</returns>
        public static ImageHolder CreateImage(SubCaption caption, SubtitleStyle style, int number, int videoWidth, int videoHeight, string baseFName)
        {
            _boldStyle = false;
            _italicStyle = false;
            _underlineStyle = false;
            _strikeStyle = false;

            var result = new ImageHolder();
            if (string.IsNullOrEmpty(baseFName)) return new ImageHolder();

            var basePath = Path.GetDirectoryName(baseFName);
            var baseName = Path.GetFileNameWithoutExtension(baseFName);
            if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(baseName)) return new ImageHolder();
            
            result.FileName = string.Format("{0}_{1:00000}.png", Path.Combine(basePath, baseName), number);
            var imgSize = new SizeF();

            var styleFontStyle = FontStyle.Regular;
            if (style.Bold)
                styleFontStyle = styleFontStyle | FontStyle.Bold;
            if (style.Italic)
                styleFontStyle = styleFontStyle | FontStyle.Italic;

            var styleFont = new Font(style.FontName, style.FontSize, styleFontStyle, GraphicsUnit.Point);
            var stringFormat = new StringFormat();

            var lineSizes = new List<SizeF>();

            var rawText = Regex.Replace(caption.Text, "</*?(?:i|b)>", "", RegexOptions.Singleline | RegexOptions.Multiline);

            var rawTextLines = rawText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            foreach (var rawTextLine in rawTextLines)
            {
                using (var rawLinePath = new GraphicsPath())
                {
                    rawLinePath.AddString(rawTextLine, styleFont.FontFamily, (int) styleFontStyle,
                                          styleFont.SizeInPoints, new PointF(), stringFormat);
                    lineSizes.Add(rawLinePath.GetBounds().Size);
                }
            }

            var textLines = caption.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            var lastLineBreak = 0f;
            foreach (var lineSize in lineSizes)
            {
                
                imgSize.Height += lineSize.Height;
                lastLineBreak = lineSize.Height/3;
                imgSize.Height += lastLineBreak;
                if (lineSize.Width > imgSize.Width)
                    imgSize.Width = lineSize.Width;
            }
            imgSize.Height -= lastLineBreak;
            
            if (imgSize.IsEmpty) return new ImageHolder();
            stringFormat.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, 1) });

            RectangleF whiteSpace;

            using (Image img = new Bitmap((int)imgSize.Width, (int)imgSize.Height, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(img))
                {
                    var origin = new RectangleF(new PointF(0f,0f), imgSize);
                    var regions2 = g.MeasureCharacterRanges(" .", styleFont, origin, stringFormat);
                    if (!regions2.Any()) return new ImageHolder();

                    whiteSpace = regions2[0].GetBounds(g);
                }
            }

            var wordpath = new GraphicsPath();
            var wordPathShadow = new GraphicsPath();

            var shadowOffset = style.Shadow;

            var wStart = new RectangleF { Y = 0, X = 0 };
            var wStartShadow = wStart;
            wStartShadow.Offset(shadowOffset, shadowOffset);

            for (var i = 0; i < textLines.Length; i++)
            {
                var textLine = textLines[i];
                var lineSize = lineSizes[i];
                wStart.Offset(imgSize.Width / 2 - lineSize.Width / 2, 0);
                wStartShadow.Offset(imgSize.Width / 2 - lineSize.Width / 2, 0);

                var words = textLine.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words)
                {
                    using (GraphicsPath singleWord = new GraphicsPath(),
                        singleWordShadow = new GraphicsPath())
                    {
                        string lWord;
                        var fontStyle = GetStyleFont(word, out lWord, styleFontStyle);
                        if (string.IsNullOrEmpty(lWord)) continue;

                        singleWord.AddString(lWord, styleFont.FontFamily, (int) fontStyle, styleFont.SizeInPoints,
                                             wStart.Location, stringFormat);
                        singleWordShadow.AddString(lWord, styleFont.FontFamily, (int) fontStyle,
                                                   styleFont.SizeInPoints, wStartShadow.Location, stringFormat);
                        wordpath.AddPath(singleWord, false);
                        wordPathShadow.AddPath(singleWordShadow, false);
                        wStart.Offset(singleWord.GetBounds().Size.Width + whiteSpace.Size.Width, 0);
                        wStartShadow.Offset(singleWordShadow.GetBounds().Size.Width + whiteSpace.Size.Width, 0);
                    }
                }
                wStart.X = 0;
                wStart.Offset(0, lineSize.Height + lineSize.Height / 3);
                wStartShadow.X = shadowOffset;
                wStartShadow.Offset(0, lineSize.Height + lineSize.Height / 3);
            }

            imgSize.Width = wordPathShadow.GetBounds().Right;
            imgSize.Height = wordPathShadow.GetBounds().Bottom;

            using (Image img = new Bitmap((int)imgSize.Width + style.MarginL + style.MarginR, (int)imgSize.Height + style.MarginV * 2, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(img))
                {
                    g.CompositingMode = CompositingMode.SourceOver;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.High;
                    
                    Brush primBrush = new SolidBrush(style.PrimaryColor);
                    Brush shadowBrush = new SolidBrush(Color.FromArgb(64, style.BackColor));

                    var outPen = new Pen(style.OutlineColor) {Alignment = PenAlignment.Outset};

                    if (style.BorderStyle == 1)
                    {
                        outPen.Width = style.Outline == 0 && style.Shadow > 0 ? 1 : style.Outline;
                        style.Outline = (int) outPen.Width;
                    }
                    g.FillRectangle(Brushes.Transparent, 0, 0, img.Width, img.Height);

                    // draw shadow
                    if (style.BorderStyle == 1 && style.Shadow > 0)
                        g.FillPath(shadowBrush, wordPathShadow);
                    
                    g.FillPath(primBrush, wordpath);
                    
                    // draw outline
                    if (style.BorderStyle == 1 && style.Outline > 0)
                        g.DrawPath(outPen, wordpath);
                }

                img.Save(result.FileName, ImageFormat.Png);
                result.FileName = Path.GetFileName(result.FileName);
                result.Height = img.Height;
                result.Width = img.Width;
            }

            return result;
        }

        private static bool _boldStyle;
        private static bool _italicStyle;
        private static bool _underlineStyle;
        private static bool _strikeStyle;

        private static FontStyle GetStyleFont(string word, out string lWord, FontStyle fontStyle)
        {
            var result = fontStyle;
            lWord = word;

            if (lWord.Contains("<b>"))
                _boldStyle = true;
            if (lWord.Contains("<i>"))
                _italicStyle = true;
            if (lWord.Contains("<u>"))
                _underlineStyle = true;
            if (lWord.Contains("<s>"))
                _strikeStyle = true;

            lWord =
                lWord.Replace("<b>", string.Empty).Replace("<i>", string.Empty).Replace("<u>", string.Empty).Replace(
                    "<s>", string.Empty);

            if (_boldStyle)
                result = result | FontStyle.Bold;
            if (_italicStyle)
                result = result | FontStyle.Italic;
            if (_underlineStyle)
                result = result | FontStyle.Underline;
            if (_strikeStyle)
                result = result | FontStyle.Strikeout;

            if (lWord.Contains("</b>"))
                _boldStyle = false;
            if (lWord.Contains("</i>"))
                _italicStyle = false;
            if (lWord.Contains("</u>"))
                _underlineStyle = false;
            if (lWord.Contains("</s>"))
                _strikeStyle = false;

            lWord =
                lWord.Replace("</b>", string.Empty).Replace("</i>", string.Empty).Replace("</u>", string.Empty).Replace(
                    "</s>", string.Empty);

            return result;
        }
    }
}
