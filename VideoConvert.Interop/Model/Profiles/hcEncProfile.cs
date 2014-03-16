// --------------------------------------------------------------------------------------------------------------------
// <copyright file="hcEncProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encoder Profile for HcEnc
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    /// <summary>
    /// Encoder Profile for HcEnc
    /// </summary>
    public class HcEncProfile : EncoderProfile
    {
        /// <summary>
        /// Target bitrate
        /// </summary>
        public int Bitrate { get; set; }

        /// <summary>
        /// Encoding profile
        /// </summary>
        public int Profile { get; set; }

        /// <summary>
        /// DC precision
        /// </summary>
        public int DcPrecision { get; set; }

        /// <summary>
        /// Interlacing
        /// </summary>
        public int Interlacing { get; set; }

        /// <summary>
        /// Field order
        /// </summary>
        public int FieldOrder { get; set; }

        /// <summary>
        /// Chroma downsampling
        /// </summary>
        public int ChromaDownsampling { get; set; }

        /// <summary>
        /// GOP length
        /// </summary>
        public int GopLength { get; set; }

        /// <summary>
        /// Num B-Frames
        /// </summary>
        public int BFrames { get; set; }

        /// <summary>
        /// Luminance gain
        /// </summary>
        public int LuminanceGain { get; set; }

        /// <summary>
        /// AQ
        /// </summary>
        public int Aq { get; set; }

        /// <summary>
        /// Quant Matrix
        /// </summary>
        public int Matrix { get; set; }

        /// <summary>
        /// Intra VLC
        /// </summary>
        public int IntraVlc { get; set; }

        /// <summary>
        /// Target Colorimetry
        /// </summary>
        public int Colorimetry { get; set; }

        /// <summary>
        /// Target MPEG Level
        /// </summary>
        public int MpgLevel { get; set; }

        /// <summary>
        /// VBR Bias
        /// </summary>
        public int VbrBias { get; set; }

        /// <summary>
        /// Enable closed GOPS
        /// </summary>
        public bool ClosedGops { get; set; }

        /// <summary>
        /// Detect Scene Change
        /// </summary>
        public bool SceneChange { get; set; }

        /// <summary>
        /// Enable auto-GOP
        /// </summary>
        public bool AutoGop { get; set; }

        /// <summary>
        /// Enable SMP processing (Multi-CPU)
        /// </summary>
        public bool Smp { get; set; }

        /// <summary>
        /// Check VBV buffer
        /// </summary>
        public bool VbvCheck { get; set; }

        /// <summary>
        /// Encode Last Frame as I-Frame
        /// </summary>
        public bool LastIFrame { get; set; }

        /// <summary>
        /// Write Sequence endcode
        /// </summary>
        public bool SeqEndCode { get; set; }

        /// <summary>
        /// Allow 3 B-Frames (non DVD-compliant)
        /// </summary>
        public bool Allow3BFrames { get; set; }

        /// <summary>
        /// Use Lossless File for 2 pass encoding
        /// </summary>
        public bool UseLosslessFile { get; set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        public HcEncProfile()
        {
            Type = ProfileType.HcEnc;
            Bitrate = 8000;
            Profile = 2;
            DcPrecision = 2;
            Interlacing = 0;
            FieldOrder = 0;
            ChromaDownsampling = 0;
            GopLength = 15;
            BFrames = 2;
            LuminanceGain = 0;
            Aq = 2;
            Matrix = 0;
            IntraVlc = 0;
            Colorimetry = 0;
            MpgLevel = 0;
            VbrBias = 0;

            ClosedGops = true;
            SceneChange = true;
            AutoGop = true;
            Smp = true;
            VbvCheck = true;
            LastIFrame = true;
            SeqEndCode = true;
            Allow3BFrames = false;
            UseLosslessFile = false;
        }
    }
}
