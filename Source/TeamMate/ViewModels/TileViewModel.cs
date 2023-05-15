using Microsoft.Tools.TeamMate.Foundation.Threading;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Services;
using System;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public abstract class TileViewModel : ViewModelBase
    {
        private TileInfo tileInfo;
        private QueryViewModelBase query;

        public event EventHandler Refreshed;

        public TileInfo TileInfo
        {
            get { return this.tileInfo; }
            set
            {
                if (SetProperty(ref this.tileInfo, value))
                {
                    if (this.Query != null)
                    {
                        this.Query.QueryExecuted -= HandleQueryExecuted;
                    }

                    // On purpose not use the property as we don't want to fire an event here. Also, do this before creating
                    // the query!
                    this.showNotifications = this.TileInfo != null && this.tileInfo.ShowNotifications;
                    this.includeInItemCountSummary = this.TileInfo != null && this.tileInfo.IncludeInItemCountSummary;
                    this.backgroundColor = this.GetBackgroundColor();
                    this.isDefaultBackgroundColor = this.IsDefaultBackColor();
                    this.fontColor = this.GetFontColor();
                    this.tileInfo.FontColor = this.fontColor;
                    this.isDefaultFontColor = this.IsDefaultTextFontColor();

                    this.Query = (this.TileInfo != null) ? CreateQueryViewModel(this.TileInfo) : null;

                    if (this.Query != null)
                    {
                        this.Query.QueryExecuted += HandleQueryExecuted;
                    }
                }
            }
        }

        private bool showNotifications;

        public bool ShowNotifications
        {
            get { return this.showNotifications; }
            set
            {
                if (SetProperty(ref this.showNotifications, value))
                {
                    if (this.TileInfo != null)
                    {
                        this.TileInfo.ShowNotifications = value;
                        this.TileInfo.FireChanged();
                    }

                    if (this.Query != null)
                    {
                        this.Query.ShowNotifications = value;
                    }
                }
            }
        }


        private bool includeInItemCountSummary;

        public bool IncludeInItemCountSummary
        {
            get { return this.includeInItemCountSummary; }
            set
            {
                if (SetProperty(ref this.includeInItemCountSummary, value))
                {
                    if (this.TileInfo != null)
                    {
                        this.TileInfo.IncludeInItemCountSummary = value;
                        this.TileInfo.FireChanged();
                    }

                    if (this.Query != null)
                    {
                        this.Query.IncludeInItemCountSummary = value;
                    }
                }
            }
        }

        private bool isDefaultBackgroundColor;
        public bool IsDefaultBackgroundColor
        {
            get
            {
                return this.isDefaultBackgroundColor;
            }
            set
            {
                SetProperty(ref this.isDefaultBackgroundColor, value);
            }
        }

        private bool isDefaultFontColor;
        public bool IsDefaultFontColor
        {
            get
            {
                return this.isDefaultFontColor;
            }
            set
            {
                SetProperty(ref this.isDefaultFontColor, value);
            }
        }

        private string backgroundColor;
        /// <summary>
        /// Holds the hex code for the background color of the tile.
        /// Defaults to a predefined color based on the type of the tile, if no color is explicitly selected for it.
        /// </summary>
        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.TileInfo.BackgroundColor = value;
                this.TileInfo.FireChanged();

                SetProperty(ref this.backgroundColor, this.GetBackgroundColor());
                this.IsDefaultBackgroundColor = this.IsDefaultBackColor();
            }
        }

        private string GetBackgroundColor()
        {
            if (this.TileInfo != null && !string.IsNullOrEmpty(this.TileInfo.BackgroundColor))
            {
                return this.TileInfo.BackgroundColor;
            }
            return this.GetDefaultBackgroundColor();
        }

        private bool IsDefaultBackColor()
        {
            return string.Equals(this.backgroundColor, this.GetDefaultBackgroundColor(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get the default background color for the tile based on its type
        /// </summary>
        /// <returns>Hex value of the default color for the tile</returns>
        public string GetDefaultBackgroundColor()
        {
            const string DefaultBackgroundColor = "#00A200";
            const string PullRequestQueryTileBackgroundColor = "#0063B1";

            if (this.TileInfo != null)
            {
                switch (this.TileInfo.Type)
                {
                    case TileType.PullRequestQuery:
                        return PullRequestQueryTileBackgroundColor;
                    default:
                        return DefaultBackgroundColor;
                }
            }
            return DefaultBackgroundColor;
        }

        protected const string DefaultFontColor = "White";

        private string fontColor = DefaultFontColor;
        /// <summary>
        /// Holds the hex code for the background color of the tile.
        /// Defaults to a predefined color based on the type of the tile, if no color is explicitly selected for it.
        /// </summary>
        public string FontColor
        {
            get { return this.fontColor; }
            set
            {
                this.TileInfo.FontColor = value;
                this.TileInfo.FireChanged();

                SetProperty(ref this.fontColor, this.GetFontColor());
                this.IsDefaultFontColor = this.IsDefaultTextFontColor();
            }
        }

        private string GetFontColor()
        {
            if (this.TileInfo != null && !string.IsNullOrEmpty(this.TileInfo.FontColor))
            {
                return this.TileInfo.FontColor;
            }
            return this.GetDefaultFontColor();
        }
        public void ResetFontColor()
        {
            this.FontColor = DefaultFontColor;
        }

        private bool IsDefaultTextFontColor()
        {
            return string.Equals(this.FontColor, this.GetDefaultFontColor(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get the default foreground color for the tile based on its type
        /// </summary>
        /// <returns>Hex value of the default color for the tile</returns>
        public string GetDefaultFontColor()
        {
            return DefaultFontColor;
        }

        public QueryViewModelBase Query
        {
            get { return this.query; }
            private set { SetProperty(ref this.query, value); }
        }

        protected abstract QueryViewModelBase CreateQueryViewModel(TileInfo tileInfo);

        public Task RefreshAsync(NotificationScope notificationScope = null)
        {
            if (query != null)
            {
                return query.RefreshAsync(notificationScope);
            }
            else
            {
                return TaskUtilities.NullTask;
            }
        }

        public virtual void Activate()
        {
        }

        private void HandleQueryExecuted(object sender, EventArgs e)
        {
            this.OnQueryExecuted();

            if (this.TileInfo != null)
            {
                this.TileInfo.LastUpdated = this.Query.LastUpdated;
                this.TileInfo.FireChanged();
            }

            this.Refreshed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnQueryExecuted()
        {
        }
    }
}
