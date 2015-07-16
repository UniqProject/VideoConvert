// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSystemHelper.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   File system helper class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// File system helper class
    /// </summary>
    public class FileSystemHelper
    {
        /// <summary>
        /// Generate temp file name
        /// </summary>
        /// <param name="tempPath">Location of temp files</param>
        /// <param name="extension">File extension</param>
        /// <returns>Temp file name</returns>
        public static string CreateTempFile(string tempPath, string extension)
        {
            return CreateTempFile(tempPath, null, extension);
        }

        /// <summary>
        /// Generate temp file name
        /// </summary>
        /// <param name="tempPath">Location of temp files</param>
        /// <param name="baseName">Base name for temp file</param>
        /// <param name="extension">File extension</param>
        /// <returns>Temp file name</returns>
        public static string CreateTempFile(string tempPath, string baseName, string extension)
        {
            string output;
            if (string.IsNullOrEmpty(tempPath))
                tempPath = Path.GetTempPath();

            if (string.IsNullOrEmpty(baseName))
                output = Path.ChangeExtension(Path.Combine(tempPath, Guid.NewGuid().ToString()),
                                              extension);
            else
            {
                if (string.IsNullOrEmpty(Path.GetDirectoryName(baseName)))
                    output = Path.Combine(tempPath, $"{baseName}.{extension}");
                else
                {
                    var inFile = Path.GetFileNameWithoutExtension(baseName);
                    output = Path.Combine(tempPath, $"{inFile}.{extension}");
                }
            }

            if (output.LastIndexOf('.') == output.Length - 1)
                output = output.Remove(output.Length - 1);

            return output;
        }

        /// <summary>
        /// Get filename in ASCII encoding
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>Filename in ASCII encoding</returns>
        public static string GetAsciiFileName(string fileName)
        {
            return
                Encoding.ASCII.GetString(Encoding.Convert(Encoding.Unicode, Encoding.ASCII,
                                                          Encoding.Unicode.GetBytes(fileName))).Replace('?', '_');
        }
    }
}