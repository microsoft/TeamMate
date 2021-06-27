// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    [View(typeof(HomePageViewModel))]
    public partial class HomePage : UserControl
    {
        public HomePage()
        {
            InitializeComponent();
            View.Initialize(this);
        }
    }
}
