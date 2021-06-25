using Microsoft.Internal.Tools.TeamMate.Foundation.Native;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.DragAndDrop;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Microsoft.Internal.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for OverviewWindow.xaml
    /// </summary>
    public partial class OverviewWindow : Window
    {
        private Point? mouseLeftButtonDownPoint;
        private bool isDragging = false;

        public OverviewWindow()
        {
            InitializeComponent();
            View.Initialize(this);

            this.MouseLeftButtonDown += OverviewWindow_MouseLeftButtonDown;
            this.MouseMove += OverviewWindow_MouseMove;
            this.MouseLeftButtonUp += OverviewWindow_MouseLeftButtonUp;
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Additional trick to avoid showing this window in the ALT-TAB selector
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = NativeMethods.GetWindowLong(wndHelper.Handle, GetWindowLong.GWL_EXSTYLE);
            exStyle |= (int)WindowStyles.WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLong(wndHelper.Handle, GetWindowLong.GWL_EXSTYLE, exStyle);
        }

        private OverviewWindowViewModel ViewModel
        {
            get { return this.DataContext as OverviewWindowViewModel; }
        }

        private void OverviewWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseLeftButtonDownPoint = null;

            if (!isDragging)
            {
                if (this.ViewModel != null)
                {
                    this.ViewModel.ShowMainWindow();
                    e.Handled = true;
                }
            }
            else
            {
                isDragging = false;
            }
        }

        private void OverviewWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseLeftButtonDownPoint != null && !isDragging && DragDropUtilities.ShouldBeginDrag(mouseLeftButtonDownPoint.Value, e.GetPosition(this)))
            {
                mouseLeftButtonDownPoint = null;
                isDragging = true;
                DragMoveWithinBoundsHelper dragMoveHelper = new DragMoveWithinBoundsHelper(this);
                dragMoveHelper.DragMove();
            }
        }

        private void OverviewWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            mouseLeftButtonDownPoint = e.GetPosition(this);
        }
    }
}
