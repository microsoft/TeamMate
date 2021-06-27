// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Animation;
using System.Windows;
using System.Windows.Media.Animation;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// A custom control to display a linear Metro-style indeterminate progress bar.
    /// </summary>
    public partial class ProgressIndicator
    {
        private AnimationHelper animationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressIndicator"/> class.
        /// </summary>
        public ProgressIndicator()
        {
            InitializeComponent();

            Storyboard animateStoryboard = (Storyboard)this.Resources["Storyboard"];
            this.animationHelper = new AnimationHelper(this, animateStoryboard);
            this.animationHelper.Enable();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // TODO: Hack. This was an attempt to deal with the fact that the progress indicator animation resource
            // attempts to bind to the parent user control actual width. That can change. And it was updating itself
            // after change. Not sure if this wil work though.
            this.animationHelper.Invalidate();
        }
    }
}
