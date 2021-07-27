using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public class SearchExpression
    {
        private static readonly Regex SearchRegex = new Regex(@"(?<key>\w+):""(?<value>[^\""]*)""|(?<key>\w+):(?<value>\w*)|(?<value>\w+)", RegexOptions.Compiled);
        private const string KeyGroup = "key";
        private const string ValueGroup = "value";

        private SearchExpression()
        {
            this.Tokens = new List<SearchExpressionToken>();
        }

        public static SearchExpression Parse(string text)
        {
            text = TextMatcher.NormalizeSearchText(text);

            SearchExpression expression = new SearchExpression();
            expression.Text = text;

            if (!String.IsNullOrWhiteSpace(text))
            {
                var matches = SearchRegex.Matches(text);
                foreach (Match item in matches)
                {
                    var valueGroup = item.Groups[ValueGroup];

                    if (valueGroup != null)
                    {
                        string value = valueGroup.Value;
                        if (!String.IsNullOrWhiteSpace(value))
                        {
                            var keyGroup = item.Groups[KeyGroup];
                            string key = (keyGroup != null) ? keyGroup.Value : null;

                            var token = (!String.IsNullOrWhiteSpace(key)) ? new SearchExpressionToken(key, value) : new SearchExpressionToken(value);
                            expression.Tokens.Add(token);
                        }
                    }
                }
            }

            return expression;
        }

        public IList<SearchExpressionToken> Tokens { get; private set; }
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
                PullRequestViewModel review = item as PullRequestViewModel;
                if (review != null)
                {
                    matches = Matches(review);
                }
            }

            return matches;
        }

        private Predicate<WorkItemRowViewModel> workItemPredicate;
        private Predicate<PullRequestViewModel> codeReviewPredicate;

        public bool Matches(PullRequestViewModel item)
        {
            if (this.codeReviewPredicate == null)
            {
                this.codeReviewPredicate = BuildCodeReviewPredicate();
            }

            return this.codeReviewPredicate(item);
        }

        private Predicate<PullRequestViewModel> BuildCodeReviewPredicate()
        {
            List<string> plainWords = new List<string>();
            List<Predicate<PullRequestViewModel>> predicates = new List<Predicate<PullRequestViewModel>>();
            foreach (var token in Tokens)
            {
                string value = token.Value;
                if (IsKey(token.Key, "c"))
                {
                    predicates.Add((r) => Matches(r.Reference.CreatedBy.DisplayName, value));
                }
                else
                {
                    // If we don't recognize the key token, treat the value as a plain word for matching
                    plainWords.Add(value);
                }
            }

            if (plainWords.Any())
            {
                var predicate = TextMatcher.MatchAllWordStartsMultiText(plainWords);
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
            List<string> plainWords = new List<string>();
            List<Predicate<WorkItemRowViewModel>> predicates = new List<Predicate<WorkItemRowViewModel>>();
            foreach (var token in Tokens)
            {
                string value = token.Value;
                if (IsKey(token.Key, "a"))
                {
                    predicates.Add((wi) => Matches(wi.AssignedTo, value));
                }
                else if (IsKey(token.Key, "s"))
                {
                    predicates.Add((wi) => Matches(wi.State, value));
                }
                else if (IsKey(token.Key, "t"))
                {
                    predicates.Add((wi) => Matches(wi.Type, value));
                }
                else if (IsKey(token.Key, "c"))
                {
                    predicates.Add((wi) => Matches(wi.CreatedBy, value));
                }
                else
                {
                    // If we don't recognize the key token, treat the value as a plain word for matching
                    plainWords.Add(value);
                }
            }

            if (plainWords.Any())
            {
                var predicate = TextMatcher.MatchAllWordStartsMultiText(plainWords);
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

            List<string> plainWords = new List<string>();
            List<Predicate<WorkItemRowViewModel>> predicates = new List<Predicate<WorkItemRowViewModel>>();
            foreach (var token in Tokens)
            {
                string value = token.Value;
                if (IsKey(token.Key, "a"))
                {
                    builder.Condition = builder.Condition.And(new FieldConditionInfo(WorkItemConstants.CoreFields.AssignedTo, Operators.Contains, value));
                }
                else if (IsKey(token.Key, "s"))
                {
                    builder.Condition = builder.Condition.And(new FieldConditionInfo(WorkItemConstants.CoreFields.State, Operators.Contains, value));
                }
                else if (IsKey(token.Key, "t"))
                {
                    builder.Condition = builder.Condition.And(new FieldConditionInfo(WorkItemConstants.CoreFields.WorkItemType, Operators.Contains, value));
                }
                else if (IsKey(token.Key, "c"))
                {
                    builder.Condition = builder.Condition.And(new FieldConditionInfo(WorkItemConstants.CoreFields.CreatedBy, Operators.Contains, value));
                }
                else
                {
                    // If we don't recognize the key token, treat the value as a plain word for matching
                    plainWords.Add(value);
                }
            }

            if (plainWords.Any())
            {
                builder.Condition = builder.Condition.And(WorkItemQueryFactory.CreateWordSearchClause(plainWords));
            }

            builder.AddOrderBy(WorkItemConstants.CoreFields.ChangedDate, false);
            return builder.ToString();
        }
    }

    public class SearchExpressionToken
    {
        public SearchExpressionToken(string value)
        {
            this.Value = value;
        }

        public SearchExpressionToken(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
        public bool HasKey { get { return Key != null; } }

        public override string ToString()
        {
            string escapedValue = (Value.Contains(' ')) ? '\"' + Value + '\"' : Value;
            return (Key != null) ? String.Format("{0}:{1}", Key, escapedValue) : escapedValue;
        }
    }
}
