using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;

namespace Microsoft.Tools.TeamMate.Office.AddIns
{
    [RunInstaller(true)]
    public class InstallUtilInstaller : Installer
    {
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            AddInInstaller installer = new AddInInstaller();
            installer.InstallAddIns();
        }

        public override void Uninstall(IDictionary savedState)
        {
            AddInInstaller installer = new AddInInstaller();
            installer.UninstallAddIns();

            base.Uninstall(savedState);
        }
    }
}
