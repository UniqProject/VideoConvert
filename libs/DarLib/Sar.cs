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
    public struct Sar
    {
        public decimal Ar;

        public Sar(ulong x, ulong y)
        {
            Ar = x / (decimal)y;
        }

        public Sar(decimal sar)
        {
            Ar = sar;
        }

        public ulong X
        {
            get
            {
                ulong x, y;
                RatioUtils.Approximate(Ar, out x, out y);
                return x;
            }
        }

        public ulong Y
        {
            get
            {
                ulong x, y; RatioUtils.Approximate(Ar, out x, out y);
                return y;
            }
        }

        public Dar ToDar(int hres, int vres)
        {
            return new Dar(Ar * hres / vres);
        }
    }
}