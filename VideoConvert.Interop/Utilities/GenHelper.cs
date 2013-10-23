// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenHelper.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using Model.MediaInfo;

    public static class GenHelper
    {
        /// <summary>
        /// Gets the Description for enum Types
        /// </summary>
        /// <param name="value"></param>
        /// <returns>string containing the description</returns>
        public static string StringValueOf(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString("F"));
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString("F");
        }

        public static ulong GetFileSize(string fName)
        {
            return (ulong)new FileInfo(fName).Length;
        }

        // Get media information with an 10 sec timeout
        private delegate MediaInfoContainer MiWorkDelegate(string fileName);
        public static MediaInfoContainer GetMediaInfo(string fileName)
        {
            MiWorkDelegate d = DoWorkHandler;
            IAsyncResult res = d.BeginInvoke(fileName, null, null);
            if (res.IsCompleted == false)
            {
                res.AsyncWaitHandle.WaitOne(10000, false);
                if (res.IsCompleted == false)
                    throw new TimeoutException("Could not open media file!");
            }
            return d.EndInvoke(res);
        }

        private static MediaInfoContainer DoWorkHandler(string fileName)
        {
            return MediaInfoContainer.GetMediaInfo(fileName);
        }
    }
}
