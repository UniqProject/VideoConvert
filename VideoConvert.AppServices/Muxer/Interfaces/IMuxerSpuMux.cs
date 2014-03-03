

namespace VideoConvert.AppServices.Muxer.Interfaces
{
    using System;
    using Encoder.Interfaces;
    using Interop.Model;

    /// <summary>
    /// IMuxerSpuMux interface
    /// </summary>
    public interface IMuxerSpuMux
    {
        /// <summary>
        /// Execute a spumux process.
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