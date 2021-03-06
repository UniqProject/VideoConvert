﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DirSecurity.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Helper class for setting correct ACL of directories
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities
{
    using System.Security.AccessControl;
    using System.Security.Principal;

    /// <summary>
    /// Helper class for setting correct ACL of directories
    /// </summary>
    public class DirSecurity
    {
        /// <summary>
        /// Create a new ACL
        /// </summary>
        /// <param name="securityClass"></param>
        /// <returns></returns>
        public static DirectorySecurity CreateDirSecurity (SecurityClass securityClass)
        {
            var security = new DirectorySecurity();

            var windowsIdentity = WindowsIdentity.GetCurrent();

            var identity = windowsIdentity?.User;

            FileSystemAccessRule accessRule;
            if (identity != null)
            {
                security.SetOwner(identity);
                accessRule = new FileSystemAccessRule(identity,
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit |
                    InheritanceFlags.ContainerInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow);
                security.SetAccessRule(accessRule);
            }

            if (securityClass != SecurityClass.Everybody) return security;

            var everybodyIdentity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            accessRule = new FileSystemAccessRule(everybodyIdentity,
                FileSystemRights.FullControl,
                InheritanceFlags.ObjectInherit |
                InheritanceFlags.ContainerInherit,
                PropagationFlags.None,
                AccessControlType.Allow);
            security.AddAccessRule(accessRule);

            return security;
        }
    }
}
