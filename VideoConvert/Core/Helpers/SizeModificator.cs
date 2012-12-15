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
    class SizeModificator
    {
        private readonly int _id;
        private readonly string _name;
        private readonly long _mod;

        public int ID { get { return _id; } }
        public string Name { get { return _name; } }
        public long Mod { get { return _mod; } }

        public SizeModificator(int id, string name, long mod)
        {
            _id = id;
            _name = name;
            _mod = mod;
        }

        public static List<SizeModificator> GenerateList()
        {
            List<SizeModificator> modList = new List<SizeModificator>
                                                {
                                                    new SizeModificator(0, "byte", 1),
                                                    new SizeModificator(1, "KB", 1000),
                                                    new SizeModificator(2, "KiB", 1024),
                                                    new SizeModificator(3, "MB", 1000000),
                                                    new SizeModificator(4, "MiB", 1048576),
                                                    new SizeModificator(5, "GB", 1000000000),
                                                    new SizeModificator(6, "GiB", 1073741824)
                                                };

            return modList;
        }
    }
}