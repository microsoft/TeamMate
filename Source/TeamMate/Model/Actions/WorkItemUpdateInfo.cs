// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.Model.Actions
{
    public class WorkItemUpdateInfo
    {
        private IDictionary<string, object> fields = new Dictionary<string, object>();
        private ICollection<AttachmentInfo> attachments = new List<AttachmentInfo>();

        public IDictionary<string, object> Fields
        {
            get { return this.fields; }
        }

        public ICollection<AttachmentInfo> Attachments
        {
            get { return this.attachments; }
        }
    }

    public class AttachmentInfo
    {
        public string Path { get; set; }
        public string Comment { get; set; }
        public bool DeleteOnSave { get; set; }
    }
}
