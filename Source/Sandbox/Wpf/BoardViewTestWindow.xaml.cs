using Microsoft.Internal.Tools.TeamMate.Foundation.Windows;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf.Board
{
    /// <summary>
    /// Interaction logic for BoardViewTestWindow.xaml
    /// </summary>
    public partial class BoardViewTestWindow : Window
    {
        public BoardViewTestWindow()
        {
            InitializeComponent();

            board.ItemTemplate = null;
            board.ItemTemplateSelector = new TileSelector();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.F11)
            {
                WindowUtilities.ToggleFullScreen(this);
                e.Handled = true;
            }
        }

        private class TileSelector : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                string text = item as string;
                string resource = (!String.IsNullOrEmpty(text) && text.Contains("3")) ? "RedItemTemplate" : "ItemTemplate";
                return (DataTemplate)((FrameworkElement)container).FindResource(resource);
            }
        }
    }
}
