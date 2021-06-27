// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Tools.TeamMate.Foundation.CommandLine
{
    public abstract class CommandBase : ICommand
    {
        protected CommandBase()
        {
        }

        protected void Initialize(string name, string description, string usage)
        {
            this.CommandName = name;
            this.CommandDescription = description;
            this.CommandUsage = usage;
        }

        public string CommandName { get; private set; }

        public string CommandDescription { get; private set; }

        public string CommandUsage { get; private set; }

        public abstract void Run(string[] args);
    }
}
