// ****************************************************************************
// 
// Copyright (C) 2005-2012  Doom9 & al
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// 
// ****************************************************************************

using System;
using System.ComponentModel;

namespace DarLib
{
    /// <summary>
    /// Display aspect ratio (DAR)
    /// </summary>
    [TypeConverter(typeof(DarConverter))]
    public struct Dar
    {
        /// <summary>
        /// ITU 16:9 PAL aspect ratio
        /// </summary>
        public static readonly Dar Itu16X9Pal = new Dar(640, 351); // 720/576 * 512/351

        /// <summary>
        /// ITU 4:3 PAL aspect ratio
        /// </summary>
        public static readonly Dar Itu4X3Pal = new Dar(160, 117); // 720/576 * 128/117

        /// <summary>
        /// ITU 16:9 NTSC aspect ratio
        /// </summary>
        public static readonly Dar Itu16X9Ntsc = new Dar(8640, 4739); // 720/480 * 5760/4739

        /// <summary>
        /// ITU 4:3 NTSC aspect ratio
        /// </summary>
        public static readonly Dar Itu4X3Ntsc = new Dar(6480, 4739); // 720/480 * 4320/4739

        /// <summary>
        /// Default 4:3 aspect ratio
        /// </summary>
        public static readonly Dar Static4X3 = new Dar(4, 3);

        /// <summary>
        /// Default 16:9 aspect ratio
        /// </summary>
        public static readonly Dar Static16X9 = new Dar(16, 9);

        /// <summary>
        /// 1:1 aspect ratio
        /// </summary>
        public static readonly Dar A1X1 = new Dar(1, 1);

        /// <summary>
        /// Decimal aspect ratio
        /// </summary>
        public decimal Ar;

        /// <summary>
        /// Initialize aspect ratio using long values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Dar(ulong x, ulong y)
        {
            Ar = -1;
            Init(x, y);
        }

        /// <summary>
        /// Initialize aspect ratio using decimal value
        /// </summary>
        /// <param name="dar"></param>
        public Dar(decimal dar)
        {
            Ar = dar;
        }

        /// <summary>
        /// Initialize aspect ratio
        /// </summary>
        /// <param name="dar"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Dar(decimal? dar, ulong width, ulong height)
        {
            Ar = -1;
            if (dar.HasValue)
                Ar = dar.Value;
            else
                Init(width, height);
        }

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void Init(ulong x, ulong y)
        {
            Ar = x / (decimal)y;
        }

        /// <summary>
        /// Initialize aspect ratio
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Dar(int x, int y, ulong width, ulong height)
        {
            Ar = -1;
            if (x > 0 && y > 0)
                Init((ulong)x, (ulong)y);
            else
                Init(width, height);
        }

        /// <summary>
        /// Aspect ratio X-value
        /// </summary>
        public ulong X
        {
            get
            {
                ulong x, y;
                RatioUtils.Approximate(Ar, out x, out y);
                return x;
            }
        }

        /// <summary>
        /// Aspect ratio Y-value
        /// </summary>
        public ulong Y
        {
            get
            {
                ulong x, y; RatioUtils.Approximate(Ar, out x, out y);
                return y;
            }
        }

        public override string ToString()
        {
            var culture = new System.Globalization.CultureInfo("en-us");
            return Ar.ToString("#.########", culture);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Dar)) return false;
            decimal ar2 = ((Dar)obj).Ar;

            return (Math.Abs(Ar - ar2) < 0.0001M * Math.Min(Ar, ar2));
        }

        public override int GetHashCode()
        {
            return 0;
        }

        /// <summary>
        /// Convert to Storage Aspect Ratio (SAR)
        /// </summary>
        /// <param name="hres"></param>
        /// <param name="vres"></param>
        /// <returns></returns>
        public Sar ToSar(int hres, int vres)
        {
            // sarX
            // ----   must be the amount the video needs to be stretched horizontally.
            // sarY
            //
            //    horizontalResolution
            // --------------------------  is the ratio of the pixels. This must be stretched to equal realAspectRatio
            //  scriptVerticalResolution
            //
            // To work out the stretching amount, we then divide realAspectRatio by the ratio of the pixels:
            // sarX      parX        horizontalResolution        realAspectRatio * scriptVerticalResolution
            // ---- =    ---- /   -------------------------- =  --------------------------------------------   
            // sarY      parY     scriptVerticalResolution               horizontalResolution
            //
            // rounding value is mandatory here because some encoders (x264, xvid...) clamp sarX & sarY
            decimal ratio = Ar * vres / hres;
            return new Sar(ratio);
        }
    }
}
