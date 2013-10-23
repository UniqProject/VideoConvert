﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDemuxerEac3To.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Demuxer.Interfaces
{
    using System;
    using Encoder.Interfaces;
    using Interop.Model;

    /// <summary>
    /// IDemuxerEac3To interface
    /// </summary>
    public interface IDemuxerEac3To
    {
        /// <summary>
        /// Execute a eac3to demux process.
        /// This should only be called from the UI thread.
        /// </summary>
        /// <param name="encodeQueueTask">
        /// The encodeQueueTask.
        /// </param>
        void Start(EncodeInfo encodeQueueTask);

        /// <summary>
        /// Kill the CLI process
        /// </summary>
        void Stop();

        /// <summary>
        /// Shutdown the service.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Fires when a new CLI QueueTask starts
        /// </summary>
        event EventHandler EncodeStarted;

        /// <summary>
        /// Fires when a CLI QueueTask finishes.
        /// </summary>
        event EncodeCompletedStatus EncodeCompleted;

        /// <summary>
        /// Encode process has progressed
        /// </summary>
        event EncodeProgessStatus EncodeStatusChanged;

        /// <summary>
        /// Gets or sets a value indicating whether IsEncoding.
        /// </summary>
        bool IsEncoding { get; }
    }
}