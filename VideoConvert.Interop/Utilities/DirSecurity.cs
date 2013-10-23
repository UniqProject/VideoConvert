// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DirSecurity.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities
{
    using System.Security.AccessControl;
    using System.Security.Principal;

    public enum SecurityClass {Everybody, CurrentUser}
    
    public class DirSecurity
    {
        public static DirectorySecurity CreateDirSecurity (SecurityClass securityClass)
        {
            DirectorySecurity security = new DirectorySecurity();

            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null)
            {
                SecurityIdentifier identity = windowsIdentity.User;
                if (identity != null)
                {
                    security.SetOwner(identity);
                    FileSystemAccessRule accessRule = new FileSystemAccessRule(identity,
                                                              FileSystemRights.FullControl,
                                                              InheritanceFlags.ObjectInherit |
                                                              InheritanceFlags.ContainerInherit,
                                                              PropagationFlags.None,
                                                              AccessControlType.Allow);
                    security.SetAccessRule(accessRule);
                }
            }

            if (securityClass == SecurityClass.Everybody)
            {
                SecurityIdentifier everybodyIdentity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

                FileSystemAccessRule accessRule = new FileSystemAccessRule(everybodyIdentity,
                                                          FileSystemRights.FullControl,
                                                          InheritanceFlags.ObjectInherit |
                                                          InheritanceFlags.ContainerInherit,
                                                          PropagationFlags.None,
                                                          AccessControlType.Allow);
                security.AddAccessRule(accessRule);
            }

            return security;
        }
    }
}
