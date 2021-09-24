using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Tools.TeamMate.Model
{
    public class PullRequestQueryInfo
    {
        public static readonly Dictionary<PullRequestQueryReviewStatus, PullRequestStatus> ReviewStatusesMap = new Dictionary<PullRequestQueryReviewStatus, PullRequestStatus>()
        {
            { PullRequestQueryReviewStatus.Active, PullRequestStatus.Active },
            { PullRequestQueryReviewStatus.Completed, PullRequestStatus.Completed },
            { PullRequestQueryReviewStatus.All, PullRequestStatus.All }
        };

        public string Name { get; set; }
        public string[] Authors { get; set; }
        public string[] Reviewers { get; set; }
        public PullRequestQueryReviewStatus ReviewStatus { get; set; }
    }
    public enum PullRequestQueryReviewStatus
    {
        [Description("Active")]
        Active,

        [Description("Completed")]
        Completed,

        [Description("All")]
        All
    }
}

