// --------------------------------------------------------------------------------------------------------------------
// <copyright file="x264Profile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    /// <summary>
    /// Stores an Encoding Profile for x264
    /// </summary>
    public class X264Profile : EncoderProfile
    {
        /// <summary>
        /// Defines the encoding mode
        /// </summary>
        public int EncodingMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int QuantizerSetting { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int QualitySetting { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int VBRSetting { get; set; }
        public int Tuning { get; set; }
        public int AVCProfile { get; set; }
        public int AVCLevel { get; set; }
        public int Preset { get; set; }
        public int TuneDevice { get; set; }
        public bool UseDeblocking { get; set; }
        public int DeblockingStrength { get; set; }
        public int DeblockingThreshold { get; set; }
        public bool UseCabac { get; set; }
        public int GopCalculation { get; set; }
        public int MaxGopSize { get; set; }
        public int MinGopSize { get; set; }
        public bool UseOpenGop { get; set; }
        public int NumSlices { get; set; }
        public int MaxSliceSizeBytes { get; set; }
        public int MaxSliceSizeBlocks { get; set; }
        public bool UseWeightedPred { get; set; }
        public int NumBFrames { get; set; }
        public int BFrameBias { get; set; }
        public int AdaptiveBFrames { get; set; }
        public int BPyramid { get; set; }
        public int NumRefFrames { get; set; }
        public int NumExtraIFrames { get; set; }
        public int PFrameWeightedPrediction { get; set; }
        public int InterlaceMode { get; set; }
        public int Pulldown { get; set; }
        public bool UseAdaptiveIFrameDecision { get; set; }
        public int QuantizerMin { get; set; }
        public int QuantizerMax { get; set; }
        public int QuantizerDelta { get; set; }
        public float QuantizerRatioIP { get; set; }
        public float QuantizerRatioPB { get; set; }
        public int DeadZoneInter { get; set; }
        public int DeadZoneIntra { get; set; }
        public int ChromaQPOffset { get; set; }
        public int CreditsQuantizer { get; set; }
        public int VBVBufSize { get; set; }
        public int VBVMaxRate { get; set; }
        public float VBVInitialBuffer { get; set; }
        public float BitrateVariance { get; set; }
        public float QuantizerCompression { get; set; }
        public int TempBlurFrameComplexity { get; set; }
        public float TempBlurQuant { get; set; }
        public int AdaptiveQuantizersMode { get; set; }
        public float AdaptiveQuantizersStrength { get; set; }
        public int QuantizerMatrix { get; set; }
        public int NumFramesLookahead { get; set; }
        public bool UseMBTree { get; set; }
        public bool UseChromaMotionEstimation { get; set; }
        public int MotionEstimationRange { get; set; }
        public int MotionEstimationAlgorithm { get; set; }
        public int SubPixelRefinement { get; set; }
        public int MVPredictionMod { get; set; }
        public int Trellis { get; set; }
        public float PsyRDStrength { get; set; }
        public float PsyTrellisStrength { get; set; }
        public bool UseNoMixedReferenceFrames { get; set; }
        public bool UseNoDCTDecimation { get; set; }
        public bool UseNoFastPSkip { get; set; }
        public bool UseNoPsychovisualEnhancements { get; set; }
        public int NoiseReduction { get; set; }
        public int MacroBlocksPartitions { get; set; }
        public bool MacroBlocksPartitionsAdaptiveDCT { get; set; }
        public bool MacroBlocksPartitionsI4X4 { get; set; }
        public bool MacroBlocksPartitionsP4X4 { get; set; }
        public bool MacroBlocksPartitionsI8X8 { get; set; }
        public bool MacroBlocksPartitionsP8X8 { get; set; }
        public bool MacroBlocksPartitionsB8X8 { get; set; }
        public int HRDInfo { get; set; }
        public bool UseAccessUnitDelimiters { get; set; }
        public bool UseFakeInterlaced { get; set; }
        public bool UseBluRayCompatibility { get; set; }
        public int VUIRange { get; set; }
        public int ColorPrimaries { get; set; }
        public int Transfer { get; set; }
        public int ColorMatrix { get; set; }
        public bool UseAutoSelectColorSettings { get; set; }
        public bool UsePSNRCalculation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool UseSSIMCalculation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ForceSAR { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool UseAutoSelectSAR { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int NumThreads { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool UseThreadInput { get; set; }

        /// <summary>
        /// Defines wheter non-deterministic parameter should be used.
        /// From <seealso cref="http://mewiki.project357.com/wiki/X264_Settings#non-deterministic"/>:
        /// Slightly improve quality when encoding with <see cref="NumThreads"/> > 1, at the cost of non-deterministic
        /// output encodes. This enables multi-threaded mv and uses the entire lookahead buffer in slicetype decisions
        /// when slicetype is threaded -- rather than just the minimum amount known to be available.
        /// Not for general use.
        /// </summary>
        public bool UseNonDeterministic { get; set; }

        /// <summary>
        /// Defines the use of slow first pass
        /// </summary>
        public bool UseSlowFirstPass { get; set; }

        /// <summary>
        /// Defines whether picstruct should be forced.
        /// From <seealso cref="http://mewiki.project357.com/wiki/X264_Settings#pic-struct"/>:
        /// Force sending pic_struct in Picture Timing SEI.
        /// Implied when you use <see cref="Pulldown"/> or <see cref="InterlaceMode"/>.
        /// </summary>
        public bool UseForcePicStruct { get; set; }

        /// <summary>
        /// Stores commandline arguments defined by user
        /// </summary>
        public string CustomCommandLine { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public X264Profile()
        {
            Type = ProfileType.X264;

            EncodingMode = 0;
            QuantizerSetting = 20;
            QualitySetting = 20;
            VBRSetting = 16000;
            Tuning = 0;
            AVCProfile = 2;
            AVCLevel = 11;
            Preset = 5;
            TuneDevice = 0;
            UseDeblocking = true;
            DeblockingStrength = 0;
            DeblockingThreshold = 0;
            UseCabac = true;
            GopCalculation = 1;
            MaxGopSize = 250;
            MinGopSize = 25;
            UseOpenGop = false;
            NumSlices = 0;
            MaxSliceSizeBytes = 0;
            MaxSliceSizeBlocks = 0;
            UseWeightedPred = true;
            NumBFrames = 3;
            BFrameBias = 0;
            AdaptiveBFrames = 1;
            BPyramid = 2;
            NumRefFrames = 3;
            NumExtraIFrames = 40;
            PFrameWeightedPrediction = 2;
            InterlaceMode = 1;
            Pulldown = 1;
            UseAdaptiveIFrameDecision = true;
            QuantizerMin = 0;
            QuantizerMax = 69;
            QuantizerDelta = 4;
            QuantizerRatioIP = 1.4f;
            QuantizerRatioPB = 1.3f;
            DeadZoneInter = 21;
            DeadZoneIntra = 11;
            ChromaQPOffset = 0;
            CreditsQuantizer = 40;
            VBVBufSize = 0;
            VBVMaxRate = 0;
            VBVInitialBuffer = 0.9f;
            BitrateVariance = 1.0f;
            QuantizerCompression = 0.6f;
            TempBlurFrameComplexity = 20;
            TempBlurQuant = 0.5f;
            AdaptiveQuantizersMode = 1;
            AdaptiveQuantizersStrength = 1.0f;
            QuantizerMatrix = 0;
            NumFramesLookahead = 40;
            UseMBTree = true;
            UseChromaMotionEstimation = true;
            MotionEstimationRange = 16;
            MotionEstimationAlgorithm = 1;
            SubPixelRefinement = 7;
            MVPredictionMod = 1;
            Trellis = 1;
            PsyRDStrength = 1.0f;
            PsyTrellisStrength = 0.0f;
            UseNoMixedReferenceFrames = false;
            UseNoDCTDecimation = false;
            UseNoFastPSkip = false;
            UseNoPsychovisualEnhancements = false;
            NoiseReduction = 0;
            MacroBlocksPartitions = 3;
            MacroBlocksPartitionsAdaptiveDCT = true;
            MacroBlocksPartitionsI4X4 = true;
            MacroBlocksPartitionsP4X4 = false;
            MacroBlocksPartitionsI8X8 = true;
            MacroBlocksPartitionsP8X8 = true;
            MacroBlocksPartitionsB8X8 = true;
            HRDInfo = 0;
            UseAccessUnitDelimiters = false;
            UseFakeInterlaced = false;
            UseBluRayCompatibility = false;
            VUIRange = 0;
            ColorPrimaries = 0;
            Transfer = 0;
            ColorMatrix = 0;
            UseAutoSelectColorSettings = true;
            UsePSNRCalculation = false;
            UseSSIMCalculation = false;
            ForceSAR = 0;
            UseAutoSelectSAR = true;
            NumThreads = 0;
            UseThreadInput = true;
            UseNonDeterministic = false;
            UseSlowFirstPass = false;
            UseForcePicStruct = false;
            CustomCommandLine = string.Empty;
        }
    }
}
