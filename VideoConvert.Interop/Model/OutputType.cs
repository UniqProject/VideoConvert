// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputType.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
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
        [Description("Matroska-Container")] OutputMatroska = 0,
        [Description("MP4-Container")] OutputMp4 = 1,
        [Description("TS-Transport stream")] OutputTs = 2,
        [Description("M2TS-Transport stream")] OutputM2Ts = 3,
        [Description("Blu-Ray Disc")] OutputBluRay = 4,
        [Description("AVCHD-Disc")] OutputAvchd = 5,
        [Description("DVD-Disc")] OutputDvd = 6,
        [Description("WebM Video")] OutputWebM = 7,
        [Description("Undefined")] OutputUndefined = 255
    };
}