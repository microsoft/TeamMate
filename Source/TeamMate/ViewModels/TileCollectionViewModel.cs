using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Collections;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class TileCollectionViewModel : ViewModelBase
    {
        private ItemCountSummary itemCountSummary = new ItemCountSummary();
        private ObservableCollection<TileViewModel> tiles = new ObservableCollection<TileViewModel>();
        private bool adjustingTiles;

        private DeferredAction deferredInvalidateItemCount;

        public TileCollectionViewModel()
        {
            this.tiles.CollectionChanged += HandleTilesChanged;
            this.deferredInvalidateItemCount = new DeferredAction(this.InvalidateItemCountSummary);
        }

        public ItemCountSummary ItemCountSummary
        {
            get { return this.itemCountSummary; }
        }

        private string tileGroupName;

        public string TileGroupName
        {
            get { return this.tileGroupName; }
            set { SetProperty(ref this.tileGroupName, value); }
        }

        private ObservableCollection<TileInfo> rawTiles;

        public ObservableCollection<TileInfo> RawTiles
        {
            get { return this.rawTiles; }
            set
            {
                if (SetProperty(ref this.rawTiles, value))
                {
                    // A project is being changed, reset count, refresh will happen later
                    this.ItemCountSummary.Reset();

                    adjustingTiles = true;

                    foreach (var tile in this.Tiles)
                    {
                        Unregister(tile);
                    }

                    Tiles.Clear();

                    if (RawTiles != null)
                    {
                        Tiles.AddRange(RawTiles.Select(tileInfo => CreateTileViewModel(tileInfo)));

                        foreach (var tile in this.Tiles)
                        {
                            Register(tile);
                        }
                    }

                    adjustingTiles = false;
                }
            }
        }

        private void Unregister(TileViewModel tile)
        {
            tile.PropertyChanged -= HandleTilePropertyChanged;
            tile.Refreshed -= HandleTileRefreshed;
        }

        private void Register(TileViewModel tile)
        {
            tile.PropertyChanged += HandleTilePropertyChanged;
            tile.Refreshed += HandleTileRefreshed;
        }

        public ICollection<TileViewModel> Tiles
        {
            get { return this.tiles; }
        }

        public bool HasTile(WorkItemQueryReference queryReference)
        {
            return Tiles.OfType<WorkItemQueryTileViewModel>().Any(q => Object.Equals(q.TileInfo.WorkItemQueryReference, queryReference));
        }

        public async void AddAndRefreshTileViewModel(TileInfo tileInfo)
        {
            TileViewModel tile = CreateTileViewModel(tileInfo);
            Tiles.Add(tile);
            Register(tile);
            await tile.RefreshAsync();
        }

        public void RemoveTile(TileViewModel tile)
        {
            Assert.ParamIsNotNull(tile, "tile");

            Tiles.Remove(tile);
            Unregister(tile);

            if (tile.IncludeInItemCountSummary)
            {
                this.deferredInvalidateItemCount.InvokeIfNotDeferred();
            }
        }

        [Import]
        public WindowService WindowService { get; set; }

        public async void Refresh()
        {
            this.WindowService.RefreshTilesAction.Reset();

            // Use a shared scope to avoid the same notification from multiple queries
            NotificationScope sharedNotificationScope = new NotificationScope();

            List<Task> tasks = new List<Task>();

            foreach (var tile in Tiles)
            {
                var task = tile.RefreshAsync(sharedNotificationScope);
                tasks.Add(task);
            }

            if (tasks.Any())
            {
                // TODO: How do we invalidate read/unread states when a work item was read and was included in the count?
                using (this.deferredInvalidateItemCount.Acquire())
                {
                    await Task.WhenAll(tasks);
                }
            }
        }

        private object invalidateItemCountSummaryLock = new object();

        private void InvalidateItemCountSummary()
        {
            try
            {
                lock (invalidateItemCountSummaryLock)
                {
                    var allWorkItems = GetItemsTowardsCount();
                    var workItemsByState = allWorkItems.GroupBy(wi => wi.WorkItemState).ToDictionary(g => g.Key, g => g.ToArray());

                    var summary = this.ItemCountSummary;
                    UpdateCounter(summary.GlobalCounter, allWorkItems);
                    UpdateCounter(summary.ActiveCounter, workItemsByState.TryGetValueOrDefault(WorkItemState.Active));
                    UpdateCounter(summary.ResolvedCounter, workItemsByState.TryGetValueOrDefault(WorkItemState.Resolved));
                    UpdateCounter(summary.ClosedCounter, workItemsByState.TryGetValueOrDefault(WorkItemState.Closed));
                    UpdateCounter(summary.UnknownCounter, workItemsByState.TryGetValueOrDefault(WorkItemState.Unknown));
                }
            }
            catch (Exception e)
            {
                Log.ErrorAndBreak(e, "An unexpected error occurred invalidating the global item counts");
            }
        }

        private static void UpdateCounter(Counter counter, ICollection<WorkItemRowViewModel> items)
        {
            // items can be null by design, it is an optimization if there are no items
            int count = (items != null) ? items.Count : 0;
            bool isRead = (items != null) ? !items.Any(wi => !wi.IsRead) : true;

            counter.UpdateCount(count, isRead);
        }

        private IList<WorkItemRowViewModel> GetItemsTowardsCount()
        {
            // TODO: Get PullRequests too? How do these surface in the UI?
            var allWorkItems = Tiles.OfType<WorkItemQueryTileViewModel>().Where(wiq => wiq.IncludeInItemCountSummary)
                .Select(q => q.WorkItemQuery.WorkItems).Where(items => items != null).SelectMany(w => w).Distinct().ToArray();

            return allWorkItems;
        }

        private TileViewModel CreateTileViewModel(TileInfo tileInfo)
        {
            TileViewModel result = null;

            switch (tileInfo.Type)
            {
                case TileType.WorkItemQuery:
                    result = ViewModelFactory.Create<WorkItemQueryTileViewModel>();
                    break;

                case TileType.PullRequestQuery:
                    result = ViewModelFactory.Create<PullRequestQueryTileViewModel>();
                    break;

                case TileType.BuiltIn:
                    switch (tileInfo.BuiltInTileType)
                    {
                        case BuiltInTileType.Flagged:
                        case BuiltInTileType.AssignedToMe:
                        case BuiltInTileType.BugsToMe:
                            result = ViewModelFactory.Create<WorkItemQueryTileViewModel>();
                            break;
                    }

                    break;
            }

            if (result != null)
            {
                result.TileInfo = tileInfo;
                return result;
            }

            throw new NotSupportedException(String.Format("Tile type {0} is not supported", tileInfo.Type));
        }

        private void HandleTilesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!adjustingTiles && RawTiles != null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            RawTiles.Insert(e.NewStartingIndex + i, ((TileViewModel)e.NewItems[i]).TileInfo);
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            RawTiles.RemoveAt(e.OldStartingIndex);
                        }
                        break;

                    case NotifyCollectionChangedAction.Move:
                        RawTiles.Move(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                }
            }
        }

        private void HandleTilePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IncludeInItemCountSummary")
            {
                this.deferredInvalidateItemCount.InvokeIfNotDeferred();
            }
        }

        private void HandleTileRefreshed(object sender, EventArgs e)
        {
            TileViewModel tile = (TileViewModel)sender;
            if (tile.IncludeInItemCountSummary)
            {
                this.deferredInvalidateItemCount.InvokeIfNotDeferred();
            }
        }
    }
}
