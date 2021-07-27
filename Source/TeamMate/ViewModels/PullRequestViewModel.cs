using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.Tools.TeamMate.Foundation.Shell;
using Microsoft.Tools.TeamMate.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class PullRequestViewModel : TrackableViewModelBase
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

        public DateTime ChangeDate
        {
            // TODO(MEM)
            get { return this.Iterations[this.IterationCount - 1].UpdatedDate.GetValueOrDefault(DateTime.MinValue); }
        }

        private void Invalidate()
        {
            if (Reference == null)
            {
                return;
            }

            this.Name = Reference.Title;
            this.AuthorDisplayName = Reference.CreatedBy.DisplayName;
            this.CreatedOn = Reference.CreationDate;
            this.IsActive = Reference.Status == PullRequestStatus.Active;
            this.IsOwnedByMe = (Reference.CreatedBy.Id == this.IdentityRef);

            ResetTrackingToken();

            // TODO(MEM)
            //this.SignOffCount = Summary.CountReviewerStatus(ReviewerStatus.SignedOff);
            //this.WaitingCount = Summary.CountReviewerStatus(ReviewerStatus.Waiting);

            this.IsSignedOff = (this.SignOffCount > 0);
            this.IsWaiting = (this.WaitingCount > 0);
            this.IsPending = IsActive && !IsSignedOff;
            //this.IsCompleted = (Summary.Status == CodeReviewStatus.Completed);

            if (this.IsSignedOff)
            {
                this.SignedOffOn = Reference.ClosedDate;
                this.IsSignedOffByMe = Reference.Reviewers.Count(x => x.Id == this.IdentityRef && (x.Vote == 10 || x.Vote == 5)) == 1;
            }

            this.BottomLeftText = this.AuthorDisplayName;

            if (this.IterationCount > 1)
            {
                this.BottomLeftText = String.Format("{0} - Iteration {1}", this.BottomLeftText, this.IterationCount);
            }
        }

        public string BottomLeftText { get; set; }
        public int IterationCount { get; set; }
        public string AuthorDisplayName { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }

        public bool IsActive { get; set; }
        public bool IsPending { get; set; }
        public bool IsSignedOff { get; set; }
        public bool IsSignedOffByMe { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsWaiting { get; set; }

        public bool IsOwnedByMe { get; set; }

        public int SignOffCount { get; set; }
        public int WaitingCount { get; set; }

        public DateTime? SignedOffOn { get; set; }

        public Uri GetWebViewUri()
        {
            // TODO(MEM)
            return new Uri(this.Reference.Url);
        }

        public string GetFullTitle()
        {
            return Reference.Title;
        }

        public bool IsActionableByMe()
        {
            bool result = false;

            if (this.IsActive)
            {
              //  Array.Find<IdentityRefWithVote>
               // Reference.Reviewers.Find

                // TODO(MEM)
                /*
                if (!IsOwnedByMe)
                {
                    // If I have not reviewed yet, or I reviewed as "Waiting" and the author made updates...
                    result = Summary.NotReviewedByMeOrUpdatedAfterReview();
                }
                else
                {
                    // If it is my review, check that it has at least a signed off or waiting status, and it is not already completed
                    result = Summary.IsSignedOffOrWaiting();
                }
                */
            }

            return result;
        }

        public bool Matches(MultiWordMatcher matcher)
        {
            // TODO: Reviewers too? Status?
            return matcher.Matches(Reference.Title, Reference.CreatedBy.DisplayName, Reference.CreatedBy.DirectoryAlias);
        }

        public void OpenInWebBrowser()
        {
            ExternalWebBrowser.Launch(GetWebViewUri());
        }

        protected override bool WasLastChangedByMe()
        {
            return false;
            // TODO(MEM)
        //    return (Summary != null && Summary.GetLastChange().IsMe());
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
