using Microsoft.Tools.TeamMate.Foundation;
using System;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Utilities
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public static class Formatter
    {
        public const string NotYetAssigned = "Not Yet Assigned";
        public const string Undefined = "Undefined";

        public static string FormatPriority(int priority)
        {
            return String.Format("P{0}", priority);
        }

        public static string FormatAssignedTo(string assignedTo)
        {
            return (!String.IsNullOrWhiteSpace(assignedTo))? assignedTo : NotYetAssigned;
        }

        public static object FormatUndefined(object value)
        {
            bool isNullOrWhitespace = ObjectUtilities.IsNullOrWhiteSpaceString(value);
            return (!isNullOrWhitespace) ? value : Undefined;
        }
    }
}
