//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using VideoConvert.Core.Helpers;

namespace VideoConvert.Core.Profiles
{
    public class ProfilesHandler
    {
        private static XmlDocument _profilesFile;
        private readonly string _profFileName;
        private bool _profileChanged;

        public List<EncoderProfile> ProfileList;
        public List<EncoderProfile> FilteredList;

        public ProfilesHandler()
        {
            _profFileName = Path.Combine(AppSettings.CommonAppSettingsPath, "profiles.xml");
            InitialiseProfilesFile();
            LoadProfiles();
        }

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
                XmlElement xn = _profilesFile.CreateElement("MKV_Remux");
                _profilesFile.AppendChild(decl);
                _profilesFile.AppendChild(xn);

                _profilesFile.Save(_profFileName);
                _profilesFile.Load(_profFileName);
            }
        }

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

        public static List<EncoderProfile> ImportProfiles (string fileName)
        {
            List<EncoderProfile> importedProfiles = new List<EncoderProfile>();

            XmlSerializer serializer = new XmlSerializer(typeof(XmlProfiles));
            XmlTextReader xmlTextReader = new XmlTextReader(fileName);
            XmlProfiles xmlProfiles = (XmlProfiles)serializer.Deserialize(xmlTextReader);

            StreamCopyProfile copyProfile = new StreamCopyProfile();
            importedProfiles.Add(copyProfile);

            importedProfiles.AddRange(xmlProfiles.QuickSelectProfiles);
            importedProfiles.AddRange(xmlProfiles.X264Profiles);
            importedProfiles.AddRange(xmlProfiles.HcEncProfiles);
            importedProfiles.AddRange(xmlProfiles.VP8Profiles);
            importedProfiles.AddRange(xmlProfiles.AC3Profiles);
            importedProfiles.AddRange(xmlProfiles.OggProfiles);
            importedProfiles.AddRange(xmlProfiles.MP3Profiles);
            importedProfiles.AddRange(xmlProfiles.AACProfiles);
            xmlTextReader.Close();

            return importedProfiles;
        }

        private List<EncoderProfile> GetFilteredList()
        {
            List<EncoderProfile> result = new List<EncoderProfile>();
            result.AddRange(ProfileList);

            List<EncoderProfile> filter = new List<EncoderProfile>();
            if (!AppSettings.LameInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.MP3));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).AudioProfileType == ProfileType.MP3));
            }
            if (!AppSettings.OggEncInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.OGG));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).AudioProfileType == ProfileType.OGG));
            }
            if (!AppSettings.NeroAacEncInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.AAC));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile)profile).AudioProfileType == ProfileType.AAC));
            }
            if (!AppSettings.FfmpegInstalled)
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
            }

            if (!AppSettings.X264Installed)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.X264));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).VideoProfileType == ProfileType.X264));
            }
            if (!AppSettings.HcEncInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.HcEnc));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).VideoProfileType == ProfileType.HcEnc));
            }

            if (!AppSettings.VpxEncInstalled)
            {
                filter.AddRange(ProfileList.Where(profile => profile.Type == ProfileType.VP8));
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile)profile).VideoProfileType == ProfileType.VP8));
            }

            if (!AppSettings.MKVMergeInstalled)
            {
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).OutFormat == OutputType.OutputMatroska));
            }

            if (!AppSettings.TsMuxerInstalled)
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
            if (!AppSettings.MP4BoxInstalled)
            {
                filter.AddRange(
                    ProfileList.Where(
                        profile =>
                        profile.Type == ProfileType.QuickSelect &&
                        ((QuickSelectProfile) profile).OutFormat == OutputType.OutputMp4));
            }
            if (!AppSettings.DVDAuthorInstalled || !AppSettings.MjpegToolsInstalled)
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

            List<X264Profile> x264Profiles =
                ProfileList.Where(profile => profile.Type == ProfileType.X264).Cast<X264Profile>().ToList();
            List<HcEncProfile> hcEncProfiles =
                ProfileList.Where(profile => profile.Type == ProfileType.HcEnc).Cast<HcEncProfile>().ToList();
            List<VP8Profile> vp8Profiles =
                ProfileList.Where(profile => profile.Type == ProfileType.VP8).Cast<VP8Profile>().ToList();

            List<AC3Profile> ac3Profiles =
                ProfileList.Where(profile => profile.Type == ProfileType.AC3).Cast<AC3Profile>().ToList();
            List<MP3Profile> mp3Profiles =
                ProfileList.Where(profile => profile.Type == ProfileType.MP3).Cast<MP3Profile>().ToList();
            List<OggProfile> oggProfiles =
                ProfileList.Where(profile => profile.Type == ProfileType.OGG).Cast<OggProfile>().ToList();
            List<AACProfile> aacProfiles =
                ProfileList.Where(profile => profile.Type == ProfileType.AAC).Cast<AACProfile>().ToList();

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

        public bool AddProfile(EncoderProfile inProfile)
        {
            if (ProfileList == null) return false;

            ProfileList.Add(inProfile);
            TriggerUpdate();

            return true;
        }

        public bool RemoveProfile(EncoderProfile inProfile)
        {
            if (ProfileList == null) return false;

            ProfileList.Remove(inProfile);
            TriggerUpdate();

            return true;
        }
   
        public void TriggerUpdate()
        {
            _profileChanged = true;
        }
        
    }
}
