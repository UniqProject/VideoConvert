// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlProfiles.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

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

        [XmlArray("Mpeg2VideoProfiles")]
        [XmlArrayItem("Mpeg2VideoProfile")]
        public List<HcEncProfile> Mpeg2VideoProfiles { get; set; }

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