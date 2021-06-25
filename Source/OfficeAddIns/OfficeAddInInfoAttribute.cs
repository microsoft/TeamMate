using System;

namespace Microsoft.Internal.Tools.TeamMate.Office.AddIns
{
    public class OfficeAddInInfoAttribute : Attribute
    {
        public OfficeAddInInfoAttribute(string applicationName, string friendlyName, string description)
        {
            this.ApplicationName = applicationName;
            this.FriendlyName = friendlyName;
            this.Description = description;
        }

        public string ApplicationName { get; private set; }
        public string FriendlyName { get; private set; }
        public string Description { get; private set; }
    }
}
