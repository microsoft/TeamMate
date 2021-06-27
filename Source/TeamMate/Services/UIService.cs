using Microsoft.Internal.Tools.TeamMate.Controls;
using Microsoft.Internal.Tools.TeamMate.Exceptions;
using Microsoft.Internal.Tools.TeamMate.Foundation.Collections;
using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Interop;
using Microsoft.Internal.Tools.TeamMate.Model;
using Microsoft.Internal.Tools.TeamMate.Resources;
using Microsoft.Internal.Tools.TeamMate.Resources.Native;
using Microsoft.Internal.Tools.TeamMate.Utilities;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using Microsoft.Internal.Tools.TeamMate.Windows;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using ToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;

namespace Microsoft.Internal.Tools.TeamMate.Services
{
	public class UIService : IDisposable
    {
        private const int MaxJumpListItems = 5;
        private static readonly int TrayIconBallonTimeout = (int)TimeSpan.FromSeconds(15).TotalMilliseconds;

        private bool handlingGlobalException;

        private JumpList jumpList;
        private JumpTask createDefaultTask;
        private JumpTask createTask;
        private JumpTask queryTask;

        private System.Drawing.Icon defaultTrayIcon = TeamMateResources.TrayIcon;
        private NotifyIcon trayIcon;
        private ToolStripMenuItem createDefaultMenuItem;
        private ToolStripMenuItem createMenuItem;
        private ToolStripMenuItem queryMenuItem;
        private ToolStripMenuItem openMenuItem;
        private ToolStripMenuItem exitMenuItem;

        private OverviewWindow overviewWindow;

        private System.Windows.Application application;

        private string applicationPath;
        private Session session;
        private ItemCountSummary itemCountSummary;

        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public GlobalCommandService GlobalCommandService { get; set; }

        [Import]
        public WindowService WindowService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        public void Initialize()
        {
            this.application = System.Windows.Application.Current;

            System.Windows.Forms.Application.ThreadException += HandleWinFormsUnhandledException;
            Dispatcher.UnhandledException += HandleDispatcherUnhandledException;

            this.applicationPath = TeamMateApplicationInfo.ExePath;
            this.session = this.SessionService.Session;

            this.session.ConnectionInfo.PropertyChanged += HandleConnectionInfoPropertyChanged;

            this.itemCountSummary = this.ItemCountSummary;
            this.ItemCountSummary.GlobalCounter.Changed += HandleGlobalCounterChanged;

            this.SettingsService.Settings.PropertyChanged += HandleSettingsPropertyChanged;

            // For interop when hosting WinForms controls (e.g. the work item form subcontrols)
            System.Windows.Forms.Application.EnableVisualStyles();

            InitializeJumpList();
            InitializeTrayIcon();

            InvalidateJumpListAndTrayIconDescriptions();
            jumpList.Apply();

            this.overviewWindow = new OverviewWindow();
            this.overviewWindow.IsVisibleChanged += HandleOverviewWindowVisibilityChanged;

            var overviewWindowViewModel = ViewModelFactory.Create<OverviewWindowViewModel>();
            overviewWindowViewModel.ItemCountSummary = itemCountSummary;
            this.overviewWindow.DataContext = overviewWindowViewModel;

            InvalidateTaskBarItemOverlay();
            InvalidateTrayIcon();
            InvalidateOverviewWindow();
        }

        private void HandleGlobalCounterChanged(object sender, EventArgs e)
        {
            application.Dispatcher.InvokeHere(delegate ()
            {
                var settings = this.SettingsService.Settings;

                if (settings.ShowItemCountInTaskBar)
                {
                    InvalidateTaskBarItemOverlay();
                }

                if (settings.ShowItemCountInNotificationArea)
                {
                    InvalidateTrayIcon();
                }
            });
        }


        private void HandleOverviewWindowVisibilityChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!this.overviewWindow.IsVisible)
            {
                WindowStateTracker.Instance.StoreCurrentState(overviewWindow);
            }
        }


        private bool firstOverviewWindowShow = true;

        private void InvalidateOverviewWindow()
        {
            bool showItemCount = this.SettingsService.Settings.ShowItemCountInOverviewWindow;

            if (showItemCount)
            {
                overviewWindow.Show();

                if (firstOverviewWindowShow)
                {
                    firstOverviewWindowShow = false;

                    WindowStateInfo info = WindowStateTracker.Instance.GetState(overviewWindow);
                    if (info != null)
                    {
                        info.ApplyLocationOnly(overviewWindow);
                    }
                    else
                    {
                        SetDefaultOverviewWindowPosition(overviewWindow);
                    }
                }
            }
            else
            {
                overviewWindow.Hide();
            }
        }

        private static void SetDefaultOverviewWindowPosition(OverviewWindow overviewWindow)
        {
            var primaryBounds = WindowUtilities.PrimaryScreenBounds;
            int widthOfDefaultWindowTopRightButtons = 450; // some buffer
            var left = primaryBounds.Left + primaryBounds.Width - overviewWindow.Width - widthOfDefaultWindowTopRightButtons;
            var top = primaryBounds.Top;

            overviewWindow.Left = left;
            overviewWindow.Top = top;
            WindowUtilities.EnsureWithinVirtualScreen(overviewWindow);
        }

        private void HandleConnectionInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ConnectionState")
            {
                this.Dispatcher.InvokeAsync(InvalidateConnectionState);
            }
        }

        private void InvalidateConnectionState()
        {
            InvalidateTaskBarItemOverlay();
        }

        private void InvalidateTaskBarItemOverlay()
        {
            ConnectionInfo connectionInfo = session.ConnectionInfo;

            if (connectionInfo.IsConnectionFailed)
            {
                TaskbarItemInfo.Overlay = TeamMateResources.ErrorOverlayIcon;
            }
            else if (this.SettingsService.Settings.ShowItemCountInTaskBar)
            {
                TaskbarItemInfo.Overlay = CreateItemCountBadge();
            }
            else if (TaskbarItemInfo.Overlay != null)
            {
                TaskbarItemInfo.Overlay = null;
            }
        }

        public void Dispose()
        {
            if (trayIcon != null)
            {
                trayIcon.Dispose();
                trayIcon = null;
            }

            if (overviewWindow != null)
            {
                overviewWindow.Close();
                overviewWindow = null;
            }
        }

        private BitmapSource CreateItemCountBadge()
        {
            var counter = this.itemCountSummary.GlobalCounter;
            string text = (counter.HasCount) ? counter.Count.ToString() : "?";
            bool hasUpdates = (counter.HasCount && !counter.IsRead);

            OverlayTextIcon icon = new OverlayTextIcon();
            icon.Text = text;
            icon.HasUpdates = hasUpdates;
            return icon.CaptureBitmap();
        }

        private void SetTextOverlay(string text)
        {
            OverlayTextIcon icon = new OverlayTextIcon();
            icon.Text = text;
            TaskbarItemInfo.Overlay = icon.CaptureBitmap();
        }

        private ItemCountSummary ItemCountSummary
        {
            get
            {
                return this.WindowService.MainWindowViewModel.HomePage.TileCollection.ItemCountSummary;
            }
        }

        private TaskbarItemInfo TaskbarItemInfo
        {
            get
            {
                return this.WindowService.MainWindow.TaskbarItemInfo;
            }
        }

        private void InitializeJumpList()
        {
            jumpList = new JumpList();
            jumpList.ShowFrequentCategory = false;
            jumpList.ShowRecentCategory = false;

            createDefaultTask = AddJumpTask("New Default Work Item", null, CommandLineService.GetArgsForCreateDefault(), NativeResources.AddIconIndex);
            createTask = AddJumpTask("New Work Item", null, CommandLineService.GeatArgsForCreate(), NativeResources.NewTaskIconIndex);
            queryTask = AddJumpTask("Search", null, CommandLineService.GetArgsForSearch(), NativeResources.SearchIconIndex);

            JumpList.SetJumpList(this.application, jumpList);
        }

        private void RemoveJumpItems(string customCategory)
        {
            var itemsToRemove = jumpList.JumpItems.Where(ji => ji.CustomCategory == customCategory).ToArray();
            jumpList.JumpItems.RemoveRange(itemsToRemove);
        }

        private JumpTask AddJumpTask(string title, string description, string args, int iconIndex)
        {
            JumpTask task = new JumpTask();
            task.Title = title;
            task.Description = description;
            task.ApplicationPath = applicationPath;
            task.Arguments = args;
            task.IconResourcePath = applicationPath;
            task.IconResourceIndex = iconIndex;

            jumpList.JumpItems.Add(task);
            return task;
        }

        private bool HandleUnhandledException(Exception e)
        {
            Log.Error(e);

            if (!handlingGlobalException && !(e is ForceCrashException))
            {
                handlingGlobalException = true;

                try
                {
                    Telemetry.Exception(e);
                    UserFeedback.UnhandledException(e);
                    return true;
                }
                finally
                {
                    handlingGlobalException = false;
                }
            }

            return false;
        }

        private void InitializeTrayIcon()
        {
            this.trayIcon = new NotifyIcon();
            this.trayIcon.Text = TeamMateApplicationInfo.ApplicationName;
            this.trayIcon.Icon = defaultTrayIcon;
            this.trayIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();

            this.createDefaultMenuItem = AddMenuItemToTrayIcon(this.trayIcon, TeamMateCommands.QuickCreate, this.GlobalCommandService.QuickCreateDefault);
            this.createMenuItem = AddMenuItemToTrayIcon(this.trayIcon, TeamMateCommands.QuickCreateWithOptions, this.GlobalCommandService.QuickCreate);
            this.queryMenuItem = AddMenuItemToTrayIcon(this.trayIcon, TeamMateCommands.QuickSearch, this.GlobalCommandService.QuickSearch);
            this.trayIcon.ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            this.openMenuItem = AddMenuItemToTrayIcon(this.trayIcon, null, this.ShowMainWindow);

            this.openMenuItem.Font = new System.Drawing.Font(this.openMenuItem.Font, System.Drawing.FontStyle.Bold);
            this.openMenuItem.Text = "&Open";

            this.exitMenuItem = AddMenuItemToTrayIcon(this.trayIcon, TeamMateCommands.Exit, () => this.WindowService.RequestShutdown());

            // Default action on icon click
            this.trayIcon.MouseClick += delegate (object sender, System.Windows.Forms.MouseEventArgs e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    ShowMainWindow();
                }
            };

            InvalidateTrayContextMenuCommands();

            this.trayIcon.Visible = true;
        }

        private void InvalidateTrayIcon()
        {
            var settings = this.SettingsService.Settings;

            if (settings.ShowItemCountInNotificationArea)
            {
                this.trayIcon.Icon = InteropUtilities.IconFromBitmapSource(CreateItemCountBadge());
            }
            else
            {
                this.trayIcon.Icon = defaultTrayIcon;
            }
        }

        private void ShowMainWindow()
        {
            this.WindowService.ShowMainWindow();
        }

        private ToolStripMenuItem AddMenuItemToTrayIcon(NotifyIcon icon, ICommand command, Action action)
        {
            ToolStripMenuItem menuItem = new ToolStripMenuItem();
            menuItem.Tag = command;
            RoutedUICommand uiCommand = command as RoutedUICommand;
            if (uiCommand != null)
            {
                menuItem.Text = uiCommand.Text.Replace('_', '&');
                // TODO: Support for image binding, tooltips, etc...
            }

            // TODO: Show global keyboard shortcuts on these tray icon menu items

            menuItem.Click += delegate (object sender, EventArgs e)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    UserFeedback.ShowError(ex);
                }
            };

            icon.ContextMenuStrip.Items.Add(menuItem);
            return menuItem;
        }

        private void InvalidateTrayContextMenuCommands()
        {
            if (trayIcon != null && trayIcon.ContextMenuStrip != null)
            {
                foreach (var item in trayIcon.ContextMenuStrip.Items.OfType<System.Windows.Forms.ToolStripMenuItem>())
                {
                    RoutedUICommand uiCommand = item.Tag as RoutedUICommand;
                    if (uiCommand != null)
                    {
                        item.ShortcutKeyDisplayString = GetDisplayString(uiCommand.GetMainKeyGesture());
                    }
                }
            }
        }

        private static string GetDisplayString(KeyGesture gesture)
        {
            return (gesture != null) ? KeyGestureUtilities.GetDisplayString(gesture) : null;
        }

        public void RegisterHotKeys()
        {
            ApplicationHotKeys.UnregisterAllHotKeys();

            var settings = this.SettingsService.Settings;

            TryRegisterGlobalGesture(settings.QuickCreateGesture, QuickCreateDefault, TeamMateCommands.QuickCreate);
            TryRegisterGlobalGesture(settings.QuickCreateWithOptionsGesture, QuickCreate, TeamMateCommands.QuickCreateWithOptions);
            TryRegisterGlobalGesture(settings.QuickSearchGesture, QuickSearch, TeamMateCommands.QuickSearch);
            TryRegisterGlobalGesture(settings.ToggleMainWindowGesture, ShowHomePage, null);
        }

        private void InvalidateJumpListAndTrayIconDescriptions()
        {
            var settings = this.SettingsService.Settings;

            var defaultWorkItemInfo = settings.DefaultWorkItemInfo;
            string createName = (defaultWorkItemInfo != null) ? defaultWorkItemInfo.DisplayName : "Default Work Item";
            string createDefaultTitle = String.Format("New {0}", createName);

            string projectName = (defaultWorkItemInfo != null && defaultWorkItemInfo.WorkItemType != null) ?
                TryResolveProjectName(defaultWorkItemInfo.WorkItemType.Project) : null;

            string createDefaultText = (projectName != null) ?
                String.Format("Create a {0} in {1}", createName, projectName) :
                String.Format("Create a {0}", createName);

            string createDefaultDescription = KeyGestureUtilities.FormatShortcut(createDefaultText, settings.QuickCreateGesture);
            string createDescription = KeyGestureUtilities.FormatShortcut("Create a Work Item", settings.QuickCreateWithOptionsGesture);
            string queryDescription = KeyGestureUtilities.FormatShortcut("Search", settings.QuickSearchGesture);

            createDefaultTask.Title = createDefaultTitle;
            createDefaultTask.Description = createDefaultDescription;
            createTask.Description = createDescription;
            queryTask.Description = queryDescription;

            createDefaultMenuItem.Text = createDefaultTitle;
            createDefaultMenuItem.ToolTipText = createDefaultDescription;
            createMenuItem.ToolTipText = createDescription;
            queryMenuItem.ToolTipText = queryDescription;
        }

        private string TryResolveProjectName(ProjectReference project)
        {
            var projectInfo = this.SettingsService.Settings.Projects.FirstOrDefault(p => p.Reference.Equals(project));
            return (projectInfo != null) ? projectInfo.ProjectName : null;
        }

        private bool TryRegisterGlobalGesture(KeyGesture gesture, Action action, ICommand commandToUpdate)
        {
            bool registered = false;

            if (ApplicationHotKeys.IsHotKeyAvailable(gesture))
            {
                ApplicationHotKeys.RegisterHotKey(gesture, action);
                registered = true;
            }

            RoutedUICommand uiCommandToUpdate = commandToUpdate as RoutedUICommand;
            if (uiCommandToUpdate != null)
            {
                uiCommandToUpdate.InputGestures.Clear();
                uiCommandToUpdate.InputGestures.Add(gesture);
            }

            return registered;
        }

        public Dispatcher Dispatcher
        {
            get { return this.application.Dispatcher; }
        }

        private void HandleSettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO: Also listen for shortcut key changes? How do we listen for settings changes in bulk and queue just one?
            switch (e.PropertyName)
            {
                case "DefaultWorkItemInfo":
                    InvalidateJumpListAndTrayIconDescriptions();
                    jumpList.Apply();
                    break;

                case "ShowItemCountInNotificationArea":
                    InvalidateTrayIcon();
                    break;

                case "ShowItemCountInOverviewWindow":
                    InvalidateOverviewWindow();
                    break;

                case "ShowItemCountInTaskBar":
                    InvalidateTaskBarItemOverlay();
                    break;
            }
        }

        private void HandleWinFormsUnhandledException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleUnhandledException(e.Exception);
        }

        private void HandleDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = HandleUnhandledException(e.Exception);
        }

        private void ShowHomePage()
        {
            LogTelemetryHotKeyEvent("ShowHomePage");
            this.GlobalCommandService.ShowHomePage();
        }

        private static void LogTelemetryHotKeyEvent(string hotKey)
        {
            Telemetry.Event(TelemetryEvents.HotKeyUsed, new TelemetryEventProperties() {
                { TelemetryEvents.Properties.HotKey, hotKey }
            });
        }

        private void QuickSearch()
        {
            LogTelemetryHotKeyEvent("QuickSearch");
            this.GlobalCommandService.AutoQuickSearch();
        }

        private void QuickCreate()
        {
            LogTelemetryHotKeyEvent("QuickCreate");
            this.GlobalCommandService.QuickCreate();
        }

        private void QuickCreateDefault()
        {
            LogTelemetryHotKeyEvent("QuickCreateDefault");
            this.GlobalCommandService.QuickCreateDefault();
        }

        public void ShowTrayIconReminder()
        {
            var volatileSettings = this.SettingsService.VolatileSettings;
            if (!volatileSettings.TrayIconReminderWasShown)
            {
                var title = String.Format("{0} Is Still Running", TeamMateApplicationInfo.ApplicationName);
                var text = String.Format("{0} will continue to run in the background. To exit, right-click the tray icon and choose Exit.", TeamMateApplicationInfo.ApplicationName); ;
                var icon = System.Windows.Forms.ToolTipIcon.Info;
                this.trayIcon.ShowBalloonTip(TrayIconBallonTimeout, title, text, icon);

                volatileSettings.TrayIconReminderWasShown = true;
            }
        }
    }
}
