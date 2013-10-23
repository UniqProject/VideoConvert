// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CpuExtensions.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Extensions
    {
        [MarshalAs(UnmanagedType.I4)]
        public int x64;
        [MarshalAs(UnmanagedType.I4)]
        public int MMX;
        [MarshalAs(UnmanagedType.I4)]
        public int SSE;
        [MarshalAs(UnmanagedType.I4)]
        public int SSE2;
        [MarshalAs(UnmanagedType.I4)]
        public int SSE3;
        [MarshalAs(UnmanagedType.I4)]
        public int SSSE3;
        [MarshalAs(UnmanagedType.I4)]
        public int SSE41;
        [MarshalAs(UnmanagedType.I4)]
        public int SSE42;
        [MarshalAs(UnmanagedType.I4)]
        public int SSE4a;
        [MarshalAs(UnmanagedType.I4)]
        public int AVX;
        [MarshalAs(UnmanagedType.I4)]
        public int AVX2;
        [MarshalAs(UnmanagedType.I4)]
        public int XOP;
        [MarshalAs(UnmanagedType.I4)]
        public int FMA3;
        [MarshalAs(UnmanagedType.I4)]
        public int FMA4;
    }

    public class CpuExtensions
    {
        [DllImport("CpuExtensions.dll",CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetExtensions(out Extensions ext);
    }
}
