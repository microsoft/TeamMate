using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Utilities
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class SearchExpression
    {
        private SearchExpression()
        {
            this.Tokens = new string[0];
        }

        public static SearchExpression Parse(string text)
        {
            text = TextMatcher.NormalizeSearchText(text);

            SearchExpression expression = new SearchExpression();
            expression.Text = text;

            if (!String.IsNullOrWhiteSpace(text))
            {
                expression.Tokens = text.Split(' ');
            }

            return expression;
        }

        public string[] Tokens { get; private set; }
        public string Text { get; private set; }

        public bool IsEmpty { get { return !Tokens.Any(); } }

        public bool Matches(object item)
        {
            bool matches = false;

            WorkItemRowViewModel workItem = item as WorkItemRowViewModel;
            if (workItem != null)
            {
                matches = Matches(workItem);
            }
            else
            {
                PullRequestRowViewModel review = item as PullRequestRowViewModel;
                if (review != null)
                {
                    matches = Matches(review);
                }
            }

            return matches;
        }

        private Predicate<WorkItemRowViewModel> workItemPredicate;
        private Predicate<PullRequestRowViewModel> codeReviewPredicate;

        public bool Matches(PullRequestRowViewModel item)
        {
            if (this.codeReviewPredicate == null)
            {
                this.codeReviewPredicate = BuildCodeReviewPredicate();
            }

            return this.codeReviewPredicate(item);
        }

        private Predicate<PullRequestRowViewModel> BuildCodeReviewPredicate()
        {
            List<Predicate<PullRequestRowViewModel>> predicates = new List<Predicate<PullRequestRowViewModel>>();

            if (this.Tokens.Any())
            {
                var predicate = TextMatcher.MatchAllWordStartsMultiText(this.Tokens);
                var matcher = new MultiWordMatcher(predicate);
                predicates.Add((wi) => wi.Matches(matcher));
            }

            var p = PredicateUtilities.And(predicates);
            return p;
        }

        public bool Matches(WorkItemRowViewModel item)
        {
            if (this.workItemPredicate == null)
            {
                this.workItemPredicate = BuildWorkItemPredicate();
            }

            return this.workItemPredicate(item);
        }

        private Predicate<WorkItemRowViewModel> BuildWorkItemPredicate()
        {
            // IMPORTANT: Keep in sync with ToVstsWiql
            List<Predicate<WorkItemRowViewModel>> predicates = new List<Predicate<WorkItemRowViewModel>>();

            if (this.Tokens.Any())
            {
                var predicate = TextMatcher.MatchAllWordStartsMultiText(this.Tokens);
                var matcher = new MultiWordMatcher(predicate);
                predicates.Add((wi) => wi.Matches(matcher));
            }

            return PredicateUtilities.And(predicates);
        }

        private static bool IsKey(string key, string keyName)
        {
            return String.Equals(key, keyName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool Matches(string value, string text)
        {
            return value != null && value.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var token in Tokens)
            {
                if(sb.Length > 0)
                {
                    sb.Append(" ");
                }
                sb.Append(token);
            }

            return sb.ToString();
        }

        public string ToVstsWiql()
        {
            // IMPORTANT: Keep in sync with BuildWorkItemPredicate
            WorkItemQueryBuilder builder = new WorkItemQueryBuilder();
            builder.Condition = FieldConditionInfo.CurrentProjectCondition;

            if (this.Tokens.Any())
            {
                builder.Condition = builder.Condition.And(WorkItemQueryFactory.CreateWordSearchClause(this.Tokens));
            }

            builder.AddOrderBy(WorkItemConstants.CoreFields.ChangedDate, false);
            return builder.ToString();
        }
    }
}
