using System;

namespace Microsoft.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides utility methods for outputting to the console.
    /// </summary>
    public static class ConsoleUtilities
    {
        /// <summary>
        /// Writes a line to the console using the given foreground color.
        /// </summary>
        /// <param name="foregroundColor">Color of the foreground.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteLine(ConsoleColor foregroundColor, string format, params object[] args)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = foregroundColor;
                Console.WriteLine(format, args);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        /// <summary>
        /// Writes a line to the console using the default color for warnings (yellow).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteWarning(string format, params object[] args)
        {
            WriteLine(ConsoleColor.Yellow, format, args);
        }

        /// <summary>
        /// Writes a line to the console using the default color for errors (red).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteError(string format, params object[] args)
        {
            WriteLine(ConsoleColor.Red, format, args);
        }
    }
}
