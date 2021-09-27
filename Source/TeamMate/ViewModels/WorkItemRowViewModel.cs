using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using WorkItemReference = Microsoft.Tools.TeamMate.Model.WorkItemReference;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class WorkItemRowViewModel : TrackableViewModelBase
    {
        public static readonly string[] RequiredWorkItemFields = {
            WorkItemConstants.CoreFields.AreaPath,
            WorkItemConstants.CoreFields.AssignedTo,
            WorkItemConstants.CoreFields.ChangedBy,
            WorkItemConstants.CoreFields.ChangedDate,
            WorkItemConstants.CoreFields.CreatedBy,
            WorkItemConstants.CoreFields.CreatedDate,
            WorkItemConstants.CoreFields.IterationPath,
            WorkItemConstants.CoreFields.State,
            WorkItemConstants.CoreFields.Title,
            WorkItemConstants.CoreFields.Rev,
            WorkItemConstants.CoreFields.ExternalLinkCount,
            WorkItemConstants.CoreFields.HyperLinkCount,
            WorkItemConstants.CoreFields.RelatedLinkCount,
            WorkItemConstants.CoreFields.AttachedFileCount,
            WorkItemConstants.CoreFields.Tags,
            WorkItemConstants.CoreFields.WorkItemType,
            WorkItemConstants.CoreFields.TeamProject
        };

        public static readonly string[] OptionalWorkItemFields = {
            WorkItemConstants.VstsFields.SubState,
            WorkItemConstants.VstsFields.Priority,
            WorkItemConstants.VstsFields.ResolvedBy,
            WorkItemConstants.VstsFields.ResolvedDate,
            WorkItemConstants.VstsFields.ResolvedReason
        };

        private IDictionary<string, object> additionalFields;
        private string bottomLeftValue;
        private string bottomRightValue;
        private WorkItemState? workItemState;
        private WorkItem workItem;

        public WorkItem WorkItem
        {
            get { return this.workItem; }
            set
            {
                if (SetProperty(ref this.workItem, value))
                {
                    Invalidate();
                }
            }
        }

        public WorkItemReference Reference { get; set; }
        public string Type { get; set; }

        public string AreaPath { get; set; }
        public string AssignedTo { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string IterationPath { get; set; }
        public string State { get; set; }
        public string Title { get; set; }
        public int Revision { get; set; }
        public int LinkCount { get; set; }
        public int AttachmentCount { get; set; }

        // Additional, optional fields
        public int? Priority { get; set; }
        public string SubState { get; set; }
        public string ResolvedReason { get; set; }

        private string[] tags;

        public string[] Tags
        {
            get { return this.tags; }
            set { SetProperty(ref this.tags, value); }
        }

        public bool IsHighPriority { get; set; }

        public string BottomLeftValue
        {
            get
            {
                if (this.bottomLeftValue == null)
                {
                    StringBuilder sb = new StringBuilder();
                    if (!String.IsNullOrEmpty(State))
                    {
                        sb.Append(State);
                    }

                    if (Priority != null)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(" - ");
                        }

                        sb.Append(Formatter.FormatPriority(Priority.Value));
                    }

                    if (!String.IsNullOrEmpty(AssignedTo))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(" - ");
                        }

                        sb.Append(AssignedTo);
                    }

                    if (this.WorkItemState == WorkItemState.Active)
                    {
                        if (!String.IsNullOrEmpty(SubState))
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append(" - ");
                            }

                            sb.Append(SubState);
                        }
                    }
                    else if (this.workItemState == WorkItemState.Resolved || this.workItemState == WorkItemState.Closed)
                    {
                        if (!String.IsNullOrEmpty(ResolvedReason))
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append(" - ");
                            }

                            sb.Append(ResolvedReason);
                        }
                    }

                    this.bottomLeftValue = sb.ToString();
                }

                return this.bottomLeftValue;
            }
        }

        public string BottomRightValue
        {
            get
            {
                if (this.bottomRightValue == null)
                {
                    bool first = true;

                    StringBuilder sb = new StringBuilder();
                    foreach (var tag in Tags)
                    {
                        if (!first)
                        {
                            sb.Append(" | ");
                        }

                        first = false;

                        sb.Append(tag);
                    }

                    this.bottomRightValue = sb.ToString();
                }

                return this.bottomRightValue;
            }
        }

        public object this[string name]
        {
            get
            {
                object result = null;
                if (additionalFields != null)
                {
                    additionalFields.TryGetValue(name, out result);
                }

                return result;
            }

            set
            {
                if (additionalFields == null)
                {
                    additionalFields = new Dictionary<string, object>();
                }

                additionalFields[name] = value;
            }
        }

        public int Id
        {
            get { return Reference.Id; }
        }

        public Uri ProjectCollectionUri
        {
            get { return Reference.ProjectCollectionUri; }
        }

        public string ShortTitle { get; set; }

        public string FullTitle { get; set; }

        private bool wasLastChangedByMe;

        private void Invalidate()
        {
            if (WorkItem == null)
            {
                return;
            }

            ResetTrackingToken();

            this.workItemState = null;
            this.additionalFields = null;
            this.wasLastChangedByMe = false;

            this.Reference = WorkItem.GetReference();

            this.Type = WorkItem.WorkItemType();

            this.AreaPath = WorkItem.AreaPath();

            this.AssignedTo = Formatter.FormatAssignedTo(WorkItemIdentity.GetDisplayName(WorkItem.AssignedTo()));

            var workItemChangedBy = WorkItem.ChangedBy();

            this.ChangedBy = WorkItemIdentity.GetDisplayName(workItemChangedBy);
            this.ChangedDate = WorkItem.ChangedDate().Value;
            this.CreatedBy = WorkItemIdentity.GetDisplayName(WorkItem.CreatedBy());
            this.CreatedDate = WorkItem.CreatedDate().Value;
            this.IterationPath = WorkItem.IterationPath();
            this.State = WorkItem.State();
            this.Title = WorkItem.Title();
            this.ShortTitle = workItem.GetShortTitle();
            this.FullTitle = workItem.GetFullTitle();
            this.Revision = WorkItem.Rev.Value;

            this.AttachmentCount = (int)WorkItem.GetField<long>(WorkItemConstants.CoreFields.AttachedFileCount).Value;
            this.LinkCount = (int)(WorkItem.GetField<long>(WorkItemConstants.CoreFields.ExternalLinkCount).Value
                + WorkItem.GetField<long>(WorkItemConstants.CoreFields.HyperLinkCount).Value
                + WorkItem.GetField<long>(WorkItemConstants.CoreFields.RelatedLinkCount).Value);

            this.Tags = WorkItem.Tags();

            var me = this.SessionService.Session.ProjectContext?.WorkItemIdentity;
            this.wasLastChangedByMe = (me != null && me.FullName == workItemChangedBy);
        }

        [Import]
        public SessionService SessionService { get; set; }

        public WorkItemState WorkItemState
        {
            get
            {
                if (this.workItemState == null)
                {
                    this.workItemState = (this.WorkItemStateService != null) ? this.WorkItemStateService.GetWorkItemState(this.Type, this.State) : WorkItemState.Unknown;
                }

                return this.workItemState.Value;
            }
        }

        [Import]
        public WorkItemStateService WorkItemStateService { get; set; }

        [Import]
        public WindowService WindowService { get; set; }

        public void Open()
        {
            this.WindowService.ShowWorkItemWindow(this.Reference);
            this.TrackingService.MarkAsRead(this.WorkItem);
        }

        public bool Matches(MultiWordMatcher matcher)
        {
            // TODO: Improve for multiword, e.g. do AND/OR, etc?
            IEnumerable<string> values = new string[] { Title, State, AssignedTo, Id.ToString() };
            if (this.Tags != null && this.Tags.Any())
            {
                values = values.Concat(this.Tags);
            }

            return matcher.Matches(values);
        }

        public override int GetHashCode()
        {
            return Reference.Id;
        }

        public override bool Equals(object obj)
        {
            WorkItemRowViewModel other = obj as WorkItemRowViewModel;
            if (other != null)
            {
                return this.Reference.Equals(other.Reference);
            }

            return false;
        }

        public override string ToString()
        {
            return Reference.ToString();
        }

        protected override bool WasLastChangedByMe()
        {
            return wasLastChangedByMe;
        }

        protected override int GetRevision()
        {
            return this.Revision;
        }

        protected override object GetFlaggedItem()
        {
            return this;
        }

        protected override object GetTrackingTokenKey()
        {
            return this.Reference;
        }
    }
}
