using Microsoft.Tools.TeamMate.ViewModels;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for ProjectPickerDialog.xaml
    /// </summary>
    public partial class ProjectPickerDialog : Window
    {
        public ProjectPickerDialog()
        {
            InitializeComponent();

            this.urlTextBox.KeyDown += OnUrlTextBoxKeyDown;
            this.okButton.Click += OnOkClicked;

            this.Closing += OnClosing;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.ViewModel?.CancelConnect();
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            var selectedProjects = this.projectsListBox.SelectedItems.OfType<TeamProjectReference>().ToList();
            if (selectedProjects.Any())
            {
                var vm = this.ViewModel;
                if (vm != null)
                {
                    vm.SelectedProjects = selectedProjects;
                }

                this.DialogResult = true;
            }
        }

        private void OnUrlTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ViewModel?.Connect();
                e.Handled = true;
            }
        }

        private ProjectPickerDialogViewModel ViewModel
        {
            get { return this.DataContext as ProjectPickerDialogViewModel; }
        }
    }
}
