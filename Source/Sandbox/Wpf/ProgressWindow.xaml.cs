using Microsoft.Tools.TeamMate.Foundation.Threading;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
            button.Click += button_Click;
        }

        async void button_Click(object sender, RoutedEventArgs e)
        {
            using (TaskContext taskContext = new TaskContext())
            {
                ProgressDialog.Show(taskContext, this);
                await Task.Run(() => DoWork(taskContext));
            }
        }

        public void DoWork(TaskContext context)
        {
            context.Title = "Counting numbers...";
            context.ReportsProgress = true;

            for (int i = 1; i <= 100 && !context.IsCancellationRequested; i++)
            {
                context.Status = String.Format("Counting {0}", i);
                context.Report(i, 100);
                Thread.Sleep(50);

                if (i > 50)
                {
                    throw new Exception("Foo");
                }
            }
        }
    }
}
