using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class PageViewModelBase : ViewModelBase
    {
        private string title;

        public string Title
        {
            get { return this.title; }
            set { SetProperty(ref this.title, value); }
        }

        private CommandBarType commandBarType;

        public CommandBarType CommandBarType
        {
            get { return this.commandBarType; }
            set { SetProperty(ref this.commandBarType, value); }
        }


        public virtual void OnNavigatingTo()
        {
        }

        public virtual void OnNavigatingFrom()
        {
        }
    }

    public enum CommandBarType
    {
        None,
        Home,
        WorkItems,
        PullRequests,
        Projects
    }
}
