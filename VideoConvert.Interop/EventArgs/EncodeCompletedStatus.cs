// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodeCompletedStatus.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encode Completed Status
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.EventArgs
{
    /// <summary>
    /// Encode Completed Status
    /// </summary>
    /// <param name="sender">
    /// The Sender
    /// </param>
    /// <param name="args">
    /// The EncodeCompletedEventArgs
    /// </param>
    public delegate void EncodeCompletedStatus(object sender, EncodeCompletedEventArgs args);
}