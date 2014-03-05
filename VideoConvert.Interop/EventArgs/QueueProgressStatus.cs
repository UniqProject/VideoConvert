// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueProgressStatus.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The Queue Progress Status
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.EventArgs
{
    /// <summary>
    /// Queue Progress Status
    /// </summary>
    /// <param name="sender">
    /// The Sender
    /// </param>
    /// <param name="args">
    /// The EncodeProgressEventArgs
    /// </param>
    public delegate void QueueProgressStatus(object sender, QueueProgressEventArgs args);
}