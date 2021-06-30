using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using System.Windows.Input;

// TODO: Consider moving this to another namespace, seems annoying here...
namespace Microsoft.Tools.TeamMate.Resources
{
    public static class TeamMateCommands
    {
        private static readonly CommandDictionary commands = new CommandDictionary();

        // Application Commands

        // TODO: Make these 3 names right... They are inconsistent right now in where they are used
        public static ICommand QuickCreate { get { return commands.FindResource(); } }
        public static ICommand QuickCreateWithOptions { get { return commands.FindResource(); } }
        public static ICommand QuickSearch { get { return commands.FindResource(); } }
        public static ICommand Exit { get { return commands.FindResource(); } }
        public static ICommand About { get { return commands.FindResource(); } }
        public static ICommand Help { get { return commands.FindResource(); } }

        // Auto Update command
        public static ICommand Restart { get { return commands.FindResource(); } }

        // Ribbon Comands
        public static ICommand Save { get { return commands.FindResource(); } }
        public static ICommand SaveAndClose { get { return commands.FindResource(); } }
        public static ICommand Refresh { get { return commands.FindResource(); } }
        public static ICommand Revert { get { return commands.FindResource(); } }
        public static ICommand CopyWorkItem { get { return commands.FindResource(); } }
        public static ICommand CopyHyperlink { get { return commands.FindResource(); } }
        public static ICommand CopyId { get { return commands.FindResource(); } }
        public static ICommand CopyTitle { get { return commands.FindResource(); } }
        public static ICommand CopyDetails { get { return commands.FindResource(); } }
        public static ICommand AttachFile { get { return commands.FindResource(); } }
        public static ICommand AttachClipboardContents { get { return commands.FindResource(); } }
        public static ICommand AttachScreenshot { get { return commands.FindResource(); } }
        public static ICommand AttachScreenRecording { get { return commands.FindResource(); } }
        public static ICommand AddLink { get { return commands.FindResource(); } }
        public static ICommand NewLinkedWorkItem { get { return commands.FindResource(); } }
        public static ICommand SendEmail { get { return commands.FindResource(); } }
        public static ICommand ReplyWithEmail { get { return commands.FindResource(); } }
        public static ICommand ReplyAllWithEmail { get { return commands.FindResource(); } }
        public static ICommand ReplyWithIM { get { return commands.FindResource(); } }
        public static ICommand ReplyAllWithIM { get { return commands.FindResource(); } }
        public static ICommand CreateMeeting { get { return commands.FindResource(); } }
        public static ICommand Flag { get { return commands.FindResource(); } }
        public static ICommand CaptureTemplate { get { return commands.FindResource(); } }
        public static ICommand ModifyTemplateContextMenu { get { return commands.FindResource(); } }
        public static ICommand RemoveTemplateContextMenu { get { return commands.FindResource(); } }

        public static ICommand CaptureDesktop { get { return commands.FindResource(); } }
        public static ICommand CaptureMainScreen { get { return commands.FindResource(); } }
        public static ICommand CaptureSecondaryScreen { get { return commands.FindResource(); } }
        public static ICommand CaptureAllScreens { get { return commands.FindResource(); } }

        public static ICommand PreviousItem { get { return commands.FindResource(); } }
        public static ICommand NextItem { get { return commands.FindResource(); } }
        public static ICommand SaveAll { get { return commands.FindResource(); } }
        public static ICommand OpenQueryInWebAccess { get { return commands.FindResource(); } }
        public static ICommand SendEmailForQuery { get { return commands.FindResource(); } }
        public static ICommand ReplyAllInQueryWithEmail { get { return commands.FindResource(); } }

        public static ICommand ConnectToProject { get { return commands.FindResource(); } }

        public static ICommand OpenAttachment { get { return commands.FindResource(); } }
        public static ICommand SaveAttachmentAs { get { return commands.FindResource(); } }
        public static ICommand SaveAllAttachments { get { return commands.FindResource(); } }
        public static ICommand RemoveAttachment { get { return commands.FindResource(); } }
        public static ICommand CopyAttachment { get { return commands.FindResource(); } }
        public static ICommand SelectAllAttachments { get { return commands.FindResource(); } }

        public static ICommand RemoveLink { get { return commands.FindResource(); } }

        public static ICommand ChooseProjects { get { return commands.FindResource(); } }
        public static ICommand AddWorkItemQueryTile { get { return commands.FindResource(); } }
        public static ICommand AddCodeFlowTile { get { return commands.FindResource(); } }

        public static ICommand Hamburger { get { return commands.FindResource(); } }

        public static ICommand BrowseBack { get { return commands.FindResource(); } }
        public static ICommand NavigateToHomePage { get { return commands.FindResource(); } }
        public static ICommand NavigateToNewWorkItemPage { get { return commands.FindResource(); } }
        public static ICommand NavigateToProjectsPage { get { return commands.FindResource(); } }
        public static ICommand NavigateToSettingsPage { get { return commands.FindResource(); } }
        public static ICommand NavigateToDeveloperOptionsPage { get { return commands.FindResource(); } }

        public static ICommand MarkAsRead { get { return commands.FindResource(); } }
        public static ICommand MarkAsUnread { get { return commands.FindResource(); } }
        public static ICommand MarkAllAsRead { get { return commands.FindResource(); } }

        public static ICommand EditTags { get { return commands.FindResource(); } }

        public static ICommand NewCodeFlowReview { get { return commands.FindResource(); } }
        public static ICommand OpenReviewInCodeFlow { get { return commands.FindResource(); } }
        public static ICommand OpenReviewInVisualStudio { get { return commands.FindResource(); } }
        public static ICommand OpenReviewInWeb { get { return commands.FindResource(); } }
        public static ICommand SendReviewReminderMail { get { return commands.FindResource(); } }
        public static ICommand SendAllReviewsReminderMail { get { return commands.FindResource(); } }

        public static ICommand PingReviewers { get { return commands.FindResource(); } }
        public static ICommand CompleteReviews { get { return commands.FindResource(); } }


        public static ICommand StopRecordingTray { get { return commands.FindResource(); } }

        public static ICommand CancelRecordingTray { get { return commands.FindResource(); } }


        // Non resource commands

        public static ICommand RemoveTile { get { return commands.Create(); } }
        public static ICommand ModifyTile { get { return commands.Create(); } }
        public static ICommand SelectTileBackgroundColor { get { return commands.Create(); } }
        public static ICommand ResetTileBackgroundColor { get { return commands.Create(); } }
        public static ICommand RetryConnectToVsts { get { return commands.Create(); } }
    }
}