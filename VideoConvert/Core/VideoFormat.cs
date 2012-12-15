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
    /// detected videoformat
    /// </summary>
    public enum VideoFormat : byte
    {
        [Description("None")] Unknown = 0,
        [Description("480i")] Videoformat480I = 1,
        [Description("480p")] Videoformat480P = 3,
        [Description("576i")] Videoformat576I = 2,
        [Description("576p")] Videoformat576P = 7,
        [Description("720p")] Videoformat720P = 5,
        [Description("1080i")] Videoformat1080I = 4,
        [Description("1080p")] Videoformat1080P = 6
    };
}