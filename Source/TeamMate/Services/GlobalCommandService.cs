using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.Model;
using Microsoft.Internal.Tools.TeamMate.Model.Actions;
using Microsoft.Internal.Tools.TeamMate.Office.Outlook;
using Microsoft.Internal.Tools.TeamMate.Resources;
using Microsoft.Internal.Tools.TeamMate.Utilities;
using Microsoft.Internal.Tools.TeamMate.Windows;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Services
{
    public class GlobalCommandService : ICommandProvider
    {
        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public MessageBoxService MessageBoxService { get; set; }

        [Import]
        public CollaborationService CollaborationService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        [Import]
        public WindowService WindowService { get; set; }

        [Import]
        public ExternalWebBrowserService ExternalWebBrowserService { get; set; }

        public void RegisterBindings(CommandBindingCollection bindings)
        {
            bindings.Add(TeamMateCommands.VoteOnFeatures, () => this.ExternalWebBrowserService.VoteOnFeatures());
            bindings.Add(TeamMateCommands.SuggestFeature, () => this.ExternalWebBrowserService.SuggestFeature());
            bindings.Add(TeamMateCommands.ReportBug, () => this.ExternalWebBrowserService.ReportBug());

            bindings.Add(TeamMateCommands.ContactUs, ContactUs);
            bindings.Add(TeamMateCommands.YammerUs, () => this.ExternalWebBrowserService.GoToYammer());
            bindings.Add(TeamMateCommands.BecomeAContributor, BecomeAContributor);
        }


        public void QuickCreateDefault()
        {
            QuickCreateDefault(null);
        }

        public void QuickCreateDefault(WorkItemUpdateInfo updateInfo)
        {
            var defaultWorkItemInfo = EnsureDefaultWorkItemInfoIsConfigured();
            if (defaultWorkItemInfo != null)
            {
                // TODO: We can no longer update work items with update information, so there's data loss
                this.WindowService.ShowNewWorkItemWindow(defaultWorkItemInfo);
            }
        }

        public void QuickCreate()
        {
            if (EnsureProjectsHaveBeenConfigured())
            {
                this.WindowService.ShowNewWorkItemWindow();
            }
        }

        public void QuickSearch()
        {
            if (EnsureProjectsHaveBeenConfigured())
            {
                this.WindowService.ShowQuickSearchWindow();
            }
        }

        private int lastAutoQuickSearchId;

        public void AutoQuickSearch()
        {
            if (this.SettingsService.Settings.SearchIdsAutomatically)
            {
                int workItemId;
                string workItemIdText = QuickSearchWindow.TryGetWorkItemIdFromClipboard();
                if (workItemIdText != null
                    && WorkItemReference.TryParseId(workItemIdText, out workItemId)
                    && workItemId != lastAutoQuickSearchId)
                {
                    var projectContext = this.SessionService.Session.ProjectContext;
                    if (projectContext != null)
                    {
                        // If automatic search was on, and the clipboard had a work item id,
                        // and the work item id is different from the last quick searched one,
                        // and we are currently in a project context, then trigger quick search

                        lastAutoQuickSearchId = workItemId;

                        WorkItemReference reference = new WorkItemReference(projectContext.ProjectInfo.ProjectCollectionUri, workItemId);
                        this.WindowService.ShowWorkItemWindow(reference);

                        Telemetry.Event(TelemetryEvents.WorkItemOpenedUsingSearch, new TelemetryEventProperties() {
                            { TelemetryEvents.Properties.AutoQuickSearch, true }
                        });

                        return;
                    }
                }
            }

            // If I made it here, it means we have to trigger the default quick search dialog
            QuickSearch();
        }

        public void ShowHomePage()
        {
            this.WindowService.ShowHomePage();
        }

        public void ShowSettingsPage()
        {
            this.WindowService.ShowSettingsPage();
        }

        public void ContactUs()
        {
            string subject = String.Format("What I have to say about {0}", TeamMateApplicationInfo.ApplicationName);
            string body = String.Format("(I'm using version {0})", TeamMateApplicationInfo.Version);

            MailMessage message = new MailMessage();
            message.To.Add(TeamMateApplicationInfo.FeedbackEmail);
            message.Subject = subject;
            message.HtmlBody = MailMessage.WrapHtmlInDefaultFont(body);
            this.CollaborationService.SendMail(message);
        }

        public void BecomeAContributor()
        {
            MailMessage message = new MailMessage();
            message.To.Add(TeamMateApplicationInfo.FeedbackEmail);
            message.Subject = "I'd like to contribute";
            this.CollaborationService.SendMail(message);
        }

        public bool EnsureProjectsHaveBeenConfigured()
        {
            var settings = this.SettingsService.Settings;

            if (!settings.Projects.Any())
            {
                var result = this.MessageBoxService.Show("Welcome! Before you can use TeamMate, you need to configure one or more projects. Would you like to do that now?", "Welcome", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    this.WindowService.ShowMainWindow();
                    this.WindowService.MainWindowViewModel.ConnectToProject();
                }

                return false;
            }

            return true;
        }

        private DefaultWorkItemInfo EnsureDefaultWorkItemInfoIsConfigured()
        {
            var settings = this.SettingsService.Settings;

            var defaultWorkItemInfo = settings.DefaultWorkItemInfo;
            if (defaultWorkItemInfo == null)
            {
                if (EnsureProjectsHaveBeenConfigured())
                {
                    this.MessageBoxService.Show("You have not configured a default work item yet. You will want to right-click a work item in the next screen and choose it as your default.", "Pick a default work item type", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.WindowService.ShowNewWorkItemWindow();
                }
            }

            return defaultWorkItemInfo;
        }
    }
}
