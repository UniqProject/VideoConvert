using System;
using System.Xml.Serialization;

namespace UpdateCore.VersionInfo
{
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