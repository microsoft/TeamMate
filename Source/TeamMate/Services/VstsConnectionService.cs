// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Model.Settings;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.ViewModels;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectHttpClient = Microsoft.TeamFoundation.Core.WebApi.ProjectHttpClient;
using WorkItemField = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemField;
using WorkItemType = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemType;

namespace Microsoft.Tools.TeamMate.Services
{
    public class VstsConnectionService
    {
        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        [Import]
        public ProjectDataService ProjectDataService { get; set; }

        [Import]
        public WindowService WindowService { get; set; }

        private async Task<VssConnection> ConnectAsync(Uri projectCollectionUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            Assert.ParamIsNotNull(projectCollectionUri, "projectCollectionUri");

            VssConnection connection = new VssConnection(projectCollectionUri, new VssClientCredentials());
            using (Log.PerformanceBlock("Authenticating with collection at {0}", projectCollectionUri))
            {
                await connection.ConnectAsync(cancellationToken);
            }

            return connection;
        }

        public async Task<ProjectReference> ResolveProjectReferenceAsync(Uri projectCollectionUri, string projectName)
        {
            var connection = await this.ConnectAsync(projectCollectionUri);
            ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();
            var project = await projectClient.GetProject(projectName);

            return new ProjectReference(projectCollectionUri, project.Id);
        }


        private ApplicationSettings Settings => this.SettingsService.Settings;

        private VolatileSettings VolatileSettings => this.SettingsService.VolatileSettings;

        private Session Session => this.SessionService.Session;

        private ConnectionInfo ConnectionInfo => this.Session.ConnectionInfo;

        public async void BeginConnect(ProjectInfo project)
        {
            await ConnectAsync(project);
        }

        private CancellationTokenSource previousConnectionCancellationTokenSource = new CancellationTokenSource();

        private async Task ConnectAsync(ProjectInfo project)
        {
            Assert.ParamIsNotNull(project, "project");

            var projectRefence = this.Session.ProjectContext?.Reference;
            if (projectRefence != null && Object.Equals(projectRefence, project.Reference))
            {
                // Already connecting/connected...
                return;
            }

            this.Session.ResetConnection();

            var cancellationTokenSource = new CancellationTokenSource();
            this.previousConnectionCancellationTokenSource.Cancel();
            this.previousConnectionCancellationTokenSource = cancellationTokenSource;

            ProjectContext projectContext = await Task.Run(() => DoConnectAsync(project, cancellationTokenSource.Token));

            if (projectContext != null)
            {
                this.ConnectionInfo.ConnectionState = ConnectionState.Connected;
                this.Session.ProjectContext = projectContext;
                this.VolatileSettings.LastUsedProject = projectContext.ProjectInfo;

                // Whenever we connect to a project, we take the opportunity to periodically request feedback too.
                // Seems like an OK, non intrusive time to do this. Note that the call will rarely result in
                // an actual prompt to the user
                this.WindowService.PeriodicallyRequestFeedback();
            }
        }

        public async Task RetryConnectAsync()
        {
            if (this.ConnectionInfo.Project != null && this.ConnectionInfo.ConnectionState == ConnectionState.ConnectionFailed)
            {
                await this.ConnectAsync(this.ConnectionInfo.Project);
            }
        }

