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

using System.Text;
using VideoConvert.Core.Profiles;

namespace VideoConvert.Core.CommandLine
{
    class FfmpegCommandLineGenerator
    {
        private static readonly int[] BitrateList = {64, 128, 160, 192, 224, 256, 288, 320, 352, 384, 448, 512, 576, 640};

        /// <summary>
        /// Generates commandline used for encoding an audio stream to AC-3 format.
        /// </summary>
        /// <param name="jobInfo">Container which holds all encoding profiles</param>
        /// <param name="inFile">Path to inputfile, can be "-" when using pipes</param>
        /// <param name="outFile">Path to outputfile</param>
        /// <returns>Commandline arguments</returns>
        public static string GenerateAC3EncodeLine(EncodeInfo jobInfo, string inFile, string outFile)
        {
            StringBuilder sb = new StringBuilder();

            if (string.CompareOrdinal(inFile, "-") == 0)
                sb.Append(" -i -");
            else
                sb.AppendFormat("-i \"{0}\"", inFile);

            sb.Append(" -c:a ac3");

            int bitrate;
            int channels;
            bool drc;

            // safety check, should always be true
            if (jobInfo.AudioProfile.Type == ProfileType.AC3)
            {
                AC3Profile audProfile = (AC3Profile) jobInfo.AudioProfile;
                bitrate = audProfile.Bitrate;
                channels = audProfile.OutputChannels;
                drc = audProfile.ApplyDynamicRangeCompression;
            }
            else
                return string.Empty;

            if (bitrate > 10 && jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                bitrate = 10;   // limit DVD Audio Bitrate to 448 kbit/s

            bitrate = BitrateList[bitrate];

            sb.AppendFormat(" -b:a {0:0}k", bitrate);

            if (channels == 2 || channels == 3)
                sb.Append(" -dsur_mode 1");

            if (drc)
                sb.Append(" -dialnorm -27");

            sb.AppendFormat(" -vn -y \"{0}\"", outFile);

            return sb.ToString();
        }
    }
}
