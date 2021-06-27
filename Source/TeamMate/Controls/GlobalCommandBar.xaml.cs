// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for GlobalCommandBar.xaml
    /// </summary>
    public partial class GlobalCommandBar : UserControl
    {
        public GlobalCommandBar()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof(CommandBarType), typeof(GlobalCommandBar)
        );

        public CommandBarType Type
        {
            get { return (CommandBarType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
    }
}
