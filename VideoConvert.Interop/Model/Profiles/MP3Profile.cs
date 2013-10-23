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
    public class MP3Profile : EncoderProfile
    {
        public int OutputChannels { get; set; }
        public int SampleRate { get; set; }
        public int EncodingMode { get; set; }
        public int Bitrate { get; set; }
        public int Quality { get; set; }
        public string Preset { get; set; }


        public MP3Profile()
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