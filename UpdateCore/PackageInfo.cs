// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the UpdateCore source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UpdateCore
{
    public class PackageInfo
    {
        public string PackageName { get; set; }
        public string PackageLocation { get; set; }
        public string Destination { get; set; }
        public string Version { get; set; }
        public bool WriteVersion { get; set; }
        public bool ClearDirectory { get; set; }
        public bool RecursiveClearDirectory { get; set; }
    }
}
