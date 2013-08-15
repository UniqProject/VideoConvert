using System;
using System.Runtime.InteropServices;

namespace VideoConvert.Core
{
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

    class CpuExtensions
    {
        [DllImport("CpuExtensions.dll",CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetExtensions(out Extensions ext);
    }
}
