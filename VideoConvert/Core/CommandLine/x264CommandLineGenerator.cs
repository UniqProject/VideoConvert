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

using System;
using System.Text;
using System.Text.RegularExpressions;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;
using VideoConvert.Core.Video.x264;

namespace VideoConvert.Core.CommandLine
{
    class X264CommandLineGenerator
    {
        private static readonly string[] CLILevelNames =
        {
            "1", "1.1", "1.2", "1.3", "2", "2.1", "2.2", "3", "3.1",
            "3.2", "4", "4.1", "4.2", "5", "5.1"
        };
        /// <summary>
        /// Generates commandline arguments used for encoding an video stream to h.264 format.
        /// Input is either stdin or file/avisynth script.
        /// </summary>
        /// <param name="inProfile">Encoding profile</param>
        /// <param name="bitrate">Target bitrate</param>
        /// <param name="hRes">Video width</param>
        /// <param name="vRes">Video height</param>
        /// <param name="pass">Encoding pass</param>
        /// <param name="fpsN">Framerate numerator</param>
        /// <param name="fpsD">Framerate denominator</param>
        /// <param name="stereo">Defines, which stereo encoding mode should be used</param>
        /// <param name="format">Image format</param>
        /// <param name="inFile">Path to input file</param>
        /// <param name="outFile">Path to output file</param>
        /// <returns>Commandline arguments</returns>
        public static string Generate(X264Profile inProfile, int bitrate, int hRes, int vRes, int pass, int fpsN, int fpsD, StereoEncoding stereo = StereoEncoding.None,
                                      VideoFormat format = VideoFormat.Unknown, string inFile = "input", string outFile = "output")
        {
            StringBuilder sb = new StringBuilder();
            if (inProfile != null)
            {
                bool display;
                X264Device device = X264Device.CreateDeviceList()[inProfile.TuneDevice];

                // AVC Profiles
                switch (inProfile.AVCProfile)
                {
                    case 0:
                        sb.Append("--profile baseline ");
                        break;
                    case 1:
                        sb.Append("--profile main ");
                        break;
                    default:
                        sb.Append("--profile high ");
                        break;
                }

                // AVC Levels
                if (inProfile.AVCLevel != 15) // unrestricted
                    sb.AppendFormat("--level {0} ", CLILevelNames[inProfile.AVCLevel]);

                // Blu-Ray compatibility
                if (inProfile.UseBluRayCompatibility)
                    sb.Append("--bluray-compat ");

                // x264 Presets
                if (!inProfile.CustomCommandLine.Contains("--preset"))
                {
                    switch (inProfile.Preset)
                    {
                        case 0: sb.Append("--preset ultrafast "); break;
                        case 1: sb.Append("--preset superfast "); break;
                        case 2: sb.Append("--preset veryfast "); break;
                        case 3: sb.Append("--preset faster "); break;
                        case 4: sb.Append("--preset fast "); break;
                        //case 5: sb.Append("--preset medium "); break; // default value
                        case 6: sb.Append("--preset slow "); break;
                        case 7: sb.Append("--preset slower "); break;
                        case 8: sb.Append("--preset veryslow "); break;
                        case 9: sb.Append("--preset placebo "); break;
                    }
                }

                // x264 Tunings
                if (!inProfile.CustomCommandLine.Contains("--tune"))
                {
                    switch (inProfile.Tuning)
                    {
                        case 1: sb.Append("--tune film "); break;
                        case 2: sb.Append("--tune animation "); break;
                        case 3: sb.Append("--tune grain "); break;
                        case 4: sb.Append("--tune psnr "); break;
                        case 5: sb.Append("--tune ssim "); break;
                        case 6: sb.Append("--tune fastdecode "); break;
                    }
                }

                // Encoding Modes
                int tempPass = pass;

                int tempBitrate = bitrate;
                int vbvBuf = GetVBVMaxrate(inProfile, device);

                if (tempBitrate <= 0)
                    tempBitrate = inProfile.VBRSetting;

                if (vbvBuf > 0 && tempBitrate > vbvBuf)   // limit Bitrate to max vbvbuf size
                    tempBitrate = vbvBuf;

                switch (inProfile.EncodingMode)
                {
                    case 0: // ABR
                        if (!inProfile.CustomCommandLine.Contains("--bitrate"))
                            sb.AppendFormat(AppSettings.CInfo, "--bitrate {0:0} ", tempBitrate);
                        break;
                    case 1: // Constant Quantizer
                        if (!inProfile.CustomCommandLine.Contains("--qp"))
                            sb.AppendFormat(AppSettings.CInfo, "--qp {0:0}", inProfile.QuantizerSetting);
                        break;
                    case 2: // automated 2 pass
                    case 3: // automated 3 pass
                        sb.AppendFormat(AppSettings.CInfo, "--pass {0:0} --bitrate {1:0} ", tempPass, tempBitrate);
                        break;
                    default:
                        if (!inProfile.CustomCommandLine.Contains("--crf") && inProfile.QualitySetting != 23)
                            sb.AppendFormat(AppSettings.CInfo, "--crf {0:0} ", inProfile.QualitySetting);
                        break;
                }

                // Slow 1st Pass
                if (!inProfile.CustomCommandLine.Contains("--slow-firstpass"))
                    if ((inProfile.UseSlowFirstPass) && inProfile.Preset < 9 && // 9 = placebo
                       ((inProfile.EncodingMode == 2) || // automated twopass
                        (inProfile.EncodingMode == 3)))  // automated threepass
                        sb.Append("--slow-firstpass ");

                // Threads
                if (!inProfile.CustomCommandLine.Contains("--thread-input"))
                    if (inProfile.UseThreadInput && inProfile.NumThreads == 1)
                        sb.Append("--thread-input ");
                if (!inProfile.CustomCommandLine.Contains("--threads"))
                    if (inProfile.NumThreads > 0)
                        sb.AppendFormat(AppSettings.CInfo, "--threads {0:0} ", inProfile.NumThreads);

                #region frame-type tab

                // H.264 Features
                if (inProfile.UseDeblocking)
                {
                    display = false;
                    switch (inProfile.Tuning)
                    {
                        case 1: if (inProfile.DeblockingStrength != -1 || inProfile.DeblockingThreshold != -1) display = true; break; // film
                        case 2: if (inProfile.DeblockingStrength != 1 || inProfile.DeblockingThreshold != 1) display = true; break; // animation
                        case 3: if (inProfile.DeblockingStrength != -2 || inProfile.DeblockingThreshold != -2) display = true; break; // grain
                        default: if (inProfile.DeblockingStrength != 0 || inProfile.DeblockingThreshold != 0) display = true;
                            break;
                    }

                    if (!inProfile.CustomCommandLine.Contains("--deblock "))
                        if (display)
                            sb.AppendFormat(AppSettings.CInfo, "--deblock {0:0}:{1:0} ", inProfile.DeblockingStrength,
                                            inProfile.DeblockingThreshold);
                }
                else
                {
                    if (!inProfile.CustomCommandLine.Contains("--no-deblock"))
                        if (inProfile.Preset != 0 && inProfile.Tuning != 7) // ultrafast preset and not fast decode tuning
                            sb.Append("--no-deblock ");
                }

                if (inProfile.AVCProfile > 0 && !inProfile.CustomCommandLine.Contains("--no-cabac"))
                {
                    if (!inProfile.UseCabac)
                    {
                        if (inProfile.Preset != 0 && inProfile.Tuning != 7) // ultrafast preset and not fast decode tuning
                            sb.Append("--no-cabac ");
                    }
                }

                // GOP Size
                int backupMaxGopSize = inProfile.MaxGopSize;
                int backupMinGopSize = inProfile.MinGopSize;

                inProfile.MaxGopSize = GetKeyInt(fpsN, fpsD, backupMaxGopSize, device, inProfile.GopCalculation);

                if (inProfile.MaxGopSize != 250) // default size
                {
                    if (inProfile.MaxGopSize == 0)
                        sb.Append("--keyint infinite ");
                    else
                        sb.AppendFormat(AppSettings.CInfo, "--keyint {0:0} ", inProfile.MaxGopSize);
                }

                if (!inProfile.UseBluRayCompatibility)
                {
                    inProfile.MinGopSize = GetMinKeyInt(fpsN, fpsD, backupMinGopSize, inProfile.MaxGopSize, device,
                                                        inProfile.GopCalculation);
                    if (inProfile.MinGopSize > (inProfile.MaxGopSize / 2 + 1))
                    {
                        inProfile.MinGopSize = inProfile.MaxGopSize / 2 + 1;
                    }
                    int Default = Math.Min(inProfile.MaxGopSize / 10, fpsN / fpsD);

                    if (inProfile.MinGopSize != Default) // (MIN(--keyint / 10,--fps)) is default
                        sb.AppendFormat(AppSettings.CInfo, "--min-keyint {0:0} ", inProfile.MinGopSize);
                }

                inProfile.MaxGopSize = backupMaxGopSize;
                inProfile.MinGopSize = backupMinGopSize;

                if (!inProfile.CustomCommandLine.Contains("--open-gop") && (inProfile.UseOpenGop || inProfile.UseBluRayCompatibility))
                    sb.Append("--open-gop ");

                // B-Frames
                inProfile.NumBFrames = GetBFrames(inProfile, device);
                if (inProfile.AVCProfile > 0 && inProfile.NumBFrames != X264Settings.GetDefaultNumberOfBFrames(inProfile.AVCLevel, inProfile.Tuning, inProfile.AVCProfile, device))
                    sb.AppendFormat(AppSettings.CInfo, "--bframes {0:0} ", inProfile.NumBFrames);

                if (inProfile.NumBFrames > 0)
                {
                    if (!inProfile.CustomCommandLine.Contains("--b-adapt"))
                    {
                        display = false;
                        if (inProfile.Preset > 5) // medium
                        {
                            if (inProfile.AdaptiveBFrames != 2)
                                display = true;
                        }
                        else if (inProfile.Preset > 0) // ultrafast
                        {
                            if (inProfile.AdaptiveBFrames != 1)
                                display = true;
                        }
                        else
                        {
                            if (inProfile.AdaptiveBFrames != 0)
                                display = true;
                        }
                        if (display)
                            sb.AppendFormat(AppSettings.CInfo, "--b-adapt {0:0} ", inProfile.AdaptiveBFrames);
                    }

                    inProfile.BPyramid = GetBPyramid(inProfile, device);
                    if (inProfile.NumBFrames > 1 && (inProfile.BPyramid != 2 && !inProfile.UseBluRayCompatibility || inProfile.BPyramid != 1 && inProfile.UseBluRayCompatibility))
                    {
                        switch (inProfile.BPyramid) // pyramid needs a minimum of 2 b frames
                        {
                            case 1: sb.Append("--b-pyramid strict "); break;
                            case 0: sb.Append("--b-pyramid none "); break;
                        }
                    }

                    if (!inProfile.CustomCommandLine.Contains("--no-weightb"))
                        if (!inProfile.UseWeightedPred && inProfile.Tuning != 7 && inProfile.Preset != 0) // no weightpredb + tuning != fastdecode + preset != ultrafast
                            sb.Append("--no-weightb ");
                }

                // B-Frames bias
                if (!inProfile.CustomCommandLine.Contains("--b-bias "))
                    if (inProfile.BFrameBias != 0)
                        sb.AppendFormat(AppSettings.CInfo, "--b-bias {0:0} ", inProfile.BFrameBias);


                // Other
                if (inProfile.UseAdaptiveIFrameDecision)
                {
                    if (!inProfile.CustomCommandLine.Contains("--scenecut "))
                        if (inProfile.NumExtraIFrames != 40 && inProfile.Preset != 0 ||
                            inProfile.NumExtraIFrames != 0 && inProfile.Preset == 0)
                            sb.AppendFormat(AppSettings.CInfo, "--scenecut {0:0} ", inProfile.NumExtraIFrames);
                }
                else
                {
                    if (!inProfile.CustomCommandLine.Contains("--no-scenecut"))
                        if (inProfile.Preset != 0)
                            sb.Append("--no-scenecut ");
                }


                // reference frames
                int iRefFrames = GetRefFrames(hRes, vRes, inProfile, device);
                if (iRefFrames != X264Settings.GetDefaultNumberOfRefFrames(inProfile.Preset, inProfile.Tuning, null, inProfile.AVCLevel, hRes, vRes))
                    sb.AppendFormat(AppSettings.CInfo, "--ref {0:0} ", iRefFrames);

                // WeightedPPrediction
                inProfile.PFrameWeightedPrediction = GetWeightp(inProfile, device);
                if (inProfile.PFrameWeightedPrediction != X264Settings.GetDefaultNumberOfWeightp(inProfile.Preset,
                                                                                                 inProfile.Tuning,
                                                                                                 inProfile.AVCProfile,
                                                                                                 inProfile.UseBluRayCompatibility))
                    sb.AppendFormat(AppSettings.CInfo, "--weightp {0:0} ", inProfile.PFrameWeightedPrediction);

                // Slicing
                inProfile.NumSlices = GetSlices(inProfile, device);
                if (inProfile.NumSlices != 0)
                    sb.AppendFormat(AppSettings.CInfo, "--slices {0:0} ", inProfile.NumSlices);

                if (!inProfile.CustomCommandLine.Contains("--slice-max-size "))
                    if (inProfile.MaxSliceSizeBytes != 0)
                        sb.AppendFormat(AppSettings.CInfo, "--slice-max-size {0:0} ", inProfile.MaxSliceSizeBytes);

                if (!inProfile.CustomCommandLine.Contains("--slice-max-mbs "))
                    if (inProfile.MaxSliceSizeBlocks != 0)
                        sb.AppendFormat(AppSettings.CInfo, "--slice-max-mbs {0:0} ", inProfile.MaxSliceSizeBlocks);

                #endregion

                #region rc tab

                if (!inProfile.CustomCommandLine.Contains("--qpmin"))
                    if (inProfile.QuantizerMin != 0)
                        sb.AppendFormat(AppSettings.CInfo, "--qpmin {0:0} ", inProfile.QuantizerMin);

                if (!inProfile.CustomCommandLine.Contains("--qpmax"))
                    if (inProfile.QuantizerMax != 69)
                        sb.AppendFormat(AppSettings.CInfo, "--qpmax {0:0} ", inProfile.QuantizerMax);

                if (!inProfile.CustomCommandLine.Contains("--qpstep"))
                    if (inProfile.QuantizerDelta != 4)
                        sb.AppendFormat(AppSettings.CInfo, "--qpstep {0:0} ", inProfile.QuantizerDelta);

                if (Math.Abs(inProfile.QuantizerRatioIP - 1.4F) > 0)
                {
                    display = true;
                    if (inProfile.Tuning == 3 && Math.Abs(inProfile.QuantizerRatioIP - 1.1F) <= 0)
                        display = false;

                    if (!inProfile.CustomCommandLine.Contains("--ipratio"))
                        if (display)
                            sb.AppendFormat(AppSettings.CInfo, "--ipratio {0:0} ", inProfile.QuantizerRatioIP);
                }

                if (Math.Abs(inProfile.QuantizerRatioPB - 1.3F) > 0)
                {
                    display = true;
                    if (inProfile.Tuning == 3 && Math.Abs(inProfile.QuantizerRatioPB - 1.1F) <= 0)
                        display = false;

                    if (!inProfile.CustomCommandLine.Contains("--pbratio"))
                        if (display)
                            sb.AppendFormat(AppSettings.CInfo, "--pbratio {0:0} ", inProfile.QuantizerRatioPB);
                }

                if (!inProfile.CustomCommandLine.Contains("--chroma-qp-offset"))
                    if (inProfile.ChromaQPOffset != 0)
                        sb.AppendFormat(AppSettings.CInfo, "--chroma-qp-offset {0:0} ", inProfile.ChromaQPOffset);

                if (inProfile.EncodingMode != 1) // doesn't apply to CQ mode
                {
                    inProfile.VBVBufSize = GetVBVBufsize(inProfile, device);
                    if (inProfile.VBVBufSize > 0)
                        sb.AppendFormat(AppSettings.CInfo, "--vbv-bufsize {0:0} ", inProfile.VBVBufSize);

                    inProfile.VBVMaxRate = GetVBVMaxrate(inProfile, device);
                    if (inProfile.VBVMaxRate > 0)
                        sb.AppendFormat(AppSettings.CInfo, "--vbv-maxrate {0:0} ", inProfile.VBVMaxRate);

                    if (!inProfile.CustomCommandLine.Contains("--vbv-init"))
                        if (Math.Abs(inProfile.VBVInitialBuffer - 0.9F) > 0)
                            sb.AppendFormat(AppSettings.CInfo, "--vbv-init {0:0.0} ", inProfile.VBVInitialBuffer);

                    if (!inProfile.CustomCommandLine.Contains("--ratetol"))
                        if (Math.Abs(inProfile.BitrateVariance - 1.0F) > 0)
                            sb.AppendFormat(AppSettings.CInfo, "--ratetol {0:0.0} ", inProfile.BitrateVariance);

                    if (!inProfile.CustomCommandLine.Contains("--qcomp"))
                    {
                        display = true;
                        if ((inProfile.Tuning == 3 && Math.Abs(inProfile.QuantizerCompression - 0.8F) <= 0) || (inProfile.Tuning != 3 && Math.Abs(inProfile.QuantizerCompression - 0.6F) <= 0))
                            display = false;
                        if (display)
                            sb.AppendFormat(AppSettings.CInfo, "--qcomp {0:0.0} ", inProfile.QuantizerCompression);
                    }

                    if (inProfile.EncodingMode > 1) // applies only to twopass
                    {
                        if (!inProfile.CustomCommandLine.Contains("--cplxblur"))
                            if (inProfile.TempBlurFrameComplexity != 20)
                                sb.AppendFormat(AppSettings.CInfo, "--cplxblur {0:0} ", inProfile.TempBlurFrameComplexity);

                        if (!inProfile.CustomCommandLine.Contains("--qblur"))
                            if (Math.Abs(inProfile.TempBlurQuant - 0.5F) > 0)
                                sb.AppendFormat(AppSettings.CInfo, "--qblur {0:0.0} ", inProfile.TempBlurQuant);
                    }
                }

                // Dead Zones
                if (!inProfile.CustomCommandLine.Contains("--deadzone-inter"))
                {
                    display = true;
                    if ((inProfile.Tuning != 3 && inProfile.DeadZoneInter == 21 && inProfile.DeadZoneIntra == 11) ||
                        (inProfile.Tuning == 3 && inProfile.DeadZoneInter == 6 && inProfile.DeadZoneIntra == 6))
                        display = false;
                    if (display)
                        sb.AppendFormat(AppSettings.CInfo, "--deadzone-inter {0:0} ", inProfile.DeadZoneInter);
                }

                if (!inProfile.CustomCommandLine.Contains("--deadzone-intra"))
                {
                    display = true;
                    if ((inProfile.Tuning != 3 && inProfile.DeadZoneIntra == 11) || (inProfile.Tuning == 3 && inProfile.DeadZoneIntra == 6))
                        display = false;
                    if (display)
                        sb.AppendFormat(AppSettings.CInfo, "--deadzone-intra {0:0} ", inProfile.DeadZoneIntra);
                }

                // Disable Macroblok Tree
                if (!inProfile.UseMBTree)
                {
                    if (!inProfile.CustomCommandLine.Contains("--no-mbtree"))
                        if (inProfile.Preset > 0) // preset veryfast
                            sb.Append("--no-mbtree ");
                }
                else
                {
                    // RC Lookahead
                    if (!inProfile.CustomCommandLine.Contains("--rc-lookahead"))
                    {
                        display = false;
                        switch (inProfile.Preset)
                        {
                            case 0:
                            case 1: if (inProfile.NumFramesLookahead != 0) display = true; break;
                            case 2: if (inProfile.NumFramesLookahead != 10) display = true; break;
                            case 3: if (inProfile.NumFramesLookahead != 20) display = true; break;
                            case 4: if (inProfile.NumFramesLookahead != 30) display = true; break;
                            case 5: if (inProfile.NumFramesLookahead != 40) display = true; break;
                            case 6: if (inProfile.NumFramesLookahead != 50) display = true; break;
                            case 7:
                            case 8:
                            case 9: if (inProfile.NumFramesLookahead != 60) display = true; break;
                        }
                        if (display)
                            sb.AppendFormat("--rc-lookahead {0:0} ", inProfile.NumFramesLookahead);
                    }
                }

                // AQ-Mode
                if (inProfile.EncodingMode != 1)
                {
                    if (!inProfile.CustomCommandLine.Contains("--aq-mode"))
                    {
                        if (inProfile.AdaptiveQuantizersMode != X264Settings.GetDefaultAQMode(inProfile.Preset, inProfile.Tuning))
                            sb.AppendFormat("--aq-mode {0:0} ", inProfile.AdaptiveQuantizersMode);
                    }

                    if (inProfile.AdaptiveQuantizersMode > 0)
                    {
                        display = false;
                        switch (inProfile.Tuning)
                        {
                            case 2: if (Math.Abs(inProfile.AdaptiveQuantizersStrength - 0.6F) > 0) display = true; break;
                            case 3: if (Math.Abs(inProfile.AdaptiveQuantizersStrength - 0.5F) > 0) display = true; break;
                            case 7: if (Math.Abs(inProfile.AdaptiveQuantizersStrength - 1.3F) > 0) display = true; break;
                            default: if (Math.Abs(inProfile.AdaptiveQuantizersStrength - 1.0F) > 0) display = true; break;
                        }
                        if (!inProfile.CustomCommandLine.Contains("--aq-strength"))
                            if (display)
                                sb.AppendFormat(AppSettings.CInfo, "--aq-strength {0:0.0} ", inProfile.AdaptiveQuantizersStrength);
                    }
                }

                // custom matrices 
                if (inProfile.AVCProfile > 1 && inProfile.QuantizerMatrix > 0)
                {
                    switch (inProfile.QuantizerMatrix)
                    {
                        case 1: if (!inProfile.CustomCommandLine.Contains("--cqm")) sb.Append("--cqm \"jvt\" "); break;
                    }
                }
                #endregion

                #region analysis tab

                // Disable Chroma Motion Estimation
                if (!inProfile.CustomCommandLine.Contains("--no-chroma-me"))
                    if (!inProfile.UseChromaMotionEstimation)
                        sb.Append("--no-chroma-me ");

                // Motion Estimation Range
                if (!inProfile.CustomCommandLine.Contains("--merange"))
                {
                    if ((inProfile.Preset <= 7 && inProfile.MotionEstimationRange != 16) ||
                        (inProfile.Preset >= 8 && inProfile.MotionEstimationRange != 24))
                        sb.AppendFormat("--merange {0:0} ", inProfile.MotionEstimationRange);
                }

                // ME Type
                if (!inProfile.CustomCommandLine.Contains("--me "))
                {
                    display = false;
                    switch (inProfile.Preset)
                    {
                        case 0:
                        case 1: if (inProfile.MotionEstimationAlgorithm != 0) display = true; break;
                        case 2:
                        case 3:
                        case 4:
                        case 5: if (inProfile.MotionEstimationAlgorithm != 1) display = true; break;
                        case 6:
                        case 7:
                        case 8: if (inProfile.MotionEstimationAlgorithm != 2) display = true; break;
                        case 9: if (inProfile.MotionEstimationAlgorithm != 4) display = true; break;
                    }

                    if (display)
                    {
                        switch (inProfile.MotionEstimationAlgorithm)
                        {
                            case 0: sb.Append("--me dia "); break;
                            case 1: sb.Append("--me hex "); break;
                            case 2: sb.Append("--me umh "); break;
                            case 3: sb.Append("--me esa "); break;
                            case 4: sb.Append("--me tesa "); break;
                        }
                    }

                }

                if (!inProfile.CustomCommandLine.Contains("--direct "))
                {
                    display = false;
                    if (inProfile.Preset > 5) // preset medium
                    {
                        if (inProfile.MVPredictionMod != 3)
                            display = true;
                    }
                    else if (inProfile.MVPredictionMod != 1)
                        display = true;

                    if (display)
                    {
                        switch (inProfile.MVPredictionMod)
                        {
                            case 0: sb.Append("--direct none "); break;
                            case 1: sb.Append("--direct spatial "); break;
                            case 2: sb.Append("--direct temporal "); break;
                            case 3: sb.Append("--direct auto "); break;
                        }
                    }
                }

                if (!inProfile.CustomCommandLine.Contains("--nr "))
                    if (inProfile.NoiseReduction > 0)
                        sb.AppendFormat("--nr {0:0} ", inProfile.NoiseReduction);


                // subpel refinement
                if (!inProfile.CustomCommandLine.Contains("--subme "))
                {
                    display = false;
                    switch (inProfile.Preset)
                    {
                        case 0: if (inProfile.SubPixelRefinement != 0) display = true; break;
                        case 1: if (inProfile.SubPixelRefinement != 1) display = true; break;
                        case 2: if (inProfile.SubPixelRefinement != 2) display = true; break;
                        case 3: if (inProfile.SubPixelRefinement != 4) display = true; break;
                        case 4: if (inProfile.SubPixelRefinement != 6) display = true; break;
                        case 5: if (inProfile.SubPixelRefinement != 7) display = true; break;
                        case 6: if (inProfile.SubPixelRefinement != 8) display = true; break;
                        case 7: if (inProfile.SubPixelRefinement != 9) display = true; break;
                        case 8: if (inProfile.SubPixelRefinement != 10) display = true; break;
                        case 9: if (inProfile.SubPixelRefinement != 11) display = true; break;
                    }
                    if (display)
                        sb.AppendFormat("--subme {0:0} ", inProfile.SubPixelRefinement);
                }

                // macroblock types
                if (!inProfile.CustomCommandLine.Contains("--partitions "))
                {
                    bool bExpectedP8X8Mv = true;
                    bool bExpectedB8X8Mv = true;
                    bool bExpectedI4X4Mv = true;
                    bool bExpectedI8X8Mv = true;
                    bool bExpectedP4X4Mv = true;

                    switch (inProfile.Preset)
                    {
                        case 0:
                            bExpectedP8X8Mv = false;
                            bExpectedB8X8Mv = false;
                            bExpectedI4X4Mv = false;
                            bExpectedI8X8Mv = false;
                            bExpectedP4X4Mv = false;
                            break;
                        case 1:
                            bExpectedP8X8Mv = false;
                            bExpectedB8X8Mv = false;
                            bExpectedP4X4Mv = false;
                            break;
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            bExpectedP4X4Mv = false;
                            break;
                    }
                    if (inProfile.Tuning == 7 && bExpectedP8X8Mv)
                        bExpectedP4X4Mv = true;

                    if (inProfile.AVCProfile < 2)
                        bExpectedI8X8Mv = false;

                    if (bExpectedP8X8Mv != inProfile.MacroBlocksPartitionsP8X8 || bExpectedB8X8Mv != inProfile.MacroBlocksPartitionsB8X8
                        || bExpectedI4X4Mv != inProfile.MacroBlocksPartitionsI4X4 || bExpectedI8X8Mv != inProfile.MacroBlocksPartitionsI8X8
                        || bExpectedP4X4Mv != inProfile.MacroBlocksPartitionsP4X4)
                    {
                        if (inProfile.MacroBlocksPartitionsP8X8 ||
                            inProfile.MacroBlocksPartitionsB8X8 ||
                            inProfile.MacroBlocksPartitionsI4X4 ||
                            inProfile.MacroBlocksPartitionsI8X8 ||
                            inProfile.MacroBlocksPartitionsP4X4)
                        {
                            sb.Append("--partitions ");
                            if (inProfile.MacroBlocksPartitionsI4X4 &&
                                    inProfile.MacroBlocksPartitionsI8X8 &&
                                    inProfile.MacroBlocksPartitionsP4X4 &&
                                    inProfile.MacroBlocksPartitionsP8X8 &&
                                    inProfile.MacroBlocksPartitionsB8X8)
                                sb.Append("all ");
                            else
                            {
                                if (inProfile.MacroBlocksPartitionsP8X8) // default is checked
                                    sb.Append("p8x8,");
                                if (inProfile.MacroBlocksPartitionsB8X8) // default is checked
                                    sb.Append("b8x8,");
                                if (inProfile.MacroBlocksPartitionsI4X4) // default is checked
                                    sb.Append("i4x4,");
                                if (inProfile.MacroBlocksPartitionsP4X4) // default is unchecked
                                    sb.Append("p4x4,");
                                if (inProfile.MacroBlocksPartitionsI8X8) // default is checked
                                    sb.Append("i8x8");
                                if (sb.ToString().EndsWith(","))
                                    sb.Remove(sb.Length - 1, 1);
                            }

                            if (!sb.ToString().EndsWith(" "))
                                sb.Append(" ");
                        }
                        else
                            sb.Append("--partitions none ");
                    }
                }

                if (inProfile.AVCProfile > 1 && !inProfile.CustomCommandLine.Contains("--no-8x8dct"))
                    if (!inProfile.MacroBlocksPartitionsAdaptiveDCT)
                        if (inProfile.Preset > 0)
                            sb.Append("--no-8x8dct ");

                // Trellis
                if (!inProfile.CustomCommandLine.Contains("--trellis "))
                {
                    display = false;
                    switch (inProfile.Preset)
                    {
                        case 0:
                        case 1:
                        case 2: if (inProfile.Trellis != 0) display = true; break;
                        case 3:
                        case 4:
                        case 5:
                        case 6: if (inProfile.Trellis != 1) display = true; break;
                        case 7:
                        case 8:
                        case 9: if (inProfile.Trellis != 2) display = true; break;
                    }
                    if (display)
                        sb.AppendFormat("--trellis {0:0} ", inProfile.Trellis);
                }

                if (!inProfile.CustomCommandLine.Contains("--psy-rd "))
                {
                    if (inProfile.SubPixelRefinement > 5)
                    {
                        display = false;
                        switch (inProfile.Tuning)
                        {
                            case 1: if ((Math.Abs(inProfile.PsyRDStrength - 1.0F) > 0) || (Math.Abs(inProfile.PsyTrellisStrength - 0.15F) > 0)) display = true; break;
                            case 2: if ((Math.Abs(inProfile.PsyRDStrength - 0.4F) > 0) || (Math.Abs(inProfile.PsyTrellisStrength - 0.0F) > 0)) display = true; break;
                            case 3: if ((Math.Abs(inProfile.PsyRDStrength - 1.0F) > 0) || (Math.Abs(inProfile.PsyTrellisStrength - 0.25F) > 0)) display = true; break;
                            case 7: if ((Math.Abs(inProfile.PsyRDStrength - 1.0F) > 0) || (Math.Abs(inProfile.PsyTrellisStrength - 0.2F) > 0)) display = true; break;
                            default: if ((Math.Abs(inProfile.PsyRDStrength - 1.0F) > 0) || (Math.Abs(inProfile.PsyTrellisStrength - 0.0F) > 0)) display = true; break;
                        }

                        if (display)
                            sb.AppendFormat(AppSettings.CInfo, "--psy-rd {0:0.00}:{1:0.00} ", inProfile.PsyRDStrength, inProfile.PsyTrellisStrength);
                    }
                }

                if (!inProfile.CustomCommandLine.Contains("--no-mixed-refs"))
                    if (inProfile.UseNoMixedReferenceFrames)
                        if (inProfile.Preset >= 4) // preset fast
                            sb.Append("--no-mixed-refs ");

                if (!inProfile.CustomCommandLine.Contains("--no-dct-decimate"))
                    if (inProfile.UseNoDCTDecimation)
                        if (inProfile.Tuning != 3) // tune grain
                            sb.Append("--no-dct-decimate ");

                if (!inProfile.CustomCommandLine.Contains("--no-fast-pskip"))
                    if (inProfile.UseNoFastPSkip)
                        if (inProfile.Preset != 9) // preset placebo
                            sb.Append("--no-fast-pskip ");

                if (!inProfile.CustomCommandLine.Contains("--no-psy"))
                    if (inProfile.UseNoPsychovisualEnhancements && (inProfile.Tuning != 5 && inProfile.Tuning != 6))
                        sb.Append("--no-psy ");

                inProfile.UseAccessUnitDelimiters = GetAud(inProfile, device);
                if (inProfile.UseAccessUnitDelimiters && !inProfile.UseBluRayCompatibility)
                    sb.Append("--aud ");

                inProfile.HRDInfo = GetNalHrd(inProfile, device);
                switch (inProfile.HRDInfo)
                {
                    case 1: if (!inProfile.UseBluRayCompatibility) sb.Append("--nal-hrd vbr "); break;
                    case 2: sb.Append("--nal-hrd cbr "); break;
                }

                if (!inProfile.CustomCommandLine.Contains("--non-deterministic"))
                    if (inProfile.UseNonDeterministic)
                        sb.Append("--non-deterministic ");
                #endregion

                #region misc tab

                if (!inProfile.CustomCommandLine.Contains("--psnr"))
                    if (inProfile.UsePSNRCalculation)
                        sb.Append("--psnr ");

                if (!inProfile.CustomCommandLine.Contains("--ssim"))
                    if (inProfile.UseSSIMCalculation)
                        sb.Append("--ssim ");

                if (!inProfile.CustomCommandLine.Contains("--range "))
                    switch (inProfile.VUIRange)
                    {
                        case 1:
                            sb.AppendFormat("--range tv ");
                            break;
                        case 2:
                            sb.Append("--range pc ");
                            break;
                    }

                #endregion

                #region input / ouput / custom

                string customSarValue = string.Empty;

                Dar? d = new Dar((ulong)hRes, (ulong)vRes);

                if (inProfile.UseAutoSelectSAR)
                {
                    int tempValue = GetSar(inProfile, d, hRes, vRes, out customSarValue, String.Empty);
                    inProfile.ForceSAR = tempValue;
                }

                if (inProfile.UseAutoSelectColorSettings)
                {
                    inProfile.ColorPrimaries = GetColorprim(inProfile, format);

                    inProfile.Transfer = GetTransfer(inProfile, format);

                    inProfile.ColorMatrix = GetColorMatrix(inProfile, format);
                }

                if (device.BluRay)
                {
                    if (inProfile.InterlaceMode < 2)
                        inProfile.InterlaceMode = GetInterlacedMode(format);

                    inProfile.UseFakeInterlaced = GetFakeInterlaced(inProfile, format, fpsN, fpsD);

                    inProfile.UseForcePicStruct = GetPicStruct(inProfile, format);

                    inProfile.Pulldown = GetPulldown(inProfile, format, fpsN, fpsD);
                }
                else
                {
                    if (inProfile.InterlaceMode == 0)
                        inProfile.InterlaceMode = GetInterlacedMode(format);

                    if (inProfile.Pulldown == 0)
                        inProfile.Pulldown = GetPulldown(inProfile, format, fpsN, fpsD);
                }

                if (!inProfile.CustomCommandLine.Contains("--bff") &&
                    !inProfile.CustomCommandLine.Contains("--tff"))
                {
                    switch (inProfile.InterlaceMode)
                    {
                        case 2: sb.Append("--bff "); break;
                        case 3: sb.Append("--tff "); break;
                    }
                }

                if (!inProfile.CustomCommandLine.Contains("--fake-interlaced"))
                {
                    if (inProfile.UseFakeInterlaced && inProfile.InterlaceMode == 1)
                        sb.Append("--fake-interlaced ");
                }

                if (!inProfile.CustomCommandLine.Contains("--pic-struct"))
                {
                    if (inProfile.UseForcePicStruct && inProfile.InterlaceMode == 1 && inProfile.Pulldown == 0)
                        sb.Append("--pic-struct ");
                }

                if (!inProfile.CustomCommandLine.Contains("--colorprim"))
                {
                    switch (inProfile.ColorPrimaries)
                    {
                        case 0: break;
                        case 1: sb.Append("--colorprim bt709 "); break;
                        case 2: sb.Append("--colorprim bt470m "); break;
                        case 3: sb.Append("--colorprim bt470bg "); break;
                        case 4: sb.Append("--colorprim smpte170m "); break;
                        case 5: sb.Append("--colorprim smpte240m "); break;
                        case 6: sb.Append("--colorprim film "); break;
                    }
                }

                if (!inProfile.CustomCommandLine.Contains("--transfer"))
                {
                    switch (inProfile.Transfer)
                    {
                        case 0: break;
                        case 1: sb.Append("--transfer bt709 "); break;
                        case 2: sb.Append("--transfer bt470m "); break;
                        case 3: sb.Append("--transfer bt470bg "); break;
                        case 4: sb.Append("--transfer linear "); break;
                        case 5: sb.Append("--transfer log100 "); break;
                        case 6: sb.Append("--transfer log316 "); break;
                        case 7: sb.Append("--transfer smpte170m "); break;
                        case 8: sb.Append("--transfer smpte240m "); break;
                    }
                }

                if (!inProfile.CustomCommandLine.Contains("--colormatrix"))
                {
                    switch (inProfile.ColorMatrix)
                    {
                        case 0: break;
                        case 1: sb.Append("--colormatrix bt709 "); break;
                        case 2: sb.Append("--colormatrix fcc "); break;
                        case 3: sb.Append("--colormatrix bt470bg "); break;
                        case 4: sb.Append("--colormatrix smpte170m "); break;
                        case 5: sb.Append("--colormatrix smpte240m "); break;
                        case 6: sb.Append("--colormatrix GBR "); break;
                        case 7: sb.Append("--colormatrix YCgCo "); break;
                    }
                }

                if (!inProfile.CustomCommandLine.Contains("--pulldown"))
                {
                    switch (inProfile.Pulldown)
                    {
                        case 0: break;
                        case 1: break;
                        case 2: sb.Append("--pulldown 22 "); break;
                        case 3: sb.Append("--pulldown 32 "); break;
                        case 4: sb.Append("--pulldown 64 "); break;
                        case 5: sb.Append("--pulldown double "); break;
                        case 6: sb.Append("--pulldown triple "); break;
                        case 7: sb.Append("--pulldown euro "); break;
                    }
                }


                if (!String.IsNullOrEmpty(inProfile.CustomCommandLine)) // add custom encoder options
                    sb.Append(Regex.Replace(inProfile.CustomCommandLine, @"\r\n?|\n", string.Empty).Trim() + " ");

                if (!inProfile.CustomCommandLine.Contains("--sar"))
                {
                    switch (inProfile.ForceSAR)
                    {
                        case 0:
                            {
                                if (!String.IsNullOrEmpty(customSarValue))
                                    sb.Append("--sar " + customSarValue + " ");
                                break;
                            }
                        case 1: sb.Append("--sar 1:1 "); break;
                        case 2: sb.Append("--sar 4:3 "); break;
                        case 3: sb.Append("--sar 8:9 "); break;
                        case 4: sb.Append("--sar 10:11 "); break;
                        case 5: sb.Append("--sar 12:11 "); break;
                        case 6: sb.Append("--sar 16:11 "); break;
                        case 7: sb.Append("--sar 32:27 "); break;
                        case 8: sb.Append("--sar 40:33 "); break;
                        case 9: sb.Append("--sar 64:45 "); break;
                    }
                }

                if (!inProfile.CustomCommandLine.Contains("--frame-packing"))
                {
                    if (stereo != StereoEncoding.None)
                        sb.Append("--frame-packing 3 ");
                }

                //add the rest of the commandline regarding the output
                if ((inProfile.EncodingMode == 2 || inProfile.EncodingMode == 3) && (tempPass == 1))
                    sb.Append("--output NUL ");
                else if (!String.IsNullOrEmpty(outFile))
                    sb.AppendFormat("--output \"{0}\" ", outFile);

                if (!String.IsNullOrEmpty(inFile))
                {
                    if (String.CompareOrdinal(inFile, "-") == 0)
                        sb.AppendFormat("--demuxer y4m - ");
                    else
                        sb.AppendFormat("\"{0}\" ", inFile);
                }
                    
                #endregion
            }
            return sb.ToString();
        }

