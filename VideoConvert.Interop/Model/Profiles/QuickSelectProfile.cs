// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuickSelectProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    using Utilities;

    public class QuickSelectProfile : EncoderProfile
    {
        public string AudioProfile { get; set; }
        public ProfileType AudioProfileType { get; set; }
        
        public string VideoProfile { get; set; }
        public ProfileType VideoProfileType { get; set; }

        public OutputType OutFormat { get; set; }
        public string OutFormatStr { get { return GenHelper.StringValueOf(OutFormat); } }
        public StereoEncoding StereoType { get; set; }

        public int SystemType { get; set; }

        public bool Deinterlace { get; set; }

        public bool AutoCropResize { get; set; }
        public bool KeepInputResolution { get; set; }

        public int TargetWidth { get; set; }
        public ulong TargetFileSize { get; set; }

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
