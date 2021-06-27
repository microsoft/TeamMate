using Microsoft.Tools.TeamMate.Foundation.Diagnostics.Reports;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Capture;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Microsoft.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for SendFeedbackDialog.xaml
    /// </summary>
    public partial class SendFeedbackDialog : Window
    {
        private BitmapSource screenshotBitmap;

        public static readonly DependencyProperty FeedbackTypeProperty = DependencyProperty.Register(
            "FeedbackType", typeof(FeedbackType), typeof(SendFeedbackDialog), new PropertyMetadata(FeedbackType.Smile)
        );

        public FeedbackType FeedbackType
        {
            get { return (FeedbackType)GetValue(FeedbackTypeProperty); }
            set { SetValue(FeedbackTypeProperty, value); }
        }

        public SendFeedbackDialog()
        {
            InitializeComponent();
            sendButton.Click += HandleSendButtonClicked;
            this.Loaded += HandleLoaded;

            screenshotBitmap = ScreenCapture.CaptureScreen();
            screenshotImage.Source = screenshotBitmap;
        }

        public FeedbackReport FeedbackReport { get; private set; }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            this.FeedbackReport = null;
            includeEmailCheckbox.Checked += HandleIncludeEmailCheckboxChecked;
        }

        private void HandleSendButtonClicked(object sender, RoutedEventArgs e)
        {
            FeedbackReport report = FeedbackReport.Create();
            report.Type = this.FeedbackType;
            report.Text = this.feedbackTextBox.Text;

            if (includeScreenshotCheckbox.IsChecked == true)
            {
                MemoryStream stream = new MemoryStream();
                BitmapUtilities.SaveAsPng(screenshotBitmap, stream);

                report.Attachments.Add(new Attachment("screenshot.png", stream.GetBuffer()));
            }

            if (includeEmailCheckbox.IsChecked == true && !String.IsNullOrEmpty(emailTextBox.Text.Trim()))
            {
                report.EmailAddress = emailTextBox.Text.Trim();
            }

            this.FeedbackReport = report;
            this.DialogResult = true;
        }

        private void HandleIncludeEmailCheckboxChecked(object sender, RoutedEventArgs e)
        {
            // Invoke later to give time to the IsEnabled binding to change first on checked...
            Dispatcher.BeginInvoke((Action)delegate()
            {
                emailTextBox.Focus();
                emailTextBox.SelectAll();
            });
        }

        internal void SetEmail(string email)
        {
            includeEmailCheckbox.IsChecked = true;
            emailTextBox.Text = email;
        }
    }
}
