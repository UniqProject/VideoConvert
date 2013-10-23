// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputType.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
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
    /// Type of input file.
    /// Note: only this types of input can be processed
    /// </summary>
    public enum InputType
    {
        [Description("AVI-Container")] InputAvi = 0,
        [Description("MP4-Container")] InputMp4 = 1,
        [Description("Matroska-Container")] InputMatroska = 2,
        [Description("TS-Transportstream")] InputTs = 3,
        [Description("Windows Media Container")] InputWm = 4,
        [Description("Flash Video")] InputFlash = 5,
        [Description("DVD Disc")] InputDvd = 6,
        [Description("Blu-Ray Disc")] InputBluRay = 7,
        [Description("AVCHD-Disc")] InputAvchd = 8,
        [Description("HD-DVD Disc")] InputHddvd = 9,
        [Description("MPEG-PS")] InputMpegps = 10,
        [Description("AviSynth Script")] InputAviSynth = 11,
        [Description("WebM")] InputWebM = 12,
        [Description("OGG")] InputOgg = 13,
        [Description("Undefined")] InputUndefined = 255
    };
}