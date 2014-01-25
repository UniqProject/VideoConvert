// --------------------------------------------------------------------------------------------------------------------
// <copyright file="x264Profile.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   x264 Encoding Profile
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    using VideoConvert.Interop.Model.x264;

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
        /// Constant Quantizer
        /// The number you give here specifies the P-frame quantizer. The quantizer used for I- and B-frames is derived from <see cref="QuantizerRatioIP"/> and <see cref="QuantizerRatioPB"/>.
        /// CQ mode targets a certain quantizer, which means final filesize is not known (although it can be reasonably accurately estimated with some methods). 
        /// A setting of 0 will produce lossless output. qp produces larger files than <see cref="QualitySetting"/> for the same visual quality. 
        /// qp mode also disables adaptive quantization, since by definition 'constant quantizer' implies no adaptive quantization.
        /// 
        /// This option is mutually exclusive with <see cref="VbrSetting"/> and <see cref="QualitySetting"/>. 
        /// See http://git.videolan.org/?p=x264.git;a=blob_plain;f=doc/ratecontrol.txt;hb=HEAD for more information on the various ratecontrol systems.
        /// You should generally use <see cref="QualitySetting"/> instead, although qp doesn't require lookahead to run and thus can be faster.
        /// </summary>
        public int QuantizerSetting { get; set; }

        /// <summary>
        /// Constant Ratefactor
        /// While qp targets a certain quantizer, and bitrate targets a certain filesize, crf targets a certain 'quality'. 
        /// The idea is for crf to give the same perceptual quality as qp n, just in a smaller space. The arbitrary unit of measure for crf values is the "ratefactor".
        /// CRF achieves this by reducing the quality of 'less important' frames. In this context, 'less important' means frames in complex or high-motion scenes,
        ///  where quality is either more expensive (in terms of bits) or less visible, will have their quantizer increased. 
        /// The bits saved in frames like these are redistributed to frames where they will be more effective.
        /// CRF will take less time than a 2pass bitrate encode, because the 'first pass' from a 2pass encode was skipped. 
        /// On the other hand, it's impossible to predict the bitrate a CRF encode will come out to. It's up to you to decide which rate-control mode is better for your circumstances.
        /// This option is mutually exclusive with <see cref="QuantizerSetting"/> and <see cref="VbrSetting"/>. 
        /// See http://git.videolan.org/?p=x264.git;a=blob_plain;f=doc/ratecontrol.txt;hb=HEAD for more information on the various ratecontrol systems.
        /// </summary>
        public int QualitySetting { get; set; }

        /// <summary>
        /// VBR Bitrate
        /// Encode the video in target bitrate mode. Target bitrate mode means the final filesize is known, but the final quality is not. 
        /// x264 will attempt to encode the video to target the given bitrate as the overall average. The parameter given is the bitrate in kilobits/sec. (8bits = 1byte and so on).
        /// Note that 1 kilobit is 1000, not 1024 bits.
        /// This setting is often used in conjunction with --pass for two-pass encoding.
        /// This option is mutually exclusive with <see cref="QuantizerSetting"/> and <see cref="QualitySetting"/>. 
        /// See http://git.videolan.org/?p=x264.git;a=blob_plain;f=doc/ratecontrol.txt;hb=HEAD for more information on the various ratecontrol systems.
        /// </summary>
        public int VbrSetting { get; set; }

        /// <summary>
        /// Tune options to further optimize them for your input content. If you specify a tuning, the changes will be applied after <see cref="Preset"/> but before all other parameters.
        /// If your source content matches one of the available tunings you can use this, otherwise leave unset.
        /// Values available: film, animation, grain, stillimage, psnr, ssim, fastdecode, zerolatency.
        /// </summary>
        public int Tuning { get; set; }

        /// <summary>
        /// Limit the profile of the output stream. If you specify a profile, it overrides all other settings, so if you use it, you will be guaranteed a compatible stream.
        ///  If you set this option, you cannot use lossless encoding (<see cref="QuantizerSetting"/> 0 or <see cref="QualitySetting"/> 0).
        /// You should set this if you know your playback device only supports a certain profile. Most decoders support High profile, so there's no need to set this.
        /// Values available: baseline, main, high, high10, high422, high444.
        /// </summary>
        public int AvcProfile { get; set; }

        /// <summary>
        /// Sets the level flag in the output bitstream (as defined by Annex A of the H.264 standard). Permissible levels are:
        /// 1 1b 1.1 1.2 1.3 2 2.1 2.2 3 3.1 3.2 4 4.1 4.2 5 5.1
        /// If you do not specify --level on the commandline, x264 will attempt to autodetect the level. 
        /// This detection is not perfect and may underestimate the level if you are not using VBV. 
        /// x264 will also automatically limit the DPB size (see <see cref="NumRefFrames"/>) to remain in compliance with the level you select (unless you also manually specify <see cref="NumRefFrames"/>).
        /// Note: specifying the level does not automatically set the <see cref="VBVMaxRate"/> or <see cref="VBVBufSize"/>, however it will warn if the level specific properties are exceeded.
        /// 
        /// What Level Do I Pick?
        /// Level 4.1 is often considered the highest level you can rely on desktop consumer hardware to support. 
        /// Blu-ray Discs only support level 4.1, and many non-mobile devices like the Xbox 360 specify level 4.1 as the highest they officially support. 
        /// Mobile devices like the iPhone/Android are a totally different story.
        /// Wikipedia has a nice chart detailing the restrictions for each level, if you want to read it. http://en.wikipedia.org/wiki/H.264/MPEG-4_AVC#Levels"
        /// Recommendation: Default, unless you are aiming for a specific device.
        /// </summary>
        public int AvcLevel { get; set; }

        /// <summary>
        /// Change options to trade off compression efficiency against encoding speed. If you specify a preset, the changes it makes will be applied before all other parameters are applied.
        /// You should generally set this option to the slowest you can bear.
        /// Values available: ultrafast, superfast, veryfast, faster, fast, medium, slow, slower, veryslow, placebo.
        /// </summary>
        public int Preset { get; set; }

        /// <summary>
        /// Tune for specific device
        /// See <see cref="X264Device"/> for a list of supported devices
        /// </summary>
        public int TuneDevice { get; set; }

        /// <summary>
        /// Use the loop filter (aka inloop deblocker), which is part of the H.264 standard. It is very efficient in terms of encoding time vs. quality gained.
        /// </summary>
        public bool UseDeblocking { get; set; }

        /// <summary>
        /// Deblocking strength
        /// The strength parameter refers to Alpha Deblocking.
        /// Alpha deblocking effects the overal amount of deblocking to be applied to the picture, higher values deblock more effectively, but also destroy more detail and cause the entire image to be softened.
        /// The default value of 0 is almost always sufficient to get rid of most blocking (especialy when using a cqm), but leaves the picture noticibly blurier. 
        /// In general use this value should be no lower then -3 and no higher then 3. 
        /// When using a cqm the authors recomended settings should be used as the default value, and shouldn't be altered by more then +/-2. 
        /// Alpha Deblocking is the most important parameter in determining the overal sharpness of your encode.
        /// Source: http://forum.doom9.org/showthread.php?t=109747
        /// </summary>
        public int DeblockingStrength { get; set; }

        /// <summary>
        /// Deblocking Threshold
        /// The Threshold Parameter refers to Beta Deblocking.
        /// Beta Deblocking is a bit more tricky to use, Beta Deblocking determines whether something in a block is a detail or not when deblocking is aplied to it.
        /// Lower values of Beta Deblocking apply less deblocking to more flat blocks with details present (but more deblocking to blocks without details), 
        /// while Higher values cause more deblocking to be applied to less flat blocks with details present. 
        /// Generally Beta Deblocking shouldn't be altered unless you are haveing problems with the default setting. 
        /// Raising Beta deblocking is a good way to help get rid of ringing artifacts by aplying more aggressive filtering to blocks that aren't very flat. 
        /// Lowering beta Deblocking is a good way to reduce the amount of DCT blocks without bluring the entire picture. 
        /// A high value of beta deblocking will cause nonflat blocks to be deblocked more aggressively, while a low value will cause the opposite.
        /// Source: http://forum.doom9.org/showthread.php?t=109747
        /// </summary>
        public int DeblockingThreshold { get; set; }

        /// <summary>
        /// Enable cabac
        /// Setting this to false disables CABAC (Context Adaptive Binary Arithmetic Coder) stream compression and falls back to the less efficient CAVLC (Context Adaptive Variable Length Coder) system, 
        /// which significantly reduces both the compression efficiency (10-20% typically) and the decoding requirements.
        /// </summary>
        public bool UseCabac { get; set; }

        /// <summary>
        /// GOP Calculation
        /// </summary>
        public int GopCalculation { get; set; }

        /// <summary>
        /// Max GOP Size
        /// </summary>
        public int MaxGopSize { get; set; }

        /// <summary>
        /// Min GOP Size
        /// </summary>
        public int MinGopSize { get; set; }

        /// <summary>
        /// Enable open GOP
        /// </summary>
        public bool UseOpenGop { get; set; }

        /// <summary>
        /// Number of encoding slices
        /// </summary>
        public int NumSlices { get; set; }

        /// <summary>
        /// Max slice size in bytes
        /// </summary>
        public int MaxSliceSizeBytes { get; set; }

        /// <summary>
        /// Max slice size in blocks
        /// </summary>
        public int MaxSliceSizeBlocks { get; set; }

        /// <summary>
        /// Enable weighted prediction
        /// </summary>
        public bool UseWeightedPred { get; set; }

        /// <summary>
        /// Number of B-Frames
        /// </summary>
        public int NumBFrames { get; set; }

        /// <summary>
        /// B-Frame Bias
        /// </summary>
        public int BFrameBias { get; set; }

        /// <summary>
        /// Number of Adaptive B-Frames
        /// </summary>
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
        /// From http://mewiki.project357.com/wiki/X264_Settings#non-deterministic:
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
        /// From http://mewiki.project357.com/wiki/X264_Settings#pic-struct:
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
            VbrSetting = 16000;
            Tuning = 0;
            AvcProfile = 2;
            AvcLevel = 11;
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
