// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodingStep.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Processing step
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    /// <summary>
    /// Processing step
    /// </summary>
    public enum EncodingStep
    {
        /// <summary>
        /// Default, not set
        /// </summary>
        NotSet,

        /// <summary>
        /// Copying Temp file
        /// </summary>
        CopyTempFile,

        /// <summary>
        /// Dump source title (DVD-only)
        /// </summary>
        Dump,

        /// <summary>
        /// Demux source streams
        /// </summary>
        Demux,

        /// <summary>
        /// Encode audio streams
        /// </summary>
        EncodeAudio,

        /// <summary>
        /// Demux subtitles
        /// </summary>
        DemuxSubtitle,

        /// <summary>
        /// Process Subtitles
        /// </summary>
        ProcessSubtitle,

        /// <summary>
        /// Index video stream
        /// </summary>
        IndexVideo,

        /// <summary>
        /// Detect crop rectangle
        /// </summary>
        GetCropRect,

        /// <summary>
        /// Encode video stream
        /// </summary>
        EncodeVideo,

        /// <summary>
        /// Premux audio+video streams (DVD-only)
        /// </summary>
        PreMuxResult,

        /// <summary>
        /// Premux subtitle streams into container (DVD-only)
        /// </summary>
        PremuxSubtitle,

        /// <summary>
        /// Mux encoded streams into output container
        /// </summary>
        MuxResult,

        /// <summary>
        /// Move output file
        /// </summary>
        MoveOutFile,

        /// <summary>
        /// Write XBMC info file
        /// </summary>
        WriteInfoFile,

        /// <summary>
        /// Processing done
        /// </summary>
        Done
    };
}