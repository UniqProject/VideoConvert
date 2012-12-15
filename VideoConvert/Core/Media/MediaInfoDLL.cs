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
    public enum StreamKind
    {
        General,
        Video,
        Audio,
        Text,
        Chapters,
        Image,
        Menu
    }

    public enum InfoKind
    {
        Name,
        Text,
        Measure,
        Options,
        NameText,
        MeasureText,
        Info,
        HowTo
    }

    public enum InfoOptions
    {
        ShowInInform,
        Support,
        ShowInSupported,
        TypeOfValue
    }

    public enum InfoFileOptions
    {
        FileOptionNothing = 0x00,
        FileOptionNoRecursive = 0x01,
        FileOptionCloseAll = 0x02,
        FileOptionMax = 0x04
    };


    public class MediaInfo
    {
        //Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)  
        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_New();

        [DllImport("MediaInfo.dll")]
        private static extern void MediaInfo_Delete(IntPtr handle);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Open(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string fileName);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoA_Open(IntPtr handle, IntPtr fileName);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Open_Buffer_Init(IntPtr handle, Int64 fileSize, Int64 fileOffset);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Open_Buffer_Continue(IntPtr handle, IntPtr buffer, IntPtr bufferSize);

        [DllImport("MediaInfo.dll")]
        private static extern Int64 MediaInfo_Open_Buffer_Continue_GoTo_Get(IntPtr handle);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Open_Buffer_Finalize(IntPtr handle);

        [DllImport("MediaInfo.dll")]
        private static extern void MediaInfo_Close(IntPtr handle);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Inform(IntPtr handle, IntPtr reserved);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoA_Inform(IntPtr handle, IntPtr reserved);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_GetI(IntPtr handle, IntPtr streamKind, IntPtr streamNumber,
                                                    IntPtr parameter, IntPtr kindOfInfo);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoA_GetI(IntPtr handle, IntPtr streamKind, IntPtr streamNumber,
                                                     IntPtr parameter, IntPtr kindOfInfo);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Get(IntPtr handle, IntPtr streamKind, IntPtr streamNumber,
                                                   [MarshalAs(UnmanagedType.LPWStr)] string parameter, IntPtr kindOfInfo,
                                                   IntPtr kindOfSearch);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoA_Get(IntPtr handle, IntPtr streamKind, IntPtr streamNumber,
                                                    IntPtr parameter, IntPtr kindOfInfo, IntPtr kindOfSearch);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Option(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string option,
                                                      [MarshalAs(UnmanagedType.LPWStr)] string value);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfoA_Option(IntPtr handle, IntPtr option, IntPtr value);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_State_Get(IntPtr handle);

        [DllImport("MediaInfo.dll")]
        private static extern IntPtr MediaInfo_Count_Get(IntPtr handle, IntPtr streamKind, IntPtr streamNumber);

        //MediaInfo class
        public MediaInfo()
        {
            try
            {
                _handle = MediaInfo_New();
            }
            catch
            {
                _handle = (IntPtr)0;
            }
            _mustUseAnsi = Environment.OSVersion.ToString().IndexOf("Windows", StringComparison.Ordinal) == -1;
        }

        ~MediaInfo() 
        {
            if (_handle == (IntPtr)0) return;
            MediaInfo_Delete(_handle);
        }

        public int Open(String fileName)
        {
            if (_handle == (IntPtr)0)
                return 0;    
            if (_mustUseAnsi)
            {
                IntPtr fileNamePtr = Marshal.StringToHGlobalAnsi(fileName);
                int toReturn = (int)MediaInfoA_Open(_handle, fileNamePtr);
                Marshal.FreeHGlobal(fileNamePtr);
                return toReturn;
            }
            return (int)MediaInfo_Open(_handle, fileName);
        }

        public int OpenBufferInit(Int64 fileSize, Int64 fileOffset)
        {
            if (_handle == (IntPtr)0) return 0;
            return (int) MediaInfo_Open_Buffer_Init(_handle, fileSize, fileOffset);
        }

        public int OpenBufferContinue(IntPtr buffer, IntPtr bufferSize)
        {
            if (_handle == (IntPtr)0) return 0;
            return (int) MediaInfo_Open_Buffer_Continue(_handle, buffer, bufferSize);
        }

        public Int64 OpenBufferContinueGoToGet()
        {
            if (_handle == (IntPtr)0) return 0;
            return (int) MediaInfo_Open_Buffer_Continue_GoTo_Get(_handle);
        }

        public int OpenBufferFinalize()
        {
            if (_handle == (IntPtr)0) return 0;
            return (int) MediaInfo_Open_Buffer_Finalize(_handle);
        }

        public void Close()
        {
            if (_handle == (IntPtr)0) return;
            MediaInfo_Close(_handle);
        }

        public String Inform()
        {
            if (_handle == (IntPtr)0)
                return "Unable to load MediaInfo library";
            return _mustUseAnsi 
                ? Marshal.PtrToStringAnsi(MediaInfoA_Inform(_handle, (IntPtr)0)) 
                : Marshal.PtrToStringUni(MediaInfo_Inform(_handle, (IntPtr)0));
        }

        public String Get(StreamKind streamKind, int streamNumber, String parameter, InfoKind kindOfInfo, InfoKind kindOfSearch)
        {
            if (_handle == (IntPtr)0)
                return "Unable to load MediaInfo library";
            if (_mustUseAnsi)
            {
                IntPtr parameterPtr=Marshal.StringToHGlobalAnsi(parameter);
                String toReturn =
                    Marshal.PtrToStringAnsi(MediaInfoA_Get(_handle, (IntPtr) streamKind, (IntPtr) streamNumber,
                                                           parameterPtr, (IntPtr) kindOfInfo, (IntPtr) kindOfSearch));
                Marshal.FreeHGlobal(parameterPtr);
                return toReturn;
            }
            return
                Marshal.PtrToStringUni(MediaInfo_Get(_handle, (IntPtr) streamKind, (IntPtr) streamNumber, parameter,
                                                     (IntPtr) kindOfInfo, (IntPtr) kindOfSearch));
        }

        public String Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo)
        {
            if (_handle == (IntPtr)0)
                return "Unable to load MediaInfo library";
            return _mustUseAnsi
                       ? Marshal.PtrToStringAnsi(MediaInfoA_GetI(_handle, (IntPtr) streamKind, (IntPtr) streamNumber,
                                                                 (IntPtr) parameter, (IntPtr) kindOfInfo))
                       : Marshal.PtrToStringUni(MediaInfo_GetI(_handle, (IntPtr) streamKind, (IntPtr) streamNumber,
                                                               (IntPtr) parameter, (IntPtr) kindOfInfo));
        }

        public String Option(String option, String value)
        {
            if (_handle == (IntPtr)0)
                return "Unable to load MediaInfo library";
            if (!_mustUseAnsi)
                return Marshal.PtrToStringUni(MediaInfo_Option(_handle, option, value));

            IntPtr optionPtr = Marshal.StringToHGlobalAnsi(option);
            IntPtr valuePtr = Marshal.StringToHGlobalAnsi(value);
            String toReturn = Marshal.PtrToStringAnsi(MediaInfoA_Option(_handle, optionPtr, valuePtr));
            Marshal.FreeHGlobal(optionPtr);
            Marshal.FreeHGlobal(valuePtr);
            return toReturn;
        }

        public int StateGet()
        {
            if (_handle == (IntPtr)0) return 0;
            return (int) MediaInfo_State_Get(_handle);
        }

        public int CountGet(StreamKind streamKind, int streamNumber)
        {
            if (_handle == (IntPtr)0) return 0;
            return (int) MediaInfo_Count_Get(_handle, (IntPtr) streamKind, (IntPtr) streamNumber);
        }

        private readonly IntPtr _handle;
        private readonly bool _mustUseAnsi;

        //Default values, if you know how to set default values in C#, say me
        public String Get(StreamKind streamKind, int streamNumber, String parameter, InfoKind kindOfInfo)
        {
            return Get(streamKind, streamNumber, parameter, kindOfInfo, InfoKind.Name);
        }

        public String Get(StreamKind streamKind, int streamNumber, String parameter)
        {
            return Get(streamKind, streamNumber, parameter, InfoKind.Text, InfoKind.Name);
        }

        public String Get(StreamKind streamKind, int streamNumber, int parameter)
        {
            return Get(streamKind, streamNumber, parameter, InfoKind.Text);
        }

        public String Option(String option)
        {
            return Option(option, "");
        }

        public int CountGet(StreamKind streamKind)
        {
            return CountGet(streamKind, -1);
        }
    }
} //NameSpace