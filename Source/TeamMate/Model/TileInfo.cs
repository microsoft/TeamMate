using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using Microsoft.Tools.TeamMate.Foundation.Reflection;
using System;
using System.ComponentModel;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Model
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class TileInfo : ObservableObjectBase
    {
        // TODO: Should we make this smarter, with property events, etc...?
        public event EventHandler Changed;

        private TileType type;

        public TileType Type
        {
            get { return this.type; }
            set { SetProperty(ref this.type, value); }
        }

        private string name;

        public string Name
        {
            get { return this.name; }
            set { SetProperty(ref this.name, value); }
        }

        private bool showNotifications;

        public bool ShowNotifications
        {
            get { return this.showNotifications; }
            set { SetProperty(ref this.showNotifications, value); }
        }

        private DateTime? lastUpdated;

        public DateTime? LastUpdated
        {
            get { return this.lastUpdated; }
            set { SetProperty(ref this.lastUpdated, value); }
        }

        private WorkItemQueryReference workItemQueryReference;

        public WorkItemQueryReference WorkItemQueryReference
        {
            get { return this.workItemQueryReference; }
            set { SetProperty(ref this.workItemQueryReference, value); }
        }

        private PullRequestQueryInfo pullRequestQueryInfo;

        public PullRequestQueryInfo PullRequestQueryInfo
        {
            get { return this.pullRequestQueryInfo; }
            set { SetProperty(ref this.pullRequestQueryInfo, value); }
        }

        private BuiltInTileType builtInTileType;

        public BuiltInTileType BuiltInTileType
        {
            get { return this.builtInTileType; }
            set { SetProperty(ref this.builtInTileType, value); }
        }

        private bool includeInItemCountSummary;

        public bool IncludeInItemCountSummary
        {
            get { return this.includeInItemCountSummary; }
            set { SetProperty(ref this.includeInItemCountSummary, value); }
        }

        private String orderByFieldName;

        public String OrderByFieldName
        {
            get { return this.orderByFieldName; }
            set { SetProperty(ref this.orderByFieldName, value); }
        }

        private String filterByFieldName;
        public String FilterByFieldName
        {
            get { return this.filterByFieldName; }
            set { SetProperty(ref this.filterByFieldName, value); }
        }

        private string backgroundColor;

        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set { SetProperty(ref this.backgroundColor, value); }
        }

        private string fontColor;
        public string FontColor
        {
            get { return this.fontColor; }
            set { SetProperty(ref this.fontColor, value); }
        }

        // TODO: KLUDGE, don't make this public, refactor or cleanup
        public void FireChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public static TileInfo CreateBuiltIn(BuiltInTileType type)
        {
            TileInfo tile = new TileInfo();
            tile.Type = TileType.BuiltIn;
            tile.BuiltInTileType = type;
            tile.Name = ReflectionUtilities.GetEnumDescription(type);
            return tile;
        }
    }

    public enum TileType
    {
        WorkItemQuery,
        PullRequestQuery,
        BuiltIn
    }

    public enum BuiltInTileType
    {
        Undefined,

        [Description("Assigned to Me")]
        AssignedToMe,

        [Description("Bugs to Me")]
        BugsToMe,

        [Description("For Follow Up")]
        Flagged,
    }
}
