// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Validation;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using System;
using System.Linq;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class CodeFlowPickerViewModel : ValidatableViewModelBase
    {
        private string name;
        private CodeFlowQueryInfo queryInfo;
        private string authors;
        private string reviewer;
        private string projects;
        private CodeFlowQueryReviewPeriod reviewPeriod;
        private CodeFlowQueryReviewStatuses reviewStatuses;

        public CodeFlowPickerViewModel()
        {
            Validator.RuleForProperty(() => Name)
                .IsNotEmpty();

            Validator.Rule(
                ValidateHasAuthorOrReviewerOrProject, "You must input at least one of the following fields: author, reviewer, or project."
            );
        }

        public CodeFlowQueryInfo QueryInfo
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

        public string Projects
        {
            get { return this.projects; }
            set { SetProperty(ref this.projects, value); }
        }

        public CodeFlowQueryReviewPeriod ReviewPeriod
        {
            get { return this.reviewPeriod; }
            set { SetProperty(ref this.reviewPeriod, value); }
        }

        public CodeFlowQueryReviewStatuses ReviewStatuses
        {
            get { return this.reviewStatuses; }
            set { SetProperty(ref this.reviewStatuses, value); }
        }

        private void Invalidate()
        {
            if (this.queryInfo != null)
            {
                this.Name = this.queryInfo.Name;
                this.Authors = StringUtilities.ToCommaSeparatedList(this.queryInfo.Authors);
                this.Reviewers = StringUtilities.ToCommaSeparatedList(this.queryInfo.Reviewers);
                this.Projects = StringUtilities.ToCommaSeparatedList(this.queryInfo.Projects);
                this.ReviewPeriod = this.queryInfo.ReviewPeriod;
                this.ReviewStatuses = this.queryInfo.ReviewStatuses;
            }
        }

        public void Flush()
        {
            if (this.queryInfo != null)
            {
                this.queryInfo.Name = this.Name.Trim();
                this.queryInfo.Authors = StringUtilities.FromCommaSeparatedList(this.Authors);
                this.queryInfo.Reviewers = StringUtilities.FromCommaSeparatedList(this.Reviewers);
                this.queryInfo.Projects = StringUtilities.FromCommaSeparatedList(this.Projects);
                this.queryInfo.ReviewPeriod = this.ReviewPeriod;
                this.queryInfo.ReviewStatuses = this.ReviewStatuses;
            }
        }

        private bool ValidateHasAuthorOrReviewerOrProject()
        {
            var authors = GetAuthors();
            var reviewers = GetReviewers();
            var projects = GetProjects();

            return (authors != null && authors.Any())
                || (reviewers != null && reviewers.Any())
                || (projects != null && projects.Any());
        }

        private string[] GetProjects()
        {
            return StringUtilities.FromCommaSeparatedList(this.Projects);
        }

        private string[] GetReviewers()
        {
            return StringUtilities.FromCommaSeparatedList(this.Reviewers);
        }

        private string[] GetAuthors()
        {
            return StringUtilities.FromCommaSeparatedList(this.Authors);
        }

        public object AllReviewPeriods
        {
            get { return Enum.GetValues(typeof(CodeFlowQueryReviewPeriod)); }
        }

        public object AllReviewStatuses
        {
            get { return Enum.GetValues(typeof(CodeFlowQueryReviewStatuses)); }
        }
    }
}
