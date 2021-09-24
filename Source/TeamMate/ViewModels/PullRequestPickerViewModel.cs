using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Validation;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using System;
using System.Linq;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class PullRequestPickerViewModel : ValidatableViewModelBase
    {
        private string name;
        private PullRequestQueryInfo queryInfo;
        private string authors;
        private string reviewer;
        private PullRequestQueryReviewStatus reviewStatus;

        public PullRequestPickerViewModel()
        {
            Validator.RuleForProperty(() => Name)
                .IsNotEmpty();

            Validator.Rule(
                ValidateHasAuthorOrReviewerOrProject, "You must input at least one of the following fields: author, reviewer, or project."
            );
        }

        public PullRequestQueryInfo QueryInfo
        {
            get { return this.queryInfo; }
            set
            {
                if (SetProperty(ref this.queryInfo, value))
                {
                    Invalidate();
                }
            }
        }

        public string Name
        {
            get { return this.name; }
            set { SetProperty(ref this.name, value); }
        }

        public string Authors
        {
            get { return this.authors; }
            set { SetProperty(ref this.authors, value); }
        }

        public string Reviewers
        {
            get { return this.reviewer; }
            set { SetProperty(ref this.reviewer, value); }
        }

        public PullRequestQueryReviewStatus ReviewStatus
        {
            get { return this.reviewStatus; }
            set { SetProperty(ref this.reviewStatus, value); }
        }

        private void Invalidate()
        {
            if (this.queryInfo != null)
            {
                this.Name = this.queryInfo.Name;
                this.Authors = StringUtilities.ToCommaSeparatedList(this.queryInfo.Authors);
                this.Reviewers = StringUtilities.ToCommaSeparatedList(this.queryInfo.Reviewers);
                this.ReviewStatus = this.queryInfo.ReviewStatus;
            }
        }

        public void Flush()
        {
            if (this.queryInfo != null)
            {
                this.queryInfo.Name = this.Name.Trim();
                this.queryInfo.Authors = StringUtilities.FromCommaSeparatedList(this.Authors);
                this.queryInfo.Reviewers = StringUtilities.FromCommaSeparatedList(this.Reviewers);
                this.queryInfo.ReviewStatus = this.ReviewStatus;
            }
        }

        private bool ValidateHasAuthorOrReviewerOrProject()
        {
            var authors = GetAuthors();
            var reviewers = GetReviewers();

            return (authors != null && authors.Any())
                || (reviewers != null && reviewers.Any());
        }

        private string[] GetReviewers()
        {
            return StringUtilities.FromCommaSeparatedList(this.Reviewers);
        }

        private string[] GetAuthors()
        {
            return StringUtilities.FromCommaSeparatedList(this.Authors);
        }

        public object AllReviewStatuses
        {
            get { return Enum.GetValues(typeof(PullRequestQueryReviewStatus)); }
        }
    }
}
