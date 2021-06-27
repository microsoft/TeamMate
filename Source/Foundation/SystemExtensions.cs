using System;

namespace Microsoft.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides extension methods for types in the System namespace.
    /// </summary>
    public static class SystemExtensions
    {
        // See http://msdn.microsoft.com/en-us/library/windows/desktop/ms724832(v=vs.85).aspx

        /// <summary>
        /// Determines whether an operating system is Windows XP or later.
        /// </summary>
        /// <param name="os">The operating system.</param>
        public static bool IsWindowsXPOrGreater(this OperatingSystem os)
        {
            var version = os.Version;
            return (os.Version.Major > 5 || (os.Version.Major == 5 && os.Version.Minor >= 1));
        }

        /// <summary>
        /// Determines whether an operating system is Windows Vista or later.
        /// </summary>
        /// <param name="os">The operating system.</param>
        public static bool IsVistaOrGreater(this OperatingSystem os)
        {
            return os.Version.Major >= 6;
        }

        /// <summary>
        /// Determines whether an operating system is Windows 7 or later.
        /// </summary>
        /// <param name="os">The operating system.</param>
        public static bool IsWindows7OrGreater(this OperatingSystem os)
        {
            return os.Version.Major >= 6 && os.Version.Minor >= 1;
        }

        /// <summary>
        /// Determines whether an operating system is Windows 8 or later.
        /// </summary>
        /// <param name="os">The operating system.</param>
        public static bool IsWindows8OrGreater(this OperatingSystem os)
        {
            return os.Version.Major >= 6 && os.Version.Minor >= 2;
        }

        /// <summary>
        /// Determines whether an operating system is Windows 10 or later.
        /// </summary>
        /// <param name="os">The operating system.</param>
        public static bool IsWindows10OrGreater(this OperatingSystem os)
        {
            // NOTE: On W10, the version returned will not be "10" unless the app is declared to be
            // compatible with W10 in the .manifest file. 
            // See https://msdn.microsoft.com/en-us/library/windows/desktop/dn481241(v=vs.85).aspx#base.version_helper_apis
            return os.Version.Major >= 10;
        }
    }
}
