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
using System.Drawing;
using System.IO;
using System.Linq;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Media;
using VideoConvert.Core.Profiles;
using log4net;
using System.Text;
using System.Collections.Generic;

namespace VideoConvert.Core.Encoder
{

    class AviSynthGenerator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AviSynthGenerator));

        public static string StereoConfigFile = string.Empty;

        public static string Generate(VideoInfo videoInfo, bool changeFps, float targetFps, Size resizeTo,
                                      StereoEncoding stereoEncoding, StereoVideoInfo stereoVideoInfo, bool isDvdResolution, string subtitleFile, bool subtitleOnlyForced)
        {
            StringBuilder sb = new StringBuilder();

            bool mtUseful = (videoInfo.Interlaced && AppSettings.UseHQDeinterlace) || changeFps;

            bool useStereo = stereoEncoding != StereoEncoding.None && stereoVideoInfo.RightStreamId > -1;

            if (AppSettings.UseAviSynthMT && mtUseful)
            {
                sb.AppendLine("SetMTMode(2,0)");
                sb.AppendLine("SetMemoryMax(512)");
            }
            //loading plugins

            sb.AppendLine(string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                        Path.Combine(AppSettings.AppPath, "AvsPlugins", "ffms2.dll")));

            if (changeFps || (videoInfo.Interlaced && AppSettings.UseHQDeinterlace))
            {
                sb.AppendLine(string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                            Path.Combine(AppSettings.AppPath, "AvsPlugins", "mvtools2.dll")));
            }
            if (videoInfo.Interlaced && AppSettings.UseHQDeinterlace)
            {
                sb.AppendLine(AppSettings.LastAviSynthVer.StartsWith("2.5")
                                  ? string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                                  Path.Combine(AppSettings.AppPath, "AvsPlugins", "mt_masktools-25.dll"))
                                  : string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                                  Path.Combine(AppSettings.AppPath, "AvsPlugins", "mt_masktools-26.dll")));

                sb.AppendLine(string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                            Path.Combine(AppSettings.AppPath, "AvsPlugins", "nnedi3.dll")));
                sb.AppendLine(string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                            Path.Combine(AppSettings.AppPath, "AvsPlugins", "RemoveGrainSSE2.dll")));
                sb.AppendLine(string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                            Path.Combine(AppSettings.AppPath, "AvsPlugins", "RepairSSE2.dll")));
                sb.AppendLine(string.Format(AppSettings.CInfo, "Import(\"{0:s}\")",
                                            Path.Combine(AppSettings.AppPath, "AvsPlugins", "QTGMC-3.32.avsi")));
            }

            if (useStereo)
            {
                sb.AppendLine(string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                            Path.Combine(AppSettings.AppPath, "AvsPlugins", "H264StereoSource.dll")));
            }

            if (!string.IsNullOrEmpty(subtitleFile) && File.Exists(subtitleFile))
            {
                switch (Path.GetExtension(subtitleFile))
                {
                    case "sup":
                        sb.AppendFormat(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                        Path.Combine(AppSettings.AppPath, "AvsPlugins", "SupTitle.dll"));
                        break;
                    case "ass":
                    case "ssa":
                    case "srt":
                        sb.AppendFormat(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                        Path.Combine(AppSettings.AppPath, "AvsPlugins", "VSFilter.dll"));
                        break;
                }
                sb.AppendLine();
            }

            //generate rest of the script

            if (videoInfo.FPS <= 0)
            {
                MediaInfoContainer mi = Processing.GetMediaInfo(videoInfo.TempFile);
                if (mi.Video.Count > 0)
                {
                    videoInfo.FPS = mi.Video[0].FrameRate;
                    Processing.GetFPSNumDenom(videoInfo.FPS, out videoInfo.FrameRateEnumerator,
                                              out videoInfo.FrameRateDenominator);

                    if (videoInfo.FrameRateEnumerator == 0)
                    {
                        videoInfo.FrameRateEnumerator = (int)Math.Round(videoInfo.FPS) * 1000;
                        videoInfo.FrameRateDenominator =
                            (int) (Math.Round(Math.Ceiling(videoInfo.FPS) - Math.Floor(videoInfo.FPS)) + 1000);
                    }
                }
            }

            if (videoInfo.FrameRateEnumerator > 0 && videoInfo.FrameRateDenominator > 0)
            {
                sb.AppendLine(string.Format(AppSettings.CInfo,
                                            "FFVideoSource(\"{0:s}\",fpsnum={1:0},fpsden={2:0},threads=1)",
                                            videoInfo.TempFile, videoInfo.FrameRateEnumerator,
                                            videoInfo.FrameRateDenominator));
            }
            else
            {
                sb.AppendLine(string.Format(AppSettings.CInfo, "FFVideoSource(\"{0:s}\",threads=1)", videoInfo.TempFile));
            }

            string stereoVar = string.Empty;

            if (useStereo)
            {
                string configFile = GenerateStereoSourceConfig(stereoVideoInfo);
                sb.AppendLine(string.Format(AppSettings.CInfo, "VideoRight = H264StereoSource(\"{0:s}\",{1:g})",
                                            configFile, videoInfo.FrameCount - 50));
                StereoConfigFile = configFile;
                stereoVar = "VideoRight";
            }

            if (!string.IsNullOrEmpty(subtitleFile) && File.Exists(subtitleFile))
            {
                switch (Path.GetExtension(subtitleFile))
                {
                    case "sup":
                        sb.AppendFormat(AppSettings.CInfo, "SupTitle(\"{0}\", forcedOnly={1})", subtitleFile,
                                                        subtitleOnlyForced ? "true" : "false");
                        break;
                    case "ass":
                    case "ssa":
                    case "srt":
                        sb.AppendFormat(AppSettings.CInfo, "TextSub(\"{0}\")", subtitleFile);
                        break;
                }
                
                sb.AppendLine();
            }

            if (!videoInfo.CropRect.IsEmpty)
            {
                int temp;

                Math.DivRem(videoInfo.CropRect.X, 2, out temp);
                videoInfo.CropRect.X += temp;
                Math.DivRem(videoInfo.CropRect.Y, 2, out temp);
                videoInfo.CropRect.Y += temp;
                Math.DivRem(videoInfo.CropRect.Width, 2, out temp);
                videoInfo.CropRect.Width += temp;
                Math.DivRem(videoInfo.CropRect.Height, 2, out temp);
                videoInfo.CropRect.Height += temp;

                videoInfo.Height = videoInfo.CropRect.Height;
                videoInfo.Width = videoInfo.CropRect.Width;

                if ((videoInfo.CropRect.X > 0) || (videoInfo.CropRect.Y > 0) || (videoInfo.CropRect.Width < videoInfo.Width) ||
                    (videoInfo.CropRect.Height < videoInfo.Height))
                {
                    sb.AppendLine(string.Format(AppSettings.CInfo, "Crop({0:g},{1:g},{2:g},{3:g})",
                                                videoInfo.CropRect.Left,
                                                videoInfo.CropRect.Top,
                                                videoInfo.CropRect.Width,
                                                videoInfo.CropRect.Height));
                    if (useStereo)
                    {
                        sb.AppendLine(string.Format(AppSettings.CInfo,
                                                    "CroppedVideoRight = Crop(VideoRight,{0:g},{1:g},{2:g},{3:g})",
                                                    videoInfo.CropRect.Left,
                                                    videoInfo.CropRect.Top,
                                                    videoInfo.CropRect.Width,
                                                    videoInfo.CropRect.Height));
                        stereoVar = "CroppedVideoRight";
                    }
                }
            }

            if (!string.IsNullOrEmpty(stereoVar))
            {
                switch(stereoEncoding)
                {
                    case StereoEncoding.FullSideBySideLeft:
                    case StereoEncoding.HalfSideBySideLeft:
                        sb.AppendLine(string.Format("StackHorizontal(last,{0})", stereoVar));
                        break;
                    case StereoEncoding.FullSideBySideRight:
                    case StereoEncoding.HalfSideBySideRight:
                        sb.AppendLine(string.Format("StackHorizontal({0},last)", stereoVar));
                        break;
                }
                sb.AppendLine("ConvertToYV12()");
            }

            int calculatedHeight = videoInfo.Height;
            int calculatedWidth = videoInfo.Width;
            int borderRight = 0;
            int borderLeft = 0;
            int borderBottom = 0;
            int borderTop = 0;
            bool addBorders = false;

            if (!resizeTo.IsEmpty && (resizeTo.Height != videoInfo.Height || resizeTo.Width != videoInfo.Width))
            {
                // aspect ratios

                float toAr = (float) Math.Round(resizeTo.Width / (float)resizeTo.Height, 3);

                calculatedWidth = resizeTo.Width;

                float mod = 1f;

                if (videoInfo.AspectRatio > toAr)
                {
                    if (isDvdResolution)
                    {
                        calculatedHeight = (int)(calculatedWidth / videoInfo.AspectRatio);
                        if (calculatedHeight > resizeTo.Height)
                            calculatedHeight = resizeTo.Height;
                        calculatedWidth = 720;
                    }
                    else
                    {
                        calculatedWidth = resizeTo.Width;
                        calculatedHeight = (int)(calculatedWidth / videoInfo.AspectRatio);
                    }

                    int temp;

                    Math.DivRem(calculatedWidth, 2, out temp);
                    calculatedWidth += temp;
                    Math.DivRem(calculatedHeight, 2, out temp);
                    calculatedHeight += temp;

                    if (calculatedHeight != resizeTo.Height)
                    {
                        addBorders = true;
                        int borderHeight = resizeTo.Height - calculatedHeight;
                        borderTop = borderHeight/2;
                        Math.DivRem(borderTop, 2, out temp);
                        borderTop += temp;
                        borderBottom = borderHeight - borderTop;
                    }
                }
                else if (Math.Abs(videoInfo.AspectRatio - toAr) <= 0)
                {
                    if (isDvdResolution)
                    {
                        calculatedHeight = (int)(calculatedWidth / videoInfo.AspectRatio);
                        calculatedWidth = 720;
                        if (calculatedHeight > resizeTo.Height)
                            calculatedHeight = resizeTo.Height;
                    }
                    else
                    {
                        calculatedWidth = resizeTo.Width;
                        calculatedHeight = (int) (calculatedWidth/toAr);
                    }

                    int temp;

                    Math.DivRem(calculatedWidth, 2, out temp);
                    calculatedWidth += temp;
                    Math.DivRem(calculatedHeight, 2, out temp);
                    calculatedHeight += temp;

                    if (calculatedHeight != resizeTo.Height)
                    {
                        addBorders = true;
                        int borderHeight = resizeTo.Height - calculatedHeight;
                        borderTop = borderHeight/2;
                        Math.DivRem(borderTop, 2, out temp);
                        borderTop += temp;
                        borderBottom = borderHeight - borderTop;
                    }
                }
                else
                {
                    if (videoInfo.AspectRatio > 1.4f && isDvdResolution)
                    {
                        mod = 720f/resizeTo.Width;

                        calculatedHeight = (int)(calculatedWidth / videoInfo.AspectRatio);
                        if (calculatedHeight > resizeTo.Height)
                        {
                            calculatedHeight = resizeTo.Height;
                            calculatedWidth = (int)(calculatedHeight * videoInfo.AspectRatio * mod);
                        }
                        else
                            calculatedWidth = 720;
                    }
                    else if (isDvdResolution)
                    {
                        calculatedHeight = resizeTo.Height;
                        calculatedWidth = (int)(calculatedHeight * videoInfo.AspectRatio);
                    }
                    else
                    {
                        calculatedHeight = resizeTo.Height;
                        //calculatedWidth = (int)(calculatedHeight * videoInfo.AspectRatio * mod);
                    }

                    int temp;
                    Math.DivRem(calculatedWidth, 2, out temp);
                    calculatedWidth += temp;
                    Math.DivRem(calculatedHeight, 2, out temp);
                    calculatedHeight += temp;

                    if (Math.Abs(toAr - 1.778f) <= 0)
                    {
                        addBorders = true;
                        int borderHeight = resizeTo.Height - calculatedHeight;
                        borderTop = borderHeight/2;
                        Math.DivRem(borderTop, 2, out temp);
                        borderTop += temp;
                        borderBottom = borderHeight - borderTop;

                        int borderWidth = (int) ((resizeTo.Width*mod) - calculatedWidth);
                        borderLeft = borderWidth/2;
                        Math.DivRem(borderLeft, 2, out temp);
                        borderLeft += temp;
                        borderRight = borderWidth - borderLeft;
                    }
                    else if (calculatedWidth != resizeTo.Width)
                    {
                        addBorders = true;
                        int borderWidth = resizeTo.Width - calculatedWidth;
                        borderLeft = borderWidth/2;
                        Math.DivRem(borderLeft, 2, out temp);
                        borderLeft += temp;
                        borderRight = borderWidth - borderLeft;

                        int borderHeight = resizeTo.Height - calculatedHeight;
                        borderTop = borderHeight/2;
                        Math.DivRem(borderTop, 2, out temp);
                        borderTop += temp;
                        borderBottom = borderHeight - borderTop;
                    }
                }
            }

            if ((calculatedHeight != videoInfo.Height) || (calculatedWidth != videoInfo.Width) ||
                    ((stereoEncoding == StereoEncoding.HalfSideBySideLeft) || (stereoEncoding == StereoEncoding.HalfSideBySideRight)) 
                    && useStereo)
            {
                sb.AppendLine(string.Format(AppSettings.CInfo, "Lanczos4Resize({0:g},{1:g})",
                                                            calculatedWidth,
                                                            calculatedHeight));
            }

            if (addBorders)
            {
                sb.AppendLine(string.Format(AppSettings.CInfo, "AddBorders({0:g},{1:g},{2:g},{3:g})",
                                                        borderLeft,
                                                        borderTop,
                                                        borderRight,
                                                        borderBottom));
            }

            if (videoInfo.Interlaced)
            {
                if (AppSettings.UseHQDeinterlace)
                {
                    sb.AppendLine("QTGMC(Preset=\"Slower\")");
                }
                else
                {
                    sb.AppendLine("SeparateFields()");
                    sb.AppendLine("Bob()");
                }
                sb.AppendLine("SelectEven()");
            }

            if (changeFps)
            {
                int fpsnum;
                int fpsden;

                Processing.GetFPSNumDenom(targetFps, out fpsnum, out fpsden);
                sb.AppendLine("super = MSuper(pel=2)");
                sb.AppendLine("backward_vec = MAnalyse(super, isb = true)");
                sb.AppendLine("forward_vec = MAnalyse(super, isb = false)");
                sb.AppendFormat("MFlowFps(super, backward_vec, forward_vec, num={0:0}, den={1:0}, ml=100)", fpsnum,
                                fpsden);
                sb.AppendLine();
            }

            if (AppSettings.UseAviSynthMT && mtUseful)
            {
                sb.AppendLine("SetMTMode(1)");
                sb.AppendLine("GetMTMode(false) > 0 ? distributor() : last");
            }

            return WriteScript(sb.ToString());
        }

        private static string GenerateStereoSourceConfig(StereoVideoInfo stereoVideoInfo)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(AppSettings.CInfo, "InputFile = \"{0:s}\"", stereoVideoInfo.LeftTempFile);
            sb.AppendLine();
            sb.AppendFormat(AppSettings.CInfo, "InputFile2 = \"{0:s}\"", stereoVideoInfo.RightTempFile);
            sb.AppendLine();
            sb.AppendLine("FileFormat = 0");
            sb.AppendLine("POCScale = 1");
            sb.AppendLine("DisplayDecParams = 1");
            sb.AppendLine("ConcealMode = 0");
            sb.AppendLine("RefPOCGap = 2");
            sb.AppendLine("POCGap = 2");
            sb.AppendLine("IntraProfileDeblocking = 1");
            sb.AppendLine("DecFrmNum = 0");

            return (WriteScript(sb.ToString(), "cfg"));
        }

        public static string GenerateCropDetect(string inputFile, float targetFps, double streamLength, out int frameCount)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                        Path.Combine(AppSettings.AppPath, "AvsPlugins\\ffms2.dll")));

            int fpsnum;
            int fpsden;
            Processing.GetFPSNumDenom(targetFps, out fpsnum, out fpsden);

            if (fpsnum == 0)
            {
                fpsnum = (int)Math.Round(targetFps) * 1000;
                fpsden = (int)(Math.Round(Math.Ceiling(targetFps) - Math.Floor(targetFps)) + 1000);
            }

            sb.AppendLine(string.Format(AppSettings.CInfo,
                                        "inStream=FFVideoSource(\"{0:s}\",fpsnum={1:0},fpsden={2:0},threads=1)",
                                        inputFile, fpsnum, fpsden));

            List<int> randomList = new List<int>();

            Random rand = new Random();
            for (int i = 0; i < 5; i++)
            {
                randomList.Add(rand.Next((int)Math.Round(streamLength * targetFps, 0)));
            }
            randomList.Sort();

            frameCount = 0;

            List<string> frameList = new List<string>();
            foreach (int frame in randomList)
            {
                int endFrame = frame + (int)Math.Round(targetFps * 5f, 0);
                sb.AppendLine(string.Format("Frame{0:0}=inStream.Trim({0:0},{1:0})", frame, endFrame));
                frameList.Add("Frame" + frame.ToString(AppSettings.CInfo));
                frameCount += (endFrame - frame);
            }

            string concString = frameList.Aggregate("combined=", (current, frameStr) => current + (frameStr + "+"));

            if (concString.EndsWith("+"))
                concString = concString.Remove(concString.Length - 1);
            
            sb.AppendLine(concString);
            sb.AppendLine("return combined");

            return WriteScript(sb.ToString());
        }

        public static string GenerateTestFile()
        {
            return WriteScript("Version()");
        }

        private static string WriteScript(string script, string extension = "avs")
        {
            Log.InfoFormat("Writing AviSynth script: {1:s}{0:s}", script, Environment.NewLine);

            string avsFile = Processing.CreateTempFile(extension);
            using (StreamWriter sw = new StreamWriter(avsFile))
            {
                sw.WriteLine(script);
            }

            return avsFile;
        }

        public static string GenerateAudioScript(string inputFile, string inFormat, string inFormatProfile, 
                                                 int inChannels, int outChannels, int inSampleRate, 
                                                 int outSampleRate, long length, float speed = 1)
        {
            StringBuilder sb = new StringBuilder();

            string ext = StreamFormat.GetFormatExtension(inFormat, inFormatProfile, false);

            switch (ext)
            {
                case "ac3":
                    sb.AppendLine(ImportNicAudio());
                    sb.AppendFormat(AppSettings.CInfo, "NicAC3Source(\"{0}\")", inputFile);
                    break;

                case "dts":
                case "dtshd":
                    sb.AppendLine(ImportNicAudio());
                    sb.AppendFormat(AppSettings.CInfo, "NicDTSSource(\"{0}\")", inputFile);
                    break;

                case "mp2":
                case "mp3":
                case "mpa":
                    sb.AppendLine(ImportNicAudio());
                    sb.AppendFormat(AppSettings.CInfo, "NicMPG123Source(\"{0}\")", inputFile);
                    break;
                default:
                    sb.AppendLine(ImportFFMPEGSource());
                    sb.AppendFormat(AppSettings.CInfo, "FFAudioSource(\"{0}\")", inputFile);
                    break;
            }
            sb.AppendLine();

            if (inChannels > outChannels && outChannels > 0)
            {
                sb.AppendLine(string.Format(AppSettings.CInfo, "Import(\"{0:s}\")",
                                            Path.Combine(AppSettings.AppPath, "AvsPlugins", "audio",
                                                         "ChannelDownMix.avsi")));

                switch (inChannels)
                {
                    case 3:
                        switch (outChannels)
                        {
                            case 2:
                                sb.AppendLine("Dmix3Stereo()");
                                break;
                            case 4:
                            case 3:
                                sb.AppendLine("Dmix3Dpl()");
                                break;
                            case 1:
                                sb.AppendLine("ConvertToMono()");
                                break;
                        }
                        break;
                    case 4:
                        switch (outChannels)
                        {
                            case 2:
                                sb.AppendLine("Dmix4qStereo()");
                                break;
                            case 3:
                                sb.AppendLine("Dmix4qDpl()");
                                break;
                            case 4:
                                sb.AppendLine("Dmix4qDpl2()");
                                break;
                            case 1:
                                sb.AppendLine("ConvertToMono()");
                                break;
                        }
                        break;
                    case 5:
                        switch (outChannels)
                        {
                            case 2:
                                sb.AppendLine("Dmix5Stereo()");
                                break;
                            case 3:
                                sb.AppendLine("Dmix5Dpl()");
                                break;
                            case 4:
                                sb.AppendLine("Dmix5Dpl2()");
                                break;
                            case 1:
                                sb.AppendLine("ConvertToMono()");
                                break;
                        }
                        break;
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                        switch (outChannels)
                        {
                            case 2:
                                sb.AppendLine("Dmix6StereoLfe()");
                                break;
                            case 3:
                                sb.AppendLine("Dmix6DplLfe()");
                                break;
                            case 4:
                                sb.AppendLine("Dmix6Dpl2Lfe()");
                                break;
                            case 1:
                                sb.AppendLine("ConvertToMono()");
                                break;
                            case 6:
                                sb.AppendLine("GetChannel(1,6)");
                                break;
                        }
                        break;
                }

            }

            if (inSampleRate != outSampleRate && outSampleRate > 0)
            {
                sb.AppendFormat(AppSettings.CInfo, "SSRC({0},fast=False)", outSampleRate);
                sb.AppendLine();
            }

            sb.AppendLine("return last");

            return WriteScript(sb.ToString());
        }

        public static string ImportNicAudio()
        {
            return string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                 Path.Combine(AppSettings.AppPath, "AvsPlugins", "audio", "NicAudio.dll"));
        }

        public static string ImportFFMPEGSource()
        {
            return string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                 Path.Combine(AppSettings.AppPath, "AvsPlugins", "ffms2.dll"));
        }
    }
}
