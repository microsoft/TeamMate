using Microsoft.Internal.Tools.TeamMate.Foundation.Native;
using Microsoft.Internal.Tools.TeamMate.Foundation.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Internal.Tools.TeamMate.Office.Outlook
{
    public static class OutlookService
    {
        private const string OutlookWindowClassName = "rctrl_renwnd32";
        private const int CO_E_SERVER_EXEC_FAILURE = unchecked((int)0x80080005);

        public static void DisplayMail(MailMessage message)
        {
            using (new RetryMessageFilterScope())
            {
                Application app = LaunchOutlook();

                // The initialization hit is minimal, but by re-initializing the Outlook reference in each
                // Send operation we ensure that:
                // 1. We handle the integrity miscatch error;
                // 2. We don't need to handle the RPC error, since the Outlook reference that we have is 
                // always new;
                // 3. Lifetime of Outlook reference is short: we de-ref the Outlook Application reference
                // after every Send, so the Outlook process will be garbage collected and we won't have the 
                // Outlook sitting around the process spaces even though the user closed Outlook UI already.
                if (app.ActiveExplorer() == null) // show Inbox for the first time
                {
                    // TODO: Why were we forcing to show the inbox one?
                    // MAPIFolder inbox = app.Session.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                    // inbox.Display();
                }

                MAPIFolder outboxFolder = app.Session.GetDefaultFolder(OlDefaultFolders.olFolderOutbox);
                MailItem mailItem = (MailItem)outboxFolder.Items.Add(OlItemType.olMailItem);

                foreach (var attachment in message.Attachments)
                {
                    mailItem.Attachments.Add(attachment);
                }

                mailItem.To = (message.To != null) ? String.Join("; ", message.To) : String.Empty;
                mailItem.CC = (message.CC != null) ? String.Join("; ", message.CC) : String.Empty;
                mailItem.Subject = message.Subject;
                mailItem.BodyFormat = OlBodyFormat.olFormatHTML;
                mailItem.HTMLBody = message.HtmlBody;
                mailItem.Importance = (OlImportance)message.Importance;

                if (message.ReminderDate != null)
                {
                    mailItem.FlagDueBy = message.ReminderDate.Value;
                    mailItem.FlagStatus = OlFlagStatus.olFlagMarked;
                }

                ActivateAndMaximize(mailItem.GetInspector);
            }
        }

        public static void CreateAppointment(Appointment appointment)
        {
            using (new RetryMessageFilterScope())
            {
                Application app = LaunchOutlook();

                AppointmentItem appointmentItem = (AppointmentItem)app.CreateItem(OlItemType.olAppointmentItem);
                appointmentItem.MeetingStatus = OlMeetingStatus.olMeeting;
                appointmentItem.Subject = appointment.Subject;

                if (!String.IsNullOrEmpty(appointment.RtfBody))
                {
                    appointmentItem.RTFBody = Encoding.ASCII.GetBytes(appointment.RtfBody);
                }
                else
                {
                    appointmentItem.Body = appointment.Body;
                }

                string attendees = (appointment.RequiredAttendees != null) ? String.Join("; ", appointment.RequiredAttendees) : String.Empty;
                appointmentItem.RequiredAttendees = attendees;

                appointmentItem.Display(false);
                ActivateAndMaximize(appointmentItem.GetInspector);
            }
        }

        public static void SearchOutlookInbox(string searchText, bool searchAllItems = false)
        {
            using (new RetryMessageFilterScope())
            {
                Application app = LaunchOutlook();
                Folder folder = app.Session.GetDefaultFolder(OlDefaultFolders.olFolderInbox) as Folder;

                int count = app.Explorers.Count;
                Explorer explorer;

                bool newExplorer = false;

                if (count == 0)
                {
                    // If no explorers exist, create a new one...
                    explorer = app.Explorers.Add(folder, OlFolderDisplayMode.olFolderDisplayNormal);
                    newExplorer = true;
                }
                else
                {
                    // Otherwise, pick the active explorer (or the first available already existing explorer)
                    Explorer active = app.ActiveExplorer();
                    if (active == null)
                    {
                        active = app.Explorers[0];
                    }

                    active.CurrentFolder = folder;
                    explorer = active;
                }

                if (app.Session.DefaultStore.IsInstantSearchEnabled)
                {
                    var scope = (searchAllItems) ? OlSearchScope.olSearchScopeAllFolders : OlSearchScope.olSearchScopeCurrentFolder;
                    explorer.Search(searchText, scope);

                    // Make the explorer visible (if needed) and active
                    explorer.Activate();
                    BringToForeground(explorer.Caption);

                    if (newExplorer)
                    {
                        explorer.WindowState = OlWindowState.olMaximized;
                    }
                }
            }
        }

        private static Application LaunchOutlook()
        {
            if (!InteropUtilities.IsInterfaceRegistered(typeof(Application)))
            {
                throw new OutlookException("Failed to launch Outlook, it is not installed.");
            }

            try
            {
                Application app = new Application();
                return app;
            }
            catch (COMException comException)
            {
                if (comException.ErrorCode == CO_E_SERVER_EXEC_FAILURE)
                {
                    // E.g. System.Runtime.InteropServices.COMException (0x80080005): Retrieving the COM class factory for component with 
                    // CLSID {0006F03A-0000-0000-C000-000000000046} failed due to the following error: 80080005 Server execution failed 
                    // (Exception from HRESULT: 0x80080005 (CO_E_SERVER_EXEC_FAILURE)).

                    // Typically thrown when there is a mismatch between the integrity level of the current process
                    // and a pre-existing Outlook running instance (e.g. one is running in admin mode, the other is not)
                    // See: http://weblogs.asp.net/whaggard/archive/2007/11/05/failure-creating-an-outlook-application-object-on-vista.aspx
                    // While this is the most common cause for this error code, it is not 100% accurate.
                    throw new OutlookException("Could not start or connect to Outlook. This problem can occur if Outlook is running with different "
                    + "privileges than the current process (e.g. one is running with full administrator access and the other is not). Close Outlook and try again.",
                    comException);
                }

                throw new OutlookException(String.Format("There was an error attempting to launch Outlook: {0}", comException.Message), comException);
            }
        }

        private static void ActivateAndMaximize(_Inspector window)
        {
            if (window != null)
            {
                // Window Class name for the Outlook mail window.
                // There maybe multiple "Untitled - Message (HTML)" windows,
                // But FindWindow always find the last one created - the window
                // that was just created now.

                window.Activate();
                window.WindowState = OlWindowState.olMaximized;
                BringToForeground(window.Caption);
            }
        }

        private static void BringToForeground(string caption)
        {
            IntPtr hWnd = NativeMethods.FindWindow(OutlookWindowClassName, caption);
            if (hWnd != IntPtr.Zero)
            {
                // this is best effort attempt to activate mail window and 
                // error msg will not be displayed if the window related 
                // activate API calls fail
                NativeMethods.SetForegroundWindow(hWnd);
            }
        }
    }
}