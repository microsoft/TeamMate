// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Threading;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Office.Outlook;
using Microsoft.Tools.TeamMate.Platform.CodeFlow;
using Microsoft.Tools.TeamMate.Platform.CodeFlow.Dashboard;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class SearchPageViewModel : PageViewModelBase, ICommandProvider
    {
        // TODO: Nasty nasty duplication here! There's duplication with the CodeFlowsReviewsPageViewModel and
        // WorkItemsPageViewModel because we were lazy! Remove the duplication.
        private string searchText;

        private List<SearchResult> resultList;
        private ListCollectionView collectionView;
        private ListViewModel listViewModel;

        public event EventHandler SearchStarted;
        public event EventHandler SearchCompleted;
        public event EventHandler SearchResultsChanged;

        public SearchPageViewModel()
        {
            this.resultList = new List<SearchResult>();
            this.collectionView = new ListCollectionView(this.resultList);
            this.listViewModel = CreateListViewModel(this.collectionView);

            this.InvalidateTitle();
        }

        [Import]
        public SearchService SearchService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        public HyperlinkFactory HyperlinkFactory => this.SessionService.Session.ProjectContext.HyperlinkFactory;

        public void RegisterBindings(CommandBindingCollection commands)
        {
            // Work Items
            commands.Add(TeamMateCommands.CopyId, CopyId, HasSingleSelection);
            commands.Add(TeamMateCommands.CopyTitle, CopyTitle, HasSingleSelection);
            commands.Add(ApplicationCommands.Copy, CopyHyperlink, HasSingleSelection);
            commands.Add(TeamMateCommands.Flag, ToggleSelectionFlag, HasSelection);
            commands.Add(TeamMateCommands.ReplyWithEmail, ReplyWithEmail, HasSelection);

            // Code Reviews
            commands.Add(TeamMateCommands.OpenReviewInCodeFlow, OpenReviewInCodeFlow, HasSelection);
            commands.Add(TeamMateCommands.OpenReviewInVisualStudio, OpenReviewInVisualStudio, HasSelection);
            commands.Add(TeamMateCommands.OpenReviewInWeb, OpenReviewInWeb, HasSelection);
            commands.Add(TeamMateCommands.SendReviewReminderMail, SendReviewReminderMail, HasSelection);
            commands.Add(TeamMateCommands.PingReviewers, PingReviewers, HasSelection);

            // Shared
            commands.Add(TeamMateCommands.CopyHyperlink, CopyHyperlink, HasSingleSelection);
            commands.Add(TeamMateCommands.MarkAsRead, MarkAsRead, HasSelection);
            commands.Add(TeamMateCommands.MarkAsUnread, MarkAsUnread, HasSelection);
        }

        // TODO: Duplicated commands, should simplify
        private bool HasSelection()
        {
            return listViewModel.HasSelectedItems;
        }

        private bool HasSingleSelection()
        {
            return listViewModel.HasSingleSelectedItem;
        }

        private void CopyId()
        {
            WorkItemRowViewModel workItem = GetSelectedItem<WorkItemRowViewModel>();
            Clipboard.SetDataObject(DataObjectFactory.CopyId(workItem.WorkItem));
        }

        private void CopyTitle()
        {
            WorkItemRowViewModel workItem = GetSelectedItem<WorkItemRowViewModel>();
            Clipboard.SetDataObject(DataObjectFactory.CopyTitle(workItem.WorkItem));
        }


        private void CopyHyperlink()
        {
            object item = GetSelectedItem<object>();
            WorkItemRowViewModel workItem = item as WorkItemRowViewModel;
            CodeFlowReviewViewModel review = item as CodeFlowReviewViewModel;

            if (workItem != null)
            {
                var factory = this.SessionService.Session.ProjectContext.HyperlinkFactory;
                Clipboard.SetDataObject(DataObjectFactory.CopyHyperlink(workItem.WorkItem, factory));
            }
            else if (review != null)
            {
                Clipboard.SetDataObject(DataObjectFactory.CopyHyperlink(review.Summary));
            }
        }

        [Import]
        public CollaborationService CollaborationService { get; set; }

        private void ToggleSelectionFlag()
        {
            // TODO: Should do this in bulk to prevent many updates to the service files
            var selectedItems = GetSelectedItems<WorkItemRowViewModel>();
            foreach (WorkItemRowViewModel workItem in selectedItems)
            {
                workItem.IsFlagged = !workItem.IsFlagged;
            }
        }

        [Import]
        public StatusService StatusService { get; set; }


        private async void ReplyWithEmail()
        {
            try
            {
                using (this.StatusService.BusyIndicator())
                {
                    if (HasSingleSelection())
                    {
                        WorkItemRowViewModel workItem = GetSelectedItem<WorkItemRowViewModel>();
                        await this.CollaborationService.ReplyWithMailAsync(workItem.WorkItem);
                    }
                    else
                    {
                        var items = GetSelectedItems<WorkItemRowViewModel>().Select(wii => wii.WorkItem).ToArray();
                        this.CollaborationService.ReplyAllWithMail(items);
                    }
                }
            }
            catch (Exception e)
            {
                this.MessageBoxService.ShowError(e);
            }
        }

        [Import]
        public WindowService WindowService { get; set; }

        private void OpenReviewInCodeFlow()
        {
            ICollection<CodeFlowReviewViewModel> items = GetSelectedItems<CodeFlowReviewViewModel>().ToArray();

            if (this.WindowService.PromptShouldOpen(this, items.Count))
            {
                foreach (var review in items)
                {
                    review.OpenInCodeFlow();
                }
            }
        }


        private void OpenReviewInVisualStudio()
        {
            ICollection<CodeFlowReviewViewModel> items = GetSelectedItems<CodeFlowReviewViewModel>().ToArray();
            if (this.WindowService.PromptShouldOpen(this, items.Count))
            {
                using (this.StatusService.BusyIndicator())
                {
                    foreach (CodeFlowReviewViewModel codeReview in items)
                    {
                        codeReview.OpenInVisualStudio();
                    }
                }
            }
        }

        private void OpenReviewInWeb()
        {
            ICollection<CodeFlowReviewViewModel> items = GetSelectedItems<CodeFlowReviewViewModel>().ToArray();
            if (this.WindowService.PromptShouldOpen(this, items.Count))
            {
                using (this.StatusService.BusyIndicator())
                {
                    foreach (CodeFlowReviewViewModel codeReview in items)
                    {
                        codeReview.OpenInWebBrowser();
                    }
                }
            }
        }

        private async void SendReviewReminderMail()
        {
            bool result = await DoSendReviewReminderMail(GetSelectedItems<CodeFlowReviewViewModel>());
            if (!result)
            {
                this.MessageBoxService.Show(this, "All of the selected reviews are already completed.");
            }
        }

        private async Task<bool> DoSendReviewReminderMail(IEnumerable<CodeFlowReviewViewModel> reviews)
        {
            // Email authors to complete signed off reviews...

            var signedOffReviews = reviews.Where(r => r.Summary.Status != CodeReviewStatus.Completed).Select(cri => cri.Summary).ToArray();

            if (signedOffReviews.Any())
            {
                MailMessage message = await Task.Run(() => GenerateCompleteReviewsEmail(signedOffReviews));
                this.CollaborationService.SendMail(message);
                return true;
            }

            return false;
        }

        private static MailMessage GenerateCompleteReviewsEmail(IEnumerable<CodeReviewSummary> reviews)
        {
            CodeFlowMailGenerator mailGenerator = new CodeFlowMailGenerator();
            var result = mailGenerator.GenerateCompleteReviewsEmail(reviews);
            result.ReminderDate = DateTime.Now.AddDays(3); // TODO: Hardcoded reminder
            return result;
        }

        private async void PingReviewers()
        {
            // Ping
            // Only for my reviews, not for others...
            var selection = GetSelectedItems<CodeFlowReviewViewModel>().Where(cr => cr.IsOwnedByMe).ToArray();
            if (selection.Any())
            {
                // TODO: Poor coupling of View to ViewModel here
                CodeFlowPingDialog dialog = new CodeFlowPingDialog();
                dialog.Owner = View.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    string message = dialog.Message;
                    await PingReviewers(selection, message);
                }
            }
            else
            {
                this.MessageBoxService.ShowError(this, "You cannot Ping code reviews that are not owned by yourself!");
            }
        }

        [Import]
        public CodeFlowService CodeFlowService { get; set; }

        private async Task PingReviewers(IEnumerable<CodeFlowReviewViewModel> reviews, string pingMessage)
        {
            // TODO: Warning on pinging too many reviews? E.g. more than 5?
            CodeFlowClient client = await this.CodeFlowService.GetCodeFlowClientAsync();

            List<Task> tasks = new List<Task>();
            foreach (CodeFlowReviewViewModel review in reviews)
            {
                var task = client.ReviewServiceClient.AuthorPingAsync(review.Summary.Key, pingMessage);
                tasks.Add(task);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                this.MessageBoxService.ShowError(this, "An error occurred attempting to ping reviewers");
                Log.Warn(ex);
            }
        }


        private void MarkAsRead()
        {
            // TODO: Should do this in bulk to prevent many updates to the service files
            foreach (TrackableViewModelBase trackable in GetSelectedItems<TrackableViewModelBase>())
            {
                trackable.IsRead = true;
            }
        }

        private void MarkAsUnread()
        {
            // TODO: Should do this in bulk to prevent many updates to the service files
            foreach (TrackableViewModelBase trackable in GetSelectedItems<TrackableViewModelBase>())
            {
                trackable.IsRead = false;
            }
        }

        private TResult GetSelectedItem<TResult>() where TResult : class
        {
            return ((SearchResult)listViewModel.SingleSelectedItem).Item as TResult;
        }

        private IEnumerable<TResult> GetSelectedItems<TResult>()
        {
            return this.listViewModel.SelectedItems.OfType<SearchResult>().Select(sr => sr.Item).OfType<TResult>();
        }

        public ListViewModel ListViewModel
        {
            get { return this.listViewModel; }
        }

        private static ListViewModel CreateListViewModel(ICollectionView collectionView)
        {
            ListViewModel listViewModel = new ListViewModel(collectionView);
            listViewModel.IsGroupByVisible = false;

            listViewModel.DefaultSortDescription = new SortDescription("Item.ChangedDate", ListSortDirection.Descending);

            var sourceField = ListFieldInfo.Create<object>("Source", "Source");
            listViewModel.Fields.Add(sourceField);
            listViewModel.OrderBy(sourceField);
            listViewModel.ShowInGroups = true;

            listViewModel.Filters.Add(new ListViewFilter("All"));
            listViewModel.Filters.Add(new ListViewFilter("Work Items", (o) => ((SearchResult)o).Item is WorkItemRowViewModel));
            listViewModel.Filters.Add(new ListViewFilter("Code Reviews", (o) => ((SearchResult)o).Item is CodeFlowReviewViewModel));
            listViewModel.Filters.Add(new ListViewFilter("Local Only", (o) => ((SearchResult)o).Source.IsLocal));

            return listViewModel;
        }

        private TaskContext progressContext;

        public void ShowErrorDetails()
        {
            var progressContext = this.ProgressContext;
            if (progressContext != null && progressContext.Error != null)
            {
                this.MessageBoxService.ShowError(progressContext.Error);
            }
        }

        [Import]
        public MessageBoxService MessageBoxService { get; set; }


        public TaskContext ProgressContext
        {
            get { return this.progressContext; }
            set { SetProperty(ref this.progressContext, value); }
        }

        private int totalCount;

        public int TotalCount
        {
            get { return this.totalCount; }
            set { SetProperty(ref this.totalCount, value); }
        }

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                if (SetProperty(ref this.searchText, value))
                {
                    InvalidateTitle();
                    InvalidateSearch();
                }
            }
        }

        private SearchExpression searchExpression;

        public SearchExpression SearchExpression
        {
            get { return this.searchExpression; }
            set { SetProperty(ref this.searchExpression, value); }
        }

        public ICollectionView SearchResults
        {
            get { return this.collectionView; }
        }

        private async void InvalidateSearch()
        {
            this.SearchExpression = SearchExpression.Parse(this.searchText);
            this.TotalCount = 0;
            this.resultList.Clear();
            this.collectionView.Refresh();

            var searchService = this.SearchService;
            if (searchService == null)
            {
                return;
            }

            var projectContext = this.SessionService.Session.ProjectContext;
            if (projectContext == null)
            {
                return;
            }

            SearchStarted?.Invoke(this, EventArgs.Empty);

            TaskContext taskContext = new TaskContext();
            taskContext.ReportsProgress = false;

            using (this.ProgressContext = taskContext)
            {
                var searchTasks = new List<Task<SearchResults>>();

                var localSearchTask = searchService.LocalSearch(this.SearchExpression, CancellationToken.None);
                searchTasks.Add(localSearchTask);

                var tfsSearchTask = searchService.TfsSearch(this.SearchExpression, CancellationToken.None);
                searchTasks.Add(tfsSearchTask);
                taskContext.Status = "Searching...";

                while (searchTasks.Any())
                {
                    var readyTask = await Task.WhenAny(searchTasks);
                    searchTasks.Remove(readyTask);

                    try
                    {
                        var results = readyTask.Result;
                        AppendResults(results.Results);
                        this.TotalCount += results.TotalCount;

                        InvalidateTitle();
                    }
                    catch (Exception ex)
                    {
                        if (!taskContext.IsFailed)
                        {
                            taskContext.Fail("An error occurred while performing the search", ex);
                        }

                        Log.Warn(ex);
                    }
                }
            }

            SearchCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void InvalidateTitle()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Search results");

            if (this.searchText != null)
            {
                sb.AppendFormat(" for \"{0}\"", this.searchText);

                int itemCount = this.resultList.Count;
                if (itemCount > 0)
                {
                    if (itemCount == TotalCount)
                    {
                        sb.AppendFormat(" ({0})", itemCount);
                    }
                    else
                    {
                        sb.AppendFormat(" ({0} of {1})", itemCount, TotalCount);
                    }
                }
            }

            this.Title = sb.ToString();
        }

        private void AppendResults(IList<SearchResult> results)
        {
            if (results.Any())
            {
                this.resultList.AddRange(results);
                this.collectionView.Refresh();

                SearchResultsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void OpenMany(ICollection<SearchResult> items)
        {
            if (this.WindowService.PromptShouldOpen(this, items.Count))
            {
                using (this.StatusService.BusyIndicator())
                {
                    foreach (SearchResult searchResult in items)
                    {
                        WorkItemRowViewModel workItem = searchResult.Item as WorkItemRowViewModel;
                        CodeFlowReviewViewModel review = searchResult.Item as CodeFlowReviewViewModel;
                        if (workItem != null)
                        {
                            workItem.Open();
                        }
                        else if (review != null)
                        {
                            review.OpenInCodeFlow();
                        }
                        else
                        {
                            Debug.Fail("Unhandled item of type: " + searchResult.Item.GetType().FullName);
                        }
                    }
                }
            }
        }
    }
}
