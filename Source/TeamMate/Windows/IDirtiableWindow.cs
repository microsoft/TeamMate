// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.Tools.TeamMate.Windows
{
    public interface IDirtiableWindow
    {
        bool NoPrompt { get; set; }

        bool PromptSaveIfDirty();
    }
}
