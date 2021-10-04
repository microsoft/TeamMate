using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Services;
using System.ComponentModel.Composition;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class PullRequestQueryTileViewModel : TileViewModel
    {
        private PullRequestQueryViewModel QueryModel
        {
            get { return this.Query as PullRequestQueryViewModel; }
        }

        public override void Activate()
        {
            ShowPullRequestPage();
        }

        [Import]
        public WindowService WindowService { get; set; }

        private void ShowPullRequestPage()
        {
            PullRequestPageViewModel pageViewModel = ViewModelFactory.Create<PullRequestPageViewModel>();
            pageViewModel.Query = this.QueryModel;
            pageViewModel.TileInfo = this.TileInfo;
            this.WindowService.NavigateTo(pageViewModel);
        }

        protected override QueryViewModelBase CreateQueryViewModel(TileInfo tileInfo)
        {
            PullRequestQueryViewModel viewModel = ViewModelFactory.Create<PullRequestQueryViewModel>();
            viewModel.ShowNotifications = tileInfo.ShowNotifications;
            viewModel.IncludeInItemCountSummary = tileInfo.IncludeInItemCountSummary;
            viewModel.LastUpdated = tileInfo.LastUpdated;
            viewModel.Name = tileInfo.Name;
            viewModel.QueryInfo = tileInfo.PullRequestQueryInfo;
            return viewModel;
        }
    }
}
