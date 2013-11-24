/****************************** Module Header ******************************\
 Module Name:  NativeMethod.cs
 Project:      CSUACSelfElevation
 Copyright (c) Microsoft Corporation.

 The P/Invoke signature some native Windows APIs used by the code sample.

 This source is subject to the Microsoft Public License.
 See http://www.microsoft.com/en-us/openness/resources/licenses.aspx#MPL
 All other rights reserved.

 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

// ReSharper disable InconsistentNaming

namespace VideoConvert.Interop.Utilities
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// The TOKEN_INFORMATION_CLASS enumeration type contains values that 
    /// specify the type of information being assigned to or retrieved from 
    /// an access token.
    /// </summary>
    public enum TOKEN_INFORMATION_CLASS
    {
        /// <summary>
        /// The buffer receives a TOKEN_USER structure that contains the user account of the token.
        /// </summary>
        TokenUser = 1,

        /// <summary>
        /// The buffer receives a TOKEN_GROUPS structure that contains the group accounts associated with the token.
        /// </summary>
        TokenGroups,

        /// <summary>
        /// The buffer receives a TOKEN_PRIVILEGES structure that contains the privileges of the token.
        /// </summary>
        TokenPrivileges,

        /// <summary>
        /// The buffer receives a TOKEN_OWNER structure that contains the default owner security identifier (SID) for newly created objects.
        /// </summary>
        TokenOwner,

        /// <summary>
        /// The buffer receives a TOKEN_PRIMARY_GROUP structure that contains the default primary group SID for newly created objects.
        /// </summary>
        TokenPrimaryGroup,

        /// <summary>
        /// The buffer receives a TOKEN_DEFAULT_DACL structure that contains the default DACL for newly created objects.
        /// </summary>
        TokenDefaultDacl,

        /// <summary>
        /// The buffer receives a TOKEN_SOURCE structure that contains the source of the token. TOKEN_QUERY_SOURCE access is needed to retrieve this information.
        /// </summary>
        TokenSource,

        /// <summary>
        /// The buffer receives a TOKEN_TYPE value that indicates whether the token is a primary or impersonation token.
        /// </summary>
        TokenType,

        /// <summary>
        /// The buffer receives a SECURITY_IMPERSONATION_LEVEL value that indicates the impersonation level of the token. If the access token is not an impersonation token, the function fails.
        /// </summary>
        TokenImpersonationLevel,

        /// <summary>
        /// The buffer receives a TOKEN_STATISTICS structure that contains various token statistics.
        /// </summary>
        TokenStatistics,

        /// <summary>
        /// The buffer receives a TOKEN_GROUPS structure that contains the list of restricting SIDs in a restricted token.
        /// </summary>
        TokenRestrictedSids,

        /// <summary>
        /// The buffer receives a DWORD value that indicates the Terminal Services session identifier that is associated with the token.
        /// If the token is associated with the terminal server client session, the session identifier is nonzero.
        /// Windows Server 2003 and Windows XP:  If the token is associated with the terminal server console session, the session identifier is zero.
        /// In a non-Terminal Services environment, the session identifier is zero.
        /// If TokenSessionId is set with SetTokenInformation, the application must have the Act As Part Of the Operating System privilege, and the application must be enabled to set the session ID in a token.
        /// </summary>
        TokenSessionId,

        /// <summary>
        /// The buffer receives a TOKEN_GROUPS_AND_PRIVILEGES structure that contains the user SID, the group accounts, the restricted SIDs, and the authentication ID associated with the token.
        /// </summary>
        TokenGroupsAndPrivileges,

        /// <summary>
        /// Reserved.
        /// </summary>
        TokenSessionReference,

        /// <summary>
        /// The buffer receives a DWORD value that is nonzero if the token includes the SANDBOX_INERT flag.
        /// </summary>
        TokenSandBoxInert,

        /// <summary>
        /// Reserved.
        /// </summary>
        TokenAuditPolicy,

        /// <summary>
        /// The buffer receives a TOKEN_ORIGIN value.
        /// If the token resulted from a logon that used explicit credentials, such as passing a name, domain, and password to the LogonUser function, then the TOKEN_ORIGIN structure will contain the ID of the logon session that created it.
        /// If the token resulted from network authentication, such as a call to AcceptSecurityContext or a call to LogonUser with dwLogonType set to LOGON32_LOGON_NETWORK or LOGON32_LOGON_NETWORK_CLEARTEXT, then this value will be zero.
        /// </summary>
        TokenOrigin,

        /// <summary>
        /// The buffer receives a TOKEN_ELEVATION_TYPE value that specifies the elevation level of the token.
        /// </summary>
        TokenElevationType,

        /// <summary>
        /// The buffer receives a TOKEN_LINKED_TOKEN structure that contains a handle to another token that is linked to this token.
        /// </summary>
        TokenLinkedToken,

        /// <summary>
        /// The buffer receives a TOKEN_ELEVATION structure that specifies whether the token is elevated.
        /// </summary>
        TokenElevation,

        /// <summary>
        /// The buffer receives a DWORD value that is nonzero if the token has ever been filtered.
        /// </summary>
        TokenHasRestrictions,

        /// <summary>
        /// The buffer receives a TOKEN_ACCESS_INFORMATION structure that specifies security information contained in the token.
        /// </summary>
        TokenAccessInformation,

        /// <summary>
        /// The buffer receives a DWORD value that is nonzero if virtualization is allowed for the token.
        /// </summary>
        TokenVirtualizationAllowed,

        /// <summary>
        /// The buffer receives a DWORD value that is nonzero if virtualization is enabled for the token.
        /// </summary>
        TokenVirtualizationEnabled,
        
        /// <summary>
        /// The buffer receives a TOKEN_MANDATORY_LABEL structure that specifies the token's integrity level.
        /// </summary>
        TokenIntegrityLevel,

        /// <summary>
        /// The buffer receives a DWORD value that is nonzero if the token has the UIAccess flag set.
        /// </summary>
        TokenUIAccess,

        /// <summary>
        /// The buffer receives a TOKEN_MANDATORY_POLICY structure that specifies the token's mandatory integrity policy.
        /// </summary>
        TokenMandatoryPolicy,

        /// <summary>
        /// The buffer receives a TOKEN_GROUPS structure that specifies the token's logon SID.
        /// </summary>
        TokenLogonSid,

        /// <summary>
        /// The buffer receives a DWORD value that is nonzero if the token is an app container token. 
        /// Any callers who check the TokenIsAppContainer and have it return 0 should also verify that the caller token is not an identify level impersonation token. 
        /// If the current token is not an app container but is an identity level token, you should return AccessDenied.
        /// </summary>
        TokenIsAppContainer,

        /// <summary>
        /// The buffer receives a TOKEN_GROUPS structure that contains the capabilities associated with the token.
        /// </summary>
        TokenCapabilities,

        /// <summary>
        /// The buffer receives a TOKEN_APPCONTAINER_INFORMATION structure that contains the AppContainerSid associated with the token. If the token is not associated with an app container, the TokenAppContainer member of the TOKEN_APPCONTAINER_INFORMATION structure points to NULL.
        /// </summary>
        TokenAppContainerSid,

        /// <summary>
        /// The buffer receives a DWORD value that includes the app container number for the token. For tokens that are not app container tokens, this value is zero.
        /// </summary>
        TokenAppContainerNumber,

        /// <summary>
        /// The buffer receives a CLAIM_SECURITY_ATTRIBUTES_INFORMATION structure that contains the user claims associated with the token.
        /// </summary>
        TokenUserClaimAttributes,

        /// <summary>
        /// The buffer receives a CLAIM_SECURITY_ATTRIBUTES_INFORMATION structure that contains the device claims associated with the token.
        /// </summary>
        TokenDeviceClaimAttributes,

        /// <summary>
        /// This value is reserved.
        /// </summary>
        TokenRestrictedUserClaimAttributes,

        /// <summary>
        /// This value is reserved.
        /// </summary>
        TokenRestrictedDeviceClaimAttributes,

        /// <summary>
        /// The buffer receives a TOKEN_GROUPS structure that contains the device groups that are associated with the token.
        /// </summary>
        TokenDeviceGroups,

        /// <summary>
        /// The buffer receives a TOKEN_GROUPS structure that contains the restricted device groups that are associated with the token.
        /// </summary>
        TokenRestrictedDeviceGroups,

        /// <summary>
        /// This value is reserved.
        /// </summary>
        TokenSecurityAttributes,

        /// <summary>
        /// This value is reserved.
        /// </summary>
        TokenIsRestricted,

        /// <summary>
        /// The maximum value for this enumeration.
        /// </summary>
        MaxTokenInfoClass
    }

    /// <summary>
    /// The WELL_KNOWN_SID_TYPE enumeration type is a list of commonly used 
    /// security identifiers (SIDs). Programs can pass these values to the 
    /// CreateWellKnownSid function to create a SID from this list.
    /// </summary>
    public enum WELL_KNOWN_SID_TYPE
    {
        /// <summary>
        /// Indicates a null SID.
        /// </summary>
        WinNullSid = 0,

        /// <summary>
        /// Indicates a SID that matches everyone.
        /// </summary>
        WinWorldSid = 1,

        /// <summary>
        /// Indicates a local SID.
        /// </summary>
        WinLocalSid = 2,

        /// <summary>
        /// Indicates a SID that matches the owner or creator of an object.
        /// </summary>
        WinCreatorOwnerSid = 3,

        /// <summary>
        /// Indicates a SID that matches the creator group of an object.
        /// </summary>
        WinCreatorGroupSid = 4,

        /// <summary>
        /// Indicates a creator owner server SID.
        /// </summary>
        WinCreatorOwnerServerSid = 5,

        /// <summary>
        /// Indicates a creator group server SID.
        /// </summary>
        WinCreatorGroupServerSid = 6,

        /// <summary>
        /// Indicates a SID for the Windows NT authority account.
        /// </summary>
        WinNtAuthoritySid = 7,

        /// <summary>
        /// Indicates a SID for a dial-up account.
        /// </summary>
        WinDialupSid = 8,

        /// <summary>
        /// Indicates a SID for a network account. This SID is added to the process of a token when it logs on across a network. The corresponding logon type is LOGON32_LOGON_NETWORK.
        /// </summary>
        WinNetworkSid = 9,

        /// <summary>
        /// Indicates a SID for a batch process. This SID is added to the process of a token when it logs on as a batch job. The corresponding logon type is LOGON32_LOGON_BATCH.
        /// </summary>
        WinBatchSid = 10,

        /// <summary>
        /// Indicates a SID for an interactive account. This SID is added to the process of a token when it logs on interactively. The corresponding logon type is LOGON32_LOGON_INTERACTIVE.
        /// </summary>
        WinInteractiveSid = 11,

        /// <summary>
        /// Indicates a SID for a service. This SID is added to the process of a token when it logs on as a service. The corresponding logon type is LOGON32_LOGON_SERVICE.
        /// </summary>
        WinServiceSid = 12,

        /// <summary>
        /// Indicates a SID for the anonymous account.
        /// </summary>
        WinAnonymousSid = 13,

        /// <summary>
        /// Indicates a proxy SID.
        /// </summary>
        WinProxySid = 14,

        /// <summary>
        /// Indicates a SID for an enterprise controller.
        /// </summary>
        WinEnterpriseControllersSid = 15,

        /// <summary>
        /// Indicates a SID for self.
        /// </summary>
        WinSelfSid = 16,

        /// <summary>
        /// Indicates a SID that matches any authenticated user.
        /// </summary>
        WinAuthenticatedUserSid = 17,

        /// <summary>
        /// Indicates a SID for restricted code.
        /// </summary>
        WinRestrictedCodeSid = 18,

        /// <summary>
        /// Indicates a SID that matches a terminal server account.
        /// </summary>
        WinTerminalServerSid = 19,

        /// <summary>
        /// Indicates a SID that matches remote logons.
        /// </summary>
        WinRemoteLogonIdSid = 20,

        /// <summary>
        /// Indicates a SID that matches logon IDs.
        /// </summary>
        WinLogonIdsSid = 21,

        /// <summary>
        /// Indicates a SID that matches the local system.
        /// </summary>
        WinLocalSystemSid = 22,

        /// <summary>
        /// Indicates a SID that matches a local service.
        /// </summary>
        WinLocalServiceSid = 23,

        /// <summary>
        /// Indicates a SID that matches a network service.
        /// </summary>
        WinNetworkServiceSid = 24,

        /// <summary>
        /// Indicates a SID that matches the domain account.
        /// </summary>
        WinBuiltinDomainSid = 25,

        /// <summary>
        /// Indicates a SID that matches the administrator group.
        /// </summary>
        WinBuiltinAdministratorsSid = 26,

        /// <summary>
        /// Indicates a SID that matches built-in user accounts.
        /// </summary>
        WinBuiltinUsersSid = 27,

        /// <summary>
        /// Indicates a SID that matches the guest account.
        /// </summary>
        WinBuiltinGuestsSid = 28,

        /// <summary>
        /// Indicates a SID that matches the power users group.
        /// </summary>
        WinBuiltinPowerUsersSid = 29,

        /// <summary>
        /// Indicates a SID that matches the account operators account.
        /// </summary>
        WinBuiltinAccountOperatorsSid = 30,

        /// <summary>
        /// Indicates a SID that matches the system operators group.
        /// </summary>
        WinBuiltinSystemOperatorsSid = 31,

        /// <summary>
        /// Indicates a SID that matches the print operators group.
        /// </summary>
        WinBuiltinPrintOperatorsSid = 32,

        /// <summary>
        /// Indicates a SID that matches the backup operators group.
        /// </summary>
        WinBuiltinBackupOperatorsSid = 33,

        /// <summary>
        /// Indicates a SID that matches the replicator account.
        /// </summary>
        WinBuiltinReplicatorSid = 34,

        /// <summary>
        /// Indicates a SID that matches pre-Windows 2000 compatible accounts.
        /// </summary>
        WinBuiltinPreWindows2000CompatibleAccessSid = 35,

        /// <summary>
        /// Indicates a SID that matches remote desktop users.
        /// </summary>
        WinBuiltinRemoteDesktopUsersSid = 36,

        /// <summary>
        /// Indicates a SID that matches the network operators group.
        /// </summary>
        WinBuiltinNetworkConfigurationOperatorsSid = 37,

        /// <summary>
        /// Indicates a SID that matches the account administrator's account.
        /// </summary>
        WinAccountAdministratorSid = 38,

        /// <summary>
        /// Indicates a SID that matches the account guest group.
        /// </summary>
        WinAccountGuestSid = 39,

        /// <summary>
        /// Indicates a SID that matches account Kerberos target group.
        /// </summary>
        WinAccountKrbtgtSid = 40,

        /// <summary>
        /// Indicates a SID that matches the account domain administrator group.
        /// </summary>
        WinAccountDomainAdminsSid = 41,

        /// <summary>
        /// Indicates a SID that matches the account domain users group.
        /// </summary>
        WinAccountDomainUsersSid = 42,

        /// <summary>
        /// Indicates a SID that matches the account domain guests group.
        /// </summary>
        WinAccountDomainGuestsSid = 43,

        /// <summary>
        /// Indicates a SID that matches the account computer group.
        /// </summary>
        WinAccountComputersSid = 44,

        /// <summary>
        /// Indicates a SID that matches the account controller group.
        /// </summary>
        WinAccountControllersSid = 45,

        /// <summary>
        /// Indicates a SID that matches the certificate administrators group.
        /// </summary>
        WinAccountCertAdminsSid = 46,

        /// <summary>
        /// Indicates a SID that matches the schema administrators group.
        /// </summary>
        WinAccountSchemaAdminsSid = 47,

        /// <summary>
        /// Indicates a SID that matches the enterprise administrators group.
        /// </summary>
        WinAccountEnterpriseAdminsSid = 48,

        /// <summary>
        /// Indicates a SID that matches the policy administrators group.
        /// </summary>
        WinAccountPolicyAdminsSid = 49,

        /// <summary>
        /// Indicates a SID that matches the RAS and IAS server account.
        /// </summary>
        WinAccountRasAndIasServersSid = 50,

        /// <summary>
        /// Indicates a SID present when the Microsoft NTLM authentication package authenticated the client.
        /// </summary>
        WinNTLMAuthenticationSid = 51,

        /// <summary>
        /// Indicates a SID present when the Microsoft Digest authentication package authenticated the client.
        /// </summary>
        WinDigestAuthenticationSid = 52,

        /// <summary>
        /// Indicates a SID present when the Secure Channel (SSL/TLS) authentication package authenticated the client.
        /// </summary>
        WinSChannelAuthenticationSid = 53,

        /// <summary>
        /// Indicates a SID present when the user authenticated from within the forest or across a trust that does not have the selective authentication option enabled. If this SID is present, then WinOtherOrganizationSid cannot be present.
        /// </summary>
        WinThisOrganizationSid = 54,

        /// <summary>
        /// Indicates a SID present when the user authenticated across a forest with the selective authentication option enabled. If this SID is present, then WinThisOrganizationSid cannot be present.
        /// </summary>
        WinOtherOrganizationSid = 55,
        
        /// <summary>
        /// Indicates a SID that allows a user to create incoming forest trusts. It is added to the token of users who are a member of the Incoming Forest Trust Builders built-in group in the root domain of the forest.
        /// </summary>
        WinBuiltinIncomingForestTrustBuildersSid = 56,
        
        /// <summary>
        /// Indicates a SID that matches the performance monitor user group.
        /// </summary>
        WinBuiltinPerfMonitoringUsersSid = 57,
        
        /// <summary>
        /// Indicates a SID that matches the performance log user group.
        /// </summary>
        WinBuiltinPerfLoggingUsersSid = 58,
        
        /// <summary>
        /// Indicates a SID that matches the Windows Authorization Access group.
        /// </summary>
        WinBuiltinAuthorizationAccessSid = 59,
        
        /// <summary>
        /// Indicates a SID is present in a server that can issue terminal server licenses.
        /// </summary>
        WinBuiltinTerminalServerLicenseServersSid = 60,
        
        /// <summary>
        /// Indicates a SID that matches the distributed COM user group.
        /// </summary>
        WinBuiltinDCOMUsersSid = 61,
        
        /// <summary>
        /// Indicates a SID that matches the Internet built-in user group.
        /// </summary>
        WinBuiltinIUsersSid = 62,
        
        /// <summary>
        /// Indicates a SID that matches the Internet user group.
        /// </summary>
        WinIUserSid = 63,
        
        /// <summary>
        /// Indicates a SID that allows a user to use cryptographic operations. It is added to the token of users who are a member of the CryptoOperators built-in group.
        /// </summary>
        WinBuiltinCryptoOperatorsSid = 64,
        
        /// <summary>
        /// Indicates a SID that matches an untrusted label.
        /// </summary>
        WinUntrustedLabelSid = 65,
        
        /// <summary>
        /// Indicates a SID that matches an low level of trust label.
        /// </summary>
        WinLowLabelSid = 66,
        
        /// <summary>
        /// Indicates a SID that matches an medium level of trust label.
        /// </summary>
        WinMediumLabelSid = 67,
        
        /// <summary>
        /// Indicates a SID that matches a high level of trust label.
        /// </summary>
        WinHighLabelSid = 68,
        
        /// <summary>
        /// Indicates a SID that matches a system label.
        /// </summary>
        WinSystemLabelSid = 69,
        
        /// <summary>
        /// Indicates a SID that matches a write restricted code group.
        /// </summary>
        WinWriteRestrictedCodeSid = 70,
        
        /// <summary>
        /// Indicates a SID that matches a creator and owner rights group.
        /// </summary>
        WinCreatorOwnerRightsSid = 71,
        
        /// <summary>
        /// Indicates a SID that matches a cacheable principals group.
        /// </summary>
        WinCacheablePrincipalsGroupSid = 72,
        
        /// <summary>
        /// Indicates a SID that matches a non-cacheable principals group.
        /// </summary>
        WinNonCacheablePrincipalsGroupSid = 73,
        
        /// <summary>
        /// Indicates a SID that matches an enterprise wide read-only controllers group.
        /// </summary>
        WinEnterpriseReadonlyControllersSid = 74,
        
        /// <summary>
        /// Indicates a SID that matches an account read-only controllers group.
        /// </summary>
        WinAccountReadonlyControllersSid = 75,
        
        /// <summary>
        /// Indicates a SID that matches an event log readers group.
        /// </summary>
        WinBuiltinEventLogReadersGroup = 76,
        
        /// <summary>
        /// Indicates a SID that matches a read-only enterprise domain controller.
        /// </summary>
        WinNewEnterpriseReadonlyControllersSid = 77,
        
        /// <summary>
        /// Indicates a SID that matches the built-in DCOM certification services access group.
        /// </summary>
        WinBuiltinCertSvcDComAccessGroup = 78,
        
        /// <summary>
        /// Indicates a SID that matches the medium plus integrity label.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinMediumPlusLabelSid = 79,
        
        /// <summary>
        /// Indicates a SID that matches a local logon group.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinLocalLogonSid = 80,
        
        /// <summary>
        /// Indicates a SID that matches a console logon group.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinConsoleLogonSid = 81,
        
        /// <summary>
        /// Indicates a SID that matches a certificate for the given organization.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinThisOrganizationCertificateSid = 82,
        
        /// <summary>
        /// Indicates a SID that matches the application package authority.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinApplicationPackageAuthoritySid = 83,
        
        /// <summary>
        /// Indicates a SID that applies to all app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinBuiltinAnyPackageSid = 84,
        
        /// <summary>
        /// Indicates a SID of Internet client capability for app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinCapabilityInternetClientSid = 85,
        
        /// <summary>
        /// Indicates a SID of Internet client and server capability for app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinCapabilityInternetClientServerSid = 86,
        
        /// <summary>
        /// Indicates a SID of private network client and server capability for app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinCapabilityPrivateNetworkClientServerSid = 87,
        
        /// <summary>
        /// Indicates a SID for pictures library capability for app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinCapabilityPicturesLibrarySid = 88,
        
        /// <summary>
        /// Indicates a SID for videos library capability for app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinCapabilityVideosLibrarySid = 89,
        
        /// <summary>
        /// Indicates a SID for music library capability for app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinCapabilityMusicLibrarySid = 90,
        
        /// <summary>
        /// Indicates a SID for documents library capability for app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinCapabilityDocumentsLibrarySid = 91,
        
        /// <summary>
        /// Indicates a SID for shared user certificates capability for app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinCapabilitySharedUserCertificatesSid = 92,
        
        /// <summary>
        /// Indicates a SID for Windows credentials capability for app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinCapabilityEnterpriseAuthenticationSid = 93,
        
        /// <summary>
        /// Indicates a SID for removable storage capability for app containers.
        /// Windows Server 2008 R2, Windows 7, Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  This value is not available.
        /// </summary>
        WinCapabilityRemovableStorageSid = 94
    }

    /// <summary>
    /// The SECURITY_IMPERSONATION_LEVEL enumeration type contains values 
    /// that specify security impersonation levels. Security impersonation 
    /// levels govern the degree to which a server process can act on behalf 
    /// of a client process.
    /// </summary>
    public enum SECURITY_IMPERSONATION_LEVEL
    {
        /// <summary>
        /// The server process cannot obtain identification information about the client, and it cannot impersonate the client. 
        /// It is defined with no value given, and thus, by ANSI C rules, defaults to a value of zero.
        /// </summary>
        SecurityAnonymous,

        /// <summary>
        /// The server process can obtain information about the client, such as security identifiers and privileges, but it cannot impersonate the client. 
        /// This is useful for servers that export their own objects, for example, database products that export tables and views. Using the retrieved client-security information, 
        /// the server can make access-validation decisions without being able to use other services that are using the client's security context.
        /// </summary>
        SecurityIdentification,

        /// <summary>
        /// The server process can impersonate the client's security context on its local system. The server cannot impersonate the client on remote systems.
        /// </summary>
        SecurityImpersonation,

        /// <summary>
        /// The server process can impersonate the client's security context on remote systems.
        /// </summary>
        SecurityDelegation
    }

    /// <summary>
    /// The TOKEN_ELEVATION_TYPE enumeration indicates the elevation type of 
    /// token being queried by the GetTokenInformation function or set by 
    /// the SetTokenInformation function.
    /// </summary>
    public enum TOKEN_ELEVATION_TYPE
    {
        /// <summary>
        /// The token does not have a linked token.
        /// </summary>
        TokenElevationTypeDefault = 1,

        /// <summary>
        /// The token is an elevated token.
        /// </summary>
        TokenElevationTypeFull,

        /// <summary>
        /// The token is a limited token.
        /// </summary>
        TokenElevationTypeLimited
    }

    /// <summary>
    /// The structure represents a security identifier (SID) and its 
    /// attributes. SIDs are used to uniquely identify users or groups.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SID_AND_ATTRIBUTES
    {
        /// <summary>
        /// A pointer to a SID structure.
        /// </summary>
        public IntPtr Sid;

        /// <summary>
        /// Specifies attributes of the SID. This value contains up to 32 one-bit flags. Its meaning depends on the definition and use of the SID.
        /// </summary>
        public UInt32 Attributes;
    }

    /// <summary>
    /// The structure indicates whether a token has elevated privileges.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TOKEN_ELEVATION
    {
        /// <summary>
        /// A nonzero value if the token has elevated privileges; otherwise, a zero value.
        /// </summary>
        public Int32 TokenIsElevated;
    }

    /// <summary>
    /// The structure specifies the mandatory integrity level for a token.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TOKEN_MANDATORY_LABEL
    {
        /// <summary>
        /// A <see cref="SID_AND_ATTRIBUTES"/> structure that specifies the mandatory integrity level of the token.
        /// </summary>
        public SID_AND_ATTRIBUTES Label;
    }

    /// <summary>
    /// Represents a wrapper class for a token handle.
    /// </summary>
    public class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SafeTokenHandle()
            : base(true)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handle"></param>
        public SafeTokenHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        /// <summary>
        /// Close Token Handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// Release token handle
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {

            return CloseHandle(handle);

        }
    }

    /// <summary>
    /// Windows API Methods
    /// </summary>
    public class NativeMethods
    {
        // Token Specific Access Rights
        // Each type of securable object has a set of access rights that correspond to operations specific to that type of object. 
        // In addition to these object-specific access rights, there is a set of standard access rights that correspond to operations common to most types of securable objects.

        /// <summary>
        /// Combines DELETE, READ_CONTROL, WRITE_DAC, and WRITE_OWNER access.
        /// </summary>
        public const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;

        /// <summary>
        /// Currently defined to equal READ_CONTROL.
        /// </summary>
        public const UInt32 STANDARD_RIGHTS_READ = 0x00020000;

        // An application cannot change the access control list of an object unless the application has the rights to do so. 
        // These rights are controlled by a security descriptor in the access token for the object. For more information about security, see Access Control Model.
        // To get or set the security descriptor for an access token, call the GetKernelObjectSecurity and SetKernelObjectSecurity functions.
        // When you call the OpenProcessToken or OpenThreadToken function to get a handle to an access token, the system checks the requested access rights against the DACL in the token's security descriptor.
        //
        // The following are valid access rights for access-token objects:
        // The DELETE, READ_CONTROL, WRITE_DAC, and WRITE_OWNER standard access rights. Access tokens do not support the SYNCHRONIZE standard access right.
        // The ACCESS_SYSTEM_SECURITY right to get or set the SACL in the object's security descriptor.
        // The specific access rights for access tokens, which are listed in the following table.

        /// <summary>
        /// Required to attach a primary token to a process. The SE_ASSIGNPRIMARYTOKEN_NAME privilege is also required to accomplish this task.
        /// </summary>
        public const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;

        /// <summary>
        /// Required to duplicate an access token.
        /// </summary>
        public const UInt32 TOKEN_DUPLICATE = 0x0002;

        /// <summary>
        /// Required to attach an impersonation access token to a process.
        /// </summary>
        public const UInt32 TOKEN_IMPERSONATE = 0x0004;

        /// <summary>
        /// Required to query an access token.
        /// </summary>
        public const UInt32 TOKEN_QUERY = 0x0008;

        /// <summary>
        /// Required to query the source of an access token.
        /// </summary>
        public const UInt32 TOKEN_QUERY_SOURCE = 0x0010;

        /// <summary>
        /// Required to enable or disable the privileges in an access token.
        /// </summary>
        public const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;

        /// <summary>
        /// Required to adjust the attributes of the groups in an access token.
        /// </summary>
        public const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;

        /// <summary>
        /// Required to change the default owner, primary group, or DACL of an access token.
        /// </summary>
        public const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;

        /// <summary>
        /// Required to adjust the session ID of an access token. The SE_TCB_NAME privilege is required.
        /// </summary>
        public const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;

        /// <summary>
        /// Combines STANDARD_RIGHTS_READ and TOKEN_QUERY.
        /// </summary>
        public const UInt32 TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        /// <summary>
        /// Combines all possible access rights for a token.
        /// </summary>
        public const UInt32 TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
            TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE |
            TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES |
            TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID);


        /// <summary>
        /// The data area passed to a system call is too small.
        /// </summary>
        public const Int32 ERROR_INSUFFICIENT_BUFFER = 122;


        // Integrity Levels

        // Windows defines integrity levels by using a SID. 
        // Using a SID to represent an integrity level makes it very easy to integrate the integrity mechanism into existing 
        // security data structures without requiring code changes. Integrity level SIDs have the following form: S-1-16-xxxx.

        /// <summary>
        /// Untrusted level
        /// </summary>
        public const Int32 SECURITY_MANDATORY_UNTRUSTED_RID = 0x00000000;

        /// <summary>
        /// Low integrity level
        /// </summary>
        public const Int32 SECURITY_MANDATORY_LOW_RID = 0x00001000;

        /// <summary>
        /// Medium integrity level
        /// </summary>
        public const Int32 SECURITY_MANDATORY_MEDIUM_RID = 0x00002000;

        /// <summary>
        /// High integrity level
        /// </summary>
        public const Int32 SECURITY_MANDATORY_HIGH_RID = 0x00003000;

        /// <summary>
        /// System integrity level
        /// </summary>
        public const Int32 SECURITY_MANDATORY_SYSTEM_RID = 0x00004000;


        /// <summary>
        /// The function opens the access token associated with a process.
        /// </summary>
        /// <param name="hProcess">
        /// A handle to the process whose access token is opened.
        /// </param>
        /// <param name="desiredAccess">
        /// Specifies an access mask that specifies the requested types of 
        /// access to the access token. 
        /// </param>
        /// <param name="hToken">
        /// Outputs a handle that identifies the newly opened access token 
        /// when the function returns.
        /// </param>
        /// <returns></returns>
        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(IntPtr hProcess,
            UInt32 desiredAccess, out SafeTokenHandle hToken);


        /// <summary>
        /// The function creates a new access token that duplicates one 
        /// already in existence.
        /// </summary>
        /// <param name="ExistingTokenHandle">
        /// A handle to an access token opened with TOKEN_DUPLICATE access.
        /// </param>
        /// <param name="ImpersonationLevel">
        /// Specifies a SECURITY_IMPERSONATION_LEVEL enumerated type that 
        /// supplies the impersonation level of the new token.
        /// </param>
        /// <param name="DuplicateTokenHandle">
        /// Outputs a handle to the duplicate token. 
        /// </param>
        /// <returns></returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(
            SafeTokenHandle ExistingTokenHandle,
            SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
            out SafeTokenHandle DuplicateTokenHandle);


        /// <summary>
        /// The function retrieves a specified type of information about an 
        /// access token. The calling process must have appropriate access 
        /// rights to obtain the information.
        /// </summary>
        /// <param name="hToken">
        /// A handle to an access token from which information is retrieved.
        /// </param>
        /// <param name="tokenInfoClass">
        /// Specifies a value from the TOKEN_INFORMATION_CLASS enumerated 
        /// type to identify the type of information the function retrieves.
        /// </param>
        /// <param name="pTokenInfo">
        /// A pointer to a buffer the function fills with the requested 
        /// information.
        /// </param>
        /// <param name="tokenInfoLength">
        /// Specifies the size, in bytes, of the buffer pointed to by the 
        /// TokenInformation parameter. 
        /// </param>
        /// <param name="returnLength">
        /// A pointer to a variable that receives the number of bytes needed 
        /// for the buffer pointed to by the TokenInformation parameter. 
        /// </param>
        /// <returns></returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetTokenInformation(
            SafeTokenHandle hToken,
            TOKEN_INFORMATION_CLASS tokenInfoClass,
            IntPtr pTokenInfo,
            Int32 tokenInfoLength,
            out Int32 returnLength);


        /// <summary>
        /// Sets the elevation required state for a specified button or 
        /// command link to display an elevated icon. 
        /// </summary>
        public const UInt32 BCM_SETSHIELD = 0x160C;


        /// <summary>
        /// Sends the specified message to a window or windows. The function 
        /// calls the window procedure for the specified window and does not 
        /// return until the window procedure has processed the message. 
        /// </summary>
        /// <param name="hWnd">
        /// Handle to the window whose window procedure will receive the 
        /// message.
        /// </param>
        /// <param name="Msg">Specifies the message to be sent.</param>
        /// <param name="wParam">
        /// Specifies additional message-specific information.
        /// </param>
        /// <param name="lParam">
        /// Specifies additional message-specific information.
        /// </param>
        /// <returns></returns>
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, IntPtr lParam);


        /// <summary>
        /// The function returns a pointer to a specified subauthority in a 
        /// security identifier (SID). The subauthority value is a relative 
        /// identifier (RID).
        /// </summary>
        /// <param name="pSid">
        /// A pointer to the SID structure from which a pointer to a 
        /// subauthority is to be returned.
        /// </param>
        /// <param name="nSubAuthority">
        /// Specifies an index value identifying the subauthority array 
        /// element whose address the function will return.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is a pointer to the 
        /// specified SID subauthority. To get extended error information, 
        /// call GetLastError. If the function fails, the return value is 
        /// undefined. The function fails if the specified SID structure is 
        /// not valid or if the index value specified by the nSubAuthority 
        /// parameter is out of bounds.
        /// </returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetSidSubAuthority(IntPtr pSid, UInt32 nSubAuthority);
    }
}
// ReSharper restore InconsistentNaming