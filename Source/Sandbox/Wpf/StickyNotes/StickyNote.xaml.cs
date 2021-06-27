// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.DragAndDrop;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf.StickyNotes
{
    /// <summary>
    /// Interaction logic for StickyNote.xaml
    /// </summary>
    public partial class StickyNote : Window
    {
        public StickyNote()
        {
            InitializeComponent();

            contextMenu.AddHandler(MenuItem.ClickEvent, (RoutedEventHandler)m_Click);
            header.MouseLeftButtonDown += header_MouseLeftButtonDown;
        }

        void header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMoveWithinBoundsHelper helper = new DragMoveWithinBoundsHelper(this);
            helper.DragMove();
        }

        void m_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.OriginalSource as MenuItem;
            string color = (item != null && item.Header is string)? (string) item.Header : null;

            if(!String.IsNullOrEmpty(color))
            {
                Brush headerBrush = (Brush) TryFindResource(color +"Header");
                Brush bodyBrush = (Brush) TryFindResource(color +"Body");

                header.Background = headerBrush;
                body.Background = bodyBrush;
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            SizeToContent = SizeToContent.Height;
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDoubleClick(e);

            Close();
        }
    }
}