        private async Task<ProjectContext> DoConnectAsync(ProjectInfo projectInfo, CancellationToken cancellationToken)
        {
            Assert.ParamIsNotNull(projectInfo, "projectInfo");

            this.ConnectionInfo.Project = projectInfo;
            this.ConnectionInfo.ConnectionError = null;
            this.ConnectionInfo.ConnectionState = ConnectionState.Connecting;

            try
            {
                await ChaosMonkey.ChaosAsync(ChaosScenarios.ConnectToTfs);
                var connection = await this.ConnectAsync(projectInfo.ProjectCollectionUri, cancellationToken);
                var serverVersion = await ServerVersionUtilities.GetVersionAsync(connection, cancellationToken);

                if (serverVersion == ServerVersion.PreTfs2015)
                {
                    throw new NotSupportedException("TFS versions prior to 2015 are no longer supported. Sorry.");
                }

                WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();
                WorkItemTrackingBatchHttpClient batchWitClient = connection.GetClient<WorkItemTrackingBatchHttpClient>();
                ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();

                var projectId = projectInfo.Reference.ProjectId;

                var getProjectTask = projectClient.GetProject(projectId.ToString());
                var getWorkItemTypesTask = witClient.GetWorkItemTypesAsync(projectId, cancellationToken: cancellationToken);
                var getWorkItemTypeCategoriesTask = witClient.GetWorkItemTypeCategoriesAsync(projectId, cancellationToken: cancellationToken);
                var getFieldsTask = witClient.GetFieldsAsync(cancellationToken: cancellationToken);

                await Task.WhenAll(getProjectTask, getWorkItemTypesTask, getWorkItemTypeCategoriesTask, getFieldsTask);

                var project = getProjectTask.Result;
                var workItemTypes = getWorkItemTypesTask.Result;
                var workItemTypeCategories = getWorkItemTypeCategoriesTask.Result;
                var fields = getFieldsTask.Result;

                // If this was null, it is currently not the end of the world.
                var identity = connection.AuthorizedIdentity;
                string mail = identity.GetProperty<string>("Mail", null);
                var workItemIdentity = new WorkItemIdentity(identity.DisplayName, mail); ;

                var workItemTypeInfos = GetVisibleWorkItemTypes(projectInfo.Reference, workItemTypes, workItemTypeCategories);

                // Refresh the project name, it might change over time...
                if (!String.Equals(projectInfo.ProjectName, project.Name))
                {
                    projectInfo.ProjectName = project.Name;

                    // KLUDGE: Flush updated project name to disk...
                    // TODO: Make cleaner, move to reader of these or something?
                    this.SettingsService.FlushSettings();
                }

                ProjectContext projectContext = this.ProjectDataService.Load(projectInfo.Reference);
                projectContext.ProjectName = project.Name;
                projectContext.WorkItemTrackingClient = witClient;
                projectContext.WorkItemTrackingBatchClient = batchWitClient;
                projectContext.WorkItemTypes = workItemTypeInfos;
                projectContext.ProjectInfo = projectInfo;
                projectContext.HyperlinkFactory = HyperlinkFactory.Create(serverVersion, projectInfo.ProjectCollectionUri, project.Name);
                projectContext.WorkItemIdentity = workItemIdentity;
                projectContext.WorkItemFields = fields;
                projectContext.WorkItemFieldsByName = fields.ToDictionary(f => f.ReferenceName, StringComparer.OrdinalIgnoreCase);
                projectContext.RequiredWorkItemFieldNames = GetWorkItemFieldsToPrefetch(projectContext.WorkItemFieldsByName);

                return projectContext;
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    if (this.ConnectionInfo.Project == projectInfo)
                    {
                        // Only reset the connection if a different project connection is not already happening
                        this.ConnectionInfo.Project = null;
                        this.ConnectionInfo.ConnectionError = null;
                        this.ConnectionInfo.ConnectionState = ConnectionState.Disconnected;
                    }
                }
                else
                {
                    Log.Error(e);
                    this.ConnectionInfo.ConnectionError = e;
                    this.ConnectionInfo.ConnectionState = ConnectionState.ConnectionFailed;
                }
            }

            return null;
        }

        public ICollection<string> GetWorkItemFieldsToPrefetch(IDictionary<string, WorkItemField> availableFields)
        {
            // Prefetch the fields that are interesting to our services or object model...
            List<string> prefetchedFields = new List<string>();
            prefetchedFields.AddRange(WorkItemRowViewModel.RequiredWorkItemFields);
            prefetchedFields.AddRange(WorkItemRowViewModel.OptionalWorkItemFields);
            prefetchedFields.AddRange(ToastNotificationService.RequiredWorkItemFields);
            prefetchedFields.AddRange(CollaborationService.FieldsReferringToPeopleInPriorityOrder);
            prefetchedFields = prefetchedFields.Distinct().ToList();

            // IMPORTANT: Must remove all fields that don't exist on the other end, otherwise bad things will happen
            for (int i = 0; i < prefetchedFields.Count; i++)
            {
                var field = prefetchedFields[i];
                if (!availableFields.ContainsKey(field))
                {
                    prefetchedFields.RemoveAt(i--);
                }
            }

            prefetchedFields.Sort();

            return prefetchedFields;
        }

        private ICollection<WorkItemTypeInfo> GetVisibleWorkItemTypes(ProjectReference projectReference, ICollection<WorkItemType> workItemTypes, ICollection<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemTypeCategory> workItemTypeCategories)
        {
            var hiddenCategory = workItemTypeCategories.FirstOrDefault(c => c.ReferenceName == WorkItemConstants.Categories.Hidden);
            ICollection<string> hiddenWorkItemTypes = (hiddenCategory != null) ? hiddenCategory.WorkItemTypes.Select(wit => wit.Name).ToList() : new List<string>();

            var tuples = workItemTypeCategories.SelectMany(c => c.WorkItemTypes.Select(wit => new Tuple<string, string>(c.ReferenceName, wit.Name))).ToList();
            var categoriesByType = tuples.GroupBy(t => t.Item2, t => t.Item1).ToDictionary(g => g.Key, g => g.ToList());

            var visibleWorkItemTypes = new List<WorkItemTypeInfo>();
            foreach (var workItemType in workItemTypes.OrderBy(t => t.Name))
            {
                if (!hiddenWorkItemTypes.Contains(workItemType.Name))
                {
                    WorkItemTypeReference reference = new WorkItemTypeReference(workItemType.Name, projectReference);
                    var typeInfo = new WorkItemTypeInfo(reference);

                    List<string> categories;
                    if (categoriesByType.TryGetValue(workItemType.Name, out categories))
                    {
                        var preferredCategory = WorkItemTypeInfo.GetPreferredCategory(categories);
                        if (preferredCategory != null)
                        {
                            typeInfo.Category = preferredCategory.Value;
                        }
                    }

                    visibleWorkItemTypes.Add(typeInfo);
                }
            }

            return visibleWorkItemTypes;
        }

        public void Disconnect()
        {
            this.Session.ResetConnection();
            this.ConnectionInfo.ConnectionState = ConnectionState.Disconnected;
        }
    }
}
