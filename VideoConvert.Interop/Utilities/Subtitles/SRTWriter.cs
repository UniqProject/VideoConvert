// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SRTWriter.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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
    using log4net;
    using Model.Subtitles;

    public class SRTWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SRTWriter));

        private static readonly CultureInfo CInfo = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");

        public static bool WriteFile(string fileName, TextSubtitle subtitle)
        {
            if (subtitle.Captions.Count == 0)
            {
                Log.Error("Subtitle contains no captions");
                return false;
            }

            int capCounter = 1;
            List<string> capLines = new List<string>();
            foreach (SubCaption caption in subtitle.Captions)
            {
                string capLine = string.Format("{1:0}{0}{2} --> {3}{0}{4}",
                                                Environment.NewLine, capCounter,
                                                DateTime.MinValue.Add(caption.StartTime).ToString("HH:mm:ss,fff", CInfo),
                                                DateTime.MinValue.Add(caption.EndTime).ToString("HH:mm:ss,fff", CInfo),
                                                caption.Text);
                capLines.Add(capLine);
                capCounter++;
            }

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                string separator = string.Format("{0}{0}", Environment.NewLine);
                writer.WriteLine(string.Join(separator, capLines));
            }

            return true;
        }
    }
}