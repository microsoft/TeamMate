using Microsoft.Tools.TeamMate.Converters;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.Windows;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class WorkItemsPageViewModel : PageViewModelBase, ICommandProvider, IFilterable, IGlobalCommandProvider
    {
        private WorkItemQueryViewModel query;
        private List<WorkItemRowViewModel> workItemList;
        private ListCollectionView collectionView;
        private ListViewModel workItems;
        private TileInfo tileInfo;

        private Dictionary<string, ListFieldInfo> optionalFields = new Dictionary<string, ListFieldInfo>();

        public WorkItemsPageViewModel()
        {
            this.CommandBarType = CommandBarType.WorkItems;
            this.workItemList = new List<WorkItemRowViewModel>();
            this.collectionView = new ListCollectionView(this.workItemList);
            this.workItems = CreateWorkItemListViewModel(this.collectionView);
            this.workItems.FilterApplied += HandleFilterApplied;

            var field = ListFieldInfo.Create<string>("[" + WorkItemConstants.VstsFields.SubState + "]", "Sub State");
            field.GroupConverter = TeamMateConverters.ValueOrUndefined;
            optionalFields[WorkItemConstants.VstsFields.SubState] = field;

            field = ListFieldInfo.Create<int>("[" + WorkItemConstants.VstsFields.Priority + "]", "Priority");
            field.GroupConverter = TeamMateConverters.PriorityFormat;
            optionalFields[WorkItemConstants.VstsFields.Priority] = field;

            field = ListFieldInfo.Create<string>("[" + WorkItemConstants.VstsFields.ResolvedReason + "]", "Resolution");
            field.GroupConverter = TeamMateConverters.ValueOrUndefined;
            optionalFields[WorkItemConstants.VstsFields.ResolvedReason] = field;

            field = ListFieldInfo.Create<string>("[" + WorkItemConstants.VstsFields.ResolvedBy + "]", "Resolved By");
            field.GroupConverter = TeamMateConverters.ValueOrUndefined;
            optionalFields[WorkItemConstants.VstsFields.ResolvedBy] = field;

            field = ListFieldInfo.Create<DateTime>("[" + WorkItemConstants.VstsFields.ResolvedDate + "]", "Resolved Date");
            field.GroupConverter = TeamMateConverters.FriendlyDate;
            optionalFields[WorkItemConstants.VstsFields.ResolvedDate] = field;

            this.GlobalCommandBindings = new CommandBindingCollection();
            this.GlobalCommandBindings.Add(TeamMateCommands.Refresh, Refresh);
            this.GlobalCommandBindings.Add(TeamMateCommands.MarkAllAsRead, MarkAllAsRead);

            // TODO: KLUDGE: Re-registering commands that get registered through Query.RegisterBindings(). Remove duplication
            this.GlobalCommandBindings.Add(TeamMateCommands.OpenQueryInWebAccess, () => Query.OpenInWebAccess(), () => HasQuery() && Query.CanOpenInWebAccess);
            this.GlobalCommandBindings.Add(TeamMateCommands.SendEmailForQuery, () => Query.SendMail(), HasQuery);
            this.GlobalCommandBindings.Add(TeamMateCommands.ReplyAllInQueryWithEmail, () => Query.ReplyAll(), HasQuery);

            this.workItems.OrderByFieldChanged += HandleOrderByFieldPropertyChanged;
            this.workItems.FilterByFieldChanged += HandleFilterByFieldPropertyChanged;

            foreach (var filter in this.workItems.Filters)
            {
                filter.PropertyChanged += HandleFilterByFieldPropertyChanged;
            }
        }

        [Import]
        public ProjectDataService ProjectDataService { get; set; }


        private void HandleOrderByFieldPropertyChanged(object sender, EventArgs e)
        {
            this.tileInfo.OrderByFieldName = this.WorkItems.OrderByField.PropertyName;
            this.tileInfo.FireChanged();
        }

        private void HandleFilterByFieldPropertyChanged(object sender, EventArgs e)
        {
            ListViewFilter filter = (ListViewFilter)sender;
            if (filter.IsSelected)
            {
                this.tileInfo.FilterByFieldName = filter.Name;
                this.tileInfo.FireChanged();
            }
        }

        private bool HasQuery()
        {
            return this.Query != null;
        }

        private void HandleFilterApplied(object sender, EventArgs e)
        {
            InvalidatePageTitle();
        }

        public void RegisterBindings(CommandBindingCollection commands)
        {
            if (Query != null)
            {
                Query.RegisterBindings(commands);
            }

            commands.Add(TeamMateCommands.CopyId, CopyId, HasSingleSelection);
            commands.Add(TeamMateCommands.CopyTitle, CopyTitle, HasSingleSelection);
            commands.Add(TeamMateCommands.CopyHyperlink, CopyHyperlink, HasSingleSelection);
            commands.Add(ApplicationCommands.Copy, CopyHyperlink, HasSingleSelection);
            commands.Add(TeamMateCommands.Flag, ToggleSelectionFlag, HasSelection);
            commands.Add(TeamMateCommands.FindInOutlook, SearchInOutlook, HasSelection);
            commands.Add(TeamMateCommands.ReplyWithEmail, ReplyWithEmail, HasSelection);
            commands.Add(TeamMateCommands.MarkAsRead, MarkAsRead, HasSelection);
            commands.Add(TeamMateCommands.MarkAsUnread, MarkAsUnread, HasSelection);
            commands.Add(TeamMateCommands.EditTags, EditTags, HasSelection);
        }

        public async void Refresh()
        {
            if (Query != null)
            {
                await Query.RefreshAsync();
            }
        }

        private async void EditTags()
        {
            var selectedItems = GetSelectedItems();
            var workItems = selectedItems.Select(row => row.WorkItem).ToList();
            ICollection<string> commonTags = FindCommonTags(workItems);

            WorkItemAddTagsDialogViewModel dialogViewModel = new WorkItemAddTagsDialogViewModel(commonTags);
            WorkItemAddTagsDialog dialog = new WorkItemAddTagsDialog();
            dialog.DataContext = dialogViewModel;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.Owner = View.GetWindow(this);
            dialog.ShowDialog();

            // If all editing is done and dialog was not cancelled
            if (dialog.DialogResult == true)
            {
                var tagsToRemove = dialogViewModel.GetTagsToRemove();
                var tagsToAdd = dialogViewModel.GetTagsToAdd();

                var updateRequests = new List<WorkItemBatchUpdateRequest>();

                foreach (var workItem in workItems)
                {
                    // TODO: IMPORTANT: This could be a problem since we are using the tag values of a work item that might have changed in the server
                    // and we could be losing new tag values that were added or already removed from the server work item.
                    // How do we want to make this better?
                    var originalTags = workItem.Tags();

                    bool updated = false;
                    var updatedTags = new List<string>(workItem.Tags());
                    foreach (var item in tagsToRemove)
                    {
                        // TODO: Not case insensitive?
                        updated = updatedTags.Remove(item);
                    }

                    foreach (var newTag in tagsToAdd)
                    {
                        if (!updatedTags.Contains(newTag, StringComparer.OrdinalIgnoreCase))
                        {
                            updatedTags.Add(newTag);
                            updated = true;
                        }
                    }

                    if (updated)
                    {
                        string tagFieldValue = String.Join(WorkItemConstants.TagSeparator, updatedTags);

                        updateRequests.Add(new WorkItemBatchUpdateRequest
                        {
                            Id = workItem.Id.Value,
                            Body = new JsonPatchDocument()
                            {
                                new JsonPatchOperation
                                {
                                    Operation = VisualStudio.Services.WebApi.Patch.Operation.Add,
                                    Path = "/fields/" + WorkItemConstants.CoreFields.Tags,
                                    Value = tagFieldValue
                                }
                            }
                        });
                    }
                }

                if (updateRequests.Any())
                {
                    try
                    {
                        using (this.StatusService.BusyIndicator())
                        {
                            var batchClient = this.SessionService.Session.ProjectContext.WorkItemTrackingBatchClient;
                            var results = await batchClient.BatchUpdateWorkItemsAsync(updateRequests);

                            // KLUDGE: To refresh the work item row view models in the easiest way.
                            // Ideally, we can invaliadte each row, but the returned results might not return all the required fields that we
                            // want... Maybe re-query only those work items to make this better
                            this.Refresh();
                        }
                    }
                    catch (Exception e)
                    {
                        this.MessageBoxService.ShowError(e);
                        Log.Error(e);
                    }
                }
            }
        }

        [Import]
        public MessageBoxService MessageBoxService { get; set; }


        [Import]
        public SessionService SessionService { get; set; }


        [Import]
        public StatusService StatusService { get; set; }


        private static ICollection<string> FindCommonTags(ICollection<WorkItem> workItems)
        {
            IEnumerable<string> commonTags = null;
            foreach (var workItem in workItems)
            {
                if (commonTags == null)
                {
                    commonTags = workItem.Tags();
                }
                else
                {
                    commonTags = commonTags.Intersect(workItem.Tags(), WorkItemConstants.TagComparer);
                }
            }

            return commonTags.ToList();
        }

        private void MarkAsUnread()
        {
            using (this.ProjectDataService.DeferReadWorkItemsUpdate())
            {
                foreach (WorkItemRowViewModel workItem in GetSelectedItems())
                {
                    workItem.IsRead = false;
                }
            }
        }

        private void MarkAsRead()
        {
            using (this.ProjectDataService.DeferReadWorkItemsUpdate())
            {
                foreach (WorkItemRowViewModel workItem in GetSelectedItems())
                {
                    workItem.IsRead = true;
                }
            }
        }

        private void MarkAllAsRead()
        {
            using (this.ProjectDataService.DeferReadWorkItemsUpdate())
            {
                var allItems = WorkItems.CollectionView.OfType<WorkItemRowViewModel>();
                foreach (var item in allItems)
                {
                    item.IsRead = true;
                }
            }
        }

        [Import]
        public CollaborationService CollaborationService { get; set; }


        private async void ReplyWithEmail()
        {
            try
            {
                using (this.StatusService.BusyIndicator())
                {
                    if (HasSingleSelection())
                    {
                        WorkItemRowViewModel workItem = GetSelectedItem();
                        await this.CollaborationService.ReplyWithMailAsync(workItem.WorkItem);
                    }
                    else
                    {
                        var items = GetSelectedItems().Select(wii => wii.WorkItem).ToArray();
                        this.CollaborationService.ReplyAllWithMail(items);
                    }
                }
            }
            catch (Exception e)
            {
                this.MessageBoxService.ShowError(e);
            }
        }

        private void SearchInOutlook()
        {
            WorkItemRowViewModel workItem = GetSelectedItem();
            this.CollaborationService.SearchInOutlook(workItem.WorkItem);
        }

        private void CopyHyperlink()
        {
            WorkItemRowViewModel workItem = GetSelectedItem();
            var factory = this.SessionService.Session.ProjectContext.HyperlinkFactory;
            Clipboard.SetDataObject(DataObjectFactory.CopyHyperlink(workItem.WorkItem, factory));
        }

        private void CopyId()
        {
            WorkItemRowViewModel workItem = GetSelectedItem();
            Clipboard.SetDataObject(DataObjectFactory.CopyId(workItem.WorkItem));
        }

        private void CopyTitle()
        {
            WorkItemRowViewModel workItem = GetSelectedItem();
            Clipboard.SetDataObject(DataObjectFactory.CopyTitle(workItem.WorkItem));
        }

        private bool HasSelection()
        {
            return WorkItems.SelectedItems.Any();
        }

        private bool HasSingleSelection()
        {
            return WorkItems.SelectedItems.Count == 1;
        }

        private WorkItemRowViewModel GetSelectedItem()
        {
            return (WorkItemRowViewModel)WorkItems.SelectedItems.FirstOrDefault();
        }

        private ICollection<WorkItemRowViewModel> GetSelectedItems()
        {
            return WorkItems.SelectedItems.OfType<WorkItemRowViewModel>().ToArray();
        }

        private void ToggleSelectionFlag()
        {
            using (this.ProjectDataService.DeferFlaggedWorkItemsUpdate())
            {
                // IMPORTANT: Materialize selection. In the flagged page, the collection will change while being enumerated
                // and items are unflagged
                var selectedItems = GetSelectedItems().ToArray();
                foreach (WorkItemRowViewModel workItem in selectedItems)
                {
                    workItem.IsFlagged = !workItem.IsFlagged;
                }
            }
        }

        public WorkItemQueryViewModel Query
        {
            get { return this.query; }
            set
            {
                WorkItemQueryViewModel oldQuery = this.query;
                if (SetProperty(ref this.query, value))
                {
                    if (oldQuery != null)
                    {
                        oldQuery.QueryExecuted -= HandleQueryExecuted;
                        oldQuery.WorkItemCollectionChanged -= HandleWorkItemCollectionChanged;
                    }

                    if (this.query != null)
                    {
                        query.QueryExecuted += HandleQueryExecuted;
                        query.WorkItemCollectionChanged += HandleWorkItemCollectionChanged;
                    }

                    InvalidateCollectionView();
                }
            }
        }

        public ListViewModel WorkItems
        {
            get { return this.workItems; }
        }

        private string pageTitle;

        public string PageTitle
        {
            get { return this.pageTitle; }
            set { SetProperty(ref this.pageTitle, value); }
        }

        public CommandBindingCollection GlobalCommandBindings
        {
            get; private set;
        }

        public TileInfo TileInfo
        {
            get { return this.tileInfo; }
            set
            {
                if (SetProperty(ref this.tileInfo, value))
                {
                    this.workItems.OrderByFieldName(this.TileInfo.OrderByFieldName);
                    this.workItems.FilterByFieldName(this.TileInfo.FilterByFieldName);
                }
            }
        }

        private void InvalidateCollectionView()
        {
            this.collectionView.Dispatcher.InvokeHere(delegate ()
            {
                RefreshListFields();

                this.workItemList.Clear();
                if (this.query != null && this.query.WorkItems != null)
                {
                    this.workItemList.AddRange(this.query.WorkItems);
                }
                this.collectionView.Refresh();

                // IMPORTANT, do this AFTER results are loaded
                InvalidatePageTitle();
            });
        }

        private void InvalidatePageTitle()
        {
            string pageTitle = null;

            if (Query != null)
            {
                if (Query.ItemCount > 0)
                {
                    int displayCount = this.collectionView.Count;
                    if (displayCount < Query.ItemCount)
                    {
                        pageTitle = String.Format("{0} ({1} of {2})", Query.Name, displayCount, Query.ItemCount);
                    }
                    else
                    {
                        pageTitle = String.Format("{0} ({1})", Query.Name, Query.ItemCount);
                    }
                }
                else
                {
                    pageTitle = Query.Name;
                }
            }

            this.PageTitle = pageTitle;
            this.Title = pageTitle;
        }

        private void RefreshListFields()
        {
            var fields = this.WorkItems.Fields;
            bool removedSelected = false;
            foreach (var pair in optionalFields)
            {
                var fieldName = pair.Key;
                var field = pair.Value;

                bool hasNoField = query == null || query.Result == null || !query.Result.WorkItems.Any(wi => wi.HasField(fieldName));
                if (hasNoField)
                {
                    if (fields.Contains(field))
                    {
                        removedSelected |= field.IsSelected;
                        fields.Remove(field);
                    }
                }
                else
                {
                    if (!fields.Contains(field))
                    {
                        // Insert the field ordered based on field name
                        var nextField = fields.FirstOrDefault(f => f.Name.CompareTo(field.Name) > 0);
                        if (nextField != null)
                        {
                            fields.Insert(fields.IndexOf(nextField), field);
                        }
                        else
                        {
                            fields.Add(field);
                        }
                    }
                }
            }

            if (removedSelected && fields.Any())
            {
                fields[0].IsSelected = true;
            }
        }

        private void HandleQueryExecuted(object sender, EventArgs e)
        {
            InvalidateCollectionView();
        }

        private void HandleWorkItemCollectionChanged(object sender, EventArgs e)
        {
            InvalidateCollectionView();
        }

        public void ApplyTextFilter(string filterText)
        {
            bool wasFiltering = (this.workItems.SearchFilter != null);

            SearchExpression expression = SearchExpression.Parse(filterText);
            Predicate<object> searchFilter = (!expression.IsEmpty) ? expression.Matches : (Predicate<object>)null;
            this.workItems.SearchFilter = searchFilter;

            if (searchFilter != null)
            {
                TextFilterApplied?.Invoke(this, expression);

                if (!wasFiltering)
                {
                    Telemetry.Event(TelemetryEvents.FilterApplied);
                }
            }
        }

        public event EventHandler<SearchExpression> TextFilterApplied;

        private static ListViewModel CreateWorkItemListViewModel(ICollectionView collectionView)
        {
            ListViewModel listViewModel = new ListViewModel(collectionView);

            listViewModel.Fields.Add(ListFieldInfo.Create<string>("AreaPath", "Area"));

            var assignedToField = ListFieldInfo.Create<string>("AssignedTo", "Assigned To");
            assignedToField.GroupConverter = TeamMateConverters.AssignedTo;
            listViewModel.Fields.Add(assignedToField);

            listViewModel.Fields.Add(ListFieldInfo.Create<string>("ChangedBy", "Changed By"));

            var dateField = ListFieldInfo.Create<DateTime>("ChangedDate", "Changed Date");
            listViewModel.Fields.Add(dateField);

            listViewModel.Fields.Add(ListFieldInfo.Create<string>("CreatedBy", "Created By"));
            listViewModel.Fields.Add(ListFieldInfo.Create<DateTime>("CreatedDate", "Created Date"));

            listViewModel.Fields.Add(ListFieldInfo.Create<string>("IterationPath", "Iteration"));

            listViewModel.Fields.Add(ListFieldInfo.Create<string>("State", "State"));
            listViewModel.Fields.Add(ListFieldInfo.Create<string>("Type", "Type"));

            listViewModel.Filters.Add(new ListViewFilter("All"));
            listViewModel.Filters.Add(new ListViewFilter("Unread", (x) => !((WorkItemRowViewModel)x).IsRead));

            listViewModel.OrderBy(dateField);
            listViewModel.ShowInGroups = true;

            return listViewModel;
        }

        public HyperlinkFactory HyperlinkFactory => this.SessionService.Session.ProjectContext.HyperlinkFactory;

        [Import]
        public WindowService WindowService { get; set; }

        public void OpenMany(ICollection<WorkItemRowViewModel> items)
        {
            if (this.WindowService.PromptShouldOpen(this, items.Count))
            {
                using (this.StatusService.BusyIndicator())
                {
                    foreach (var item in items)
                    {
                        item.Open();
                    }
                }
            }
        }

        public override void OnNavigatingTo()
        {
            ApplyTextFilter(null);
        }
    }
}
