// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubCaption.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Subtitles
{
    using System;

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