        private static int GetPulldown(X264Profile inProfile, VideoFormat format, int fpsN, int fpsD)
        {
            int pullDown = inProfile.Pulldown;

            switch (format)
            {
                case VideoFormat.Unknown:
                    break;
                case VideoFormat.Videoformat480I:
                    break;
                case VideoFormat.Videoformat480P:
                    pullDown = 3;
                    break;
                case VideoFormat.Videoformat576I:
                    break;
                case VideoFormat.Videoformat576P:
                    break;
                case VideoFormat.Videoformat720P:
                    if (((fpsN == 30000) && (fpsD == 1001)) || ((fpsN == 25000) && (fpsD == 1000))) // 29.976 or 25 fps
                        pullDown = 5;
                    break;
                case VideoFormat.Videoformat1080I:
                    break;
                case VideoFormat.Videoformat1080P:
                    break;
            }

            return pullDown;
        }

        private static bool GetPicStruct(X264Profile inProfile, VideoFormat format)
        {
            bool pStruct = inProfile.UseForcePicStruct;

            switch (format)
            {
                case VideoFormat.Videoformat576P:
                    pStruct = true;
                    break;
            }

            return pStruct;
        }

        private static bool GetFakeInterlaced(X264Profile inProfile, VideoFormat format, int fpsN, int fpsD)
        {
            bool fInterlaced = inProfile.UseFakeInterlaced;

            switch (format)
            {
                case VideoFormat.Videoformat480P:
                case VideoFormat.Videoformat576P:
                    fInterlaced = true;
                    break;
                case VideoFormat.Videoformat1080P:
                    if (((fpsN == 30000) && (fpsD == 1001)) || ((fpsN == 25000) && (fpsD == 1000))) // 29.976 or 25 fps
                        fInterlaced = true;
                    break;
            }

            return fInterlaced;
        }

