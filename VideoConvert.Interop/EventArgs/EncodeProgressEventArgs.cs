// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodeProgressEventArgs.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encode progress event
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.EventArgs
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Encode progress event
    /// </summary>
    public class EncodeProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets PercentComplete.
        /// </summary>
        [DataMember]
        public double PercentComplete { get; set; }

        /// <summary>
        /// Gets or sets CurrentFrameRate.
        /// </summary>
        [DataMember]
        public float CurrentFrameRate { get; set; }

        /// <summary>
        /// Gets or sets AverageFrameRate.
        /// </summary>
        [DataMember]
        public float AverageFrameRate { get; set; }

        /// <summary>
        /// Gets or sets CurrentFrame
        /// </summary>
        [DataMember]
        public long CurrentFrame { get; set; }

        /// <summary>
        /// Gets or sets TotalFrames
        /// </summary>
        [DataMember]
        public long TotalFrames { get; set; }

        /// <summary>
        /// Gets or sets EstimatedTimeLeft.
        /// </summary>
        [DataMember]
        public TimeSpan EstimatedTimeLeft { get; set; }

        /// <summary>
        /// Gets or sets ElapsedTime.
        /// </summary>
        [DataMember]
        public TimeSpan ElapsedTime { get; set; }
    }
}