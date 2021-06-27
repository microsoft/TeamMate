// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for MyTransitionWindow.xaml
    /// </summary>
    public partial class MyTransitionWindow : Window
    {
        public MyTransitionWindow()
        {
            InitializeComponent();
            SlideTransition slideTransition = new SlideTransition();
            slideTransition.Direction = SlideDirection.Random;
            myTransition.Transition = slideTransition;

            this.Loaded += MyTransitionWindow_Loaded;

            // myTransition.Transition = new FadeTransition();
        }

        void MyTransitionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Grid red = new Grid();
            Grid blue = new Grid();
            red.Background = Brushes.Red;
            blue.Background = Brushes.Blue;

            myTransition.ItemSource = new object[] { red, blue };
        }
    }
}
