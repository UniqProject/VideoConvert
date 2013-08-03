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
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(AviSynthGenerator));

        /// <summary>
        /// Contains path to config file used by h264StereoSource plugin for AviSynth
        /// </summary>
        public static string StereoConfigFile = string.Empty;

        /// <summary>
        /// Generates AviSynth script used for video encoding
        /// </summary>
        /// <param name="videoInfo">All video properties</param>
        /// <param name="changeFps">Defines whether framerate should be changed</param>
        /// <param name="targetFps">Sets target framerate</param>
        /// <param name="resizeTo">Sets target video resolution</param>
        /// <param name="stereoEncoding">Defines, which stereo encoding mode should be used</param>
        /// <param name="stereoVideoInfo">Sets all parameters for stereo encoding</param>
        /// <param name="isDvdResolution">Defines whether target resolution is used for DVD encoding</param>
        /// <param name="subtitleFile">Sets subtitle file for hardcoding into video</param>
        /// <param name="subtitleOnlyForced">Defines whether only forced captions should be hardcoded</param>
        /// <returns>Path to AviSynth script</returns>
        public static string Generate(VideoInfo videoInfo, bool changeFps, float targetFps, Size resizeTo,
                                      StereoEncoding stereoEncoding, StereoVideoInfo stereoVideoInfo, bool isDvdResolution, string subtitleFile, bool subtitleOnlyForced, bool skipScaling)
        {
            StringBuilder sb = new StringBuilder();

            bool mtUseful = (videoInfo.Interlaced && AppSettings.UseHQDeinterlace) || changeFps;

            bool useStereo = stereoEncoding != StereoEncoding.None && stereoVideoInfo.RightStreamId > -1;

            // support for multithreaded AviSynth
            if (AppSettings.UseAviSynthMT && mtUseful)
            {
                sb.AppendLine("SetMTMode(2,0)");
                sb.AppendLine("SetMemoryMax(512)");
            }

            //loading plugins
            sb.AppendLine(ImportFFMPEGSource());  // ffms2

            if (changeFps || (videoInfo.Interlaced && AppSettings.UseHQDeinterlace))
                sb.AppendLine(string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                            Path.Combine(AppSettings.AppPath, "AvsPlugins", "mvtools2.dll")));

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
            else if (videoInfo.Interlaced)
            {
                sb.AppendLine(string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                            Path.Combine(AppSettings.AppPath, "AvsPlugins", "Decomb.dll")));
            }

            if (useStereo)
                sb.AppendLine(string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                            Path.Combine(AppSettings.AppPath, "AvsPlugins", "H264StereoSource.dll")));

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

            // calculate framerate numerator & denominator
            if (videoInfo.FPS <= 0)
            {
                MediaInfoContainer mi = new MediaInfoContainer();
                try
                {
                     mi = Processing.GetMediaInfo(videoInfo.TempFile);
                }
                catch (TimeoutException ex)
                {
                    Log.Error(ex);
                    mi = new MediaInfoContainer();
                }
                finally
                {
                    if (mi.Video.Count > 0)
                    {
                        videoInfo.FPS = mi.Video[0].FrameRate;
                        Processing.GetFPSNumDenom(videoInfo.FPS, out videoInfo.FrameRateEnumerator,
                                                  out videoInfo.FrameRateDenominator);

                        if (videoInfo.FrameRateEnumerator == 0)
                        {
                            videoInfo.FrameRateEnumerator = (int)Math.Round(videoInfo.FPS) * 1000;
                            videoInfo.FrameRateDenominator =
                                (int)(Math.Round(Math.Ceiling(videoInfo.FPS) - Math.Floor(videoInfo.FPS)) + 1000);
                        }
                    }
                }
                
            }

            if (videoInfo.FrameRateEnumerator > 0 && videoInfo.FrameRateDenominator > 0)
                sb.AppendLine(string.Format(AppSettings.CInfo,
                    "FFVideoSource(\"{0:s}\",fpsnum={1:0},fpsden={2:0},threads={3:0})",
                    videoInfo.TempFile, videoInfo.FrameRateEnumerator,
                    videoInfo.FrameRateDenominator, AppSettings.LimitDecoderThreads ? 1 : 0));
            else
                sb.AppendLine(string.Format(AppSettings.CInfo, "FFVideoSource(\"{0:s}\",threads={1:0})",
                    videoInfo.TempFile, AppSettings.LimitDecoderThreads ? 1 : 0));

            string stereoVar = string.Empty;

            if (useStereo)
            {
                string configFile = GenerateStereoSourceConfig(stereoVideoInfo);
                sb.AppendLine(string.Format(AppSettings.CInfo, "VideoRight = H264StereoSource(\"{0:s}\",{1:g})",
                                            configFile, videoInfo.FrameCount - 50));
                StereoConfigFile = configFile;
                stereoVar = "VideoRight";
            }

            // deinterlace video source
            if (videoInfo.Interlaced)
            {
                if (AppSettings.UseHQDeinterlace)
                    sb.AppendLine("QTGMC(Preset=\"Slower\")");
                else
                {
                    sb.AppendLine("ConvertToYUY2(interlaced=true)");
                    sb.AppendLine("Telecide(post=4)");
                    sb.AppendLine("Crop(4, 0, -4, 0)");
                    sb.AppendLine("AddBorders(4, 0, 4, 0)");
                    sb.AppendLine("ConvertToYV12()");
                }
            }

            // hardcode subtitles
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

            // video cropping
            if (!videoInfo.CropRect.IsEmpty && !skipScaling)
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

            // Side-By-Side stereo encoding
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

            // video resizing
            if (!resizeTo.IsEmpty && (resizeTo.Height != videoInfo.Height || resizeTo.Width != videoInfo.Width) && !skipScaling)
            {
                // aspect ratios

                float toAr = (float) Math.Round(resizeTo.Width / (float)resizeTo.Height, 3);
                float fromAr = videoInfo.AspectRatio;
                float mod = 1f;

                calculatedWidth = resizeTo.Width;

                if (fromAr > toAr) // source aspectratio higher than target aspectratio
                {
                    if (isDvdResolution)
                    {
                        calculatedHeight = (int)(calculatedWidth / fromAr);
                        if (calculatedHeight > resizeTo.Height)
                            calculatedHeight = resizeTo.Height;
                        calculatedWidth = 720;
                    }
                    else
                    {
                        calculatedWidth = resizeTo.Width;
                        calculatedHeight = (int)(calculatedWidth / fromAr);
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
                else if (Math.Abs(fromAr - toAr) <= 0)  // source and target aspectratio equals
                {
                    if (isDvdResolution)
                    {
                        calculatedHeight = (int)(calculatedWidth / fromAr);
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
                    if (fromAr > 1.4f && isDvdResolution)  // source aspectratio not 4:3, encoding for dvd resolution
                    {
                        mod = 720f/resizeTo.Width;

                        calculatedHeight = (int)(calculatedWidth / fromAr);
                        if (calculatedHeight > resizeTo.Height)
                        {
                            calculatedHeight = resizeTo.Height;
                            calculatedWidth = (int)(calculatedHeight * fromAr * mod);
                        }
                        else
                            calculatedWidth = 720;
                    }
                    else if (isDvdResolution)
                    {
                        calculatedHeight = resizeTo.Height;
                        calculatedWidth = (int) (calculatedHeight*fromAr);
                    }
                    else
                        calculatedHeight = resizeTo.Height;

                    int temp;
                    Math.DivRem(calculatedWidth, 2, out temp);
                    calculatedWidth += temp;
                    Math.DivRem(calculatedHeight, 2, out temp);
                    calculatedHeight += temp;

                    if (Math.Abs(toAr - 1.778f) <= 0)     // aspectratio 16:9
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

            // apply resize filter
            if (calculatedHeight != videoInfo.Height || calculatedWidth != videoInfo.Width ||
                (stereoEncoding == StereoEncoding.HalfSideBySideLeft ||
                 stereoEncoding == StereoEncoding.HalfSideBySideRight
                && useStereo) && !skipScaling)
            {
                if (calculatedHeight < videoInfo.Height || calculatedWidth < videoInfo.Width ||
                    (stereoEncoding == StereoEncoding.HalfSideBySideLeft ||
                     stereoEncoding == StereoEncoding.HalfSideBySideRight
                     && useStereo))
                    sb.AppendLine(string.Format(AppSettings.CInfo, "BicubicResize({0:g},{1:g})",
                                                calculatedWidth,
                                                calculatedHeight));
                else
                    sb.AppendLine(string.Format(AppSettings.CInfo, "Lanczos4Resize({0:g},{1:g})",
                                                calculatedWidth,
                                                calculatedHeight));
            }

            // add borders if needed
            if (addBorders && (borderLeft > 0 || borderRight > 0 || borderTop > 0 || borderBottom > 0) && !skipScaling)
                sb.AppendLine(string.Format(AppSettings.CInfo, "AddBorders({0:g},{1:g},{2:g},{3:g})",
                                            borderLeft,
                                            borderTop,
                                            borderRight,
                                            borderBottom));

            // change framerate
            if (changeFps)
            {
                int fpsnum;
                int fpsden;

                // get framerate numerator & denominator for target framerate
                Processing.GetFPSNumDenom(targetFps, out fpsnum, out fpsden);

                // source is 23.976 or 24 fps
                if (videoInfo.FrameRateEnumerator == 24000 && (videoInfo.FrameRateDenominator == 1001 || videoInfo.FrameRateDenominator == 1000))
                {
                    if (fpsnum == 30000 && fpsden == 1001)
                    {
                        // 3:2 pulldown / telecine
                        sb.AppendLine("AssumeFrameBased()");
                        sb.AppendLine("SeparateFields()");
                        sb.AppendLine("SelectEvery(8, 0, 1, 2, 3, 2, 5, 4, 7, 6, 7)");
                        sb.AppendLine("Weave()");
                    }
                    else if (fpsnum == 25000 && fpsden == 1000)
                    {
                        // convert to 25 fps
                        sb.AppendLine("ConvertToYUY2()");
                        sb.AppendLine("ConvertFPS(50)");
                        sb.AppendLine("AssumeTFF()");
                        sb.AppendLine("SeparateFields()");
                        sb.AppendLine("SelectEvery(4,0,3)");
                        sb.AppendLine("Weave()");
                        sb.AppendLine("ConvertToYV12()");
                    }
                }
                // source is 30fps
                else if (videoInfo.FrameRateEnumerator == 30000)
                {
                    sb.AppendLine("ConvertToYUY2()");
                    sb.AppendLine("DoubleWeave()");
                    sb.AppendLine(string.Format(AppSettings.CInfo, "ConvertFPS({0:0.000})", targetFps*2));
                    sb.AppendLine("SelectEven()");
                    sb.AppendLine("ConvertToYV12()");
                }
                // source is 25fps
                else if (videoInfo.FrameRateEnumerator == 25000 && videoInfo.FrameRateDenominator == 1000)
                {
                    if ((fpsnum == 30000 || fpsnum == 24000) && fpsden == 1001)
                    {
                        sb.AppendLine("ConvertToYUY2()");
                        sb.AppendLine(string.Format(AppSettings.CInfo, "ConvertFPS({0:0.000}*2)", 23.976));
                        if (fpsnum == 30000)
                        {
                            sb.AppendLine("AssumeFrameBased()");
                            sb.AppendLine("SeparateFields()");
                            sb.AppendLine("SelectEvery(8, 0, 1, 2, 3, 2, 5, 4, 7, 6, 7)");
                        }
                        else
                        {
                            sb.AppendLine("AssumeTFF()");
                            sb.AppendLine("SeparateFields()");
                            sb.AppendLine("SelectEven()");
                        }
                        sb.AppendLine("Weave()");
                        sb.AppendLine("ConvertToYV12()");
                    }
                }
                // every other framerate
                else
                {
                    // very slow framerate interpolation
                    sb.AppendLine("super = MSuper(pel=2)");
                    sb.AppendLine("backward_vec = MAnalyse(super, overlap=4, isb = true, search=3)");
                    sb.AppendLine("forward_vec = MAnalyse(super, overlap=4, isb = false, search=3)");
                    sb.AppendFormat("MFlowFps(super, backward_vec, forward_vec, num={0:0}, den={1:0})", fpsnum,
                                    fpsden);
                }

                sb.AppendLine();
            }

            // multithreaded avisynth
            if (AppSettings.UseAviSynthMT && mtUseful)
            {
                sb.AppendLine("SetMTMode(1)");
                sb.AppendLine("GetMTMode(false) > 0 ? distributor() : last");
            }

            return WriteScript(sb.ToString());
        }

        /// <summary>
        /// Generates configuration file used by H264StereoSource plugin
        /// </summary>
        /// <param name="stereoVideoInfo"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates AviSynth script used to determine black borders for cropping
        /// </summary>
        /// <param name="inputFile">Path to source file</param>
        /// <param name="targetFps">Sets framerate of the source file</param>
        /// <param name="streamLength">Sets duration of the source file, in seconds</param>
        /// <param name="frameCount">Calculated amount of frames</param>
        /// <returns>Path to AviSynth script</returns>
        public static string GenerateCropDetect(string inputFile, float targetFps, double streamLength, out int frameCount)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(ImportFFMPEGSource()); // ffms2

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
                randomList.Add(rand.Next((int) Math.Round(streamLength*targetFps, 0)));

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

        /// <summary>
        /// Generates simple script used to check whether AviSynth is installed on system
        /// </summary>
        /// <returns>Path to AviSynth script</returns>
        public static string GenerateTestFile()
        {
            return WriteScript("Version()");
        }

        /// <summary>
        /// Writes script content to file and returns the path of written file
        /// </summary>
        /// <param name="script">Script content</param>
        /// <param name="extension">File extension of the file, default "avs"</param>
        /// <returns>Path of written file</returns>
        private static string WriteScript(string script, string extension = "avs")
        {
            Log.InfoFormat("Writing AviSynth script: {1:s}{0:s}", script, Environment.NewLine);

            string avsFile = Processing.CreateTempFile(extension);
            using (StreamWriter sw = new StreamWriter(avsFile, false, Encoding.ASCII))
                sw.WriteLine(script);

            return avsFile;
        }

        /// <summary>
        /// Generates AviSynth script used for audio encoding
        /// </summary>
        /// <param name="inputFile">Path to input file</param>
        /// <param name="inFormat">Format of input file</param>
        /// <param name="inFormatProfile">Format profile of input file</param>
        /// <param name="inChannels">Channel count of input file</param>
        /// <param name="outChannels">Target channel count</param>
        /// <param name="inSampleRate">Samplerate of input file</param>
        /// <param name="outSampleRate">Target samplerate</param>
        /// <returns>Path to AviSynth script</returns>
        public static string GenerateAudioScript(string inputFile, string inFormat, string inFormatProfile, 
                                                 int inChannels, int outChannels, int inSampleRate, 
                                                 int outSampleRate)
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
                                sb.AppendLine("GetChannel(1,2,3,4,5,6)");
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

        /// <summary>
        /// Imports NicAudio plugin
        /// </summary>
        /// <returns></returns>
        public static string ImportNicAudio()
        {
            return string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                 Path.Combine(AppSettings.AppPath, "AvsPlugins", "audio", "NicAudio.dll"));
        }

        /// <summary>
        /// Imports ffmpegsource (ffms2) plugin
        /// </summary>
        /// <returns></returns>
        public static string ImportFFMPEGSource()
        {
            return string.Format(AppSettings.CInfo, "LoadPlugin(\"{0:s}\")",
                                 Path.Combine(AppSettings.AppPath, "AvsPlugins", "ffms2.dll"));
        }
    }
}
