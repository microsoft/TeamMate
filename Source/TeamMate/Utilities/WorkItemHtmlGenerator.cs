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
using System.Text;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Utilities
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class WorkItemHtmlGenerator
    {
        private IDictionary<string, WorkItemField> workItemFieldsByName;
        private HyperlinkFactory hyperlinkFactory;

        public WorkItemHtmlGenerator(IDictionary<string, WorkItemField> workItemFieldsByName, HyperlinkFactory hyperlinkFactory)
        {
            this.workItemFieldsByName = workItemFieldsByName;
            this.hyperlinkFactory = hyperlinkFactory;
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

        public string GenerateHtml(WorkItemQueryExpandedResult result)
        {
            Assert.ParamIsNotNull(result, "result");

            WorkItemHtmlFormatter formatter = GetHtmlFormatter();

            StringWriter writer = new StringWriter();
            formatter.FormatResults(result, writer);

            return writer.ToString();
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

        public string GenerateHtml(ICollection<WorkItem> workItems)
        {
            Assert.ParamIsNotNull(workItems, "workItems");

            WorkItemHtmlFormatter formatter = GetHtmlFormatter();

            StringWriter writer = new StringWriter();
            formatter.FormatResults(workItems, writer);

            return writer.ToString();
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
