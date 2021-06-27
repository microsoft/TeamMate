using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for ProfileImageView.xaml
    /// </summary>
    public partial class ProfileImageView : UserControl
    {
        public ProfileImageView()
        {
            InitializeComponent();
            this.profileImage.SizeChanged += ProfileImage_SizeChanged;
        }

        private void ProfileImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = this.profileImage.ActualWidth;
            var height = this.profileImage.ActualHeight;

            EllipseGeometry geometry = this.profileImage.Clip as EllipseGeometry;
            if (geometry == null)
            {
                geometry = new EllipseGeometry();
                this.profileImage.Clip = geometry;
            }

            geometry.RadiusX = this.ActualWidth / 2;
            geometry.RadiusY = this.ActualHeight / 2;

            geometry.Center = new Point(width / 2, height / 2);
        }
    }
}
