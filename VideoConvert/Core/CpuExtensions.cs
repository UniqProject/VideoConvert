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
