using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows.Transfer;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;


namespace Microsoft.Tools.TeamMate.Utilities
{
    public static class DataObjectFactory
    {
        public static IDataObject CreateDraggableItem(WorkItem workItem, HyperlinkFactory factory)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            return CopyHyperlink(workItem, factory);
        }

        public static IDataObject CreateDraggableItem(ICollection<WorkItem> workItems, HyperlinkFactory factory)
        {
            Assert.ParamIsNotNull(workItems, "workItems");

            if (workItems.Count == 0)
            {
                throw new ArgumentException("There must be at least a single work item");
            }
            else if (workItems.Count == 1)
            {
                return CreateDraggableItem(workItems.First(), factory);
            }
            else
            {
                var workItemReferences = workItems.Select(wi => wi.GetReference()).ToArray();
                return CreateDraggableItem(workItemReferences);
            }
        }

        public static IDataObject CreateDraggableItem(WorkItemReference[] rworkItems)
        {
            DataObject dataObject = new DataObject();
            dataObject.SetData(typeof(WorkItemReference[]), rworkItems);
            return dataObject;
        }

        public static IDataObject CreateDraggableItem(PullRequestRowViewModel pullRequest)
        {
            Assert.ParamIsNotNull(pullRequest, "pullRequest");

            return CopyHyperlink(pullRequest);
        }

        public static IDataObject CopyHyperlink(PullRequestRowViewModel pullRequest)
        {
            Assert.ParamIsNotNull(pullRequest, "pullRequest");

            DataObject dataObject = new DataObject();
            dataObject.SetUri(pullRequest.Url, pullRequest.GetFullTitle());
            return dataObject;
        }

        public static IDataObject CopyHyperlink(WorkItem workItem, HyperlinkFactory factory)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            var url = factory.GetWorkItemUrl(workItem);
            return CopyHyperlink(url, workItem.GetFullTitle(), workItem.GetReference());
        }

        private static IDataObject CopyHyperlink(Uri uri, string fullTitlte, WorkItemReference reference)
        {
            DataObject dataObject = new DataObject();
            dataObject.SetUri(uri, fullTitlte);
            dataObject.SetData(typeof(WorkItemReference), reference);

            return dataObject;
        }

        public static IDataObject CopyId(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");
            return CopyId(workItem.GetReference());
        }

        public static IDataObject CopyId(WorkItemReference reference)
        {
            Assert.ParamIsNotNull(reference, "reference");

            object data = reference.Id.ToString();
            DataObject dataObject = new DataObject(data);
            dataObject.SetData(typeof(WorkItemReference), reference);
            return dataObject;
        }

        public static IDataObject CopyTitle(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");
            return CopyTitle(workItem.GetFullTitle(), workItem.GetReference());
        }

        private static IDataObject CopyTitle(string title, WorkItemReference reference)
        {
            Assert.ParamIsNotNull(title, "title");
            Assert.ParamIsNotNull(reference, "reference");

            object data = title;
            DataObject dataObject = new DataObject(data);
            dataObject.SetData(typeof(WorkItemReference), reference);
            return dataObject;
        }

        public static IDataObject CopyDetails(WorkItem workItem)
        {
            Assert.ParamIsNotNull(workItem, "workItem");

            // TODO: Implement "Copy Details" action (e.g. using WorkItemPrinter?)
            object data = workItem.GetFullTitle();
            DataObject dataObject = new DataObject(data);
            dataObject.SetData(typeof(WorkItemReference), workItem.GetReference());
            return dataObject;
        }
    }
}
