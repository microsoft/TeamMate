﻿using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
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

        public static ICommand Refresh { get { return commands.FindResource(); } }
        public static ICommand Revert { get { return commands.FindResource(); } }
        public static ICommand CopyWorkItem { get { return commands.FindResource(); } }
        public static ICommand CopyHyperlink { get { return commands.FindResource(); } }
        public static ICommand CopyId { get { return commands.FindResource(); } }
        public static ICommand CopyTitle { get { return commands.FindResource(); } }
        public static ICommand NewLinkedWorkItem { get { return commands.FindResource(); } }
        public static ICommand CopyToClipboard { get { return commands.FindResource(); } }
        public static ICommand Flag { get { return commands.FindResource(); } }

        public static ICommand OpenQueryInWebAccess { get { return commands.FindResource(); } }
        public static ICommand ConnectToProject { get { return commands.FindResource(); } }
        public static ICommand ChooseProjects { get { return commands.FindResource(); } }
        public static ICommand AddWorkItemQueryTile { get { return commands.FindResource(); } }
        public static ICommand AddPullRequestTile { get { return commands.FindResource(); } }

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

        public static ICommand OpenPullRequestInWeb { get { return commands.FindResource(); } }

        // Non resource commands

        public static ICommand RemoveTile { get { return commands.Create(); } }
        public static ICommand ModifyTile { get { return commands.Create(); } }
        public static ICommand SelectTileBackgroundColor { get { return commands.Create(); } }
        public static ICommand ResetTileBackgroundColor { get { return commands.Create(); } }
        public static ICommand SelectTileFontColor { get { return commands.Create(); } }
        public static ICommand ResetTileFontColor { get { return commands.Create(); } }
        public static ICommand RetryConnectToVsts { get { return commands.Create(); } }
    }
}