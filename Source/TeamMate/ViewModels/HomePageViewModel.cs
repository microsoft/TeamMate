using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.Services;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class HomePageViewModel : PageViewModelBase, ICommandProvider, IGlobalCommandProvider
    {
        private Session session;
        private TileCollectionViewModel tileCollection;

        public HomePageViewModel()
        {
            this.CommandBarType = CommandBarType.Home;
            this.Title = "Home";
            this.tileCollection = ViewModelFactory.Create<TileCollectionViewModel>();

            this.GlobalCommandBindings = new CommandBindingCollection();
            this.GlobalCommandBindings.Add(TeamMateCommands.Refresh, Refresh, HasProjectContext);
            this.GlobalCommandBindings.Add(TeamMateCommands.AddWorkItemQueryTile, AddWorkItemQueryTile, HasProjectContext);
            this.GlobalCommandBindings.Add(TeamMateCommands.AddCodeFlowTile, AddCodeFlowTile, HasProjectContext);
        }

        public void RegisterBindings(CommandBindingCollection commands)
        {
            commands.Add<TileViewModel>(TeamMateCommands.RemoveTile, RemoveTile);
            commands.Add<TileViewModel>(TeamMateCommands.ModifyTile, ModifyTile);
            commands.Add<TileViewModel>(TeamMateCommands.SelectTileBackgroundColor, SelectBackgroundColor);
            commands.Add<TileViewModel>(TeamMateCommands.ResetTileBackgroundColor, ResetBackgroundColor);
        }

        public Session Session
        {
            get { return this.session; }
            set { SetProperty(ref this.session, value); }
        }

        public TileCollectionViewModel TileCollection
        {
            get { return this.tileCollection; }
        }

        public CommandBindingCollection GlobalCommandBindings { private set; get; }

        private bool HasProjectContext()
        {
            return this.ProjectContext != null;
        }

        private ProjectContext ProjectContext
        {
            get
            {
                return (Session != null) ? Session.ProjectContext : null;
            }
        }

        [Import]
        public WindowService WindowService { get; set; }


        public void AddWorkItemQueryTile()
        {
            var projectContext = this.ProjectContext;
            if (projectContext == null)
            {
                return;
            }

            WorkItemQuerySelection selection = this.WindowService.ShowQueryPickerDialog(this, projectContext);
            if (selection != null)
            {
                WorkItemQueryReference queryReference = selection.Reference;
                if (!this.tileCollection.HasTile(queryReference))
                {
                    TileInfo tileInfo = new TileInfo();
                    tileInfo.Type = TileType.WorkItemQuery;
                    tileInfo.WorkItemQueryReference = queryReference;
                    tileInfo.Name = selection.Name;
                    this.tileCollection.AddAndRefreshTileViewModel(tileInfo);
                }
            }
        }

        private void AddCodeFlowTile()
        {
            PullRequestQueryInfo queryInfo = this.WindowService.ShowCodeFlowQueryEditorDialog(this);

            if (queryInfo != null)
            {
                TileInfo tileInfo = new TileInfo();
                tileInfo.Type = TileType.CodeFlowQuery;
                tileInfo.Name = queryInfo.Name;
                tileInfo.PullRequestQueryInfo = queryInfo;
                this.tileCollection.AddAndRefreshTileViewModel(tileInfo);
            }
        }

        public void Refresh()
        {
            this.tileCollection.Refresh();
        }

        public void RemoveTile(TileViewModel tile)
        {
            this.tileCollection.RemoveTile(tile);
        }

        public void ModifyTile(TileViewModel tile)
        {
            Assert.ParamIsNotNull(tile, nameof(tile));

            if (tile.TileInfo.Type == TileType.CodeFlowQuery)
            {
                PullRequestQueryInfo queryInfo = this.WindowService.ShowCodeFlowQueryEditorDialog(this, tile.TileInfo.PullRequestQueryInfo);

                if (queryInfo != null)
                {
                    tile.TileInfo.PullRequestQueryInfo = queryInfo;
                    tile.TileInfo.Name = queryInfo.Name;
                    tile.Query.Name = queryInfo.Name;
                    tile.TileInfo.FireChanged();

                    ((PullRequestQueryViewModel)tile.Query).QueryInfo = queryInfo;
                    tile.RefreshAsync();
                }
            }
        }

        public void SelectBackgroundColor(TileViewModel tile)
        {
            Assert.ParamIsNotNull(tile, nameof(tile));

            string selectedColor = this.WindowService.ShowColorPickerDialog(this, tile.TileInfo.BackgroundColor);

            // Update the text box color if the user selected a different color
            if (!string.Equals(selectedColor, tile.TileInfo.BackgroundColor, System.StringComparison.OrdinalIgnoreCase))
            {
                tile.BackgroundColor = selectedColor;
            }
        }

        public void ResetBackgroundColor(TileViewModel tile)
        {
            Assert.ParamIsNotNull(tile, nameof(tile));

            // Assigning null will clear out any overridden background color, returning to default
            tile.BackgroundColor = null;
        }
    }
}
