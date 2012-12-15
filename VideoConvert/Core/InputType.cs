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

using System.ComponentModel;

namespace VideoConvert.Core
{
    /// <summary>
    /// Type of input file.
    /// Note: only this types of input can be processed
    /// </summary>
    public enum InputType
    {
        [Description("AVI-Container")] InputAvi = 0,
        [Description("MP4-Container")] InputMp4 = 1,
        [Description("Matroska-Container")] InputMatroska = 2,
        [Description("TS-Transportstream")] InputTs = 3,
        [Description("Windows Media Container")] InputWm = 4,
        [Description("Flash Video")] InputFlash = 5,
        [Description("DVD Disc")] InputDvd = 6,
        [Description("Blu-Ray Disc")] InputBluRay = 7,
        [Description("AVCHD-Disc")] InputAvchd = 8,
        [Description("HD-DVD Disc")] InputHddvd = 9,
        [Description("MPEG-PS")] InputMpegps = 10,
        [Description("AviSynth Script")] InputAviSynth = 11,
        [Description("WebM")] InputWebM = 12,
        [Description("OGG")] InputOgg = 13,
        [Description("Undefined")] InputUndefined = 255
    };
}