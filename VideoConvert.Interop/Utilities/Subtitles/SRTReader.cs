// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SRTReader.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities.Subtitles
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using log4net;
    using Model.Subtitles;

    public class SRTReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SRTReader));
        private static readonly CultureInfo CInfo = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");

        public static TextSubtitle ReadFile(string fileName)
        {
            TextSubtitle result = new TextSubtitle();
            if (!File.Exists(fileName))
            {
                Log.DebugFormat("File \"{0}\" doesn't exist. Aborting file import", fileName);
                return result;
            }

            string lines;
            using (TextReader reader = File.OpenText(fileName))
            {
                lines = reader.ReadToEnd();
            }
            if (string.IsNullOrEmpty(lines)) return result;

            List<string> textCaps = lines.Split(new[] {"\r\n\r\n", "\n\n"}, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (string textCap in textCaps)
            {
                string[] capLines = textCap.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                if (capLines.Length >= 3)
                {
                    string[] timings = capLines[1].Split(new[] {" --> "}, StringSplitOptions.RemoveEmptyEntries);
                    if (timings.Length >= 2)
                    {
                        SubCaption caption = new SubCaption
                        {
                            StartTime = DateTime.ParseExact(timings[0], "hh:mm:ss,fff", CInfo).TimeOfDay,
                            EndTime = DateTime.ParseExact(timings[1], "hh:mm:ss,fff", CInfo).TimeOfDay,
                            Text = string.Join(Environment.NewLine, capLines, 2, capLines.Length - 2),
                        };
                        result.Captions.Add(caption);
                    }
                }
            }

            result.SetDefaultStyle();
            return result;
        }
    }
}
