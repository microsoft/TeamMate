using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;

namespace Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    public class WorkItemHierarchyNode
    {
        public WorkItem WorkItem { get; set; }

        public WorkItemLink Link { get; set; }

        public WorkItemHierarchyNode Parent { get; set; }

        public int Level { get; set; }

        public ICollection<WorkItemHierarchyNode> Children { get; } = new List<WorkItemHierarchyNode>();

        public void Visit(Action<WorkItemHierarchyNode> action)
        {
            action(this);
            foreach (var child in this.Children)
            {
                child.Visit(action);
            }
        }
    }
}
