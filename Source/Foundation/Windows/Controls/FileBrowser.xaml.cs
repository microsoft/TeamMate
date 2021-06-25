using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileBrowser"/> class.
        /// </summary>
        public FileBrowser()
        {
            InitializeComponent();

            button.Click += HandleButtonClick;
        }

        // TODO: FileDialogOpening cancelable event? FileDialogOpened event?

        /// <summary>
        /// The file dialog property
        /// </summary>
        public static readonly DependencyProperty FileDialogProperty = DependencyProperty.Register(
            "FileDialog", typeof(FileDialog), typeof(FileBrowser)
        );

        /// <summary>
        /// Gets or sets the file dialog.
        /// </summary>
        public FileDialog FileDialog
        {
            get { return (FileDialog)GetValue(FileDialogProperty); }
            set { SetValue(FileDialogProperty, value); }
        }

        /// <summary>
        /// The file name property
        /// </summary>
        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(
            "FileName", typeof(string), typeof(FileBrowser), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        /// <summary>
        /// Handles the browse button click.
        /// </summary>
        private void HandleButtonClick(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog = FileDialog;

            if (fileDialog == null)
            {
                fileDialog = new OpenFileDialog();
            }

            fileDialog.FileName = FileName;

            Window owner = Window.GetWindow(this);
            if (fileDialog.ShowDialog(owner) == true)
            {
                FileName = fileDialog.FileName;
            }
        }
    }
}
