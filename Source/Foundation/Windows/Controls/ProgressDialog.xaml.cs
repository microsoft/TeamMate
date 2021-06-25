using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Threading;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        /// <summary>
        /// Shows a progress dialog for a given task context.
        /// </summary>
        /// <param name="taskContext">The task context.</param>
        /// <param name="owner">An optional dialog owner.</param>
        public static void Show(TaskContext taskContext, Window owner = null)
        {
            ProgressDialog dialog = new ProgressDialog(taskContext);
            if (owner == null)
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                dialog.Owner = owner;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            dialog.ShowDialogAndContinue();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressDialog"/> class.
        /// </summary>
        public ProgressDialog()
        {
            InitializeComponent();

            this.DataContextChanged += HandleDataContextChanged;
            this.Closing += HandleClosing;
            this.cancelButton.Click += HandleCancelButtonClick;
            this.KeyDown += HandleKeyDown;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressDialog"/> class.
        /// </summary>
        /// <param name="taskContext">The task context.</param>
        public ProgressDialog(TaskContext taskContext)
            : this()
        {
            Assert.ParamIsNotNull(taskContext, "taskContext");
            this.DataContext = taskContext;
        }

        /// <summary>
        /// Shows the dialog without blocking the thread execution.
        /// </summary>
        private void ShowDialogAndContinue()
        {
            // Do this in a BeginInvoke to support the using() pattern...
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.ShowDialog();
            });
        }

        /// <summary>
        /// Gets the task context.
        /// </summary>
        private TaskContext TaskContext
        {
            get { return this.DataContext as TaskContext; }
        }

        /// <summary>
        /// Cancels the running task.
        /// </summary>
        private void Cancel()
        {
            if (TaskContext != null)
            {
                TaskContext.Cancel();
            }
        }

        /// <summary>
        /// Handles the cancel button click.
        /// </summary>
        private void HandleCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Cancel();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the dialog closing event.
        /// </summary>
        private void HandleClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult == null)
            {
                Cancel();
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles the data context change event.
        /// </summary>
        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TaskContext oldValue = e.OldValue as TaskContext;
            TaskContext newValue = e.NewValue as TaskContext;

            if (oldValue != null)
            {
                oldValue.Completed -= HandleTaskCompleted;
            }

            if (newValue != null)
            {
                newValue.Completed += HandleTaskCompleted;
            }
        }

        /// <summary>
        /// Handles the task completed event.
        /// </summary>
        private void HandleTaskCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke((Action)delegate()
            {
                if (IsVisible)
                {
                    bool result = false;
                    if (TaskContext != null)
                    {
                        result = !TaskContext.IsCancellationRequested;
                        DataContext = null;
                    }

                    this.DialogResult = result;
                }
            });
        }

        /// <summary>
        /// Handles the key down event.
        /// </summary>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Cancel();
                e.Handled = true;
            }
        }
    }
}
