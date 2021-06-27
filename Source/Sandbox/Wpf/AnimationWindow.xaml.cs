// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for AnimationWindow.xaml
    /// </summary>
    public partial class AnimationWindow : Window
    {
        public AnimationWindow()
        {
            InitializeComponent();

            parent.PreviewMouseLeftButtonDown  += container_MouseLeftButtonDown;
        }

        void container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 1. Capture source location
            Point sourcePoint = rectangle.TranslatePoint(new Point(), parent);

            // 2. Capture current view
            Image image = new Image();
            image.Source = BitmapUtilities.CaptureBitmap(rectangle);
            image.Width = rectangle.ActualWidth;
            image.Height = rectangle.ActualHeight;

            rectangle.Fill = (rectangle.Fill == Brushes.Red) ? Brushes.Blue : Brushes.Red;

            // 3. Show captured view in top layer
            this.topCanvas.Visibility = Visibility.Visible;
            this.topCanvas.Children.Add(image);
            Canvas.SetLeft(image, sourcePoint.X);
            Canvas.SetTop(image, sourcePoint.Y);

            // 4. Hide source item
            rectangle.Visibility = Visibility.Hidden;

            // 5. Relayout (move source item while hidden)
            rectangle.HorizontalAlignment = (rectangle.HorizontalAlignment != HorizontalAlignment.Right)? HorizontalAlignment.Right : HorizontalAlignment.Center;
            rectangle.VerticalAlignment = (rectangle.VerticalAlignment != VerticalAlignment.Bottom)? VerticalAlignment.Bottom : VerticalAlignment.Center;
            parent.InvalidateArrange();
            parent.UpdateLayout();

            // 6. Capture target location
            Point targetPoint = rectangle.TranslatePoint(new Point(), parent);

            // 7. Build and run storyboard transitions
            Storyboard storyboard = new Storyboard();

            Duration duration = new Duration(TimeSpan.FromSeconds(0.3));

            AnimateTranslation(image, targetPoint, duration, storyboard);

            storyboard.Completed += delegate(object sender2, EventArgs e2)
            {
                // 8. Hide top layer
                this.topCanvas.Children.Clear();
                this.topCanvas.Visibility = Visibility.Hidden;

                // 9. Show source item
                rectangle.Visibility = Visibility.Visible;
            };

            storyboard.Begin();
        }

        private static void AnimateTranslation(UIElement element, Point targetPoint, Duration duration, Storyboard storyboard)
        {
            DoubleAnimation animationX = new DoubleAnimation(targetPoint.X, duration);
            DoubleAnimation animationY = new DoubleAnimation(targetPoint.Y, duration);

            Storyboard.SetTargetProperty(animationX, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTargetProperty(animationY, new PropertyPath("(Canvas.Top)"));

            Storyboard.SetTarget(animationX, element);
            Storyboard.SetTarget(animationY, element);

            storyboard.Children.Add(animationX);
            storyboard.Children.Add(animationY);
        }
    }
}
