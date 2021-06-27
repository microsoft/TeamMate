// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf.DataViews
{
    /// <summary>
    /// Interaction logic for ListViewWindow.xaml
    /// </summary>
    public partial class ListViewWindow : Window
    {
        public ListViewWindow()
        {
            InitializeComponent();
            this.listView.ItemsActivated += listView_ItemsActivated;
        }

        void listView_ItemsActivated(object sender, Foundation.Windows.Controls.Data.ListViewItemsActivatedEventArgs e)
        {
            MessageBox.Show(String.Join(", ", e.Items));
            e.Handled = true;
        }
    }
}
