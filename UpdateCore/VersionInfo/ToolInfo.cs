using System;

namespace UpdateCore.VersionInfo
{
    public class ToolInfo
    {
        public string PackageVersion { get; set; }
        public string PackageName { get; set; }

        public ToolInfo()
        {
            PackageVersion = string.Empty;
            PackageName = string.Empty;
        }
    }
}