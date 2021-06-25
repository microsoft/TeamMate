using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for GlobalCommandBar.xaml
    /// </summary>
    public partial class GlobalCommandBar : UserControl
    {
        public GlobalCommandBar()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof(CommandBarType), typeof(GlobalCommandBar)
        );

        public CommandBarType Type
        {
            get { return (CommandBarType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
    }
}
