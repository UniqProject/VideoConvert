﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueCompletedEventArgs.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Queue complete event arguments
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.EventArgs
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Queue complete event arguments
    /// </summary>
    [DataContract]
    public class QueueCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="sucessful">
        /// The sucessful.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <param name="errorInformation">
        /// The error information.
        /// </param>
        public QueueCompletedEventArgs(bool sucessful, Exception exception, string errorInformation)
        {
            Successful = sucessful;
            Exception = exception;
            ErrorInformation = errorInformation;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Successful.
        /// </summary>
        [DataMember]
        public bool Successful { get; set; }

        /// <summary>
        /// Gets or sets Exception.
        /// </summary>
        [DataMember]
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets ErrorInformation.
        /// </summary>
        [DataMember]
        public string ErrorInformation { get; set; }
    }
}