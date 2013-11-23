// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OggProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encoding profile for Ogg audio
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    /// <summary>
    /// Encoding profile for Ogg audio
    /// </summary>
    public class OggProfile : EncoderProfile
    {
        /// <summary>
        /// Number of output channels
        /// </summary>
        public int OutputChannels { get; set; }

        /// <summary>
        /// Target sample rate
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// Encoding mode
        /// </summary>
        public int EncodingMode { get; set; }

        /// <summary>
        /// Target bitrate
        /// </summary>
        public int Bitrate { get; set; }

        /// <summary>
        /// Target quality setting
        /// </summary>
        public float Quality { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public OggProfile()
        {
            Type = ProfileType.OGG;

            OutputChannels = 0;
            SampleRate = 0;
            Bitrate = 192;
            Quality = 3.0f;
            EncodingMode = 2;
        }
    }
}