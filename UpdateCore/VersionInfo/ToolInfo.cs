// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the UpdateCore source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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