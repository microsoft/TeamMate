// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Microsoft.Tools.TeamMate.Foundation.DirectoryServices
{
    /// <summary>
    /// Provides utility methods for reading user information from Active Directory.
    /// </summary>
    public class DirectoryBrowser
    {
        private bool defaultSearchRootResolved;
        private DirectoryEntry defaultSearchRoot;
        private DirectoryBrowserMode mode;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryBrowser"/> class.
        /// </summary>
        public DirectoryBrowser()
            : this(DirectoryBrowserMode.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryBrowser"/> class.
        /// </summary>
        /// <param name="mode">The brower mode to use.</param>
        public DirectoryBrowser(DirectoryBrowserMode mode)
        {
            this.mode = mode;
        }

        /// <summary>
        /// Finds the entry for the active user.
        /// </summary>
        /// <returns>The active user entry, or <c>null</c> if not found.</returns>
        public UserEntry FindMe()
        {
            var sid = WindowsIdentity.GetCurrent().User.Value;
            var result = FindUsersBySids(new string[] { sid }).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Finds the entry for a given user by account name.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <returns>The user entry, or <c>null</c> if not found.</returns>
        public UserEntry FindUserByAccountName(string accountName)
        {
            Assert.ParamIsNotNullOrEmpty(accountName, "accountName");

            return FindUsersByAccountName(new string[] { accountName }).FirstOrDefault();
        }

        /// <summary>
        /// Finds the user entries for one or more account names.
        /// </summary>
        /// <param name="mailAddresses">The account names.</param>
        /// <returns>The found user entries. Entries might be missing for non matching user names 
        /// (so the result array does not necessarily match the input collection count)</returns>
        public UserEntry[] FindUsersByAccountName(IEnumerable<string> accountNames)
        {
            Assert.ParamIsNotNull(accountNames, "accountNames");

            return UserEntry.FromResults(SearchUsersByProperty(DirectoryProperties.SAMAccountName, accountNames));
        }

        /// <summary>
        /// Finds the user entries for one or more account names.
        /// </summary>
        /// <param name="mailAddresses">The account names.</param>
        /// <returns>The found user entries. Entries might be missing for non matching user names 
        /// (so the result array does not necessarily match the input collection count)</returns>
        public UserEntry[] FindUsersByManager(string managerDistinguishedName)
        {
            Assert.ParamIsNotNull(managerDistinguishedName, "managerDistinguishedName");

            return UserEntry.FromResults(SearchUsersByProperty(DirectoryProperties.Manager, new string[] { managerDistinguishedName }));
        }

        /// <summary>
        /// Finds the entry for a given user by mail address.
        /// </summary>
        /// <param name="mailAddress">Mail address.</param>
        /// <returns>The user entry, or <c>null</c> if not found.</returns>
        public UserEntry FindUserByMailAddress(string mailAddress)
        {
            Assert.ParamIsNotNullOrEmpty(mailAddress, "mailAddress");

            return FindUsersByMailAddress(new string[] { mailAddress }).FirstOrDefault();
        }

        /// <summary>
        /// Finds the user entries for one or more mail adderss.
        /// </summary>
        /// <param name="accountNames">The account names.</param>
        /// <returns>The found user entries. Entries might be missing for non matching user names 
        /// (so the result array does not necessarily match the input collection count)</returns>
        public UserEntry[] FindUsersByMailAddress(IEnumerable<string> mailAddresses)
        {
            Assert.ParamIsNotNull(mailAddresses, "mailAddresses");

            return UserEntry.FromResults(SearchUsersByProperty(DirectoryProperties.Mail, mailAddresses));
        }

        /// <summary>
        /// Finds the user entries for a given display name (multiple users might have the same display name).
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns>The matching user entries.</returns>
        public UserEntry[] FindUsersByDisplayName(string displayName)
        {
            Assert.ParamIsNotNullOrEmpty(displayName, "displayName");

            return FindUsersByDisplayName(new string[] { displayName });
        }

        /// <summary>
        /// Finds the user entries for one or more display names.
        /// </summary>
        /// <param name="displayNames">The display names.</param>
        /// <returns>The matching user entries.</returns>
        public UserEntry[] FindUsersByDisplayName(IEnumerable<string> displayNames)
        {
            Assert.ParamIsNotNull(displayNames, "displayNames");

            return UserEntry.FromResults(SearchUsersByProperty(DirectoryProperties.DisplayName, displayNames));
        }

        /// <summary>
        /// Finds the user entries for one or more SIDs (security identifiers).
        /// </summary>
        /// <param name="sids">The SIDs.</param>
        /// <returns>The matching user entries.</returns>
        public UserEntry[] FindUsersBySids(IEnumerable<string> sids)
        {
            Assert.ParamIsNotNull(sids, "sids");

            // See http://us.generation-nt.com/answer/how-get-correct-sid-format-so-i-can-search-help-37063152.html
            // We need to convert the string sids into a more readable hex format
            return UserEntry.FromResults(SearchUsersByProperty(DirectoryProperties.ObjectSid, sids.Select(s => SidToHex(s))));
        }

        /// <summary>
        /// Finds the user entries by distinguished names.
        /// </summary>
        /// <param name="distinguishedNames">The distinguished names.</param>
        /// <returns>The matching user entries.</returns>
        public UserEntry[] FindUsersByDistinguishedNames(IEnumerable<string> distinguishedNames)
        {
            Assert.ParamIsNotNull(distinguishedNames, "distinguishedNames");

            return UserEntry.FromResults(SearchUsersByProperty(DirectoryProperties.DistinguishedName, distinguishedNames));
        }

        /// <summary>
        /// Creates a directory searcher and initializes it with the appropriate search root and properties to load.
        /// </summary>
        private DirectorySearcher CreateDirectorySearcher()
        {
            DirectorySearcher searcher = new DirectorySearcher(DefaultSearchRoot);

            foreach (string property in UserEntry.RequiredPropertyNames)
            {
                searcher.PropertiesToLoad.Add(property);
            }

            if ((mode & DirectoryBrowserMode.Light) != DirectoryBrowserMode.Light)
            {
                foreach (string property in UserEntry.ExtraPropertyNames)
                {
                    searcher.PropertiesToLoad.Add(property);
                }
            }

            return searcher;
        }

        /// <summary>
        /// Converts a SID (security identifier) to hexadecimal format used for searching. This is the format in which
        /// SIDs are stored and searched for in Active Directory.
        /// </summary>
        /// <param name="sid">The sid.</param>
        /// <returns>The converted sid string.</returns>
        private static string SidToHex(string sid)
        {
            SecurityIdentifier s = new SecurityIdentifier(sid);
            byte[] sidBytes = new byte[s.BinaryLength];
            s.GetBinaryForm(sidBytes, 0);
            string hexSid = '\\' + BitConverter.ToString(sidBytes).ToLower().Replace('-', '\\');
            return hexSid;
        }

        /// <summary>
        /// Searches Active Directory for users by a given property name and one or more matching values.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="values">The matching values.</param>
        /// <returns>The search result collection.</returns>
        private SearchResultCollection SearchUsersByProperty(string property, IEnumerable<string> values)
        {
            DirectorySearcher searcher = CreateDirectorySearcher();
            searcher.Filter = "(&(objectClass=user)" + OrFilter(property, values) + ")";
            return searcher.FindAll();
        }

        /// <summary>
        /// Creates an Active Directory search expression filter to match the given property name and one or more values.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="values">The values.</param>
        /// <returns>The search expression filter.</returns>
        private static string OrFilter(string propertyName, IEnumerable<string> values)
        {
            StringBuilder filter = new StringBuilder();
            foreach (object value in values)
            {
                filter.AppendFormat("({0}={1})", propertyName, value);
            }

            return (filter.Length > 0) ? "(|" + filter + ")" : String.Empty;
        }

        /// <summary>
        /// Resolves the default search root for the current domain.
        /// </summary>
        /// <value>
        /// The default search root.
        /// </value>
        public DirectoryEntry DefaultSearchRoot
        {
            get
            {
                // See http://www.infini-tec.de/post/2004/12/30/Find-an-user-in-a-multi-domain-Active-Directory-enironment-programmatically.aspx
                if (!defaultSearchRootResolved)
                {
                    // Get the directory entry for the global catalog root
                    DirectoryEntry gc = new DirectoryEntry("GC:");

                    // The catalog root has a single child which is the root node for searching
                    defaultSearchRoot = gc.Children.OfType<DirectoryEntry>().FirstOrDefault();
                    defaultSearchRootResolved = true;
                }

                return defaultSearchRoot;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we are currently in the context of an Active Directory domain.
        /// </summary>
        public bool IsInDomain
        {
            get
            {
                // Seems to work, alternatively look into something like Domain.GetComputerDomain() maybe...
                return (DefaultSearchRoot != null);
            }
        }
    }

    /// <summary>
    /// A set of flags that indicate what values a directory browser should populate in returned
    /// user entries.
    /// </summary>
    [Flags]
    public enum DirectoryBrowserMode
    {
        Default,

        /// <summary>
        /// Populate lightweight user values only (e.g. no user thumbnail)
        /// </summary>
        Light = 0x01
    }

    /// <summary>
    /// Contains the well known names of certain user Active Directory properties.
    /// </summary>
    internal static class DirectoryProperties
    {
        public const string DisplayName = "displayname";                // Joe Stevens
        public const string CommonName = "cn";                          // Joseph Stevens
        public const string UserPrincipalName = "userprincipalname";    // jstevens@microsoft.com
        public const string ThumbnailPhoto = "thumbnailphoto";          // bytes[]
        public const string MailNickname = "mailnickname";              // jstevens
        public const string Title = "title";                            // SENIOR SDE
        public const string Mail = "mail";                              // jstevens@microsoft.com
        public const string DistinguishedName = "distinguishedname";    // CN=Joe Stevens,OU=UserAccounts,DC=redmond,DC=corp,DC=microsoft,DC=com
        public const string Manager = "manager";                        // CN=Peter Parker,OU=UserAccounts,DC=europe,DC=corp,DC=microsoft,DC=com
        public const string SpokenName = "msexchumspokenname";          // bytes[]
        public const string TelephoneNumber = "telephonenumber";        // 
        public const string HomePage = "wwwhomepage";                   // 
        public const string StreetAddress = "streetaddress";            // 
        public const string State = "st";                               // 
        public const string GivenName = "givenname";                    // 
        public const string Surname = "sn";                             // 
        public const string PostalCode = "postalcode";                  // 
        public const string SAMAccountName = "sAMAccountName";          // jstevens
        public const string ObjectSid = "objectsid";                    // 
        public const string Sip = "msRTCSIP-PrimaryUserAddress";        // sip:jstevens@microsoft.com (used for OCS)
        public const string Department = "department";                  // Visual Studio
    }
}

