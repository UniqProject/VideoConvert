// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VideoFormat.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.ComponentModel;

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