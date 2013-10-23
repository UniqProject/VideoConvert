// --------------------------------------------------------------------------------------------------------------------
// <copyright file="hcEncProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
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
