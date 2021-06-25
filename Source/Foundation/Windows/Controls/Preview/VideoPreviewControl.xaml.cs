using Microsoft.Internal.Tools.TeamMate.Foundation.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls.Preview
{
    /// <summary>
    /// Interaction logic for VideoPreviewControl.xaml
    /// </summary>
    public partial class VideoPreviewControl : UserControl, IFilePreviewControl
    {
        private static readonly string[] PossiblySupportedFiles = {
            ".avi", ".mov", ".mp4", ".mpeg", ".wmv", ".xesc"
        };

        private static ICollection<string> SupportedFiles = null;

        public event EventHandler<LoadEventArgs> LoadCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPreviewControl"/> class.
        /// </summary>
        public VideoPreviewControl()
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
            ICollection<string> localSupportedFiles = SupportedFiles;
            
            if (localSupportedFiles == null)
            {
                localSupportedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var extension in PossiblySupportedFiles)
                {
                    var fileTypeInfo = FileTypeRegistry.Instance.GetInfo(extension);
                    if (fileTypeInfo.IsPerceivedAsVideo)
                    {
                        localSupportedFiles.Add(extension);
                    }
                }

                SupportedFiles = localSupportedFiles;
            }

            return localSupportedFiles.Contains(Path.GetExtension(filename));
        }

        /// <summary>
        /// Begins loading the preview for the given file..
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void BeginLoad(string filename)
        {
            videoPlayer.Source = new Uri(filename);
            LoadCompleted?.Invoke(this, new LoadEventArgs());
        }

        /// <summary>
        /// Clears the currently previewed file and releases any associated resources.
        /// </summary>
        public void Clear()
        {
            videoPlayer.Source = null;
        }

        /// <summary>
        /// Clears the supported file cache.
        /// </summary>
        public static void ClearSupportedFileCache()
        {
            SupportedFiles = null;
        }
    }
}
