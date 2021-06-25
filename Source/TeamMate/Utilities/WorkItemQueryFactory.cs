using Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using System.Collections.Generic;

namespace Microsoft.Internal.Tools.TeamMate.Utilities
{
    public static class WorkItemQueryFactory
    {
        public static string CreateSearchQuery(string searchText)
        {
            string[] words = TextMatcher.SplitDistinctWords(searchText);

            WorkItemQueryBuilder builder = new WorkItemQueryBuilder();
            ConditionInfo projectCondition = FieldConditionInfo.CurrentProjectCondition;
            builder.Condition = projectCondition.And(CreateWordSearchClause(words));
            builder.AddOrderBy(WorkItemConstants.CoreFields.ChangedDate, false);
            return builder.ToString();
        }

        public static ConditionInfo CreateWordSearchClause(IEnumerable<string> words)
        {
            List<ConditionInfo> conditions = new List<ConditionInfo>();
            foreach (var word in words)
            {
                conditions.Add(new OrConditionInfo(
                    new FieldConditionInfo(WorkItemConstants.CoreFields.Title, Operators.Contains, word),
                    new FieldConditionInfo(WorkItemConstants.CoreFields.Description, Operators.Contains, word)

                    // TODO: Check if this field exists there of course
                    // new FieldConditionInfo(WorkItemConstants.Fields.ReproSteps, Operators.ContainsWords, word)

                    // TODO: History REALLY KILLS THE PERF ON THIS. TFS doesn't do this.
                    // new FieldConditionInfo("System.History", Operators.ContainsWords, word)
                ));
            }

            return new AndConditionInfo(conditions.ToArray());
        }
    }
}
