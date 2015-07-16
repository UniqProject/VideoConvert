// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioHelper.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Audio helper class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities
{
    using System;
    using log4net;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Model.MediaInfo;
    using VideoConvert.Interop.Model.Profiles;

    /// <summary>
    /// Audio helper class
    /// </summary>
    public class AudioHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AudioHelper));

        /// <summary>
        /// Checks if Audio processing is needed for given Profile
        /// </summary>
        /// <param name="aProfile">Audio Encoding Profile</param>
        /// <returns></returns>
        public static bool AudioProcessingNeeded(EncoderProfile aProfile)
        {
            int sampleRate = -1, channelOrder = -1;
            switch (aProfile.Type)
            {
                case ProfileType.Ac3:
                    sampleRate = ((Ac3Profile)aProfile).SampleRate;
                    channelOrder = ((Ac3Profile)aProfile).OutputChannels;
                    break;
                case ProfileType.Flac:
                    break;
                default:
                    return false;
            }
            return sampleRate > -1 || channelOrder > -1;
        }

        /// <summary>
        /// Checks if Audio encoding is needed for given Profile
        /// </summary>
        /// <param name="aProfile">Audio Encoding Profile</param>
        /// <returns></returns>
        public static bool AudioEncodingNeeded(EncoderProfile aProfile)
        {
            switch (aProfile.Type)
            {
                case ProfileType.Ac3:
                case ProfileType.Flac:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if we can use W64 (Wave64) for PCM streams
        /// </summary>
        /// <param name="outType"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks if we can use FLAC streams
        /// </summary>
        /// <param name="outType"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculates PCM stream runtime
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static double GetRuntimePcm(AudioInfo item)
        {
            return (item.StreamSize * 8D) / item.ChannelCount / ((double)item.SampleRate * item.BitDepth);
        }

        /// <summary>
        /// Get Audio stream properties
        /// </summary>
        /// <param name="aStream"></param>
        /// <returns></returns>
        public static AudioInfo GetStreamInfo(AudioInfo aStream)
        {
            var mi = new MediaInfoContainer();
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
                        aStream.Length = GetRuntimePcm(aStream);
                    else
                        aStream.Length = mi.Audio[0].Duration/1000d; // convert from ms to seconds
                }
            }
            return aStream;
        }

    }
}
