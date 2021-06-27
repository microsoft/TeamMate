using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.Utilities;
using System;
using System.ComponentModel.Composition;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public abstract class TrackableViewModelBase : ViewModelBase
    {
        private bool isFlagged;
        private bool isRead;
        private TrackingToken trackingToken;
        private bool invalidatingTrackingToken;

        private bool? overrideIsRead;

        public bool? OverrideIsRead
        {
            get { return this.overrideIsRead; }
            set { SetProperty(ref this.overrideIsRead, value); }
        }

        public void ToggleFlag()
        {
            IsFlagged = !IsFlagged;
        }

        public bool IsFlagged
        {
            get
            {
                EnsureTrackingTokenIsLoaded();
                return this.isFlagged;
            }

            set
            {
                EnsureTrackingTokenIsLoaded();
                if (SetProperty(ref this.isFlagged, value) && !invalidatingTrackingToken)
                {
                    if (this.trackingToken != null)
                    {
                        this.trackingToken.SetFlag(value, GetFlaggedItem());
                    }
                }
            }
        }

        public bool IsRead
        {
            get
            {
                if (OverrideIsRead != null)
                {
                    return OverrideIsRead.Value;
                }

                EnsureTrackingTokenIsLoaded();
                return this.isRead;
            }

            set
            {
                if (OverrideIsRead != null)
                {
                    return;
                }

                EnsureTrackingTokenIsLoaded();
                if (SetProperty(ref this.isRead, value) && !invalidatingTrackingToken)
                {
                    if (this.trackingToken != null)
                    {
                        if (value)
                        {
                            this.trackingToken.MarkAsRead(GetRevision());
                        }
                        else
                        {
                            this.trackingToken.MarkAsUnread();
                        }
                    }
                }
            }
        }

        [Import]
        public TrackingService TrackingService { get; set; }


        protected void EnsureTrackingTokenIsLoaded()
        {
            if (this.trackingToken == null)
            {
                var trackingService = this.TrackingService;
                if (trackingService != null)
                {
                    var trackingTokenKey = GetTrackingTokenKey();
                    if (trackingTokenKey != null)
                    {
                        this.trackingToken = trackingService.GetToken(trackingTokenKey);
                        this.trackingToken.AddChangeListener(HandleTrackingTokenChanged);
                        InvalidateTrackingToken();
                    }
                }
            }
        }

        private void HandleTrackingTokenChanged(object sender, EventArgs e)
        {
            InvalidateTrackingToken();
        }

        protected void ResetTrackingToken()
        {
            if (this.trackingToken != null)
            {
                this.trackingToken.RemoveChangeListener(HandleTrackingTokenChanged);
                this.trackingToken = null;
            }
        }

        private void InvalidateTrackingToken()
        {
            try
            {
                invalidatingTrackingToken = true;

                this.IsFlagged = this.trackingToken.IsFlagged;
                this.IsRead = this.trackingToken.IsRead(GetRevision()) || WasLastChangedByMe();
            }
            finally
            {
                invalidatingTrackingToken = false;
            }
        }

        protected abstract bool WasLastChangedByMe();
        protected abstract int GetRevision();
        protected abstract object GetFlaggedItem();
        protected abstract object GetTrackingTokenKey();
    }
}
