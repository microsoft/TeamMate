using Microsoft.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Tools.TeamMate.Foundation.Threading;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.PullRequests;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class PullRequestQueryViewModel : QueryViewModelBase
    {
        private PullRequestQueryInfo queryInfo;

        public PullRequestQueryInfo QueryInfo
        {
            get { return this.queryInfo; }
            set { SetProperty(ref this.queryInfo, value); }
        }

        private ICollection<PullRequestRowViewModel> pullRequests;

        public ICollection<PullRequestRowViewModel> PullRequests
        {
            get { return this.pullRequests; }
            set
            {
                var oldPullRequests = this.pullRequests;
                if (SetProperty(ref this.pullRequests, value))
                {
                    if (oldPullRequests != null)
                    {
                        foreach (var item in oldPullRequests)
                        {
                            item.PropertyChanged -= HandlePullRequestPropertyChanged;
                        }
                    }

                    if (this.pullRequests != null)
                    {
                        foreach (var item in this.pullRequests)
                        {
                            item.PropertyChanged += HandlePullRequestPropertyChanged;
                        }
                    }
                }
            }
        }

        private void HandlePullRequestPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRead")
            {
                InvalidateUnreadItemCount();
            }
        }

        private SingleTaskRunner sharedRefreshTask = new SingleTaskRunner();

        public override Task RefreshAsync(NotificationScope notificationScope = null)
        {
            return this.sharedRefreshTask.Run(() => DoRefreshAsync(notificationScope));
        }

        [Import]
        public SessionService SessionService { get; set; }
 
        public ProjectInfo ProjectInfo { get; private set; }

        private async Task DoRefreshAsync(NotificationScope notificationScope)
        {
            var projectContext = this.SessionService.Session.ProjectContext;

            TaskContext taskContext = new TaskContext();
            taskContext.ReportsProgress = false;

            using (this.ProgressContext = taskContext)
            {
                try
                {
                    FireQueryExecuting();

                    PullRequestQuery query = CreateBuiltInQuery();

                    if (query != null)
                    {
                        await ChaosMonkey.ChaosAsync(ChaosScenarios.PullRequestQueryExecution);

                        List<Task> tasks = new List<Task>();

                        var queryAsyncTask = projectContext.GitHttpClient.GetPullRequestsByProjectAsync(
                            projectContext.ProjectInfo.ProjectName,
                            query.GitPullRequestSearchCriteria);

                        tasks.Add(queryAsyncTask);

                        await Task.WhenAll(tasks.ToArray());

                        List<Task<List<GitPullRequestIteration>>> iterationTasks = new List<Task<List<GitPullRequestIteration>>>();

                        var pullRequests =
                            queryAsyncTask.Result
                            .Where(r => query.MatchesSourceRef(r.SourceRefName))
                            .Select(r => CreateViewModel(r, projectContext)).ToArray();

                        foreach (var pullRequest in pullRequests)
                        {
                            var asyncTask = projectContext.GitHttpClient.GetPullRequestIterationsAsync(
                                pullRequest.Reference.Repository.Id,
                                pullRequest.Reference.PullRequestId);

                            iterationTasks.Add(asyncTask);

                            pullRequest.Url = projectContext.HyperlinkFactory.GetPullRequestUrl(
                                pullRequest.Reference.PullRequestId,
                                pullRequest.Reference.Repository.Name);
                        }

                        await Task.WhenAll(iterationTasks.ToArray());

                        int i = 0;
                        foreach (var pullRequest in pullRequests)
                        {
                            pullRequest.Iterations = iterationTasks[i++].Result;
                        }

                        OnQueryCompleted(projectContext, pullRequests, notificationScope);
                    }
                    else
                    {
                        ItemCount = 0;
                        ProjectInfo = null;
                        PullRequests = new List<PullRequestRowViewModel>();
                        InvalidateUnreadItemCount();
                    }
                }
                catch (Exception e)
                {
                    ItemCount = 0;
                    ProjectInfo = null;
                    PullRequests = null;
                    InvalidateUnreadItemCount();
                    taskContext.Fail(e);
                }
                finally
                {
                    FireQueryExecuted();
                }
            }
        }

        private PullRequestQuery CreateBuiltInQuery()
        {
            var pc = this.SessionService.Session.ProjectContext;
          
            var query = new PullRequestQuery();
            query.ProjectName = pc.ProjectName;
            query.GitPullRequestSearchCriteria = new GitPullRequestSearchCriteria
            {
                Status = PullRequestQueryInfo.ReviewStatusesMap[this.queryInfo.ReviewStatus],
            };

            if (this.queryInfo.AssignedTo == "@me")
            {
                query.GitPullRequestSearchCriteria.ReviewerId = pc.Identity.Id;
            }

            if (this.queryInfo.CreatedBy == "@me")
            {
                query.GitPullRequestSearchCriteria.CreatorId = pc.Identity.Id;
            }

            query.SourceRefMatchExpression = this.queryInfo.SourceRefMatchExpression;

            return query;
        }

        [Import]
        public SettingsService SettingsService { get; set; }

        private void OnQueryCompleted(ProjectContext projectContext, PullRequestRowViewModel[] pullRequests, NotificationScope notificationScope)
        {
            DateTime? previousUpdate = this.LastUpdated;
            this.LastUpdated = DateTime.Now;

            this.PullRequests = pullRequests;
            this.ItemCount = pullRequests.Length;
            InvalidateUnreadItemCount();

            if (ShowNotifications && this.PullRequests != null)
            {
                IEnumerable<PullRequestRowViewModel> modifiedItems;

                if (this.SettingsService.DeveloperSettings.DebugAllNotifications)
                {
                    modifiedItems = this.PullRequests;
                }
                else
                {
                    modifiedItems = this.PullRequests.Where(review => ShouldNotify(review, previousUpdate)).ToArray();
                }

                if (modifiedItems.Any())
                {
                    this.ToastNotificationService.QueueNotifications(modifiedItems, previousUpdate, notificationScope);
                }
            }
        }

        [Import]
        public ToastNotificationService ToastNotificationService { get; set; }

        private void InvalidateUnreadItemCount()
        {
            this.UnreadItemCount = (PullRequests != null) ? PullRequests.Count(review => !review.IsRead) : 0;
        }

        private static bool ShouldNotify(PullRequestRowViewModel pullRequest, DateTime? previousUpdate)
        {
            return (pullRequest.ChangedDate > previousUpdate) && !pullRequest.IsRead;
        }

        private PullRequestRowViewModel CreateViewModel(GitPullRequest gitPullRequest, ProjectContext projectContext)
        {
            PullRequestRowViewModel viewModel = ViewModelFactory.Create<PullRequestRowViewModel>();
            viewModel.IdentityRef = projectContext.Identity.Id.ToString();
            viewModel.Reference = gitPullRequest;
            viewModel.ProjectName = projectContext.ProjectName;
            return viewModel;
        }
    }
}
