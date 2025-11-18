using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Tools.TeamMate.Foundation.Shell;
using Microsoft.Tools.TeamMate.Foundation.Threading;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class WorkItemQueryViewModel : QueryViewModelBase, ICommandProvider
    {
        private ICollection<WorkItemRowViewModel> workItems;
        private WorkItemQueryExpandedResult result;

        private SingleTaskRunner sharedRefreshTask = new SingleTaskRunner();

        public event EventHandler WorkItemCollectionChanged;

        public WorkItemQueryReference Reference { get; set; }

        public BuiltInTileType BuiltInTileType { get; set; }

        public bool CanOpenInWebAccess
        {
            get { return this.Reference != null; }
        }

        public ICollection<WorkItemRowViewModel> WorkItems
        {
            get { return this.workItems; }
            private set
            {
                var oldWorkItems = this.workItems;
                if (SetProperty(ref this.workItems, value))
                {
                    if (oldWorkItems != null)
                    {
                        foreach (var item in oldWorkItems)
                        {
                            Unregister(item);
                        }
                    }

                    if (this.workItems != null)
                    {
                        foreach (var item in this.workItems)
                        {
                            Register(item);
                        }
                    }
                }
            }
        }

        protected void Register(WorkItemRowViewModel item)
        {
            item.PropertyChanged += HandleWorkItemPropertyChanged;
        }

        protected void Unregister(WorkItemRowViewModel item)
        {
            item.PropertyChanged -= HandleWorkItemPropertyChanged;
        }

        private void HandleWorkItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRead")
            {
                InvalidateUnreadItemCount();
            }
        }

        public WorkItemQueryExpandedResult Result
        {
            get { return this.result; }
            private set { SetProperty(ref this.result, value); }
        }

        public ProjectInfo ProjectInfo { get; private set; }

        public override Task RefreshAsync(NotificationScope notificationScope = null)
        {
            return this.sharedRefreshTask.Run(() => DoRefreshAsync(notificationScope));
        }

        [Import]
        public SessionService SessionService { get; set; }

        private async Task DoRefreshAsync(NotificationScope notificationScope)
        {
            var projectContext = this.SessionService.Session.ProjectContext;

            TaskContext taskContext = new TaskContext();
            taskContext.ReportsProgress = false;

            using (this.ProgressContext = taskContext)
            {
                try
                {
                    FireQueryExecuting();

                    // TODO: Prevent multiple refreshes at the same time?
                    WorkItemQuery query = CreateWorkItemQuery();
                    WorkItemQueryExpandedResult queryResult = null;

                    if (query != null)
                    {
                        query.ProjectName = projectContext.ProjectName;
                        query.RequiredFields = projectContext.RequiredWorkItemFieldNames;

                        await ChaosMonkey.ChaosAsync(ChaosScenarios.WorkItemQueryExecution);

                        List<Task> tasks = new List<Task>();

                        var queryAsyncTask = projectContext.WorkItemTrackingClient.QueryAsync(query);
                        tasks.Add(queryAsyncTask);

                        Task<QueryHierarchyItem> getQueryTask = null;
                        if (query.QueryId != Guid.Empty)
                        {
                            getQueryTask = projectContext.WorkItemTrackingClient.GetQueryAsync(query.ProjectName, query.QueryId.ToString(), QueryExpand.All);
                            tasks.Add(getQueryTask);
                        }

                        await Task.WhenAll(tasks.ToArray());

                        queryResult = queryAsyncTask.Result;
                        if (getQueryTask != null)
                        {
                            queryResult.QueryHierarchyItem = getQueryTask.Result;
                        }

                        OnQueryCompleted(projectContext, queryResult, notificationScope);
                    }
                    else
                    {
                        ItemCount = 0;
                        Result = null;
                        ProjectInfo = null;
                        WorkItems = new List<WorkItemRowViewModel>();
                        InvalidateUnreadItemCount();
                    }
                }
                catch (Exception e)
                {
                    ItemCount = 0;
                    Result = null;
                    ProjectInfo = null;
                    WorkItems = null;
                    InvalidateUnreadItemCount();
                    taskContext.Fail(e);
                }
                finally
                {
                    FireQueryExecuted();
                }
            }
        }

        private void OnQueryCompleted(ProjectContext projectContext, WorkItemQueryExpandedResult queryResult, NotificationScope notificationScope)
        {
            DateTime? previousUpdate = LastUpdated;

            ItemCount = queryResult.WorkItems.Count;
            ProjectInfo = projectContext.ProjectInfo;
            
            LastUpdated = queryResult.QueryResult.AsOf;
            Result = queryResult;
            WorkItems = CreateWorkItemViewModels(projectContext, queryResult);
            InvalidateUnreadItemCount();

            string storedQueryName = (queryResult.QueryHierarchyItem != null) ? queryResult.QueryHierarchyItem.Name : null;
            if (!String.IsNullOrEmpty(storedQueryName))
            {
                // If this was a stored query, refresh the name, which might have changed...
                this.Name = storedQueryName;
            }

            if (ShowNotifications)
            {
                IEnumerable<WorkItem> modifiedItems;

                if (this.SettingsService.DeveloperSettings.DebugAllNotifications)
                {
                    modifiedItems = queryResult.WorkItems;
                }
                else
                {
                    modifiedItems = workItems.Where(wi => wi.ChangedDate.IsAfter(previousUpdate) && !wi.IsRead).Select(vm => vm.WorkItem).ToArray();
                }

                if (modifiedItems.Any())
                {
                    this.ToastNotificationService.QueueNotifications(modifiedItems, previousUpdate, notificationScope);
                }
            }
        }

        [Import]
        public ToastNotificationService ToastNotificationService { get; set; }

        [Import]
        public SettingsService SettingsService { get; set; }


        protected void InvalidateUnreadItemCount()
        {
            this.UnreadItemCount = (WorkItems != null) ? WorkItems.Count(wi => !wi.IsRead) : 0;
        }

        private ICollection<WorkItemRowViewModel> CreateWorkItemViewModels(ProjectContext projectContext, WorkItemQueryExpandedResult result)
        {
            var viewModels = result.WorkItems.Select(r => CreateWorkItemViewModel(r)).ToList();

            foreach (var fieldName in WorkItemRowViewModel.OptionalWorkItemFields)
            {
                if (ResultsHaveField(result, fieldName))
                {
                    foreach (var viewModel in viewModels)
                    {
                        if (viewModel.WorkItem.HasField(fieldName))
                        {
                            viewModel[fieldName] = viewModel.WorkItem.Fields[fieldName];
                        }
                    }
                }
            }

            // Other optional field assignment
            string subStateField = WorkItemConstants.VstsFields.SubState;
            if (ResultsHaveField(result, subStateField))
            {
                foreach (var viewModel in viewModels)
                {
                    string subState;
                    if (viewModel.WorkItem.TryGetField(subStateField, out subState))
                    {
                        viewModel.SubState = subState;
                    }
                }
            }

            string priorityField = WorkItemConstants.VstsFields.Priority;
            if (ResultsHaveField(result, priorityField))
            {
                foreach (var viewModel in viewModels)
                {
                    long priority;
                    if (viewModel.WorkItem.TryGetField(priorityField, out priority))
                    {
                        viewModel.Priority = (int)priority;
                        viewModel.IsHighPriority = projectContext.IsWorkItemPriorityHigh((int)priority);
                    }
                }
            }

            string resolvedReasonField = WorkItemConstants.VstsFields.ResolvedReason;
            if (ResultsHaveField(result, resolvedReasonField))
            {
                foreach (var viewModel in viewModels)
                {
                    string resolvedReason;
                    if (viewModel.WorkItem.TryGetField(resolvedReasonField, out resolvedReason))
                    {
                        viewModel.ResolvedReason = resolvedReason;
                    }
                }
            }

            return viewModels;
        }

        private static bool ResultsHaveField(WorkItemQueryExpandedResult result, string fieldName)
        {
            bool hasField = result.WorkItems.Any(wi => wi.HasField(fieldName));
            return hasField;
        }

        private WorkItemRowViewModel CreateWorkItemViewModel(WorkItem workItem)
        {
            var viewModel = ViewModelFactory.Create<WorkItemRowViewModel>();
            viewModel.WorkItem = workItem;
            return viewModel;
        }

        protected virtual WorkItemQuery CreateWorkItemQuery()
        {
            if (this.Reference != null)
            {
                return new WorkItemQuery
                {
                    QueryId = this.Reference.Id
                };
            }

            return CreateBuiltInQuery();
        }

        public void RegisterBindings(CommandBindingCollection bindings)
        {
            bindings.Add(TeamMateCommands.OpenQueryInWebAccess, OpenInWebAccess, () => CanOpenInWebAccess);
        }

        public void OpenInWebAccess()
        {
            var queryItem = this.Result?.QueryHierarchyItem;
            if (queryItem != null)
            {
                HyperlinkFactory factory = this.SessionService.Session.ProjectContext.HyperlinkFactory;
                Uri url = factory.GetWorkItemQueryUrl(queryItem);
                ExternalWebBrowser.Launch(url);
            }
        }

        [Import]
        public StatusService StatusService { get; set; }

        [Import]
        public TrackingService TrackingService { get; set; }

        private WorkItemQuery CreateBuiltInQuery()
        {
            var pc = this.SessionService.Session.ProjectContext;
            WorkItemQuery query = null;

            switch (this.BuiltInTileType)
            {
                case BuiltInTileType.AssignedToMe:
                    query = new WorkItemQuery();
                    query.ProjectName = pc.ProjectName;
                    query.Wiql = @"SELECT id from workitems where [System.TeamProject] = @project AND [System.AssignedTo]=@Me";
                    return query;

                case BuiltInTileType.BugsToMe:
                    query = new WorkItemQuery();
                    query.ProjectName = pc.ProjectName;
                    query.Wiql = @"SELECT id from workitems where [System.TeamProject] = @project AND [System.AssignedTo]=@Me AND  [System.WorkItemType] IN GROUP 'Microsoft.BugCategory'";
                    return query;

                case BuiltInTileType.Flagged:
                    var ids = this.TrackingService.GetFlaggedWorkItemIds();
                    if (ids.Any())
                    {
                        query = new WorkItemQuery();
                        query.ProjectName = pc.ProjectName;

                        // KLUDGE: instead of just being able to pass in the ids for flagged work items, we need to create a query with the ids
                        // in the WIQL. The reason is that, if we lose access to one of those work items, then the query result iteration fails
                        // when one of these items is iterated over with DeniedOrNotExistException. Passing the ids in the WHERE clause seems
                        // to mitigate that.

                        // query.Ids = ids;
                        query.Wiql = @"SELECT id from workitems where id in (" + String.Join(", ", ids) + ")";

                        return query;
                    }
                    else
                    {
                        return null;
                    }

                default:
                    throw new NotSupportedException("Unsupported tile type: " + this.BuiltInTileType);
            }
        }

        protected void FireWorkItemCollectionChanged()
        {
            WorkItemCollectionChanged?.Invoke(this, EventArgs.Empty);

        }
    }
}
