// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SRTWriter.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   SRT Subtitle writer class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities.Subtitles
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using log4net;
    using VideoConvert.Interop.Model.Subtitles;

    /// <summary>
    /// SRT Subtitle writer class
    /// </summary>
    public class SrtWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SrtWriter));

        private static readonly CultureInfo CInfo = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");

        /// <summary>
        /// Writes subtitle captions to file
        /// </summary>
        /// <param name="fileName">Output file name</param>
        /// <param name="subtitle">Text captions</param>
        /// <returns></returns>
        public static bool WriteFile(string fileName, TextSubtitle subtitle)
        {
            if (subtitle.Captions.Count == 0)
            {
                Log.Error("Subtitle contains no captions");
                return false;
            }

            var capCounter = 1;
            var capLines = new List<string>();

            foreach (var caption in subtitle.Captions)
            {
                var startTime = DateTime.MinValue.Add(caption.StartTime).ToString("HH: mm:ss,fff", CInfo);
                var endTime = DateTime.MinValue.Add(caption.EndTime).ToString("HH: mm:ss, fff", CInfo);

                var capLine = $"{capCounter:0}{Environment.NewLine}{startTime} --> ";
                capLine +=$"{endTime}{Environment.NewLine}{caption.Text}";

                capLines.Add(capLine);
                capCounter++;
            }

            using (var writer = new StreamWriter(fileName))
            {
                var separator = $"{Environment.NewLine}{Environment.NewLine}";
                writer.WriteLine(string.Join(separator, capLines));
            }

            return true;
        }
    }
}