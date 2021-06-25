using System;
using System.Windows;
using System.Windows.Controls;

using WinformsNativePreviewControl = Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Forms.NativePreviewControl;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls.Preview
{
    /// <summary>
    /// Interaction logic for NativePreviewControl.xaml
    /// </summary>
    public partial class NativePreviewControl : UserControl, IFilePreviewControl
    {
        private WinformsNativePreviewControl nativeControl;

        public event EventHandler<LoadEventArgs> LoadCompleted;

        /// <summary>
        /// The is starting property.
        /// </summary>
        public static readonly DependencyProperty IsStartingProperty = DependencyProperty.Register(
            "IsStarting", typeof(bool), typeof(NativePreviewControl)
        );

        /// <summary>
        /// Gets or sets a value indicating whether preview is starting for this control.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is starting]; otherwise, <c>false</c>.
        /// </value>
        public bool IsStarting
        {
            get { return (bool)GetValue(IsStartingProperty); }
            set { SetValue(IsStartingProperty, value); }
        }

        /// <summary>
        /// The preview handler description property
        /// </summary>
        public static readonly DependencyProperty PreviewHandlerDescriptionProperty = DependencyProperty.Register(
            "PreviewHandlerDescription", typeof(string), typeof(NativePreviewControl)
        );

        /// <summary>
        /// Gets or sets the preview handler description.
        /// </summary>
        public string PreviewHandlerDescription
        {
            get { return (string)GetValue(PreviewHandlerDescriptionProperty); }
            set { SetValue(PreviewHandlerDescriptionProperty, value); }
        }

        public NativePreviewControl()
        {
            InitializeComponent();

            this.nativeControl = new WinformsNativePreviewControl();
            this.nativeControl.LoadFinished += HandleLoadCompleted;

            this.Unloaded += HandleUnloaded;
        }

        /// <summary>
        /// Gets the UI element used to display the preview of the file.
        /// </summary>
        public FrameworkElement Host
        {
            get { return this; }
        }

        /// <summary>
        /// Determines whether this instance can preview the specified file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>
        ///   <c>true</c> if this instance supports previewing that file. Otherwise, <c>false</c>.
        /// </returns>
        public bool CanPreview(string filename)
        {
            return WinformsNativePreviewControl.CanPreview(filename);
        }

        /// <summary>
        /// Begins loading the preview for the given file..
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void BeginLoad(string filename)
        {
            this.PreviewHandlerDescription = WinformsNativePreviewControl.GetPreviewHandlerDescription(filename);
            this.IsStarting = true;
            this.nativeControl.FilePath = filename;
        }

        /// <summary>
        /// Clears the currently previewed file and releases any associated resources.
        /// </summary>
        public void Clear()
        {
            this.nativeControl.FilePath = null;
            this.PreviewHandlerDescription = null;
            this.IsStarting = false;
        }

        /// <summary>
        /// Handles the load completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Forms.LoadEventArgs"/> instance containing the event data.</param>
        private void HandleLoadCompleted(object sender, Forms.LoadEventArgs e)
        {
            this.host.Child = nativeControl;
            this.IsStarting = false;

            if (LoadCompleted != null)
            {
                // TODO: Mark load completed here too... Pass the right args
                var newEvent = (e.Error != null) ? new LoadEventArgs(e.Error) : new LoadEventArgs();
                LoadCompleted(this, newEvent);
            }
        }

        /// <summary>
        /// Handles the unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            this.host.Child = null;

            if (this.nativeControl != null)
            {
                // TODO: IMPORTANT. Find another clean scenario for getting rid of the native control. In the WPF Preview Container,
                // we add/remove the control from its container, which calls unload.
                // this.nativeControl.LoadFinished -= nativeControl_LoadCompleted);
                // this.nativeControl.Dispose();
                // this.nativeControl = null;
            }
        }
    }
}
