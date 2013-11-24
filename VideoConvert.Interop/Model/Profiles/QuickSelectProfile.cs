// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuickSelectProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encoding profile for quick selection, defines the target output format. Carries information about video and audio encoding profiles
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    using Utilities;

    /// <summary>
    /// Encoding profile for quick selection, defines the target output format. Carries information about video and audio encoding profiles
    /// </summary>
    public class QuickSelectProfile : EncoderProfile
    {
        /// <summary>
        /// Audio encoding profile name
        /// </summary>
        public string AudioProfile { get; set; }

        /// <summary>
        /// Audio profile type
        /// </summary>
        public ProfileType AudioProfileType { get; set; }
        
        /// <summary>
        /// Video encoding profile name
        /// </summary>
        public string VideoProfile { get; set; }

        /// <summary>
        /// Video profile type
        /// </summary>
        public ProfileType VideoProfileType { get; set; }

        /// <summary>
        /// Target output format
        /// </summary>
        public OutputType OutFormat { get; set; }

        /// <summary>
        /// Target output format, string value
        /// </summary>
        public string OutFormatStr { get { return GenHelper.StringValueOf(OutFormat); } }

        /// <summary>
        /// Target stereoscopic format
        /// </summary>
        public StereoEncoding StereoType { get; set; }

        /// <summary>
        /// Target system type (PAL / NTSC)
        /// </summary>
        public int SystemType { get; set; }

        /// <summary>
        /// Enable deinterlacing
        /// </summary>
        public bool Deinterlace { get; set; }

        /// <summary>
        /// Enable autocropping / resizing
        /// </summary>
        public bool AutoCropResize { get; set; }

        /// <summary>
        /// Keep input resolution
        /// </summary>
        public bool KeepInputResolution { get; set; }

        /// <summary>
        /// Target video width
        /// </summary>
        public int TargetWidth { get; set; }

        /// <summary>
        /// Target file size
        /// </summary>
        public ulong TargetFileSize { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public QuickSelectProfile()
        {
            Type = ProfileType.QuickSelect;

            AudioProfile = string.Empty;
            AudioProfileType = ProfileType.None;

            VideoProfile = string.Empty;
            VideoProfileType = ProfileType.None;

            OutFormat = OutputType.OutputMatroska;

            Deinterlace = false;
            AutoCropResize = true;
            KeepInputResolution = false;

            TargetWidth = 1920;
            TargetFileSize = 0;
            StereoType = StereoEncoding.None;
        }
    }
}
