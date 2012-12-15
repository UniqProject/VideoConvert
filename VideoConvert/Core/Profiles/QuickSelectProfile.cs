//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================


namespace VideoConvert.Core.Profiles
{ 
    public class QuickSelectProfile : EncoderProfile
    {
        public string AudioProfile { get; set; }
        public ProfileType AudioProfileType { get; set; }
        
        public string VideoProfile { get; set; }
        public ProfileType VideoProfileType { get; set; }

        public OutputType OutFormat { get; set; }
        public string OutFormatStr { get { return Processing.StringValueOf(OutFormat); } }
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
