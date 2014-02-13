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
        /// The number you give here specifies the P-frame quantizer. The quantizer used for I- and B-frames 
        /// is derived from <see cref="QuantizerRatioIp"/> and <see cref="QuantizerRatioPb"/>.
        /// CQ mode targets a certain quantizer, which means final filesize is not known 
        /// (although it can be reasonably accurately estimated with some methods). 
        /// A setting of 0 will produce lossless output. qp produces larger files than 
        /// <see cref="QualitySetting"/> for the same visual quality. 
        /// qp mode also disables adaptive quantization, since by definition 'constant quantizer' 
        /// implies no adaptive quantization.
        /// 
        /// This option is mutually exclusive with <see cref="VbrSetting"/> and <see cref="QualitySetting"/>. 
        /// See http://git.videolan.org/?p=x264.git;a=blob_plain;f=doc/ratecontrol.txt;hb=HEAD 
        /// for more information on the various ratecontrol systems.
        /// You should generally use <see cref="QualitySetting"/> instead, 
        /// although qp doesn't require lookahead to run and thus can be faster.
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
        /// Note: specifying the level does not automatically set the <see cref="VbvMaxRate"/> or <see cref="VbvBufSize"/>, however it will warn if the level specific properties are exceeded.
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
        /// GOP Calculation behavior
        /// </summary>
        public int GopCalculation { get; set; }

        /// <summary>
        /// Max GOP Size
        /// Sets the maximum interval between IDR-frames (aka keyframes) in x264's output. You can specify "infinite" to never insert non-scenecut IDR-frames.
        /// IDR-frames are 'delimiters' in the stream - no frame can reference data from the other side of the IDR-frame. As well as this, IDR-frames are also I-frames,
        ///  so they don't reference data from any other frame. This means they can be used as seek points in a video.
        /// Note that I-frames are generally significantly larger than P/B-frames (often 10x or more in low motion scenes), 
        /// so they can play havoc with ratecontrol when combined with aggressively low VBV settings (eg, sub-second buffer sizes). In these cases, investigate --intra-refresh.
        /// The default setting is fine for most videos. When encoding for Blu-ray, broadcast, live streaming or certain other 
        /// specialist scenarios you may require a significantly smaller GOP length (often ~1x fps).
        /// </summary>
        public int MaxGopSize { get; set; }

        /// <summary>
        /// Min GOP Size
        /// Sets the minimum length between IDR-frames.
        /// See <see cref="MaxGopSize"/> for an explanation of IDR-frames. Very small keyint ranges can cause "incorrect" IDR-frame placement (for example, a strobing scene).
        /// This option limits the minimum length in frames after each IDR-frame before another can be placed.
        /// The maximum allowed value for MinGopSize is MaxGopSize/2+1
        /// Recommendation: Default, or 1x your framerate.
        /// </summary>
        public int MinGopSize { get; set; }

        /// <summary>
        /// Enable open GOP
        /// Open-GOP is an encoding technique which increases efficiency. Some decoders don't fully support open-GOP streams, 
        /// which is why it hasn't been enabled by default. You should test with all decoders your streams will be played on, 
        /// or (if that's impossible) wait until support is generally available.
        /// </summary>
        public bool UseOpenGop { get; set; }

        /// <summary>
        /// Number of encoding slices
        /// Sets the number of slices per frame, and forces rectangular slices. (Overridden by either <see cref="MaxSliceSizeBytes"/> or <see cref="MaxSliceSizeBlocks"/> if they are set.)
        /// If you are encoding for Blu-ray, set this to 4. Otherwise, don't use this unless you know you need to.
        /// </summary>
        public int NumSlices { get; set; }

        /// <summary>
        /// Max slice size in bytes
        /// Sets the maximum slice size in bytes, including estimated NAL overhead. (Currently is not compatible with <see cref="InterlaceMode"/>.)
        /// </summary>
        public int MaxSliceSizeBytes { get; set; }

        /// <summary>
        /// Max slice size in blocks
        /// Sets the maximum slice size in macroblocks. (Currently is not compatible with <see cref="InterlaceMode"/>.)
        /// </summary>
        public int MaxSliceSizeBlocks { get; set; }

        /// <summary>
        /// Enable weighted prediction
        /// H.264 allows you to 'weight' references in B-frames, which allows you to change how much each reference affects the predicted picture.
        /// Setting this to false disables that feature.
        /// </summary>
        public bool UseWeightedPred { get; set; }

        /// <summary>
        /// Number of B-Frames
        /// Sets the maximum number of concurrent B-frames that x264 can use.
        /// Without B-frames, a typical x264 stream has frame types like so: IPPPPP...PI. With NumBFrames 2, up to two consecutive P-frames can be replaced with B-frames, like: IBPBBPBPPPB...PI.
        /// B-frames are similar to P-frames, except they can use motion prediction from future frames as well. 
        /// This can lead to significantly better efficiency in terms of compression ratio. Their average quality is controlled by <see cref="QuantizerRatioPb"/>.
        /// </summary>
        public int NumBFrames { get; set; }

        /// <summary>
        /// B-Frame Bias
        /// Controls the likelihood of B-frames being used instead of P-frames. Values greater than 0 increase the weighting towards B-frames, 
        /// while values less than 0 do the opposite. This number is an arbitrary metric. The range is from -100 to 100. 
        /// A value of 100/-100 does not guarantee every/no P-frame will be converted (use <see cref="AdaptiveBFrames"/> 0 for that).
        /// Only use this if you think you make better ratecontrol decisions than x264.
        /// </summary>
        public int BFrameBias { get; set; }

        /// <summary>
        /// Set the adaptive B-frame placement decision algorithm. This setting controls how x264 decides between placing a P- or B-frame.
        /// 0. Disabled. Pick B-frames always. This is the same as what the older no-b-adapt setting did.
        /// 1. 'Fast' algorithm, faster, speed slightly increases with higher <see cref="NumBFrames"/> setting. When using this mode, you basically always want to use <see cref="NumBFrames"/> 16.
        /// 2. 'Optimal' algorithm, slower, speed significantly decreases with higher <see cref="NumBFrames"/> setting.
        /// Note: For a multi-pass encode, this option is only needed for the first pass where frame types are decided.
        /// </summary>
        public int AdaptiveBFrames { get; set; }

        /// <summary>
        /// Allow the use of B-frames as references for other frames. Without this setting, frames can only reference I- or P-frames.
        ///  Although I/P-frames are more valued as references because of their higher quality, B-frames can also be useful. 
        /// B-frames designated as references will get a quantizer halfway between P-frames and normal B-frames. 
        /// You need to use at least two B-frames before B-pyramid will work.
        /// If you're encoding for Blu-ray, use 'none' or 'strict'.
        /// 0 (none): do not allow B-frames to be used as references.
        /// 1 (strict): allow one B-frame per minigop to be used as reference; enforces restrictions imposed by the Blu-ray standard.
        /// 2 (normal): allow numerous B-frames per minigop to be used as references.
        /// </summary>
        public int BPyramid { get; set; }

        /// <summary>
        /// Controls the size of the DPB (Decoded Picture Buffer). The range is from 0-16. 
        /// In short, this value is the number of previous frames each P-frame can use as references.
        /// (B-frames can use one or two fewer, depending on if they are used as references or not.) 
        /// The minimum number of refs that can be referenced is 1.
        /// Also note that the H.264 spec limits DPB size for each level.
        /// If adhering to Level 4.1 specs, the maximum refs for 720p and 1080p video are 9 and 4 respectively. 
        /// You can read more about levels and 4.1 in particular under <see cref="AvcLevel"/>.
        /// </summary>
        public int NumRefFrames { get; set; }

        /// <summary>
        /// Sets the threshold for I/IDR frame placement (read: scene change detection).
        /// x264 calculates a metric for every frame to estimate how different it is from the previous frame. 
        /// If the value is lower than NumExtraIFrames, a 'scenecut' is detected. 
        /// An I-frame is placed if it has been less than <see cref="MinGopSize"/> frames since the last IDR-frame, 
        /// otherwise an IDR-frame is placed. Higher values of NumExtraIFrames increase the number of scenecuts detected. 
        /// For more information on how the scenecut comparison works, see this doom9 thread: http://forum.doom9.org/showthread.php?t=121116.
        /// Setting NumExtraIFrames to 0 is equivalent to setting <see cref="UseAdaptiveIFrameDecision"/>.
        /// Recommendation: Default
        /// </summary>
        public int NumExtraIFrames { get; set; }

        /// <summary>
        /// Enables use of explicit weighted prediction to improve compression in P-frames. Also improves quality in fades. Higher modes are slower.
        /// NOTE: When encoding for Adobe Flash set this to 1 - its decoder generates artifacts otherwise. Flash 10.1 fixes this bug.
        /// Modes:
        /// 0. Disabled.
        /// 1. Simple: fade analysis, but no reference duplication.
        /// 2. Smart: fade analysis and reference duplication.
        /// </summary>
        public int PFrameWeightedPrediction { get; set; }

        /// <summary>
        /// Sets interlacing Mode
        /// 0: Auto detect from source
        /// 1: Forces x264 to output in progressive mode.
        /// 2: Enable interlaced encoding and specify the top field is first. x264's interlaced encoding uses MBAFF, and is inherently less efficient than progressive encoding. 
        ///    For that reason, you should only encode interlaced if you intend to display the video on an interlaced display (or can't deinterlace the video before sending it to x264). 
        ///    Implies <see cref="UseForcePicStruct"/>.
        /// 3: Enable interlaced encoding and specify the bottom field is first.
        /// </summary>
        public int InterlaceMode { get; set; }

        /// <summary>
        /// Signal soft telecine for your (progressive, constant framerate) input stream using one of a few preset modes. 
        /// Soft telecine is explained in more detail on the HandBrake wiki (http://trac.handbrake.fr/wiki/Telecine).
        /// The available presets are: Auto (0), none (1), 22 (2), 32 (3), 64 (4), double (5), triple (6) and euro (7).
        /// Specifying any mode but none implies <see cref="UseForcePicStruct"/>.
        /// </summary>
        public int Pulldown { get; set; }

        /// <summary>
        /// Enables adaptive I-frame decision.
        /// </summary>
        public bool UseAdaptiveIFrameDecision { get; set; }

        /// <summary>
        /// Defines the minimum quantizer that x264 will ever use. The lower the quantizer, the closer the output is to the input. 
        /// At some point, the output of x264 will look the same as the input, even though it is not exactly the same. 
        /// Usually there is no reason to allow x264 to spend more bits than this on any particular macroblock.
        /// With adaptive quantization enabled (the default), raising QuantizerMin is discouraged because
        /// this could reduce the quality of flat background areas of the frame.
        /// </summary>
        public int QuantizerMin { get; set; }

        /// <summary>
        /// The opposite of <see cref="QuantizerMin"/>. 
        /// Defines the maximum quantizer that x264 can use. The default of 51 is the highest quantizer available for use in the H.264 spec, 
        /// and is extremely low quality. This default effectively disables QuantizerMax.
        /// You may want to set this lower (values in the 30-40 range are generally as low as you'd go) if you want to cap the minimum quality x264 can output,
        /// but adjusting it is generally not recommended.
        /// </summary>
        public int QuantizerMax { get; set; }

        /// <summary>
        /// Sets the maximum change in quantizer between two frames.
        /// </summary>
        public int QuantizerDelta { get; set; }

        /// <summary>
        /// Modifies the target average increase in quantizer for I-frames as compared to P-frames. 
        /// Higher values increase the quality of I-frames generated.
        /// </summary>
        public float QuantizerRatioIp { get; set; }

        /// <summary>
        /// Modifies the target average decrease in quantizer for B-frames as compared to P-frames. 
        /// Higher values decrease the quality of B-frames generated. 
        /// Not used with <see cref="UseMbTree"/> (enabled by default), which calculates the optimum value automatically.
        /// </summary>
        public float QuantizerRatioPb { get; set; }

        /// <summary>
        /// Set the size of the inter luma quantization deadzone. Deadzones should be in the range of 0 to 32. 
        /// The deadzone value sets the level of fine detail that x264 will arbitrarily drop without attempting to preserve. 
        /// Very fine detail is both hard to see and expensive to encode, 
        /// dropping this detail without attempting to preserve it stops wasting bits on such a low-return section of the video. 
        /// Deadzone is incompatible with Trellis.
        /// </summary>
        public int DeadZoneInter { get; set; }

        /// <summary>
        /// Set the size of the intra luma quantization deadzone. Deadzones should be in the range of 0 to 32. 
        /// The deadzone value sets the level of fine detail that x264 will arbitrarily drop without attempting to preserve. 
        /// Very fine detail is both hard to see and expensive to encode, 
        /// dropping this detail without attempting to preserve it stops wasting bits on such a low-return section of the video. 
        /// Deadzone is incompatible with Trellis.
        /// </summary>
        public int DeadZoneIntra { get; set; }

        /// <summary>
        /// Add an offset to the quantizer of chroma planes when encoding. The offset can be negative.
        /// When using psy options are enabled (psy-rd, psy-trellis), x264 automatically subtracts 2 from this value to compensate
        /// for these optimisations overly favouring luma detail by default.
        /// Note: x264 only encodes the luma and chroma planes at the same quantizer up to quantizer 29. 
        /// After this, chroma is progressively quantized by a lower amount than luma until you end with luma at q51 and chroma at q39. 
        /// This behavior is required by the H.264 standard.
        /// </summary>
        public int ChromaQpOffset { get; set; }

        /// <summary>
        /// Sets the size of the VBV buffer in kilobits.
        /// VBV reduces quality, so you should only use this if you're encoding for a playback scenario that requires it.
        /// </summary>
        public int VbvBufSize { get; set; }

        /// <summary>
        /// Sets the maximum rate the VBV buffer should be assumed to refill at.
        /// VBV reduces quality, so you should only use this if you're encoding for a playback scenario that requires it.
        /// </summary>
        public int VbvMaxRate { get; set; }

        /// <summary>
        /// Sets how full the VBV Buffer must be before playback starts.
        /// If it is less than 1, then the initial fill is: vbv-init * vbv-bufsize. Otherwise it is interpreted as the initial fill in kbits.
        /// </summary>
        public float VbvInitialBuffer { get; set; }

        /// <summary>
        /// This is a dual purpose parameter:
        /// In 1-pass bitrate encodes, this settings controls the percentage that x264 can miss the target average bitrate by. 
        /// You can set this to 'inf' to disable this overflow detection completely. The lowest you can set this is to 0.01. 
        /// The higher you set this to the better x264 can react to complex scenes near the end of the movie. 
        /// The unit of measure for this purpose is percent (eg, 1.0 = 1% bitrate deviation allowed).
        /// Many movies (any action movie, for instance) are most complex at the climatic finale. 
        /// As a 1pass encode doesn't know this, the number of bits required for the end is usually underestimated. 
        /// A ratetol of inf can mitigate this by allowing the encode to function more like a <see cref="EncodingMode"/> 4 (CRF) encode, but the filesize will blow out.
        /// When VBV is activated (ie, you're specified vbv* options), this setting also affects VBV aggressiveness. 
        /// Setting this higher allows VBV to fluctuate more at the risk of possibly violating the VBV settings. 
        /// For this purpose, the unit of measure is arbitrary.
        /// </summary>
        public float BitrateVariance { get; set; }

        /// <summary>
        /// Quantizer curve compression factor. 0.0 => Constant Bitrate, 1.0 => Constant Quantizer.
        /// When used with mbtree, it affects the strength of mbtree. (Higher QuantizerCompression = weaker mbtree).
        /// </summary>
        public float QuantizerCompression { get; set; }

        /// <summary>
        /// Apply a gaussian blur with the given radius to the quantizer curve. 
        /// This means that the quantizer assigned to each frame is blurred temporally with its neighbours to limit quantizer fluctuations.
        /// </summary>
        public int TempBlurFrameComplexity { get; set; }

        /// <summary>
        /// Apply a gaussian blur with the given radius to the quantizer curve, after curve compression. Not a very important setting.
        /// </summary>
        public float TempBlurQuant { get; set; }

        /// <summary>
        /// Adaptive Quantization Mode
        /// Default: 1
        /// Without AQ, x264 tends to underallocate bits to less-detailed sections. 
        /// AQ is used to better distribute the available bits between all macroblocks in the video. 
        /// This setting changes what scope AQ re-arranges bits in:
        /// 0: Do not use AQ at all.
        /// 1: Allow AQ to redistribute bits across the whole video and within frames.
        /// 2: Auto-variance AQ (experimental) which attempts to adapt strength per-frame.
        /// </summary>
        public int AdaptiveQuantizersMode { get; set; }

        /// <summary>
        /// Adaptive Quantization Strength
        /// Default: 1.0
        /// Sets the strength of AQ bias towards low detail ('flat') macroblocks. 
        /// Negative values are not allowed. Values outside the range 0.0 - 2.0 are probably a bad idea.
        /// </summary>
        public float AdaptiveQuantizersStrength { get; set; }

        /// <summary>
        /// Default: Flat (Not Set)
        /// Sets all custom quantization matrices to those of a built-in preset. The built-in presets are 'flat' (0) or JVT (1).
        /// Recommendation: Default
        /// </summary>
        public int QuantizerMatrix { get; set; }

        /// <summary>
        /// Sets the number of frames to use for mb-tree ratecontrol and vbv-lookahead. The maximum allowed value is 250.
        /// For the mb-tree portion of this, increasing the frame count generates better results but is also slower. 
        /// The maximum buffer value used by mb-tree is the MIN( NumFramesLookahead, <see cref="MaxGopSize"/> )
        /// For the vbv-lookahead portion of this, increasing the frame count generates better stability and accuracy when using vbv. 
        /// The maximum value used by vbv-lookahead is:
        /// MIN(NumFramesLookahead, MAX(<see cref="MaxGopSize"/>, MAX(<see cref="VbvMaxRate"/>, <see cref="VbrSetting"/>) / <see cref="VbvBufSize"/> * Fps))
        /// </summary>
        public int NumFramesLookahead { get; set; }

        /// <summary>
        /// Enable macroblock tree ratecontrol. 
        /// Using macroblock tree ratecontrol overall improves the compression by keeping track of temporal propagation 
        /// across frames and weighting accordingly. 
        /// Requires a new large statsfile in addition to the already existing for multipass encodes.
        /// </summary>
        public bool UseMbTree { get; set; }

        /// <summary>
        /// Enables chroma motion estimation.
        /// Normally, motion estimation works off both the luma and chroma planes.
        /// </summary>
        public bool UseChromaMotionEstimation { get; set; }

        /// <summary>
        /// MotionEstimationRange controls the max range of the motion search in pixels. 
        /// For hex and dia, the range is clamped to 4-16, with a default of 16. 
        /// For umh and esa, it can be increased beyond the default 16 to allow for a wider-range motion search, 
        /// which is useful on HD footage and for high-motion footage. 
        /// Note that for umh, esa, and tesa, increasing merange will significantly slow down encoding.
        /// Extremely high merange (e.g. >64) is unlikely to find any new motion vectors that are useful, 
        /// so it may very slightly decrease compression in some cases by picking motion vector deltas so large 
        /// that they even worsen prediction of future motion vectors in the rare cases they're locally useful, 
        /// making them worse than useless.
        /// The effect is so small as to be near-negligible, though, and you shouldn't be using such insane settings.
        /// Default: 16
        /// </summary>
        public int MotionEstimationRange { get; set; }

        /// <summary>
        /// Set the full-pixel motion estimation method. There are five choices:
        /// 0: dia (diamond) is the simplest search, consisting of starting at the best predictor, 
        ///   checking the motion vectors at one pixel upwards, left, down, and to the right, picking the best, 
        ///   and repeating the process until it no longer finds any better motion vector.
        /// 
        /// 1: hex (hexagon) consists of a similar strategy, except it uses a range-2 search of 6 surrounding points,
        ///   thus the name. It is considerably more efficient than dia and hardly any slower, and therefore makes a good choice for general-use encoding.
        /// 
        /// 2: umh (uneven multi-hex) is considerably slower than hex, but searches a complex multi-hexagon pattern in order to avoid missing harder-to-find motion vectors. 
        ///   Unlike hex and dia, the merange parameter directly controls umh's search radius, allowing one to increase or decrease the size of the wide search.
        /// 
        /// 3: esa (exhaustive) is a highly optimized intelligent search of the entire motion search space within merange of the best predictor. 
        ///   It is mathematically equivalent to the bruteforce method of searching every single motion vector in that area, though faster. 
        ///   However, it is still considerably slower than UMH, with not too much benefit, so is not particularly useful for everyday encoding.
        /// 
        /// 4: tesa (transformed exhaustive) is an algorithm which attempts to approximate the effect of running a Hadamard transform comparison at each motion vector; 
        ///   like exhaustive, but a little bit better and a little bit slower.
        /// </summary>
        public int MotionEstimationAlgorithm { get; set; }

        /// <summary>
        /// Set the subpixel estimation complexity. Higher numbers are better. Levels 1-5 simply control the subpixel refinement strength. 
        /// Level 6 enables RDO for mode decision, and level 8 enables RDO for motion vectors and intra prediction modes. 
        /// RDO levels are significantly slower than the previous levels.
        /// Using a value less than 2 will enable a faster, and lower quality lookahead mode, 
        /// as well as cause poorer <see cref="NumExtraIFrames"/> decisions to be made, and thus it is not recommended.
        /// Possible Values:
        /// 0. fullpel only
        /// 1. QPel SAD 1 iteration
        /// 2. QPel SATD 2 iterations
        /// 3. HPel on MB then QPel
        /// 4. Always QPel
        /// 5. Multi QPel + bi-directional motion estimation
        /// 6. RD on I/P frames
        /// 7. RD on all frames
        /// 8. RD refinement on I/P frames
        /// 9. RD refinement on all frames
        /// 10. QP-RD (requires <see cref="Trellis"/>=2, <see cref="AdaptiveQuantizersMode"/> > 0)
        /// 11. Full RD [1][2]
        /// Default: 7
        /// Recommendation: Default, or higher, unless speed is very important.
        /// </summary>
        public int SubPixelRefinement { get; set; }

        /// <summary>
        /// Set prediction mode for 'direct' motion vectors. There are two modes available: spatial (1) and temporal (2). 
        /// You can also select none (0) to disable direct MVs, and auto to allow x264 to swap between them as it sees fit. 
        /// If you set auto (3), x264 outputs information on the usage at the end of the encode. 
        /// 'auto' works best in a 2pass encode, but will work in single-pass encodes too. 
        /// In first-pass auto mode, x264 keeps a running average of how well each method has so far performed, 
        /// and picks the next prediction mode from that. 
        /// Note that you should only enable auto on the second pass if it was enabled on the first pass; 
        /// if it wasn't, the second pass will default to temporal. Direct none wastes bits and is strongly discouraged.
        /// </summary>
        public int MvPredictionMod { get; set; }

        /// <summary>
        /// Performs Trellis quantization to increase efficiency.
        /// 0. Disabled
        /// 1. Enabled only on the final encode of a macroblock
        /// 2. Enabled on all mode decisions
        /// On Macroblock provides a good compromise between speed and efficiency. On all decisions reduces speed further.
        /// See: http://en.wikipedia.org/wiki/Trellis_quantization
        /// Recommendation: Default
        /// Note: Requires <see cref="UseCabac"/>
        /// </summary>
        public int Trellis { get; set; }

        /// <summary>
        /// The strength of Psy-RDO to use (requires <see cref="SubPixelRefinement"/> >= 6 to activate). 
        /// Note that Trellis is still considered 'experimental', and almost certainly is a Bad Thing for at least cartoons.
        /// See this thread on doom9 for an explanation of psy-rd: http://forum.doom9.org/showthread.php?t=138293.
        /// Life is short, and this article saved valubale time on this Earth.
        /// </summary>
        public float PsyRdStrength { get; set; }

        /// <summary>
        /// The strength of Psy-Trellis (requires <see cref="Trellis"/> >= 1 to activate). 
        /// Note that Trellis is still considered 'experimental', and almost certainly is a Bad Thing for at least cartoons.
        /// See this thread on doom9 for an explanation of psy-rd: http://forum.doom9.org/showthread.php?t=138293.
        /// Life is short, and this article saved valubale time on this Earth.
        /// </summary>
        public float PsyTrellisStrength { get; set; }

        /// <summary>
        /// Mixed refs will select refs on a per-8x8 partition, rather than per-macroblock basis. 
        /// This improves quality when using multiple reference frames, albeit at some speed cost. 
        /// Setting this option will disable it.
        /// </summary>
        public bool UseNoMixedReferenceFrames { get; set; }

        /// <summary>
        /// DCT Decimation will drop DCT blocks it deems "unnecessary". 
        /// This will improve coding efficiency, with a usually negligible loss in quality. 
        /// Setting this option will disable it.
        /// </summary>
        public bool UseNoDctDecimation { get; set; }

        /// <summary>
        /// Disables early skip detection on P-frames. 
        /// At low bitrates, provides a moderate quality increase for a large speed cost. 
        /// At high bitrates, has negligible effect on both speed and quality.
        /// </summary>
        public bool UseNoFastPSkip { get; set; }

        /// <summary>
        /// Performs fast noise reduction. 
        /// Estimates film noise based on this value and attempts to remove it by dropping small details before quantization. 
        /// This may not match the quality of a good external noise reduction filter, but it performs very fast.
        /// </summary>
        public int NoiseReduction { get; set; }

        /// <summary>
        /// Adaptive 8x8 DCT enables the intelligent adaptive use of 8x8 transforms in I-frames. 
        /// Setting this to false disables the feature.
        /// </summary>
        public bool MacroBlocksPartitionsAdaptiveDct { get; set; }

        /// <summary>
        /// H.264 video is split up into 16x16 macroblocks during compression. 
        /// These blocks can be further split up into smaller partitions, which is what this option controls.
        /// With this option, you enable 4x4 partitions for I-frames. 
        /// </summary>
        public bool MacroBlocksPartitionsI4X4 { get; set; }

        /// <summary>
        /// H.264 video is split up into 16x16 macroblocks during compression. 
        /// These blocks can be further split up into smaller partitions, which is what this option controls.
        /// With this option, you enable 4x4 partitions for P-frames. 
        /// p4x4 is generally not very useful and has an extremely high ratio of speed cost to resulting quality gain.
        /// </summary>
        public bool MacroBlocksPartitionsP4X4 { get; set; }

        /// <summary>
        /// H.264 video is split up into 16x16 macroblocks during compression. 
        /// These blocks can be further split up into smaller partitions, which is what this option controls.
        /// With this option, you enable 8x8 partitions for I-frames. 
        /// </summary>
        public bool MacroBlocksPartitionsI8X8 { get; set; }

        /// <summary>
        /// H.264 video is split up into 16x16 macroblocks during compression. 
        /// These blocks can be further split up into smaller partitions, which is what this option controls.
        /// With this option, you enable 8x8 partitions for P-frames. 
        /// </summary>
        public bool MacroBlocksPartitionsP8X8 { get; set; }

        /// <summary>
        /// H.264 video is split up into 16x16 macroblocks during compression. 
        /// These blocks can be further split up into smaller partitions, which is what this option controls.
        /// With this option, you enable 8x8 partitions for B-frames. 
        /// </summary>
        public bool MacroBlocksPartitionsB8X8 { get; set; }

        /// <summary>
        /// Signal HRD information. Required for Blu-ray streams, television broadcast and a few other specialist areas. 
        /// Acceptable values are:
        /// 
        /// none (0): Specify no HRD information
        /// vbr (1):  Specify HRD information
        /// cbr (2):  Specify HRD information and pack the bitstream to the bitrate specified by bitrate. Requires bitrate mode ratecontrol.
        /// 
        /// Recommendation: none, unless you need to signal this information.
        /// </summary>
        public int HrdInfo { get; set; }

        /// <summary>
        /// Use access unit delimiters.
        /// Default: Not Set
        /// Recommendation: Default, unless encoding for Blu-ray, in which case set this option.
        /// </summary>
        public bool UseAccessUnitDelimiters { get; set; }

        /// <summary>
        /// Mark a stream as interlaced even when not encoding as interlaced. Allows encoding of 25p and 30p Blu-ray compliant videos.
        /// Default: Not Set
        /// </summary>
        public bool UseFakeInterlaced { get; set; }

        /// <summary>
        /// Modify x264's options to ensure better compatibility with all Blu-Ray players. 
        /// Only neccessary if your video will be played by Blu-Ray hardware players.
        /// This setting makes some option changes:
        /// Cap <see cref="PFrameWeightedPrediction"/> at 1
        /// Set <see cref="MinGopSize"/> to 1
        /// Disable --intra-refresh
        /// etc...
        /// It also enables some internal x264 hacks to produce more hardware-player-friendly streams. For example:
        /// GOP/mini-GOP tweaks to size and reference lists.
        /// More verbose slice headers
        /// 
        /// Default: Not set
        /// Recommendation: Set if you're encoding for hardware Blu-Ray players.
        /// </summary>
        public bool UseBluRayCompatibility { get; set; }

        /// <summary>
        /// Indicates whether the output range of luma and chroma levels should be limited or full. 
        /// If set to TV (1), the limited ranges will be used. If set to auto (0), use the same range as input.
        /// NOTE: If range and --input-range differ, then a range conversion will occur!
        /// See this page for a simple description.
        /// 
        /// Default: auto (0)
        /// Recommendation: Default.
        /// </summary>
        public int VuiRange { get; set; }

        /// <summary>
        /// Set what color primaries for converting to RGB.
        /// Will be ignored if <see cref="UseAutoSelectColorSettings"/> is enabled, 
        /// and calculated based on resolution instead.
        /// Possible Values:
        /// undef (0)
        /// bt709 (1)
        /// bt470m (2)
        /// bt470bg (3)
        /// smpte170m (4)
        /// smpte240m (5)
        /// film (6)
        /// 
        /// Default: undef
        /// Recommendation: Default, unless you know what your source uses.
        /// </summary>
        public int ColorPrimaries { get; set; }

        /// <summary>
        /// Set the opto-electronic transfer characteristics to use. (Sets the gamma curve to use for correction.)
        /// Will be ignored if <see cref="UseAutoSelectColorSettings"/> is enabled, 
        /// and calculated based on resolution instead.
        /// Possible values:
        /// undef (0)
        /// bt709 (1)
        /// bt470m (2)
        /// bt470bg (3)
        /// linear (4)
        /// log100 (5)
        /// log316 (6)
        /// smpte170m (7)
        /// smpte240m (8)
        /// 
        /// Default: undef
        /// Recommendation: Default, unless you know what your source uses.
        /// </summary>
        public int Transfer { get; set; }

        /// <summary>
        /// Set the matrix coefficients used in deriving the luma and chroma from the RGB primaries.
        /// Will be ignored if <see cref="UseAutoSelectColorSettings"/> is enabled, 
        /// and calculated based on resolution instead.
        /// Possible values:
        /// undef (0)
        /// bt709 (1)
        /// fcc (2)
        /// bt470bg (3)
        /// smpte170m (4)
        /// smpte240m (5)
        /// GBR (6)
        /// YCgCo (7)
        /// 
        /// Default: undef
        /// Recommendation: Whatever your sources uses, or default.
        /// </summary>
        public int ColorMatrix { get; set; }

        /// <summary>
        /// calculates <see cref="ColorPrimaries"/>, <see cref="Transfer"/> and <see cref="ColorMatrix"/> based on output resolution.
        /// </summary>
        public bool UseAutoSelectColorSettings { get; set; }

        /// <summary>
        /// Enables PSNR calculations (http://en.wikipedia.org/wiki/Peak_signal-to-noise_ratio)
        /// that are reported on completion at the cost of a small decrease in speed.
        /// </summary>
        public bool UsePsnrCalculation { get; set; }

        /// <summary>
        /// Enables SSIM calculations (http://en.wikipedia.org/wiki/SSIM)
        /// that are reported on completion at the cost of a small decrease in speed.
        /// </summary>
        public bool UseSsimCalculation { get; set; }

        /// <summary>
        /// Specifies the input video's Sample Aspect Ratio (SAR) to be used by the encoder in width:height.
        /// This in conjunction with frame dimensions can be used to encode an anamorphic output by determining the 
        /// Display Aspect Ratio (DAR) via the formula: DAR = SAR x width/height
        /// 
        /// Default: Not Set
        /// </summary>
        public int ForceSar { get; set; }

        /// <summary>
        /// Calculate SAR based on input resolution
        /// </summary>
        public bool UseAutoSelectSar { get; set; }

        /// <summary>
        /// Enables parallel encoding by using more than 1 thread to increase speed on multi-core systems. 
        /// The quality loss from multiple threads is mostly negligible unless using very high numbers of threads (say, above 16).
        /// The speed gain should be slightly less than linear until you start using more than 1 thread per 40px of vertical video,
        /// at which point the gain from additional threads sharply decreases.
        /// x264 currently has an internal limit on the number of threads set at 128, realistically you should never set it this high.
        /// 
        /// Default: auto (frame based threads: 1.5 * logical processors, rounded down; slice based threads: 1 * logical processors)
        /// </summary>
        public int NumThreads { get; set; }

        /// <summary>
        /// Decodes the input video in a separate thread to the encoding process.
        /// 
        /// Default: Set if threads > 1.
        /// Recommendation: Default.
        /// </summary>
        public bool UseThreadInput { get; set; }

        /// <summary>
        /// Slightly improve quality when encoding with <see cref="NumThreads"/> > 1, at the cost of non-deterministic
        /// output encodes. This enables multi-threaded mv and uses the entire lookahead buffer in slicetype decisions
        /// when slicetype is threaded -- rather than just the minimum amount known to be available.
        /// Not for general use.
        /// </summary>
        public bool UseNonDeterministic { get; set; }

        /// <summary>
        /// Using --pass 1 applies the following settings at the end of parsing the command line:
        /// <see cref="NumRefFrames"/> 1
        /// <see cref="MacroBlocksPartitionsAdaptiveDct"/> = false
        /// <see cref="MacroBlocksPartitionsI4X4"/> (if originally enabled, else none)
        /// <see cref="MotionEstimationAlgorithm"/> dia
        /// <see cref="SubPixelRefinement"/> MIN( 2, <see cref="MotionEstimationAlgorithm"/> )
        /// <see cref="Trellis"/> 0
        /// 
        /// You can set UseSlowFirstPass to disable this.
        /// Note: <see cref="Preset"/> placebo enables UseSlowFirstPass.
        /// </summary>
        public bool UseSlowFirstPass { get; set; }

        /// <summary>
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
            QuantizerRatioIp = 1.4f;
            QuantizerRatioPb = 1.3f;
            DeadZoneInter = 21;
            DeadZoneIntra = 11;
            ChromaQpOffset = 0;
            VbvBufSize = 0;
            VbvMaxRate = 0;
            VbvInitialBuffer = 0.9f;
            BitrateVariance = 1.0f;
            QuantizerCompression = 0.6f;
            TempBlurFrameComplexity = 20;
            TempBlurQuant = 0.5f;
            AdaptiveQuantizersMode = 1;
            AdaptiveQuantizersStrength = 1.0f;
            QuantizerMatrix = 0;
            NumFramesLookahead = 40;
            UseMbTree = true;
            UseChromaMotionEstimation = true;
            MotionEstimationRange = 16;
            MotionEstimationAlgorithm = 1;
            SubPixelRefinement = 7;
            MvPredictionMod = 1;
            Trellis = 1;
            PsyRdStrength = 1.0f;
            PsyTrellisStrength = 0.0f;
            UseNoMixedReferenceFrames = false;
            UseNoDctDecimation = false;
            UseNoFastPSkip = false;
            NoiseReduction = 0;
            MacroBlocksPartitionsAdaptiveDct = true;
            MacroBlocksPartitionsI4X4 = true;
            MacroBlocksPartitionsP4X4 = false;
            MacroBlocksPartitionsI8X8 = true;
            MacroBlocksPartitionsP8X8 = true;
            MacroBlocksPartitionsB8X8 = true;
            HrdInfo = 0;
            UseAccessUnitDelimiters = false;
            UseFakeInterlaced = false;
            UseBluRayCompatibility = false;
            VuiRange = 0;
            ColorPrimaries = 0;
            Transfer = 0;
            ColorMatrix = 0;
            UseAutoSelectColorSettings = true;
            UsePsnrCalculation = false;
            UseSsimCalculation = false;
            ForceSar = 0;
            UseAutoSelectSar = true;
            NumThreads = 0;
            UseThreadInput = true;
            UseNonDeterministic = false;
            UseSlowFirstPass = false;
            UseForcePicStruct = false;
            CustomCommandLine = string.Empty;
        }
    }
}
