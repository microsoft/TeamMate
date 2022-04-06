using Microsoft.TeamFoundation.SourceControl.WebApi;
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
        public PullRequestQueryReviewStatus ReviewStatus { get; set; }
        public string CreatedBy { get; set; }

        public string AssignedTo { get; set; }

        public string SourceRefMatchExpression { get; set; }
    }
    public enum PullRequestQueryReviewStatus
    {
        [Description("Active")]
        Active,

        [Description("Completed")]
        Completed,

        [Description("All")]
        All,
    }
}

