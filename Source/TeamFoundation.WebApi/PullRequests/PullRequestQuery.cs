using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.Text.RegularExpressions;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi.PullRequests
{
    public class PullRequestQuery
    {
        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string QueryPath { get; set; }

        public Guid QueryId { get; set; }

        public GitPullRequestSearchCriteria GitPullRequestSearchCriteria { get; set; }

        public DateTime? AsOf { get; set; }

        public int? MaxItemsToFetch { get; set; }

        public string SourceRefMatchExpression { get; set; }

        public bool MatchesSourceRef(string sourceRef) =>
            SourceRefMatchExpression.IsNullOrEmpty()
            || Regex.IsMatch(sourceRef, SourceRefMatchExpression, RegexOptions.Compiled);

        public void Validate()
        {
            int projectInputs = (ProjectId != Guid.Empty ? 1 : 0) + (ProjectName != null ? 1 : 0);
            if (projectInputs == 0)
            {
                throw new InvalidOperationException("You must specify a ProjectId or ProjectName");
            }
            else if (projectInputs > 1)
            {
                throw new InvalidOperationException("You must specify only one of ProjectId or ProjectName");
            }

            int inputs = (QueryPath != null ? 1 : 0) + (GitPullRequestSearchCriteria != null ? 1 : 0) + (QueryId != Guid.Empty ? 1 : 0);
            if (inputs == 0)
            {
                throw new InvalidOperationException("You must specify a QueryPath, QueryId or GitPullRequestSearchCriteria");
            }
            else if (inputs > 1)
            {
                throw new InvalidOperationException("You must specify only one of QueryPath, QueryId or GitPullRequestSearchCriteria");
            }
        }
    }
}
