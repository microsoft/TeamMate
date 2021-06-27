// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    public class WorkItemIdentity
    {
        public static string GetDisplayName(string identityName)
        {
            if (!string.IsNullOrEmpty(identityName))
            {
                var identity = TryParse(identityName);
                if (identity != null)
                {
                    identityName = identity.DisplayName;
                }
            }

            return identityName;
        }

        public static WorkItemIdentity TryParse(string name)
        {
            int indexOfMailStart = name.IndexOf(" <");
            if (indexOfMailStart >= 0)
            {
                int indexOfMailEnd = name.LastIndexOf('>');
                if (indexOfMailEnd >= 0 && indexOfMailEnd > indexOfMailStart && indexOfMailEnd == name.Length - 1)
                {
                    string displayName = name.Substring(0, indexOfMailStart);
                    int length = indexOfMailEnd - (indexOfMailStart + 2);
                    string mail = name.Substring(indexOfMailStart + 2, length);
                    return new WorkItemIdentity(displayName, mail, name);
                }
            }

            return null;
        }

        public static WorkItemIdentity Parse(string name)
        {
            var result = TryParse(name);
            if (result == null)
            {
                throw new FormatException($"Work item identity display name was in incorrect format: '{name}'");
            }

            return result;
        }

        public WorkItemIdentity(string displayName, string emailAddress) 
            : this(displayName, emailAddress, ToFullName(displayName, emailAddress))
        {
        }

        private WorkItemIdentity(string displayName, string emailAddress, string fullName)
        {
            this.DisplayName = displayName;
            this.EmailAddress = emailAddress;
            this.FullName = fullName;
        }

        public string DisplayName { get; private set; }

        public string EmailAddress { get; private set; }

        public string FullName { get; private set; }

        public override string ToString()
        {
            return this.FullName;
        }

        public override int GetHashCode()
        {
            return this.FullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            WorkItemIdentity other = obj as WorkItemIdentity;
            if (other != null)
            {
                return String.Equals(this.FullName, other.FullName);
            }

            return false;
        }

        private static string ToFullName(string displayName, string emailAddress)
        {
            return (emailAddress != null) ? $"{displayName} <{emailAddress}>" : displayName;
        }
    }
}
