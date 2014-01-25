// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputType.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Input file types
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.ComponentModel;

    /// <summary>
    /// Type of input file.
    /// Note: only this types of input can be processed
    /// </summary>
    public enum InputType
    {
        /// <summary>
        /// AVI-Container
        /// </summary>
        [Description("AVI-Container")] 
        InputAvi = 0,

        /// <summary>
        /// MP4-Container
        /// </summary>
        [Description("MP4-Container")] 
        InputMp4 = 1,

        /// <summary>
        /// Matroska-Container
        /// </summary>
        [Description("Matroska-Container")] 
        InputMatroska = 2,

        /// <summary>
        /// TS-Transportstream
        /// </summary>
        [Description("TS-Transportstream")] 
        InputTs = 3,

        /// <summary>
        /// Windows Media Container
        /// </summary>
        [Description("Windows Media Container")] 
        InputWm = 4,

        /// <summary>
        /// Flash Video
        /// </summary>
        [Description("Flash Video")] 
        InputFlash = 5,

        /// <summary>
        /// DVD Disc
        /// </summary>
        [Description("DVD Disc")] 
        InputDvd = 6,

        /// <summary>
        /// Blu-Ray Disc
        /// </summary>
        [Description("Blu-Ray Disc")] 
        InputBluRay = 7,

        /// <summary>
        /// AVCHD-Disc
        /// </summary>
        [Description("AVCHD-Disc")] 
        InputAvchd = 8,

        /// <summary>
        /// HD-DVD Disc
        /// </summary>
        [Description("HD-DVD Disc")] 
        InputHddvd = 9,

        /// <summary>
        /// MPEG-PS
        /// </summary>
        [Description("MPEG-PS")] 
        InputMpegps = 10,

        /// <summary>
        /// AviSynth Script
        /// </summary>
        [Description("AviSynth Script")] 
        InputAviSynth = 11,

        /// <summary>
        /// WebM
        /// </summary>
        [Description("WebM")] 
        InputWebM = 12,

        /// <summary>
        /// OGG
        /// </summary>
        [Description("OGG")] 
        InputOgg = 13,

        /// <summary>
        /// Undefined
        /// </summary>
        [Description("Undefined")] 
        InputUndefined = 255
    };
}