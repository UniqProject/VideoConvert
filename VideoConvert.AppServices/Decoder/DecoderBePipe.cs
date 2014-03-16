// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DecoderBePipe.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Helper class for creating bepipe decoding processes
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Decoder
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Helper class for creating bepipe decoding processes
    /// </summary>
    public class DecoderBePipe
    {
        /// <summary>
        /// Executable filename
        /// </summary>
        private const string Executable = "BePipe.exe";

        /// <summary>
        /// Creates a BePipe decoding process for given avisynth script input
        /// </summary>
        /// <param name="scriptName">Path to AviSynth script</param>
        /// <param name="aviSynthPath">Path to avsynth plugins location</param>
        /// <returns>Created <see cref="Process" /></returns>
        public static Process CreateDecodingProcess(string scriptName, string aviSynthPath)
        {
            var localExecutable = Path.Combine(aviSynthPath, "audio", Executable);
            var info = new ProcessStartInfo
            {
                FileName = localExecutable,
                Arguments =
                    String.Format("--script \"Import(^{0}^)\"", scriptName),
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            var bePipe = new Process { StartInfo = info };
            return bePipe;
        }
    }
}