        private static int GetInterlacedMode(VideoFormat format)
        {
            int iMode;

            switch (format)
            {
                case VideoFormat.Videoformat480I:
                case VideoFormat.Videoformat576I:
                case VideoFormat.Videoformat1080I:
                    iMode = 2;
                    break;
                default:
                    iMode = 1;
                    break;
            }

            return iMode;
        }

        private static int GetColorMatrix(X264Profile inProfile, VideoFormat format)
        {
            int matrix = inProfile.ColorMatrix;
            switch (format)
            {
                case VideoFormat.Videoformat480I:
                case VideoFormat.Videoformat480P:
                    matrix = 4;
                    break;
                case VideoFormat.Videoformat576I:
                case VideoFormat.Videoformat576P:
                    matrix = 3;
                    break;
                case VideoFormat.Videoformat720P:
                case VideoFormat.Videoformat1080I:
                case VideoFormat.Videoformat1080P:
                    matrix = 1;
                    break;
            }
            return matrix;
        }

        private static int GetTransfer(X264Profile inProfile, VideoFormat format)
        {
            int transfer = inProfile.Transfer;
            switch (format)
            {
                case VideoFormat.Videoformat480I:
                case VideoFormat.Videoformat480P:
                    transfer = 7;
                    break;
                case VideoFormat.Videoformat576I:
                case VideoFormat.Videoformat576P:
                    transfer = 3;
                    break;
                case VideoFormat.Videoformat720P:
                case VideoFormat.Videoformat1080I:
                case VideoFormat.Videoformat1080P:
                    transfer = 1;
                    break;
            }
            return transfer;
        }

