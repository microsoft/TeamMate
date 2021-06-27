using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Client
{
    /// <summary>
    /// A client class to launch and interact with an installed TeamMateClient instace.
    /// </summary>
    public class TeamMateClient
    {
        private const string TeamMateDownloadUrl = "http://toolbox/teammate";

        /// <summary>
        /// Determines whether TeamMate is installed for the current user.
        /// </summary>
        public static bool IsInstalled()
        {
            bool isInstalled = Registry.GetValue(@"HKEY_CLASSES_ROOT\TeamMate.tmx", null, null) != null;
            return isInstalled;
        }

        /// <summary>
        /// Open the TeamMate download page in an external web browser.
        /// </summary>
        public static void LaunchDownloadPage()
        {
            Process.Start(TeamMateDownloadUrl);
        }

        /// <summary>
        /// Launches TeamMate, creating a work item with the given creation information.
        /// </summary>
        /// <param name="createInfo">The create information.</param>
        public void CreateWorkItem(WorkItemCreationInfo createInfo)
        {
            if (createInfo == null)
            {
                throw new ArgumentNullException("createInfo");
            }

            EnsureIsInstalled();

            string tmxFile = Path.Combine(Path.GetTempPath(), "Outlook.tmx");
            tmxFile = PathUtilities.EnsureFilenameIsUnique(tmxFile);

            XDocument doc = CreateActionXml(createInfo);
            doc.Save(tmxFile);
            Process.Start(tmxFile);
        }

        private static XDocument CreateActionXml(WorkItemCreationInfo createInfo)
        {
            XElement fieldsElement = new XElement("Fields");
            foreach (var entry in createInfo.Fields)
            {
                fieldsElement.Add(new XElement("Field", new XAttribute("Name", entry.Key), entry.Value));
            }

            XElement attachmentsElement = new XElement("Attachments");
            foreach (var attachment in createInfo.Attachments)
            {
                attachmentsElement.Add(new XElement("Attachment",
                    new XAttribute("Path", attachment.Path),
                    new XAttribute("DeleteOnSave", attachment.DeleteOnSave)
                ));
            }

            XDocument doc = new XDocument(
                new XElement("TeamMate",
                    new XAttribute("Version", "1.0"),
                    new XAttribute("DeleteOnLoad", "true"),
                    new XElement("Action",
                        new XAttribute("Type", "CreateWorkItem"),
                        new XElement("WorkItem",
                            fieldsElement,
                            attachmentsElement
                        )
                    )
                )
            );

            return doc;
        }

        private static void EnsureIsInstalled()
        {
            if (!IsInstalled())
            {
                throw new InvalidOperationException("TeamMate cannot be launched. Please install TeamMate from " + TeamMateDownloadUrl);
            }
        }
    }
}
