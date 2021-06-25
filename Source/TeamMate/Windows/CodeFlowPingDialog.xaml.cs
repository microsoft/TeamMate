using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for CodeFlowPingDialog.xaml
    /// </summary>
    public partial class CodeFlowPingDialog : Window
    {
        public CodeFlowPingDialog()
        {
            InitializeComponent();
            this.okButton.Click += HandleOkButtonClick;
        }

        private void HandleOkButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string Message
        {
            get { return inputTextBox.Text.Trim(); }
        }
    }
}
