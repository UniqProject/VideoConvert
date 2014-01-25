// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputType.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   supported output formats
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.ComponentModel;

    /// <summary>
    /// supported output formats
    /// </summary>
    public enum OutputType
    {
        /// <summary>
        /// Matroska-Container
        /// </summary>
        [Description("Matroska-Container")] 
        OutputMatroska = 0,

        /// <summary>
        /// MP4-Container
        /// </summary>
        [Description("MP4-Container")] 
        OutputMp4 = 1,

        /// <summary>
        /// TS-Transport stream
        /// </summary>
        [Description("TS-Transport stream")] 
        OutputTs = 2,

        /// <summary>
        /// M2TS-Transport stream
        /// </summary>
        [Description("M2TS-Transport stream")] 
        OutputM2Ts = 3,

        /// <summary>
        /// Blu-Ray Disc
        /// </summary>
        [Description("Blu-Ray Disc")] 
        OutputBluRay = 4,

        /// <summary>
        /// AVCHD-Disc
        /// </summary>
        [Description("AVCHD-Disc")] 
        OutputAvchd = 5,

        /// <summary>
        /// DVD-Disc
        /// </summary>
        [Description("DVD-Disc")] 
        OutputDvd = 6,

        /// <summary>
        /// WebM Video
        /// </summary>
        [Description("WebM Video")] 
        OutputWebM = 7,

        /// <summary>
        /// Undefined
        /// </summary>
        [Description("Undefined")] 
        OutputUndefined = 255
    };
}