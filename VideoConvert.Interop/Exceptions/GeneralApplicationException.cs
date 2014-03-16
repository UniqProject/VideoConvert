﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralApplicationException.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   General Application exception
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Exceptions
{
    using System;

    /// <summary>
    /// General Application exception
    /// </summary>
    public class GeneralApplicationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralApplicationException"/> class. 
        /// </summary>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="solution">
        /// The solution.
        /// </param>
        /// <param name="innerException">
        /// The inner Exception.
        /// </param>
        public GeneralApplicationException(string error, string solution, Exception innerException)
        {
            this.Error = error;
            this.Solution = solution;
            this.ActualException = innerException;
        }

        /// <summary>
        /// Gets or sets FailureReason.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets Solution.
        /// </summary>
        public string Solution { get; set; }

        /// <summary>
        /// Gets or sets InnerException.
        /// </summary>
        public Exception ActualException { get; set; }
    }
}