// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProfileType.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Profiles
{
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Enumerates a list of implemented profile types
    /// </summary>
    public enum ProfileType
    {
        /// <summary>
        /// default profile type
        /// </summary>
        None,

        /// <summary>
        /// Defines a profile for copying streams
        /// </summary>
        Copy,

        /// <summary>
        /// A profile, which holds information about used video and audio profiles
        /// </summary>
        QuickSelect,

        /// <summary>
        /// Profile for the x264 video encoder
        /// </summary>
        X264,

        /// <summary>
        /// Profile for the xvid video encoder
        /// </summary>
        XVid,

        /// <summary>
        /// Profile for the hc encoder
        /// </summary>
        HcEnc,

        /// <summary>
        /// Profile for mpeg 2 video encoder
        /// </summary>
        Mpeg2Video,

        /// <summary>
        /// Profile for the vp8 video encoder
        /// </summary>
        [XmlEnum(Name = "VP8")]
        Vp8,

        /// <summary>
        /// Profile for the ac3 audio encoder
        /// </summary>
        [XmlEnum(Name="AC3")]
        Ac3,

        /// <summary>
        /// Profile for the flac audio encoder
        /// </summary>
        [XmlEnum(Name="FLAC")]
        Flac,

        /// <summary>
        /// Profile for the ogg audio encoder
        /// </summary>
        [XmlEnum(Name = "OGG")]
        Ogg,

        /// <summary>
        /// Profile for the mp3 audio encoder
        /// </summary>
        [XmlEnum(Name = "MP3")]
        Mp3,

        /// <summary>
        /// Profile for the aac audio encoder
        /// </summary>
        [XmlEnum(Name="AAC")]
        Aac
    };
}