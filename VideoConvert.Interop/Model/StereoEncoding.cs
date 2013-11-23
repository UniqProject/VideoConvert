// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StereoEncoding.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Stereoscopic encode format
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    /// <summary>
    /// Stereoscopic encode format
    /// </summary>
    public enum StereoEncoding
    {
        /// <summary>
        /// Not Stereoscopic
        /// </summary>
        None = 0,

        /// <summary>
        /// Side-by-side, left eye first, full framesize
        /// </summary>
        FullSideBySideLeft = 1,

        /// <summary>
        /// Side-by-side, left eye first, half width
        /// </summary>
        HalfSideBySideLeft = 2,

        /// <summary>
        /// Side-by-side, right eye first, full framesize
        /// </summary>
        FullSideBySideRight = 3,

        /// <summary>
        /// Side-by-side, right eye first, half width
        /// </summary>
        HalfSideBySideRight = 4
    };
}