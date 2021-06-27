using Microsoft.Tools.TeamMate.Office.AddIns.Outlook;
using System;

namespace Microsoft.Tools.TeamMate.Office.AddIns
{
    public class AddInInstaller
    {
        private static readonly Type[] AddInTypes = new Type[] {
            typeof(OutlookAddIn)
        };

        public void InstallAddIns()
        {
            AddInRegistrationServices registration = new AddInRegistrationServices();
            foreach (Type type in AddInTypes)
            {
                registration.RegisterOfficeAddIn(type, LoadBehavior.Loaded_LoadFirstTimeThenOnDemand);
            }
        }

        public void UninstallAddIns()
        {
            AddInRegistrationServices registration = new AddInRegistrationServices();
            foreach (Type type in AddInTypes)
            {
                registration.UnregisterOfficeAddIn(type);
            }
        }
    }
}
