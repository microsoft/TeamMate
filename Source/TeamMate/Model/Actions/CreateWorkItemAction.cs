
namespace Microsoft.Internal.Tools.TeamMate.Model.Actions
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
