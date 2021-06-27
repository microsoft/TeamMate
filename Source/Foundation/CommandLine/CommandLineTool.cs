// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Tools.TeamMate.Foundation.CommandLine
{
    public class CommandLineTool
    {
        private IDictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

        public string Description { get; set; }

        public void RegisterCommandsInNamespace(Type typeInNamespace)
        {
            string nsName = typeInNamespace.Namespace;
            var typesInNamespace = typeInNamespace.Assembly.GetTypes().Where(t => t.Namespace == nsName).ToArray();
            var commandTypes = typesInNamespace.Where(t => typeof(ICommand).IsAssignableFrom(t)).ToArray();

            foreach (var commandType in commandTypes)
            {
                var command = (ICommand)Activator.CreateInstance(commandType);
                RegisterCommand(command);
            }
        }

        public void RegisterCommand(ICommand command)
        {
            if (command.CommandName == null)
            {
                throw new ArgumentException(String.Format("Cannot register a command with a null name. Command type was {0}", command.GetType().FullName), "command");
            }

            ICommand existingCommand;
            if (this.commands.TryGetValue(command.CommandName, out existingCommand))
            {
                throw new InvalidOperationException(string.Format("A command with the name {0} already exists. Command type is {1}",
                    command.CommandName, existingCommand.GetType().Name));
            }

            this.commands[command.CommandName] = command;
        }

        public void Run(string[] args)
        {
            string commandName = args.FirstOrDefault();
            string[] commandArgs = args.Skip(1).ToArray();

            if (commandName == null
                || commandName == "/?"
                || commandName == "-?"
                || (commandName == "help" && commandArgs.Length == 0))
            {
                Console.Write(GetHelp());
                return;
            }

            if (commandName == "help")
            {
                string helpCommandName = commandArgs.First();
                ICommand helpCommand = GetCommand(helpCommandName);
                Console.WriteLine(GetCommandHelp(helpCommand));
                return;
            }

            ICommand command = GetCommand(commandName);
            command.Run(commandArgs);
        }

        private ICommand GetCommand(string commandName)
        {
            ICommand command;
            if (!commands.TryGetValue(commandName, out command))
            {
                throw new InvalidOperationException(string.Format("Unrecognized command: {0}.", commandName));
            }
            return command;
        }

        private string GetHelp()
        {
            StringBuilder sb = new StringBuilder();
            AppendDescription(sb);

            string processName = GetProcessName();
            int maxCommandNameLength = commands.Values.Select(c => c.CommandName.Length).Max();
            int helpLinePaddingLength = processName.Length + 1 + maxCommandNameLength + 2;
            string helpLinePadding = GetPadding(helpLinePaddingLength);

            foreach (var command in commands.Values.OrderBy(c => c.CommandName))
            {
                string padding = GetPadding(maxCommandNameLength - command.CommandName.Length);
                var description = String.Join('\n' + helpLinePadding, command.CommandDescription.Trim().Split('\n'));

                sb.AppendFormat("{0} {1}{2}  {3}", processName, command.CommandName, padding, description);
                sb.AppendLine();
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string GetProcessName()
        {
            return Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.ModuleName);
        }


        /// <summary>
        /// Gets the current program version.
        /// </summary>
        public static string GetProcessVersion()
        {
            var assembly = Assembly.GetEntryAssembly();
            AssemblyFileVersionAttribute fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

            if (fileVersion != null)
            {
                return fileVersion.Version;
            }

            // Fall back to assembly version if a file version is not available...
            return new AssemblyName(assembly.FullName).Version.ToString();
        }

        private void AppendDescription(StringBuilder sb)
        {
            if (this.Description != null)
            {
                var description = this.Description.Trim();
                if (!String.IsNullOrWhiteSpace(description))
                {
                    sb.AppendLine(description);
                    sb.AppendLine();
                }
            }
        }

        private string GetCommandHelp(ICommand helpCommand)
        {
            StringBuilder sb = new StringBuilder();
            AppendDescription(sb);
            sb.AppendLine(helpCommand.CommandDescription.Trim());
            sb.AppendLine();
            sb.AppendLine(helpCommand.CommandUsage.Trim());
            sb.AppendLine();

            string commandHelp = sb.ToString();
            return commandHelp;
        }

        private static string GetPadding(int paddingLength)
        {
            return new StringBuilder(paddingLength).Append(' ', paddingLength).ToString();
        }
    }
}
