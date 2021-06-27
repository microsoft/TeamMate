using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Xml;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public class WorkItemHtmlFormatter
    {
        private static readonly FieldInfo[] WellKnownTextFields = {
            new FieldInfo("Description", WorkItemConstants.CoreFields.Description),
            new FieldInfo("Repro Steps", WorkItemConstants.VstsFields.ReproSteps),
            new FieldInfo("Acceptance Criteria", WorkItemConstants.VstsFields.AcceptanceCriteria),
        };

        private static readonly FieldInfo IdField = new FieldInfo("ID", WorkItemConstants.CoreFields.Id);
        private static readonly FieldInfo TitleField = new FieldInfo("Title", WorkItemConstants.CoreFields.Title);

        private static readonly FieldInfo[] DefaultDisplayFields = {
            IdField,
            TitleField,
            new FieldInfo("Assigned To", WorkItemConstants.CoreFields.AssignedTo),
            new FieldInfo("State", WorkItemConstants.CoreFields.State),
            new FieldInfo("Tags", WorkItemConstants.CoreFields.Tags)
        };

        private IDictionary<string, WorkItemField> workItemFieldsByName;
        private HyperlinkFactory hyperlinkFactory;

        public WorkItemHtmlFormatter(IDictionary<string, WorkItemField> workItemFieldsByName, HyperlinkFactory hyperlinkFactory)
        {
            this.workItemFieldsByName = workItemFieldsByName;
            this.hyperlinkFactory = hyperlinkFactory;
        }

        public string NoThumbnail { get; set; }

        public bool OutlookMode { get; set; }

        public IDictionary<string, string> Thumbnails { get; set; }

        public void FormatWorkItem(WorkItemWithUpdates workItemWithUpdates, TextWriter writer)
        {
            Assert.ParamIsNotNull(workItemWithUpdates, "workItemWithUpdates");
            Assert.ParamIsNotNull(writer, "writer");

            XDocument doc = CreateXml(workItemWithUpdates);
            Transform(doc, EmailResources.EmailStylesheet, writer);
        }

        public void FormatResults(ICollection<WorkItem> workItems, TextWriter writer)
        {
            Assert.ParamIsNotNull(workItems, "workItems");
            Assert.ParamIsNotNull(writer, "writer");

            XDocument doc = CreateXml(workItems);
            Transform(doc, EmailResources.EmailStylesheet, writer);
        }

        public void FormatResults(WorkItemQueryExpandedResult results, TextWriter writer)
        {
            Assert.ParamIsNotNull(results, "results");
            Assert.ParamIsNotNull(writer, "writer");

            XDocument doc = CreateXml(results);
            Transform(doc, EmailResources.EmailStylesheet, writer);
        }

        private void Transform(XDocument doc, byte[] xslStyleSheet, TextWriter writer)
        {
            string xml = doc.ToString();


            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(XmlReader.Create(new MemoryStream(xslStyleSheet)));

            XsltArgumentList args = new XsltArgumentList();
            if (!String.IsNullOrEmpty(NoThumbnail))
            {
                args.AddParam("NoThumbnail", String.Empty, NoThumbnail);
            }

            if (OutlookMode)
            {
                args.AddParam("Mode", String.Empty, "Outlook");
            }

            // Execute the transform and output the results to a writer.
            xslt.Transform(doc.CreateReader(), args, writer);
        }

        private XDocument CreateXml(WorkItemWithUpdates workItemWithUpdates)
        {
            WorkItem workItem = workItemWithUpdates.WorkItem;
            XElement rootElement = new XElement("Discussion");

            XElement fieldsElement = new XElement("Fields");
            rootElement.Add(fieldsElement);

            foreach (var field in WellKnownTextFields)
            {
                object fieldValueObject;
                if (workItem.Fields.TryGetValue(field.ReferenceName, out fieldValueObject))
                {
                    string fieldValue = fieldValueObject != null ? fieldValueObject.ToString() : null;
                    if (!String.IsNullOrWhiteSpace(fieldValue))
                    {
                        fieldsElement.Add(new XElement("Field",
                            new XAttribute("ReferenceName", field.ReferenceName),
                            new XAttribute("Name", GetLocalizedName(field)),
                            new XCData(fieldValue)));
                    }
                }
            }

            rootElement.Add(WriteWorkItemCollection(new WorkItem[] { workItem }));

            foreach (WorkItemUpdate revision in GetHistoryUpdatesInReverseOrder(workItemWithUpdates.Updates))
            {
                string changedBy = revision.RevisedBy.Name;
                DateTime changedDate = revision.GetField<DateTime>(WorkItemConstants.CoreFields.ChangedDate).Value;
                string history = revision.GetField(WorkItemConstants.CoreFields.History);

                string thumbnail = null;
                if (Thumbnails != null && Thumbnails.TryGetValue(changedBy, out thumbnail))
                {
                    // We have a thumbnail filename
                }

                XElement revisionElement = new XElement("Revision",
                    new XElement("ChangedBy", changedBy),
                    new XElement("ChangedDate", changedDate.ToFriendlyShortDateString()),
                    (thumbnail != null) ? new XElement("Thumbnail", thumbnail) : null,
                    new XElement("History", new XCData(history))
                );

                rootElement.Add(revisionElement);
            }

            XDocument doc = new XDocument(rootElement);
            return doc;
        }

        private string GetLocalizedName(FieldInfo field)
        {
            WorkItemField workItemField;
            if(this.workItemFieldsByName.TryGetValue(field.ReferenceName, out workItemField))
            {
                return workItemField.Name;
            }

            return field.Name;
        }

        private static IEnumerable<WorkItemUpdate> GetHistoryUpdatesInReverseOrder(ICollection<WorkItemUpdate> updates)
        {
            foreach (var update in updates.Reverse())
            {
                WorkItemFieldUpdate historyUpdate;
                if (update.Fields != null && update.Fields.TryGetValue(WorkItemConstants.CoreFields.History, out historyUpdate) && historyUpdate.NewValue != null)
                {
                    yield return update;
                }
            }
        }

        private XDocument CreateXml(WorkItemQueryExpandedResult queryResult)
        {
            XElement rootElement = WriteWorkItemCollection(queryResult.WorkItems, queryResult);
            XDocument doc = new XDocument(rootElement);
            return doc;
        }

        private XDocument CreateXml(ICollection<WorkItem> workItems)
        {
            XElement rootElement = WriteWorkItemCollection(workItems);
            XDocument doc = new XDocument(rootElement);
            return doc;
        }

        private XElement WriteWorkItemCollection(ICollection<WorkItem> workItemCollection, WorkItemQueryExpandedResult queryResult = null)
        {
            QueryHierarchyItem query = queryResult?.QueryHierarchyItem;

            var displayFields = PrepareDisplayFields(queryResult);

            XElement rootElement = new XElement("WorkItemCollection");
            XElement infoElement = new XElement("Info");

            if (query != null)
            {
                var url = this.hyperlinkFactory.GetWorkItemQueryUrl(query);

                XElement queryElement = new XElement("Query", query.Name);
                queryElement.SetAttribute("URL", url);
                infoElement.Add(queryElement);
            }
            rootElement.Add(infoElement);

            rootElement.Add(new XElement("Fields",
                displayFields.Select(field => new XElement("Field",
                    new XAttribute("ReferenceName", field.ReferenceName),
                    field.Name
            ))));

            XElement workItemsElement = new XElement("WorkItems");
            rootElement.Add(workItemsElement);

            if (queryResult != null && !queryResult.IsFlatQuery)
            {
                foreach (var node in queryResult.WorkItemHierarchy.AllNodes)
                {
                    AddWorkItemElement(node.WorkItem, node.Level, displayFields, workItemsElement);
                }
            }
            else
            {
                foreach (WorkItem workItem in workItemCollection)
                {
                    int level = 0;
                    AddWorkItemElement(workItem, level, displayFields, workItemsElement);
                }
            }

            return rootElement;
        }

        private ICollection<FieldInfo> PrepareDisplayFields(WorkItemQueryExpandedResult queryResult)
        {
            List<FieldInfo> fieldInfos;
            QueryHierarchyItem item = queryResult?.QueryHierarchyItem;
            if (item != null)
            {
                fieldInfos = item.Columns.Select(c => new FieldInfo(c.Name, c.ReferenceName)).ToList();
            }
            else
            {
                var localizedDefaultDisplayFields = DefaultDisplayFields.Select(f => new FieldInfo(GetLocalizedName(f), f.ReferenceName));
                fieldInfos = new List<FieldInfo>(localizedDefaultDisplayFields);
            }

            PrepareDefaultFields(fieldInfos);
            return fieldInfos;
        }

        private void AddWorkItemElement(WorkItem workItem, int level, ICollection<FieldInfo> fields, XElement container)
        {
            XElement workItemElement = new XElement("WorkItem");
            var url = this.hyperlinkFactory.GetWorkItemUrl(workItem);
            workItemElement.SetAttribute("URL", url);

            if (level > 0)
            {
                workItemElement.SetAttribute("Indentation", level);
            }

            foreach (var f in fields)
            {
                object fieldValue;
                if(f.ReferenceName == WorkItemConstants.CoreFields.Id)
                {
                    // KLUDGE: Don't ask me why, but the System.Id field is not part of the fields collection
                    fieldValue = workItem.Id;
                }
                else
                {
                    workItem.Fields.TryGetValue(f.ReferenceName, out fieldValue);
                }

                workItemElement.Add(new XElement("Value", fieldValue));
            }

            container.Add(workItemElement);
        }

        private static void PrepareDefaultFields(IList<FieldInfo> fields)
        {
            // Ensure that both ID and Title exist, and move them to the front...
            EnsureFieldExists(fields, TitleField);
            EnsureFieldExists(fields, IdField);

            MoveToFront(fields, TitleField);
            MoveToFront(fields, IdField);
        }

        private static void EnsureFieldExists(IList<FieldInfo> fields, FieldInfo field)
        {
            if (!fields.Contains(field))
            {
                fields.Insert(0, field);
            }
        }

        private static void MoveToFront(IList<FieldInfo> fields, FieldInfo fieldId)
        {
            var index = fields.IndexOf(fieldId);
            if (index > 0)
            {
                var item = fields[index];
                fields.RemoveAt(index);
                fields.Insert(0, item);
            }
        }

        private class FieldInfo
        {
            public FieldInfo(string name, string referenceName)
            {
                this.Name = name;
                this.ReferenceName = referenceName;
            }

            public string Name { get; private set; }
            public string ReferenceName { get; private set; }

            public override int GetHashCode()
            {
                return this.ReferenceName.ToLowerInvariant().GetHashCode();
            }

            public override bool Equals(object obj)
            {
                FieldInfo other = obj as FieldInfo;
                if (other != null)
                {
                    return String.Equals(this.ReferenceName, other.ReferenceName, StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }
        }
    }
}
