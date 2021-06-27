using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.IO;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Office.Outlook;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Services
{
    public class CollaborationService : IDisposable
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
        public TempFileService TempFileService { get; set; }

        [Import]
        public MessageBoxService MessageBoxService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        [Import]
        public SettingsService SettingsService { get; set; }

        private TempDirectory tempDirectory;

        private TempDirectory TempDirectory
        {
            get
            {
                if (this.tempDirectory == null)
                {
                    // TODO: How do we determine when this should and should not be called
                    // TODO: Have temp file service give us a unique name for Mail?
                    this.tempDirectory = this.TempFileService.CreateTempSubDirectory();
                }

                return tempDirectory;
            }
        }

        public void SendMail(MailMessage message)
        {
            Assert.ParamIsNotNull(message, "message");

            try
            {
                OutlookService.DisplayMail(message);
            }
            catch (OutlookException e)
            {
                this.MessageBoxService.ShowError(e);
            }
        }

        public async Task SendMailAsync(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            MailMessage message = await CreateMessageAsync(workItem, false, false);
            SendMail(message);
        }

        public async Task ReplyWithMailAsync(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            MailMessage message = await CreateMessageAsync(workItem, true, false);
            SendMail(message);
        }

        public async Task ReplyAllWithMailAsync(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            MailMessage message = await CreateMessageAsync(workItem, false, true);
            SendMail(message);
        }

        public void SendMail(WorkItemQueryExpandedResult workItems)
        {
            MailMessage message = CreateMessage(workItems.WorkItems, false);
            SendMail(message);
        }

        public void ReplyAllWithMail(ICollection<WorkItem> workItems)
        {
            MailMessage message = CreateMessage(workItems, true);
            SendMail(message);
        }

        public void ReplyAllWithMail(WorkItemQueryExpandedResult workItems)
        {
            MailMessage message = CreateMessage(workItems, true);
            SendMail(message);
        }

        public void Dispose()
        {
            if (this.tempDirectory != null)
            {
                this.tempDirectory.Dispose();
                this.tempDirectory = null;
            }
        }

        private async Task<MailMessage> CreateMessageAsync(WorkItem workItem, bool replyTo, bool replyToAll)
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

            WorkItemWithUpdates workItemWithUpdates = new WorkItemWithUpdates(workItem, updates);

            WorkItemMailGenerator generator = GetEmailGenerator();
            MailMessage message = generator.GenerateEmail(workItemWithUpdates);

            if (replyToAll)
            {
                var recipients = ResolveEveryoneInvolvedExceptMyself(workItem);
                foreach (var recipient in recipients)
                {
                    message.To.Add(recipient);
                }
            }
            else if (replyTo)
            {
                foreach (var fieldName in FieldsReferringToPeopleInPriorityOrder)
                {
                    string name;
                    WorkItemIdentity identity;
                    if (workItem.TryGetField(fieldName, out name) && (identity = WorkItemIdentity.TryParse(name)) != null)
                    {
                        message.To.Add(GetEmailOrDisplayName(identity));
                        break;
                    }
                }
            }

            return message;
        }

        private WorkItemMailGenerator GetEmailGenerator()
        {
            var projectContext = this.SessionService.Session.ProjectContext;

            WorkItemMailGenerator generator = new WorkItemMailGenerator(projectContext.WorkItemFieldsByName, projectContext.HyperlinkFactory);
            generator.TempDirectory = TempDirectory.Path;
            return generator;
        }

        private MailMessage CreateMessage(WorkItemQueryExpandedResult result, bool replyToAll)
        {
            var workItems = result.WorkItems;

            WorkItemMailGenerator generator = GetEmailGenerator();
            MailMessage message = generator.GenerateEmail(result);

            if (replyToAll)
            {
                if (workItems.Any())
                {
                    var recipients = ResolvedAssignedToExceptMyself(workItems);
                    foreach (var recipient in recipients)
                    {
                        message.To.Add(recipient);
                    }
                }
            }

            return message;
        }


        private MailMessage CreateMessage(ICollection<WorkItem> workItems, bool replyToAll)
        {
            WorkItemMailGenerator generator = GetEmailGenerator();
            MailMessage message = generator.GenerateEmail(workItems);

            if (replyToAll)
            {
                if (workItems.Any())
                {
                    var recipients = ResolvedAssignedToExceptMyself(workItems);
                    foreach (var recipient in recipients)
                    {
                        message.To.Add(recipient);
                    }
                }
            }

            return message;
        }

        private ICollection<string> ResolvedAssignedToExceptMyself(ICollection<WorkItem> workItems)
        {
            Assert.ParamIsNotNull(workItems, "workItems");

            var names = workItems.Select(wi => wi.AssignedTo());
            return RecipientsButMe(names);
        }

        private ICollection<string> ResolveEveryoneInvolvedExceptMyself(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            var names = FieldsReferringToPeopleInPriorityOrder.Select(fieldName => workItem.GetField(fieldName));
            return RecipientsButMe(names);
        }

        private ICollection<string> RecipientsButMe(IEnumerable<string> names)
        {
            var me = this.SessionService.Session.ProjectContext?.WorkItemIdentity;
            var people = names.Where(v => !String.IsNullOrEmpty(v)).Distinct().ToList();
            var identities = people.Select(a => WorkItemIdentity.TryParse(a)).Where(i => i != null && !Object.Equals(i, me)).ToList();
            var recipients = identities.Select(i => GetEmailOrDisplayName(i)).ToList();
            return recipients;
        }

        private static string GetEmailOrDisplayName(WorkItemIdentity identity)
        {
            return identity.EmailAddress != null ? identity.EmailAddress : identity.DisplayName;
        }
    }
}
