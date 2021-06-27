using System;

namespace Microsoft.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides utility methods to format data.
    /// </summary>
    public static class FormatUtilities
    {
        /// <summary>
        /// Formats bytes into a friendly string (e.g. KB, MB, GB, etc.)
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatBytes(long bytes)
        {
            string[] units = { "bytes", "KB", "MB", "GB", "TB", "PB" };

            int unitIndex = 0;

            // TODO: We are losing some rounding here, e.g. 3.72 MB gets rounded to 3MB
            while (bytes >= 1024 && unitIndex < units.Length)
            {
                bytes /= 1024;
                unitIndex++;
            }

            return String.Format("{0} {1}", bytes, units[unitIndex]);
        }
    }
}
