using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for ErrorPage.xaml
    /// </summary>
    public partial class ErrorPage : UserControl
    {
        public ErrorPage()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(ErrorPage)
        );

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(string), typeof(ErrorPage)
        );

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
            "Footer", typeof(string), typeof(ErrorPage)
        );

        public string Footer
        {
            get { return (string)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly DependencyProperty RetryCommandProperty = DependencyProperty.Register(
            "RetryCommand", typeof(ICommand), typeof(ErrorPage)
        );

        public ICommand RetryCommand
        {
            get { return (ICommand)GetValue(RetryCommandProperty); }
            set { SetValue(RetryCommandProperty, value); }
        }
    }
}
