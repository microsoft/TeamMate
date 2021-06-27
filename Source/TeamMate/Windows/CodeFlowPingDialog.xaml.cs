// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Windows;

namespace Microsoft.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for CodeFlowPingDialog.xaml
    /// </summary>
    public partial class CodeFlowPingDialog : Window
    {
        public CodeFlowPingDialog()
        {
            InitializeComponent();
            this.okButton.Click += HandleOkButtonClick;
        }

        private void HandleOkButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string Message
        {
            get { return inputTextBox.Text.Trim(); }
        }
    }
}
