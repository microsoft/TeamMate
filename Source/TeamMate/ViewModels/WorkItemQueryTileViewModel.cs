using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Services;
using System;
using System.ComponentModel.Composition;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class WorkItemQueryTileViewModel : TileViewModel
    {
        public WorkItemQueryViewModel WorkItemQuery
        {
            get { return this.Query as WorkItemQueryViewModel; }
        }

        public override void Activate()
        {
            ShowWorkItemsPage();
        }


        protected override QueryViewModelBase CreateQueryViewModel(TileInfo tileInfo)
        {
            WorkItemQueryViewModel query;

            if (tileInfo.WorkItemQueryReference != null)
            {
                query = ViewModelFactory.Create<WorkItemQueryViewModel>();
                query.Reference = tileInfo.WorkItemQueryReference;
            }
            else if (tileInfo.Type == TileType.BuiltIn)
            {
                if (tileInfo.BuiltInTileType == BuiltInTileType.Flagged)
                {
                    query = ViewModelFactory.Create<FlaggedWorkItemsViewModel>();
                }
                else
                {
                    query = ViewModelFactory.Create<WorkItemQueryViewModel>();
                    query.BuiltInTileType = tileInfo.BuiltInTileType;
                }
            }
            else
            {
                throw new NotSupportedException("Cannot create WorkItemQueryTileViewModel from tile info: " + tileInfo);
            }

            query.ShowNotifications = tileInfo.ShowNotifications;
            query.IncludeInItemCountSummary = tileInfo.IncludeInItemCountSummary;
            query.LastUpdated = tileInfo.LastUpdated;
            query.Name = tileInfo.Name;

            return query;
        }

        [Import]
        public WindowService WindowService { get; set; }

        private void ShowWorkItemsPage()
        {
            WorkItemsPageViewModel pageViewModel = ViewModelFactory.Create<WorkItemsPageViewModel>();
            pageViewModel.Query = this.WorkItemQuery;
            pageViewModel.TileInfo = this.TileInfo;
            this.WindowService.NavigateTo(pageViewModel);
        }

        protected override void OnQueryExecuted()
        {
            base.OnQueryExecuted();
            this.UpdateTileNameFromQueryName();
        }

        private void UpdateTileNameFromQueryName()
        {
            WorkItemQueryViewModel query = this.Query as WorkItemQueryViewModel;
            if (query != null)
            {
                this.TileInfo.Name = query.Name;
            }
        }
    }
}