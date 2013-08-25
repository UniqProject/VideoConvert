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
                            StartTime = DateTime.ParseExact(timings[0], "hh:mm:ss,fff", AppSettings.CInfo).TimeOfDay,
                            EndTime = DateTime.ParseExact(timings[1], "hh:mm:ss,fff", AppSettings.CInfo).TimeOfDay,
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
