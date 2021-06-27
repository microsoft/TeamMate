using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Win32;
using Microsoft.Tools.TeamMate.Foundation.Windows.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Preview
{
    /// <summary>
    /// Interaction logic for FilePreviewControl.xaml
    /// </summary>
    public partial class FilePreviewControl : UserControl
    {
        private IFilePreviewControl activeControl;
        private IList<IFilePreviewControl> registeredHandlers = new List<IFilePreviewControl>();

        // Order matters...
        private static readonly Type[] DefaultPreviewerControlTypes = {
            typeof(ImagePreviewControl), typeof(WebPreviewControl), typeof(VideoPreviewControl), typeof(NativePreviewControl)
        };

        private FileSystemWatcher fileSystemWatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePreviewControl"/> class.
        /// </summary>
        public FilePreviewControl()
        {
            InitializeComponent();
            HideAll();

            this.Unloaded += HandleUnloaded;
            openExternallyHyperlink.Click += HandleOpenExternallyHyperlinkClick;
            previewUnavailablePluginHyperlink.Click += HandlePreviewUnavailableHyperlinkClick;

            RegisterDefaultPreviewerControls();
        }

        /// <summary>
        /// Gets or sets the preview plugin configured for this control.
        /// </summary>
        public IFilePreviewPlugin PreviewPlugin { get; set; }

        /// <summary>
        /// Registers the specified preview control.
        /// </summary>
        /// <param name="previewControl">The preview control.</param>
        public void Register(IFilePreviewControl previewControl)
        {
            Assert.ParamIsNotNull(previewControl, "previewControl");

            registeredHandlers.Add(previewControl);
        }

        /// <summary>
        /// Unregisters the specified preview control.
        /// </summary>
        /// <param name="previewControl">The preview control.</param>
        public void Unregister(IFilePreviewControl previewControl)
        {
            Assert.ParamIsNotNull(previewControl, "previewControl");

            registeredHandlers.Remove(previewControl);
        }

        /// <summary>
        /// Determines whether this instance can preview the specified file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns><c>true</c> if this control can preview that file type. Otherwise, <c>false</c>.</returns>
        public bool CanPreview(string filePath)
        {
            Assert.ParamIsNotNull(filePath, "filePath");

            return FindPreviewControl(filePath) != null;
        }

        /// <summary>
        /// Registers the default previewer controls.
        /// </summary>
        private void RegisterDefaultPreviewerControls()
        {
            foreach (var previewType in DefaultPreviewerControlTypes)
            {
                try
                {
                    IFilePreviewControl previewControl = Activator.CreateInstance(previewType) as IFilePreviewControl;
                    if (previewControl != null)
                    {
                        Register(previewControl);
                    }
                }
                catch (Exception e)
                {
                    Log.ErrorAndBreak(e);
                }
            }
        }

        /// <summary>
        /// Releases the active preview control, if one is currently active.
        /// </summary>
        private void ReleaseActivePreviewControl()
        {
            if (activeControl != null)
            {
                HideAll();

                previewControlContainer.Children.Clear();
                activeControl.LoadCompleted -= HandlePreviewControlLoaded;
                activeControl.Clear();
                activeControl = null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the configured preview file path exists.
        /// </summary>
        private bool FileExists { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file path has an associated external program to open it.
        /// </summary>
        private bool HasExternalProgram { get; set; }

        /// <summary>
        /// The file path property.
        /// </summary>
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
            "FilePath", typeof(string), typeof(FilePreviewControl), new PropertyMetadata(OnFilePathChanged)
        );

        /// <summary>
        /// Gets or sets the path of the file to be previewed.
        /// </summary>
        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        /// <summary>
        /// Called when file path changed.
        /// </summary>
        private static void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FilePreviewControl)d;
            control.InvalidatePreview();
        }

        /// <summary>
        /// Invalidates the preview.
        /// </summary>
        private void InvalidatePreview()
        {
            DisposeFileSystemWatcher();

            this.FileExists = (!String.IsNullOrEmpty(FilePath) && File.Exists(FilePath));

            bool hasExternalProgram = false;
            FilePreviewControlState state = FilePreviewControlState.Unavailable;
            string pluginText = null;

            // TODO: Hitting Previous/Next quickly in the preview panel ends up showing an empty preview at times.
            // I think it has to do with us not properly cancelling a previous preview request, which messes things up...
            ReleaseActivePreviewControl();

            if (this.FileExists)
            {
                IFilePreviewControl control = FindPreviewControl(FilePath);
                if (control != null)
                {
                    activeControl = control;

                    activeControl.LoadCompleted += HandlePreviewControlLoaded;
                    previewControlContainer.Children.Add(activeControl.Host);
                    activeControl.BeginLoad(FilePath);
                    state = FilePreviewControlState.Initializing;

                    if (IsFileSystemWatcherEnabled)
                    {
                        try
                        {
                            this.fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(FilePath));
                            this.fileSystemWatcher.Filter = Path.GetFileName(FilePath);
                            this.fileSystemWatcher.Changed += HandleFileChanged;
                            this.fileSystemWatcher.EnableRaisingEvents = true;
                        }
                        catch (Exception e)
                        {
                            Log.WarnAndBreak(e, "Failed to register file system watcher for {0}", FilePath);
                        }
                    }
                }

                FileTypeInfo typeInfo = FileTypeRegistry.Instance.GetInfoFromPath(FilePath);
                if (typeInfo != null && !String.IsNullOrEmpty(typeInfo.DefaultOpenExeDescription)
                    && typeInfo.FullPathToDefaultOpenExe != null && File.Exists(typeInfo.FullPathToDefaultOpenExe))
                {
                    hasExternalProgram = true;

                    bool isThumbnail;
                    var systemImage = SystemImageCache.Instance.GetSystemImage(typeInfo.FullPathToDefaultOpenExe);
                    ImageSource image = systemImage.GetPreferredImage(SystemImageSizes.LargeSize, out isThumbnail);
                    programImage.Source = image;
                    programName.Text = typeInfo.DefaultOpenExeDescription;
                }

                if (PreviewPlugin != null)
                {
                    pluginText = PreviewPlugin.GetPluginText(FilePath);
                }
            }

            if (!hasExternalProgram)
            {
                programImage.Source = null;
                programName.Text = null;
            }

            previewUnavailablePluginText.Text = pluginText;

            this.HasExternalProgram = hasExternalProgram;

            InvalidateVisibility(state);
        }

        /// <summary>
        /// The is file system watcher enabled property
        /// </summary>
        public static readonly DependencyProperty IsFileSystemWatcherEnabledProperty = DependencyProperty.Register(
            "IsFileSystemWatcherEnabled", typeof(bool), typeof(FilePreviewControl)
        );

        /// <summary>
        /// Gets or sets a value indicating whether the preview control uses a file system watcher to update
        /// itself if the currently previewed file has changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if [is file system watcher enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool IsFileSystemWatcherEnabled
        {
            get { return (bool)GetValue(IsFileSystemWatcherEnabledProperty); }
            set { SetValue(IsFileSystemWatcherEnabledProperty, value); }
        }

        /// <summary>
        /// Disposes the file system watcher.
        /// </summary>
        private void DisposeFileSystemWatcher()
        {
            if (this.fileSystemWatcher != null)
            {
                this.fileSystemWatcher.Dispose();
                this.fileSystemWatcher = null;
            }
        }

        /// <summary>
        /// Finds the preview control that supports previewing the given file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The compatible preview control, or <c>null</c> if none can preview that file.</returns>
        private IFilePreviewControl FindPreviewControl(string filePath)
        {
            return registeredHandlers.FirstOrDefault(handler => handler.CanPreview(filePath));
        }


        /// <summary>
        /// Invalidates the visibility of elements.
        /// </summary>
        /// <param name="state">The state.</param>
        private void InvalidateVisibility(FilePreviewControlState state)
        {
            previewControlContainer.Visibility = (state == FilePreviewControlState.Viewing || state == FilePreviewControlState.Initializing) ? Visibility.Visible : Visibility.Hidden;
            noPreviewContainer.Visibility = (previewControlContainer.Visibility != Visibility.Visible) ? Visibility.Visible : Visibility.Hidden;

            errorTextBlock.Visibility = (state == FilePreviewControlState.Error) ? Visibility.Visible : Visibility.Collapsed;
            unavailableBlock.Visibility = (state == FilePreviewControlState.Unavailable) ? Visibility.Visible : Visibility.Collapsed;
            externalProgramBlock.Visibility = (HasExternalProgram) ? Visibility.Visible : Visibility.Collapsed;
            previewUnavailablePluginBlock.Visibility = (!HasExternalProgram && !String.IsNullOrEmpty(previewUnavailablePluginText.Text)) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Hides all elements.
        /// </summary>
        private void HideAll()
        {
            previewControlContainer.Visibility = Visibility.Collapsed;
            noPreviewContainer.Visibility = Visibility.Collapsed;
            errorTextBlock.Visibility = Visibility.Collapsed;
            unavailableBlock.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Prompts the user on whether a file should be opened.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns><c>true</c> if the file was safe to open or the user chose to open the file; otherwise, <c>false</c>.</returns>
        private static bool PromptShouldOpen(string fileName)
        {
            // TODO: Duplicated with AttachmentViewModel
            bool launch = true;

            if (FileTypeRegistry.Instance.GetInfoFromPath(fileName).IsPotentiallyUnsafeToOpen)
            {
                string name = System.IO.Path.GetFileName(fileName);
                string message = String.Format("The file {0} could be harmful to your computer and data. Don't open it unless you trust the person who created the file.\n\nDo you want to proceed?", name);
                var result = MessageBox.Show(message, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                launch = (result == MessageBoxResult.Yes);
            }

            return launch;
        }

        /// <summary>
        /// Handles the unloaded event.
        /// </summary>
        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            // TODO: Need a simetrical Loaded event handler, otherwise this can leave the control in a bad state when the session
            // locks for a long time
            DisposeFileSystemWatcher();
            ReleaseActivePreviewControl();
        }

        /// <summary>
        /// Handles the preview control loaded event.
        /// </summary>
        private void HandlePreviewControlLoaded(object sender, LoadEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                if (e.Success)
                {
                    InvalidateVisibility(FilePreviewControlState.Viewing);
                }
                else
                {
                    // TODO: Consume error if it makes sense...
                    InvalidateVisibility(FilePreviewControlState.Error);
                }
            });
        }

        /// <summary>
        /// Handles the open externally hyperlink click.
        /// </summary>
        private void HandleOpenExternallyHyperlinkClick(object sender, RoutedEventArgs e)
        {
            bool shouldOpen = (!String.IsNullOrEmpty(FilePath) && PromptShouldOpen(FilePath));

            if (shouldOpen)
            {
                if (File.Exists(FilePath))
                {
                    Process.Start(FilePath);
                }
                else
                {
                    UserFeedback.ShowError(String.Format("File {0} was not found.", FilePath));
                }
            }
        }

        /// <summary>
        /// Handles the preview unavailable hyperlink click.
        /// </summary>
        private async void HandlePreviewUnavailableHyperlinkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentFilePath = FilePath;
                if (PreviewPlugin != null && !String.IsNullOrEmpty(currentFilePath))
                {
                    bool invalidatePreview = await PreviewPlugin.PrepareForPreviewAsync(currentFilePath);
                    if (invalidatePreview && IsLoaded && currentFilePath == FilePath)
                    {
                        InvalidatePreview();
                    }
                }
            }
            catch (Exception ex)
            {
                UserFeedback.ShowError(ex);
            }
        }


        /// <summary>
        /// Handles the file changed.
        /// </summary>
        private void HandleFileChanged(object sender, FileSystemEventArgs e)
        {
            // The previwed file has changed, invalidate it
            this.Dispatcher.BeginInvoke((Action)InvalidatePreview);
        }

        /// <summary>
        /// The state of the file preview control.
        /// </summary>
        private enum FilePreviewControlState
        {
            Initializing,
            Viewing,
            Unavailable,
            Error
        }
    }
}
