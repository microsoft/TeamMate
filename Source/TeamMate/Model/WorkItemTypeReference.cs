// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;

namespace Microsoft.Tools.TeamMate.Model
{
    public class WorkItemTypeReference
    {
        public WorkItemTypeReference(string name, ProjectReference project)
        {
            Assert.ParamIsNotNull(name, "name");
            Assert.ParamIsNotNull(project, "project");

            this.Name = name;
            this.Project = project;
        }

        public string Name { get; private set; }

        public ProjectReference Project { get; private set; }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            WorkItemTypeReference other = obj as WorkItemTypeReference;
            if (other != null)
            {
                return Object.Equals(this.Name, other.Name)
                    && Object.Equals(this.Project, other.Project);
            }

            return false;
        }

        public override string ToString()
        {
            return String.Format("{0}, {1}", Name, Project);
        }
    }
}
