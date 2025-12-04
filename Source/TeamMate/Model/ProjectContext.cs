using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Model.Settings;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.Graph.Client;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Model
{
    [SupportedOSPlatform("windows10.0.19041.0")]
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

        public VssConnection Connection { get; set; }

        /// <summary>
        /// Function to refresh the connection when the token expires.
        /// </summary>
        public Func<CancellationToken, Task> RefreshConnectionAsync { get; set; }

        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Executes an API call with automatic token refresh on authentication failure.
        /// </summary>
        public async Task<T> ExecuteWithTokenRefreshAsync<T>(Func<Task<T>> apiCall, CancellationToken cancellationToken = default)
        {
            try
            {
                return await apiCall();
            }
            catch (VssUnauthorizedException)
            {
                // Token expired, refresh and retry
                await RefreshConnectionIfNeededAsync(cancellationToken);
                return await apiCall();
            }
            catch (VssServiceException ex) when (ex.Message.Contains("VS30063") || ex.Message.Contains("not authorized"))
            {
                // Authorization error, refresh and retry
                await RefreshConnectionIfNeededAsync(cancellationToken);
                return await apiCall();
            }
        }

        /// <summary>
        /// Executes an API call with automatic token refresh on authentication failure (for void returns).
        /// </summary>
        public async Task ExecuteWithTokenRefreshAsync(Func<Task> apiCall, CancellationToken cancellationToken = default)
        {
            try
            {
                await apiCall();
            }
            catch (VssUnauthorizedException)
            {
                // Token expired, refresh and retry
                await RefreshConnectionIfNeededAsync(cancellationToken);
                await apiCall();
            }
            catch (VssServiceException ex) when (ex.Message.Contains("VS30063") || ex.Message.Contains("not authorized"))
            {
                // Authorization error, refresh and retry
                await RefreshConnectionIfNeededAsync(cancellationToken);
                await apiCall();
            }
        }

        private async Task RefreshConnectionIfNeededAsync(CancellationToken cancellationToken)
        {
            await _refreshLock.WaitAsync(cancellationToken);
            try
            {
                if (RefreshConnectionAsync != null)
                {
                    await RefreshConnectionAsync(cancellationToken);
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public WorkItemTrackingHttpClient WorkItemTrackingClient { get; set; }

        public GitHttpClient GitHttpClient { get; set; }

        public Microsoft.VisualStudio.Services.Identity.Identity Identity { get; set; }

        public WorkItemTrackingBatchHttpClient WorkItemTrackingBatchClient { get; set; }

        public GraphHttpClient GraphClient { get; set; }

        public Task<List<GraphUser>> UsersAsync { get; set; }

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
