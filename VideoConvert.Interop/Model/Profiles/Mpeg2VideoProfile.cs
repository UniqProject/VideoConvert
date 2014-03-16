// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Mpeg2VideoProfile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Profile for use with a mpeg 2 video encoder
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    /// <summary>
    /// Profile for use with a mpeg 2 video encoder
    /// </summary>
    public class Mpeg2VideoProfile : EncoderProfile
    {
        /// <summary>
        /// Gets or sets the Encoding Mode. 0 - 1pass VBR, 1 - 2pass VBR
        /// </summary>
        public int EncodingMode { get; set; }

        /// <summary>
        /// Sets or gets the target bitrate
        /// </summary>
        public int Bitrate { get; set; }

        /// <summary>
        /// Sets or gets the target intra dc precision
        /// </summary>
        public int DcPrecision { get; set; }

        /// <summary>
        /// Gets or sets the Macroblock Decision Algorithm (high quality mode)
        /// </summary>
        public int MbDecision { get; set; }

        /// <summary>
        /// Gets or sets Trellis (rate-distortion optimal quantization)
        /// </summary>
        public int Trellis { get; set; }

        /// <summary>
        /// Gets or sets full-pel ME compare function
        /// </summary>
        public int Cmp { get; set; }

        /// <summary>
        /// Gets or sets sub-pel ME compare function
        /// </summary>
        public int SubCmp { get; set; }

        /// <summary>
        /// Sets or gets the target amount of B-Frames
        /// </summary>
        public int BFrames { get; set; }

        /// <summary>
        /// Sets or gets the target GOP (Group of pictures) length
        /// </summary>
        public int GopLength { get; set; }

        /// <summary>
        /// Sets or gets the target field order (top=1/bottom=0/auto=-1 field first)
        /// </summary>
        public int FieldOrder { get; set; }

        /// <summary>
        /// Sets or gets forced use of closed gops
        /// </summary>
        public bool ClosedGops { get; set; }

        /// <summary>
        /// Sets or gets the use of auto calculated GOP
        /// </summary>
        public bool AutoGop { get; set; }

        /// <summary>
        /// Default constructor. Creates an Mpeg2Video Profile with default settings.
        /// </summary>
        public Mpeg2VideoProfile()
        {
            Type = ProfileType.Mpeg2Video;
            EncodingMode = 1;
            Bitrate = 8000;
            DcPrecision = 2;
            MbDecision = 0;
            Trellis = 0;
            Cmp = 0;
            SubCmp = 0;
            BFrames = 2;
            GopLength = 15;
            FieldOrder = 0;
            
            ClosedGops = true;
            AutoGop = true;
        }

        
    }
}