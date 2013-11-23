// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamCopyProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encoding Profile for Stream remuxing
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    /// <summary>
    /// Encoding Profile for Stream remuxing
    /// </summary>
    public class StreamCopyProfile : EncoderProfile
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public StreamCopyProfile()
        {
            Name = "Stream Copy";
            Type = ProfileType.Copy;
        }
    }
}