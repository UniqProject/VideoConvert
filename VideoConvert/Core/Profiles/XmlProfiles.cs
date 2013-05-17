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
using System.Xml.Serialization;

namespace VideoConvert.Core.Profiles
{
    [XmlRoot("VideoConvert")]
    public class XmlProfiles
    {
        [XmlArray("QuickSelectProfiles")]
        [XmlArrayItem("QuickSelectProfile")]
        public List<QuickSelectProfile> QuickSelectProfiles { get; set; }

        [XmlArray("x264Profiles")]
        [XmlArrayItem("x264Profile")]
        public List<X264Profile> X264Profiles { get; set; }

        [XmlArray("hcencProfiles")]
        [XmlArrayItem("hcencProfile")]
        public List<HcEncProfile> HcEncProfiles { get; set; }

        [XmlArray("VP8Profiles")]
        [XmlArrayItem("VP8Profile")]
        public List<VP8Profile> VP8Profiles { get; set; }

        [XmlArray("AC3Profiles")]
        [XmlArrayItem("AC3Profile")]
        public List<AC3Profile> AC3Profiles { get; set; }

        [XmlArray("MP3Profiles")]
        [XmlArrayItem("MP3Profile")]
        public List<MP3Profile> MP3Profiles { get; set; }

        [XmlArray("OGGProfiles")]
        [XmlArrayItem("OGGProfile")]
        public List<OggProfile> OggProfiles { get; set; }

        [XmlArray("AACProfiles")]
        [XmlArrayItem("AACProfile")]
        public List<AACProfile> AACProfiles { get; set; }
    }
}