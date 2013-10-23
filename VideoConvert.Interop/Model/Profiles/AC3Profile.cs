// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AC3Profile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    public class AC3Profile : EncoderProfile
    {
        public bool ApplyDynamicRangeCompression { get; set; }
        public int OutputChannels { get; set; }
        public int SampleRate { get; set; }
        public int Bitrate { get; set; }

        public AC3Profile()
        {
            Type = ProfileType.AC3;

            ApplyDynamicRangeCompression = true;
            OutputChannels = 0;
            SampleRate = 0;
            Bitrate = 10;
        }
    }
}
