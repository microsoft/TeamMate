
namespace Microsoft.Internal.Tools.TeamMate.Model.Actions
{
    public abstract class TeamMateAction
    {
        protected TeamMateAction(ActionType type)
        {
            this.Type = type;
        }

        public ActionType Type { get; private set; }
        public bool DeleteOnLoad { get; set; }
    }

    public enum ActionType
    {
        CreateWorkItem
    }
}
