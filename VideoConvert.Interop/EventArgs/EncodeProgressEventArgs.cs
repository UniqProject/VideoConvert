// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodeProgressEventArgs.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.EventArgs
{
    using System;
    using System.Runtime.Serialization;

    public class EncodeProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets PercentComplete.
        /// </summary>
        [DataMember]
        public float PercentComplete { get; set; }

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
        /// Gets or sets EstimatedTimeLeft.
        /// </summary>
        [DataMember]
        public TimeSpan EstimatedTimeLeft { get; set; }

        /// <summary>
        /// Gets or sets Task.
        /// </summary>
        [DataMember]
        public int Task { get; set; }

        /// <summary>
        /// Gets or sets TaskCount.
        /// </summary>
        [DataMember]
        public int TaskCount { get; set; }

        /// <summary>
        /// Gets or sets ElapsedTime.
        /// </summary>
        [DataMember]
        public TimeSpan ElapsedTime { get; set; }
    }
}