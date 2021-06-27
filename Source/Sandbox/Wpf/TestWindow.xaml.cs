// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
            this.Loaded += TestWindow_Loaded;


            /*
            this.editableDropDown.Value = 77;
            this.editableDropDown.ItemsSource = new double[] {
                1, 2, 3, 5, 8, 13
            };

            /*
            var dd2 = DragDropHelper2.Create(tb);
            dd2.DragRequested += dd2_DragRequested;
            this.GiveFeedback += TestWindow_GiveFeedback;
             */
        }

        void TestWindow_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            e.Handled = true;
        }

        void dd2_DragRequested(object sender, EventArgs e)
        {
            /*
            var wpfBitmap = BitmapUtilities.CaptureBitmap(tb);
            Cursor customCursor = InteropUtilities.CursorFromBitmapSource(wpfBitmap);

            Mouse.OverrideCursor = customCursor;
            DragDrop.DoDragDrop(this, "Hello", DragDropEffects.Move);
            Mouse.OverrideCursor = null;
             */
        }

        void TestWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // metroTile.DataContext = SandboxSampleData.Tile;
        }
    }
}
