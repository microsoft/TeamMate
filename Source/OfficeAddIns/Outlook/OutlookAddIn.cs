using Extensibility;
using Microsoft.Internal.Tools.TeamMate.Client;
using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.IO;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows;
using Microsoft.Internal.Tools.TeamMate.Office.AddIns.Resources;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Application = Microsoft.Office.Interop.Outlook.Application;

namespace Microsoft.Internal.Tools.TeamMate.Office.AddIns.Outlook
{
    [Guid("488182BC-A0F0-454B-8D29-0B452D93D6A8"), ProgId("TeamMate.OutlookAddIn"), ComVisible(true)]
    [OfficeAddInInfo("Outlook", "TeamMate Outlook Add-in", "This add-in integrates Outlook with TeamMate and Team Foundation Server")]
    public class OutlookAddIn : IDTExtensibility2, IRibbonExtensibility
    {
        private Application app;
        private IRibbonUI ribbon;
        private Explorer activeExplorer;

        public void OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
        {
            this.app = (Application)Application;

            // TODO: If no active explorer?
            this.activeExplorer = this.app.ActiveExplorer();
            if (this.activeExplorer != null)
            {
                this.activeExplorer.SelectionChange += HandleSelectionChange;
            }
        }

        public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
            if (this.activeExplorer != null)
            {
                this.activeExplorer.SelectionChange -= HandleSelectionChange;
                this.activeExplorer = null;
            }

            this.ribbon = null;
            this.app = null;
        }

        public void OnStartupComplete(ref Array custom)
        {
        }

        public void OnBeginShutdown(ref Array custom)
        {
        }

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public string GetCustomUI(string RibbonID)
        {
            return AddInResources.OutlookCustomUI;
        }

        private void SendCurrentMailToTeamMate()
        {
            MailItem mailItem = app.ActiveInspector().CurrentItem as MailItem;
            if (mailItem != null)
            {
                SendToTeamMate(new MailItem[] { mailItem });
            }
        }

        private void SendSelectionToTeamMate()
        {
            var mailItems = GetSelectedMailItems();
            if (mailItems.Any())
            {
                SendToTeamMate(mailItems);
            }
        }

        [DebuggerStepThrough]
        private ICollection<MailItem> GetSelectedMailItems()
        {
            try
            {
                return app.ActiveExplorer().Selection.OfType<MailItem>().ToArray();
            }
            catch (COMException e)
            {
                // Sometimes, the Selection propety above throws a COMException (e.g. when you unflag a currently
                // selected work item from the flagged list)
                Log.Error(e, "An error occurred attempting to get the current mail item selection");
                return new MailItem[0];
            }
        }

        private void SendToTeamMate(ICollection<MailItem> mailItems)
        {
            if (mailItems.Any())
            {
                MessageBox.Show("Sorry, creating work items from Outlook is no longer supported.\n\nWe are working on a work item experience optimized for VSTS and plan to restore this feature in the future.", "Send to TeamMate", MessageBoxButton.OK, MessageBoxImage.Information);

                /*
                // TODO: Creating a work item from Outlook is no longer doable since we can no longer create a temporary client-side work item
                // that gets edited in the rich work item form. All we can do is launch a web browser. Temporarily disable this feature until
                // VSTS adds support for pre-populating a work item with a bunch of information.

                WorkItemCreationInfo createInfo = new WorkItemCreationInfo();

                if (mailItems.Count == 1)
                {
                    MailItem item = mailItems.First();

                    createInfo.Title = GetTitle(item);
                    createInfo.History = GetHtmlBody(item);

                    // TODO: If description was present and visible, set on description and leave history as this...
                    // history = "See attached thread for more information.";
                }

                foreach (var item in mailItems)
                {
                    string outputMessageFile = PathUtilities.ToValidFileName(item.Subject + ".msg");

                    // TODO: Need to make sure path did not exceed max path length.
                    string tempAttachmentPath = PathUtilities.GetTempFilename(outputMessageFile);
                    item.SaveAs(tempAttachmentPath);

                    createInfo.Attachments.Add(new AttachmentInfo(tempAttachmentPath, deleteOnSave: true));
                }

                TeamMateClient teamMate = new TeamMateClient();
                teamMate.CreateWorkItem(createInfo);
                */
            }
        }

        private static string GetTitle(MailItem item)
        {
            string title = item.Subject;

            // TODO: These are different by language I think... Do I care?
            if (title.StartsWith("RE: ") || title.StartsWith("FW: "))
            {
                title = title.Substring(4);
            }

            return title;
        }

        private static string GetHtmlBody(MailItem item)
        {
            MailItem reply = null;
            string htmlBody = null;

            try
            {
                reply = item.Reply();
                htmlBody = reply.HTMLBody;
            }
            catch (System.Exception ex)
            {
                Log.ErrorAndBreak(ex);

                if (reply != null)
                {
                    reply.Delete();
                }
            }
            return htmlBody;
        }

        public void Ribbon_OnLoad(IRibbonUI ribbon)
        {
            this.ribbon = ribbon;
        }

        public object Ribbon_LoadImage(string imageId)
        {
            try
            {
                System.Drawing.Icon resource = AddInResources.ResourceManager.GetObject(imageId, AddInResources.Culture) as System.Drawing.Icon;
                return (resource != null) ? PictureDispInteropUtilities.IconToPictureDisp(resource) : null;
            }
            catch (System.Exception e)
            {
                Log.ErrorAndBreak(e);
                return null;
            }
        }

        public void Ribbon_OnAction(IRibbonControl control)
        {
            try
            {
                string id = control.Id;

                switch (id)
                {
                    case "SendSelectionToTeamMate":
                    case "SendSelectionToTeamMate2":
                        SendSelectionToTeamMate();
                        break;

                    default:
                        SendCurrentMailToTeamMate();
                        break;
                }
            }
            catch (System.Exception e)
            {
                Log.ErrorAndBreak(e);
                UserFeedback.ShowError(e);
            }
        }

        public bool Ribbon_GetEnabled(IRibbonControl control)
        {
            try
            {
                string id = control.Id;

                switch (id)
                {
                    case "SendSelectionToTeamMate":
                    case "SendSelectionToTeamMate2":
                        var selection = app.ActiveExplorer().Selection;
                        return (selection.Count > 0);

                    default:
                        return true;
                }
            }
            catch (System.Exception e)
            {
                Log.ErrorAndBreak(e);
                return false;
            }
        }

        private void HandleSelectionChange()
        {
            try
            {
                if (ribbon != null)
                {
                    ribbon.InvalidateControl("SendSelectionToTeamMate2");
                    ribbon.InvalidateControl("SendSelectionToTeamMate");
                }
            }
            catch (System.Exception e)
            {
                Log.ErrorAndBreak(e);
            }
        }
    }
}
