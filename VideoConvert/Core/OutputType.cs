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
    /// supported output formats
    /// </summary>
    public enum OutputType
    {
        [Description("Matroska-Container")] OutputMatroska = 0,
        [Description("MP4-Container")] OutputMp4 = 1,
        [Description("TS-Transport stream")] OutputTs = 2,
        [Description("M2TS-Transport stream")] OutputM2Ts = 3,
        [Description("Blu-Ray Disc")] OutputBluRay = 4,
        [Description("AVCHD-Disc")] OutputAvchd = 5,
        [Description("DVD-Disc")] OutputDvd = 6,
        [Description("WebM Video")] OutputWebM = 7,
        [Description("Undefined")] OutputUndefined = 255
    };
}