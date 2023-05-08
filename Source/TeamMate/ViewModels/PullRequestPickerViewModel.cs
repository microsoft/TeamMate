using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Validation;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Services;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using static Microsoft.TeamFoundation.Client.CommandLine.Options;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class PullRequestPickerViewModel : ValidatableViewModelBase
    {
        private string name;
        private PullRequestQueryInfo queryInfo;
        private PullRequestQueryReviewStatus reviewStatus;

        private string _selectedProject;
        private ObservableCollection<string> _project = new ObservableCollection<string>()
        {
        };

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
        public IEnumerable Project
        {
            get { return this._project; }
        }
        public void AddProject(string projectName)
        {
            this._project.Add(projectName);
        }
        public string SelectedProject
        {
            get { return this._selectedProject; }
            set
            {
                this._selectedProject = value;
                OnPropertyChanged("SelectedProject");
            }
        }
        public string NewProject
        {
            set
            {
                if (SelectedProject != null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    this._project.Add(value);
                    SelectedProject = value;
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
                this.SelectedProject = this.queryInfo.Project;
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
                this.queryInfo.Project = this.SelectedProject;
            }
        }

        public object AllReviewStatuses
        {
            get { return Enum.GetValues(typeof(PullRequestQueryReviewStatus)); }
        }
    }
}
