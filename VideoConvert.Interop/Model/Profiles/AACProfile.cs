// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AACProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    public class AACProfile : EncoderProfile
    {
        public int OutputChannels { get; set; }
        public int SampleRate { get; set; }
        public int EncodingMode { get; set; }
        public int Bitrate { get; set; }
        public float Quality { get; set; }


        public AACProfile()
        {
            Type = ProfileType.AAC;

            OutputChannels = 0;
            SampleRate = 0;
            Bitrate = 192;
            Quality = 0.3f;
            EncodingMode = 2;
        }
    }
}