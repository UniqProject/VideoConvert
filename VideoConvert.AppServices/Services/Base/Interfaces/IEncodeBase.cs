namespace VideoConvert.AppServices.Services.Base.Interfaces
{
    using System;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;

    /// <summary>
    /// Base Encoder Interface
    /// </summary>
    public interface IEncodeBase
    {
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
        /// A Start Method to be implemented
        /// </summary>
        /// <param name="encodeQueueTask"></param>
        void Start(EncodeInfo encodeQueueTask);

        /// <summary>
        /// A Stop Method to be implemeneted.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets or sets a value indicating whether IsEncoding.
        /// </summary>
        bool IsEncoding { get; }
    }
}