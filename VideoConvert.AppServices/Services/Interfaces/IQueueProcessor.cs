﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueueProcessor.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The Queue Processor Interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Services.Interfaces
{
    using System;
    using System.Collections.ObjectModel;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;

    /// <summary>
    /// Queue Processor Interface
    /// </summary>
    public interface IQueueProcessor
    {
        /// <summary>
        /// Fires when Queue processing starts.
        /// </summary>
        event EventHandler QueueStarted;

        /// <summary>
        /// Fires when Queue processing finishes.
        /// </summary>
        event QueueCompletedStatus QueueCompleted;

        /// <summary>
        /// Queue has progressed
        /// </summary>
        event QueueProgressStatus QueueProgressChanged;

        /// <summary>
        /// Stops queue processing
        /// </summary>
        void Stop();

        /// <summary>
        /// Starts queue processing
        /// </summary>
        /// <param name="queue"></param>
        void StartProcessing(ObservableCollection<EncodeInfo> queue);
    }
}