using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Model;
using Microsoft.Internal.Tools.TeamMate.Services;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Microsoft.Internal.Tools.TeamMate.ViewModels
{
    public class NewWorkItemPageViewModel : PageViewModelBase
    {
        public NewWorkItemPageViewModel()
        {
            this.Title = "New Work Item";
        }

        private Session session;

        public Session Session
        {
            get { return this.session; }
            set { SetProperty(ref this.session, value); }
        }

        private ICollection<WorkItemTypeInfo> workItemTypes;

        public ICollection<WorkItemTypeInfo> WorkItemTypes
        {
            get { return this.workItemTypes; }
            set { SetProperty(ref this.workItemTypes, value); }
        }

        public void SetDefaultWorkItemInfo(DefaultWorkItemInfo workItemInfo)
        {
            this.SettingsService.Settings.DefaultWorkItemInfo = workItemInfo;
        }

        public void CreateWorkItem(WorkItemTypeInfo workItemType)
        {
            Assert.ParamIsNotNull(workItemType, "workItemType");

            this.WindowService.ShowNewWorkItemWindow(workItemType.Reference);
        }

        [Import]
        public WindowService WindowService { get; set; }

        [Import]
        public SettingsService SettingsService { get; set; }
    }
}
