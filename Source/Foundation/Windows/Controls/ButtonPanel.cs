using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// A panel that can contain buttons that are automatically arranged and sized.
    /// </summary>
    public partial class ButtonPanel : Panel
    {
        static ButtonPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonPanel), new FrameworkPropertyMetadata(typeof(ButtonPanel)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonPanel"/> class.
        /// </summary>
        public ButtonPanel()
        {
            Style style = ControlResources.Controls.FindResource<Style>("ButtonPanelButtonStyle");
            Style inheritStyle = new Style(typeof(Button), style);
            Resources.Add(inheritStyle.TargetType, style);
        }

        /// <summary>
        /// Occurs when the orientation has changed.
        /// </summary>
        public event DependencyPropertyChangedEventHandler OrientationChanged;

        /// <summary>
        /// The horizontal content alignment property
        /// </summary>
        public static readonly DependencyProperty HorizontalContentAlignmentProperty = DependencyProperty.Register(
            "HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(ButtonPanel),
            new FrameworkPropertyMetadata(HorizontalAlignment.Right, FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

        /// <summary>
        /// Gets or sets the horizontal content alignment.
        /// </summary>
        public HorizontalAlignment HorizontalContentAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
            set { SetValue(HorizontalContentAlignmentProperty, value); }
        }

        /// <summary>
        /// The vertical content alignment property
        /// </summary>
        public static readonly DependencyProperty VerticalContentAlignmentProperty = DependencyProperty.Register(
            "VerticalContentAlignment", typeof(VerticalAlignment), typeof(ButtonPanel),
            new FrameworkPropertyMetadata(VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

        /// <summary>
        /// Gets or sets the vertical content alignment.
        /// </summary>
        public VerticalAlignment VerticalContentAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); }
            set { SetValue(VerticalContentAlignmentProperty, value); }
        }

        /// <summary>
        /// The orientation property
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(ButtonPanel),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged)
        );

        /// <summary>
        /// Called when the orientation changes.
        /// </summary>
        private static void OnOrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ButtonPanel buttonPanel = obj as ButtonPanel;
            buttonPanel.OrientationChanged?.Invoke(buttonPanel, args);
        }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the orientation is horizontal.
        /// </summary>
        private bool IsHorizontal
        {
            get { return Orientation == Orientation.Horizontal; }
        }

        /// <summary>
        /// The spacing property
        /// </summary>
        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
            "Spacing", typeof(double), typeof(ButtonPanel),
            new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

        /// <summary>
        /// Gets or sets the spacing between buttons.
        /// </summary>
        public double Spacing
        {
            get { return (double)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        /// <summary>
        /// Overridden to measure the panel size based on the contained buttons.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The measured panel size.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            int childCount = 0;
            foreach (UIElement child in Children)
            {
                if (child.IsVisible)
                {
                    child.Measure(availableSize);
                    childCount++;
                }
            }

            Size normalizedSize = GetNormalizedButtonSize();

            double desiredWidth = 0;
            double desiredHeight = 0;

            if (IsHorizontal)
            {
                desiredWidth = childCount * normalizedSize.Width;
                if (childCount > 1)
                {
                    desiredWidth += (childCount - 1) * Spacing;
                }

                desiredHeight = normalizedSize.Height;
            }
            else
            {
                desiredHeight = childCount * normalizedSize.Height;
                if (childCount > 1)
                {
                    desiredHeight += (childCount - 1) * Spacing;
                }

                desiredWidth = normalizedSize.Width;
            }

            // TODO: Deal with positive infinity earlier
            Size resultSize = new Size(desiredWidth, desiredHeight);
            //resultSize.Width = double.IsPositiveInfinity(availableSize.Width) ? resultSize.Width : availableSize.Width;
            //resultSize.Height = double.IsPositiveInfinity(availableSize.Height) ? resultSize.Height : availableSize.Height;
            return resultSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement" /> derived class.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>
        /// The actual size used.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            int childCount = 0;
            foreach (UIElement child in Children)
            {
                if (child.IsVisible)
                {
                    childCount++;
                }
            }

            if (childCount == 0)
                return finalSize;

            Size buttonSize = GetNormalizedButtonSize();
            double totalSpacing = (childCount > 1) ? Spacing * (childCount - 1) : 0;

            double nextX = 0;
            double nextY = 0;
            double buttonWidth = buttonSize.Width;
            double buttonHeight = buttonSize.Height;

            switch (EffectiveVerticalContentAlignment)
            {
                case VerticalAlignment.Top:
                    break;

                case VerticalAlignment.Bottom:
                    if (IsHorizontal)
                    {
                        nextY = Math.Max(0, finalSize.Height - buttonSize.Height);
                    }
                    else
                    {
                        double tentativeOffset1 = finalSize.Height - (buttonSize.Height * childCount) - totalSpacing;
                        nextY = Math.Max(0, tentativeOffset1);
                    }

                    break;

                case VerticalAlignment.Center:
                    if (IsHorizontal)
                    {
                        nextY = Math.Max(0, (finalSize.Height - buttonSize.Height) / 2);
                    }
                    else
                    {
                        double tentativeOffset2 = finalSize.Height - (buttonSize.Height * childCount) - totalSpacing;
                        nextY = Math.Max(0, tentativeOffset2 / 2);
                    }

                    break;

                case VerticalAlignment.Stretch:
                    if (IsHorizontal)
                    {
                        buttonHeight = finalSize.Height;
                    }
                    else
                    {
                        double tentativeSize = finalSize.Height - totalSpacing;

                        if (tentativeSize > 0)
                            buttonHeight = (tentativeSize / childCount);
                    }

                    break;
            }

            switch (EffectiveHorizontalContentAlignment)
            {
                case HorizontalAlignment.Left:
                    break;

                case HorizontalAlignment.Right:
                    if (IsHorizontal)
                    {
                        double tentativeOffset1 = finalSize.Width - (buttonSize.Width * childCount) - totalSpacing;
                        nextX = Math.Max(0, tentativeOffset1);
                    }
                    else
                    {
                        nextX = Math.Max(0, finalSize.Width - buttonSize.Width);
                    }

                    break;

                case HorizontalAlignment.Center:
                    if (IsHorizontal)
                    {
                        double tentativeOffset2 = finalSize.Width - (buttonSize.Width * childCount) - totalSpacing;
                        nextX = Math.Max(0, tentativeOffset2 / 2);
                    }
                    else
                    {
                        nextX = Math.Max(0, (finalSize.Width - buttonSize.Width) / 2);
                    }

                    break;

                case HorizontalAlignment.Stretch:
                    if (IsHorizontal)
                    {
                        double tentativeSize = finalSize.Width - totalSpacing;

                        if (tentativeSize > 0)
                            buttonWidth = (tentativeSize / childCount);
                    }
                    else
                    {
                        buttonWidth = finalSize.Width;
                    }

                    break;
            }

            foreach (UIElement child in Children)
            {
                if (child.IsVisible)
                {
                    Rect bounds = new Rect(nextX, nextY, buttonWidth, buttonHeight);
                    child.Arrange(bounds);

                    if (IsHorizontal)
                        nextX += bounds.Width + Spacing;
                    else
                        nextY += bounds.Height + Spacing;
                }
            }

            return finalSize;
        }


        /// <summary>
        /// Gets the normalized shared button size, based on the sizes of all child buttons.
        /// </summary>
        /// <returns>The normalized single button size.</returns>
        private Size GetNormalizedButtonSize()
        {
            Size normalizedSize = new Size(0, 0);
            foreach (UIElement child in Children)
            {
                if (child.IsVisible)
                {
                    normalizedSize.Width = Math.Max(child.DesiredSize.Width, normalizedSize.Width);
                    normalizedSize.Height = Math.Max(child.DesiredSize.Height, normalizedSize.Height);
                }
            }
            return normalizedSize;
        }

        /// <summary>
        /// Gets the effective horizontal content alignment.
        /// </summary>
        private HorizontalAlignment EffectiveHorizontalContentAlignment
        {
            get
            {
                object value = ReadLocalValue(HorizontalContentAlignmentProperty);
                if (value != DependencyProperty.UnsetValue)
                {
                    return (HorizontalAlignment)value;
                }

                return (IsHorizontal) ? HorizontalAlignment.Right : HorizontalAlignment.Center;
            }
        }

        /// <summary>
        /// Gets the effective vertical content alignment.
        /// </summary>
        private VerticalAlignment EffectiveVerticalContentAlignment
        {
            get
            {
                object value = ReadLocalValue(VerticalContentAlignmentProperty);
                if (value != DependencyProperty.UnsetValue)
                {
                    return (VerticalAlignment)value;
                }

                return (IsHorizontal) ? VerticalAlignment.Center : VerticalAlignment.Top;
            }
        }
    }
}
