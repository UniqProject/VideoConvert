// --------------------------------------------------------------------------------------------------------------------
// <copyright file="x264Device.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Special encoding parameter tuning for devices
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.x264
{
    using System.Collections.Generic;

    /// <summary>
    /// Special encoding parameter tuning for devices
    /// </summary>
    public class X264Device
    {
        /// <summary>
        /// Creates a list of supported devices
        /// </summary>
        /// <returns></returns>
        public static List<X264Device> CreateDeviceList()
        {
            var x264DeviceList = new List<X264Device>
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

            x264DeviceList[2].MaxGop = 1;
            x264DeviceList[2].BluRay = true;
            x264DeviceList[3].MaxGop = 1;
            x264DeviceList[3].BluRay = true;
            x264DeviceList[4].MaxGop = 4;
            x264DeviceList[13].BPyramid = 0;
            return x264DeviceList;
        }

        /// <summary>
        /// Initialize a device tuning
        /// </summary>
        /// <param name="iId">ID</param>
        /// <param name="strName">Device Name</param>
        /// <param name="iProfile">AVC-Profile</param>
        /// <param name="iLevel">AVC-Level</param>
        /// <param name="iVbvBufsize">VBV Buffer size</param>
        /// <param name="iVbvMaxrate">Max VBV bitrate</param>
        /// <param name="iBframes">Number of B-Frames</param>
        /// <param name="iReframes">Number of ref-Frames</param>
        /// <param name="iMaxWidth">Max Width</param>
        /// <param name="iMaxHeight">Max Height</param>
        public X264Device(int iId, string strName, int iProfile, int iLevel, int iVbvBufsize, int iVbvMaxrate, int iBframes, int iReframes, int iMaxWidth, int iMaxHeight)
        {
            ID = iId;
            Name = strName;
            Profile = iProfile;
            Level = iLevel;
            VbvBufsize = iVbvBufsize;
            VbvMaxrate = iVbvMaxrate;
            BFrames = iBframes;
            ReferenceFrames = iReframes;
            Width = iMaxWidth;
            Height = iMaxHeight;
            BluRay = false;
            BPyramid = -1;
            MaxGop = -1;
        }

        /// <summary>
        /// Device ID
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Device Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// AVC Profile
        /// </summary>
        public int Profile { get; }

        /// <summary>
        /// AVC Level
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// VBV Buffer size
        /// </summary>
        public int VbvBufsize { get; }

        /// <summary>
        /// Max VBV bitrate
        /// </summary>
        public int VbvMaxrate { get; }

        /// <summary>
        /// Number of B-Frames
        /// </summary>
        public int BFrames { get; }

        /// <summary>
        /// Number of ref-frames
        /// </summary>
        public int ReferenceFrames { get; }

        /// <summary>
        /// Max Height
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Max Width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Max GOP size
        /// </summary>
        public int MaxGop { get; set; }

        /// <summary>
        /// Blu-Ray compatibility
        /// </summary>
        public bool BluRay { get; set; }

        /// <summary>
        /// B-Pyramid
        /// </summary>
        public int BPyramid { get; set; }

        /// <summary>
        /// Gibt eine Zeichenfolge zurück, die das aktuelle Objekt darstellt.
        /// </summary>
        /// <returns>
        /// Eine Zeichenfolge, die das aktuelle Objekt darstellt.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return Name;
        }
    }
}
