// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for DropDownSelector.xaml
    /// </summary>
    public partial class DropDownSelector : UserControl
    {
        public DropDownSelector()
        {
            InitializeComponent();

            this.listPopup.PlacementTarget = this.button;
            this.button.Click += Button_Click;
            this.listBox.PreviewMouseLeftButtonDown += ListPopup_MouseLeftButtonDown;
        }

        private void ListPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = VisualTreeUtilities.GetListBoxItemAt<object>(e);
            if (item != null)
            {
                this.SelectedItem = item;
                ISelectableItem selectableItem = item as ISelectableItem;
                if (selectableItem != null && !selectableItem.IsSelected)
                {
                    selectableItem.IsSelected = true;
                }

                this.listPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.listPopup.IsOpen = true;
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(DropDownSelector)
        );

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(object), typeof(DropDownSelector), new PropertyMetadata((d, e) => ((DropDownSelector)d).InvalidateSelectedItem())
        );

        private void InvalidateSelectedItem()
        {
            object selectedItem = null;

            var items = this.ItemsSource as System.Collections.IEnumerable;
            if (items != null)
            {
                selectedItem = items.OfType<ISelectableItem>().FirstOrDefault(i => i.IsSelected);
            }

            this.SelectedItem = selectedItem;
        }

        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(object), typeof(DropDownSelector)
        );

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemProperty, value); }
        }
    }
}
