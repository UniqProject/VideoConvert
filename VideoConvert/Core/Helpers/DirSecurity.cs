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

using System.Security.AccessControl;
using System.Security.Principal;

namespace VideoConvert.Core.Helpers
{
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
