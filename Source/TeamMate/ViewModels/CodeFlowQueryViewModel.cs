using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Tools.TeamMate.Foundation.Threading;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Platform.CodeFlow;
using Microsoft.Tools.TeamMate.Platform.CodeFlow.Dashboard;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class CodeFlowQueryViewModel : QueryViewModelBase
    {
        private CodeFlowQueryInfo queryInfo;

        public CodeFlowQueryInfo QueryInfo
        {
            get { return this.queryInfo; }
            set { SetProperty(ref this.queryInfo, value); }
        }

        private ICollection<CodeFlowReviewViewModel> reviews;

        public ICollection<CodeFlowReviewViewModel> Reviews
        {
            get { return this.reviews; }
            set
            {
                var oldReviews = this.reviews;
                if (SetProperty(ref this.reviews, value))
                {
                    if (oldReviews != null)
                    {
                        foreach (var item in oldReviews)
                        {
                            item.PropertyChanged -= HandleReviewPropertyChanged;
                        }
                    }

                    if (this.reviews != null)
                    {
                        foreach (var item in this.reviews)
                        {
                            item.PropertyChanged += HandleReviewPropertyChanged;
                        }
                    }
                }
            }
        }

        private void HandleReviewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
        public CodeFlowService CodeFlowService { get; set; }


        private async Task DoRefreshAsync(NotificationScope notificationScope)
        {
            TaskContext taskContext = new TaskContext();
            taskContext.ReportsProgress = false;

            using (this.ProgressContext = taskContext)
            {
                try
                {
                    // TODO: Refactor to shared stuff
                    var query = QueryInfo.CreateCodeReviewQuery();
                    query.MaxResults = 500;
                    query.UserAgent = TeamMateApplicationInfo.ApplicationName;

                    await ChaosMonkey.ChaosAsync(ChaosScenarios.CodeFlowQueryExecution);

                    CodeFlowClient client = await this.CodeFlowService.GetCodeFlowClientAsync();

                    DateTime queryStartTime = DateTime.Now;
                    QueryReviewSummariesResult result = await client.ReviewDashboardServiceClient.QueryAllReviewSummariesAsync(query);

                    OnQueryCompleted(queryStartTime, result, notificationScope);
                }
                catch (Exception e)
                {
                    this.ItemCount = 0;
                    this.Reviews = null;
                    taskContext.Fail(e);
                }
                finally
                {
                    FireQueryExecuted();
                }
            }
        }

        [Import]
        public SettingsService SettingsService { get; set; }


        private void OnQueryCompleted(DateTime queryStartTime, QueryReviewSummariesResult queryResult, NotificationScope notificationScope)
        {
            DateTime? previousUpdate = this.LastUpdated;
            this.LastUpdated = queryStartTime;

            var reviews = queryResult.Reviews;
            this.Reviews = (reviews != null) ? reviews.Select(r => CreateViewModel(r)).ToArray() : null;
            this.ItemCount = (reviews != null) ? reviews.Length : 0;
            InvalidateUnreadItemCount();

            if (ShowNotifications && this.Reviews != null)
            {
                IEnumerable<CodeFlowReviewViewModel> modifiedItems;

                if (this.SettingsService.DeveloperSettings.DebugAllNotifications)
                {
                    modifiedItems = this.Reviews;
                }
                else
                {
                    modifiedItems = this.Reviews.Where(review => ShouldNotify(review, previousUpdate)).ToArray();
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
            this.UnreadItemCount = (Reviews != null) ? Reviews.Count(review => !review.IsRead) : 0;
        }

        private static bool ShouldNotify(CodeFlowReviewViewModel review, DateTime? previousUpdate)
        {
            var lastChange = review.Summary.GetLastChange();
            return lastChange.ChangeDate.IsAfter(previousUpdate) && !review.IsRead;
        }

        private CodeFlowReviewViewModel CreateViewModel(CodeReviewSummary summary)
        {
            CodeFlowReviewViewModel viewModel = ViewModelFactory.Create<CodeFlowReviewViewModel>();
            viewModel.Summary = summary;
            return viewModel;
        }
    }
}
