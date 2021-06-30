using Microsoft.Tools.TeamMate.Model;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.Services
{
    public class WorkItemStateService
    {
        // This could be smarter and customizable, not hardcoded.
        private Dictionary<string, WorkItemState> workItemTypeStates = new Dictionary<string, WorkItemState>()
        {
            // Fields from default TFS Service Scrum Template
            { "Bug.New", WorkItemState.Active },
            { "Bug.Approved", WorkItemState.Active },
            { "Bug.Committed", WorkItemState.Active },
            { "Bug.Done", WorkItemState.Closed },
            { "Bug.Removed", WorkItemState.Unknown },

            { "Feature.New", WorkItemState.Active },
            { "Feature.Done", WorkItemState.Closed },
            { "Feature.In Progress", WorkItemState.Resolved },
            { "Feature.Removed", WorkItemState.Unknown },

            { "Impediment.Open", WorkItemState.Active },
            { "Impediment.Closed", WorkItemState.Closed },

            { "Product Backlog Item.New", WorkItemState.Active },
            { "Product Backlog Item.Approved", WorkItemState.Active },
            { "Product Backlog Item.Committed", WorkItemState.Active },
            { "Product Backlog Item.Done", WorkItemState.Closed },
            { "Product Backlog Item.Removed", WorkItemState.Unknown },

            { "Task.To Do", WorkItemState.Active },
            { "Task.In Progress", WorkItemState.Resolved },
            { "Task.Done", WorkItemState.Closed },
            { "Task.Removed", WorkItemState.Unknown },

            // Fields from CMMI template in CodeBox
            { "Bug.Active", WorkItemState.Active },
            { "Bug.Proposed", WorkItemState.Active },
            { "Bug.Resolved", WorkItemState.Resolved },
            { "Bug.Closed", WorkItemState.Closed },

            { "Task.Proposed", WorkItemState.Active },
            { "Task.Active", WorkItemState.Active},
            { "Task.Resolved", WorkItemState.Resolved},
            { "Task.Closed", WorkItemState.Closed },

            { "Feature.Proposed", WorkItemState.Active },
            { "Feature.Active", WorkItemState.Active},
            { "Feature.Closed", WorkItemState.Closed },

            { "Scenario.Proposed", WorkItemState.Active },
            { "Scenario.Active", WorkItemState.Active},
            { "Scenario.Closed", WorkItemState.Closed },

            { "Deliverable.Proposed", WorkItemState.Active },
            { "Deliverable.Active", WorkItemState.Active},
            { "Deliverable.Closed", WorkItemState.Closed },

            { "Test Case.Design", WorkItemState.Active },
            { "Test Case.Ready", WorkItemState.Active},
            { "Test Case.Closed", WorkItemState.Closed },

            // Fields from other TFS servers (XBox Accessories)
            { "Task.Not Started", WorkItemState.Active },

            { "Feature.Not Started", WorkItemState.Active },
            { "Feature.Resolved", WorkItemState.Resolved},

            // Fields from other TFS servers (AppEx)
            { "Feature.Committed", WorkItemState.Active },

            // Fields from other TFS servers (RD, Windows Azure)
            // http://vstfrd:8080/azure, vstfs:///Classification/TeamProject/3c2fef47-e4c8-473f-b5f1-2bed4fa28db7, Name="RD"
            // From Karl Reinsch (karlrein)
            { "Ability.Active", WorkItemState.Active },
            { "Ability.Closed", WorkItemState.Closed },
            { "Ability.Proposed", WorkItemState.Active },
            { "Deliverable.Cancelled", WorkItemState.Active },
            { "Deliverable.Completed", WorkItemState.Active },
            { "Deployment.Active", WorkItemState.Active },
            { "Deployment.Blocked", WorkItemState.Active },
            { "Deployment.Closed", WorkItemState.Closed },
            { "Deployment.Deploying to Integration", WorkItemState.Active },
            { "Deployment.Deploying to Production", WorkItemState.Active },
            { "Deployment.Deploying to Stage", WorkItemState.Active },
            { "Deployment.Exited Integration", WorkItemState.Active },
            { "Deployment.In Integration", WorkItemState.Active },
            { "Deployment.In Stage", WorkItemState.Active },
            { "Deployment.Ready", WorkItemState.Active },
            { "DeploymentTask.Awaiting Deployment", WorkItemState.Active },
            { "DeploymentTask.Awaiting Pre-Execution", WorkItemState.Active },
            { "DeploymentTask.Blocked", WorkItemState.Active },
            { "DeploymentTask.Cancelled", WorkItemState.Active },
            { "DeploymentTask.Completed", WorkItemState.Active },
            { "DeploymentTask.Deploying", WorkItemState.Active },
            { "DeploymentTask.On-Hold", WorkItemState.Active },
            { "DeploymentTask.Pending", WorkItemState.Active },
            { "DeploymentTask.Queued", WorkItemState.Active },
            { "Incident.Active", WorkItemState.Active },
            { "Incident.Closed", WorkItemState.Closed },
            { "Incident.Resolved", WorkItemState.Resolved },
            { "Operations Document.Active", WorkItemState.Active },
            { "Operations Document.Draft", WorkItemState.Active },
            { "Operations Document.Retired", WorkItemState.Active },
            { "Pillar Area.Active", WorkItemState.Active },
            { "Pillar Area.Closed", WorkItemState.Closed },
            { "Pillar Area.Proposed", WorkItemState.Active },
            { "RDBug.Active", WorkItemState.Active },
            { "RDBug.Closed", WorkItemState.Closed },
            { "RDBug.Resolved", WorkItemState.Resolved },
            { "RDCapacity.Deleted", WorkItemState.Active },
            { "RDCapacity.Delivery Complete", WorkItemState.Active },
            { "RDCapacity.Demand Cancelled", WorkItemState.Active },
            { "RDCapacity.Deploy Complete", WorkItemState.Active },
            { "RDCapacity.Planned", WorkItemState.Active },
            { "RDCapacity.Production Complete", WorkItemState.Active },
            { "RDCapacity.Source Complete", WorkItemState.Active },
            { "RDCommitment.Active", WorkItemState.Active },
            { "RDCommitment.Cancelled", WorkItemState.Active },
            { "RDCommitment.Completed", WorkItemState.Active },
            { "RDIncident.Blocked", WorkItemState.Active },
            { "RDIncident.Closed", WorkItemState.Closed },
            { "RDIncident.Investigate", WorkItemState.Active },
            { "RDIncident.Mitigate", WorkItemState.Active },
            { "RDIncident.Resolved", WorkItemState.Resolved },
            { "RDIncident.Triage", WorkItemState.Active },
            { "RDTask.Active", WorkItemState.Active },
            { "RDTask.Closed", WorkItemState.Closed },
            { "RDTask.Resolved", WorkItemState.Resolved },
            { "Release Signoff Record.Active", WorkItemState.Active },
            { "Release Signoff Record.Closed", WorkItemState.Closed },
            { "Secret Type Dependency.Active", WorkItemState.Active },
            { "Secret Type Dependency.Deprecated", WorkItemState.Active },
            { "Secret Type Dependency.Draft", WorkItemState.Active },
            { "Secret Type Dependency.Retired", WorkItemState.Active },
            { "Secret Type Dependency.Review", WorkItemState.Active },
            { "Secret Type.Active", WorkItemState.Active },
            { "Secret Type.Deprecated", WorkItemState.Active },
            { "Secret Type.Draft", WorkItemState.Active },
            { "Secret Type.Retired", WorkItemState.Active },
            { "Secret Type.Review", WorkItemState.Active },
            { "Shared Steps.Active", WorkItemState.Active },
            { "Shared Steps.Closed", WorkItemState.Closed },
            { "Sprint.Created", WorkItemState.Active },
        };

        // Default states that we apply across the board as fallbacks
        private Dictionary<string, WorkItemState> defaultStates = new Dictionary<string, WorkItemState>()
        {
            { "New", WorkItemState.Active },
            { "Active", WorkItemState.Active },
            { "Proposed", WorkItemState.Active },
            { "Resolved", WorkItemState.Resolved },
            { "Closed", WorkItemState.Closed },
            { "Completed", WorkItemState.Closed },
        };

        public WorkItemState GetWorkItemState(string workItemTypeName, string state)
        {
            string key = workItemTypeName + "." + state;
            WorkItemState workItemState;
            if (!workItemTypeStates.TryGetValue(key, out workItemState))
            {
                if(!defaultStates.TryGetValue(state, out workItemState))
                {
                    workItemState = WorkItemState.Unknown;
                }
            }

            return workItemState;
        }
    }
}
