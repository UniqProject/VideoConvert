﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSystemHelper.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities
{
    using System;
    using System.IO;
    using System.Text;

    public class FileSystemHelper
    {
        public static string CreateTempFile(string tempPath, string extension)
        {
            return CreateTempFile(tempPath, null, extension);
        }

        public static string CreateTempFile(String tempPath, string baseName, string extension)
        {
            string output;
            if (String.IsNullOrEmpty(baseName))
                output = Path.ChangeExtension(Path.Combine(tempPath, Guid.NewGuid().ToString()),
                                              extension);
            else
            {
                if (String.IsNullOrEmpty(Path.GetDirectoryName(baseName)))
                    output = Path.Combine(tempPath, String.Format("{0}.{1}", baseName, extension));
                else
                {
                    string inFile = Path.GetFileNameWithoutExtension(baseName);
                    output = Path.Combine(tempPath, String.Format("{0}.{1}", inFile, extension));
                }
            }

            if (output.LastIndexOf('.') == output.Length - 1)
                output = output.Remove(output.Length - 1);

            return output;
        }

        public static string GetAsciiFileName(string fileName)
        {
            return
                Encoding.ASCII.GetString(Encoding.Convert(Encoding.Unicode, Encoding.ASCII,
                                                          Encoding.Unicode.GetBytes(fileName))).Replace('?', '_');
        }
    }
}