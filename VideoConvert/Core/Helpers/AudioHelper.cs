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

using VideoConvert.Core.Media;
using VideoConvert.Core.Profiles;

namespace VideoConvert.Core.Helpers
{
    class AudioHelper
    {
        public static bool AudioProcessingNeeded(EncoderProfile aProfile)
        {
            int sampleRate = -1, channelOrder = -1;
            switch (aProfile.Type)
            {
                case ProfileType.AC3:
                    sampleRate = ((AC3Profile)aProfile).SampleRate;
                    channelOrder = ((AC3Profile)aProfile).OutputChannels;
                    break;
                case ProfileType.FLAC:
                    break;
                default:
                    return false;
            }
            return sampleRate > -1 || channelOrder > -1;
        }

        public static bool AudioEncodingNeeded(EncoderProfile aProfile)
        {
            switch (aProfile.Type)
            {
                case ProfileType.AC3:
                case ProfileType.FLAC:
                    return true;
                default:
                    return false;
            }
        }

        public static bool OutputSupportsW64(OutputType outType)
        {
            switch (outType)
            {
                case OutputType.OutputTs:
                case OutputType.OutputM2Ts:
                case OutputType.OutputBluRay:
                case OutputType.OutputAvchd:
                    return true;

                default:
                    return false;
            }
        }

        public static bool OutputSupportsFlac(OutputType outType)
        {
            switch (outType)
            {
                case OutputType.OutputMatroska:
                    return true;
                default:
                    return false;
            }
        }

        public static double GetRuntimePCM(AudioInfo item)
        {
            return (item.StreamSize * 8D) / item.ChannelCount / ((double)item.SampleRate * item.BitDepth);
        }

        public static AudioInfo GetStreamInfo(AudioInfo aStream)
        {
            using (MediaInfoContainer mi = Processing.GetMediaInfo(aStream.TempFile))
            {
                if (mi.Audio.Count > 0)
                {
                    aStream.Bitrate = mi.Audio[0].BitRate;
                    aStream.BitDepth = mi.Audio[0].BitDepth;
                    aStream.ChannelCount = mi.Audio[0].Channels;
                    aStream.SampleRate = mi.Audio[0].SamplingRate;
                    aStream.Format = mi.Audio[0].Format;
                    aStream.FormatProfile = mi.Audio[0].FormatProfile;
                    aStream.StreamSize = Processing.GetFileSize(aStream.TempFile);
                    if (aStream.Format == "PCM")
                        aStream.Length = GetRuntimePCM(aStream);
                    else
                        aStream.Length = mi.Audio[0].Duration/1000d; // convert from ms to seconds
                }
            }
            return aStream;
        }

    }
}
