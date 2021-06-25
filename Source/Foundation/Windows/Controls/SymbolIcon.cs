using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls
{
    public class SymbolIcon : FontIcon
    {
        static SymbolIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SymbolIcon), new FrameworkPropertyMetadata(typeof(SymbolIcon)));
        }

        public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
            "Symbol", typeof(Symbol), typeof(SymbolIcon), new PropertyMetadata((d, e) => ((SymbolIcon)d).OnSymbolChanged())
        );

        public Symbol Symbol
        {
            get { return (Symbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        private void OnSymbolChanged()
        {
            this.Glyph = ((char)this.Symbol).ToString();
        }
    }
}
