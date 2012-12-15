using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;

namespace VideoConvert.Core.Subtitles
{
    public class SRTReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SRTReader));

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

            foreach (SubCaption caption in from textCap in textCaps
                                           select textCap.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries) into capLines
                                           where capLines.Length >= 3
                                           let timings = capLines[1].Split(new[] { " --> " }, StringSplitOptions.RemoveEmptyEntries)
                                           where timings.Length >= 2
                                           select new SubCaption
                                                      {
                                                          StartTime =
                                                              DateTime.ParseExact(timings[0], "hh:mm:ss,fff", AppSettings.CInfo).TimeOfDay,
                                                          EndTime =
                                                              DateTime.ParseExact(timings[1], "hh:mm:ss,fff", AppSettings.CInfo).TimeOfDay,
                                                          Text = string.Join(Environment.NewLine, capLines, 2, capLines.Length - 2),
                                                      })
            {
                result.Captions.Add(caption);
            }

            return result;
        }
    }
}
