// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioHelper.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities
{
    using System;
    using log4net;
    using Model;
    using Model.MediaInfo;
    using Model.Profiles;

    public class AudioHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AudioHelper));

        public static bool AudioProcessingNeeded(EncoderProfile aProfile)
        {
            int sampleRate = -1, channelOrder = -1;
            switch (aProfile.Type)
            {
                case ProfileType.AC3:
                    sampleRate = ((Ac3Profile)aProfile).SampleRate;
                    channelOrder = ((Ac3Profile)aProfile).OutputChannels;
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
            MediaInfoContainer mi = new MediaInfoContainer();
            try
            {
                mi = GenHelper.GetMediaInfo(aStream.TempFile);
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex);
                mi = new MediaInfoContainer();
            }
            finally
            {
                if (mi.Audio.Count > 0)
                {
                    aStream.Bitrate = mi.Audio[0].BitRate;
                    aStream.BitDepth = mi.Audio[0].BitDepth;
                    aStream.ChannelCount = mi.Audio[0].Channels;
                    aStream.SampleRate = mi.Audio[0].SamplingRate;
                    aStream.Format = mi.Audio[0].Format;
                    aStream.FormatProfile = mi.Audio[0].FormatProfile;
                    aStream.StreamSize = GenHelper.GetFileSize(aStream.TempFile);
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
