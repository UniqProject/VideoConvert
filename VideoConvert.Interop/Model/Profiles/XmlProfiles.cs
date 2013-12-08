// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlProfiles.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Profiles XML Serialization
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Profiles XML Serialization
    /// </summary>
    [XmlRoot("VideoConvert")]
    public class XmlProfiles
    {
        /// <summary>
        /// List of Quickselection profiles
        /// </summary>
        [XmlArray("QuickSelectProfiles")]
        [XmlArrayItem("QuickSelectProfile")]
        public List<QuickSelectProfile> QuickSelectProfiles { get; set; }

        /// <summary>
        /// List of x264 encoding profiles
        /// </summary>
        [XmlArray("x264Profiles")]
        [XmlArrayItem("x264Profile")]
        public List<X264Profile> X264Profiles { get; set; }

        /// <summary>
        /// List of HcEnc profiles
        /// </summary>
        [XmlArray("hcencProfiles")]
        [XmlArrayItem("hcencProfile")]
        public List<HcEncProfile> HcEncProfiles { get; set; }

        /// <summary>
        /// List of MPEG2 video encoding profiles
        /// </summary>
        [XmlArray("Mpeg2VideoProfiles")]
        [XmlArrayItem("Mpeg2VideoProfile")]
        public List<HcEncProfile> Mpeg2VideoProfiles { get; set; }

        /// <summary>
        /// List of VP8 Profiles
        /// </summary>
        [XmlArray("VP8Profiles")]
        [XmlArrayItem("VP8Profile")]
        public List<Vp8Profile> Vp8Profiles { get; set; }

        /// <summary>
        /// List of AC3 encoding profiles
        /// </summary>
        [XmlArray("AC3Profiles")]
        [XmlArrayItem("AC3Profile")]
        public List<Ac3Profile> Ac3Profiles { get; set; }

        /// <summary>
        /// List of MP3 encoding profiles
        /// </summary>
        [XmlArray("MP3Profiles")]
        [XmlArrayItem("MP3Profile")]
        public List<Mp3Profile> Mp3Profiles { get; set; }

        /// <summary>
        /// List of OGG encoding profiles
        /// </summary>
        [XmlArray("OGGProfiles")]
        [XmlArrayItem("OGGProfile")]
        public List<OggProfile> OggProfiles { get; set; }

        /// <summary>
        /// List of AAC encoding profiles
        /// </summary>
        [XmlArray("AACProfiles")]
        [XmlArrayItem("AACProfile")]
        public List<AacProfile> AacProfiles { get; set; }
    }
}