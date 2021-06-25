using Microsoft.Internal.Tools.TeamMate.Foundation.DirectoryServices;
using System;

namespace Microsoft.Internal.Tools.TeamMate.Platform.CodeFlow
{
    // TODO: Try and deprecate this... Should be deprecated by the correct use of the CodeFlowService class
    // There's currently 2 IsMe() extension methods making use of this.
    public static class CodeFlowContext
    {
        /// <summary>
        /// Attempts to retrieve my username account for CodeFlow. Can return null when not connected to a domain.
        /// </summary>
        public static string MyCodeFlowAccount
        {
            get
            {
                string accountName = GetMyCodeFlowAccountRaw();

                /*
                if (accountName == null)
                {
                    throw new Exception("Could not determine CodeFlow account, you are likely working in a non-domain environment (e.g. working remotely without access to CodeFlow.)");
                }
                 */

                return accountName;
            }
        }

        public static bool IsCodeFlowAvailable
        {
            get
            {
                return GetMyCodeFlowAccountRaw() != null;
            }
        }

        private static string GetMyCodeFlowAccountRaw()
        {
            // TODO: Maybe check that domain is Microsoft too?
            // TODO: Could we still get the CodeFlow username in a VPN environment?
            if (DirectoryUtilities.IsCurrentUserInDomain)
            {
                return Environment.UserName;
            }

            return null;
        }
    }
}
