using Microsoft.Tools.TeamMate.Foundation.Native;
using Microsoft.Tools.TeamMate.Model;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Security.Permissions;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Services
{
    public class RestartService
    {
        [Import]
        public MessageBoxService MessageBoxService { get; set; }

        [Import]
        public WindowService WindowService { get; set; }

        // Originally from System.Windows.Forms.Application, changed to suit needs
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void Restart()
        {
            if (Assembly.GetEntryAssembly() == null)
            {
                throw new NotSupportedException("RestartNotSupported");
            }

            if (TeamMateApplicationInfo.IsNetworkDeployed)
            {
                // IMPORTANT to get the full name before invoking the shutdown piece...
                string updatedApplicationFullName = TeamMateApplicationInfo.CurrentDeployment.UpdatedApplicationFullName;
                if (this.WindowService.RequestShutdown())
                {
                    NativeMethods.CorLaunchApplication(0, updatedApplicationFullName, 0, null, 0, null, new PROCESS_INFORMATION());
                }
            }
            else
            {
                this.MessageBoxService.Show("Sorry, restart is only available when deployed through ClickOnce.\n\nYou'll need to shutdown and restart the application manually.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
