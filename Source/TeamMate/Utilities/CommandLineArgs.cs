using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Utilities
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class CommandLineArgs
    {
        private string[] args;
        private IDictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private ICollection<string> switches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public const string SwitchChar = "/";

        public static CommandLineArgs Parse(string[] args)
        {
            Assert.ParamIsNotNull(args, "args");

            CommandLineArgs result = new CommandLineArgs();
            result.args = args;

            foreach (string arg in args)
            {
                bool isValid = false;
                if (arg.StartsWith(SwitchChar))
                {
                    string value = arg.Substring(1);
                    if (value.Length > 0)
                    {
                        int indexOfSeparator = value.IndexOf(':');
                        if (indexOfSeparator >= 0)
                        {
                            string key = value.Substring(0, indexOfSeparator);
                            string keyValue = value.Substring(indexOfSeparator + 1);

                            if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(keyValue))
                            {
                                result.SetValue(key, keyValue);
                                isValid = true;

                                // TODO: What if it is specified multiple times?
                            }
                        }
                        else
                        {
                            result.SetSwitch(value);
                            isValid = true;

                            // TODO: What if it is specified multiple times?
                        }
                    }
                }

                if (!isValid)
                {
                    // TODO: What do we do with these guys?
                    string message = String.Format("Unexpected command line argument {0}", arg);
                    throw new Exception(message);
                }
            }

            return result;
        }

        public void SetSwitch(string value)
        {
            Assert.ParamIsNotNull(value, "value");

            switches.Add(value);
        }

        public void SetValue(string key, string value)
        {
            Assert.ParamIsNotNull(key, "key");
            Assert.ParamIsNotNull(value, "value");

            values[key] = value;
        }

        public bool Any()
        {
            return switches.Any() || values.Any();
        }

        public bool HasValue(string arg)
        {
            Assert.ParamIsNotNull(arg, "arg");

            return values.ContainsKey(arg);
        }

        public bool HasSwitch(string arg)
        {
            Assert.ParamIsNotNull(arg, "arg");

            return switches.Contains(arg);
        }

        public void RemoveValue(string arg)
        {
            this.values.Remove(arg);
        }

        public T GetValue<T>(string arg)
        {
            Assert.ParamIsNotNull(arg, "arg");

            string value;
            if (!TryGetValue(arg, out value))
            {
                throw new Exception(String.Format("Please specify a value for {0}", ToSwitch(arg)));
            }

            return ChangeType<T>(arg, value);
        }

        private static string ToSwitch(string arg)
        {
            return SwitchChar + arg;
        }

        private bool TryGetValue(string arg, out string value)
        {
            return values.TryGetValue(arg, out value);
        }

        public T GetValue<T>(string arg, T defaultValue)
        {
            string value;
            if (TryGetValue(arg, out value))
            {
                return ChangeType<T>(arg, value);
            }

            return defaultValue;
        }

        private static T ChangeType<T>(string arg, string value)
        {
            try
            {
                return ConvertUtilities.ChangeType<T>(value);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Value {0} is an invalid value for {1}", value, ToSwitch(arg)), e);
            }
        }
    }
}