        private static int GetColorprim(X264Profile inProfile, VideoFormat format)
        {
            int colorPrim = inProfile.ColorPrimaries;
            switch (format)
            {
                case VideoFormat.Videoformat480I:
                case VideoFormat.Videoformat480P:
                    colorPrim = 4;
                    break;
                case VideoFormat.Videoformat576I:
                case VideoFormat.Videoformat576P:
                    colorPrim = 3;
                    break;
                case VideoFormat.Videoformat720P:
                case VideoFormat.Videoformat1080I:
                case VideoFormat.Videoformat1080P:
                    colorPrim = 1;
                    break;
            }
            return colorPrim;
        }

        private static int GetSar(X264Profile inProfile, Dar? d, int hRes, int vRes, out string customSarValue, string customSarValueInput)
        {
            string strCustomValue = string.Empty;
            int sar = inProfile.ForceSAR;

            customSarValue = String.Empty;
            if (String.IsNullOrEmpty(customSarValueInput))
            {
                switch (strCustomValue.ToLower())
                {
                    case "1:1": sar = 1; break;
                    case "4:3": sar = 2; break;
                    case "8:9": sar = 3; break;
                    case "10:11": sar = 4; break;
                    case "12:11": sar = 5; break;
                    case "16:11": sar = 6; break;
                    case "32:27": sar = 7; break;
                    case "40:33": sar = 8; break;
                    case "64:45": sar = 9; break;
                    default:
                        customSarValue = strCustomValue;
                        sar = 0; break;
                }
            }

            if (d.HasValue && sar == 0 &&
                String.IsNullOrEmpty(customSarValue) && String.IsNullOrEmpty(customSarValueInput))
            {
                Sar s = d.Value.ToSar(hRes, vRes);
                switch (s.X + ":" + s.Y)
                {
                    case "1:1": sar = 1; break;
                    case "4:3": sar = 2; break;
                    case "8:9": sar = 3; break;
                    case "10:11": sar = 4; break;
                    case "12:11": sar = 5; break;
                    case "16:11": sar = 6; break;
                    case "32:27": sar = 7; break;
                    case "40:33": sar = 8; break;
                    case "64:45": sar = 9; break;
                    default: customSarValue = s.X + ":" + s.Y; break;
                }
            }

            return sar;
        }

