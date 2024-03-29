﻿using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Services
{
    public class CollaborationService
    {
        public static readonly string[] FieldsReferringToPeopleInPriorityOrder = {
            WorkItemConstants.CoreFields.AssignedTo,
            WorkItemConstants.VstsFields.ClosedBy,
            WorkItemConstants.VstsFields.ResolvedBy,
            WorkItemConstants.VstsFields.ActivatedBy,
            WorkItemConstants.CoreFields.CreatedBy,
            WorkItemConstants.CoreFields.ChangedBy,
        };

        [Import]
        public MessageBoxService MessageBoxService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        [Import]
        public SettingsService SettingsService { get; set; }

        private void CopyToClipboard(string html, string text)
        {
            Assert.ParamIsNotNull(html, "html");
            Assert.ParamIsNotNull(text, "text");

            ClipboardUtilities.CopyToClipboard(html, text);
        }

        public async Task CopyToClipboardAsync(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            var html = await CreateWorkItemHtmlAsync(workItem);
            var text = await CreateWorkItemTextAsync(workItem);
            CopyToClipboard(html, text);
        }

        public async Task CopyToClipboard(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            var html = await CreateWorkItemHtmlAsync(workItem);
            var text = await CreateWorkItemTextAsync(workItem);
            CopyToClipboard(html, text);
        }

        public void CopyToClipboard(WorkItemQueryExpandedResult workItems)
        {
            var html = CreateWorkItemHtml(workItems);
            var text = CreateWorkItemText(workItems);
            CopyToClipboard(html, text);
        }

        public void CopyToClipboard(ICollection<WorkItem> workItems)
        {
            var html = CreateWorkItemHtml(workItems);
            var text = CreateWorkItemText(workItems);
            CopyToClipboard(html, text);
        }

        private async Task<string> CreateWorkItemHtmlAsync(WorkItem workItem)
        {
            // IMPORTANT: The workItem we are receiving here should generally be a summarized work item, and does not
            // contain all fields. For richer emails on single work items, we need the full work item and history
            // so we are going to get that.

            var witClient = this.SessionService.Session.ProjectContext.WorkItemTrackingClient;

            var getWorkItemTask = witClient.GetWorkItemAsync(workItem.Id.Value, expand: WorkItemExpand.Fields | WorkItemExpand.Links);
            var getUpdatesTask = witClient.GetUpdatesAsync(workItem.Id.Value);

            await Task.WhenAll(getWorkItemTask, getUpdatesTask);

            workItem = getWorkItemTask.Result;
            var updates = getUpdatesTask.Result;

            WorkItemHtmlGenerator generator = GetHtmlGenerator();

            WorkItemWithUpdates workItemWithUpdates = new WorkItemWithUpdates(workItem, updates);
            return generator.GenerateHtml(workItemWithUpdates);
        }

        private async Task<string> CreateWorkItemTextAsync(WorkItem workItem)
        {
            var witClient = this.SessionService.Session.ProjectContext.WorkItemTrackingClient;

            var getWorkItemTask = witClient.GetWorkItemAsync(workItem.Id.Value, expand: WorkItemExpand.Fields | WorkItemExpand.Links);
            var getUpdatesTask = witClient.GetUpdatesAsync(workItem.Id.Value);

            await Task.WhenAll(getWorkItemTask, getUpdatesTask);

            workItem = getWorkItemTask.Result;
            var updates = getUpdatesTask.Result;

            WorkItemTextGenerator generator = new WorkItemTextGenerator();

            WorkItemWithUpdates workItemWithUpdates = new WorkItemWithUpdates(workItem, updates);
            return generator.GenerateText(workItemWithUpdates);
        }

        private WorkItemHtmlGenerator GetHtmlGenerator()
        {
            var projectContext = this.SessionService.Session.ProjectContext;

            WorkItemHtmlGenerator generator = new WorkItemHtmlGenerator(projectContext.WorkItemFieldsByName, projectContext.HyperlinkFactory);
            return generator;
        }

        private string CreateWorkItemHtml(WorkItemQueryExpandedResult result)
        {
            WorkItemHtmlGenerator generator = GetHtmlGenerator();
            return generator.GenerateHtml(result);
        }

        private string CreateWorkItemText(WorkItemQueryExpandedResult result)
        {
            WorkItemTextGenerator generator = new WorkItemTextGenerator();
            return generator.GenerateText(result);
        }

        private string CreateWorkItemHtml(ICollection<WorkItem> workItems)
        {
            WorkItemHtmlGenerator generator = GetHtmlGenerator();
            return generator.GenerateHtml(workItems);
        }

        private string CreateWorkItemText(ICollection<WorkItem> workItems)
        {
            WorkItemTextGenerator generator = new WorkItemTextGenerator();
            return generator.GenerateText(workItems);
        }
    }
}
