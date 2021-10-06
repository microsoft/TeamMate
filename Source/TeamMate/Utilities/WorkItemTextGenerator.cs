using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public class WorkItemTextGenerator
    {
        private static IEnumerable<WorkItemUpdate> GetHistoryUpdatesInReverseOrder(ICollection<WorkItemUpdate> updates)
        {
            foreach (var update in updates.Reverse())
            {
                WorkItemFieldUpdate historyUpdate;
                if (update.Fields != null && update.Fields.TryGetValue(WorkItemConstants.CoreFields.History, out historyUpdate) && historyUpdate.NewValue != null)
                {
                    yield return update;
                }
            }
        }

        public string GenerateText(WorkItemWithUpdates workItemWithUpdates)
        {
            Assert.ParamIsNotNull(workItemWithUpdates, "workItemWithUpdates");
            var workItem = workItemWithUpdates.WorkItem;

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(workItemWithUpdates.WorkItem.GetFullTitle());
            stringBuilder.AppendLine();

            foreach (WorkItemUpdate revision in GetHistoryUpdatesInReverseOrder(workItemWithUpdates.Updates))
            {
                string changedBy = revision.RevisedBy.Name;
                DateTime changedDate = revision.GetField<DateTime>(WorkItemConstants.CoreFields.ChangedDate).Value;

                stringBuilder.Append("Changed by ");
                stringBuilder.Append(changedBy);
                stringBuilder.AppendLine(changedDate.ToFriendlyShortDateString());
            }

            return stringBuilder.ToString();
        }

        public string GenerateText(WorkItemQueryExpandedResult result)
        {
            Assert.ParamIsNotNull(result, "result");

            StringBuilder stringBuilder = new StringBuilder();

            foreach (var workItem in result.WorkItems)
            {
                stringBuilder.AppendLine(workItem.GetFullTitle());
            }

            return stringBuilder.ToString();
        }
        public string GenerateText(ICollection<WorkItem> workItems)
        {
            Assert.ParamIsNotNull(workItems, "workItems");

            StringBuilder stringBuilder = new StringBuilder();

            foreach (var workItem in workItems)
            {
                stringBuilder.AppendLine(workItem.GetFullTitle());
            }

            return stringBuilder.ToString();
        }
    }
}
