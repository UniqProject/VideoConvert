// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueCompletedStatus.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Queue Completed Status
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.EventArgs
{
    /// <summary>
    /// Queue Completed Status
    /// </summary>
    /// <param name="sender">
    /// The Sender
    /// </param>
    /// <param name="args">
    /// The QueueCompletedEventArgs
    /// </param>
    public delegate void QueueCompletedStatus(object sender, QueueCompletedEventArgs args);
}