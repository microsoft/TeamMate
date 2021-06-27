using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    public class FontIcon : Control
    {
        static FontIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontIcon), new FrameworkPropertyMetadata(typeof(FontIcon)));
        }

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
            "Glyph", typeof(string), typeof(FontIcon)
        );

        public string Glyph
        {
            get { return (string)GetValue(GlyphProperty); }
            set { SetValue(GlyphProperty, value); }
        }
    }
}
