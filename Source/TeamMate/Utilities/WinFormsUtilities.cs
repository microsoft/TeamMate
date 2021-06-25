using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Reflection;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Interop;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Microsoft.Internal.Tools.TeamMate.Utilities
{
    public static class WinFormsUtilities
    {
        public static ToolStripMenuItem CreateMenuItem(ICommand command, Action action = null)
        {
            ToolStripMenuItem menuItem = new ToolStripMenuItem();

            RoutedCommand routedCommand = command as RoutedCommand;
            if (routedCommand != null)
            {
                KeyGesture gesture = routedCommand.InputGestures.OfType<KeyGesture>().FirstOrDefault();
                if (gesture != null)
                {
                    menuItem.ShortcutKeyDisplayString = KeyGestureUtilities.GetDisplayString(gesture);
                }
            }

            RoutedUICommand routedUICommand = command as RoutedUICommand;
            if (routedUICommand != null && routedUICommand.Text != null)
            {
                menuItem.Text = ReplaceMnemonics(routedUICommand.Text);
            }

            UICommand uiCommand = command as UICommand;
            if (uiCommand != null)
            {
                if (uiCommand.Text != null)
                {
                    menuItem.Text = ReplaceMnemonics(uiCommand.Text);
                }

                BitmapSource bitmapSource = uiCommand.Icon as BitmapSource;
                if (bitmapSource != null)
                {
                    menuItem.Image = InteropUtilities.BitmapFromBitmapSource(bitmapSource);
                }
            }

            if (action != null)
            {
                menuItem.Click += delegate(object sender, EventArgs e)
                {
                    action();
                };
            }

            return menuItem;
        }

        private static string ReplaceMnemonics(string text)
        {
            return text.Replace('_', '&');
        }


        public static void ShowContextMenu(NotifyIcon icon)
        {
            try
            {
                ReflectionUtilities.InvokeMethod(icon, "ShowContextMenu");
            }
            catch (Exception e)
            {
                Log.WarnAndBreak(e);
                if (icon.ContextMenuStrip != null && !icon.ContextMenuStrip.Visible)
                {
                    icon.ContextMenuStrip.Show(System.Windows.Forms.Cursor.Position);
                }
            }
        }
    }
}
