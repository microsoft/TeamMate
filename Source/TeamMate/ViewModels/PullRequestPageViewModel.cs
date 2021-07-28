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
    public class PullRequestPageViewModel : PageViewModelBase, ICommandProvider, IFilterable, IGlobalCommandProvider
    {
        private PullRequestQueryViewModel query;
        private List<PullRequestViewModel> reviewList;
        private ListCollectionView collectionView;
        private ListViewModel reviews;

        public PullRequestPageViewModel()
        {
            this.CommandBarType = CommandBarType.CodeReviews;
            this.reviewList = new List<PullRequestViewModel>();
            this.collectionView = new ListCollectionView(this.reviewList);

            this.reviews = CreateListViewModel(this.collectionView);
            this.reviews.FilterApplied += HandleFilterApplied;

            this.GlobalCommandBindings = new CommandBindingCollection();
            this.GlobalCommandBindings.Add(TeamMateCommands.NewCodeFlowReview, NewCodeFlowReview);
            this.GlobalCommandBindings.Add(TeamMateCommands.Refresh, Refresh);
            this.GlobalCommandBindings.Add(TeamMateCommands.MarkAllAsRead, MarkAllAsRead);
        }

        private ListViewModel CreateListViewModel(ICollectionView collectionView)
        {
            ListViewModel model = new ListViewModel(collectionView);

            model.DefaultSortDescription = new SortDescription("Summary.LastUpdatedOn", ListSortDirection.Descending);

            model.Filters.Add(new ListViewFilter("All"));
            model.Filters.Add(new ListViewFilter("Unread", (o) => !((PullRequestViewModel)o).IsRead));

            var actionableFilter = new ListViewFilter("Assigned To Me", (o) => ((PullRequestViewModel)o).IsAssignedToMe);
            model.Filters.Add(actionableFilter);
            model.Filters.Add(new ListViewFilter("Pending", (o) => ((PullRequestViewModel)o).IsPending));
            model.Filters.Add(new ListViewFilter("Waiting", (o) => ((PullRequestViewModel)o).IsWaiting));
            model.Filters.Add(new ListViewFilter("Signed Off", (o) => ((PullRequestViewModel)o).IsSignedOff));
            model.Filters.Add(new ListViewFilter("Not Signed Off By Me", (o) => !((PullRequestViewModel)o).IsSignedOffByMe));
            model.Filters.Add(new ListViewFilter("Completed", (o) => ((PullRequestViewModel)o).IsCompleted));

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

        public PullRequestQueryViewModel Query
        {
            get { return this.query; }
            set
            {
                PullRequestQueryViewModel oldQuery = this.query;
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
                if (this.query != null && this.query.PullRequests != null)
                {
                    this.reviewList.AddRange(this.query.PullRequests);
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
            commands.Add(TeamMateCommands.OpenReviewInWeb, OpenReviewInWeb, HasSelection);
            commands.Add(TeamMateCommands.CopyHyperlink, CopyHyperlink, HasSingleSelection);
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
            var allReviews = this.collectionView.OfType<PullRequestViewModel>().ToArray();
            foreach (var review in allReviews)
            {
                review.IsRead = true;
            }
        }

        private void CopyHyperlink()
        {
            var pullRequest = GetSelectedItem();
            if (pullRequest != null)
            {
                DataObject dataObject = new DataObject();
                dataObject.SetUri(pullRequest.Url, pullRequest.GetFullTitle());
                Clipboard.SetDataObject(dataObject);
            }
        }

        [Import]
        public WindowService WindowService { get; set; }

        public void OpenMany(ICollection<PullRequestViewModel> items)
        {
            if (this.WindowService.PromptShouldOpen(this, items.Count))
            {
                using (this.StatusService.BusyIndicator())
                {
                    foreach (var item in items)
                    {
                        item.OpenInWebBrowser();
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

        private void OpenReviewInWeb()
        {
            var items = GetSelectedItems();
            if (this.WindowService.PromptShouldOpen(this, items.Count))
            {
                using (this.StatusService.BusyIndicator())
                {
                    foreach (PullRequestViewModel codeReview in items)
                    {
                        codeReview.OpenInWebBrowser();
                    }
                }
            }
        }

        [Import]
        public CollaborationService CollaborationService { get; set; }

        private bool HasSelection()
        {
            return Reviews.SelectedItems.Any();
        }

        private bool HasSingleSelection()
        {
            return Reviews.SelectedItems.Count == 1;
        }

        private PullRequestViewModel GetSelectedItem()
        {
            return (PullRequestViewModel)Reviews.SelectedItems.FirstOrDefault();
        }

        private ICollection<PullRequestViewModel> GetSelectedItems()
        {
            return Reviews.SelectedItems.OfType<PullRequestViewModel>().ToArray();
        }

        public override void OnNavigatingTo()
        {
            ApplyTextFilter(null);
        }
    }
}
