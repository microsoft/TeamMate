using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.Tools.TeamMate.Foundation.Shell;
using Microsoft.Tools.TeamMate.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class PullRequestRowViewModel : TrackableViewModelBase
    {
        private GitPullRequest reference;
        private List<GitPullRequestIteration> iterations;

        public GitPullRequest Reference
        {
            get { return this.reference; }
            set
            {
                if (SetProperty(ref this.reference, value))
                {
                    Invalidate();
                }
            }
        }

        public string IdentityRef { get; set; }

        public List<GitPullRequestIteration> Iterations
        {
            get { return this.iterations;  }
            set
            {
                if (SetProperty(ref this.iterations, value))
                {
                    this.IterationCount = this.iterations.Count;
                }

                this.iterations.Sort((a, b) => a.UpdatedDate.GetValueOrDefault(DateTime.MinValue).CompareTo(b.UpdatedDate.GetValueOrDefault(DateTime.MinValue)));
            }
        }

        public DateTime ChangedDate
        {
            get { return this.Iterations[this.IterationCount - 1].UpdatedDate.GetValueOrDefault(DateTime.MinValue); }
        }

        public string ChangedBy
        {
            get { return this.Iterations[this.IterationCount - 1].Author.DisplayName; }
        }

        private void Invalidate()
        {
            if (Reference == null)
            {
                return;
            }

            this.Name = this.Reference.Title;
            this.CreatedBy = this.Reference.CreatedBy.DisplayName;
            this.CreatedDate = this.Reference.CreationDate;
            this.IsActive = this.Reference.Status == PullRequestStatus.Active;
            this.IsOwnedByMe = (this.Reference.CreatedBy.Id == this.IdentityRef);

            ResetTrackingToken();

            this.IsSignedOff = this.Reference.Reviewers.Count(x => x.IsRequired && x.Vote != 10 && x.Vote != 5) == 0;
            this.IsWaiting = this.Reference.Reviewers.Count(x => x.IsRequired && x.Vote == -5) > 0;
            this.IsPending = this.IsActive && !this.IsSignedOff;
            this.IsCompleted = (this.Reference.Status == PullRequestStatus.Completed);
            this.IsAssignedToMe = this.IsActive && this.Reference.Reviewers.Count(x => x.Id == this.IdentityRef) == 1;

            if (this.IsSignedOff)
            {
                this.IsSignedOffByMe = this.Reference.Reviewers.Count(x => x.Id == this.IdentityRef && (x.Vote == 10 || x.Vote == 5)) == 1;
            }

            this.BottomLeftText = this.CreatedBy;

            if (this.IterationCount > 1)
            {
                this.BottomLeftText = String.Format("{0} - Iteration {1}", this.BottomLeftText, this.IterationCount);
            }
        }

        public string BottomLeftText { get; set; }
        public int IterationCount { get; set; }
        public string CreatedBy { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPending { get; set; }
        public bool IsSignedOff { get; set; }
        public bool IsSignedOffByMe { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsWaiting { get; set; }

        public bool IsOwnedByMe { get; set; }

        public bool IsAssignedToMe { get; set; }

        public Uri Url { get; set; }
 
        public string ProjectName { get; set; }

        public string GetFullTitle()
        {
            return Reference.Title;
        }

        public bool Matches(MultiWordMatcher matcher)
        {
            // TODO: Reviewers too? Status?
            return matcher.Matches(Reference.Title, Reference.CreatedBy.DisplayName, Reference.CreatedBy.DirectoryAlias);
        }

        public void OpenInWebBrowser()
        {
            ExternalWebBrowser.Launch(this.Url);
        }

        protected override bool WasLastChangedByMe()
        {
            if (this.Iterations == null)
            {
                return false;
            }

            return this.Iterations[this.IterationCount - 1].Author.DisplayName == this.IdentityRef;
        }

        protected override int GetRevision()
        {
            return this.IterationCount;
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
