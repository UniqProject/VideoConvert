// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VP8Profile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    public class VP8Profile : EncoderProfile
    {
        public int EncodingMode { get; set; }
        public int Bitrate { get; set; }
        public int BitrateMode { get; set; }
        public int Encoder { get; set; } // 0 = vp8, 1 = vp9
        public int Profile { get; set; }
        public int SpeedControl { get; set; }
        public int CPUModifier { get; set; }
        public int DeadlinePerFrame { get; set; }
        public int TokenPart { get; set; }
        public int NoiseFiltering { get; set; }
        public int Sharpness { get; set; }
        public int Threads { get; set; }
        public int StaticThreshold { get; set; }
        public bool UseErrorResilience { get; set; }
        public int GopMin { get; set; }
        public int GopMax { get; set; }
        public int MaxFramesLag { get; set; }
        public int FrameDrop { get; set; }
        public bool UseSpatialResampling { get; set; }
        public int DownscaleThreshold { get; set; }
        public int UpscaleThreshold { get; set; }
        public bool UseArnrFrameDecision { get; set; }
        public int ArnrMaxFrames { get; set; }
        public int ArnrStrength { get; set; }
        public int InitialBufferSize { get; set; }
        public int OptimalBufferSize { get; set; }
        public int BufferSize { get; set; }
        public int UndershootDataRate { get; set; }
        public int QuantizerMin { get; set; }
        public int QuantizerMax { get; set; }
        public int BiasFrameAdjust { get; set; }
        public int SectionMin { get; set; }
        public int SectionMax { get; set; }

        public VP8Profile()
        {
            Type = ProfileType.VP8;

            EncodingMode = 0;
            Bitrate = 1000;
            BitrateMode = 0;
            Encoder = 0;
            Profile = 0;
            SpeedControl = 1;
            CPUModifier = 3;
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
