using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Tools.TeamMate.Foundation.CommandLine
{
    /// <summary>
    /// A helper class to parse command-lines.
    /// </summary>
    public class CommandLineArgumentParser
    {
        private static readonly ICollection<string> EmptyArray = new string[0];

        private ICollection<string> validOptions = EmptyArray;
        private ICollection<string> validMultiValueOptions = EmptyArray;
        private ICollection<string> validSwitches = EmptyArray;
        private IDictionary<string, IList<string>> arguments = new Dictionary<string, IList<string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineArgumentParser"/> class.
        /// </summary>
        public CommandLineArgumentParser()
        {
        }

        public ICollection<string> ValidOptions
        {
            get { return this.validOptions; }
            set { this.validOptions = value ?? EmptyArray; }
        }

        public ICollection<string> ValidSwitches
        {
            get { return this.validSwitches; }
            set { this.validSwitches = value ?? EmptyArray; }
        }

        public ICollection<string> ValidMultiValueOptions
        {
            get { return this.validMultiValueOptions; }
            set { this.validMultiValueOptions = value ?? EmptyArray; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has any arguments.
        /// </summary>
        public bool HasArguments
        {
            get { return arguments.Any(); }
        }

        /// <summary>
        /// Determines whether a given switch was specified.
        /// </summary>
        public bool HasSwitch(string name)
        {
            return arguments.Keys.Contains(name);
        }

        /// <summary>
        /// Gets the value for a given argument. Returns <c>null</c> if the argument was not defined.
        /// </summary>
        public string GetValue(string name)
        {
            IList<string> values = GetValues(name);
            return values.FirstOrDefault();
        }

        /// <summary>
        /// Gets the multiple available values for a given argument.
        /// </summary>
        public IList<string> GetValues(string name)
        {
            IList<string> values;
            arguments.TryGetValue(name, out values);
            return (values != null) ? values : new string[0];
        }

        /// <summary>
        /// Gets the value for a given argument. Throws an exception if the argument was not defined.
        /// </summary>
        public string GetRequiredValue(string name)
        {
            string value = GetValue(name);
            if (String.IsNullOrEmpty(value))
            {
                throw new CommandLineArgumentException(String.Format("Please specify a value for required option '{0}'.", name));
            }

            return value;
        }

        /// <summary>
        /// Gets an input argument as a required value that needs to be an existing file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The validated input file.</returns>
        public string GetRequiredFile(string name)
        {
            string value = GetRequiredValue(name);

            if (!File.Exists(value))
            {
                throw new CommandLineArgumentException(String.Format("File {0} does not exist", value));
            }

            return value;
        }

        /// <summary>
        /// Gets the URI for a given argument. Returns <c>null</c> if the argument was not defined.
        /// </summary>
        public Uri GetUri(string name)
        {
            Uri result = null;
            string value = GetValue(name);
            if (value != null)
            {
                if (!Uri.TryCreate(value, UriKind.Absolute, out result))
                {
                    throw new CommandLineArgumentException(value + " is not a valid absolute URL");
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the filename for a given argument. Returns <c>null</c> if the argument was not defined.
        /// </summary>
        public string GetExistingFile(string name)
        {
            string filename = GetValue(name);
            if (filename != null)
            {
                if (!File.Exists(filename))
                {
                    throw new CommandLineArgumentException("Could not find file " + filename);
                }
            }

            return filename;
        }

        /// <summary>
        /// Parses the command-line arguments into a dictionary.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public void Parse(IEnumerable<string> args)
        {
            arguments.Clear();

            Regex regex = new Regex("/([^:]+)(?::(.*))?");
            foreach (var arg in args)
            {
                Match match = regex.Match(arg);
                if (match.Success)
                {
                    string argName = match.Groups[1].Value;
                    string value = (match.Groups.Count >= 2) ? match.Groups[2].Value : String.Empty;

                    bool isSwitch = validSwitches != null && validSwitches.Contains(argName);
                    bool isOption = validOptions != null && validOptions.Contains(argName);
                    bool isMultiValueOpton = validMultiValueOptions != null && validMultiValueOptions.Contains(argName);

                    if (!isSwitch && !isOption && !isMultiValueOpton)
                    {
                        throw new CommandLineArgumentException("Unrecognized option " + arg);
                    }

                    if ((isOption || isMultiValueOpton) && String.IsNullOrEmpty(value))
                    {
                        throw new CommandLineArgumentException("Please specify a value for the " + argName + " option");
                    }

                    if ((isSwitch || isOption) && arguments.Keys.Contains(argName))
                    {
                        throw new CommandLineArgumentException("Argument " + argName + " was specified more than once");
                    }

                    IList<string> values;
                    if (!arguments.TryGetValue(argName, out values))
                    {
                        values = new List<string>();
                        arguments[argName] = values;
                    }

                    values.Add(value);
                }
                else
                {
                    throw new CommandLineArgumentException("Unexpected argument " + arg);
                }
            }
        }
    }
}
