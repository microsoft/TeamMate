using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for CodeFlowQueryEditorDialog.xaml
    /// </summary>
    public partial class CodeFlowQueryEditorDialog : Window
    {
        public CodeFlowQueryEditorDialog()
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
