// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingChangedEventArgs.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Setting Changed event
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.EventArgs
{
    /// <summary>
    /// Setting Changed event
    /// </summary>
    public class SettingChangedEventArgs
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value { get; set; }
    }
}