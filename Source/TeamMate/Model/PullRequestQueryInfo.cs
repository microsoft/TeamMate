﻿using Microsoft.TeamFoundation.SourceControl.WebApi;
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
        public PullRequestQueryReviewStatus ReviewStatus { get; set; }
        public Guid? CreatedBy { get; set; }

        public Guid? AssignedTo { get; set; }

        public string UICreatedBy { get; set; }

        public string UIAssignedTo { get; set; }

        public string Project { get; set; }

        public PullRequestQueryFilter Filter { get; set; }
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

    public enum PullRequestQueryFilter
    {
        [Description("None")]
        None,

        [Description("Needs Action")]
        NeedsAction,
    }
}

