// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProfileHandler.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Model.Profiles
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;
    using Interop.Model;
    using Interop.Model.Profiles;
    using Interop.Utilities;
    using Services.Interfaces;

    /// <summary>
    /// A Helper class which handles the profile xml serialization / deserialization
    /// </summary>
    public class ProfilesHandler
    {
        private static XmlDocument _profilesFile;
        private readonly string _profFileName;
        private bool _profileChanged;

        private readonly IAppConfigService _config;

        /// <summary>
        /// Contains all Profiles
        /// </summary>
        public List<EncoderProfile> ProfileList;


        /// <summary>
        /// Contains Profiles for only installed tools
        /// </summary>
        public List<EncoderProfile> FilteredList;

        /// <summary>
        /// Creates a new object with all saved profiles
        /// </summary>
        /// <param name="config">Application settings</param>
        public ProfilesHandler(IAppConfigService config)
        {
            _config = config;
            _profFileName = Path.Combine(_config.CommonAppSettingsPath, "profiles.xml");
            InitialiseProfilesFile();
            LoadProfiles();
        }

        /// <summary>
        /// Frees up memory
        /// </summary>
        public void Destroy()
        {
            if (_profileChanged)
                SaveProfiles();
            ProfileList.Clear();
            ProfileList = null;
        }

        private void InitialiseProfilesFile()
        {
            _profilesFile = new XmlDocument();
            try
            {
                _profilesFile.Load(_profFileName);
            }
            catch (Exception)
            {
                string dir = Path.GetDirectoryName(_profFileName);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));
                }
                XmlDeclaration decl = _profilesFile.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                XmlElement xn = _profilesFile.CreateElement("VideoConvert");
                _profilesFile.AppendChild(decl);
                _profilesFile.AppendChild(xn);

                _profilesFile.Save(_profFileName);
                _profilesFile.Load(_profFileName);
            }
        }

        /// <summary>
        /// Reloads all profiles from file
        /// </summary>
        public void ReloadProfiles()
        {
            ProfileList.Clear();
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            ProfileList = ImportProfiles(_profFileName);

            FilteredList = GetFilteredList();
        }

        /// <summary>
        /// Import Profiles from give filename
        /// </summary>
        /// <param name="fileName">file which holds all the profile information</param>
        /// <returns>List of imported profiles</returns>
        public static List<EncoderProfile> ImportProfiles (string fileName)
        {
            List<EncoderProfile> importedProfiles = new List<EncoderProfile>();

            XmlSerializer serializer = new XmlSerializer(typeof(XmlProfiles));
            XmlTextReader xmlTextReader = new XmlTextReader(fileName);
            XmlProfiles xmlProfiles = (XmlProfiles)serializer.Deserialize(xmlTextReader);

            
            StreamCopyProfile copyProfile = new StreamCopyProfile();
            importedProfiles.Add(copyProfile);

            xmlProfiles.QuickSelectProfiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
            importedProfiles.AddRange(xmlProfiles.QuickSelectProfiles);

            xmlProfiles.X264Profiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
            importedProfiles.AddRange(xmlProfiles.X264Profiles);

            xmlProfiles.HcEncProfiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
            importedProfiles.AddRange(xmlProfiles.HcEncProfiles);

            xmlProfiles.Mpeg2VideoProfiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
            importedProfiles.AddRange(xmlProfiles.Mpeg2VideoProfiles);

            xmlProfiles.VP8Profiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
            importedProfiles.AddRange(xmlProfiles.VP8Profiles);

            xmlProfiles.AC3Profiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
            importedProfiles.AddRange(xmlProfiles.AC3Profiles);

            xmlProfiles.OggProfiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
            importedProfiles.AddRange(xmlProfiles.OggProfiles);

            xmlProfiles.MP3Profiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
            importedProfiles.AddRange(xmlProfiles.MP3Profiles);

            xmlProfiles.AACProfiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
            importedProfiles.AddRange(xmlProfiles.AACProfiles);
            xmlTextReader.Close();

            return importedProfiles;
        }

        private List<EncoderProfile> GetFilteredList()
        {
            List<EncoderProfile> result = new List<EncoderProfile>();
            result.AddRange(ProfileList);

            List<EncoderProfile> filter = new List<EncoderProfile>();
            if (!_config.LameInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.MP3));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).AudioProfileType == ProfileType.MP3));
            }
            if (!_config.OggEncInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.OGG));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).AudioProfileType == ProfileType.OGG));
            }
            if (!_config.NeroAacEncInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.AAC));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile)profile).AudioProfileType == ProfileType.AAC));
            }
            if (!_config.FfmpegInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.AC3));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).AudioProfileType == ProfileType.AC3));
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.FLAC));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).AudioProfileType == ProfileType.FLAC));
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.VP8));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile)profile).VideoProfileType == ProfileType.VP8));
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.Mpeg2Video));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile)profile).VideoProfileType == ProfileType.Mpeg2Video));
            }

            if (!_config.X264Installed)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.X264));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).VideoProfileType == ProfileType.X264));
            }
            if (!_config.HcEncInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.HcEnc));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).VideoProfileType == ProfileType.HcEnc));
            }

            if (!_config.VpxEncInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.VP8));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile)profile).VideoProfileType == ProfileType.VP8));
            }

            if (!_config.MKVMergeInstalled)
            {
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).OutFormat == OutputType.OutputMatroska));
            }

            if (!_config.TsMuxerInstalled)
            {
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).OutFormat == OutputType.OutputAvchd));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).OutFormat == OutputType.OutputBluRay));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).OutFormat == OutputType.OutputM2Ts));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).OutFormat == OutputType.OutputTs));
            }
            if (!_config.MP4BoxInstalled)
            {
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).OutFormat == OutputType.OutputMp4));
            }
            if (!_config.DVDAuthorInstalled || !_config.MjpegToolsInstalled)
            {
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).OutFormat == OutputType.OutputDvd));
            }

            foreach (EncoderProfile encoderProfile in filter)
            {
                result.Remove(encoderProfile);
            }

            return result;
        }

        private void SaveProfiles()
        {
            List<QuickSelectProfile> quickSelectProfiles =
                ProfileList.Where(profile => profile.Type == ProfileType.QuickSelect).Cast<QuickSelectProfile>().ToList();
            quickSelectProfiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));

            List<X264Profile> x264Profiles =
                ProfileList.Where(profile => profile.Type == ProfileType.X264).Cast<X264Profile>().ToList();
            x264Profiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));

            List<HcEncProfile> hcEncProfiles =
                ProfileList.Where(profile => profile.Type == ProfileType.HcEnc).Cast<HcEncProfile>().ToList();
            hcEncProfiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));

            List<Vp8Profile> vp8Profiles =
                ProfileList.Where(profile => profile.Type == ProfileType.VP8).Cast<Vp8Profile>().ToList();
            vp8Profiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));

            List<Ac3Profile> ac3Profiles =
                ProfileList.Where(profile => profile.Type == ProfileType.AC3).Cast<Ac3Profile>().ToList();
            ac3Profiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));

            List<Mp3Profile> mp3Profiles =
                ProfileList.Where(profile => profile.Type == ProfileType.MP3).Cast<Mp3Profile>().ToList();
            mp3Profiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));

            List<OggProfile> oggProfiles =
                ProfileList.Where(profile => profile.Type == ProfileType.OGG).Cast<OggProfile>().ToList();
            oggProfiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));

            List<AacProfile> aacProfiles =
                ProfileList.Where(profile => profile.Type == ProfileType.AAC).Cast<AacProfile>().ToList();
            aacProfiles.Sort((p1, p2) => String.Compare(p1.Name, p2.Name, StringComparison.Ordinal));

            XmlProfiles profiles = new XmlProfiles
                {
                    AC3Profiles =
                        ac3Profiles,
                    HcEncProfiles =
                        hcEncProfiles,
                    MP3Profiles =
                        mp3Profiles,
                    OggProfiles =
                        oggProfiles,
                    QuickSelectProfiles =
                        quickSelectProfiles,
                    X264Profiles =
                        x264Profiles,
                    AACProfiles = aacProfiles,
                    VP8Profiles = vp8Profiles,
                };
            XmlSerializer serializer = new XmlSerializer(typeof(XmlProfiles));
            using (FileStream writer = new FileStream(_profFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                serializer.Serialize(writer, profiles);
            }
            ReloadProfiles();
        }

        /// <summary>
        /// Adds a profile to the list
        /// </summary>
        /// <param name="inProfile"></param>
        /// <returns></returns>
        public bool AddProfile(EncoderProfile inProfile)
        {
            if (ProfileList == null) return false;

            ProfileList.Add(inProfile);
            TriggerUpdate();

            return true;
        }

        /// <summary>
        /// Removes a profile from the list
        /// </summary>
        /// <param name="inProfile"></param>
        /// <returns></returns>
        public bool RemoveProfile(EncoderProfile inProfile)
        {
            if (ProfileList == null) return false;

            ProfileList.Remove(inProfile);
            TriggerUpdate();

            return true;
        }
   
        /// <summary>
        /// Triggers an update
        /// </summary>
        public void TriggerUpdate()
        {
            _profileChanged = true;
        }
        
    }
}
