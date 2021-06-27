// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Model
{
    public class Session : ObservableObjectBase
    {
        private ConnectionInfo connection = new ConnectionInfo();

        public event EventHandler ProjectContextChanged;

        private ProjectContext projectContext;

        public ProjectContext ProjectContext
        {
            get { return this.projectContext; }
            set
            {
                if (SetProperty(ref this.projectContext, value))
                {
                    ProjectContextChanged?.Invoke(this, EventArgs.Empty);

                    // Take the opportunity to updates commands now that the app is connected
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ConnectionInfo ConnectionInfo
        {
            get { return this.connection; }
        }

        public void ResetConnection()
        {
            this.ConnectionInfo.ConnectionError = null;
            this.ConnectionInfo.Project = null;
            this.ProjectContext = null;
        }
    }
}
