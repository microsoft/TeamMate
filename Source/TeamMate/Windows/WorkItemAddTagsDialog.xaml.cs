using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for WorkItemAddTagsDialog.xaml
    /// </summary>
    public partial class WorkItemAddTagsDialog : Window
    {
        public WorkItemAddTagsDialog()
        {
            InitializeComponent();
            this.Loaded += HandleLoaded;

            this.okTagButton.Click += OkTagButton_Click;
            this.tagsListBox.SelectionChanged += RemoveTagSelectionChanged;
        }

        private void OkTagButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Initialize();
        }

        private WorkItemAddTagsDialogViewModel ViewModel
        {
            get { return this.DataContext as WorkItemAddTagsDialogViewModel; }
        }

        private void RemoveTagSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            ListBox listbox = (ListBox)sender;
            this.ViewModel.SelectedItems.Clear();
            foreach (var item in listbox.SelectedItems)
            {
                this.ViewModel.SelectedItems.Add((string)item);
            }
        }
    }
}
