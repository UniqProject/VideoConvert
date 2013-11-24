// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecurityClass.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Helper class for directory creation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities
{
    /// <summary>
    /// Helper class for directory creation
    /// </summary>
    public enum SecurityClass 
    {
        /// <summary>
        /// Create directory for all users
        /// </summary>
        Everybody,

        /// <summary>
        /// Create directory only for current user
        /// </summary>
        CurrentUser
    }
}