using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Model
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ProjectReference
    {
        public Uri ProjectUri { get; private set; }
        public Uri ProjectCollectionUri { get; private set; }
        public Guid ProjectId { get; private set; }

        public ProjectReference(Uri projectCollectionUri, Guid projectId)
        {
            this.ProjectCollectionUri = projectCollectionUri;
            this.ProjectId = projectId;
            this.ProjectUri = new Uri($"vstfs:///Classification/TeamProject/{projectId}");
        }

        public ProjectReference(Uri projectCollectionUri, Uri projectUri)
        {
            Assert.ParamIsNotNull(projectCollectionUri, "projectCollectionUri");
            Assert.ParamIsNotNull(projectUri, "projectUri");

            ArtifactId artifact = LinkingUtilities.DecodeUri(projectUri.AbsoluteUri);
            Guid projectId;
            if(artifact.ArtifactType != ArtifactTypeNames.Project || artifact.Tool != ToolNames.Classification || !Guid.TryParse(artifact.ToolSpecificId, out projectId))
            {
                throw new ArgumentException("Invalid project URI " + projectUri, "projectUri");
            }

            this.ProjectCollectionUri = projectCollectionUri;
            this.ProjectUri = projectUri;
            this.ProjectId = projectId;
        }

        public override int GetHashCode()
        {
            return this.ProjectUri.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            ProjectReference other = obj as ProjectReference;
            if (other != null)
            {
                return UriUtilities.AreEqual(this.ProjectCollectionUri, other.ProjectCollectionUri)
                    && UriUtilities.AreEqual(this.ProjectUri, other.ProjectUri);
            }

            return false;
        }

        public override string ToString()
        {
            return String.Format("{0} (at {1})", ProjectUri, ProjectCollectionUri);
        }
    }
}
