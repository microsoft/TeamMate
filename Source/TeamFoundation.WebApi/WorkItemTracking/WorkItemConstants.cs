using System;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    public static class WorkItemConstants
    {
        public static readonly IEqualityComparer<string> TagComparer = StringComparer.OrdinalIgnoreCase;
        public const string TagSeparator = "; ";

        public static class Types
        {
            public const string Bug = "Bug";
            public const string CodeReviewRequest = "Code Review Request";
            public const string CodeReviewResponse = "Code Review Response";
            public const string Epic = "Epic";
            public const string Feature = "Feature";
            public const string FeedbackRequest = "Feedback Request";
            public const string FeedbackResponse = "Feedback Response";
            public const string Issue = "Issue";
            public const string SharedParameter = "Shared Parameter";
            public const string SharedSteps = "Shared Steps";
            public const string Task = "Task";
            public const string TestCase = "Test Case";
            public const string TestPlan = "Test Plan";
            public const string TestSuite = "Test Suite";
            public const string UserStory = "User Story";
        }

        public static class Categories
        {
            public const string Bug = "Microsoft.BugCategory";
            public const string Hidden = "Microsoft.HiddenCategory";
            public const string Requirement = "Microsoft.RequirementCategory";
            public const string Task = "Microsoft.TaskCategory";
            public const string TestCase = "Microsoft.TestCaseCategory";
        }

        public static class CoreFields
        {
            public const string AreaId = "System.AreaId";
            public const string AreaLevel1 = "System.AreaLevel1";
            public const string AreaLevel2 = "System.AreaLevel2";
            public const string AreaLevel3 = "System.AreaLevel3";
            public const string AreaLevel4 = "System.AreaLevel4";
            public const string AreaLevel5 = "System.AreaLevel5";
            public const string AreaLevel6 = "System.AreaLevel6";
            public const string AreaLevel7 = "System.AreaLevel7";
            public const string AreaPath = "System.AreaPath";
            public const string AssignedTo = "System.AssignedTo";
            public const string AttachedFileCount = "System.AttachedFileCount";
            public const string AttachedFiles = "System.AttachedFiles";
            public const string AuthorizedAs = "System.AuthorizedAs";
            public const string AuthorizedDate = "System.AuthorizedDate";
            public const string BISLinks = "System.BISLinks";
            public const string BoardColumn = "System.BoardColumn";
            public const string BoardColumnDone = "System.BoardColumnDone";
            public const string BoardLane = "System.BoardLane";
            public const string ChangedBy = "System.ChangedBy";
            public const string ChangedDate = "System.ChangedDate";
            public const string CreatedBy = "System.CreatedBy";
            public const string CreatedDate = "System.CreatedDate";
            public const string Description = "System.Description";
            public const string ExternalLinkCount = "System.ExternalLinkCount";
            public const string History = "System.History";
            public const string HyperLinkCount = "System.HyperLinkCount";
            public const string Id = "System.Id";
            public const string InAdminOnlyTreeFlag = "System.InAdminOnlyTreeFlag";
            public const string InDeletedTreeFlag = "System.InDeletedTreeFlag";
            public const string IsDeleted = "System.IsDeleted";
            public const string IterationId = "System.IterationId";
            public const string IterationLevel1 = "System.IterationLevel1";
            public const string IterationLevel2 = "System.IterationLevel2";
            public const string IterationLevel3 = "System.IterationLevel3";
            public const string IterationLevel4 = "System.IterationLevel4";
            public const string IterationLevel5 = "System.IterationLevel5";
            public const string IterationLevel6 = "System.IterationLevel6";
            public const string IterationLevel7 = "System.IterationLevel7";
            public const string IterationPath = "System.IterationPath";
            public const string LinkedFiles = "System.LinkedFiles";
            public const string LinkType = "System.Links.LinkType";
            public const string NodeName = "System.NodeName";
            public const string NodeType = "System.NodeType";
            public const string PersonId = "System.PersonId";
            public const string ProjectId = "System.ProjectId";
            public const string Reason = "System.Reason";
            public const string RelatedLinkCount = "System.RelatedLinkCount";
            public const string RelatedLinks = "System.RelatedLinks";
            public const string Rev = "System.Rev";
            public const string RevisedDate = "System.RevisedDate";
            public const string State = "System.State";
            public const string Tags = "System.Tags";
            public const string TeamProject = "System.TeamProject";
            public const string TFServer = "System.TFServer";
            public const string Title = "System.Title";
            public const string Watermark = "System.Watermark";
            public const string WorkItemForm = "System.WorkItemForm";
            public const string WorkItemFormId = "System.WorkItemFormId";
            public const string WorkItemType = "System.WorkItemType";
        }

        public static class VstsFields
        {
            public const string SubState = "Microsoft.VSTS.Common.SubState";
            public const string IntegrationBuild = "Microsoft.VSTS.Build.IntegrationBuild";
            public const string AcceptedBy = "Microsoft.VSTS.CodeReview.AcceptedBy";
            public const string AcceptedDate = "Microsoft.VSTS.CodeReview.AcceptedDate";
            public const string ClosedStatus = "Microsoft.VSTS.CodeReview.ClosedStatus";
            public const string ClosedStatusCode = "Microsoft.VSTS.CodeReview.ClosedStatusCode";
            public const string ClosingComment = "Microsoft.VSTS.CodeReview.ClosingComment";
            public const string Context = "Microsoft.VSTS.CodeReview.Context";
            public const string ContextCode = "Microsoft.VSTS.CodeReview.ContextCode";
            public const string ContextOwner = "Microsoft.VSTS.CodeReview.ContextOwner";
            public const string ContextType = "Microsoft.VSTS.CodeReview.ContextType";
            public const string AcceptanceCriteria = "Microsoft.VSTS.Common.AcceptanceCriteria";
            public const string ActivatedBy = "Microsoft.VSTS.Common.ActivatedBy";
            public const string ActivatedDate = "Microsoft.VSTS.Common.ActivatedDate";
            public const string Activity = "Microsoft.VSTS.Common.Activity";
            public const string BusinessValue = "Microsoft.VSTS.Common.BusinessValue";
            public const string ClosedBy = "Microsoft.VSTS.Common.ClosedBy";
            public const string ClosedDate = "Microsoft.VSTS.Common.ClosedDate";
            public const string Issue = "Microsoft.VSTS.Common.Issue";
            public const string Priority = "Microsoft.VSTS.Common.Priority";
            public const string Rating = "Microsoft.VSTS.Common.Rating";
            public const string ResolvedBy = "Microsoft.VSTS.Common.ResolvedBy";
            public const string ResolvedDate = "Microsoft.VSTS.Common.ResolvedDate";
            public const string ResolvedReason = "Microsoft.VSTS.Common.ResolvedReason";
            public const string ReviewedBy = "Microsoft.VSTS.Common.ReviewedBy";
            public const string Risk = "Microsoft.VSTS.Common.Risk";
            public const string Severity = "Microsoft.VSTS.Common.Severity";
            public const string StackRank = "Microsoft.VSTS.Common.StackRank";
            public const string StateChangeDate = "Microsoft.VSTS.Common.StateChangeDate";
            public const string StateCode = "Microsoft.VSTS.Common.StateCode";
            public const string TimeCriticality = "Microsoft.VSTS.Common.TimeCriticality";
            public const string ValueArea = "Microsoft.VSTS.Common.ValueArea";
            public const string ApplicationLaunchInstructions = "Microsoft.VSTS.Feedback.ApplicationLaunchInstructions";
            public const string ApplicationStartInformation = "Microsoft.VSTS.Feedback.ApplicationStartInformation";
            public const string ApplicationType = "Microsoft.VSTS.Feedback.ApplicationType";
            public const string CompletedWork = "Microsoft.VSTS.Scheduling.CompletedWork";
            public const string DueDate = "Microsoft.VSTS.Scheduling.DueDate";
            public const string Effort = "Microsoft.VSTS.Scheduling.Effort";
            public const string FinishDate = "Microsoft.VSTS.Scheduling.FinishDate";
            public const string OriginalEstimate = "Microsoft.VSTS.Scheduling.OriginalEstimate";
            public const string RemainingWork = "Microsoft.VSTS.Scheduling.RemainingWork";
            public const string StartDate = "Microsoft.VSTS.Scheduling.StartDate";
            public const string StoryPoints = "Microsoft.VSTS.Scheduling.StoryPoints";
            public const string TargetDate = "Microsoft.VSTS.Scheduling.TargetDate";
            public const string AutomatedTestId = "Microsoft.VSTS.TCM.AutomatedTestId";
            public const string AutomatedTestName = "Microsoft.VSTS.TCM.AutomatedTestName";
            public const string AutomatedTestStorage = "Microsoft.VSTS.TCM.AutomatedTestStorage";
            public const string AutomatedTestType = "Microsoft.VSTS.TCM.AutomatedTestType";
            public const string AutomationStatus = "Microsoft.VSTS.TCM.AutomationStatus";
            public const string LocalDataSource = "Microsoft.VSTS.TCM.LocalDataSource";
            public const string Parameters = "Microsoft.VSTS.TCM.Parameters";
            public const string QueryText = "Microsoft.VSTS.TCM.QueryText";
            public const string ReproSteps = "Microsoft.VSTS.TCM.ReproSteps";
            public const string Steps = "Microsoft.VSTS.TCM.Steps";
            public const string SystemInfo = "Microsoft.VSTS.TCM.SystemInfo";
            public const string TestSuiteAudit = "Microsoft.VSTS.TCM.TestSuiteAudit";
            public const string TestSuiteType = "Microsoft.VSTS.TCM.TestSuiteType";
            public const string TestSuiteTypeId = "Microsoft.VSTS.TCM.TestSuiteTypeId";
        }
    }
}
