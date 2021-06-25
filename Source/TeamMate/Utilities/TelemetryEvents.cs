
namespace Microsoft.Internal.Tools.TeamMate.Utilities
{
    public static class TelemetryEvents
    {
        // Work Items
        public const string WorkItemOpened = "WorkItemOpened";
        public const string WorkItemCreated = "WorkItemCreated";

        // Search and Filtering
        public const string WorkItemOpenedUsingSearch = "WorkItemOpenedUsingSearch";
        public const string Search = "Search";
        public static string FilterApplied = "FilterApplied";
        
        // Actions
        public const string FlaggedItem = "FlaggedItem";

        public const string WorkItemSendMail = "WorkItemSendMail";
        public const string WorkItemReplyWithMail = "WorkItemReplyWithMail";
        public const string WorkItemSearchInOutlook = "WorkItemSearchInOutlook";
        public const string WorkItemCollectionReplyAllWithMail = "WorkItemCollectionReplyAllWithMail";
        public const string WorkItemCollectionSendMail = "WorkItemCollectionSendMail";
        public const string WorkItemReplyAllWithMail = "WorkItemReplyAllWithMail";

        // CodeFlow
        public const string CodeFlowTileAdded = "CodeFlowTileAdded";
        public const string CodeFlowReviewOpened = "CodeFlowReviewOpened";
        public const string CodeFlowSendReminderEmail = "CodeFlowSendReminderEmail";

        // Shortcuts
        public const string HotKeyUsed = "HotKeyUsed";

        // Feedback
        public const string FeedbackReported = "FeedbackReported";

        // Updates
        public const string BackgroundCheckFoundUpdate = "BackgroundCheckFoundUpdate";

        public static class Properties
        {
            public const string AutoQuickSearch = "AutoQuickSearch";
            public const string QuickSearch = "QuickSearch";

            public const string InVisualStudio = "InVisualStudio";
            public const string InWebBrowser = "InWebBrowser";

            public const string HotKey = "HotKey";
        }
    }
}
