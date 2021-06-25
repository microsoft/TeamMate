using Microsoft.Internal.Tools.TeamMate.Foundation.Collections;
using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.IO;
using Microsoft.Internal.Tools.TeamMate.Model;
using Microsoft.Internal.Tools.TeamMate.Office.Outlook;
using Microsoft.Internal.Tools.TeamMate.Resources;
using Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Utilities
{
    public class WorkItemMailGenerator
    {
        private IDictionary<string, WorkItemField> workItemFieldsByName;
        private HyperlinkFactory hyperlinkFactory;

        public WorkItemMailGenerator(IDictionary<string, WorkItemField> workItemFieldsByName, HyperlinkFactory hyperlinkFactory)
        {
            this.workItemFieldsByName = workItemFieldsByName;
            this.hyperlinkFactory = hyperlinkFactory;
        }

        public string TempDirectory { get; set; }

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

        public MailMessage GenerateEmail(WorkItemWithUpdates workItemWithUpdates)
        {
            Assert.ParamIsNotNull(workItemWithUpdates, "workItemWithUpdates");
            var workItem = workItemWithUpdates.WorkItem;

            string temp = (!String.IsNullOrEmpty(TempDirectory)) ? TempDirectory : Path.GetTempPath();

            ICollection<string> attachments = new List<string>();
            Dictionary<string, string> thumbnails = new Dictionary<string, string>();
            string unknownImagePath = null;


            // TODO: Restore obtaining all unique images for different people, maybe we can use avatars somehow?
            /*
            ICollection<string> allChangedBy = GetAllChangers(workItemWithUpdates);

            foreach (var person in people)
            {
                if (person.ThumbnailPhotoBytes != null)
                {
                    string thumbnailPath = GetUniqueFilename(temp, ".jpg");
                    File.WriteAllBytes(thumbnailPath, person.ThumbnailPhotoBytes);
                    thumbnails[person.DisplayName] = Path.GetFileName(thumbnailPath);
                    attachments.Add(thumbnailPath);
                }
            }
            */

            if (HadAnyHistoryUpdates(workItemWithUpdates))
            {
                // Either a person was not resolved or a they didn't have a thumbnail, generate an unknown one... 
                unknownImagePath = GetUniqueFilename(temp, ".png");
                File.WriteAllBytes(unknownImagePath, EmailResources.ContactPhotoImage);

                // If this is not going to be consumed, we shoudln't attach it, as it will generate an annoying unused
                // attachment visible in Outlook
                attachments.Add(unknownImagePath);
            }

            WorkItemHtmlFormatter formatter = GetHtmlFormatter();
            formatter.Thumbnails = thumbnails;
            formatter.NoThumbnail = (!String.IsNullOrEmpty(unknownImagePath)) ? Path.GetFileName(unknownImagePath) : null;
            formatter.OutlookMode = true;
            StringWriter writer = new StringWriter();
            formatter.FormatWorkItem(workItemWithUpdates, writer);

            MailMessage message = new MailMessage();

            string fullTitle = workItem.GetFullTitle();

            // TODO: Restore IsHighestPriority at some point
            /*
            if (workItem.IsHighestPriority())
            {
                message.Importance = MessageImportance.High;

                int? priority = workItem.Priority();
                if (priority != null)
                {
                    fullTitle = $"P{priority} {fullTitle}";
                }
            }
            */

            message.Subject = fullTitle;
            message.HtmlBody = writer.ToString();
            message.Attachments.AddRange(attachments);


            return message;
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

        private static bool HadAnyHistoryUpdates(WorkItemWithUpdates workItemWithUpdates)
        {
            var updates = workItemWithUpdates.Updates;
            foreach (var update in updates)
            {
                WorkItemFieldUpdate historyUpdate;
                if (update.Fields != null && update.Fields.TryGetValue(WorkItemConstants.CoreFields.History, out historyUpdate) && historyUpdate.NewValue != null)
                {
                    return true;
                }
            }

            return false;
        }

        public MailMessage GenerateEmail(WorkItemQueryExpandedResult result)
        {
            Assert.ParamIsNotNull(result, "result");

            WorkItemHtmlFormatter formatter = GetHtmlFormatter();
            formatter.OutlookMode = true;

            StringWriter writer = new StringWriter();
            formatter.FormatResults(result, writer);

            MailMessage message = new MailMessage();
            message.HtmlBody = writer.ToString();

            if (result.QueryHierarchyItem != null)
            {
                string queryName = result.QueryHierarchyItem.Name;
                string subject = (result.WorkItems.Count > 0) ? String.Format("{0} ({1})", queryName, result.WorkItems.Count) : queryName;
                message.Subject = subject;
            }

            return message;
        }

        public MailMessage GenerateEmail(ICollection<WorkItem> workItems)
        {
            Assert.ParamIsNotNull(workItems, "workItems");

            WorkItemHtmlFormatter formatter = GetHtmlFormatter();
            formatter.OutlookMode = true;

            StringWriter writer = new StringWriter();
            formatter.FormatResults(workItems, writer);

            MailMessage message = new MailMessage();
            message.HtmlBody = writer.ToString();

            return message;
        }
    }
}
