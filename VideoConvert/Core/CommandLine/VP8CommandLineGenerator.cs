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
using VideoConvert.Core.Profiles;

namespace VideoConvert.Core.CommandLine
{
    class VP8CommandLineGenerator
    {
        public static string Generate(VP8Profile inProfile, int bitrate, int hRes, int vRes, int pass, int fpsN, int fpsD,
                                      bool preview = false, VideoFormat format = VideoFormat.Unknown, string outFile = "output")
        {
            StringBuilder sb = new StringBuilder();
            if (inProfile != null)
            {
                int tempPass = pass;

                if (preview)
                    sb.Append("program ");

                sb.Append("--debug --codec=vp8 ");
                sb.AppendFormat(AppSettings.CInfo, "--width={0:g} --height={1:g} ", hRes, vRes);

                sb.AppendFormat(AppSettings.CInfo, " --deadline={0:g} ", inProfile.DeadlinePerFrame);

                sb.Append("--passes=");
                switch (inProfile.EncodingMode)
                {
                    case 0:
                        sb.Append("1 ");
                        break;
                    case 1:
                        sb.Append("2 ");
                        if (preview)
                            tempPass = 2;
                        sb.AppendFormat(AppSettings.CInfo, "--pass={0:g} ", tempPass);
                        break;
                }

                int tempBitrate = bitrate;
                if (tempBitrate <= 0)
                    tempBitrate = inProfile.Bitrate;
                sb.AppendFormat(AppSettings.CInfo, "--target-bitrate={0:g} ", tempBitrate);

                sb.AppendFormat(AppSettings.CInfo, "--end-usage={0:g} ", inProfile.BitrateMode);

                if (inProfile.EncodingMode == 1)
                {
                    string fpfFile = Processing.CreateTempFile(outFile, "stats");
                    sb.AppendFormat("--fpf=\"{0}\" ", fpfFile);
                }

                sb.AppendFormat(AppSettings.CInfo, "--profile={0:g} ", inProfile.Profile);

                switch (inProfile.SpeedControl)
                {
                    case 0:
                        sb.Append("--rt ");
                        break;
                    case 1:
                        sb.Append("--good ");
                        break;
                    case 2:
                        sb.Append("--best ");
                        break;
                }

                sb.AppendFormat(AppSettings.CInfo, "--cpu-used={0:g} ", inProfile.CPUModifier);
                sb.AppendFormat(AppSettings.CInfo, "--token-parts={0:g} ", inProfile.TokenPart);
                sb.AppendFormat(AppSettings.CInfo, "--noise-sensitivity={0:g} ", inProfile.NoiseFiltering);
                sb.AppendFormat(AppSettings.CInfo, "--sharpness={0:g} ", inProfile.Sharpness);

                int tempThreads = inProfile.Threads;
                if (tempThreads == 0)
                    tempThreads = Environment.ProcessorCount*2;
                sb.AppendFormat(AppSettings.CInfo, "--threads={0:g} ", tempThreads);

                sb.AppendFormat(AppSettings.CInfo, "--static-thresh={0:g} ", inProfile.StaticThreshold);

                sb.Append("--error-resilient=");
                sb.Append(inProfile.UseErrorResilience ? "1 " : "0 ");

                sb.AppendFormat(AppSettings.CInfo, "--kf-min-dist={0:g} --kf-max-dist={1:g} ", 
                                inProfile.GopMin, inProfile.GopMax);

                sb.AppendFormat(AppSettings.CInfo, "--lag-in-frames={0:g} ", inProfile.MaxFramesLag);
                sb.AppendFormat(AppSettings.CInfo, "--drop-frame={0:g} ", inProfile.FrameDrop);

                if (inProfile.UseSpatialResampling)
                    sb.AppendFormat(AppSettings.CInfo, "--resize-allowed=1 --resize-up={0:g} --resize-down={1:g} ",
                                    inProfile.UpscaleThreshold, inProfile.DownscaleThreshold);
                else
                    sb.Append("--resize-allowed=0 ");

                if (inProfile.UseArnrFrameDecision)
                    sb.AppendFormat(AppSettings.CInfo, "--auto-alt-ref=1 --arnr-maxframes={0:g} --arnr-strength={1:g} ",
                                    inProfile.ArnrMaxFrames, inProfile.ArnrStrength);
                else
                    sb.Append("--auto-alt-ref=0 ");

                sb.AppendFormat(AppSettings.CInfo, "--buf-initial-sz={0:g} ", inProfile.InitialBufferSize);
                sb.AppendFormat(AppSettings.CInfo, "--buf-optimal-sz={0:g} ", inProfile.OptimalBufferSize);
                sb.AppendFormat(AppSettings.CInfo, "--buf-sz={0:g} ", inProfile.BufferSize);
                sb.AppendFormat(AppSettings.CInfo, "--undershoot-pct={0:g} ", inProfile.UndershootDataRate);

                sb.AppendFormat(AppSettings.CInfo, "--min-q={0:g} --max-q={1:g} ",
                                inProfile.QuantizerMin, inProfile.QuantizerMax);

                sb.AppendFormat(AppSettings.CInfo, "--bias-pct={0:g} ", inProfile.BiasFrameAdjust);
                sb.AppendFormat(AppSettings.CInfo, "--minsection-pct={0:g} --maxsection-pct={1:g} ", 
                                inProfile.SectionMin, inProfile.SectionMax);

                if (inProfile.EncodingMode == 1 && tempPass == 1)
                    sb.Append("-o NUL ");
                else if (!String.IsNullOrEmpty(outFile))
                    sb.AppendFormat("-o \"{0}\" ", outFile);

                sb.Append("- ");
            }
            return sb.ToString();
        }
    }
}