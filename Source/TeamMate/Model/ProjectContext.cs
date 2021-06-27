// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Model.Settings;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Tools.TeamMate.Model
{
    public class ProjectContext : ObservableObjectBase
    {
        private TrackingInfo trackingInfo = new TrackingInfo();
        private ObservableCollection<TileInfo> tiles = new ObservableCollection<TileInfo>();

        public ProjectContext(ProjectReference reference)
        {
            Assert.ParamIsNotNull(reference, "reference");

            this.Reference = reference;
            this.ProjectSettings = new ProjectSettings();
        }

        public ProjectInfo ProjectInfo { get; set; }

        public ProjectReference Reference { get; private set; }

        public WorkItemTrackingHttpClient WorkItemTrackingClient { get; set; }

        public WorkItemTrackingBatchHttpClient WorkItemTrackingBatchClient { get; set; }

        public string ProjectName { get; set; }

        public ICollection<WorkItemTypeInfo> WorkItemTypes { get; set; }

        public TrackingInfo TrackingInfo
        {
            get { return this.trackingInfo; }
        }

        public ObservableCollection<TileInfo> Tiles
        {
            get { return this.tiles; }
        }

        public ProjectSettings ProjectSettings { get; set; }

        public ICollection<string> RequiredWorkItemFieldNames { get; internal set; }

        public ICollection<WorkItemField> WorkItemFields { get; internal set; }

        public WorkItemIdentity WorkItemIdentity { get; internal set; }

        public IDictionary<string, WorkItemField> WorkItemFieldsByName { get; internal set; }
        public HyperlinkFactory HyperlinkFactory { get; internal set; }

        public bool IsWorkItemPriorityHigh(int priority)
        {
            // TODO: REST API limitation
            // There's currently no way of getting the allowed values list of a field using the REST API, hopefully one day
            // Get the allowed value, parse the smallest, and compare with input when available.

            // var field = this.WorkItemFields.FirstOrDefault(f => f.ReferenceName == WorkItemConstants.VstsFields.Priority);
            return false;
        }
    }
}
