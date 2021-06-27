using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        private const double ZoomIncrement = 0.1;

        private bool panning;
        private Point panningPoint;

        private Window loadedWindow;

        public ImageViewer()
        {
            InitializeComponent();
            container.PreviewMouseLeftButtonDown += HandleMouseLeftButtonDown;
            container.PreviewMouseMove += HandleMouseMove;
            container.PreviewMouseLeftButtonUp += HandleMouseLeftButtonUp;

            ResetZoomCommand = new RelayCommand(SetInitialZoomFactor);
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);

            this.Loaded += HandleLoaded;
            this.Unloaded += HandleUnloaded;

            this.CommandBindings.Add(ApplicationCommands.Copy, CopyImageToClipboard);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(ImageViewer), new PropertyMetadata(null, OnSourceChanged)
        );

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageViewer viewer = (ImageViewer)d;

            ImageSource newSource = (ImageSource)e.NewValue;
            if (newSource != null)
            {
                // A new image was set, calculate the initial preferred zoom factor to fit the full image in
                viewer.SetInitialZoomFactor();
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }


        /// <summary>
        /// Gets the reset zoom command.
        /// </summary>
        public ICommand ResetZoomCommand { get; private set; }

        /// <summary>
        /// Gets the zoom in command.
        /// </summary>
        public ICommand ZoomInCommand { get; private set; }

        /// <summary>
        /// Gets the zoom out command.
        /// </summary>
        public ICommand ZoomOutCommand { get; private set; }

        /// <summary>
        /// Gets the UI element used to display the preview of the file.
        /// </summary>
        public FrameworkElement Host
        {
            get { return this; }
        }

        /// <summary>
        /// Invalidates the cursor.
        /// </summary>
        private void InvalidateCursor()
        {
            container.Cursor = (CanPan()) ? ControlResources.OpenHandCursor : null;
        }

        /// <summary>
        /// Determines whether this instance can pan.
        /// </summary>
        private bool CanPan()
        {
            return scrollViewer.ScrollableHeight > 0 || scrollViewer.ScrollableWidth > 0;
        }

        /// <summary>
        /// Sets the initial zoom factor to fit in the current viewport size.
        /// </summary>
        private void SetInitialZoomFactor()
        {
            BitmapImage loadedImage = image.Source as BitmapImage;

            if (loadedImage != null)
            {
                double logicalPixelWidth, logicalPixelHeight;
                BitmapUtilities.GetLogicalSize(loadedImage, out logicalPixelWidth, out logicalPixelHeight);

                if (logicalPixelWidth <= ActualWidth && logicalPixelHeight <= ActualHeight)
                {
                    UI.ResetZoomFactor(container);
                }
                else if (ActualWidth > 0 && ActualHeight > 0)
                {
                    // TODO: On first display, this component is not materialized so actual width and height are 0 still, what do we do in that case?
                    double padding = 20;
                    double viewWidth = Math.Max(0, ActualWidth - padding);
                    double viewHeight = Math.Max(0, ActualHeight - padding);

                    double scaleFactor = BitmapUtilities.GetScaleFactor(loadedImage, viewWidth, viewHeight);
                    UI.SetZoomFactor(container, scaleFactor);
                }
            }
        }

        /// <summary>
        /// Zooms in by a predefined increment.
        /// </summary>
        private void ZoomIn()
        {
            UI.IncrementZoomFactor(container, ZoomIncrement);
        }

        /// <summary>
        /// Zooms out by a predefined increment.
        /// </summary>
        private void ZoomOut()
        {
            UI.IncrementZoomFactor(container, -ZoomIncrement);
        }

        /// <summary>
        /// Copies the image to clipboard.
        /// </summary>
        private void CopyImageToClipboard()
        {
            var bitmapSource = image.Source as BitmapSource;
            if (bitmapSource != null)
            {
                Clipboard.SetImage(bitmapSource);
            }
        }

        /// <summary>
        /// Handles the loaded event.
        /// </summary>
        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            loadedWindow = Window.GetWindow(this);
            if (loadedWindow != null)
            {
                // TODO: Check there is no existing binding for this KeyGesture
                loadedWindow.InputBindings.Add(new KeyBinding(ResetZoomCommand, new KeyGesture(Key.D0, ModifierKeys.Control)));
                loadedWindow.InputBindings.Add(new KeyBinding(ZoomInCommand, new KeyGesture(Key.OemPlus, ModifierKeys.Control)));
                loadedWindow.InputBindings.Add(new KeyBinding(ZoomOutCommand, new KeyGesture(Key.OemMinus, ModifierKeys.Control)));
            }
        }

        /// <summary>
        /// Handles the unloaded event.
        /// </summary>
        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            if (loadedWindow != null)
            {
                loadedWindow.InputBindings.Remove(ResetZoomCommand);
                loadedWindow.InputBindings.Remove(ZoomInCommand);
                loadedWindow.InputBindings.Remove(ZoomOutCommand);
                loadedWindow = null;
            }
        }


        /// <summary>
        /// Handles the mouse left button down.
        /// </summary>
        private void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: Refactor these to a panning helper....
            if (CanPan())
            {
                panning = true;
                panningPoint = e.GetPosition(this);
                container.CaptureMouse();
                Mouse.OverrideCursor = ControlResources.ClosedHandCursor;
            }
        }

        /// <summary>
        /// Handles the mouse move.
        /// </summary>
        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (panning)
            {
                Point oldPoint = panningPoint;
                panningPoint = e.GetPosition(this);

                double deltaX = panningPoint.X - oldPoint.X;
                double deltaY = panningPoint.Y - oldPoint.Y;

                if (scrollViewer.ScrollableWidth > 0)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - deltaX);
                }

                if (scrollViewer.ScrollableHeight > 0)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - deltaY);
                }
            }
            else
            {
                // TODO: Do not calculate this every time on mouse move... Calculate it on enter only
                InvalidateCursor();
            }
        }

        /// <summary>
        /// Handles the mouse left button up.
        /// </summary>
        private void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (panning)
            {
                panning = false;
                container.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
            }
        }
    }
}
