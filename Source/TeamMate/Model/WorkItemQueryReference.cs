using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;

namespace Microsoft.Tools.TeamMate.Model
{
    public class WorkItemQueryReference
    {
        public Uri ProjectCollectionUri { get; private set; }

        public Guid Id { get; private set; }

        public WorkItemQueryReference(Uri projectCollectionUri, Guid id)
        {
            Assert.ParamIsNotNull(projectCollectionUri, "projectCollectionUri");

            this.ProjectCollectionUri = projectCollectionUri;
            this.Id = id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            WorkItemQueryReference other = obj as WorkItemQueryReference;
            if (other != null)
            {
                return Object.Equals(this.Id, other.Id)
                    && UriUtilities.AreEqual(this.ProjectCollectionUri, other.ProjectCollectionUri);
            }

            return false;
        }

        public override string ToString()
        {
            return String.Format("{0}, {1}", Id, ProjectCollectionUri);
        }
    }
}
