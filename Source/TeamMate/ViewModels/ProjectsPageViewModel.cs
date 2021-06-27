using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class ProjectsPageViewModel : PageViewModelBase, IGlobalCommandProvider
    {
        public ProjectsPageViewModel()
        {
            this.CommandBarType = CommandBarType.Projects;
            this.Title = "Projects";

            this.GlobalCommandBindings = new CommandBindingCollection();
            this.GlobalCommandBindings.Add(TeamMateCommands.ChooseProjects, this.ChooseProjects);
        }

        public override void OnNavigatingTo()
        {
            if (this.Projects == null)
            {
                this.Projects = this.SettingsService.Settings.Projects;
            }
        }

        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public VstsConnectionService VstsConnectionService { get; set; }


        public ICollection<ProjectInfo> Projects { get; set; }

        public CommandBindingCollection GlobalCommandBindings
        {
            get; private set;
        }

        [Import]
        public SessionService SessionService { get; set; }

        public void Remove(ProjectInfo project)
        {
            Projects.Remove(project);

            var session = this.SessionService.Session;
            var projectContext = session.ProjectContext;

            if (projectContext != null && Object.Equals(projectContext.Reference, project.Reference))
            {
                // If the removed project is the current active project, disconnect
                this.VstsConnectionService.Disconnect();
            }

            var settings = this.SettingsService.Settings;
            DefaultWorkItemInfo info = settings.DefaultWorkItemInfo;
            if (info != null && info.IsWorkItemType && info.WorkItemType.Project.Equals(project.Reference))
            {
                settings.DefaultWorkItemInfo = null;
            }

            var volatileSettings = this.SettingsService.VolatileSettings;
            if (Object.Equals(project, volatileSettings.LastUsedProject))
            {
                volatileSettings.LastUsedProject = null;
            }
        }

        [Import]
        public WindowService WindowService { get; set; }


        public void Select(ProjectInfo project)
        {
            this.VstsConnectionService.BeginConnect(project);
            this.WindowService.ShowHomePage();

            // TODO: We should consider what other good cleanup should happen when the current project has changed.

            // Clear navigation history so far, we do not want to keep data related to other projects
            this.WindowService.MainWindowViewModel.Navigation.ClearHistory();
        }

        public void ChooseProjects()
        {
            var selectedProjects = this.WindowService.ShowProjectPickerDialog(this);
            if (selectedProjects != null)
            {
                var settings = this.SettingsService.Settings;
                bool hadAnyProjects = settings.Projects.Any();

                using (settings.DeferUpdate())
                {
                    foreach (var project in selectedProjects)
                    {
                        if (!settings.Projects.Any(p => p.Reference.Equals(project.Reference)))
                        {
                            settings.Projects.Add(project);
                        }
                    }
                }

                // If I didn't have any projects, and now I have some, connect to the first one (a bit arbitrary, could be better)
                if (!hadAnyProjects && settings.Projects.Any())
                {
                    Select(settings.Projects.First());
                }
            }
        }
    }
}
