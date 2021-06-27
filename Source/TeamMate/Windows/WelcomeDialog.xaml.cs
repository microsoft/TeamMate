// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Windows;

namespace Microsoft.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for WelcomeDialog.xaml
    /// </summary>
    public partial class WelcomeDialog : Window
    {
        public WelcomeDialog()
        {
            InitializeComponent();
            this.startButton.Click += HandleStartButtonClick;
        }

        private void HandleStartButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
