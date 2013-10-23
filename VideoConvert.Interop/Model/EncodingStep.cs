// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodingStep.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    public enum EncodingStep
    {
        NotSet,
        CopyTempFile,
        Dump,
        Demux,
        EncodeAudio,
        DemuxSubtitle,
        ProcessSubtitle,
        IndexVideo,
        GetCropRect,
        EncodeVideo,
        PreMuxResult,
        PremuxSubtitle,
        MuxResult,
        MoveOutFile,
        WriteInfoFile,
        Done
    };
}