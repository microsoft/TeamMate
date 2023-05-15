using Microsoft.Tools.TeamMate.Foundation.Threading;
using Microsoft.Tools.TeamMate.Foundation.Validation;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Services;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class PullRequestPickerViewModel : ValidatableViewModelBase
    {
        private string name;
        private PullRequestQueryInfo queryInfo;
        private PullRequestQueryReviewStatus reviewStatus;
        private PullRequestQueryFilter filter;

        private string _selectedProject;
        private ObservableCollection<string> _project = new ObservableCollection<string>()
        {
        };

        private string assignedTo;

        private string createdBy;

        [Import]
        public ResolverService ResolverService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        [Import]
        public SettingsService SettingsService { get; set; }

        public PullRequestPickerViewModel()
        {
            Validator
                .RuleForProperty(() => Name)
                    .IsNotEmpty();

            Validator
                .RuleForProperty(() => SelectedProject)
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
        public string UIAssignedTo
        {
            get { return this.assignedTo; }
            set { SetProperty(ref this.assignedTo, value); }
        }

        public string UICreatedBy
        {
            get { return this.createdBy; }
            set { SetProperty(ref this.createdBy, value); }
        }

        public PullRequestQueryFilter Filter
        {
            get { return this.filter; }
            set { SetProperty(ref this.filter, value); }
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

        private void Invalidate()
        {
            if (this.queryInfo != null)
            {
                this.Name = this.queryInfo.Name;
                this.ReviewStatus = this.queryInfo.ReviewStatus;
                this.UIAssignedTo = this.queryInfo.UIAssignedTo;
                this.UICreatedBy = this.queryInfo.UICreatedBy;
                this.Filter = this.queryInfo.Filter;
            }

            var projects = this.SettingsService.Settings.Projects;
            if (projects.Count > 0)
            {
                foreach (var project in projects)
                {
                    // Today we can only handle Projects within the same project collection.
                    if (this.SessionService.Session.ProjectContext.ProjectInfo.ProjectCollectionUri == project.ProjectCollectionUri)
                    {
                        AddProject(project.ProjectName);
                    }
                }
            }
        }

        public void Flush()
        {
            if (this.queryInfo != null)
            {
                this.queryInfo.Name = this.Name.Trim();
                this.queryInfo.ReviewStatus = this.ReviewStatus;
                this.queryInfo.Project = this.SelectedProject.Trim();
                this.queryInfo.UIAssignedTo = this.UIAssignedTo;
                this.queryInfo.UICreatedBy = this.UICreatedBy;
                this.queryInfo.Filter = this.Filter;
            }
        }

        public object AllReviewStatuses
        {
            get { return Enum.GetValues(typeof(PullRequestQueryReviewStatus)); }
        }

        public object AllFilters
        {
            get { return Enum.GetValues(typeof(PullRequestQueryFilter)); }
        }
    }
}
