using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for ProjectsPage.xaml
    /// </summary>
    [View(typeof(ProjectsPageViewModel))]
    public partial class ProjectsPage : UserControl
    {
        public ProjectsPage()
        {
            InitializeComponent();
            View.Initialize(this);
        }

        private void HandleRemoveClicked(object sender, RoutedEventArgs e)
        {
            ProjectInfo projectInfo = ((FrameworkElement)sender).DataContext as ProjectInfo;
            ((ProjectsPageViewModel)DataContext).Remove(projectInfo);
        }

        private void HandleProjectClicked(object sender, RoutedEventArgs e)
        {
            ProjectInfo projectInfo = ((FrameworkElement)sender).DataContext as ProjectInfo;
            ((ProjectsPageViewModel)DataContext).Select(projectInfo);
        }
    }
}
