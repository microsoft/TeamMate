using Microsoft.Internal.Tools.TeamMate.Model;
using Microsoft.Internal.Tools.TeamMate.Services;
using Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.ViewModels
{
    public class FlaggedWorkItemsViewModel : WorkItemQueryViewModel
    {
        public FlaggedWorkItemsViewModel()
        {
            this.BuiltInTileType = BuiltInTileType.Flagged;
        }

        protected override WorkItemQuery CreateWorkItemQuery()
        {
            // TODO: KLUDGE: This is the ugliest registration of a mediator ever. Clean up.
            // Also, there is no unregistration. Memory leak!
            if (!isRegisteredWithTrackingService)
            {
                isRegisteredWithTrackingService = true;
                this.TrackingService.FlaggedItemChanged += HandleFlaggedItemChanged;
            }

            return base.CreateWorkItemQuery();
        }

        private void HandleFlaggedItemChanged(object sender, FlaggedItemChangedEventArgs e)
        {
            // TODO: What if the collection is refreshing while I've flagged? Something will be out of sync.
            // Deal with this better.

            var workItems = WorkItems;
            if (workItems != null)
            {
                var existingItem = workItems.FirstOrDefault(wir => wir.Reference.Equals(e.Key));

                if (!e.IsFlagged)
                {
                    if (existingItem != null)
                    {
                        // TODO: REALLY REALLY UGLY IMPLEMENTATION OF THIS... REDO.
                        Unregister(existingItem);
                        workItems.Remove(existingItem);
                        ItemCount -= 1;

                        if (!existingItem.IsRead)
                        {
                            InvalidateUnreadItemCount();
                        }

                        FireWorkItemCollectionChanged();
                    }
                }
                else
                {
                    // Only add it if the item doesn't already exist in this list, just being defensive
                    if (existingItem == null)
                    {
                        // TODO: Update this so that it is not hardcoded to work items only
                        var workItem = e.Item as WorkItemRowViewModel;
                        if (workItem != null)
                        {
                            Register(workItem);
                            workItems.Add(workItem);

                            // TODO: Add it to the collection...
                            ItemCount += 1;

                            if (!workItem.IsRead)
                            {
                                InvalidateUnreadItemCount();
                            }

                            FireWorkItemCollectionChanged();
                        }
                        else
                        {
                            // TODO: If we do not have the work item, we could still call async resolve on it and add it here
                            Debug.Fail("Need work item to add it to the actual flagged list");
                        }
                    }
                }
            }
        }

        private bool isRegisteredWithTrackingService;
    }
}
