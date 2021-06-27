using Microsoft.Tools.TeamMate.Foundation.Windows.Shell;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Media.Capture
{
    public class BoundsSelectionWindow : Window
    {
        private const double WindowOpacity = 0.65;
        private SolidColorBrush semiTransparentColor = new SolidColorBrush(Color.FromArgb((int)(255 * WindowOpacity), 255, 255, 255));
        private SolidColorBrush almostTransparentColor = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255));

        private bool isDragging;
        private bool hasSecondPoint;
        private Point mouseDown;
        private Point otherEnd;

        private bool isClosing;

        private Int32Rect? capturedBounds;

        public BoundsSelectionWindow()
        {
            ShowInTaskbar = false;
            ShowActivated = true;
            Topmost = true;
            WindowStartupLocation = WindowStartupLocation.Manual;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
            Cursor = Cursors.Cross;

            this.Loaded += HandleLoaded;
        }

        private bool minimizedAll;
        private BitmapSource screenBitmap;

        public bool SelectBounds(BitmapSource screenBitmap, out Int32Rect bounds)
        {
            this.capturedBounds = null;
            this.minimizedAll = false;
            this.screenBitmap = screenBitmap;

            if(ShellUtilities.IsInMetroMode())
            {
                // KLUDGE: MinimizeAll does not minimize Metro windows, use ToggleDesktop by design
                ShellUtilities.ToggleDesktop();
                this.minimizedAll = true;
            }

            ShowDialog();

            this.screenBitmap = null;

            if (capturedBounds != null)
            {
                bounds = capturedBounds.Value;
                return true;
            }

            bounds = new Int32Rect();
            return false;
        }

        public bool SelectBounds(out Int32Rect bounds)
        {
            return SelectBounds(null, out bounds);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            isDragging = true;
            hasSecondPoint = false;
            mouseDown = e.GetPosition(this);
            InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (isDragging)
            {
                hasSecondPoint = true;
                otherEnd = e.GetPosition(this);
                InvalidateVisual();
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (isDragging)
            {
                isDragging = false;
                hasSecondPoint = false;

                Point mouseUp = e.GetPosition(this);
                Rect bounds = new Rect(mouseDown, mouseUp);

                int width = (int)bounds.Width;
                int height = (int)bounds.Height;

                if (width > 0 && height > 0)
                {
                    this.capturedBounds = new Int32Rect((int)bounds.X, (int)bounds.Y, width, height);
                }

                TryClose();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect fullScreenBounds = new Rect(0, 0, Width, Height);

            if(this.screenBitmap != null)
            {
                drawingContext.DrawImage(this.screenBitmap, fullScreenBounds);
            }

            if (isDragging && hasSecondPoint && mouseDown.X != otherEnd.X && mouseDown.Y != otherEnd.Y)
            {
                Rect selectionBounds = new Rect(mouseDown, otherEnd);

                CombinedGeometry cg = new CombinedGeometry(GeometryCombineMode.Exclude, 
                    new RectangleGeometry(fullScreenBounds), new RectangleGeometry(selectionBounds));

                drawingContext.DrawGeometry(semiTransparentColor, null, cg);

                // Need to draw an almost transparent rectangle over the selection, otherwise, mouse inputs get
                // sent to the window under the selected box
                drawingContext.DrawGeometry(almostTransparentColor, null, new RectangleGeometry(selectionBounds));

                Pen pen = new Pen(Brushes.Black, ThicknessInPixels(1));
                double halfPenWidth = pen.Thickness / 2;

                // Using guidelines will "snap the rectangle to device pixels"
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(selectionBounds.Left + halfPenWidth);
                guidelines.GuidelinesX.Add(selectionBounds.Right + halfPenWidth);
                guidelines.GuidelinesY.Add(selectionBounds.Top + halfPenWidth);
                guidelines.GuidelinesY.Add(selectionBounds.Bottom + halfPenWidth);

                drawingContext.PushGuidelineSet(guidelines);
                drawingContext.DrawRectangle(null, pen, selectionBounds);
                drawingContext.Pop();
            }
            else
            {
                drawingContext.DrawRectangle(semiTransparentColor, null, fullScreenBounds);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            isClosing = true;

            if (minimizedAll)
            {
                ShellUtilities.UndoMinimizeAll();
            }
            
            base.OnClosing(e);
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            // KLUDGE: This trick is necessary to ensure the windows has focus when shown.
            // This window is not the foreground window if we used the Shell ToggleDesktop trick.
            WindowUtilities.ForceToForeground(this);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Escape)
            {
                TryClose();
                e.Handled = true;
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            TryClose();
        }

        private void TryClose()
        {
            if (!isClosing)
            {
                Close();
            }
        }

        private double ThicknessInPixels(int pixels)
        {
            Matrix matrix = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            double dpiFactor = 1 / matrix.M11;
            return pixels * dpiFactor;
        }
    }
}