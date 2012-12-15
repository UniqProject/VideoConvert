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

using System;
using System.Runtime.InteropServices;

namespace VideoConvert.Core.Media
{
    public class MediaInfoList
    {
        //Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)  
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_New();

        [DllImport("MediaInfo.dll")]
        private static extern void MediaInfoList_Delete(IntPtr handle);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_Open(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string fileName,
                                                        IntPtr options);

        [DllImport("MediaInfo.dll")]
        private static extern void MediaInfoList_Close(IntPtr handle, IntPtr filePos);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_Inform(IntPtr handle, IntPtr filePos, IntPtr reserved);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_GetI(IntPtr handle, IntPtr filePos, IntPtr streamKind,
                                                        IntPtr streamNumber, IntPtr parameter, IntPtr kindOfInfo);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_Get(IntPtr handle, IntPtr filePos, IntPtr streamKind,
                                                       IntPtr streamNumber,
                                                       [MarshalAs(UnmanagedType.LPWStr)] string parameter,
                                                       IntPtr kindOfInfo, IntPtr kindOfSearch);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_Option(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string option,
                                                          [MarshalAs(UnmanagedType.LPWStr)] string value);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_State_Get(IntPtr handle);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoList_Count_Get(IntPtr handle, IntPtr filePos, IntPtr streamKind,
                                                             IntPtr streamNumber);

        //MediaInfo class
        public MediaInfoList()
        {
            _handle = MediaInfoList_New();
        }

        ~MediaInfoList()
        {
            MediaInfoList_Delete(_handle);
        }

        public int Open(String fileName, InfoFileOptions options)
        {
            return (int) MediaInfoList_Open(_handle, fileName, (IntPtr) options);
        }

        public void Close(int filePos)
        {
            MediaInfoList_Close(_handle, (IntPtr) filePos);
        }

        public String Inform(int filePos)
        {
            return Marshal.PtrToStringUni(MediaInfoList_Inform(_handle, (IntPtr) filePos, (IntPtr) 0));
        }

        public String Get(int filePos, StreamKind streamKind, int streamNumber, String parameter, InfoKind kindOfInfo, InfoKind kindOfSearch)
        {
            return
                Marshal.PtrToStringUni(MediaInfoList_Get(_handle, (IntPtr) filePos, (IntPtr) streamKind,
                                                         (IntPtr) streamNumber, parameter, (IntPtr) kindOfInfo,
                                                         (IntPtr) kindOfSearch));
        }

        public String Get(int filePos, StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo)
        {
            return
                Marshal.PtrToStringUni(MediaInfoList_GetI(_handle, (IntPtr) filePos, (IntPtr) streamKind,
                                                          (IntPtr) streamNumber, (IntPtr) parameter, (IntPtr) kindOfInfo));
        }

        public String Option(String option, String value)
        {
            return Marshal.PtrToStringUni(MediaInfoList_Option(_handle, option, value));
        }

        public int StateGet()
        {
            return (int) MediaInfoList_State_Get(_handle);
        }

        public int CountGet(int filePos, StreamKind streamKind, int streamNumber)
        {
            return (int) MediaInfoList_Count_Get(_handle, (IntPtr) filePos, (IntPtr) streamKind, (IntPtr) streamNumber);
        }

        private readonly IntPtr _handle;

        //Default values, if you know how to set default values in C#, say me
        public void Open(String fileName)
        {
            Open(fileName, 0);
        }

        public void Close()
        {
            Close(-1);
        }

        public String Get(int filePos, StreamKind streamKind, int streamNumber, String parameter, InfoKind kindOfInfo)
        {
            return Get(filePos, streamKind, streamNumber, parameter, kindOfInfo, InfoKind.Name);
        }

        public String Get(int filePos, StreamKind streamKind, int streamNumber, String parameter)
        {
            return Get(filePos, streamKind, streamNumber, parameter, InfoKind.Text, InfoKind.Name);
        }

        public String Get(int filePos, StreamKind streamKind, int streamNumber, int parameter)
        {
            return Get(filePos, streamKind, streamNumber, parameter, InfoKind.Text);
        }

        public String Option(String option)
        {
            return Option(option, "");
        }

        public int CountGet(int filePos, StreamKind streamKind)
        {
            return CountGet(filePos, streamKind, -1);
        }
    }
}