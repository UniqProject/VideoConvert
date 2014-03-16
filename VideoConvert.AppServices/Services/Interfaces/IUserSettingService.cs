// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUserSettingService.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The User Settings Service Interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Services.Interfaces
{
    using Interop.EventArgs;
    using System.Collections.Specialized;

    /// <summary>
    /// The Setting Event Handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SettingEventHandler(object sender, SettingChangedEventArgs e);

    /// <summary>
    /// The User Settings Service Interface
    /// </summary>
    public interface IUserSettingService
    {
        /// <summary>
        /// The setting changed.
        /// </summary>
        event SettingEventHandler SettingChanged;

        /// <summary>
        /// Set the specified user setting.
        /// </summary>
        /// <param name="name">
        /// Name of the property
        /// </param>
        /// <param name="value">
        /// The value to store.
        /// </param>
        void SetUserSetting(string name, object value);

        /// <summary>
        /// Get user setting for a given key.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <typeparam name="T">
        /// The Type of the setting
        /// </typeparam>
        /// <returns>
        /// The user setting
        /// </returns>
        T GetUserSetting<T>(string name);

        /// <summary>
        /// Get an StringCollection type user setting
        /// </summary>
        /// <param name="name">
        /// The setting name
        /// </param>
        /// <returns>
        /// The settings value
        /// </returns>
        StringCollection GetUserSettingStringCollection(string name);
    }
}