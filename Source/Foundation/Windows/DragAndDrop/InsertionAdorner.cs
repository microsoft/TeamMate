// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.DragAndDrop
{
    /// <summary>
    /// An adorner used to indicate the insertion point of an item.
    /// </summary>
    /// <remarks>
    /// Used by the ItemsControlDragDropService.
    /// </remarks>
	internal class InsertionAdorner : Adorner
	{
        private static Pen pen;
        private static PathGeometry triangle;
        private bool isSeparatorHorizontal;
		private AdornerLayer adornerLayer;

		// Create the pen and triangle in a static constructor and freeze them to improve performance.
		static InsertionAdorner()
		{
			pen = new Pen { Brush = Brushes.Gray, Thickness = 2 };
			pen.Freeze();

			LineSegment firstLine = new LineSegment(new Point(0, -5), false);
			firstLine.Freeze();
			LineSegment secondLine = new LineSegment(new Point(0, 5), false);
			secondLine.Freeze();

			PathFigure figure = new PathFigure { StartPoint = new Point(5, 0) };
			figure.Segments.Add(firstLine);
			figure.Segments.Add(secondLine);
			figure.Freeze();

			triangle = new PathGeometry();
			triangle.Figures.Add(figure);
			triangle.Freeze();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertionAdorner"/> class.
        /// </summary>
        /// <param name="isSeparatorHorizontal">if set to <c>true</c> the sparator is horizontal.</param>
        /// <param name="isInFirstHalf">if set to <c>true</c> the adorner should show on the first half of the element.</param>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="adornerLayer">The adorner layer.</param>
		public InsertionAdorner(bool isSeparatorHorizontal, bool isInFirstHalf, UIElement adornedElement, AdornerLayer adornerLayer)
			: base(adornedElement)
		{
			this.isSeparatorHorizontal = isSeparatorHorizontal;
			this.IsInFirstHalf = isInFirstHalf;
			this.adornerLayer = adornerLayer;
			this.IsHitTestVisible = false;

			this.adornerLayer.Add(this);
		}

        public bool IsInFirstHalf { get; set; }

        public void Detach()
        {
            this.adornerLayer.Remove(this);
        }

        /// <summary>
        /// Overridden to draw a line with two triangles at the end.
        /// </summary>
		protected override void OnRender(DrawingContext drawingContext)
		{
			Point startPoint;
			Point endPoint;

			CalculateStartAndEndPoint(out startPoint, out endPoint);
			drawingContext.DrawLine(pen, startPoint, endPoint);

			if (this.isSeparatorHorizontal)
			{
				DrawTriangle(drawingContext, startPoint, 0);
				DrawTriangle(drawingContext, endPoint, 180);
			}
			else
			{
				DrawTriangle(drawingContext, startPoint, 90);
				DrawTriangle(drawingContext, endPoint, -90);
			}
		}

        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="drawingContext">The drawing context.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="angle">The angle.</param>
		private void DrawTriangle(DrawingContext drawingContext, Point origin, double angle)
		{
			drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
			drawingContext.PushTransform(new RotateTransform(angle));

			drawingContext.DrawGeometry(pen.Brush, null, triangle);

			drawingContext.Pop();
			drawingContext.Pop();
		}

        /// <summary>
        /// Calculates the start and end point of the adorner line based on the size of the adorner element.
        /// </summary>
        /// <param name="startPoint">The output start point.</param>
        /// <param name="endPoint">The output end point.</param>
		private void CalculateStartAndEndPoint(out Point startPoint, out Point endPoint)
		{
			startPoint = new Point();
			endPoint = new Point();
			
			double width = this.AdornedElement.RenderSize.Width;
			double height = this.AdornedElement.RenderSize.Height;

			if (this.isSeparatorHorizontal)
			{
				endPoint.X = width;
				if (!this.IsInFirstHalf)
				{
					startPoint.Y = height;
					endPoint.Y = height;
				}
			}
			else
			{
				endPoint.Y = height;
				if (!this.IsInFirstHalf)
				{
					startPoint.X = width;
					endPoint.X = width;
				}
			}		
		}
	}
}
