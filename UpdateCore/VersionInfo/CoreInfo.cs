// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CoreInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the UpdateCore source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UpdateCore.VersionInfo
{
    using System;
    using System.Xml.Serialization;

    public class CoreInfo
    {
        [XmlElement("PackageVersion")]
        public string PackageVersionStr { get; set; }

        public string PackageName { get; set; }
        public Version PackageVersion { get { return Version.Parse(PackageVersionStr); } }

        public CoreInfo()
        {
            PackageVersionStr = string.Empty;
            PackageName = string.Empty;
        }
    }
}