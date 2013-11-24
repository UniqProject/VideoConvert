// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   CPU extensions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// CPU extensions
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Extensions
    {
        /// <summary>
        /// x86_64
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int x64;

        /// <summary>
        /// MMX
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int MMX;

        /// <summary>
        /// SSE
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int SSE;

        /// <summary>
        /// SSE2
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int SSE2;

        /// <summary>
        /// SSE3
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int SSE3;

        /// <summary>
        /// SSSE3
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int SSSE3;

        /// <summary>
        /// SSE4.1
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int SSE41;

        /// <summary>
        /// SSE4.2
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int SSE42;

        /// <summary>
        /// SSE4A
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int SSE4a;

        /// <summary>
        /// AVX
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int AVX;

        /// <summary>
        /// AVX2
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int AVX2;

        /// <summary>
        /// XOP
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int XOP;

        /// <summary>
        /// FMA3
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int FMA3;

        /// <summary>
        /// FMA4
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int FMA4;
    }
}