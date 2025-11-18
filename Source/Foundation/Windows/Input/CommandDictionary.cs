using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Input
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class CommandDictionary
    {
        private IDictionary<string, ICommand> commands = new Dictionary<string, ICommand>();
        private object commandsLock = new object();

        public ICommand Create([CallerMemberName] string commandName = null)
        {
            return LookupOrCreate(commandName, (name) => new RoutedCommand());
        }

        public ICommand FindResource([CallerMemberName] string commandName = null)
        {
            return LookupOrCreate(commandName, (name) => TryFindResource(name));
        }

        private ICommand LookupOrCreate(string commandName, Func<string, ICommand> creator)
        {
            ICommand result;
            lock (commandsLock)
            {
                if (!commands.TryGetValue(commandName, out result))
                {
                    result = creator(commandName);
                    commands[commandName] = result;
                }
            }

            return result;
        }

        private static ICommand TryFindResource(string commandName)
        {
            var app = Application.Current;
            if (app != null)
            {
                string resourceKey = commandName + "Command";
                return app.FindResource<ICommand>(resourceKey);
            }

            return null;
        }
    }
}
