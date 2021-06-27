// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Foundation.Windows.Transfer;
using Microsoft.Tools.TeamMate.Office.Outlook;
using Microsoft.Tools.TeamMate.Platform.CodeFlow;
using Microsoft.Tools.TeamMate.Platform.CodeFlow.Dashboard;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class CodeFlowReviewsPageViewModel : PageViewModelBase, ICommandProvider, IFilterable, IGlobalCommandProvider
    {
        private CodeFlowQueryViewModel query;
        private List<CodeFlowReviewViewModel> reviewList;
        private ListCollectionView collectionView;
        private ListViewModel reviews;

        public CodeFlowReviewsPageViewModel()
        {
            this.CommandBarType = CommandBarType.CodeReviews;
            this.reviewList = new List<CodeFlowReviewViewModel>();
            this.collectionView = new ListCollectionView(this.reviewList);

            this.reviews = CreateListViewModel(this.collectionView);
            this.reviews.FilterApplied += HandleFilterApplied;

            this.GlobalCommandBindings = new CommandBindingCollection();
            this.GlobalCommandBindings.Add(TeamMateCommands.NewCodeFlowReview, NewCodeFlowReview);
            this.GlobalCommandBindings.Add(TeamMateCommands.SendAllReviewsReminderMail, SendAllReviewsReminderMail);
            this.GlobalCommandBindings.Add(TeamMateCommands.Refresh, Refresh);
            this.GlobalCommandBindings.Add(TeamMateCommands.MarkAllAsRead, MarkAllAsRead);
        }

        private ListViewModel CreateListViewModel(ICollectionView collectionView)
        {
            ListViewModel model = new ListViewModel(collectionView);

            model.DefaultSortDescription = new SortDescription("Summary.LastUpdatedOn", ListSortDirection.Descending);

            model.Filters.Add(new ListViewFilter("All"));
            model.Filters.Add(new ListViewFilter("Unread", (o) => !((CodeFlowReviewViewModel)o).IsRead));

            var actionableFilter = new ListViewFilter("Actionable", (o) => ((CodeFlowReviewViewModel)o).IsActionableByMe());
            model.Filters.Add(actionableFilter);
            model.Filters.Add(new ListViewFilter("Pending", (o) => ((CodeFlowReviewViewModel)o).IsPending));
            model.Filters.Add(new ListViewFilter("Waiting", (o) => ((CodeFlowReviewViewModel)o).IsWaiting));
            model.Filters.Add(new ListViewFilter("Signed Off", (o) => ((CodeFlowReviewViewModel)o).IsSignedOff));
            model.Filters.Add(new ListViewFilter("Not Signed Off By Me", (o) => !((CodeFlowReviewViewModel)o).IsSignedOffByMe));
            model.Filters.Add(new ListViewFilter("Completed", (o) => ((CodeFlowReviewViewModel)o).IsCompleted));

            model.Fields.Add(ListFieldInfo.Create<string>("Summary.Author.DisplayName", "Created By"));

            var createdDate = ListFieldInfo.Create<DateTime>("Summary.CreatedOn", "Created Date");
            model.Fields.Add(createdDate);
            model.Fields.Add(ListFieldInfo.Create<DateTime>("Summary.LastUpdatedOn", "Last Updated"));
            model.Fields.Add(ListFieldInfo.Create<string>("Summary.ProjectShortName", "Project"));

            actionableFilter.IsSelected = true;
            model.OrderBy(createdDate);
            model.ShowInGroups = true;

            return model;
        }

        private void HandleFilterApplied(object sender, EventArgs e)
        {
            InvalidatePageTitle();
        }

        private string pageTitle;

        public string PageTitle
        {
            get { return this.pageTitle; }
            set { SetProperty(ref this.pageTitle, value); }
        }

        public void ApplyTextFilter(string filterText)
        {
            SearchExpression expression = SearchExpression.Parse(filterText);
            Predicate<object> searchFilter = (!expression.IsEmpty) ? expression.Matches : (Predicate<object>)null;
            this.reviews.SearchFilter = searchFilter;

            if (searchFilter != null)
            {
                TextFilterApplied?.Invoke(this, expression);
            }
        }

        public event EventHandler<SearchExpression> TextFilterApplied;

        public CodeFlowQueryViewModel Query
        {
            get { return this.query; }
            set
            {
                CodeFlowQueryViewModel oldQuery = this.query;
                if (SetProperty(ref this.query, value))
                {
                    if (oldQuery != null)
                    {
                        oldQuery.QueryExecuted -= HandleQueryExecuted;
                    }

                    if (this.query != null)
                    {
                        query.QueryExecuted += HandleQueryExecuted;
                    }

                    InvalidateCollectionView();
                }
            }
        }

        public ListViewModel Reviews
        {
            get { return this.reviews; }
        }

        public CommandBindingCollection GlobalCommandBindings
        {
            get; private set;
        }

        private void InvalidateCollectionView()
        {
            this.collectionView.Dispatcher.InvokeHere(delegate ()
            {
                this.reviewList.Clear();
                if (this.query != null && this.query.Reviews != null)
                {
                    this.reviewList.AddRange(this.query.Reviews);
                }
                this.collectionView.Refresh();

                // IMPORTANT, do this AFTER results are loaded
                InvalidatePageTitle();
            });
        }


        private void InvalidatePageTitle()
        {
            string pageTitle = null;

            if (Query != null)
            {
                if (Query.ItemCount > 0)
                {
                    int displayCount = this.collectionView.Count;
                    if (displayCount < Query.ItemCount)
                    {
                        pageTitle = String.Format("{0} ({1} of {2})", Query.Name, displayCount, Query.ItemCount);
                    }
                    else
                    {
                        pageTitle = String.Format("{0} ({1})", Query.Name, Query.ItemCount);
                    }
                }
                else
                {
                    pageTitle = Query.Name;
                }
            }

            this.PageTitle = pageTitle;
            this.Title = pageTitle;
        }

        private void HandleQueryExecuted(object sender, EventArgs e)
        {
            InvalidateCollectionView();
        }

        public void RegisterBindings(CommandBindingCollection commands)
        {
            commands.Add(TeamMateCommands.OpenReviewInCodeFlow, OpenReviewInCodeFlow, HasSelection);
            commands.Add(TeamMateCommands.OpenReviewInVisualStudio, OpenReviewInVisualStudio, HasSelection);
            commands.Add(TeamMateCommands.OpenReviewInWeb, OpenReviewInWeb, HasSelection);
            commands.Add(TeamMateCommands.CopyHyperlink, CopyHyperlink, HasSingleSelection);
            commands.Add(TeamMateCommands.SendReviewReminderMail, SendReviewReminderMail, HasSelection);
            commands.Add(TeamMateCommands.PingReviewers, PingReviewers, HasSelection);
            commands.Add(TeamMateCommands.CompleteReviews, CompleteReviews, HasSelection);
            commands.Add(TeamMateCommands.MarkAsRead, MarkAsRead, HasSelection);
            commands.Add(TeamMateCommands.MarkAsUnread, MarkAsUnread, HasSelection);
        }

        private void MarkAsUnread()
        {
            foreach (var review in GetSelectedItems())
            {
                review.IsRead = false;
            }
        }

        private void MarkAsRead()
        {
            foreach (var review in GetSelectedItems())
            {
                review.IsRead = true;
            }
        }

        private void MarkAllAsRead()
        {
            var allReviews = this.collectionView.OfType<CodeFlowReviewViewModel>().ToArray();
            foreach (var review in allReviews)
            {
                review.IsRead = true;
            }
        }

        private void CopyHyperlink()
        {
            var review = GetSelectedItem();
            if (review != null)
            {
                DataObject dataObject = new DataObject();
                dataObject.SetUri(review.GetWebViewUri(), review.GetFullTitle());
                Clipboard.SetDataObject(dataObject);
            }
        }

        [Import]
        public WindowService WindowService { get; set; }

        public void OpenMany(ICollection<CodeFlowReviewViewModel> items)
        {
            if (this.WindowService.PromptShouldOpen(this, items.Count))
            {
                using (this.StatusService.BusyIndicator())
                {
                    foreach (var item in items)
                    {
                        item.OpenInCodeFlow();
                    }
                }
            }
        }

        [Import]
        public StatusService StatusService { get; set; }

        [Import]
        public MessageBoxService MessageBoxService { get; set; }

        public async void Refresh()
        {
            if (Query != null)
            {
                await Query.RefreshAsync();
            }
        }

        private void NewCodeFlowReview()
        {
            Process.Start(CodeFlowUriBuilder.LaunchClient().AbsoluteUri);
        }

        private async void SendAllReviewsReminderMail()
        {
            var allReviews = this.collectionView.OfType<CodeFlowReviewViewModel>().ToArray();
            bool result = await DoSendReviewReminderMail(allReviews);
            if (!result)
            {
                this.MessageBoxService.Show(this, "There are no incomplete reviews to send a reminder for.");
            }
        }

        private void OpenReviewInCodeFlow()
        {
            var items = GetSelectedItems();
            if (this.WindowService.PromptShouldOpen(this, items.Count))
            {
                using (this.StatusService.BusyIndicator())
                {
                    foreach (CodeFlowReviewViewModel codeReview in items)
                    {
                        codeReview.OpenInCodeFlow();
                    }
                }
            }
        }

        private void OpenReviewInVisualStudio()
        {
            var items = GetSelectedItems();
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
            var items = GetSelectedItems();
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
            bool result = await DoSendReviewReminderMail(GetSelectedItems());
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

        [Import]
        public CollaborationService CollaborationService { get; set; }


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
            var selection = GetSelectedItems().Where(cr => cr.IsOwnedByMe).ToArray();
            if (selection.Any())
            {
                // TODO: Poor coupling of View to ViewModel here
                CodeFlowPingDialog dialog = new CodeFlowPingDialog();
                dialog.Owner = View.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    string message = dialog.Message;
                    await PingReviewersAsync(selection, message);
                }
            }
            else
            {
                this.MessageBoxService.ShowError(this, "You cannot Ping code reviews that are not owned by yourself!");
            }
        }

        [Import]
        public CodeFlowService CodeFlowService { get; set; }

        private async Task PingReviewersAsync(IEnumerable<CodeFlowReviewViewModel> reviews, string pingMessage)
        {
            try
            {
                // TODO: Warning on pinging too many reviews? E.g. more than 5?
                CodeFlowClient client = await this.CodeFlowService.GetCodeFlowClientAsync();

                List<Task> tasks = new List<Task>();
                foreach (CodeFlowReviewViewModel review in reviews)
                {
                    var task = client.ReviewServiceClient.AuthorPingAsync(review.Summary.Key, pingMessage);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                this.MessageBoxService.ShowError(this, "An error occurred attempting to ping reviewers. Please try again later.");
            }
        }

        private async void CompleteReviews()
        {
            // Only for my reviews, not for others... And only for active reviews.
            var selection = GetSelectedItems().Where(cr => cr.IsOwnedByMe && cr.IsActive).ToArray();
            if (selection.Any())
            {
                await CompleteReviewsAsync(selection);
            }
            else
            {
                this.MessageBoxService.ShowError(this, "You can only complete code reviews that are owned by yourself and have not already been completed.");
            }
        }

        private async Task CompleteReviewsAsync(IEnumerable<CodeFlowReviewViewModel> reviews)
        {
            try
            {
                // TODO: Warning on completing too many reviews? E.g. more than 5?
                CodeFlowClient client = await this.CodeFlowService.GetCodeFlowClientAsync();

                List<Task> tasks = new List<Task>();
                foreach (CodeFlowReviewViewModel review in reviews)
                {
                    var task = client.ReviewServiceClient.CompleteReviewAsync(review.Summary.Key, null);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                this.MessageBoxService.ShowError(this, "An error occurred attempting to complete code reviews. Please try again later.");
            }
        }

        private bool HasSelection()
        {
            return Reviews.SelectedItems.Any();
        }

        private bool HasSingleSelection()
        {
            return Reviews.SelectedItems.Count == 1;
        }

        private CodeFlowReviewViewModel GetSelectedItem()
        {
            return (CodeFlowReviewViewModel)Reviews.SelectedItems.FirstOrDefault();
        }

        private ICollection<CodeFlowReviewViewModel> GetSelectedItems()
        {
            return Reviews.SelectedItems.OfType<CodeFlowReviewViewModel>().ToArray();
        }

        public override void OnNavigatingTo()
        {
            ApplyTextFilter(null);
        }
    }
}
