using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using System;
using System.Media;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    public static class UserFeedback
    {
        public static void PlayAlertSound()
        {
            SystemSounds.Beep.Play();
        }

        public static void PlayErrorSound()
        {
            SystemSounds.Hand.Play();
        }

        public static void ShowError(Exception e)
        {
            ShowError(null, e);
        }

        public static void ShowError(Window owner, Exception e)
        {
            ShowError(owner, e.Message, e);
        }

        public static void ShowError(Window owner, string message, Exception e)
        {
            if (e != null)
                Log.Error(e);
            else
                Log.Error(message);

            #if DEBUG
            if (e != null)
            {
                // TODO: This is good, but hide the Continue/Quit buttons, just have an OK button (e.g. relabel Continue)
                ExceptionDialog.Show(owner, message, e);
            }
            else
            {
                DoShowError(owner, message);
            }
            #else
            DoShowError(owner, message);
            #endif
        }

        public static void ShowError(string message)
        {
            ShowError(null, message, null);
        }

        public static void ShowError(Window owner, string message)
        {
            ShowError(owner, message, null);
        }

        private static void DoShowError(Window owner, string message)
        {
            if (owner != null)
                MessageBox.Show(owner, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void UnhandledException(Exception e)
        {
            ExceptionDialog.Show(e);
        }
    }
}
