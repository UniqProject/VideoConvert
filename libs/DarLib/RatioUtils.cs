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

namespace DarLib
{
    struct RatioUtils
    {
        /// <summary>
        /// Puts x and y in simplest form, by dividing by all their factors.
        /// </summary>
        /// <param name="x">First number to reduce</param>
        /// <param name="y">Second number to reduce</param>
        public static void Reduce(ref ulong x, ref ulong y)
        {
            ulong g = Gcd(x, y);
            x /= g;
            y /= g;
        }

        private static ulong Gcd(ulong x, ulong y)
        {
            while (y != 0)
            {
                ulong t = y;
                y = x % y;
                x = t;
            }
            return x;
        }

        public static void Approximate(decimal val, out ulong x, out ulong y)
        {
            Approximate(val, out x, out y, 1.0E-5);
        }

        public static void Approximate(decimal val, out ulong x, out ulong y, double precision)
        {
            // Fraction.Test();
            Fraction f = Fraction.ToFract((double)val, precision);

            x = f.Num;
            y = f.Denom;

            Reduce(ref x, ref y);
            // [i_a] ^^^ initial tests with the new algo show this is 
            // rather unnecessary, but we'll keep it anyway, just in case.
        }
    }
}