using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    public static class UI
    {
        /// <summary>
        /// Identifies the EscapeAction dependency property.
        /// </summary>
        public static readonly DependencyProperty EscapeActionProperty = DependencyProperty.RegisterAttached(
          "EscapeAction", typeof(EscapeAction), typeof(UI), new PropertyMetadata(EscapeAction.None, OnEscapeActionChanged)
        );

        /// <summary>
        /// Sets the action that occurs when the Escape key is pressed for a given window.
        /// </summary>
        /// <param name="element">An element.</param>
        /// <param name="value">The the action that occurs when the Escape key is pressed for a given window.</param>
        public static void SetEscapeAction(DependencyObject element, EscapeAction value)
        {
            element.SetValue(EscapeActionProperty, value);
        }

        /// <summary>
        /// Gets the action that occurs when the Escape key is pressed for a given window.
        /// </summary>
        /// <param name="element">An element.</param>
        /// <returns>The action that occurs when the Escape key is pressed for a given window.</returns>
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static EscapeAction GetEscapeAction(DependencyObject element)
        {
            return (EscapeAction)element.GetValue(EscapeActionProperty);
        }

        /// <summary>
        /// Handles the changes in the EscapeAction property.
        /// </summary>
        private static void OnEscapeActionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Window window = obj as Window;

            if (window != null)
            {
                EscapeAction oldValue = (EscapeAction)args.OldValue;
                EscapeAction newValue = (EscapeAction)args.NewValue;

                if (oldValue != EscapeAction.None)
                {
                    window.KeyDown -= HandleEscapeKey;
                }

                if (newValue != EscapeAction.None)
                {
                    window.KeyDown += HandleEscapeKey;
                }
            }
        }

        /// <summary>
        /// Handles the pressing of the escape key on a given window.
        /// </summary>
        private static void HandleEscapeKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Window window = (Window)sender;
                EscapeAction action = GetEscapeAction(window);

                switch (action)
                {
                    case EscapeAction.Close:
                        CloseOrCancel(window);
                        e.Handled = true;
                        break;

                    case EscapeAction.Hide:
                        window.Hide();
                        e.Handled = true;
                        break;
                }
            }
        }

        public static readonly DependencyProperty IsDialogProperty = DependencyProperty.RegisterAttached(
          "IsDialog", typeof(bool), typeof(UI), new PropertyMetadata(OnIsDialogChanged)
        );

        public static void SetIsDialog(DependencyObject element, bool value)
        {
            element.SetValue(IsDialogProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetIsDialog(DependencyObject element)
        {
            return (bool)element.GetValue(IsDialogProperty);
        }

        private static void OnIsDialogChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Window window = obj as Window;
            if (window != null)
            {
                bool value = (bool)args.NewValue;

                if (value)
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.ResizeMode = ResizeMode.NoResize;
                    window.ShowInTaskbar = false;

                    if (window.Icon == null)
                    {
                        // There are tricks using NativeMethods.SetWindowLong() to change the window's title bar appearence,
                        // but they stopped working on Windows 10, so using a cheaper strategy
                        window.Icon = ControlResources.EmptyIcon;
                    }
                }
            }
        }

        public static readonly DependencyProperty ZoomWithWheelProperty = DependencyProperty.RegisterAttached(
            "ZoomWithWheel", typeof(bool), typeof(UI), new PropertyMetadata(OnZoomWithWheelChanged)
        );

        public static void SetZoomWithWheel(DependencyObject element, bool value)
        {
            element.SetValue(ZoomWithWheelProperty, value);
        }

        public static bool GetZoomWithWheel(DependencyObject element)
        {
            return (bool)element.GetValue(ZoomWithWheelProperty);
        }

        private static void OnZoomWithWheelChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement e = source as FrameworkElement;
            if (e != null)
            {
                if ((bool)args.NewValue)
                {
                    e.PreviewMouseWheel += HandleZoomPreviewMouseWheel;
                    e.PreviewMouseDown += HandleZoomPreviewMouseDown;
                }
                else
                {
                    e.LayoutTransform = null;
                    e.PreviewMouseWheel -= HandleZoomPreviewMouseWheel;
                    e.PreviewMouseDown -= HandleZoomPreviewMouseDown;
                }
            }
        }

        private static void HandleZoomPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.MiddleButton == MouseButtonState.Pressed)
            {
                SetZoomFactor((FrameworkElement)sender, 1);
                e.Handled = true;
            }
        }

        private static void HandleZoomPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double delta = (e.Delta > 0) ? 0.1 : -0.1;
                IncrementZoomFactor((FrameworkElement)sender, delta);
                e.Handled = true;
            }
        }

        private const double MaxZoomFactor = 4;
        private const double MinZoomFactor = 0.3;

        public static void IncrementZoomFactor(FrameworkElement element, double delta)
        {
            double zoomFactor = GetZoomFactor(element);
            zoomFactor = Math.Min(MaxZoomFactor, Math.Max(MinZoomFactor, zoomFactor + delta));
            SetZoomFactor(element, zoomFactor);
        }

        public static void ResetZoomFactor(FrameworkElement e)
        {
            if (GetZoomFactor(e) != 1)
            {
                e.LayoutTransform = null;
            }
        }

        public static double GetZoomFactor(FrameworkElement e)
        {
            ScaleTransform transform = e.LayoutTransform as ScaleTransform;
            return (transform != null) ? transform.ScaleX : 1;
        }

        public static void SetZoomFactor(FrameworkElement e, double zoomFactor)
        {
            Assert.ParamIsGreaterThanZero(zoomFactor, "zoomFactor");

            ScaleTransform transform = e.LayoutTransform as ScaleTransform;
            if (transform == null)
            {
                transform = new ScaleTransform(1, 1);
                e.LayoutTransform = transform;
            }

            transform.ScaleX = zoomFactor;
            transform.ScaleY = zoomFactor;
        }

        public static readonly DependencyProperty ButtonTypeProperty = DependencyProperty.RegisterAttached(
            "ButtonType", typeof(ButtonType), typeof(UI), new PropertyMetadata(ButtonType.None, OnButtonTypeChanged)
        );

        public static void SetButtonType(DependencyObject element, ButtonType value)
        {
            element.SetValue(ButtonTypeProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Button))]
        public static ButtonType GetButtonType(DependencyObject element)
        {
            return (ButtonType)element.GetValue(ButtonTypeProperty);
        }

        private static void OnButtonTypeChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            Button button = source as Button;
            if (button != null)
            {
                ButtonType oldValue = (ButtonType)args.OldValue;
                ButtonType newValue = (ButtonType)args.NewValue;

                if (newValue != ButtonType.None)
                {
                    button.Click += HandleButtonClick;
                    if (newValue == ButtonType.OK)
                    {
                        button.Content = "OK";
                        button.IsDefault = true;
                    }
                    else if (newValue == ButtonType.Cancel)
                    {
                        // TODO: Resourcify
                        button.Content = "Cancel";
                        button.IsCancel = true;
                    }
                }
                else
                {
                    button.Click -= HandleButtonClick;
                }
            }
        }

        private static void HandleButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Window window;
                ButtonType buttonType = GetButtonType(button);
                switch (buttonType)
                {
                    case ButtonType.OK:
                        window = Window.GetWindow(button);
                        if (window != null)
                        {
                            window.DialogResult = true;
                            e.Handled = true;
                        }
                        break;

                    case ButtonType.Cancel:
                        window = Window.GetWindow(button);
                        if (window != null)
                        {
                            window.DialogResult = false;
                            e.Handled = true;
                        }
                        break;
                }
            }
        }

        public static readonly DependencyProperty DoubleClickTargetProperty = DependencyProperty.RegisterAttached(
            "DoubleClickTarget", typeof(Button), typeof(UI), new FrameworkPropertyMetadata(OnDoubleClickTargetChanged)
        );

        public static void SetDoubleClickTarget(DependencyObject element, Button value)
        {
            element.SetValue(DoubleClickTargetProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Selector))]
        public static Button GetDoubleClickTarget(DependencyObject element)
        {
            return (Button)element.GetValue(DoubleClickTargetProperty);
        }

        private static void OnDoubleClickTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Control control = obj as Control;
            if (control != null)
            {
                if (args.NewValue != null)
                {
                    control.MouseDoubleClick += HandleSourceDoubleClick;
                }
                else
                {
                    control.MouseDoubleClick -= HandleSourceDoubleClick;
                }
            }
        }

        private static void HandleSourceDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                object item = null;

                if (e.Source is ListBox)
                {
                    item = VisualTreeUtilities.GetListBoxItemAt<object>(e);
                }
                else if (e.Source is ListView)
                {
                    item = VisualTreeUtilities.GetListViewItemAt<object>(e);
                }
                else if (e.Source is TreeView)
                {
                    item = VisualTreeUtilities.GetTreeViewItemAt<object>(e);
                }

                if (item != null)
                {
                    Button target = GetDoubleClickTarget((DependencyObject)sender);
                    if (target != null && target.IsEnabled)
                    {
                        target.PerformClick();
                        e.Handled = true;
                    }
                }
            }
        }

        public static readonly DependencyProperty DeselectionDisabledProperty = DependencyProperty.RegisterAttached(
          "DeselectionDisabled", typeof(bool), typeof(UI), new FrameworkPropertyMetadata(OnDeselectionDisabledChanged)
        );

        public static void SetDeselectionDisabled(DependencyObject element, bool value)
        {
            element.SetValue(DeselectionDisabledProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Selector))]
        public static bool GetDeselectionDisabled(DependencyObject element)
        {
            return (bool)element.GetValue(DeselectionDisabledProperty);
        }

        private static void OnDeselectionDisabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Selector selector = obj as Selector;
            if (selector != null)
            {
                if ((bool)args.NewValue == true)
                {
                    selector.SelectionChanged += HandleSelectionChanged;
                }
                else
                {
                    selector.SelectionChanged -= HandleSelectionChanged;
                }
            }
        }

        private static void HandleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Selector selector = e.OriginalSource as Selector;
            ListBox listBox = e.OriginalSource as ListBox;

            if (selector.SelectedItem == null && e.RemovedItems.Count > 0)
            {
                if (listBox == null || listBox.SelectionMode == SelectionMode.Single)
                {
                    selector.SelectedItem = e.RemovedItems[0];
                }
                else if (listBox != null)
                {
                    foreach (var item in e.RemovedItems)
                        listBox.SelectedItems.Add(item);
                }
            }
        }

        public static readonly DependencyProperty HintTextProperty = DependencyProperty.RegisterAttached(
            "HintText", typeof(string), typeof(UI), new PropertyMetadata(OnHintTextChanged)
        );

        public static void SetHintText(DependencyObject element, string value)
        {
            element.SetValue(HintTextProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static string GetHintText(DependencyObject element)
        {
            return (string)element.GetValue(HintTextProperty);
        }

        private static void OnHintTextChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            Control control = (Control)source;
            string hintText = (string)args.NewValue;

            if (!String.IsNullOrEmpty(hintText))
            {
                if (control.IsLoaded)
                {
                    HintTextAdorner.RemoveHintTexts(control);
                    HintTextAdorner.AddHintText(control, hintText);
                }
                else
                {
                    control.Loaded += HandleHintTextControlLoaded;
                }
            }
            else
            {
                control.Loaded -= HandleHintTextControlLoaded;
            }
        }

        private static void HandleHintTextControlLoaded(object sender, RoutedEventArgs e)
        {
            Control control = (Control)sender;
            string hintText = GetHintText((Control)sender);
            HintTextAdorner.RemoveHintTexts(control);
            HintTextAdorner.AddHintText(control, hintText);
        }

        public static readonly DependencyProperty ShowToolTipWhenTrimmedProperty = DependencyProperty.RegisterAttached(
            "ShowToolTipWhenTrimmed", typeof(bool), typeof(UI), new PropertyMetadata(OnShowToolTipWhenTrimmedChanged)
        );

        public static void SetShowToolTipWhenTrimmed(DependencyObject element, bool value)
        {
            element.SetValue(ShowToolTipWhenTrimmedProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static bool GetShowToolTipWhenTrimmed(DependencyObject element)
        {
            return (bool)element.GetValue(ShowToolTipWhenTrimmedProperty);
        }

        private static void OnShowToolTipWhenTrimmedChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            TextBlock textBlock = source as TextBlock;
            if (textBlock != null)
            {
                if ((bool)args.NewValue == true)
                {
                    textBlock.SizeChanged += HandleTextBlockSizeChanged;
                    InvalidateToolTip(textBlock);
                }
                else
                {
                    textBlock.SizeChanged -= HandleTextBlockSizeChanged;
                }
            }
        }

        private static void HandleTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            InvalidateToolTip(textBlock);
        }

        private static void InvalidateToolTip(TextBlock textBlock)
        {
            if (textBlock.TextTrimming != TextTrimming.None)
            {
                bool isTrimmed = IsTextTrimmed(textBlock);
                if (isTrimmed)
                {
                    textBlock.ToolTip = textBlock.Text;
                }
                else
                {
                    textBlock.ToolTip = null;
                }
            }
        }

        private static bool IsTextTrimmed(TextBlock textBlock)
        {
            Typeface typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);

            FormattedText formattedText = new FormattedText(textBlock.Text, Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection, typeface, textBlock.FontSize, textBlock.Foreground);

            return formattedText.Width > textBlock.ActualWidth;
        }

        public static readonly DependencyProperty ToggledContentStoreProperty = DependencyProperty.RegisterAttached(
            "ToggledContentStore", typeof(object), typeof(UI)
        );

        private static void SetToggledContentStore(DependencyObject element, object value)
        {
            element.SetValue(ToggledContentStoreProperty, value);
        }

        private static object GetToggledContentStore(DependencyObject element)
        {
            return (object)element.GetValue(ToggledContentStoreProperty);
        }

        public static readonly DependencyProperty ToggledContentProperty = DependencyProperty.RegisterAttached(
            "ToggledContent", typeof(object), typeof(UI), new PropertyMetadata(OnToggledContentChanged)
        );

        public static void SetToggledContent(DependencyObject element, object value)
        {
            element.SetValue(ToggledContentProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(ToggleButton))]
        public static object GetToggledContent(DependencyObject element)
        {
            return (object)element.GetValue(ToggledContentProperty);
        }

        private static void OnToggledContentChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            ToggleButton button = (ToggleButton)source;
            object oldValue = args.OldValue;
            object newValue = args.NewValue;

            if (newValue != null)
            {
                button.Checked += HandleToggleButtonChecked;
                button.Unchecked += HandleToggleButtonChecked;
            }
            else
            {
                button.Checked -= HandleToggleButtonChecked;
                button.Unchecked -= HandleToggleButtonChecked;
            }
        }

        private static void HandleToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            if (button.IsChecked == true)
            {
                SetToggledContentStore(button, button.Content);
                button.Content = GetToggledContent(button);
            }
            else
            {
                button.Content = GetToggledContentStore(button);
                SetToggledContentStore(button, null);
            }
        }

        public static readonly DependencyProperty SliderThumbContentProperty = DependencyProperty.RegisterAttached(
            "SliderThumbContent", typeof(object), typeof(UI)
        );

        public static void SetSliderThumbContent(DependencyObject element, object value)
        {
            element.SetValue(SliderThumbContentProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Slider))]
        public static object GetSliderThumbContent(DependencyObject element)
        {
            return (object)element.GetValue(SliderThumbContentProperty);
        }

        public static readonly DependencyProperty FullScreenStateProperty = DependencyProperty.RegisterAttached(
            "FullScreenState", typeof(FullScreenWindowStateInfo), typeof(UI)
        );

        public static void SetFullScreenState(DependencyObject element, FullScreenWindowStateInfo value)
        {
            element.SetValue(FullScreenStateProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static FullScreenWindowStateInfo GetFullScreenState(DependencyObject element)
        {
            return (FullScreenWindowStateInfo)element.GetValue(FullScreenStateProperty);
        }

        public static readonly DependencyProperty InlinesProperty = DependencyProperty.RegisterAttached(
            "Inlines", typeof(object), typeof(UI), new PropertyMetadata(OnInlinesChanged)
        );

        public static void SetInlines(DependencyObject element, object value)
        {
            element.SetValue(InlinesProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static object GetInlines(DependencyObject element)
        {
            return (object)element.GetValue(InlinesProperty);
        }

        private static void OnInlinesChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            TextBlock textBlock = (TextBlock)source;
            textBlock.Inlines.Clear();

            object value = args.NewValue;
            Inline single;
            IEnumerable<Inline> many;

            if ((single = value as Inline) != null)
            {
                textBlock.Inlines.Add(single);
            }
            else if ((many = value as IEnumerable<Inline>) != null)
            {
                foreach (var inline in many)
                {
                    textBlock.Inlines.Add(inline);
                }
            }
            else if (value != null)
            {
                Run run = new Run(value.ToString());
                textBlock.Inlines.Add(run);
            }
        }

        public static readonly DependencyProperty CancelDialogBehaviorProperty = DependencyProperty.RegisterAttached(
            "CancelDialogBehavior", typeof(bool), typeof(UI), new PropertyMetadata(OnCancelDialogBehaviorChanged)
        );

        public static void SetCancelDialogBehavior(DependencyObject element, bool value)
        {
            element.SetValue(CancelDialogBehaviorProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Button))]
        public static bool GetCancelDialogBehavior(DependencyObject element)
        {
            return (bool)element.GetValue(CancelDialogBehaviorProperty);
        }

        private static void OnCancelDialogBehaviorChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            Button button = (Button)source;

            bool oldCancelDialogBehavior = (bool)args.OldValue;
            bool newCancelDialogBehavior = (bool)args.NewValue;

            if (oldCancelDialogBehavior)
            {
                button.Click -= HandleCancelDialogButtonClicked;
            }

            if (newCancelDialogBehavior)
            {
                button.Click += HandleCancelDialogButtonClicked;
            }
        }

        private static void HandleCancelDialogButtonClicked(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Window window = Window.GetWindow(button);
            if (window != null)
            {
                CloseOrCancel(window);
            }
        }

        private static void CloseOrCancel(Window window)
        {
            if (ComponentDispatcher.IsThreadModal)
            {
                // The current thread is displaying a modal dialog (hopefully the window :))
                window.DialogResult = false;
            }
            else
            {
                // Setting DialogResult on a non-modal window of course throws
                window.Close();
            }
        }

        public static readonly DependencyProperty DragMoveBehaviorProperty = DependencyProperty.RegisterAttached(
            "DragMoveBehavior", typeof(bool), typeof(UI), new PropertyMetadata(OnDragMoveBehaviorChanged)
        );

        public static void SetDragMoveBehavior(DependencyObject element, bool value)
        {
            element.SetValue(DragMoveBehaviorProperty, value);
        }

        public static bool GetDragMoveBehavior(DependencyObject element)
        {
            return (bool)element.GetValue(DragMoveBehaviorProperty);
        }

        private static void OnDragMoveBehaviorChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement element = (FrameworkElement)source;

            bool oldDragMoveBehavior = (bool)args.OldValue;
            bool newDragMoveBehavior = (bool)args.NewValue;

            if (oldDragMoveBehavior)
            {
                element.MouseLeftButtonDown -= HandleDragMoveMouseDown;
            }

            if (newDragMoveBehavior)
            {
                element.MouseLeftButtonDown += HandleDragMoveMouseDown;
            }
        }

        private static void HandleDragMoveMouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Window window = Window.GetWindow(element);
            if (window != null)
            {
                window.DragMove();
            }
        }
    }

    /// <summary>
    /// Defines the different values that can be taken on a window, when the Escape key is pressed.
    /// </summary>
    public enum EscapeAction
    {
        None,
        Close,
        Hide
    }

    public enum ButtonType
    {
        None,
        OK,
        Cancel
    }
}
