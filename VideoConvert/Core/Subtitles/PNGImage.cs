using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VideoConvert.Core.Subtitles
{
    public class ImageHolder
    {
        public string FileName;
        public int Width;
        public int Height;
        
        public ImageHolder()
        {
            FileName = string.Empty;
            Width = 0;
            Height = 0;
        }
    }
    public class PNGImage
    {
        public static ImageHolder CreateImage(SubCaption caption, SubtitleStyle style, int number, int videoWidth, int videoHeight, string baseFName)
        {
            _boldStyle = false;
            _italicStyle = false;
            _underlineStyle = false;
            _strikeStyle = false;

            ImageHolder result = new ImageHolder();
            if (string.IsNullOrEmpty(baseFName)) return new ImageHolder();

            string basePath = Path.GetDirectoryName(baseFName);
            string baseName = Path.GetFileNameWithoutExtension(baseFName);
            if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(baseName)) return new ImageHolder();
            
            result.FileName = string.Format(AppSettings.CInfo, "{0}_{1:g}.png", Path.Combine(basePath, baseName), number);
            SizeF imgSize = new SizeF();

            FontStyle styleFontStyle = FontStyle.Regular;
            if (style.Bold)
                styleFontStyle = styleFontStyle | FontStyle.Bold;
            if (style.Italic)
                styleFontStyle = styleFontStyle | FontStyle.Italic;

            Font styleFont = new Font(style.FontName, style.FontSize, styleFontStyle, GraphicsUnit.Point);
            StringFormat stringFormat = new StringFormat();

            List<SizeF> lineSizes = new List<SizeF>();

            string rawText = Regex.Replace(caption.Text, "</*?(?:i|b)>", "", RegexOptions.Singleline | RegexOptions.Multiline);

            string[] rawTextLines = rawText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            foreach (string rawTextLine in rawTextLines)
            {
                using (GraphicsPath rawLinePath = new GraphicsPath())
                {
                    rawLinePath.AddString(rawTextLine, styleFont.FontFamily, (int) styleFontStyle,
                                          styleFont.SizeInPoints, new PointF(), stringFormat);
                    lineSizes.Add(rawLinePath.GetBounds().Size);
                }
            }

            string[] textLines = caption.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            float lastLineBreak = 0f;
            foreach (SizeF lineSize in lineSizes)
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

            RectangleF whiteSpace = new RectangleF(0f, 0f, 0f, 0f);

            using (Image img = new Bitmap((int)imgSize.Width, (int)imgSize.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(img))
                {
                    RectangleF origin = new RectangleF(new PointF(0f,0f), imgSize);
                    Region[] regions2 = g.MeasureCharacterRanges(" .", styleFont, origin, stringFormat);
                    if (!regions2.Any()) return new ImageHolder();

                    whiteSpace = regions2[0].GetBounds(g);
                }
            }

            GraphicsPath wordpath = new GraphicsPath();
            GraphicsPath wordPathShadow = new GraphicsPath();

            int shadowOffset = style.Shadow;

            RectangleF wStart = new RectangleF { Y = 0, X = 0 };
            RectangleF wStartShadow = wStart;
            wStartShadow.Offset(shadowOffset, shadowOffset);

            for (int i = 0; i < textLines.Length; i++)
            {
                string textLine = textLines[i];
                SizeF lineSize = lineSizes[i];
                wStart.Offset(imgSize.Width / 2 - lineSize.Width / 2, 0);
                wStartShadow.Offset(imgSize.Width / 2 - lineSize.Width / 2, 0);

                string[] words = textLine.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string word in words)
                {
                    using (GraphicsPath singleWord = new GraphicsPath(),
                        singleWordShadow = new GraphicsPath())
                    {
                        string lWord = word;
                        FontStyle fontStyle = GetStyleFont(word, out lWord, styleFontStyle);
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
                using (Graphics g = Graphics.FromImage(img))
                {
                    g.CompositingMode = CompositingMode.SourceOver;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.High;
                    
                    Brush primBrush = new SolidBrush(style.PrimaryColor);
                    Brush secBrush = new SolidBrush(style.SecondaryColor);
                    Brush shadowBrush = new SolidBrush(Color.FromArgb(64, style.BackColor));

                    Pen outPen = new Pen(style.OutlineColor) {Alignment = PenAlignment.Outset};

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

        private static bool _boldStyle = false;
        private static bool _italicStyle = false;
        private static bool _underlineStyle = false;
        private static bool _strikeStyle = false;

        private static FontStyle GetStyleFont(string word, out string lWord, FontStyle fontStyle)
        {
            FontStyle result = fontStyle;
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
