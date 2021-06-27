using System.Windows;
using System.Windows.Controls.Primitives;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    public class SplitViewButton : ButtonBase
    {
        public const double IconWidth = 48;

        static SplitViewButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitViewButton), new FrameworkPropertyMetadata(typeof(SplitViewButton)));
        }

        public SplitViewButton()
        {
            this.SizeChanged += SplitViewButton_SizeChanged;
        }

        private void SplitViewButton_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            IsExpanded = (this.ActualWidth > IconWidth);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(object), typeof(SplitViewButton)
        );

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(SplitViewButton)
        );

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            private set { SetValue(IsExpandedProperty, value); }
        }
    }
}
