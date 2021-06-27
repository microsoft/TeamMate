using Microsoft.Tools.TeamMate.Controls;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = System.Windows.ShutdownMode.OnLastWindowClose;

            Dispatcher.UnhandledException += HandleDispatcherUnhandledException;


            /*
            var viewModel = SampleData.CustomDialogViewModel;
            CustomDialog dialog = new CustomDialog();
            dialog.UseLayoutRounding = true;
            dialog.DataContext = viewModel;
            if (dialog.ShowDialog() == true)
            {
                var pressedButton = viewModel.PressedButton;
                Console.WriteLine(pressedButton.Text);
            }
            */

            //Environment.Exit(0);



            Window window;
            // window = new CodeFlowReviews();

            /*
            var cfpd = new CodeFlowQueryEditorDialog();
            var vm = new CodeFlowPickerViewModel();
            vm.QueryInfo = new CodeFlowQueryInfo();
            cfpd.DataContext = vm;
            cfpd.ShowDialog();
             */

            window = new TestWindow();
            // window = new ValidationWindow();
            // window = new DesignWindow();

            // window = new Tiles.TileWindow();
            // new VideoWindow().Show();
            // window = new SampleWorkItemWindow();

            // window = new Windows8RibbonWindow();

            // window = new FileViewerWindow();
            // window.DataContext = SandboxSampleData.FileCollectionViewModel;
            // window = new PreviewWindow();
            // window = new ThumbnailWindow();

            // window = new MetroOptionsWindow();
            // window = new GrowingSplashWindow();
            // window = new NewMetroTabs();

            // window = new AttachmentWindow();
            // window = new TeamMembersWindow();

            // window = new AutoCompleteWindow();

            // window = new TileWindow();

            // window = new ListViewWindow();
            // window = new HighlightWindow();

            // window = new StickyNote();

            // window = new BoardViewTestWindow();
            // window = new TfsBoardTestWindow();

            // window = new AnimationWindow();

            // window = new NavigationWindow();
            // window = new DesignWindow();

            // window = new GroupedListBoxWindow();
            // window = new WorkItemListViewWindow();

            // window = new SliderWindow();
            // window = new VideoWindow();

            /*

            var view = CollectionViewSource.GetDefaultView(SandboxSampleData.Messages);
            view.GroupDescriptions.Add(new PropertyGroupDescription("From"));
            window.DataContext = view;
             */

            // window = new DropTestWindow();
            // window = new ProgressWindow();
            // window = new TelerikChartWindow();

            // window = new CalloutWindow();
            // window = new PeopleSummaryWindow();

            // window = new AppPreviewWindow();
            // window = new SendFeedbackWindow();
            // window = new TestWindow();
            // window = new MyTransitionWindow();
            // window = new TransitionWindow();

            // window = new ZuneWindow2();

            // window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            // window.WindowState = WindowState.Maximized;

            window.Show();

            /*
            if (window.ShowDialog() == true)
            {
                var fi = ((SendFeedbackWindow)window).FeedbackItem;

                string filename = Environment.ExpandEnvironmentVariables(@"%TEMP%\Feedback.fdbx.zip");
                fi.Save(filename);

                var fi2 = FeedbackItem.FromFile(filename);
            }
             */

            /*
            ToastViewManager toastViewManager = new ToastViewManager();

            toastViewManager.Queue(new ToastViewModel("1"));
            toastViewManager.Queue(new ToastViewModel("2"));
            toastViewManager.Queue(new ToastViewModel("3"));
            toastViewManager.Queue(new ToastViewModel("4"));
            toastViewManager.Queue(new ToastViewModel("5"));
            toastViewManager.Queue(new ToastViewModel("1"));
            toastViewManager.Queue(new ToastViewModel("2"));
            toastViewManager.Queue(new ToastViewModel("3"));
            toastViewManager.Queue(new ToastViewModel("4"));
            toastViewManager.Queue(new ToastViewModel("5"));
             */
        }

        private void HandleDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ExceptionDialog.Show(e.Exception);
        }

        private void DrawAndExit()
        {
            BitmapSource bitmap = DrawCountBitmap("5");
            string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "image.png");
            BitmapUtilities.Save(bitmap, filename);
            Process.Start(filename);
            Environment.Exit(0);
        }

        private static BitmapSource DrawCountBitmap(string count)
        {
            OverlayTextIcon icon = new OverlayTextIcon();
            icon.SnapsToDevicePixels = true;
            icon.Text = count;
            icon.HasUpdates = true;
            return icon.CaptureBitmap();
        }
    }
}