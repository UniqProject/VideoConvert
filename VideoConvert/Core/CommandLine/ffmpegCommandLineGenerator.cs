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
        public static string GenerateAC3EncodeLine(EncodeInfo jobInfo, AudioInfo audioItem, string inFile, string outFile)
        {
            StringBuilder sb = new StringBuilder();

            if (string.CompareOrdinal(inFile, "-") == 0)
                sb.Append(" -i -");
            else
                sb.AppendFormat("-i \"{0}\"", inFile);

            sb.Append(" -c:a ac3");

            int bitrate = 10;
            int channels = 0;
            bool drc = false;
            if (jobInfo.AudioProfile.Type == ProfileType.AC3)
            {
                AC3Profile audProfile = (AC3Profile) jobInfo.AudioProfile;
                bitrate = audProfile.Bitrate;
                channels = audProfile.OutputChannels;
                drc = audProfile.ApplyDynamicRangeCompression;
            }

            if (bitrate > 10 && jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                bitrate = 10;   // limit DVD Audio Bitrate to 448 kbit/s

            switch (bitrate)
            {
                case 0:
                    bitrate = 64;
                    break;
                case 1:
                    bitrate = 128;
                    break;
                case 2:
                    bitrate = 160;
                    break;
                case 3:
                    bitrate = 192;
                    break;
                case 4:
                    bitrate = 224;
                    break;
                case 5:
                    bitrate = 256;
                    break;
                case 6:
                    bitrate = 288;
                    break;
                case 7:
                    bitrate = 320;
                    break;
                case 8:
                    bitrate = 352;
                    break;
                case 9:
                    bitrate = 384;
                    break;
                case 10:
                    bitrate = 448;
                    break;
                case 11:
                    bitrate = 512;
                    break;
                case 12:
                    bitrate = 576;
                    break;
                case 13:
                    bitrate = 640;
                    break;
            }

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
