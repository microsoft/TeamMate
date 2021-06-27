// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Platform.CodeFlow.Dashboard;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Tools.TeamMate.Model
{
    public class CodeFlowQueryInfo
    {
        private static readonly Dictionary<CodeFlowQueryReviewPeriod, TimeSpan> ReviewPeriodsMap = new Dictionary<CodeFlowQueryReviewPeriod, TimeSpan>()
        {
            { CodeFlowQueryReviewPeriod.LastDay, TimeSpan.FromDays(1) },
            { CodeFlowQueryReviewPeriod.LastWeek, TimeSpan.FromDays(7) },
            { CodeFlowQueryReviewPeriod.LastTwoWeeks, TimeSpan.FromDays(14) },
            { CodeFlowQueryReviewPeriod.LastMonth, TimeSpan.FromDays(30) },
            { CodeFlowQueryReviewPeriod.LastTwoMonths, TimeSpan.FromDays(2 * 30) },
            { CodeFlowQueryReviewPeriod.LastSixMonths, TimeSpan.FromDays(6 * 30) },
            { CodeFlowQueryReviewPeriod.LastYear, TimeSpan.FromDays(365) },
        };

        private static readonly Dictionary<CodeFlowQueryReviewStatuses, CodeReviewStatus[]> ReviewStatusesMap = new Dictionary<CodeFlowQueryReviewStatuses, CodeReviewStatus[]>()
        {
            { CodeFlowQueryReviewStatuses.Active, new CodeReviewStatus[] { CodeReviewStatus.Active, CodeReviewStatus.Created } },
            { CodeFlowQueryReviewStatuses.Completed, new CodeReviewStatus[] { CodeReviewStatus.Completed } },
            { CodeFlowQueryReviewStatuses.ActiveOrCompleted, new CodeReviewStatus[] { CodeReviewStatus.Active, CodeReviewStatus.Created, CodeReviewStatus.Completed } },
            { CodeFlowQueryReviewStatuses.All, null },
        };

        public string Name { get; set; }
        public string[] Authors { get; set; }
        public string[] Reviewers { get; set; }
        public string[] Projects { get; set; }
        public CodeFlowQueryReviewPeriod ReviewPeriod { get; set; }
        public CodeFlowQueryReviewStatuses ReviewStatuses { get; set; }

        public CodeReviewQuery CreateCodeReviewQuery()
        {
            CodeReviewQuery query = new CodeReviewQuery();

            query.Authors = this.Authors;
            query.Projects = this.Projects;
            query.Reviewers = this.Reviewers;

            query.CreatedAfterDate = (DateTime.Now - ReviewPeriodsMap[this.ReviewPeriod]).ToUniversalTime();
            query.ReviewStatuses = ReviewStatusesMap[this.ReviewStatuses];

            return query;
        }
    }

    public enum CodeFlowQueryReviewPeriod
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

    public enum CodeFlowQueryReviewStatuses
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

