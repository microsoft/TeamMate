using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Collections.Generic;

namespace Microsoft.Internal.Tools.TeamMate.Model
{
    public class WorkItemWithUpdates
    {
        public WorkItemWithUpdates(WorkItem workItem, ICollection<WorkItemUpdate> updates)
        {
            this.WorkItem = workItem;
            this.Updates = updates;
        }

        public WorkItem WorkItem { get; private set; }

        public ICollection<WorkItemUpdate> Updates { get; private set; }
    }
}
