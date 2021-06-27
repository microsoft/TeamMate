using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for NewWorkItemPage.xaml
    /// </summary>
    [View(typeof(NewWorkItemPageViewModel))]
    public partial class NewWorkItemPage : UserControl
    {
        public NewWorkItemPage()
        {
            InitializeComponent();
            View.Initialize(this);
        }

        private NewWorkItemPageViewModel ViewModel
        {
            get { return DataContext as NewWorkItemPageViewModel; }
        }

        private void HandleTypeOrTemplateClicked(object sender, RoutedEventArgs e)
        {
            FrameworkElement button = e.OriginalSource as FrameworkElement;
            if (button != null)
            {
                WorkItemTypeInfo workItemType = button.DataContext as WorkItemTypeInfo;

                if (workItemType != null)
                {
                    e.Handled = true;
                    ViewModel.CreateWorkItem(workItemType);
                }
            }
        }

        private void HandleSetDefaultClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element != null)
            {
                WorkItemTypeInfo workItemType = element.DataContext as WorkItemTypeInfo;

                if (workItemType != null)
                {
                    ViewModel.SetDefaultWorkItemInfo(new DefaultWorkItemInfo(workItemType.Reference));
                    e.Handled = true;
                }
            }
        }
    }
}
