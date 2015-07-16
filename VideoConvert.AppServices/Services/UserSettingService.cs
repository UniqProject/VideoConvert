// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserSettingService.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VideoConvert.AppServices.Services
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.Collections;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Exceptions;

    /// <summary>
    /// The User Settings Service
    /// </summary>
    public class UserSettingService : IUserSettingService
    {
        /// <summary>
        /// The Settings File
        /// </summary>
        private readonly string _settingsFile = Path.Combine(AppConfigService.GetAppSettingsPath(), "settings.xml");

        /// <summary>
        /// The XML Serializer 
        /// </summary>
        readonly XmlSerializer _serializer = new XmlSerializer(typeof(SerializableDictionary<string, object>));

        /// <summary>
        /// The User Settings
        /// </summary>
        private SerializableDictionary<string, object> _userSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettingService"/> class.
        /// </summary>
        public UserSettingService()
        {
            Load();
        }

        /// <summary>
        /// The setting changed.
        /// </summary>
        public event SettingEventHandler SettingChanged;

        /// <summary>
        /// Set the specified user setting.
        /// </summary>
        /// <param name="name">
        /// Name of the property
        /// </param>
        /// <param name="value">
        /// The value to store.
        /// </param>
        public void SetUserSetting(string name, object value)
        {
            _userSettings[name] = value;
            Save();

            OnSettingChanged(new SettingChangedEventArgs { Key = name, Value = value });
        }

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
        public T GetUserSetting<T>(string name)
        {
            if (_userSettings.ContainsKey(name))
            {
                return (T)_userSettings[name];
            }

            return default(T);
        }

        /// <summary>
        /// Get an StringCollection type user setting
        /// </summary>
        /// <param name="name">
        /// The setting name
        /// </param>
        /// <returns>
        /// The settings value
        /// </returns>
        public StringCollection GetUserSettingStringCollection(string name)
        {
            return (StringCollection)_userSettings[name];
        }

        /// <summary>
        /// The on setting changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected virtual void OnSettingChanged(SettingChangedEventArgs e)
        {
            var handler = SettingChanged;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Save the User Settings
        /// </summary>
        private void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(_settingsFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var strm = new FileStream(_settingsFile, FileMode.Create, FileAccess.Write))
                {
                    _serializer.Serialize(strm, _userSettings);
                }
            }
            catch (Exception exc)
            {
                throw new GeneralApplicationException(
                    "A problem occured when trying to save your preferences.",
                    "Any settings you changed may need to be reset the next time HandBrake launches.",
                    exc);
            }
        }

        /// <summary>
        /// Load the User Settings
        /// </summary>
        private void Load()
        {
            try
            {
                // Load up the users current settings file.
                if (File.Exists(_settingsFile))
                {
                    using (var reader = new StreamReader(_settingsFile))
                    {
                        var data = (SerializableDictionary<string, object>)_serializer.Deserialize(reader);
                        _userSettings = data;
                    }
                }
                else
                {
                    _userSettings = new SerializableDictionary<string, object>();
                }

                // Add any missing / new settings
                var defaults = GetDefaults();
                foreach (var item in defaults.Where(item => !_userSettings.Keys.Contains(item.Key)))
                {
                    _userSettings.Add(item.Key, item.Value);
                    Save();
                }
            }
            catch (Exception exc)
            {
                try
                {
                    _userSettings = GetDefaults();
                    if (File.Exists(_settingsFile))
                    {
                        File.Delete(_settingsFile);
                    }
                    Save();

                    throw new GeneralApplicationException("Warning, your settings have been reset!", "Your user settings file was corrupted or inaccessible. Settings have been reset to defaults.", exc);
                }
                catch (Exception)
                {
                    throw new GeneralApplicationException("Unable to load user settings.", "Your user settings file appears to be inaccessible or corrupted. You may have to delete the file and let HandBrake generate a new one.", exc);
                }
            }
        }

        /// <summary>
        /// Load Default Settings
        /// </summary>
        /// <returns>
        /// The get defaults.
        /// </returns>
        private SerializableDictionary<string, object> GetDefaults()
        {
            if (!File.Exists("defaultsettings.xml")) return new SerializableDictionary<string, object>();

            using (var reader = new StreamReader("defaultsettings.xml"))
            {
                return (SerializableDictionary<string, object>)_serializer.Deserialize(reader);
            }
        }
    }
}