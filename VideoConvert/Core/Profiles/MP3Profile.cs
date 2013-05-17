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

namespace VideoConvert.Core.Profiles
{
    public class MP3Profile : EncoderProfile
    {
        public int OutputChannels { get; set; }
        public int SampleRate { get; set; }
        public int EncodingMode { get; set; }
        public int Bitrate { get; set; }
        public int Quality { get; set; }
        public string Preset { get; set; }


        public MP3Profile()
        {
            Type = ProfileType.MP3;

            OutputChannels = 0;
            SampleRate = 0;
            Bitrate = 192;
            Quality = 4;
            EncodingMode = 2;
            Preset = "standard";
        }
    }
}