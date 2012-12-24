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

using System.Collections.Generic;

namespace VideoConvert.Core.Helpers
{
    class TargetSize
    {
        private readonly int _id;
        private readonly string _name;
        private readonly ulong _size;

        public int ID { get { return _id; } }
        public string Name { get { return _name; } }
        public ulong Size { get { return _size; } }

        public TargetSize(int id, string name, ulong size)
        {
            _id = id;
            _name = name;
            _size = size;
        }

        public static List<TargetSize> GenerateList()
        {
            List<TargetSize> sizeList = new List<TargetSize>
                {
                    new TargetSize(0, "None (Profile Setting)", 0),
                    new TargetSize(1, "USB 512 MB", 512000000),
                    new TargetSize(2, "USB 1 GB", 1000000000),
                    new TargetSize(3, "USB 2 GB", 2000000000),
                    new TargetSize(4, "USB 4 GB", 4000000000),
                    new TargetSize(5, "DVD-/+R 4,7 GB", 4700000000),
                    new TargetSize(6, "USB 8 GB", 8000000000),
                    new TargetSize(7, "DVD-/+R DL 8,5 GB", 8500000000),
                    new TargetSize(8, "USB 16 GB", 16000000000),
                    new TargetSize(9, "BD-R/RE 25GB", 25000000000),
                    new TargetSize(10, "USB 32GB", 32000000000),
                    new TargetSize(11, "BD-R 50GB", 50000000000),
                    new TargetSize(12, "Custom", 1)
                };

            return sizeList;
        }
    }
}
