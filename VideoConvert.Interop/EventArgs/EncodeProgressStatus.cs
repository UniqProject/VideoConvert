// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodeProgressStatus.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encode Progress Status
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.EventArgs
{
    /// <summary>
    /// Encode Progress Status
    /// </summary>
    /// <param name="sender">
    /// The Sender
    /// </param>
    /// <param name="args">
    /// The EncodeProgressEventArgs
    /// </param>
    public delegate void EncodeProgressStatus(object sender, EncodeProgressEventArgs args);
}