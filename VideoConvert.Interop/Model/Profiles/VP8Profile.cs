// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VP8Profile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encoder Profile for VPX library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    /// <summary>
    /// Encoder Profile for VPX library
    /// </summary>
    public class Vp8Profile : EncoderProfile
    {
        /// <summary>
        /// Encoding Mode
        /// </summary>
        public int EncodingMode { get; set; }

        /// <summary>
        /// Target bitrate
        /// </summary>
        public int Bitrate { get; set; }

        /// <summary>
        /// Bitrate mode
        /// </summary>
        public int BitrateMode { get; set; }

        /// <summary>
        /// Selected encoder (0 = vp8, 1 = vp9)
        /// </summary>
        public int Encoder { get; set; } // 0 = vp8, 1 = vp9

        /// <summary>
        /// Encoding profile
        /// </summary>
        public int Profile { get; set; }

        /// <summary>
        /// Speed control
        /// </summary>
        public int SpeedControl { get; set; }

        /// <summary>
        /// CPU modifier
        /// </summary>
        public int CpuModifier { get; set; }

        /// <summary>
        /// Deadline per frame
        /// </summary>
        public int DeadlinePerFrame { get; set; }

        /// <summary>
        /// Token part
        /// </summary>
        public int TokenPart { get; set; }

        /// <summary>
        /// Noise filtering
        /// </summary>
        public int NoiseFiltering { get; set; }

        /// <summary>
        /// Sharpness
        /// </summary>
        public int Sharpness { get; set; }

        /// <summary>
        /// Encoding threads count
        /// </summary>
        public int Threads { get; set; }

        /// <summary>
        /// Static threshold
        /// </summary>
        public int StaticThreshold { get; set; }

        /// <summary>
        /// Enable Error Resilience
        /// </summary>
        public bool UseErrorResilience { get; set; }

        /// <summary>
        /// GOP min
        /// </summary>
        public int GopMin { get; set; }

        /// <summary>
        /// GOP max
        /// </summary>
        public int GopMax { get; set; }

        /// <summary>
        /// Max frames lag count
        /// </summary>
        public int MaxFramesLag { get; set; }

        /// <summary>
        /// Frame drop
        /// </summary>
        public int FrameDrop { get; set; }

        /// <summary>
        /// Enable spatial resampling
        /// </summary>
        public bool UseSpatialResampling { get; set; }

        /// <summary>
        /// Downscale Threshold
        /// </summary>
        public int DownscaleThreshold { get; set; }

        /// <summary>
        /// Upscale threshold
        /// </summary>
        public int UpscaleThreshold { get; set; }

        /// <summary>
        /// Enable ARNR frame decision
        /// </summary>
        public bool UseArnrFrameDecision { get; set; }

        /// <summary>
        /// ARNR max frames
        /// </summary>
        public int ArnrMaxFrames { get; set; }

        /// <summary>
        /// ARNR strength
        /// </summary>
        public int ArnrStrength { get; set; }

        /// <summary>
        /// Initial buffer size
        /// </summary>
        public int InitialBufferSize { get; set; }

        /// <summary>
        /// Optimal buffer size
        /// </summary>
        public int OptimalBufferSize { get; set; }

        /// <summary>
        /// Buffer size
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// Undershoot data rate
        /// </summary>
        public int UndershootDataRate { get; set; }

        /// <summary>
        /// Quantizer Min
        /// </summary>
        public int QuantizerMin { get; set; }

        /// <summary>
        /// Quantizer max
        /// </summary>
        public int QuantizerMax { get; set; }

        /// <summary>
        /// Bias frame adjust
        /// </summary>
        public int BiasFrameAdjust { get; set; }

        /// <summary>
        /// section min
        /// </summary>
        public int SectionMin { get; set; }

        /// <summary>
        /// section max
        /// </summary>
        public int SectionMax { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Vp8Profile()
        {
            Type = ProfileType.Vp8;

            EncodingMode = 0;
            Bitrate = 1000;
            BitrateMode = 0;
            Encoder = 0;
            Profile = 0;
            SpeedControl = 1;
            CpuModifier = 3;
            DeadlinePerFrame = 1000000;
            TokenPart = 1;
            NoiseFiltering = 0;
            Sharpness = 0;
            Threads = 0;
            StaticThreshold = 0;
            GopMin = 0;
            GopMax = 250;
            MaxFramesLag = 25;
            FrameDrop = 0;
            UseSpatialResampling = false;
            DownscaleThreshold = 100;
            UpscaleThreshold = 100;
            UseArnrFrameDecision = true;
            ArnrMaxFrames = 5;
            ArnrStrength = 3;
            InitialBufferSize = 4;
            OptimalBufferSize = 5;
            BufferSize = 6;
            UndershootDataRate = 0;
            QuantizerMin = 0;
            QuantizerMax = 63;
            BiasFrameAdjust = 70;
            SectionMin = 15;
            SectionMax = 10000;
        }
    }
}
