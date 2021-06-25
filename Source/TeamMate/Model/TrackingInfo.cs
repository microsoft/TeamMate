using Microsoft.Internal.Tools.TeamMate.Foundation.Collections;
using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace Microsoft.Internal.Tools.TeamMate.Model
{
    public class TrackingInfo
    {
        private const int MaxItems = 20;

        private bool recentWorkItemsChanged;

        private IDictionary<object, LastReadEntry> entries = new Dictionary<object, LastReadEntry>();

        // TODO: [TRACKING REFACTORING] More work here...
        private ObservableCollection<WorkItemReference> flagged = new ObservableCollection<WorkItemReference>();

        private ObservableCollection<WorkItemReference> recentlyViewed = new ObservableCollection<WorkItemReference>();
        private ObservableCollection<WorkItemReference> recentlyCreated = new ObservableCollection<WorkItemReference>();
        private ObservableCollection<WorkItemReference> recentlyUpdated = new ObservableCollection<WorkItemReference>();

        public event EventHandler LastReadChanged;
        public event EventHandler FlaggedWorkItemsChanged;
        public event EventHandler RecentWorkItemsChanged;

        public TrackingInfo()
        {
            recentlyViewed.CollectionChanged += HandleRecentCollectionChanged;
            recentlyCreated.CollectionChanged += HandleRecentCollectionChanged;
            recentlyUpdated.CollectionChanged += HandleRecentCollectionChanged;
        }

        public ObservableCollection<WorkItemReference> FlaggedWorkItems
        {
            get { return this.flagged; }
        }

        public ObservableCollection<WorkItemReference> RecentlyViewedWorkItems
        {
            get { return this.recentlyViewed; }
        }

        public ObservableCollection<WorkItemReference> RecentlyCreatedWorkItems
        {
            get { return this.recentlyCreated; }
        }

        public ObservableCollection<WorkItemReference> RecentlyUpdatedWorkItems
        {
            get { return this.recentlyUpdated; }
        }

        public bool IsFlagged(object key)
        {
            Assert.ParamIsNotNull(key, "key");

            // TODO: [TRACKING REFACTORING] More work here...
            WorkItemReference workItemReference = key as WorkItemReference;

            return (workItemReference != null && flagged.Contains(workItemReference));
        }

        public bool SetFlagged(object key, bool flag)
        {
            Assert.ParamIsNotNull(key, "key");

            // TODO: [TRACKING REFACTORING] More work here...
            WorkItemReference workItemReference = (WorkItemReference)key;

            bool flaggedChanged = false;

            if (flag)
            {
                if (!IsFlagged(key))
                {
                    flagged.Add(workItemReference);
                    flaggedChanged = true;
                }
            }
            else
            {
                flaggedChanged = flagged.Remove(workItemReference);
            }

            if (flaggedChanged)
            {
                FireFlaggedWorkItemsChanged();
            }

            return flaggedChanged;
        }

        public void RecentlyViewed(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            WorkItemReference reference = workItem.GetReference();
            RecentlyViewed(workItem);
        }

        public void RecentlyViewed(WorkItemReference reference)
        {
            Assert.ParamIsNotNull(reference, "workItem");

            AddToMru(reference, recentlyViewed);
            FireRecentWorkItemsChanged();
        }

        public void RecentlyCreated(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            WorkItemReference reference = workItem.GetReference();
            AddToMru(reference, recentlyCreated);
            AddToMru(reference, recentlyViewed);
            FireRecentWorkItemsChanged();
        }

        public void RecentlyUpdated(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            WorkItemReference reference = workItem.GetReference();
            AddToMru(reference, recentlyUpdated);
            AddToMru(reference, recentlyViewed);
            FireRecentWorkItemsChanged();
        }

        private void AddToMru(WorkItemReference reference, ObservableCollection<WorkItemReference> list)
        {
            int currentIndex = list.IndexOf(reference);
            if (currentIndex >= 0)
            {
                if (currentIndex != 0)
                {
                    // Move to the front of the list if already existed...
                    list.Move(currentIndex, 0);
                }
            }
            else
            {
                list.AddToMruList(reference, MaxItems);
            }
        }

        private void FireRecentWorkItemsChanged()
        {
            if (recentWorkItemsChanged)
            {
                recentWorkItemsChanged = false;

                RecentWorkItemsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void FireFlaggedWorkItemsChanged()
        {
            FlaggedWorkItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandleRecentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            recentWorkItemsChanged = true;
        }

        public void MarkAsRead(object key, int revision)
        {
            Assert.ParamIsNotNull(key, "key");
            Assert.ParamIsGreaterThanZero(revision, "revision");

            if (!IsRead(key, revision))
            {
                AddEntry(new LastReadEntry(key, revision, DateTime.Now));
                LastReadChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void MarkAsUnread(object key)
        {
            Assert.ParamIsNotNull(key, "key");

            if (entries.Remove(key))
            {
                LastReadChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsRead(object key, int revision)
        {
            Assert.ParamIsNotNull(key, "key");
            Assert.ParamIsGreaterThanZero(revision, "revision");

            LastReadEntry entry;
            if (entries.TryGetValue(key, out entry) && entry.Revision >= revision)
            {
                return true;
            }

            return false;
        }

        public LastReadEntry GetLastReadyEntry(object key)
        {
            Assert.ParamIsNotNull(key, "key");

            LastReadEntry entry;
            entries.TryGetValue(key, out entry);
            return entry;
        }

        public IEnumerable<LastReadEntry> LastReadEntries
        {
            get
            {
                return entries.Values;
            }
        }

        public void ClearLastReadEntries()
        {
            entries.Clear();
        }

        public void AddEntry(LastReadEntry entry)
        {
            entries[entry.Key] = entry;
        }
    }

    public class LastReadEntry
    {
        public LastReadEntry(object key, int revision, DateTime date)
        {
            this.Key = key;
            this.Revision = revision;
            this.Date = date;
        }

        public object Key { get; private set; }
        public int Revision { get; private set; }
        public DateTime Date { get; private set; }
    }
}
