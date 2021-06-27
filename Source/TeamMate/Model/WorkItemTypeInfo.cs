using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Tools.TeamMate.Model
{
    public class WorkItemTypeInfo
    {
        public WorkItemTypeInfo(WorkItemTypeReference reference)
        {
            Assert.ParamIsNotNull(reference, "reference");

            this.Reference = reference;
        }

        public string Name
        {
            get { return this.Reference.Name; }
        }

        public string Description { get; set; }

        public WorkItemTypeReference Reference { get; set; }

        public WorkItemTypeCategory Category { get; set; }

        public static WorkItemTypeCategory? GetPreferredCategory(ICollection<string> categoryNames)
        {
            if (categoryNames.Any())
            {
                foreach (var item in categoryMap)
                {
                    if (categoryNames.Contains(item.Item1))
                    {
                        return item.Item2;
                    }
                }
            }

            return null;
        }

        private static readonly ICollection<Tuple<string, WorkItemTypeCategory>> categoryMap = new List<Tuple<string, WorkItemTypeCategory>>()
        {
            new Tuple<string, WorkItemTypeCategory>(WorkItemConstants.Categories.Bug, WorkItemTypeCategory.Bug),
            new Tuple<string, WorkItemTypeCategory>(WorkItemConstants.Categories.Requirement, WorkItemTypeCategory.Requirement),
            new Tuple<string, WorkItemTypeCategory>(WorkItemConstants.Categories.Task, WorkItemTypeCategory.Task),
            new Tuple<string, WorkItemTypeCategory>(WorkItemConstants.Categories.TestCase, WorkItemTypeCategory.TestCase),
        };

    }

    public enum WorkItemTypeCategory
    {
        None,
        Bug,
        Requirement,
        Task,
        TestCase
    }
}
