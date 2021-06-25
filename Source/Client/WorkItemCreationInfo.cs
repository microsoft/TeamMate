using System;
using System.Collections.Generic;

namespace Microsoft.Internal.Tools.TeamMate.Client
{
    public class WorkItemCreationInfo
    {
        private IDictionary<string, object> fields = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public WorkItemCreationInfo()
        {
            this.Attachments = new List<AttachmentInfo>();
        }

        public string Title
        {
            get { return this.GetField("System.Title") as string; }
            set { this.SetField("System.Title", value); }
        }

        public string History
        {
            get { return this.GetField("System.History") as string; }
            set { this.SetField("System.History", value); }
        }

        public string Description
        {
            get { return this.GetField("System.Description") as string; }
            set { this.SetField("System.Description", value); }
        }

        public IDictionary<string, object> Fields
        {
            get { return this.fields; }
        }

        public void SetField(string name, object value)
        {
            if (value != null)
            {
                this.fields[name] = value;
            }
            else
            {
                this.fields.Remove(name);
            }
        }

        public object GetField(string name)
        {
            object result;
            this.fields.TryGetValue(name, out result);
            return result;
        }

        public ICollection<AttachmentInfo> Attachments { get; private set; }
    }

    public class AttachmentInfo
    {
        public AttachmentInfo(string path, bool deleteOnSave = false)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            this.Path = path;
            this.DeleteOnSave = deleteOnSave;
        }

        public string Path { get; set; }

        public bool DeleteOnSave { get; set; }
    }
}
