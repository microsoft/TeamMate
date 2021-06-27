// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Security.Principal;

namespace Microsoft.Tools.TeamMate.Foundation.DirectoryServices
{
    /// <summary>
    /// Describes an entry for a user in Active Directory.
    /// </summary>
    public class UserEntry
    {
        /// <summary>
        /// The names of the required properties to load from Active Directory to populate a user.
        /// </summary>
        public static readonly ICollection<string> RequiredPropertyNames = new ReadOnlyCollection<string>(new string[] {
            DirectoryProperties.SAMAccountName,  DirectoryProperties.DisplayName, DirectoryProperties.GivenName, 
            DirectoryProperties.Surname, DirectoryProperties.DistinguishedName, DirectoryProperties.UserPrincipalName, 
            DirectoryProperties.MailNickname, DirectoryProperties.Mail, DirectoryProperties.CommonName, DirectoryProperties.Sip,
            DirectoryProperties.ObjectSid, DirectoryProperties.Title, DirectoryProperties.Department
        });


        /// <summary>
        /// The advanced additional property names to load from Active Directory for a user (depending on the mode).
        /// </summary>
        public static readonly ICollection<string> ExtraPropertyNames = new ReadOnlyCollection<string>(new string[] {
            DirectoryProperties.ThumbnailPhoto
        });

        /// <summary>
        /// Creates a user entry from a directory entry.
        /// </summary>
        /// <param name="entry">The directory entry.</param>
        /// <returns>The user entry.</returns>
        public static UserEntry FromEntry(DirectoryEntry entry)
        {
            Assert.ParamIsNotNull(entry, "entry");

            return FromEntryOrResult(entry);
        }

        /// <summary>
        /// Creates a user entry from a search result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>A user entry.</returns>
        public static UserEntry FromResult(SearchResult result)
        {
            Assert.ParamIsNotNull(result, "result");

            return FromEntryOrResult(result);
        }

        /// <summary>
        /// Creates one or more user entries from a search result collection.
        /// </summary>
        /// <param name="results">The search results.</param>
        /// <returns>The user entries.</returns>
        public static UserEntry[] FromResults(SearchResultCollection results)
        {
            Assert.ParamIsNotNull(results, "results");

            UserEntry[] userEntries = new UserEntry[results.Count];

            for (int i = 0; i < results.Count; i++)
            {
                userEntries[i] = UserEntry.FromResult(results[i]);
            }

            return userEntries;
        }

        /// <summary>
        /// Creates a user entry from a directory entry or search result.
        /// </summary>
        /// <param name="entryOrResult">The directory entry or search result.</param>
        /// <returns>The user entry.</returns>
        public static UserEntry FromEntryOrResult(object entryOrResult)
        {
            Assert.ParamIsNotNull(entryOrResult, "entryOrResult");

            UserEntry item = new UserEntry();
            item.AccountName = GetPropertyValue<string>(entryOrResult, DirectoryProperties.SAMAccountName);
            item.CommonName = GetPropertyValue<string>(entryOrResult, DirectoryProperties.CommonName);
            item.DisplayName = GetPropertyValue<string>(entryOrResult, DirectoryProperties.DisplayName);
            item.GivenName = GetPropertyValue<string>(entryOrResult, DirectoryProperties.GivenName);
            item.Surname = GetPropertyValue<string>(entryOrResult, DirectoryProperties.Surname);
            item.DistinguishedName = GetPropertyValue<string>(entryOrResult, DirectoryProperties.DistinguishedName);
            item.UserPrincipalName = GetPropertyValue<string>(entryOrResult, DirectoryProperties.UserPrincipalName);
            item.MailNickname = GetPropertyValue<string>(entryOrResult, DirectoryProperties.MailNickname);
            item.Mail = GetPropertyValue<string>(entryOrResult, DirectoryProperties.Mail);
            item.Sip = GetPropertyValue<string>(entryOrResult, DirectoryProperties.Sip);
            item.Title = GetPropertyValue<string>(entryOrResult, DirectoryProperties.Title);
            item.Department = GetPropertyValue<string>(entryOrResult, DirectoryProperties.Department);

            byte[] sidBytes = GetPropertyValue<byte[]>(entryOrResult, DirectoryProperties.ObjectSid);
            if (sidBytes != null)
            {
                item.Sid = new SecurityIdentifier(sidBytes, 0).Value;
            }

            byte[] photoBytes = GetPropertyValue<byte[]>(entryOrResult, DirectoryProperties.ThumbnailPhoto);
            item.ThumbnailPhotoBytes = photoBytes;

            return item;
        }

        /// <summary>
        /// Gets a property value by name from a directory entry or search result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entryOrResult">The directory entry or search result.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The property value, or default value type if the property was not available.</returns>
        private static T GetPropertyValue<T>(object entryOrResult, string propertyName)
        {
            object value = null;

            if (entryOrResult is DirectoryEntry)
            {
                PropertyValueCollection values = ((DirectoryEntry)entryOrResult).Properties[propertyName];
                value = (values != null && values.Count > 0) ? values[0] : null;
            }
            else if (entryOrResult is SearchResult)
            {
                ResultPropertyValueCollection values = ((SearchResult)entryOrResult).Properties[propertyName];
                value = (values != null && values.Count > 0) ? values[0] : null;
            }

            return (value is T) ? (T)value : default(T);
        }

        /// <summary>
        /// Gets the name of the account.
        /// </summary>
        public string AccountName { get; private set; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the name of the given.
        /// </summary>
        public string GivenName { get; private set; }

        /// <summary>
        /// Gets the surname.
        /// </summary>
        public string Surname { get; private set; }

        /// <summary>
        /// Gets the distinguished name.
        /// </summary>
        public string DistinguishedName { get; private set; }

        /// <summary>
        /// Gets the user principal name.
        /// </summary>
        public string UserPrincipalName { get; private set; }

        /// <summary>
        /// Gets the mail nickname.
        /// </summary>
        public string MailNickname { get; private set; }

        /// <summary>
        /// Gets the mail address.
        /// </summary>
        public string Mail { get; private set; }

        /// <summary>
        /// Gets the thumbnail photo bytes.
        /// </summary>
        public byte[] ThumbnailPhotoBytes { get; private set; }

        /// <summary>
        /// Gets the the common name.
        /// </summary>
        public string CommonName { get; private set; }

        /// <summary>
        /// Gets the users SIP address (e.g. for Lync). Might or might not match the users email address.
        /// </summary>
        public string Sip { get; private set; }

        /// <summary>
        /// Gets the sid.
        /// </summary>
        public string Sid { get; private set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets the department
        /// </summary>
        public string Department { get; set; }
        
    }
}
