using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for PullRequestQueryEditorDialog.xaml
    /// </summary>
    public partial class PullRequestQueryEditorDialog : Window
    {
        public PullRequestQueryEditorDialog()
        {
            InitializeComponent();
            this.okButton.Click += HandleOkButtonClick;
        }

        private void HandleOkButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            bool isValid = ValidationUtilities.Validate(this);
            if (isValid)
            {
                CodeFlowPickerViewModel viewModel = (CodeFlowPickerViewModel)this.DataContext;
                viewModel.Flush();

                this.DialogResult = true;
            }
        }
    }
}
