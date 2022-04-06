using Microsoft.Tools.TeamMate.Foundation.Validation;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class PullRequestPickerViewModel : ValidatableViewModelBase
    {
        private string name;
        private string sourceRefMatchExpression;
        private PullRequestQueryInfo queryInfo;
        private PullRequestQueryReviewStatus reviewStatus;

        private string _selectedAssignedTo;
        private ObservableCollection<string> _assignedTo = new ObservableCollection<string>()
        {
            "@me",
            "",
        };

        private string _selectedCreatedBy;
        private ObservableCollection<string> _createdBy = new ObservableCollection<string>()
        {
            "@me",
            "",
        };

        public PullRequestPickerViewModel()
        {
            Validator.RuleForProperty(() => Name)
                .IsNotEmpty();
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

        public string SourceRefMatchExpression
        {
            get { return this.sourceRefMatchExpression; }
            set { SetProperty(ref this.sourceRefMatchExpression, value); }
        }

        public PullRequestQueryReviewStatus ReviewStatus
        {
            get { return this.reviewStatus; }
            set { SetProperty(ref this.reviewStatus, value); }
        }
        public IEnumerable AssignedTo
        {
            get { return this._assignedTo; }
        }
        public string SelectedAssignedTo
        {
            get { return this._selectedAssignedTo; }
            set
            {
                this._selectedAssignedTo = value;
                OnPropertyChanged("SelectedAssignedTo");
            }
        }

        public string NewAssignedTo
        {
            set
            {
                if (SelectedAssignedTo != null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    this._assignedTo.Add(value);
                    SelectedAssignedTo = value;
                }
            }
        }

        public IEnumerable CreatedBy
        {
            get { return this._createdBy; }
        }
        public string SelectedCreatedBy
        {
            get { return this._selectedCreatedBy; }
            set
            {
                this._selectedCreatedBy = value;
                OnPropertyChanged("SelectedCreatedBy");
            }
        }

        public string NewCreatedBy
        {
            set
            {
                if (SelectedCreatedBy != null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    this._createdBy.Add(value);
                    SelectedCreatedBy = value;
                }
            }
        }
        private void Invalidate()
        {
            if (this.queryInfo != null)
            {
                this.Name = this.queryInfo.Name;
                this.ReviewStatus = this.queryInfo.ReviewStatus;
                this.SelectedAssignedTo = this.queryInfo.AssignedTo;
                this.SelectedCreatedBy = this.queryInfo.CreatedBy;
                this.SourceRefMatchExpression = this.queryInfo.SourceRefMatchExpression;
            }
        }

        public void Flush()
        {
            if (this.queryInfo != null)
            {
                this.queryInfo.Name = this.Name.Trim();
                this.queryInfo.ReviewStatus = this.ReviewStatus;
                this.queryInfo.AssignedTo = this.SelectedAssignedTo;
                this.queryInfo.CreatedBy = this.SelectedCreatedBy;
                this.queryInfo.SourceRefMatchExpression = this.SourceRefMatchExpression.Trim();
            }
        }

        public object AllReviewStatuses
        {
            get { return Enum.GetValues(typeof(PullRequestQueryReviewStatus)); }
        }
    }
}
