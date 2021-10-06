using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.IO;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public class WorkItemHtmlGenerator
    {
        private IDictionary<string, WorkItemField> workItemFieldsByName;
        private HyperlinkFactory hyperlinkFactory;

        public WorkItemHtmlGenerator(IDictionary<string, WorkItemField> workItemFieldsByName, HyperlinkFactory hyperlinkFactory)
        {
            this.workItemFieldsByName = workItemFieldsByName;
            this.hyperlinkFactory = hyperlinkFactory;
        }

        private static string GetUniqueFilename(string directory, string extension)
        {
            Assert.ParamIsNotNull(directory, "directory");
            Assert.ParamIsNotNull(extension, "extension");

            string path;

            do
            {
                // IMPORTANT, KLUDGE! Apparently if the filename had spaces or parenthesis, Outlook did not
                // render the attachment nicely inline... Hence, we use a custom routine to ensure filenames are unique,
                // again, the important part is to make sure the generated filename has no spaces or parenthesis!
                path = Path.Combine(directory, Path.ChangeExtension(Path.GetRandomFileName(), extension));
            }
            while (PathUtilities.Exists(path));

            return path;
        }

        public string GenerateHtml(WorkItemWithUpdates workItemWithUpdates)
        {
            Assert.ParamIsNotNull(workItemWithUpdates, "workItemWithUpdates");
            var workItem = workItemWithUpdates.WorkItem;

            ICollection<string> attachments = new List<string>();
            Dictionary<string, string> thumbnails = new Dictionary<string, string>();
            string unknownImagePath = null;

            WorkItemHtmlFormatter formatter = GetHtmlFormatter();
            formatter.Thumbnails = thumbnails;
            formatter.NoThumbnail = (!String.IsNullOrEmpty(unknownImagePath)) ? Path.GetFileName(unknownImagePath) : null;
            StringWriter writer = new StringWriter();
            formatter.FormatWorkItem(workItemWithUpdates, writer);

            return writer.ToString();
        }

        private WorkItemHtmlFormatter GetHtmlFormatter()
        {
            return new WorkItemHtmlFormatter(this.workItemFieldsByName, this.hyperlinkFactory);
        }

        private static ICollection<string> GetAllChangers(WorkItemWithUpdates workItemWithUpdates)
        {
            var updates = workItemWithUpdates.Updates;
            List<string> allChangedBy = new List<string>();
            foreach (var update in updates)
            {
                WorkItemFieldUpdate historyUpdate;
                if (update.Fields != null
                    && update.Fields.TryGetValue(WorkItemConstants.CoreFields.History, out historyUpdate)
                    && historyUpdate.NewValue != null)
                {
                    string changedBy = update.RevisedBy.Name;
                    if (!string.IsNullOrEmpty(changedBy))
                    {
                        allChangedBy.Add(changedBy);
                    }
                }
            }

            allChangedBy = allChangedBy.Distinct().ToList();
            return allChangedBy;
        }

        public string GenerateHtml(WorkItemQueryExpandedResult result)
        {
            Assert.ParamIsNotNull(result, "result");

            WorkItemHtmlFormatter formatter = GetHtmlFormatter();

            StringWriter writer = new StringWriter();
            formatter.FormatResults(result, writer);

            return writer.ToString();
        }

        public string GenerateHtml(ICollection<WorkItem> workItems)
        {
            Assert.ParamIsNotNull(workItems, "workItems");

            WorkItemHtmlFormatter formatter = GetHtmlFormatter();

            StringWriter writer = new StringWriter();
            formatter.FormatResults(workItems, writer);

            return writer.ToString();
        }
    }
}
