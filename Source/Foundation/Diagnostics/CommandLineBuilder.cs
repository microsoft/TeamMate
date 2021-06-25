using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// A helper class to build command line arguments for starting processes.
    /// </summary>
    /// <remarks>
    /// This class takes care of escaping arguments as needed for argument values that
    /// require escaping.
    /// </remarks>
    public class CommandLineBuilder
    {
        private static readonly char[] EscapeChars = new char[] { ' ', '\t', '\"' };

        private List<string> args = new List<string>();

        /// <summary>
        /// Determines if an argument value requires escaping.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if it requires esaping, <c>false</c> if it can be used as is.</returns>
        public static bool NeedsEscaping(string value)
        {
            return value.IndexOfAny(EscapeChars) >= 0;
        }

        /// <summary>
        /// Adds an argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        public void Add(string arg)
        {
            this.args.Add(arg);
        }

        /// <summary>
        /// Adds one or more arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Add(params string[] args)
        {
            this.args.AddRange(args);
        }

        /// <summary>
        /// Adds one or more arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Add(IEnumerable<string> args)
        {
            this.args.AddRange(args);
        }

        /// <summary>
        /// Adds two arguments, a switch name and a switch value (e.g. "/priority" "1"), if and only
        /// if, the value is not empty.
        /// </summary>
        /// <param name="switchName">The switch name.</param>
        /// <param name="switchValue">The switch value (possibly empty).</param>
        public void AddSwitchIfNotNull(string switchName, string switchValue)
        {
            if (!String.IsNullOrWhiteSpace(switchValue))
            {
                Add(switchName, switchValue);
            }
        }

        /// <summary>
        /// Returns the escaped command line arguments as a single string.
        /// </summary>
        public override string ToString()
        {
            return String.Join(" ", args.Select(arg => EscapeArgument(arg)));
        }

        /// <summary>
        /// Escapes an argument if required.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The escaped value (or the original one if no escaping was required).</returns>
        private static string EscapeArgument(string value)
        {
            bool needsEscaping = NeedsEscaping(value);
            if (needsEscaping)
            {
                char escapeChar = (value.Contains('"')) ? '\'' : '\"';
                return escapeChar + value + escapeChar;
            }

            return value;
        }
    }
}
