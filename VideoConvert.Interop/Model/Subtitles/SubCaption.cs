// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubCaption.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Subtitle caption
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Subtitles
{
    using System;

    /// <summary>
    /// Subtitle caption
    /// </summary>
    public class SubCaption
    {
        /// <summary>
        /// Start timestamp
        /// </summary>
        public TimeSpan StartTime;

        /// <summary>
        /// End timestamp
        /// </summary>
        public TimeSpan EndTime;

        /// <summary>
        /// Caption text
        /// </summary>
        public string Text;

        /// <summary>
        /// Text alignment
        /// </summary>
        public int Alignment;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SubCaption()
        {
            StartTime = new TimeSpan();
            EndTime = new TimeSpan();
            Text = string.Empty;
            Alignment = 2;
        }
    }
}