// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StereoVideoInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Helper class for stereoscopic video streams
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.Drawing;

    /// <summary>
    /// Helper class for stereoscopic video streams
    /// </summary>
    public class StereoVideoInfo
    {
        /// <summary>
        /// stream id for left eye
        /// </summary>
        public int LeftStreamId { get; set; }

        /// <summary>
        /// temp file for left eye
        /// </summary>
        public string LeftTempFile { get; set; }

        /// <summary>
        /// position of left stream
        /// </summary>
        public Point LeftPosition { get; set; }

        /// <summary>
        /// Size of left stream
        /// </summary>
        public Size LeftSize { get; set; }

        /// <summary>
        /// stream id for right eye
        /// </summary>
        public int RightStreamId { get; set; }

        /// <summary>
        /// temp file for right eye
        /// </summary>
        public string RightTempFile { get; set; }

        /// <summary>
        /// position of right stream
        /// </summary>
        public Point RightPosition { get; set; }

        /// <summary>
        /// Size of right stream
        /// </summary>
        public Size RightSize { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public StereoVideoInfo()
        {
            LeftStreamId = -1;
            LeftTempFile = string.Empty;
            LeftPosition = new Point();
            LeftSize = new Size();

            RightStreamId = -1;
            RightTempFile = string.Empty;
            RightPosition = new Point();
            RightSize = new Size();
        }
    }
}