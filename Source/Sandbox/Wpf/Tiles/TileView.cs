using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf.Tiles
{
    public class TileView : ContentControl
    {
        static TileView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TileView), new FrameworkPropertyMetadata(typeof(TileView)));
        }
    }
}
