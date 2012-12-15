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

namespace VideoConvert.Core.Encoder
{
    class BePipe
    {
        private const string Executable = "BePipe.exe";

        public static Process GenerateProcess(string scriptName)
        {
            string localExecutable = Path.Combine(AppSettings.AppPath, "AvsPlugins", "audio", Executable);

            ProcessStartInfo info = new ProcessStartInfo
                                        {
                                            FileName = localExecutable,
                                            Arguments =
                                                String.Format(AppSettings.CInfo, "--script \"Import(^{0}^)\"",
                                                              scriptName),
                                            CreateNoWindow = true,
                                            RedirectStandardOutput = true,
                                            UseShellExecute = false
                                        };
            Process bePipe = new Process {StartInfo = info};
            return bePipe;
        }
    }
}
