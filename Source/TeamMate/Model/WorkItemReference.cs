using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;

namespace Microsoft.Tools.TeamMate.Model
{
    [Serializable]
    public class WorkItemReference
    {
        public WorkItemReference(Uri projectCollectionUri, int id)
        {
            Assert.ParamIsNotNull(projectCollectionUri, "projectCollectionUri");
            Assert.ParamIsGreaterThanZero(id, "id");

            this.Id = id;
            this.ProjectCollectionUri = projectCollectionUri;
        }

        public int Id { get; private set; }

        public Uri ProjectCollectionUri { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            WorkItemReference other = obj as WorkItemReference;
            if (other != null)
            {
                return Object.Equals(Id, other.Id) && UriUtilities.AreEqual(ProjectCollectionUri, other.ProjectCollectionUri);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        public static bool TryParseId(string text, out int id)
        {
            id = 0;
            return (!String.IsNullOrEmpty(text) && Int32.TryParse(text, out id) && id > 0);
        }
    }

    public static class WorkItemReferenceExtensions
    {
        public static WorkItemReference GetReference(this WorkItem workItem)
        {
            Uri projectCollectionUri = workItem.GetProjectCollectionUrl();
            int id = workItem.Id.Value;
            return new WorkItemReference(projectCollectionUri, id);
        }
    }
}
