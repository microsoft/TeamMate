using Microsoft.Tools.TeamMate.Foundation.Threading;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
                PullRequestPickerViewModel viewModel = (PullRequestPickerViewModel)this.DataContext;
                viewModel.Flush();
                this.DialogResult = true;
            }
        }
    }
}
