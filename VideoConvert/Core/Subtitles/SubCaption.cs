using System;

namespace VideoConvert.Core.Subtitles
{
    public class SubCaption
    {
        public TimeSpan StartTime;
        public TimeSpan EndTime;
        public string Text;
        public int Alignment;

        public SubCaption()
        {
            StartTime = new TimeSpan();
            EndTime = new TimeSpan();
            Text = string.Empty;
            Alignment = 2;
        }
    }
}