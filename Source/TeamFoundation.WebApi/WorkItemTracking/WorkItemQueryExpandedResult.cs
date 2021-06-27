using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    public class WorkItemQueryExpandedResult
    {
        public WorkItemQuery Query { get; set; }

        public QueryHierarchyItem QueryHierarchyItem { get; set; }

        public WorkItemQueryResult QueryResult { get; set; }

        public ICollection<WorkItem> WorkItems { get; set; }

        public WorkItemHierarchy WorkItemHierarchy { get; set; }

        public bool IsFlatQuery => this.WorkItemHierarchy == null;
    }
}
