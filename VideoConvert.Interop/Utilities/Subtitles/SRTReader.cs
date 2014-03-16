// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SRTReader.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   SRT Subtitle reader class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities.Subtitles
{
    using log4net;
    using Model.Subtitles;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// SRT Subtitle reader class
    /// </summary>
    public class SrtReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SrtReader));
        private static readonly CultureInfo CInfo = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");

        /// <summary>
        /// Reads SRT formatted text file into single captions
        /// </summary>
        /// <param name="fileName">input file name</param>
        /// <returns></returns>
        public static TextSubtitle ReadFile(string fileName)
        {
            var result = new TextSubtitle();
            if (!File.Exists(fileName))
            {
                Log.DebugFormat("File \"{0}\" doesn't exist. Aborting file import", fileName);
                return result;
            }

            string lines;
            using (var reader = File.OpenText(fileName))
            {
                lines = reader.ReadToEnd();
            }
            if (string.IsNullOrEmpty(lines)) return result;

            var textCaps = lines.Split(new[] {"\r\n\r\n", "\n\n"}, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var textCap in textCaps)
            {
                var capLines = textCap.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);

                if (capLines.Length < 3) continue;

                var timings = capLines[1].Split(new[] {" --> "}, StringSplitOptions.RemoveEmptyEntries);

                if (timings.Length < 2) continue;

                var caption = new SubCaption
                {
                    StartTime = DateTime.ParseExact(timings[0], "hh:mm:ss,fff", CInfo).TimeOfDay,
                    EndTime = DateTime.ParseExact(timings[1], "hh:mm:ss,fff", CInfo).TimeOfDay,
                    Text = string.Join(Environment.NewLine, capLines, 2, capLines.Length - 2),
                };
                result.Captions.Add(caption);
            }

            result.SetDefaultStyle();
            return result;
        }
    }
}
