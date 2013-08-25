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
using log4net;

namespace VideoConvert.Core.Subtitles
{
    public class SRTWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SRTWriter));

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
                                                DateTime.MinValue.Add(caption.StartTime).ToString("HH:mm:ss,fff", AppSettings.CInfo),
                                                DateTime.MinValue.Add(caption.EndTime).ToString("HH:mm:ss,fff", AppSettings.CInfo),
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