        private static int GetNalHrd(X264Profile inProfile, X264Device device)
        {
            int nalHrd = inProfile.HRDInfo;

            if (device.BluRay && nalHrd < 1)
            {
                nalHrd = 1;
            }

            return nalHrd;
        }

        private static bool GetAud(X264Profile inProfile, X264Device device)
        {
            bool aud = inProfile.UseAccessUnitDelimiters || device.BluRay && inProfile.UseAccessUnitDelimiters == false;

            return aud;
        }

        private static int GetVBVMaxrate(X264Profile inProfile, X264Device device)
        {
            int vbvMaxRate = inProfile.VBVMaxRate;

            if (device.VBVMaxrate > -1 && (vbvMaxRate > device.VBVMaxrate || vbvMaxRate == 0))
            {
                vbvMaxRate = device.VBVMaxrate;
            }

            return vbvMaxRate;
        }

        private static int GetVBVBufsize(X264Profile inProfile, X264Device device)
        {
            int vbvBufSize = inProfile.VBVBufSize;

            if (device.VBVBufsize > -1 && (vbvBufSize > device.VBVBufsize || vbvBufSize == 0))
            {
                vbvBufSize = device.VBVBufsize;
            }

            return vbvBufSize;
        }

        private static int GetSlices(X264Profile inProfile, X264Device device)
        {
            int numSlices = inProfile.NumSlices;
            
            if (device.BluRay && numSlices != 4)
            {
                numSlices = 4;
            }

            return numSlices;
        }

