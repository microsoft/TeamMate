using Microsoft.Tools.TeamMate.Foundation.Chaos;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public static class ChaosScenarios
    {
        public static readonly ChaosScenario ConnectToTfs = new ChaosScenario("ConnectToTfs");
        public static readonly ChaosScenario WorkItemQueryExecution = new ChaosScenario("WorkItemQueryExecution");
        public static readonly ChaosScenario CodeFlowQueryExecution = new ChaosScenario("CodeFlowQueryExecution");
        public static readonly ChaosScenario DownloadAttachment = new ChaosScenario("DownloadAttachment");
        public static readonly ChaosScenario SaveWorkItem = new ChaosScenario("SaveWorkItem");
        public static readonly ChaosScenario LocalSearch = new ChaosScenario("LocalSearch");
        public static readonly ChaosScenario TfsSearch = new ChaosScenario("TfsSearch");
        public static readonly ChaosScenario GetLinkedChangesetInfo = new ChaosScenario("GetLinkedChangesetInfo");
        public static readonly ChaosScenario GetLinkedWorkItemsInfo = new ChaosScenario("GetLinkedWorkItemsInfo");
        public static readonly ChaosScenario FileUpload = new ChaosScenario("FileUpload");
        public static readonly ChaosScenario LoadQueryFolder = new ChaosScenario("LoadQueryFolder");
        public static readonly ChaosScenario ChooseProject = new ChaosScenario("ChooseProject");
    }
}
