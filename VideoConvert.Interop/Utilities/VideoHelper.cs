// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VideoHelper.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Helper class for video streams
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using Model;
    using Model.MediaInfo;
    using Model.Profiles;

    /// <summary>
    /// Helper class for video streams
    /// </summary>
    public class VideoHelper
    {
        private static readonly CultureInfo CInfo = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");

        /// <summary>
        /// Read stream info
        /// </summary>
        /// <param name="mi"><see cref="MediaInfoContainer"/></param>
        /// <param name="vStream">Video stream</param>
        /// <param name="bluRayTarget"></param>
        /// <returns></returns>
        public static VideoInfo GetStreamInfo(MediaInfoContainer mi, VideoInfo vStream, bool bluRayTarget)
        {
            if (mi.Video.Count > 0)
            {
                Single.TryParse(mi.Video[0].DisplayAspectRatio, NumberStyles.Number, CInfo,
                                out vStream.AspectRatio);
                vStream.Bitrate = mi.Video[0].BitRate;
                vStream.Format = mi.Video[0].Format;
                vStream.FormatProfile = mi.Video[0].FormatProfile;
                if (mi.Video[0].FrameRateEnumerator < vStream.FrameRateEnumerator*2 || !bluRayTarget)
                {
                    vStream.FPS = mi.Video[0].FrameRate;
                    vStream.FrameCount = mi.Video[0].FrameCount;
                    vStream.FrameRateDenominator = mi.Video[0].FrameRateDenominator;
                    vStream.FrameRateEnumerator = mi.Video[0].FrameRateEnumerator;

                }
                vStream.Height = mi.Video[0].Height;
                vStream.Width = mi.Video[0].Width;
                vStream.Interlaced = mi.Video[0].ScanType != "Progressive";
                vStream.Length = mi.Video[0].DurationTime.TimeOfDay.TotalSeconds;
                vStream.PicSize = mi.Video[0].VideoSize;
                vStream.StreamSize = GenHelper.GetFileSize(vStream.TempFile);
                vStream.FrameMode = mi.Video[0].FormatFrameMode;
            }
            return vStream;
        }

        /// <summary>
        /// Get output resolution for encode job
        /// </summary>
        /// <param name="encodeInfo"></param>
        /// <returns></returns>
        public static Size GetTargetSize(EncodeInfo encodeInfo)
        {
            var resizeTo = new Size {Width = encodeInfo.VideoStream.Width, Height = encodeInfo.VideoStream.Height};

            if ((!encodeInfo.VideoStream.CropRect.IsEmpty) && (!encodeInfo.EncodingProfile.KeepInputResolution))
            {
                resizeTo.Height = encodeInfo.VideoStream.CropRect.Height;
                resizeTo.Width = encodeInfo.VideoStream.CropRect.Width;
            }

            if ((encodeInfo.EncodingProfile.OutFormat == OutputType.OutputBluRay) ||
                (encodeInfo.EncodingProfile.OutFormat == OutputType.OutputAvchd))
                resizeTo = GetVideoDimensions(encodeInfo.VideoStream.PicSize, encodeInfo.VideoStream.AspectRatio,
                                                         encodeInfo.EncodingProfile.OutFormat);
            else if ((!encodeInfo.EncodingProfile.KeepInputResolution) && (encodeInfo.EncodingProfile.TargetWidth > 0))
            {
                resizeTo.Width = encodeInfo.EncodingProfile.TargetWidth;
                if (!encodeInfo.VideoStream.CropRect.IsEmpty)
                {
                    var aspectRatio = (double) encodeInfo.VideoStream.CropRect.Width/
                                         encodeInfo.VideoStream.CropRect.Height;
                    resizeTo.Height = (int) Math.Ceiling(resizeTo.Width/aspectRatio);
                }
                else
                {
                    resizeTo.Height = (int) Math.Ceiling(resizeTo.Width/encodeInfo.VideoStream.AspectRatio);
                }

                int temp;
                Math.DivRem(resizeTo.Height, 2, out temp);
                resizeTo.Height += temp;

                Math.DivRem(resizeTo.Width, 2, out temp);
                resizeTo.Width += temp;
            }
            return resizeTo;
        }

        /// <summary>
        /// Get video dimensions
        /// </summary>
        /// <param name="videoFormat"></param>
        /// <param name="aspect">Aspect ratio</param>
        /// <param name="outType"></param>
        /// <returns></returns>
        public static Size GetVideoDimensions(VideoFormat videoFormat, float aspect, OutputType outType)
        {
            var outPut = new Size();
            var aspect16To9 = Math.Round(16 / (double)9, 3);
            var aspect16To10 = Math.Round(16 / (double)10, 3);

            switch (videoFormat)
            {
                case VideoFormat.Videoformat480I:
                case VideoFormat.Videoformat480P:
                    if ((Math.Abs(Math.Round(aspect, 3) - aspect16To9) < 0) && (outType == OutputType.OutputDvd))
                    {
                        outPut.Width = 1024;
                    }
                    else
                    {
                        outPut.Width = 720;
                    }
                    outPut.Height = 480;
                    break;
                case VideoFormat.Videoformat576I:
                case VideoFormat.Videoformat576P:
                    if ((Math.Abs(Math.Round(aspect, 3) - aspect16To9) < 0) && (outType == OutputType.OutputDvd))
                    {
                        outPut.Width = 1024;
                    }
                    else
                    {
                        outPut.Width = 720;
                    }
                    outPut.Height = 576;
                    break;
                case VideoFormat.Videoformat720P:
                    outPut.Width = 1280;
                    outPut.Height = 720;
                    break;
                case VideoFormat.Videoformat1080I:
                case VideoFormat.Videoformat1080P:
                    if (Math.Round(aspect, 3) >= aspect16To9 || Math.Round(aspect, 3) >= aspect16To10)
                    {
                        outPut.Width = 1920;
                    }
                    else
                    {
                        outPut.Width = 1440;
                    }
                    outPut.Height = 1080;
                    break;
            }

            return outPut;
        }

        /// <summary>
        /// Get Framerate enumerator and denominator from given decimal
        /// </summary>
        /// <param name="fps">Source fps</param>
        /// <param name="fpsEnumerator">Calculated Enumerator</param>
        /// <param name="fpsDenominator">Calculated demominator</param>
        public static void GetFpsNumDenom(float fps, out int fpsEnumerator, out int fpsDenominator)
        {
            var tempFrameRate = Convert.ToInt32(Math.Round(fps, 3) * 1000);

            fpsEnumerator = 0;
            fpsDenominator = 0;

            switch (tempFrameRate)
            {
                case 23976: // 23.976
                    fpsEnumerator = 24000;
                    fpsDenominator = 1001;
                    break;
                case 24000: // 24
                    fpsEnumerator = 24000;
                    fpsDenominator = 1000;
                    break;
                case 25000: // 25
                    fpsEnumerator = 25000;
                    fpsDenominator = 1000;
                    break;
                case 29970: // 29.97
                    fpsEnumerator = 30000;
                    fpsDenominator = 1001;
                    break;
                case 30000: // 30
                    fpsEnumerator = 30000;
                    fpsDenominator = 1000;
                    break;
                case 50000: // 50
                    fpsEnumerator = 50000;
                    fpsDenominator = 1000;
                    break;
                case 59940: // 59.94
                    fpsEnumerator = 60000;
                    fpsDenominator = 1001;
                    break;
                case 60000: // 60
                    fpsEnumerator = 60000;
                    fpsDenominator = 1000;
                    break;
                case 0: // Forbidden
                    break;
                default: // Reserved
                    fpsEnumerator = tempFrameRate;
                    fpsDenominator = 1000;
                    break;
            }
        }

        // TODO: check overhead calculation
        /// <summary>
        /// Calculate target bitrate for video stream
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns></returns>
        public static int CalculateVideoBitrate(EncodeInfo jobInfo)
        {
            const double tsOverhead = 0.1D; // 10%
            const double mkvOverhead = 0.005D; // 0.5%
            const double dvdOverhead = 0.04D; // 4%

            var targetSize = jobInfo.EncodingProfile.TargetFileSize;
            var overhead = 0D;

            switch (jobInfo.EncodingProfile.OutFormat)
            {
                case OutputType.OutputBluRay:
                    overhead = tsOverhead;
                    break;
                case OutputType.OutputAvchd:
                    overhead = tsOverhead;
                    break;
                case OutputType.OutputMatroska:
                case OutputType.OutputWebM:
                    overhead = mkvOverhead;
                    break;
                case OutputType.OutputMp4:
                    break;
                case OutputType.OutputTs:
                case OutputType.OutputM2Ts:
                    overhead = tsOverhead;
                    break;
                case OutputType.OutputDvd:
                    overhead = dvdOverhead;
                    break;
            }

            ulong streamSizes = 0;

            var maxRate = -1;

            if (jobInfo.VideoProfile.Type == ProfileType.X264)
                maxRate = CalculateMaxRatex264((X264Profile)jobInfo.VideoProfile, jobInfo.EncodingProfile.OutFormat);

            foreach (var item in jobInfo.AudioStreams)
            {
                streamSizes += item.StreamSize + (ulong)Math.Floor(item.StreamSize * overhead);
                if ((item.IsHdStream) && (Math.Abs(overhead - tsOverhead) <= 0))
                    streamSizes += (ulong)Math.Floor(item.StreamSize * 0.03D);
            }
            streamSizes = jobInfo.SubtitleStreams.Aggregate(streamSizes,
                                                            (current, item) =>
                                                            current +
                                                            (item.StreamSize +
                                                             (ulong)Math.Floor(item.StreamSize * overhead)));

            var sizeRemains = targetSize - streamSizes;

            sizeRemains -= (ulong)Math.Floor(sizeRemains * overhead);
            var bitrateCalc = (int)Math.Floor(sizeRemains / jobInfo.VideoStream.Length / 1000 * 8);

            if (jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                bitrateCalc = Math.Min(bitrateCalc, 8000);

            if (maxRate > -1)
                bitrateCalc = Math.Min(bitrateCalc, maxRate);

            return bitrateCalc;
        }

        /// <summary>
        /// Calculate max allowed bitrate
        /// </summary>
        /// <param name="x264Prof">Encoding profile</param>
        /// <param name="outType">Target type</param>
        /// <returns>Max allowed bitrate</returns>
        public static int CalculateMaxRatex264(X264Profile x264Prof, OutputType outType)
        {
            int[] baseLineBitrates =
            {
                64, 192, 384, 786, 2000, 4000, 4000, 10000, 14000, 20000, 20000, 50000, 50000,
                135000, 240000, -1
            };

            int[] highBitrates =
            {
                80, 240, 480, 960, 2500, 5000, 5000, 12500, 17500, 25000, 25000, 62500, 62500,
                168750, 300000, -1
            };

            const int maxBlurayBitrate = 40000;

            if (outType == OutputType.OutputBluRay)
                return maxBlurayBitrate;

            return x264Prof.AvcProfile < 2 ? baseLineBitrates[x264Prof.AvcLevel] : highBitrates[x264Prof.AvcLevel];
        }
    }
}
