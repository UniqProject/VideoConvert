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
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using log4net;

namespace VideoConvert.Core.Encoder
{
    class LsDvd
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LsDvd));

        private const string Executable = "lsdvd.exe";

        public string GetDvdInfo(string path)
        {
            string output = string.Empty;

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                                                 {
                                                     WorkingDirectory = AppSettings.DemuxLocation,
                                                     Arguments = string.Format("-x -Ox \"{0}\"", path),
                                                     RedirectStandardOutput = true,
                                                     UseShellExecute = false,
                                                     CreateNoWindow = true
                                                 };
                encoder.StartInfo = parameter;

                Log.InfoFormat("lsdvd: {0:s}", parameter.Arguments);

                bool processStarted;
                try
                {
                    processStarted = encoder.Start();
                }
                catch (Exception ex)
                {
                    processStarted = false;
                    Log.ErrorFormat("lsdvd exception: {0}", ex);
                }

                if (processStarted)
                {
                    while (!encoder.HasExited)
                    {
                        output += encoder.StandardOutput.ReadLine() + "\n";
                    }

                    output += encoder.StandardOutput.ReadToEnd();
                }
            }

            output = output.Replace("Pan&Scan", "Pan&amp;Scan").Replace("P&S", "P&amp;S");

            return output;
        }

        public string GetVersionInfo()
        {
            return GetVersionInfo(AppSettings.ToolsPath);
        }

        public string GetVersionInfo(string encPath)
        {
            string verInfo = string.Empty;

            string localExecutable = Path.Combine(encPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable, "-V")
                                                 {
                                                     CreateNoWindow = true,
                                                     UseShellExecute = false,
                                                     RedirectStandardError = true
                                                 };

                encoder.StartInfo = parameter;

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("lsdvd exception: {0}", ex);
                }

                if (started)
                {
                    string output = encoder.StandardError.ReadToEnd();
                    Regex regObj = new Regex(@"^.*lsdvd ([\d\.]+) - .*$",
                                             RegexOptions.Singleline | RegexOptions.Multiline);
                    Match result = regObj.Match(output);
                    if (result.Success)
                    {
                        verInfo = result.Groups[1].Value;
                    }
                }
                if (started)
                {
                    if (!encoder.HasExited)
                    {
                        encoder.Kill();
                    }
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("lsdvd \"{0:s}\" found", verInfo);
            }

            return verInfo;
        }
    }
}
