using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Interaction logic for DialogPanel.xaml
    /// </summary>
    public class DialogPanel : ContentControl
    {
        static DialogPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogPanel), new FrameworkPropertyMetadata(typeof(DialogPanel)));
            MarginProperty.OverrideMetadata(typeof(DialogPanel), new FrameworkPropertyMetadata(new Thickness(12)));
        }

        /// <summary>
        /// The button panel property
        /// </summary>
        public static readonly DependencyProperty ButtonPanelProperty = DependencyProperty.Register(
            "ButtonPanel", typeof(ButtonPanel), typeof(DialogPanel), new PropertyMetadata(OnButtonPanelChanged)
        );

        /// <summary>
        /// The button panel dock property
        /// </summary>
        public static readonly DependencyProperty ButtonPanelDockProperty = DependencyProperty.Register(
            "ButtonPanelDock", typeof(Dock), typeof(DialogPanel), new PropertyMetadata(Dock.Bottom)
        );

        /// <summary>
        /// Gets or sets the button panel dock.
        /// </summary>
        public Dock ButtonPanelDock
        {
            get { return (Dock)GetValue(ButtonPanelDockProperty); }
            set { SetValue(ButtonPanelDockProperty, value); }
        }

        /// <summary>
        /// The button panel margin property
        /// </summary>
        public static readonly DependencyProperty ButtonPanelMarginProperty = DependencyProperty.Register(
            "ButtonPanelMargin", typeof(Thickness), typeof(DialogPanel), new PropertyMetadata(new Thickness(0, 12, 0, 0))
        );

        /// <summary>
        /// Gets or sets the button panel margin.
        /// </summary>
        public Thickness ButtonPanelMargin
        {
            get { return (Thickness)GetValue(ButtonPanelMarginProperty); }
            set { SetValue(ButtonPanelMarginProperty, value); }
        }

        /// <summary>
        /// The button panel visibility property
        /// </summary>
        public static readonly DependencyProperty ButtonPanelVisibilityProperty = DependencyProperty.Register(
            "ButtonPanelVisibility", typeof(Visibility), typeof(DialogPanel), new PropertyMetadata(Visibility.Collapsed)
        );

        /// <summary>
        /// Gets or sets the button panel visibility.
        /// </summary>
        public Visibility ButtonPanelVisibility
        {
            get { return (Visibility)GetValue(ButtonPanelVisibilityProperty); }
            set { SetValue(ButtonPanelVisibilityProperty, value); }
        }

        /// <summary>
        /// Called when the button panel has changed.
        /// </summary>
        internal static void OnButtonPanelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DialogPanel panel = obj as DialogPanel;
            ButtonPanel oldPanel = args.OldValue as ButtonPanel;
            ButtonPanel newPanel = args.NewValue as ButtonPanel;

            if (oldPanel != null)
            {
                newPanel.OrientationChanged -= panel.HandleButtonPanelOrientationChanged;
                newPanel.IsVisibleChanged -= panel.HandleButtonPanelVisibilityChanged;
            }

            if (newPanel != null)
            {
                newPanel.OrientationChanged += panel.HandleButtonPanelOrientationChanged;
                newPanel.IsVisibleChanged += panel.HandleButtonPanelVisibilityChanged;
            }

            panel.InvalidateButtonPanel();
        }

        /// <summary>
        /// Handles the button panel visibility changed.
        /// </summary>
        private void HandleButtonPanelVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            InvalidateButtonPanel();
        }

        /// <summary>
        /// Handles the button panel orientation changed.
        /// </summary>
        private void HandleButtonPanelOrientationChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            InvalidateButtonPanel();
        }

        /// <summary>
        /// Invalidates the button panel.
        /// </summary>
        private void InvalidateButtonPanel()
        {
            bool buttonPanelIsVisible = (ButtonPanel != null && ButtonPanel.IsVisible);
            ButtonPanelDock = (ButtonPanel == null || ButtonPanel.Orientation == Orientation.Horizontal) ? Dock.Bottom : Dock.Right;

            if (!buttonPanelIsVisible)
            {
                ButtonPanelMargin = new Thickness();
            }
            else
            { 
                ButtonPanelMargin = (ButtonPanel == null || ButtonPanel.Orientation == Orientation.Horizontal) ? new Thickness(0, 12, 0, 0) : new Thickness(12, 0, 0, 0);
            }
        }

        /// <summary>
        /// Gets or sets the button panel.
        /// </summary>
        public ButtonPanel ButtonPanel
        {
            get { return (ButtonPanel)GetValue(ButtonPanelProperty); }
            set { SetValue(ButtonPanelProperty, value); }
        }
    }
}
