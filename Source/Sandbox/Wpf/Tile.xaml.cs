using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        public event EventHandler Activated;

        public static readonly DependencyProperty TileSizeProperty = DependencyProperty.Register(
            "TileSize", typeof(TileSize), typeof(Tile), new PropertyMetadata(TileSize.Double, (d, e) => ((Tile)d).InvalidateSize())
        );

        public TileSize TileSize
        {
            get { return (TileSize)GetValue(TileSizeProperty); }
            set { SetValue(TileSizeProperty, value); }
        }

        private void InvalidateSize()
        {
            // TODO: Account for border width in a cleaner way, or set size on the inner element
            int borderPadding = 3 * 2;

            if (TileSize == TileSize.Single)
            {
                this.Width = 120 + borderPadding;
                this.Height = 120 + borderPadding;
            }
            else if (TileSize == TileSize.Double)
            {
                this.Width = 248 + borderPadding;
                this.Height = 120 + borderPadding;
            }
        }

        public Tile()
        {
            this.InvalidateSize();

            InitializeComponent();

            this.MouseLeftButtonDown += Tile_MouseLeftButtonDown;
            this.MouseLeftButtonUp += Tile_MouseLeftButtonUp;
            this.KeyDown += Tile_KeyDown;
            this.KeyUp += Tile_KeyUp;
        }

        void Tile_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Activate();
                e.Handled = true;
            }
        }

        private void Activate()
        {
            if (IsPressed)
            {
                IsPressed = false;
                Activated?.Invoke(this, EventArgs.Empty);
            }
        }

        void Tile_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsPressed = true;
                e.Handled = true;
            }
        }

        void Tile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            Activate();
        }

        void Tile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsPressed = true;
            this.CaptureMouse();
        }

        public static readonly DependencyProperty IsPressedProperty = DependencyProperty.Register(
            "IsPressed", typeof(bool), typeof(Tile)
        );

        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }
    }

    public enum TileSize
    {
        Single,
        Double
    }
}
