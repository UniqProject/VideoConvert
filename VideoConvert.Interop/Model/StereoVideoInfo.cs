// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StereoVideoInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.Drawing;

    public class StereoVideoInfo
    {
        public int LeftStreamId { get; set; }
        public string LeftTempFile { get; set; }
        public Point LeftPosition { get; set; }
        public Size LeftSize { get; set; }

        public int RightStreamId { get; set; }
        public string RightTempFile { get; set; }
        public Point RightPosition { get; set; }
        public Size RightSize { get; set; }

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