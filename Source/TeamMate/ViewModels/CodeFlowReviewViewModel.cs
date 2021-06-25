using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.DirectoryServices;
using Microsoft.Internal.Tools.TeamMate.Foundation.Shell;
using Microsoft.Internal.Tools.TeamMate.Platform.CodeFlow;
using Microsoft.Internal.Tools.TeamMate.Platform.CodeFlow.Dashboard;
using Microsoft.Internal.Tools.TeamMate.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.Internal.Tools.TeamMate.ViewModels
{
    public class CodeFlowReviewViewModel : TrackableViewModelBase
    {
        private CodeReviewSummary summary;

        public CodeReviewSummary Summary
        {
            get { return this.summary; }
            set
            {
                if (SetProperty(ref this.summary, value))
                {
                    Invalidate();
                }
            }
        }

        public DateTime ChangeDate
        {
            get { return Summary.LastUpdatedOn; }
        }

        private void Invalidate()
        {
            if (Summary == null)
            {
                return;
            }

            this.IterationCount = Summary.IterationCount;
            this.Name = Summary.Name;
            this.AuthorDisplayName = Summary.Author.DisplayName;
            this.CreatedOn = Summary.CreatedOn;
            this.IsActive = Summary.IsActive();
            this.Reference = Summary.GetReference();

            ResetTrackingToken();

            this.SignOffCount = Summary.CountReviewerStatus(ReviewerStatus.SignedOff);
            this.WaitingCount = Summary.CountReviewerStatus(ReviewerStatus.Waiting);

            this.IsSignedOff = (this.SignOffCount > 0);
            this.IsWaiting = (this.WaitingCount > 0);
            this.IsPending = IsActive && !IsSignedOff;
            this.IsCompleted = (Summary.Status == CodeReviewStatus.Completed);
            this.IsOwnedByMe = Summary.Author.IsMe();
            this.IsActionedByMe = Summary.MyFeedbackStatus().IsActioned();

            if (this.IsSignedOff)
            {
                this.SignedOffOn = Summary.SignedOffOn();
                this.IsSignedOffByMe = (Summary.MyFeedbackStatus() == ReviewerStatus.SignedOff);
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

        public int SignOffCount { get; set; }
        public int WaitingCount { get; set; }

        public bool IsOwnedByMe { get; set; }
        public bool IsActionedByMe { get; set; }

        public CodeFlowReviewReference Reference { get; set; }

        public DateTime? SignedOffOn { get; set; }

        public Uri GetWebViewUri()
        {
            return Summary.GetWebViewUri();
        }

        public Uri GetLaunchClientUri()
        {
            return Summary.GetLaunchClientUri();
        }

        public Uri GetLaunchVisualStudioUri()
        {
            return Summary.GetLaunchVisualStudioUri();
        }

        public string GetFullTitle()
        {
            return Summary.GetFullTitle();
        }

        public bool IsActionableByMe()
        {
            bool result = false;

            if (Summary.IsActive())
            {
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
            }

            return result;
        }

        public bool Matches(MultiWordMatcher matcher)
        {
            // TODO: Reviewers too? Status?
            return matcher.Matches(Summary.Name, Summary.Author.Name, Summary.Author.DisplayName);
        }

        public void OpenInCodeFlow()
        {
            Process.Start(GetLaunchClientUri().AbsoluteUri);
            this.IsRead = true;

            Telemetry.Event(TelemetryEvents.CodeFlowReviewOpened);
        }

        public void OpenInVisualStudio()
        {
            Process.Start(GetLaunchVisualStudioUri().AbsoluteUri);
            this.IsRead = true;

            Telemetry.Event(TelemetryEvents.CodeFlowReviewOpened, new TelemetryEventProperties() {
                { TelemetryEvents.Properties.InVisualStudio, true }
            });
        }

        public void OpenInWebBrowser()
        {
            ExternalWebBrowser.Launch(GetWebViewUri());

            Telemetry.Event(TelemetryEvents.CodeFlowReviewOpened, new TelemetryEventProperties() {
                { TelemetryEvents.Properties.InWebBrowser, true }
            });
        }

        private static async Task<UserEntry> TryGetDomainUserEntry(string authorAccountName)
        {
            UserEntry user = null;
            // TODO: Move to service, caching? Use identity cache?...
            DirectoryBrowser browser = new DirectoryBrowser();
            if (browser.IsInDomain)
            {
                try
                {
                    // TODO: These user entries need to be cached...
                    user = await Task.Run(() => browser.FindUserByAccountName(authorAccountName));
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }

            return user;
        }

        protected override bool WasLastChangedByMe()
        {
            return (Summary != null && Summary.GetLastChange().IsMe());
        }

        protected override int GetRevision()
        {
            return (Summary != null) ? Summary.Revision : 0;
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
