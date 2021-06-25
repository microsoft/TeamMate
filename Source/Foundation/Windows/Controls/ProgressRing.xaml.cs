using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Media.Animation;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Interaction logic for ProgressRing.xaml
    /// </summary>
    public partial class ProgressRing
    {
        private const double MinimumSizeForExtraCircle = 60;

        /// <summary>
        /// The is extra circle visible property
        /// </summary>
        public static readonly DependencyProperty IsExtraCircleVisibleProperty = DependencyProperty.Register(
            "IsExtraCircleVisible", typeof(bool), typeof(ProgressRing));

        /// <summary>
        /// The ellipse diameter property
        /// </summary>
        public static readonly DependencyProperty EllipseDiameterProperty = DependencyProperty.Register(
            "EllipseDiameter", typeof(double), typeof(ProgressRing));

        /// <summary>
        /// The ellipse offset property
        /// </summary>
        public static readonly DependencyProperty EllipseOffsetProperty = DependencyProperty.Register(
            "EllipseOffset", typeof(Thickness), typeof(ProgressRing));

        /// <summary>
        /// The size property
        /// </summary>
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(ProgressRingSize), typeof(ProgressRing), new PropertyMetadata(ProgressRingSize.Small)
        );

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressRing"/> class.
        /// </summary>
        public ProgressRing()
        {
            InitializeComponent();

            Storyboard rotateStoryBoard = (Storyboard)Resources["Storyboard"];

            AnimationHelper animationHelper = new AnimationHelper(this, rotateStoryBoard);
            animationHelper.Enable();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            InvalidateSize();
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        public ProgressRingSize Size
        {
            get { return (ProgressRingSize)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        /// <summary>
        /// Gets the ellipse diameter.
        /// </summary>
        public double EllipseDiameter
        {
            get { return (double)GetValue(EllipseDiameterProperty); }
            private set { SetValue(EllipseDiameterProperty, value); }
        }

        /// <summary>
        /// Gets the ellipse offset.
        /// </summary>
        public Thickness EllipseOffset
        {
            get { return (Thickness)GetValue(EllipseOffsetProperty); }
            private set { SetValue(EllipseOffsetProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether there is an extra circle visible.
        /// </summary>
        public bool IsExtraCircleVisible
        {
            get { return (bool)GetValue(IsExtraCircleVisibleProperty); }
            private set { SetValue(IsExtraCircleVisibleProperty, value); }
        }

        /// <summary>
        /// Updates the sizes.
        /// </summary>
        private void InvalidateSize()
        {
            double size = ActualWidth;

            EllipseDiameter = size * 0.1;

            double offset = Math.Max(0, (size - EllipseDiameter) / 2);
            EllipseOffset = new Thickness(0, offset, 0, 0);

            IsExtraCircleVisible = (size >= MinimumSizeForExtraCircle);
        }
   }

    /// <summary>
    /// The well know progress ring size.
    /// </summary>
    public enum ProgressRingSize
    {
        Custom,
        Small,
        Medium,
        Large
    }
}