using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for MetroTile.xaml
    /// </summary>
    public partial class MetroTile4 : UserControl
    {
        public MetroTile4()
        {
            InitializeComponent();
            transition.MouseLeftButtonDown += transition_MouseLeftButtonDown;
        }

        void transition_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ci = transition.CurrentItem;
            if (ci != null)
            {
                var title = (ci as TileDataModel).Title;
                MessageBox.Show("Clicked on " + title);
            }
        }
    }
}
