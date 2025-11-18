using Microsoft.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Model.Settings;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.ViewModels;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Azure.Identity;
using Azure.Core;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ProjectHttpClient = Microsoft.TeamFoundation.Core.WebApi.ProjectHttpClient;
using WorkItemField = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemField;
using WorkItemType = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemType;
using Microsoft.VisualStudio.Services.Graph.Client;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Services
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class VstsConnectionService
    {
        // Cached MSAL public client application - reused across connections
        private static IPublicClientApplication _msalApp;
        private static readonly object _msalLock = new object();
        private const string ClientId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46"; // Azure CLI client ID
        private const string Authority = "https://login.microsoftonline.com/common";
        private static readonly string[] Scopes = new[] { "499b84ac-1321-427f-aa17-267ca6975798/.default" };

        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        [Import]
        public ProjectDataService ProjectDataService { get; set; }

        [Import]
        public WindowService WindowService { get; set; }
    
        [Import]
        public ResolverService ResolverService { get; set; }


        private async Task<VssConnection> ConnectAsync(Uri projectCollectionUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            Assert.ParamIsNotNull(projectCollectionUri, "projectCollectionUri");

            // Get or create the MSAL app
            var app = GetOrCreateMsalApp();

            AuthenticationResult authResult;
            try
            {
                // Try to acquire token silently first (from cache)
                var accounts = await app.GetAccountsAsync();
                authResult = await app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync(cancellationToken);
            }
            catch (MsalUiRequiredException)
            {
                // Silent acquisition failed, need interactive login
                authResult = await app.AcquireTokenInteractive(Scopes)
                    .WithUseEmbeddedWebView(false) // Use system browser
                    .ExecuteAsync(cancellationToken);
            }

            // Use the token with VssAadCredential
            var credentials = new VssAadCredential(new VssAadToken("Bearer", authResult.AccessToken));

            var settings = new VssClientHttpRequestSettings();
            settings.AllowAutoRedirect = true;

            VssConnection connection = new VssConnection(projectCollectionUri, credentials, settings);
            using (Log.PerformanceBlock("Authenticating with collection at {0}", projectCollectionUri))
            {
                await connection.ConnectAsync(cancellationToken);
            }

            return connection;
        }

        private static IPublicClientApplication GetOrCreateMsalApp()
        {
            lock (_msalLock)
            {
                if (_msalApp == null)
                {
                    // Get the app data folder for token cache
                    var cacheDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Microsoft", "TeamMate", "MsalCache");
                    
                    Directory.CreateDirectory(cacheDirectory);
                    var cacheFileName = Path.Combine(cacheDirectory, "msal.cache");

                    // Build the MSAL public client application with persistent cache
                    _msalApp = PublicClientApplicationBuilder.Create(ClientId)
                        .WithAuthority(Authority)
                        .WithRedirectUri("http://localhost")
                        .Build();

                    // Register the token cache serialization
                    RegisterTokenCache(_msalApp.UserTokenCache, cacheFileName);
                }

                return _msalApp;
            }
        }

        private static void RegisterTokenCache(ITokenCache tokenCache, string cacheFilePath)
        {
            tokenCache.SetBeforeAccess(notificationArgs =>
            {
                // Read cache from file
                if (File.Exists(cacheFilePath))
                {
                    try
                    {
                        byte[] cacheData = File.ReadAllBytes(cacheFilePath);
                        notificationArgs.TokenCache.DeserializeMsalV3(cacheData);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
                    {
                        // Ignore cache read errors, will just re-authenticate
                    }
                }
            });

            tokenCache.SetAfterAccess(notificationArgs =>
            {
                // Write cache to file if it has changed
                if (notificationArgs.HasStateChanged)
                {
                    try
                    {
                        byte[] cacheData = notificationArgs.TokenCache.SerializeMsalV3();
                        File.WriteAllBytes(cacheFilePath, cacheData);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
                    {
                        // Ignore cache write errors
                    }
                }
            });
        }
        private static VssClientCredentialStorage GetVssClientCredentialStorage(double tokenLeaseInSeconds)
        {
            // TODO: Restore custom caching storage for .NET 9 - using in-memory storage for now
            // The Azure DevOps SDK API has changed significantly and requires a new implementation
            // For now, use in-memory storage which will work but won't persist across restarts
            return new VssClientCredentialStorage("TeamMate", new InMemoryTokenStorage());
        }
        
        // Temporary in-memory token storage for .NET 9 migration
        private class InMemoryTokenStorage : Microsoft.VisualStudio.Services.Common.TokenStorage.VssTokenStorage
        {
            // Simple in-memory dictionary for tokens - not persisted
            private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> tokens = new();
            
            private static string GetTokenKeyString(Microsoft.VisualStudio.Services.Common.TokenStorage.VssTokenKey tokenKey)
            {
                // Create a unique key from token key's hash code
                // VssTokenKey is a value type, so its hash code is based on its content
                return tokenKey.GetHashCode().ToString();
            }
            
            protected override Microsoft.VisualStudio.Services.Common.TokenStorage.VssToken AddToken(
                Microsoft.VisualStudio.Services.Common.TokenStorage.VssTokenKey tokenKey, string tokenValue)
            {
                string key = GetTokenKeyString(tokenKey);
                tokens[key] = tokenValue;
                return null; // Return null - credential storage will handle token object
            }
            
            protected override Microsoft.VisualStudio.Services.Common.TokenStorage.VssToken RetrieveToken(
                Microsoft.VisualStudio.Services.Common.TokenStorage.VssTokenKey tokenKey)
            {
                return null; // Let SDK manage tokens
            }
            
            protected override bool RemoveToken(Microsoft.VisualStudio.Services.Common.TokenStorage.VssTokenKey tokenKey)
            {
                string key = GetTokenKeyString(tokenKey);
                return tokens.TryRemove(key, out _);
            }
            
            public override System.Collections.Generic.IEnumerable<Microsoft.VisualStudio.Services.Common.TokenStorage.VssToken> RetrieveAll(string tokenType)
            {
                return System.Linq.Enumerable.Empty<Microsoft.VisualStudio.Services.Common.TokenStorage.VssToken>();
            }
            
            public override bool RemoveAll()
            {
                tokens.Clear();
                return true;
            }
            
            public override string RetrieveTokenSecret(Microsoft.VisualStudio.Services.Common.TokenStorage.VssToken token)
            {
                return null;
            }
            
            public override bool SetTokenSecret(Microsoft.VisualStudio.Services.Common.TokenStorage.VssToken token, string tokenValue)
            {
                return true;
            }
            
            public override bool RemoveTokenSecret(Microsoft.VisualStudio.Services.Common.TokenStorage.VssToken token)
            {
                return true;
            }
            
            public override string GetProperty(Microsoft.VisualStudio.Services.Common.TokenStorage.VssToken token, string propertyName)
            {
                return null;
            }
            
            public override bool SetProperty(Microsoft.VisualStudio.Services.Common.TokenStorage.VssToken token, string propertyName, string value)
            {
                return true;
            }
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
                await ChaosMonkey.ChaosAsync(ChaosScenarios.ConnectToVsts);
                var connection = await this.ConnectAsync(projectInfo.ProjectCollectionUri, cancellationToken);

                WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();
                WorkItemTrackingBatchHttpClient batchWitClient = connection.GetClient<WorkItemTrackingBatchHttpClient>();
                ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();
                GitHttpClient gitClient = connection.GetClient<GitHttpClient>();
                GraphHttpClient graphClient = connection.GetClient<GraphHttpClient>();

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
                projectContext.GitHttpClient = gitClient;
                projectContext.GraphClient = graphClient;
                projectContext.WorkItemTypes = workItemTypeInfos;
                projectContext.ProjectInfo = projectInfo;
                projectContext.Identity = identity;
                projectContext.HyperlinkFactory = new HyperlinkFactory(projectInfo.ProjectCollectionUri, project.Name);
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
