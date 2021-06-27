// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using System;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    public static class WorkItemExtensions
    {
        private static readonly string[] TagSeparatorArray = new string[] { WorkItemConstants.TagSeparator };
        private static readonly string[] EmptyTags = new string[0];

        public static bool HasField(this WorkItem workItem, string fieldName)
        {
            return workItem.Fields.ContainsKey(fieldName);
        }

        public static T? GetField<T>(this WorkItem workItem, string fieldName) where T : struct
        {
            T result;
            if (workItem.Fields.TryGetValue(fieldName, out result))
            {
                return result;
            }

            return null;
        }

        public static T GetField<T>(this WorkItem workItem, string fieldName, T defaultValue) where T : struct
        {
            T result;
            if (workItem.Fields.TryGetValue(fieldName, out result))
            {
                return result;
            }

            return defaultValue;
        }

        public static string GetField(this WorkItem workItem, string fieldName)
        {
            string result;
            workItem.Fields.TryGetValue(fieldName, out result);
            return result;
        }

        public static bool TryGetField<T>(this WorkItem workItem, string fieldName, out T result)
        {
            return workItem.Fields.TryGetValue(fieldName, out result);
        }

        // Updates (aka Revisions)

        public static T? GetField<T>(this WorkItemUpdate workItem, string fieldName) where T : struct
        {
            WorkItemFieldUpdate result;
            if (workItem.Fields.TryGetValue(fieldName, out result) && result.NewValue is T)
            {
                return (T)result.NewValue;
            }

            return null;
        }


        public static string GetField(this WorkItemUpdate workItem, string fieldName)
        {
            WorkItemFieldUpdate result;
            if(workItem.Fields.TryGetValue(fieldName, out result))
            {
                return result.NewValue as string;
            }

            return null;
        }

        // Core Field Shortcuts

        public static string AreaPath(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.AreaPath);
        }

        public static string AssignedTo(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.AssignedTo);
        }

        public static string ChangedBy(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.ChangedBy);
        }

        public static DateTime? ChangedDate(this WorkItem workItem)
        {
            return workItem.GetField<DateTime>(WorkItemConstants.CoreFields.ChangedDate);
        }

        public static string CreatedBy(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.CreatedBy);
        }

        public static DateTime? CreatedDate(this WorkItem workItem)
        {
            return workItem.GetField<DateTime>(WorkItemConstants.CoreFields.CreatedDate);
        }

        public static string Description(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.Description);
        }

        public static string History(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.History);
        }

        public static string IterationPath(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.IterationPath);
        }

        public static string State(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.State);
        }

        public static string Title(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.Title);
        }

        public static string WorkItemType(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.WorkItemType);
        }

        public static string TeamProject(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.CoreFields.TeamProject);
        }

        public static string[] Tags(this WorkItem workItem)
        {
            var tags = workItem.GetField(WorkItemConstants.CoreFields.Tags);
            return (!string.IsNullOrEmpty(tags)) ? tags.Split(TagSeparatorArray, StringSplitOptions.RemoveEmptyEntries) : EmptyTags;
        }

        // Vsts Field Shortcuts

        public static string ActivatedBy(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.VstsFields.ActivatedBy);
        }

        public static DateTime? ActivatedDate(this WorkItem workItem)
        {
            return workItem.GetField<DateTime>(WorkItemConstants.VstsFields.ActivatedDate);
        }

        public static string ClosedBy(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.VstsFields.ClosedBy);
        }

        public static DateTime? ClosedDate(this WorkItem workItem)
        {
            return workItem.GetField<DateTime>(WorkItemConstants.VstsFields.ClosedDate);
        }

        public static string Issue(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.VstsFields.Issue);
        }

        public static int? Priority(this WorkItem workItem)
        {
            var priority = workItem.GetField<long>(WorkItemConstants.VstsFields.Priority);
            return priority != null ? (int)priority.Value : (int?)null;
        }

        public static string ResolvedBy(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.VstsFields.ResolvedBy);
        }

        public static DateTime? ResolvedDate(this WorkItem workItem)
        {
            return workItem.GetField<DateTime>(WorkItemConstants.VstsFields.ResolvedDate);
        }

        public static string ResolvedReason(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.VstsFields.ResolvedReason);
        }

        public static int? StackRank(this WorkItem workItem)
        {
            return workItem.GetField<int>(WorkItemConstants.VstsFields.StackRank);
        }

        public static DateTime? StateChangeDate(this WorkItem workItem)
        {
            return workItem.GetField<DateTime>(WorkItemConstants.VstsFields.StateChangeDate);
        }

        public static double? OriginalEstimate(this WorkItem workItem)
        {
            return workItem.GetField<double>(WorkItemConstants.VstsFields.OriginalEstimate);
        }

        public static double? RemainingWork(this WorkItem workItem)
        {
            return workItem.GetField<double>(WorkItemConstants.VstsFields.RemainingWork);
        }

        public static double? StoryPoints(this WorkItem workItem)
        {
            return workItem.GetField<double>(WorkItemConstants.VstsFields.StoryPoints);
        }

        public static string ReproSteps(this WorkItem workItem)
        {
            return workItem.GetField(WorkItemConstants.VstsFields.ReproSteps);
        }

        // Other

        public static string GetShortTitle(this WorkItem workItem)
        {
            return String.Format("{0} {1}", workItem.WorkItemType(), workItem.Id);
        }

        public static string GetFullTitle(this WorkItem workItem)
        {
            string title = workItem.Title();

            if (!String.IsNullOrEmpty(title))
            {
                return String.Format("{0} {1}: {2}", workItem.WorkItemType(), workItem.Id, title.Trim());
            }
            else
            {
                return String.Format("{0} {1}", workItem.WorkItemType(), workItem.Id);
            }
        }

        public static Uri GetProjectCollectionUrl(this WorkItem workItem)
        {
            string url = workItem.Url;
            int index = url.IndexOf("/_apis");
            string projectCollectionUrl = url.Substring(0, index);
            return new Uri(projectCollectionUrl);
        }
    }
}
