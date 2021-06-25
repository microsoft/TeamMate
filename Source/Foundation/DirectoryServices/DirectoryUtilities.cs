using System;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Principal;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.DirectoryServices
{
    /// <summary>
    /// Provides utility methods for Active Directory.
    /// </summary>
    public static class DirectoryUtilities
    {
        private static Lazy<bool> isComputerInDomain = new Lazy<bool>(() => EvaluateIsComputerInDomain());
        private static bool? isCurrentUserInDomain;
        private static string lastUserSid;

        /// <summary>
        /// Gets a value indicating whether the current computer is in a domain.
        /// </summary>
        public static bool IsComputerInDomain
        {
            get { return isComputerInDomain.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether the curren tuser is in a domain.
        /// </summary>
        public static bool IsCurrentUserInDomain
        {
            get
            {
                string currentSid = WindowsIdentity.GetCurrent().User.Value;
                if (!String.Equals(lastUserSid, currentSid))
                {
                    // A simple check for impersonation, clear caches when the user changes...
                    isCurrentUserInDomain = null;
                }

                if (isCurrentUserInDomain == null)
                {
                    lastUserSid = currentSid;
                    isCurrentUserInDomain = EvaluateIsCurrentUserInDomain();
                }

                return isCurrentUserInDomain.Value;
            }
        }

        /// <summary>
        /// Determines whether the current computer is in a domain.
        /// </summary>
        /// <returns><c>true</c> if the current computer is in a domain.</returns>
        [DebuggerStepThrough]
        private static bool EvaluateIsComputerInDomain()
        {
            try
            {
                var ignore = Domain.GetComputerDomain();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the current user is in a domain.
        /// </summary>
        /// <returns><c>true</c> if the current user is in a domain.</returns>
        [DebuggerStepThrough]
        private static bool EvaluateIsCurrentUserInDomain()
        {
            try
            {
                var ignore = Domain.GetCurrentDomain();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
