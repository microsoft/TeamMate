using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace Microsoft.Internal.Tools.TeamMate.Services
{
    public class TrackingService
    {
        private static readonly TimeSpan CollectionInterval = TimeSpan.FromMinutes(1);

        private IDictionary<object, WeakReference<TrackingToken>> trackingTokens =
            new Dictionary<object, WeakReference<TrackingToken>>();

        private DateTime? lastCollected;

        public event EventHandler<FlaggedItemChangedEventArgs> FlaggedItemChanged;

        [Import]
        public SessionService SessionService { get; set; }


        public void Initialize()
        {
            this.SessionService.Session.ProjectContextChanged += HandleProjectContextChanged;
        }

        private void HandleProjectContextChanged(object sender, EventArgs e)
        {
            this.trackingTokens.Clear();
        }

        private TrackingInfo TrackingInfo
        {
            get
            {
                var context = this.SessionService.Session.ProjectContext;
                return (context != null) ? context.TrackingInfo : null;
            }
        }

        // This method should only be called from TrackingToken
        internal void CommitMarkAsRead(object key, int revision)
        {
            var trackingInfo = TrackingInfo;
            if (trackingInfo != null)
            {
                trackingInfo.MarkAsRead(key, revision);
            }
        }

        // This method should only be called from TrackingToken
        internal void CommitMarkAsUnread(object key)
        {
            var trackingInfo = TrackingInfo;
            if (trackingInfo != null)
            {
                trackingInfo.MarkAsUnread(key);
            }
        }

        // This method should only be called from TrackingToken
        internal void CommitSetFlagged(object key, bool isFlagged, object flaggedItem)
        {
            Assert.ParamIsNotNull(key, "key");

            var trackingInfo = TrackingInfo;
            if (trackingInfo != null)
            {
                bool changed = trackingInfo.SetFlagged(key, isFlagged);
                if (changed)
                {
                    FlaggedItemChanged?.Invoke(this, new FlaggedItemChangedEventArgs(flaggedItem, key, isFlagged));
                }
            }
        }

        public void RecentlyViewed(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            MarkAsRead(workItem);
            RecentlyViewed(workItem.GetReference());
        }

        public void RecentlyViewed(WorkItemReference workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            var trackingInfo = TrackingInfo;
            if (trackingInfo != null)
            {
                trackingInfo.RecentlyViewed(workItem);
            }
        }

        public void MarkAsRead(WorkItem workItem)
        {
            GetToken(workItem.GetReference()).MarkAsRead(workItem.Rev.Value);
        }

        public void RecentlyCreated(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            MarkAsRead(workItem);

            var trackingInfo = TrackingInfo;
            if (trackingInfo != null)
            {
                trackingInfo.RecentlyCreated(workItem);
            }
        }

        public void RecentlyUpdated(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            MarkAsRead(workItem);

            var trackingInfo = TrackingInfo;
            if (trackingInfo != null)
            {
                trackingInfo.RecentlyUpdated(workItem);
            }
        }

        public int[] GetFlaggedWorkItemIds()
        {
            var trackingInfo = TrackingInfo;
            if (trackingInfo != null)
            {
                return trackingInfo.FlaggedWorkItems.Select(wi => wi.Id).ToArray();
            }

            return new int[0];
        }

        public TrackingToken GetToken(object key)
        {
            TrackingToken token;
            WeakReference<TrackingToken> weakToken;
            if (!trackingTokens.TryGetValue(key, out weakToken))
            {
                token = CreateToken(key);
                trackingTokens[key] = new WeakReference<TrackingToken>(token);

            }
            else if (!weakToken.TryGetTarget(out token))
            {
                token = CreateToken(key);
                weakToken.SetTarget(token);
            }

            return token;
        }

        private TrackingToken CreateToken(object key)
        {
            CollectTokens();

            TrackingToken token = new TrackingToken(key, this);

            var trackingInfo = TrackingInfo;
            if (trackingInfo != null)
            {
                token.InitializeIsFlagged(trackingInfo.IsFlagged(key));

                LastReadEntry entry = trackingInfo.GetLastReadyEntry(key);
                if (entry != null)
                {
                    token.LastReadDate = entry.Date;
                    token.LastReadRevision = entry.Revision;
                }
            }

            return token;
        }

        public void CollectTokens()
        {
            if (ShouldCollect())
            {
                lastCollected = DateTime.Now;

                var keysToCollect = trackingTokens.Where(kvp => !HasValue(kvp.Value)).Select(kvp => kvp.Key).ToArray();

                Log.Info("Collecting tracing tokens, {0} tokens will be collected", keysToCollect.Length);

                foreach (var key in keysToCollect)
                {
                    trackingTokens.Remove(key);
                }
            }
        }

        private bool ShouldCollect()
        {
            return trackingTokens.Count > 1000 && (lastCollected == null || (DateTime.Now - lastCollected.Value) > CollectionInterval);
        }

        private static bool HasValue<T>(WeakReference<T> weakReference) where T : class
        {
            T value;
            return weakReference.TryGetTarget(out value);
        }
    }

    public class FlaggedItemChangedEventArgs : EventArgs
    {
        public FlaggedItemChangedEventArgs(object flaggedItem, object key, bool isFlagged)
        {
            this.Item = flaggedItem;
            this.Key = key;
            this.IsFlagged = isFlagged;
        }

        public object Item { get; private set; }
        public object Key { get; private set; }
        public bool IsFlagged { get; private set; }
    }

    public class TrackingToken
    {
        public event EventHandler<EventArgs> Changed;

        public TrackingToken(object key, TrackingService trackingService)
        {
            this.Key = key;
            this.TrackingService = trackingService;
        }

        public void AddChangeListener(EventHandler<EventArgs> listener)
        {
            WeakEventManager<TrackingToken, EventArgs>.AddHandler(this, "Changed", listener);
        }

        public void RemoveChangeListener(EventHandler<EventArgs> listener)
        {
            WeakEventManager<TrackingToken, EventArgs>.RemoveHandler(this, "Changed", listener);
        }

        private object Key { get; set; }
        private TrackingService TrackingService { get; set; }
        public bool IsFlagged { get; set; }

        public void InitializeIsFlagged(bool isFlagged)
        {
            this.IsFlagged = isFlagged;
        }

        public void SetFlag(bool isFlagged, object flaggedItem)
        {
            if (this.IsFlagged != isFlagged)
            {
                Debug.Assert(!isFlagged || flaggedItem != null, "A flagged item (e.g. work item) is required when a tracking token is flagged");

                this.IsFlagged = isFlagged;
                TrackingService.CommitSetFlagged(Key, this.IsFlagged, flaggedItem);
                FireChanged();
            }
        }

        public int? LastReadRevision { get; internal set; }
        public DateTime? LastReadDate { get; internal set; }

        public bool IsRead(int revision)
        {
            return this.LastReadRevision != null && this.LastReadRevision.Value >= revision;
        }

        public void MarkAsRead(int revision)
        {
            if (this.LastReadRevision == null || this.LastReadRevision.Value < revision)
            {
                this.LastReadRevision = revision;
                this.LastReadDate = DateTime.Now;

                TrackingService.CommitMarkAsRead(Key, revision);
                FireChanged();
            }
        }

        public void MarkAsUnread()
        {
            if (this.LastReadRevision != null)
            {
                this.LastReadRevision = null;
                TrackingService.CommitMarkAsUnread(Key);
                FireChanged();
            }
        }

        private void FireChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
