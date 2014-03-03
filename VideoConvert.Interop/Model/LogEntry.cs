// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogEntry.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encode Log Entry
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System;

    /// <summary>
    /// Encode Log Entry
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Log Entry time
        /// </summary>
        public DateTime EntryTime { get; set; }

        /// <summary>
        /// Job Name
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// Log Entry message
        /// </summary>
        public string LogText { get; set; }
    }
}