using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Model
{
    [Serializable]
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class PullRequestReference
    {
        public PullRequestReference(Guid projectId, int id)
        {
            Assert.ParamIsNotNull(projectId, "projectId");
            Assert.ParamIsGreaterThanZero(id, "id");

            this.Id = id;
            this.ProjectId = projectId;
        }

        public int Id { get; private set; }

        public Guid ProjectId { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            PullRequestReference other = obj as PullRequestReference;
            if (other != null)
            {
                return Object.Equals(Id, other.Id) && (this.ProjectId == other.ProjectId);
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

    public static class PullRequestReferenceExtensions
    {
        [SupportedOSPlatform("windows10.0.19041.0")]
        public static PullRequestReference GetReference(this GitPullRequest pullRequest)
        {
            Guid projectId = pullRequest.Repository.Id;
            int id = pullRequest.PullRequestId;
            return new PullRequestReference(projectId, id);
        }
    }
}
