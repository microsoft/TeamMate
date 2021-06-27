// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Interaction logic for ExceptionDialog.xaml
    /// </summary>
    public partial class ExceptionDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionDialog"/> class.
        /// </summary>
        public ExceptionDialog()
        {
            InitializeComponent();
            Icon = null;

            this.okButton.Click += HandleOkClicked;
            this.ContentRendered += HandleContentRendered;
            this.Loaded += HandleLoaded;
        }

        /// <summary>
        /// Shows an exception dialog for the given exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void Show(Exception exception)
        {
            Show(null, null, exception);
        }

        /// <summary>
        /// Shows an exception dialog.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void Show(string message, Exception exception)
        {
            Show(null, message, exception);
        }

        /// <summary>
        /// Shows an exception dialog.
        /// </summary>
        /// <param name="owner">The dialog owner.</param>
        /// <param name="exception">The exception.</param>
        public static void Show(Window owner, Exception exception)
        {
            Show(owner, null, exception);
        }

        /// <summary>
        /// Shows an exception dialog.
        /// </summary>
        /// <param name="owner">The dialog owner.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void Show(Window owner, string message, Exception exception)
        {
            // TODO: Make sure this is callable from ANY thread (not only dispatcher threads).

            ExceptionDialog dialog = new ExceptionDialog();

            if (message == null)
            {
                message = "An unexpected error occurred. If you continue seeing unexpected behaviour, please restart the application and try again.";
            }

            dialog.Message = message;
            dialog.Exception = exception;
            dialog.Topmost = true;

            if (owner != null)
            {
                dialog.Owner = owner;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            dialog.ShowDialog();
        }

        /// <summary>
        /// The message property
        /// </summary>
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(string), typeof(ExceptionDialog)
        );

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        /// The exception property
        /// </summary>
        public static readonly DependencyProperty ExceptionProperty = DependencyProperty.Register(
            "Exception", typeof(Exception), typeof(ExceptionDialog)
        );

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        public Exception Exception
        {
            get { return (Exception)GetValue(ExceptionProperty); }
            set { SetValue(ExceptionProperty, value); }
        }

        /// <summary>
        /// Handles the loaded.
        /// </summary>
        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            // Mimic the error message sound...
            UserFeedback.PlayErrorSound();
        }

        /// <summary>
        /// Handles the content rendered.
        /// </summary>
        private void HandleContentRendered(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Height;
        }

        /// <summary>
        /// Handles the ok clicked.
        /// </summary>
        private void HandleOkClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
