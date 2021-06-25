using Microsoft.Internal.Tools.TeamMate.Foundation.ComponentModel;
using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;

namespace Microsoft.Internal.Tools.TeamMate.Model
{
    public class ProjectInfo : ObservableObjectBase
    {
        private string projectName;
        private string preferredName;
        private string displayName;

        public ProjectInfo(ProjectReference reference, string projectName)
        {
            Assert.ParamIsNotNull(reference, "project");
            Assert.ParamIsNotNull(projectName, "projectName");

            this.Reference = reference;
            this.ProjectName = projectName;
        }

        public ProjectReference Reference { get; private set; }

        public Uri ProjectCollectionUri
        {
            get { return Reference.ProjectCollectionUri; }
        }

        public string PreferredName
        {
            get { return this.preferredName; }
            set
            {
                if (SetProperty(ref this.preferredName, value))
                {
                    InvalidateDisplayName();
                }
            }
        }

        public string ProjectName
        {
            get { return this.projectName; }
            set
            {
                if (SetProperty(ref this.projectName, value))
                {
                    InvalidateDisplayName();
                }
            }
        }

        public string DisplayName
        {
            get { return this.displayName; }
            private set { SetProperty(ref this.displayName, value); }
        }

        private void InvalidateDisplayName()
        {
            DisplayName = (!String.IsNullOrEmpty(PreferredName)) ? PreferredName : ProjectName;
        }

        public override bool Equals(object obj)
        {
            ProjectInfo other = obj as ProjectInfo;
            if (other != null)
            {
                return Object.Equals(this.Reference, other.Reference);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.Reference.GetHashCode();
        }

        public override string ToString()
        {
            return this.Reference.ToString();
        }

        public ProjectInfo Clone()
        {
            ProjectInfo newInfo = new ProjectInfo(this.Reference, this.ProjectName);
            newInfo.PreferredName = this.PreferredName;
            return newInfo;
        }
    }
}
