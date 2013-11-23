// --------------------------------------------------------------------------------------------------------------------
// <copyright file="X264Preset.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   X264 Encoder Preset
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.x264
{
    /// <summary>
    /// X264 Encoder Preset
    /// </summary>
    public enum X264Preset
    {
        /// <summary>
        /// Ultra fast
        /// </summary>
        Ultrafast = 0,

        /// <summary>
        /// Super fast
        /// </summary>
        Superfast = 1,

        /// <summary>
        /// Very fast
        /// </summary>
        Veryfast = 2,

        /// <summary>
        /// Faster
        /// </summary>
        Faster = 3,

        /// <summary>
        /// Fast
        /// </summary>
        Fast = 4,

        /// <summary>
        /// Medium
        /// </summary>
        Medium = 5,

        /// <summary>
        /// Slow
        /// </summary>
        Slow = 6,

        /// <summary>
        /// Slower
        /// </summary>
        Slower = 7,

        /// <summary>
        /// Very slow
        /// </summary>
        Veryslow = 8,

        /// <summary>
        /// Placebo (extremely slow)
        /// </summary>
        Placebo = 9
    }
}