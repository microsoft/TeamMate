// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Xml;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Model.Actions
{
    public class ActionSerializer
    {
        public TeamMateAction ReadAction(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            XDocument document = XDocument.Load(filename);
            XElement root = document.Root;
            // Check that root is TeamMate v 1.0

            TeamMateAction result = null;

            XElement firstElement = root.Elements().FirstOrDefault();
            if (firstElement != null && firstElement.Name == Schema.Action)
            {
                ActionType type = firstElement.GetRequiredAttribute<ActionType>(Schema.Type);
                switch (type)
                {
                    case ActionType.CreateWorkItem:
                        result = ReadCreateWorkItemAction(firstElement);
                        break;
                }

                if (result != null)
                {
                    result.DeleteOnLoad = firstElement.GetAttribute<bool>(Schema.DeleteOnLoad);
                }
            }

            if (result == null)
            {
                throw new NotSupportedException("UnsupportedAction");
            }

            return result;
        }

        private CreateWorkItemAction ReadCreateWorkItemAction(XElement element)
        {
            XElement workItem = element.Element(Schema.WorkItem);
            WorkItemUpdateInfo info = ReadWorkItemInfo(workItem);
            CreateWorkItemAction action = new CreateWorkItemAction(info);
            return action;
        }

        private WorkItemUpdateInfo ReadWorkItemInfo(XElement element)
        {
            WorkItemUpdateInfo workItem = new WorkItemUpdateInfo();

            foreach (var fieldElement in element.Elements(Schema.Fields, Schema.Field))
            {
                string fieldName = fieldElement.GetRequiredAttribute<string>(Schema.Name);
                string value = fieldElement.Value;

                workItem.Fields[fieldName] = value;
            }

            foreach (var attachmentElement in element.Elements(Schema.Attachments, Schema.Attachment))
            {
                AttachmentInfo attachment = new AttachmentInfo();
                attachment.Path = attachmentElement.GetRequiredAttribute<string>(Schema.Path);
                attachment.Comment = attachmentElement.GetAttribute<string>(Schema.Comment);
                attachment.DeleteOnSave = attachmentElement.GetAttribute<bool>(Schema.DeleteOnSave);

                workItem.Attachments.Add(attachment);
            }

            return workItem;
        }

        private static class Schema
        {
            public static readonly Version CurrentVersion = new Version("1.0");

            public static readonly XName TeamMate = "TeamMate";
            public static readonly string Version = "Version";
            public static readonly string DeleteOnLoad = "DeleteOnLoad";

            public static readonly XName Action = "Action";
            public static readonly string Type = "Type";

            public static readonly XName WorkItem = "WorkItem";
            public static readonly XName Fields = "Fields";
            public static readonly XName Field = "Field";
            public static readonly string Name = "Name";

            public static readonly XName Attachments = "Attachments";
            public static readonly XName Attachment = "Attachment";
            public static readonly string Path = "Path";
            public static readonly string Comment = "Comment";
            public static readonly string DeleteOnSave = "DeleteOnSave";
        }
    }
}
