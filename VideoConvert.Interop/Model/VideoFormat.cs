// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VideoFormat.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Videoformat
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.ComponentModel;

    /// <summary>
    /// Videoformat
    /// </summary>
    public enum VideoFormat : byte
    {
        /// <summary>
        /// Unknown
        /// </summary>
        [Description("None")] 
        Unknown = 0,

        /// <summary>
        /// 480i (NTSC SD resolution, up to 720x480 / 704x480 interlaced)
        /// </summary>
        [Description("480i")] 
        Videoformat480I = 1,

        /// <summary>
        /// 480p (NTSC SD resolution, up to 720x480 / 704x480 progressive)
        /// </summary>
        [Description("480p")] 
        Videoformat480P = 3,

        /// <summary>
        /// 576i (PAL SD resolution, up to 720x576 / 704x576 interlaced)
        /// </summary>
        [Description("576i")] 
        Videoformat576I = 2,

        /// <summary>
        /// 576p (PAL SD resolution, up to 720x576 / 704x576 progressive)
        /// </summary>
        [Description("576p")] 
        Videoformat576P = 7,

        /// <summary>
        /// 720p (HD resolution, up to 1280x720 progressive only)
        /// </summary>
        [Description("720p")] 
        Videoformat720P = 5,

        /// <summary>
        /// 1080i (Full HD resolution, up to 1920x1080 interlaced)
        /// </summary>
        [Description("1080i")] 
        Videoformat1080I = 4,

        /// <summary>
        /// 1080p (Full HD resolution, up to 1440x1080 / 1920x1080 progressive)
        /// </summary>
        [Description("1080p")] 
        Videoformat1080P = 6
    };
}