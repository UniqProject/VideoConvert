// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MP3Profile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    /// <summary>
    /// Encoding profile for MP3 audio
    /// </summary>
    public class Mp3Profile : EncoderProfile
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
        public int Quality { get; set; }

        /// <summary>
        /// Encoding preset
        /// </summary>
        public string Preset { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Mp3Profile()
        {
            Type = ProfileType.MP3;

            OutputChannels = 0;
            SampleRate = 0;
            Bitrate = 192;
            Quality = 4;
            EncodingMode = 2;
            Preset = "standard";
        }
    }
}