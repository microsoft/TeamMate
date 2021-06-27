using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    public class WorkItemQuery
    {
        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string TeamName { get; set; }

        public string QueryPath { get; set; }

        public Guid QueryId { get; set; }

        public string Wiql { get; set; }

        public ICollection<string> RequiredFields { get; set; } = new List<string>();

        public WorkItemExpand? Expand { get; set; }

        public DateTime? AsOf { get; set; }

        public int? MaxItemsToFetch { get; set; }

        public void Validate()
        {
            int projectInputs = (ProjectId != Guid.Empty ? 1 : 0) + (ProjectName != null ? 1 : 0);
            if (projectInputs == 0)
            {
                throw new InvalidOperationException("You must specify a ProjectId or ProjectName");
            }
            else if (projectInputs > 1)
            {
                throw new InvalidOperationException("You must specify only one of ProjectId or ProjectName");
            }

            int inputs = (QueryPath != null ? 1 : 0) + (Wiql != null ? 1 : 0) + (QueryId != Guid.Empty ? 1 : 0);
            if (inputs == 0)
            {
                throw new InvalidOperationException("You must specify a QueryPath, QueryId or Wiql");
            }
            else if (inputs > 1)
            {
                throw new InvalidOperationException("You must specify only one of QueryPath, QueryId or Wiql");
            }
        }
    }
}
