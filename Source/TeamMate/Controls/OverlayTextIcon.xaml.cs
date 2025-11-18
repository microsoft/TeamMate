using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for OverlayTextIcon.xaml
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class OverlayTextIcon : UserControl
    {
        public OverlayTextIcon()
        {
            InitializeComponent();
            InvalidateFontSize();
        }

        public BitmapSource CaptureBitmap()
        {
            return BitmapUtilities.CaptureUnsourcedBitmap(this);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(OverlayTextIcon), new PropertyMetadata(OnTextChanged)
        );

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((OverlayTextIcon)d).InvalidateFontSize();
        }

        private void InvalidateFontSize()
        {
            int textLength = (Text != null) ? Text.Length : 0;
            this.FontSize = (textLength >= 3) ? 8 : 11;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty HasUpdatesProperty = DependencyProperty.Register(
            "HasUpdates", typeof(bool), typeof(OverlayTextIcon)
        );

        public bool HasUpdates
        {
            get { return (bool)GetValue(HasUpdatesProperty); }
            set { SetValue(HasUpdatesProperty, value); }
        }
    }
}
