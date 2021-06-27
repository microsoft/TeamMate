using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Preview
{
    /// <summary>
    /// Interaction logic for WebPreviewControl.xaml
    /// </summary>
    public partial class WebPreviewControl : UserControl, IFilePreviewControl
    {
        private static readonly ICollection<string> SupportedFiles = new HashSet<string>(new string[] {
            ".xml", ".txt"
        }, StringComparer.OrdinalIgnoreCase);

        public event EventHandler<LoadEventArgs> LoadCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebPreviewControl"/> class.
        /// </summary>
        public WebPreviewControl()
        {
            InitializeComponent();

            this.webBrowser.Navigated += HandleNavigated;
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
            return SupportedFiles.Contains(System.IO.Path.GetExtension(filename));
        }

        /// <summary>
        /// Begins loading the preview for the given file..
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void BeginLoad(string filename)
        {
            // TODO: XML opening is showing up with the warning yellow bar at the top sometimes
            webBrowser.Navigate(filename);
        }

        /// <summary>
        /// Clears the currently previewed file and releases any associated resources.
        /// </summary>
        public void Clear()
        {
            webBrowser.Source = null;
        }

        /// <summary>
        /// Handles the borwser navigated event.
        /// </summary>
        private void HandleNavigated(object sender, NavigationEventArgs e)
        {
            LoadCompleted?.Invoke(this, new LoadEventArgs());
        }
    }
}
