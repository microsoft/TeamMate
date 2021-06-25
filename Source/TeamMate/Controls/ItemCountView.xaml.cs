using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Internal.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for ItemCountView.xaml
    /// </summary>
    public partial class ItemCountView : UserControl
    {
        public ItemCountView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            "Image", typeof(ImageSource), typeof(ItemCountView)
        );

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            "Brush", typeof(Brush), typeof(ItemCountView)
        );

        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }
    }
}
