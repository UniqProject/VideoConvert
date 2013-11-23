// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Default encoder profile
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    /// <summary>
    /// Default encoder profile
    /// </summary>
    public class EncoderProfile
    {
        /// <summary>
        /// Profile name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Profile type
        /// </summary>
        public ProfileType Type { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EncoderProfile()
        {
            Name = string.Empty;
            Type = ProfileType.None;
        }
    }
}
