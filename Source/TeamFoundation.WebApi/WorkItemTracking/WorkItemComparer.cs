using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    /// <summary>
    /// Compares work items by id.
    /// </summary>
    /// <remarks>
    public class WorkItemComparer : IEqualityComparer<WorkItem>
    {
        public static WorkItemComparer Instance { get; } = new WorkItemComparer();

        public bool Equals(WorkItem x, WorkItem y)
        {
            if (x.Id != null && y.Id != null)
            {
                return x.Id == y.Id;
            }

            return Object.Equals(x, y);
        }

        public int GetHashCode(WorkItem item)
        {
            return item.Id != null ? item.Id.Value : 0;
        }
    }
}