        private static int GetWeightp(X264Profile inProfile, X264Device device)
        {
            int weightP = inProfile.PFrameWeightedPrediction;

            if (device.BluRay && weightP > 1)
            {
                weightP = 1;
            }

            return weightP;
        }

        private static int GetRefFrames(int hRes, int vRes, X264Profile inProfile, X264Device device)
        {
            int refFrames = inProfile.NumRefFrames;
            
            if (device.ReferenceFrames > -1 && refFrames > device.ReferenceFrames)
            {
                refFrames = device.ReferenceFrames;
            }

            int iMaxRefForLevel = X264Settings.GetMaxRefForLevel(inProfile.AVCLevel, hRes, vRes);
            if (iMaxRefForLevel > -1 && iMaxRefForLevel < refFrames)
            {
                refFrames = iMaxRefForLevel;
            }

            return refFrames;

        }

        private static int GetBPyramid(X264Profile inProfile, X264Device device)
        {
            int bPyramid = inProfile.BPyramid;

            if (device.BluRay && inProfile.BPyramid > 1)
            {
                bPyramid = 1;
            }

            if (device.BPyramid > -1 && bPyramid != device.BPyramid)
            {
                bPyramid = device.BPyramid;
            }

            return bPyramid;
        }

        private static int GetBFrames(X264Profile inProfile, X264Device device)
        {
            int numBframes = inProfile.NumBFrames;

            if (device.BFrames > -1 && inProfile.NumBFrames > device.BFrames)
            {
                numBframes = device.BFrames;
            }

            return numBframes;
        }

        private static int GetMinKeyInt(int fpsN, int fpsD, int minGop, int maxGop, X264Device device, int gopCalculation)
        {
            int keyInt = 0;

            double fps = (double)fpsN / fpsD;
            if (gopCalculation == 1) // calculate min-keyint based on 25fps
                keyInt = (int)(minGop / 25.0 * fps);

            int maxValue = maxGop / 2 + 1;
            if (device.MaxGOP > -1 && minGop > maxValue)
            {
                int Default = maxGop / 10;
                keyInt = Default;
            }

            return keyInt;
        }

        private static int GetKeyInt(int fpsN, int fpsD, int maxGop, X264Device device, int gopCalculation)
        {
            int keyInt = 0;

            if (gopCalculation == 1)// calculate min-keyint based on 25fps
                keyInt = (int)Math.Round(maxGop / 25.0 * (fpsN / (double)fpsD), 0);

            int fps = (int)Math.Round((decimal)fpsN / fpsD, 0);

            if (device.MaxGOP > -1 && maxGop > fps * device.MaxGOP)
            {
                keyInt = fps * device.MaxGOP;
            }

            return keyInt;
        }
    }
}
