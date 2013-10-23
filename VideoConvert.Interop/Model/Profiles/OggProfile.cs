// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OggProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    public class OggProfile : EncoderProfile
    {
        public int OutputChannels { get; set; }
        public int SampleRate { get; set; }
        public int EncodingMode { get; set; }
        public int Bitrate { get; set; }
        public float Quality { get; set; }


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