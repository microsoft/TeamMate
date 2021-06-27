// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.Foundation.Threading
{
    /// <summary>
    /// Implements a simple linear regression algorithm to extrapolate
    /// points in a line.
    /// </summary>
    internal class LinearRegression
    {
        private List<DoublePoint> points = new List<DoublePoint>();

        private int maximumPoints;
        private bool recalculate;
        private double intercept = Double.NaN;
        private double slope = Double.NaN;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearRegression"/> class.
        /// </summary>
        public LinearRegression()
            : this(Int32.MaxValue)
        {
        }

        public LinearRegression(int maximumPoints)
        {
            if (maximumPoints < 2)
                throw new ArgumentOutOfRangeException("The maximum number of points needs to be greater than or equal to 2.");

            this.maximumPoints = maximumPoints;
        }

        /// <summary>
        /// Gets the current number of points in use for the regression.
        /// </summary>
        /// <value>The point count.</value>
        public int PointCount { get { return points.Count; } }

        /// <summary>
        /// Gets the calculation of the slope after applying the linear regression.
        /// </summary>
        /// <value>The slope.</value>
        public double Slope
        {
            get
            {
                EnsureCalculated();
                return slope;
            }
        }

        /// <summary>
        /// Gets the calculation of the intercept with the Y axis after applying the linear regression.
        /// </summary>
        /// <value>The slope.</value>
        public double Intercept
        {
            get
            {
                EnsureCalculated();
                return intercept;
            }
        }

        /// <summary>
        /// Returns the calculated value of the y coordinate for a given value of the x coordinate.
        /// </summary>
        /// <param name="x">The value for the x coordinate.</param>
        /// <returns>The intra or extrapolated value of the y coordinate.</returns>
        public double ValueAt(double x)
        {
            EnsureCalculated();
            return intercept + slope * x;
        }

        /// <summary>
        /// Adds a new point to the linear regression.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public void AddPoint(double x, double y)
        {
            int i;

            for (i = 0; i < points.Count; i++)
            {
                if (points[i].X == x)
                    break;
            }

            if (i < points.Count)
                points[i] = new DoublePoint(x, y);
            else
                points.Add(new DoublePoint(x, y));

            // Only use the last "n" points...
            if (points.Count > maximumPoints)
                points.RemoveAt(0);

            recalculate = true;
        }

        /// <summary>
        /// Clears all the points previously used for the linear regression.
        /// </summary>
        public void ClearPoints()
        {
            points.Clear();
            recalculate = true;
        }

        /// <summary>
        /// Ensures that the factors for the slope and the intercept are recalculated
        /// if necessary.
        /// </summary>
        private void EnsureCalculated()
        {
            if (recalculate)
            {
                int n = points.Count;

                if (n >= 2)
                {
                    double sumX = 0;
                    double sumY = 0;
                    double sumXX = 0;
                    double sumXY = 0;

                    foreach (DoublePoint p in points)
                    {
                        sumX += p.X;
                        sumY += p.Y;
                        sumXX += p.X * p.X;
                        sumXY += p.X * p.Y;
                    }

                    // Standard formulas for a linear regression... Look them up online.
                    slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
                    intercept = (sumY - slope * sumX) / n;
                }
                else
                {
                    intercept = slope = Double.NaN;
                }

                recalculate = false;
            }
        }

        /// <summary>
        /// Captures the coordinates for a point.
        /// </summary>
        private struct DoublePoint
        {
            public double X;
            public double Y;

            /// <summary>
            /// Initializes a new instance of the <see cref="DoublePoint"/> struct.
            /// </summary>
            /// <param name="x">The x coordinate value.</param>
            /// <param name="y">The y coordinate value.</param>
            public DoublePoint(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }
        }
    }
}
