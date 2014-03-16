// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AC3Profile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encoding profile for AC3 audio
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    /// <summary>
    /// Encoding profile for AC3 audio
    /// </summary>
    public class Ac3Profile : EncoderProfile
    {
        /// <summary>
        /// Apply Dynamic range compression
        /// </summary>
        public bool ApplyDynamicRangeCompression { get; set; }

        /// <summary>
        /// Output channel count
        /// </summary>
        public int OutputChannels { get; set; }

        /// <summary>
        /// Target sample rate
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// Target bitrate
        /// </summary>
        public int Bitrate { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Ac3Profile()
        {
            Type = ProfileType.Ac3;

            ApplyDynamicRangeCompression = true;
            OutputChannels = 0;
            SampleRate = 0;
            Bitrate = 10;
        }
    }
}
