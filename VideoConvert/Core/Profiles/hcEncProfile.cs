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
    public class HcEncProfile : EncoderProfile
    {
        public int Bitrate { get; set; }
        public int Profile { get; set; }
        public int DCPrecision { get; set; }
        public int Interlacing { get; set; }
        public int FieldOrder { get; set; }
        public int ChromaDownsampling { get; set; }
        public int GopLength { get; set; }
        public int BFrames { get; set; }
        public int LuminanceGain { get; set; }
        public int AQ { get; set; }
        public int Matrix { get; set; }
        public int IntraVLC { get; set; }
        public int Colorimetry { get; set; }
        public int MPGLevel { get; set; }
        public int VBRBias { get; set; }

        public bool ClosedGops { get; set; }
        public bool SceneChange { get; set; }
        public bool AutoGOP { get; set; }
        public bool SMP { get; set; }
        public bool VBVCheck { get; set; }
        public bool LastIFrame { get; set; }
        public bool SeqEndCode { get; set; }
        public bool Allow3BFrames { get; set; }
        public bool UseLosslessFile { get; set; }


        public HcEncProfile()
        {
            Type = ProfileType.HcEnc;
            Bitrate = 8000;
            Profile = 2;
            DCPrecision = 2;
            Interlacing = 0;
            FieldOrder = 0;
            ChromaDownsampling = 0;
            GopLength = 15;
            BFrames = 2;
            LuminanceGain = 0;
            AQ = 2;
            Matrix = 0;
            IntraVLC = 0;
            Colorimetry = 0;
            MPGLevel = 0;
            VBRBias = 0;

            ClosedGops = true;
            SceneChange = true;
            AutoGOP = true;
            SMP = true;
            VBVCheck = true;
            LastIFrame = true;
            SeqEndCode = true;
            Allow3BFrames = false;
            UseLosslessFile = false;
        }
    }
}
