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
using System.IO;
using System.Text;
using VideoConvert.Core.Profiles;
using log4net;

namespace VideoConvert.Core.CommandLine
{
    class HcencCommandLineGenerator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HcencCommandLineGenerator));

        /// <summary>
        /// Generates ini file used for encoding an video stream to MPEG-2 format.
        /// </summary>
        /// <param name="encProfile">Encoder profile for HcEnc</param>
        /// <param name="inFile">Path to input file</param>
        /// <param name="outFile">Path to ouput file</param>
        /// <param name="aspect">Aspect ratio for output, valid values are: 0 (4:3), 1 (16:9), 2 (16:9 pan&amp;scan), 3 (1:1) or 4 (2.21:1)</param>
        /// <param name="bitrate">Target bitrate</param>
        /// <param name="maxBitrate">Maximal bitrate</param>
        /// <returns>Path to generated ini file</returns>
        public static string Generate(HcEncProfile encProfile, string inFile, string outFile, int aspect, int bitrate, int maxBitrate)
        {
            StringBuilder sb = new StringBuilder();
            if (encProfile != null)
            {
                sb.AppendLine("*INFILE      " + inFile);
                sb.AppendLine("*OUTFILE     " + outFile);

                if (encProfile.UseLosslessFile)
                {
                    sb.AppendLine("*LLPATH      " + AppSettings.DemuxLocation);
                    sb.AppendLine("*LOSSLESS");
                }

                int tempBitrate = bitrate;
                if (tempBitrate <= 0)
                    tempBitrate = encProfile.Bitrate;
                if (tempBitrate > maxBitrate)
                    tempBitrate = maxBitrate;

                sb.AppendLine("*BITRATE     " + tempBitrate.ToString(AppSettings.CInfo));
                sb.AppendLine("*MAXBITRATE  " + maxBitrate.ToString(AppSettings.CInfo));

                sb.Append("*PROFILE     ");
                switch (encProfile.Profile)
                {
                    case 0:
                        sb.Append("fast");
                        break;
                    case 2:
                        sb.Append("best");
                        break;
                }
                sb.AppendLine();

                string ar = string.Empty;
                switch (aspect)
                {
                    case 0:
                        ar = "4:3";
                        break;
                    case 1:
                        break;
                    case 2:
                        sb.AppendLine("*PANSCAN     540 576 0");
                        encProfile.Colorimetry = 3;
                        break;
                    case 3:
                        ar = "1:1";
                        break;
                    case 4:
                        ar = "2.21:1";
                        break;
                }
                if (!string.IsNullOrEmpty(ar))
                    sb.AppendLine("*ASPECT      " + ar);

                sb.Append(encProfile.AutoGOP ? "*AUTOGOP     " : "*GOP         ");

                sb.Append(encProfile.GopLength);
                if (!encProfile.AutoGOP)
                    sb.Append(" " + encProfile.BFrames.ToString(AppSettings.CInfo));
                sb.AppendLine();

                if (encProfile.AQ != 2)
                    sb.AppendLine("*AQ      " + encProfile.AQ.ToString(AppSettings.CInfo));

                sb.AppendLine("*DC_PREC      " + (encProfile.DCPrecision + 8).ToString(AppSettings.CInfo));

                switch (encProfile.Interlacing)
                {
                    case 0:
                        break;
                    case 1:
                        sb.AppendLine("*PROGRESSIVE");
                        break;
                    case 2:
                        sb.AppendLine("*INTERLACED");
                        break;
                    case 3:
                        sb.AppendLine("*DVSOURCE");
                        break;
                }

                if (encProfile.FieldOrder == 0 && encProfile.Interlacing == 3)
                    sb.AppendLine("*TFF");
                else if (encProfile.FieldOrder == 1)
                {
                    if (encProfile.Interlacing != 3)
                        sb.AppendLine("*BFF");
                }

                if (encProfile.ChromaDownsampling == 1)
                    sb.AppendLine("*CHROMADOWNSAMPLE 1");

                if (!encProfile.SceneChange)
                    sb.AppendLine("*NOSCD");

                if (!encProfile.SMP)
                    sb.AppendLine("*NOSMP");

                if (!encProfile.SeqEndCode)
                    sb.AppendLine("*NOVBV");

                if (encProfile.ClosedGops)
                    sb.AppendLine("*CLOSEDGOPS");

                if (encProfile.Allow3BFrames)
                    sb.AppendLine("*B3");

                if (encProfile.VBRBias > 0)
                    sb.AppendLine("*BIAS    " + encProfile.VBRBias.ToString(AppSettings.CInfo));

                if (encProfile.LastIFrame)
                    sb.AppendLine("*LASTIFRAME");

                switch (encProfile.MPGLevel)
                {
                    case 0:
                        break;
                    case 1:
                        sb.AppendLine("*MPEGLEVEL       MP@ML");
                        break;
                    case 2:
                        sb.AppendLine("*MPEGLEVEL       MP@H-14");
                        break;
                    case 3:
                        sb.AppendLine("*MPEGLEVEL       MP@HL");
                        break;
                }

                sb.Append("*INTRAVLC    ");
                switch (encProfile.IntraVLC)
                {
                    case 0:
                        sb.Append("2");
                        break;
                    case 1:
                        sb.Append("0");
                        break;
                    case 2:
                        sb.Append("1");
                        break;
                }
                sb.AppendLine();

                string[] matrices = {
                                        "MPEG", "QLB", "NOTCH", "BACH1", "HC", "HCLOW", "JAWOR1CD", "HVSGOOD", "HVSBETTER",
                                        "HVSBEST", "AVAMAT6", "AVAMAT7", "FOX1", "FOX2", "FOX3", "MANONO1", "MANONO2",
                                        "MANONO3", "MPEGSTD"
                                    };
                sb.AppendLine("*MATRIX      " + matrices[encProfile.Matrix].ToLower());

                if (encProfile.LuminanceGain > 0)
                    sb.Append("*LUMGAIN     " + encProfile.LuminanceGain.ToString(AppSettings.CInfo));

                if (encProfile.Colorimetry > 0)
                {
                    int color = encProfile.Colorimetry;
                    if (color > 1)
                        color += 2;
                    sb.Append("*COLOUR      " + color.ToString(AppSettings.CInfo));
                }

                sb.Append("*WAIT        0");

            }

            return WriteScript(sb.ToString());
        }

        private static string WriteScript(string content)
        {
            Log.InfoFormat("Writing hcenc ini-File: {1:s}{0:s}", content, Environment.NewLine);

            string iniFile = Processing.CreateTempFile("ini");
            using (StreamWriter sw = new StreamWriter(iniFile))
            {
                sw.WriteLine(content);
            }

            return iniFile;
        }
    }
}
