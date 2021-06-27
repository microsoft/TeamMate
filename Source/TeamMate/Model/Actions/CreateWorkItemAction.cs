// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.Tools.TeamMate.Model.Actions
{
    class CreateWorkItemAction : TeamMateAction
    {
        public CreateWorkItemAction(WorkItemUpdateInfo updateInfo)
            : base(ActionType.CreateWorkItem)
        {
            this.UpdateInfo = updateInfo;
        }

        public WorkItemUpdateInfo UpdateInfo { get; private set; }
    }
}
