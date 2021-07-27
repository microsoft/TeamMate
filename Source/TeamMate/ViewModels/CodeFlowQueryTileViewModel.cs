using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Services;
using System.ComponentModel.Composition;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class CodeFlowQueryTileViewModel : TileViewModel
    {
        private PullRequestQueryViewModel CodeFlowQuery
        {
            get { return this.Query as PullRequestQueryViewModel; }
        }

        public override void Activate()
        {
            ShowCodeFlowReviewsPage();
        }

        [Import]
        public WindowService WindowService { get; set; }

        private void ShowCodeFlowReviewsPage()
        {
            PullRequestPageViewModel pageViewModel = ViewModelFactory.Create<PullRequestPageViewModel>();
            pageViewModel.Query = this.CodeFlowQuery;
            this.WindowService.NavigateTo(pageViewModel);
        }

        protected override QueryViewModelBase CreateQueryViewModel(TileInfo tileInfo)
        {
            PullRequestQueryViewModel viewModel = ViewModelFactory.Create<PullRequestQueryViewModel>();
            viewModel.ShowNotifications = tileInfo.ShowNotifications;
            viewModel.IncludeInItemCountSummary = tileInfo.IncludeInItemCountSummary;
            viewModel.LastUpdated = tileInfo.LastUpdated;
            viewModel.Name = tileInfo.Name;
            viewModel.QueryInfo = tileInfo.CodeFlowQueryInfo;
            return viewModel;
        }
    }
}
