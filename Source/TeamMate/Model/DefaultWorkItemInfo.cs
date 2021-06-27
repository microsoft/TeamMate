// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;

namespace Microsoft.Tools.TeamMate.Model
{
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
