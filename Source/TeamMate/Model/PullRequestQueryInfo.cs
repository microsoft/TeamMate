using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Tools.TeamMate.Model
{
    public class PullRequestQueryInfo
    {
        private static readonly Dictionary<PullRequestQueryReviewPeriod, TimeSpan> ReviewPeriodsMap = new Dictionary<PullRequestQueryReviewPeriod, TimeSpan>()
        {
            { PullRequestQueryReviewPeriod.LastDay, TimeSpan.FromDays(1) },
            { PullRequestQueryReviewPeriod.LastWeek, TimeSpan.FromDays(7) },
            { PullRequestQueryReviewPeriod.LastTwoWeeks, TimeSpan.FromDays(14) },
            { PullRequestQueryReviewPeriod.LastMonth, TimeSpan.FromDays(30) },
            { PullRequestQueryReviewPeriod.LastTwoMonths, TimeSpan.FromDays(2 * 30) },
            { PullRequestQueryReviewPeriod.LastSixMonths, TimeSpan.FromDays(6 * 30) },
            { PullRequestQueryReviewPeriod.LastYear, TimeSpan.FromDays(365) },
        };

        private static readonly Dictionary<PullRequestQueryReviewStatuses, PullRequestStatus[]> ReviewStatusesMap = new Dictionary<PullRequestQueryReviewStatuses, PullRequestStatus[]>()
        {
            { PullRequestQueryReviewStatuses.Active, new PullRequestStatus[] { PullRequestStatus.Active } },
            { PullRequestQueryReviewStatuses.Completed, new PullRequestStatus[] { PullRequestStatus.Completed } },
            { PullRequestQueryReviewStatuses.ActiveOrCompleted, new PullRequestStatus[] { PullRequestStatus.Active, PullRequestStatus.Completed } },
            { PullRequestQueryReviewStatuses.All, null }
        };

        public string Name { get; set; }
        public string[] Authors { get; set; }
        public string[] Reviewers { get; set; }
        public string[] Projects { get; set; }
        public PullRequestQueryReviewPeriod ReviewPeriod { get; set; }
        public PullRequestQueryReviewStatuses ReviewStatuses { get; set; }
    }

    public enum PullRequestQueryReviewPeriod
    {
        [Description("Day")]
        LastDay,

        [Description("Week")]
        LastWeek,

        [Description("Two Weeks")]
        LastTwoWeeks,

        [Description("Month")]
        LastMonth,

        [Description("Two Months")]
        LastTwoMonths,

        [Description("Six Months")]
        LastSixMonths,

        [Description("Year")]
        LastYear
    }

    public enum PullRequestQueryReviewStatuses
    {
        [Description("Active")]
        Active,

        [Description("Completed")]
        Completed,

        [Description("Active or Completed")]
        ActiveOrCompleted,

        [Description("All")]
        All
    }
}

