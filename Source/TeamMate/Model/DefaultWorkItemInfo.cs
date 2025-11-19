using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Model
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class DefaultWorkItemInfo
    {
        public DefaultWorkItemInfo(WorkItemTypeReference workItemType)
        {
            Assert.ParamIsNotNull(workItemType, "workItemType");

            this.WorkItemType = workItemType;
        }

        public WorkItemTypeReference WorkItemType { get; set; }

        public string DisplayName
        {
            get
            {
                return (WorkItemType != null) ? WorkItemType.Name : null;
            }
        }

        public bool IsWorkItemType
        {
            get
            {
                return WorkItemType != null;
            }
        }
    }
}
