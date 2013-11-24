// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CpuExtensions.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Supported CPU extensions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Supported CPU extensions
    /// </summary>
    public class CpuExtensions
    {
        /// <summary>
        /// Get supported CPU extensions
        /// </summary>
        /// <param name="ext">empty <see cref="Extensions"/> struct, returns list of extensions supported by current CPU</param>
        [DllImport("CpuExtensions.dll",CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetExtensions(out Extensions ext);
    }
}
