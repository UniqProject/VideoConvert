using System.Security.AccessControl;
using System.Security.Principal;

namespace VideoConvert.Core.Helpers
{
    public enum SecurityClass {Everybody, CurrentUser}
    
    public class DirSecurity
    {
        public DirSecurity()
        {
            
        }

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
                SecurityIdentifier everybodyIdentity = new SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);

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
