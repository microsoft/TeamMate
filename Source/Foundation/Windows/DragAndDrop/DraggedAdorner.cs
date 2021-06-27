using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.DragAndDrop
{
    /// <summary>
    /// An adorner that displays a preview of the dragged item.
    /// </summary>
    public class DraggedAdorner : Adorner
    {
        private const double DefaultOpacity = 0.7;

        private FrameworkElement content;
        private double left;
        private double top;
        private AdornerLayer adornerLayer;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="adornerLayer">The adorner layer.</param>
        /// <param name="content">The content to display.</param>
        private DraggedAdorner(FrameworkElement adornedElement, AdornerLayer adornerLayer, FrameworkElement content)
            : base(adornedElement)
        {
            this.adornerLayer = adornerLayer;
            this.content = content;
            this.content.Opacity = DefaultOpacity;
            this.adornerLayer.Add(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DraggedAdorner"/> class.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="adornerLayer">The adorner layer.</param>
        public DraggedAdorner(FrameworkElement adornedElement, AdornerLayer adornerLayer)
            : this(adornedElement, adornerLayer, CreateContent(adornedElement))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DraggedAdorner"/> class.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="adornerLayer">The adorner layer.</param>
        /// <param name="data">The data to be rendered.</param>
        /// <param name="template">The data template.</param>
        public DraggedAdorner(FrameworkElement adornedElement, AdornerLayer adornerLayer, object data, DataTemplate template)
            : this(adornedElement, adornerLayer, CreateContent(data, template))
        {
        }

        /// <summary>
        /// Creates the content preview element to display during dragging.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <returns>The created preview item.</returns>
        private static FrameworkElement CreateContent(FrameworkElement adornedElement)
        {
            Canvas canvas = new Canvas();
            canvas.Background = new VisualBrush(adornedElement);
            canvas.Width = adornedElement.ActualWidth;
            canvas.Height = adornedElement.ActualHeight;
            return canvas;
        }

        /// <summary>
        /// Creates the content, given drag and drop data and a custom tempalte used to render that data.
        /// </summary>
        /// <param name="dragDropData">The drag drop data.</param>
        /// <param name="dragDropTemplate">The drag drop template.</param>
        /// <returns>A content presenter.</returns>
        private static ContentPresenter CreateContent(object dragDropData, DataTemplate dragDropTemplate)
        {
            ContentPresenter presenter = new ContentPresenter();
            presenter.Content = dragDropData;
            presenter.ContentTemplate = dragDropTemplate;
            return presenter;
        }

        /// <summary>
        /// Gets the content that will be previewed.
        /// </summary>
        public FrameworkElement Content
        {
            get { return this.content; }
        }

        /// <summary>
        /// Sets the position of the adorned element.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        public void SetPosition(double left, double top)
        {
            this.left = left;
            this.top = top;
            if (this.adornerLayer != null)
            {
                this.adornerLayer.Update(this.AdornedElement);
            }
        }

        /// <summary>
        /// Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Size" /> object representing the amount of layout space needed by the adorner.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            this.content.Measure(constraint);
            return this.content.DesiredSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement" /> derived class.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>
        /// The actual size used.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            this.content.Arrange(new Rect(finalSize));
            return finalSize;
        }

        /// <summary>
        /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)" />, and returns a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>
        /// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
        /// </returns>
        protected override Visual GetVisualChild(int index)
        {
            return this.content;
        }

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>The number of visual child elements for this element.</returns>
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.Windows.Media.Transform" /> for the adorner, based on the transform that is currently applied to the adorned element.
        /// </summary>
        /// <param name="transform">The transform that is currently applied to the adorned element.</param>
        /// <returns>
        /// A transform to apply to the adorner.
        /// </returns>
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(this.left, this.top));

            return result;
        }

        /// <summary>
        /// Detaches this instance.
        /// </summary>
        public void Detach()
        {
            this.adornerLayer.Remove(this);
        }

    }
}
