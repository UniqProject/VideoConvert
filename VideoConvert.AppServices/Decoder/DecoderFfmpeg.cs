// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DecoderFfmpeg.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Creates a ffmpeg decoding process with avisynth script as input, and outputs raw video to named pipe
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Decoder
{
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper class for creation of ffmpeg decoding processes
    /// </summary>
    public class DecoderFfmpeg
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DecoderFfmpeg));

        /// <summary>
        /// Executable filename
        /// </summary>
        private const string Executable = "ffmpeg.exe";

        /// <summary>
        /// <see cref="CultureInfo"/> for use at string formatting
        /// </summary>
        private static readonly CultureInfo CInfo = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");

        /// <summary>
        /// <see cref="Regex"/> which helps to reduce decoder verbosity in the log file
        /// </summary>
        private static readonly Regex FrameReg = new Regex(@"^.*frame=\s*(\d*).*$",
            RegexOptions.Singleline | RegexOptions.Multiline);

        /// <summary>
        /// Creates an ffmpeg process for decoding of given avisynth script
        /// </summary>
        /// <param name="scriptName">AviSynth script to decode</param>
        /// <param name="useScaling">Make use of ffmpeg internal scaling</param>
        /// <param name="originalSize">The size of input video</param>
        /// <param name="fromAr">Original Aspectratio</param>
        /// <param name="cropRect">Crop Rectangle</param>
        /// <param name="resize">Target Size</param>
        /// <param name="toolPath">Path to executable</param>
        /// <param name="pipeName">Name of the pipe for output</param>
        /// <returns>Created <see cref="Process"/></returns>
        public static Process CreateDecodingProcess(string scriptName, bool useScaling, Size originalSize, float fromAr,
            Rectangle cropRect, Size resize, string toolPath, string pipeName)
        {
            var localExecutable = Path.Combine(toolPath, Executable);

            var filterArray = new List<string>();

            var filterChain = string.Empty;

            if (useScaling)
            {
                if (!cropRect.IsEmpty)
                {
                    int temp;
                    Math.DivRem(cropRect.X, 2, out temp);
                    cropRect.X += temp;
                    Math.DivRem(cropRect.Y, 2, out temp);
                    cropRect.Y += temp;
                    Math.DivRem(cropRect.Width, 2, out temp);
                    cropRect.Width += temp;
                    Math.DivRem(cropRect.Height, 2, out temp);
                    cropRect.Height += temp;

                    if ((cropRect.X > 0) || (cropRect.Y > 0) || (cropRect.Width < originalSize.Width) ||
                        (cropRect.Height < originalSize.Height))
                    {
                        filterArray.Add(string.Format("crop={0:D}:{1:D}:{2:D}:{3:D}", cropRect.Width, cropRect.Height, cropRect.X, cropRect.Y));
                    }
                }
                var calculatedWidth = originalSize.Width;
                var calculatedHeight = originalSize.Height;

                if (!resize.IsEmpty)
                {
                    var toAr = (float)Math.Round(resize.Width / (float)resize.Height, 3);
                    fromAr = (float)Math.Round(fromAr, 3);
                    int temp;
                    if (fromAr > toAr) // source aspectratio higher than target aspectratio
                    {

                        calculatedWidth = resize.Width;
                        calculatedHeight = (int)(calculatedWidth / fromAr);

                        Math.DivRem(calculatedWidth, 2, out temp);
                        calculatedWidth += temp;
                        Math.DivRem(calculatedHeight, 2, out temp);
                        calculatedHeight += temp;
                    }
                    else if (Math.Abs(fromAr - toAr) <= 0)  // source and target aspectratio equals
                    {
                        calculatedWidth = resize.Width;
                        calculatedHeight = (int)(calculatedWidth / toAr);

                        Math.DivRem(calculatedWidth, 2, out temp);
                        calculatedWidth += temp;
                        Math.DivRem(calculatedHeight, 2, out temp);
                        calculatedHeight += temp;
                    }
                    else
                    {
                        calculatedHeight = resize.Height;
                        calculatedWidth = (int)(calculatedHeight / toAr);

                        Math.DivRem(calculatedWidth, 2, out temp);
                        calculatedWidth += temp;
                        Math.DivRem(calculatedHeight, 2, out temp);
                        calculatedHeight += temp;
                    }

                    filterArray.Add(string.Format("scale={0:D}:{1:D}", calculatedWidth, calculatedHeight));
                }

                if (!resize.IsEmpty && (calculatedHeight < resize.Height || calculatedWidth < resize.Width))
                {
                    var posLeft = (int)Math.Ceiling((decimal)(resize.Width - calculatedWidth) / 2);
                    var posTop = (int)Math.Ceiling((decimal)(resize.Height - calculatedHeight) / 2);
                    filterArray.Add(string.Format("pad={0:D}:{1:D}:{2:D}:{3:D}", resize.Width, resize.Height,
                        posLeft > 0 ? posLeft : 0, posTop > 0 ? posTop : 0));
                }
            }

            if (filterArray.Count > 0)
            {
                filterChain = string.Format("-vf \"{0}\" ", string.Join(",", filterArray));
            }

            var info = new ProcessStartInfo
            {
                FileName = localExecutable,
                Arguments =
                    String.Format(CInfo, "-i \"{0}\" {1} -f yuv4mpegpipe -y \"{2}\"",
                                  scriptName, filterChain, pipeName),
                CreateNoWindow = true,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            var ffmpeg = new Process { StartInfo = info };

            ffmpeg.ErrorDataReceived += DecodeOnErrorDataReceived;

            Log.Info("ffmpeg decoding process created!");
            Log.Info("params: ffmpeg " + ffmpeg.StartInfo.Arguments);

            return ffmpeg;
        }

        private static void DecodeOnErrorDataReceived(object sender, DataReceivedEventArgs args)
        {
            var line = args.Data;
            if (string.IsNullOrEmpty(line)) return;

            var frameResult = FrameReg.Match(line);
            if (!frameResult.Success)
                Log.Info(line);
        }
    }
}