using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Foundation.Windows.Transfer;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class PullRequestPageViewModel : PageViewModelBase, ICommandProvider, IFilterable, IGlobalCommandProvider
    {
        private PullRequestQueryViewModel query;
        private List<PullRequestRowViewModel> modelList;
        private ListCollectionView collectionView;
        private ListViewModel model;
        private TileInfo tileInfo;

        public PullRequestPageViewModel()
        {
            this.CommandBarType = CommandBarType.CodeReviews;
            this.modelList = new List<PullRequestRowViewModel>();
            this.collectionView = new ListCollectionView(this.modelList);

            this.model = CreateListViewModel(this.collectionView);
            this.model.FilterApplied += HandleFilterApplied;

            this.GlobalCommandBindings = new CommandBindingCollection();
            this.GlobalCommandBindings.Add(TeamMateCommands.Refresh, Refresh);
            this.GlobalCommandBindings.Add(TeamMateCommands.MarkAllAsRead, MarkAllAsRead);

            this.model.OrderByFieldChanged += HandleOrderByFieldPropertyChanged;
            this.model.FilterByFieldChanged += HandleFilterByFieldPropertyChanged;

            foreach (var filter in this.model.Filters)
            {
                filter.PropertyChanged += HandleFilterByFieldPropertyChanged;
            }
        }

        private ListViewModel CreateListViewModel(ICollectionView collectionView)
        {
            ListViewModel model = new ListViewModel(collectionView);

            model.DefaultSortDescription = new SortDescription("ChangedDate", ListSortDirection.Descending);

            model.Filters.Add(new ListViewFilter("All"));
            model.Filters.Add(new ListViewFilter("Unread", (o) => !((PullRequestRowViewModel)o).IsRead));

            var actionableFilter = new ListViewFilter("Assigned To Me", (o) => ((PullRequestRowViewModel)o).IsAssignedToMe);
            model.Filters.Add(actionableFilter);
            model.Filters.Add(new ListViewFilter("Pending", (o) => ((PullRequestRowViewModel)o).IsPending));
            model.Filters.Add(new ListViewFilter("Waiting", (o) => ((PullRequestRowViewModel)o).IsWaiting));
            model.Filters.Add(new ListViewFilter("Signed Off", (o) => ((PullRequestRowViewModel)o).IsSignedOff));
            model.Filters.Add(new ListViewFilter("Not Signed Off By Me", (o) => !((PullRequestRowViewModel)o).IsSignedOffByMe));
            model.Filters.Add(new ListViewFilter("Completed", (o) => ((PullRequestRowViewModel)o).IsCompleted));

            model.Fields.Add(ListFieldInfo.Create<string>("CreatedBy", "Created By"));

            var createdDate = ListFieldInfo.Create<DateTime>("CreatedDate", "Created Date");
            model.Fields.Add(createdDate);
            model.Fields.Add(ListFieldInfo.Create<DateTime>("ChangedDate", "Last Updated"));
            model.Fields.Add(ListFieldInfo.Create<string>("ProjectName", "Project"));

            actionableFilter.IsSelected = true;
            model.OrderBy(createdDate);
            model.ShowInGroups = true;

            return model;
        }

        private void HandleOrderByFieldPropertyChanged(object sender, EventArgs e)
        {
            this.tileInfo.OrderByFieldName = this.model.OrderByField.PropertyName;
            this.tileInfo.FireChanged();
        }

        private void HandleFilterByFieldPropertyChanged(object sender, EventArgs e)
        {
            ListViewFilter filter = (ListViewFilter)sender;
            if (filter.IsSelected)
            {
                this.tileInfo.FilterByFieldName = filter.Name;
                this.tileInfo.FireChanged();
            }
        }

        public TileInfo TileInfo
        {
            get { return this.tileInfo; }
            set
            {
                if (SetProperty(ref this.tileInfo, value))
                {
                    this.model.OrderByFieldName(this.TileInfo.OrderByFieldName);
                    this.model.FilterByFieldName(this.TileInfo.FilterByFieldName);
                }
            }
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
            this.model.SearchFilter = searchFilter;

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
            get { return this.model; }
        }

        public CommandBindingCollection GlobalCommandBindings
        {
            get; private set;
        }

        private void InvalidateCollectionView()
        {
            this.collectionView.Dispatcher.InvokeHere(delegate ()
            {
                this.modelList.Clear();
                if (this.query != null && this.query.PullRequests != null)
                {
                    this.modelList.AddRange(this.query.PullRequests);
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
            commands.Add(TeamMateCommands.OpenPullRequestInWeb, OpenPullRequestInWeb, HasSelection);
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
            var allReviews = this.collectionView.OfType<PullRequestRowViewModel>().ToArray();
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

        public void OpenMany(ICollection<PullRequestRowViewModel> items)
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

        private void OpenPullRequestInWeb()
        {
            var items = GetSelectedItems();
            if (this.WindowService.PromptShouldOpen(this, items.Count))
            {
                using (this.StatusService.BusyIndicator())
                {
                    foreach (PullRequestRowViewModel codeReview in items)
                    {
                        codeReview.OpenInWebBrowser();
                    }
                }
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

        private PullRequestRowViewModel GetSelectedItem()
        {
            return (PullRequestRowViewModel)Reviews.SelectedItems.FirstOrDefault();
        }

        private ICollection<PullRequestRowViewModel> GetSelectedItems()
        {
            return Reviews.SelectedItems.OfType<PullRequestRowViewModel>().ToArray();
        }

        public override void OnNavigatingTo()
        {
            ApplyTextFilter(null);
        }
    }
}
