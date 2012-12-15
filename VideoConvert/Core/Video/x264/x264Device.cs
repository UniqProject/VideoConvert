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

using System.Collections.Generic;

namespace VideoConvert.Core.Video.x264
{
    public class X264Device
    {
        private readonly string _strName;
        private readonly int _iID;
        private readonly int _iProfile;
        private readonly int _iLevel;
        private readonly int _iVBVBufsize;
        private readonly int _iVBVMaxrate;
        private readonly int _iBframes;
        private readonly int _iReframes;
        private readonly int _iMaxWidth;
        private readonly int _iMaxHeight;

        public static List<X264Device> CreateDeviceList()
        {
            List<X264Device> x264DeviceList = new List<X264Device>
                                                  {
                                                      new X264Device(0, "Default", -1, 15, -1, -1, -1, -1, -1, -1),
                                                      new X264Device(1, "Android G1", 0, 7, 2500, 2500, 0, -1, 480, 368),
                                                      new X264Device(2, "AVCHD", 2, 11, 14000, 14000, 3, 6, 1920, 1080),
                                                      new X264Device(3, "Blu-ray", 2, 11, 30000, 40000, 3, 6, 1920, 1080),
                                                      new X264Device(4, "DivX Plus HD", 2, 10, 25000, 20000, 3, -1, 1920,
                                                                     1080),
                                                      new X264Device(5, "DXVA", 2, 11, -1, -1, -1, -1, -1, -1),
                                                      new X264Device(6, "iPad", 2, 8, -1, -1, -1, -1, 1024, 768),
                                                      new X264Device(7, "iPhone", 0, 7, 10000, 10000, 0, -1, 480, 320),
                                                      new X264Device(8, "iPhone 4", 1, 8, -1, -1, -1, -1, 960, 640),
                                                      new X264Device(9, "iPod", 0, 7, 10000, 10000, 0, 5, 320, 240),
                                                      new X264Device(10, "Nokia N8", 2, 8, -1, -1, -1, -1, 640, 360),
                                                      new X264Device(11, "Nokia N900", 0, 7, -1, -1, -1, -1, 800, 480),
                                                      new X264Device(12, "PS3", 2, 12, 31250, 31250, -1, -1, 1920, 1080),
                                                      new X264Device(13, "PSP", 1, 7, 10000, 10000, -1, 3, 480, 272),
                                                      new X264Device(14, "Xbox 360", 2, 11, 24000, 24000, 3, 3, 1920,
                                                                     1080),
                                                      new X264Device(15, "WDTV", 2, 11, -1, -1, -1, -1, 1920, 1080)
                                                  };
            x264DeviceList[2].MaxGOP = 1;
            x264DeviceList[2].BluRay = true;
            x264DeviceList[3].MaxGOP = 1;
            x264DeviceList[3].BluRay = true;
            x264DeviceList[4].MaxGOP = 4;
            x264DeviceList[13].BPyramid = 0;
            return x264DeviceList;
        }

        public X264Device(int _iID, string strName, int iProfile, int iLevel, int _iVBVBufsize, int _iVBVMaxrate, int iBframes, int iReframes, int iMaxWidth, int iMaxHeight)
        {
            this._iID = _iID;
            _strName = strName;
            _iProfile = iProfile;
            _iLevel = iLevel;
            this._iVBVBufsize = _iVBVBufsize;
            this._iVBVMaxrate = _iVBVMaxrate;
            _iBframes = iBframes;
            _iReframes = iReframes;
            _iMaxWidth = iMaxWidth;
            _iMaxHeight = iMaxHeight;
            BluRay = false;
            BPyramid = -1;
            MaxGOP = -1;
        }

        public int ID
        {
            get { return _iID; }
        }

        public string Name
        {
            get { return _strName; }
        }

        public int Profile
        {
            get { return _iProfile; }
        }

        public int Level
        {
            get { return _iLevel; }
        }

        public int VBVBufsize
        {
            get { return _iVBVBufsize; }
        }

        public int VBVMaxrate
        {
            get { return _iVBVMaxrate; }
        }

        public int BFrames
        {
            get { return _iBframes; }
        }

        public int ReferenceFrames
        {
            get { return _iReframes; }
        }

        public int Height
        {
            get { return _iMaxHeight; }
        }

        public int Width
        {
            get { return _iMaxWidth; }
        }

        public int MaxGOP { get; set; }

        public bool BluRay { get; set; }

        public int BPyramid { get; set; }

        public override string ToString()
        {
            return _strName;
        }
    }
}
