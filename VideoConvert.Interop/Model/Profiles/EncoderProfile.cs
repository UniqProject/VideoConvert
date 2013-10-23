// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    public class EncoderProfile
    {
        public string Name { get; set; }
        public ProfileType Type { get; set; }

        public EncoderProfile()
        {
            Name = string.Empty;
            Type = ProfileType.None;
        }
    }
}
