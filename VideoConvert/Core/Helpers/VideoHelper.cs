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
using System.Globalization;
using VideoConvert.Core.Media;

namespace VideoConvert.Core.Helpers
{
    class VideoHelper
    {
        public static VideoInfo GetStreamInfo(VideoInfo vStream)
        {
            MediaInfoContainer mi = Processing.GetMediaInfo(vStream.TempFile);
            if (mi.Video.Count > 0)
            {
                Single.TryParse(mi.Video[0].DisplayAspectRatio, NumberStyles.Number, AppSettings.CInfo,
                                out vStream.AspectRatio);
                vStream.Bitrate = mi.Video[0].BitRate;
                vStream.Format = mi.Video[0].Format;
                vStream.FormatProfile = mi.Video[0].FormatProfile;
                vStream.FPS = mi.Video[0].FrameRate;
                vStream.FrameCount = mi.Video[0].FrameCount;
                vStream.FrameRateDenominator = mi.Video[0].FrameRateDenominator;
                vStream.FrameRateEnumerator = mi.Video[0].FrameRateEnumerator;
                vStream.Height = mi.Video[0].Height;
                vStream.Width = mi.Video[0].Width;
                vStream.Interlaced = mi.Video[0].ScanType != "Progressive";
                vStream.Length = mi.Video[0].DurationTime.TimeOfDay.TotalSeconds;
                vStream.PicSize = mi.Video[0].VideoSize;
                vStream.StreamSize = Processing.GetFileSize(vStream.TempFile);
            }
            return vStream;
        }

        public static Size GetTargetSize(EncodeInfo encodeInfo)
        {
            Size resizeTo = new Size {Width = encodeInfo.VideoStream.Width, Height = encodeInfo.VideoStream.Height};

            if ((!encodeInfo.VideoStream.CropRect.IsEmpty) && (!encodeInfo.EncodingProfile.KeepInputResolution))
            {
                resizeTo.Height = encodeInfo.VideoStream.CropRect.Height;
                resizeTo.Width = encodeInfo.VideoStream.CropRect.Width;
            }

            if ((encodeInfo.EncodingProfile.OutFormat == OutputType.OutputBluRay) ||
                (encodeInfo.EncodingProfile.OutFormat == OutputType.OutputAvchd))
                resizeTo = Processing.GetVideoDimensions(encodeInfo.VideoStream.PicSize, encodeInfo.VideoStream.AspectRatio,
                                                         encodeInfo.EncodingProfile.OutFormat);
            else if ((!encodeInfo.EncodingProfile.KeepInputResolution) && (encodeInfo.EncodingProfile.TargetWidth > 0))
            {
                resizeTo.Width = encodeInfo.EncodingProfile.TargetWidth;
                if (!encodeInfo.VideoStream.CropRect.IsEmpty)
                {
                    double aspectRatio = (double) encodeInfo.VideoStream.CropRect.Width/
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
    }
}
