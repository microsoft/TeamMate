using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Preview
{
    /// <summary>
    /// Interaction logic for ImagePreviewControl.xaml
    /// </summary>
    public partial class ImagePreviewControl : UserControl, IFilePreviewControl
    {
        public event EventHandler<LoadEventArgs> LoadCompleted;

        public ImagePreviewControl()
        {
            InitializeComponent();
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
            return BitmapUtilities.IsSupportedImageFile(filename);
        }

        /// <summary>
        /// Begins loading the preview for the given file..
        /// </summary>
        /// <param name="filename">The filename.</param>
        public async void BeginLoad(string filename)
        {
            Exception error = null;

            try
            {
                var imageSource = await Task.Run(() => BitmapUtilities.LoadImage(filename));
                this.imageViewer.Source = imageSource;
            }
            catch (Exception e)
            {
                error = e;
            }

            if (LoadCompleted != null)
            {
                var args = (error != null) ? new LoadEventArgs(error) : new LoadEventArgs();
                LoadCompleted(this, args);
            }
        }

        /// <summary>
        /// Clears the currently previewed file and releases any associated resources.
        /// </summary>
        public void Clear()
        {
            this.imageViewer.Source = null;
        }
    }
}
