using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls;
using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Controls
{
    public class MsnIcon : FontIcon
    {
        public static readonly DependencyProperty MsnProperty = DependencyProperty.Register(
            "Symbol", typeof(MsnSymbol), typeof(MsnIcon), new PropertyMetadata((d, e) => ((MsnIcon)d).OnSymbolChanged())
        );

        public MsnSymbol Symbol
        {
            get { return (MsnSymbol)GetValue(MsnProperty); }
            set { SetValue(MsnProperty, value); }
        }

        private void OnSymbolChanged()
        {
            this.Glyph = ((char)this.Symbol).ToString();
        }
    }
}
