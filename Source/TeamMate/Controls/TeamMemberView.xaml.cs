using Microsoft.Tools.TeamMate.Foundation.Shell;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for TeamMemberView.xaml
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class TeamMemberView : UserControl
    {
        public TeamMemberView()
        {
            InitializeComponent();
            this.container.MouseLeftButtonDown += Container_MouseLeftButtonDown;
        }

        private void Container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(this.LaunchUrl != null)
            {
                ExternalWebBrowser.Launch(this.LaunchUrl);
            }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource", typeof(ImageSource), typeof(TeamMemberView)
        );

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
            "DisplayName", typeof(string), typeof(TeamMemberView)
        );

        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public static readonly DependencyProperty LaunchUrlProperty = DependencyProperty.Register(
            "LaunchUrl", typeof(Uri), typeof(TeamMemberView)
        );

        public Uri LaunchUrl
        {
            get { return (Uri)GetValue(LaunchUrlProperty); }
            set { SetValue(LaunchUrlProperty, value); }
        }
    }
}
