// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Tools.TeamMate.Foundation.CommandLine
{
    public interface ICommand
    {
        string CommandName { get; }

        string CommandDescription { get; }

        string CommandUsage { get; }

        void Run(string[] args);
    }
}
