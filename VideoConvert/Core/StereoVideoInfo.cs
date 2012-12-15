//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System.Drawing;

namespace VideoConvert.Core
{
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