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
