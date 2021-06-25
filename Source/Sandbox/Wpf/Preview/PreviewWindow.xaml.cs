using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf.Preview
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
        public PreviewWindow()
        {
            InitializeComponent();
            this.Loaded += PreviewWindow_Loaded;
        }

        void PreviewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            previewControl.FilePath = @"zipfile.zip";
        }
    }
}
