using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Win32
{
    /// <summary>
    /// Provides information about unsafe file extensions.
    /// </summary>
    public static class UnsafeFileExtensions
    {
        // From: http://office.microsoft.com/en-us/outlook-help/blocked-attachments-in-outlook-HA001229952.aspx

        private static readonly string[] unsafeExtensions = {
            ".ade", 	// Access Project Extension (Microsoft)
            ".adp", 	// Access Project (Microsoft)
            ".app", 	// Executable Application
            ".asp", 	// Active Server Page
            ".bas", 	// BASIC Source Code
            ".bat", 	// Batch Processing
            ".cer", 	// Internet Security Certificate File
            ".chm", 	// Compiled HTML Help
            ".cmd", 	// DOS CP/M Command File, Command File for Windows NT
            ".cnt", 	// Help file index
            ".com", 	// Command
            ".cpl", 	// Windows Control Panel Extension (Microsoft)
            ".crt", 	// Certificate File
            ".csh", 	// csh Script
            ".der", 	// DER Encoded X509 Certificate File
            ".exe", 	// Executable File
            ".fxp", 	// FoxPro Compiled Source (Microsoft)
            ".gadget", 	// Windows Vista gadget
            ".hlp", 	// Windows Help File
            ".hpj", 	// Project file used to create Windows Help File
            ".hta", 	// Hypertext Application
            ".inf", 	// Information or Setup File
            ".ins", 	// IIS Internet Communications Settings (Microsoft)
            ".isp", 	// IIS Internet Service Provider Settings (Microsoft)
            ".its", 	// Internet Document Set, Internet Translation
            ".js", 	    // JavaScript Source Code
            ".jse", 	// JScript Encoded Script File
            ".ksh", 	// UNIX Shell Script
            ".lnk", 	// Windows Shortcut File
            ".mad", 	// Access Module Shortcut (Microsoft)
            ".maf", 	// Access (Microsoft)
            ".mag", 	// Access Diagram Shortcut (Microsoft)
            ".mam", 	// Access Macro Shortcut (Microsoft)
            ".maq", 	// Access Query Shortcut (Microsoft)
            ".mar", 	// Access Report Shortcut (Microsoft)
            ".mas", 	// Access Stored Procedures (Microsoft)
            ".mat", 	// Access Table Shortcut (Microsoft)
            ".mau", 	// Media Attachment Unit
            ".mav", 	// Access View Shortcut (Microsoft)
            ".maw", 	// Access Data Access Page (Microsoft)
            ".mda", 	// Access Add-in (Microsoft), MDA Access 2 Workgroup (Microsoft)
            ".mdb", 	// Access Application (Microsoft), MDB Access Database (Microsoft)
            ".mde", 	// Access MDE Database File (Microsoft)
            ".mdt", 	// Access Add-in Data (Microsoft)
            ".mdw", 	// Access Workgroup Information (Microsoft)
            ".mdz", 	// Access Wizard Template (Microsoft)
            ".msc", 	// Microsoft Management Console Snap-in Control File (Microsoft)
            ".msh", 	// Microsoft Shell
            ".msh1", 	// Microsoft Shell
            ".msh2", 	// Microsoft Shell
            ".mshxml", 	// Microsoft Shell
            ".msh1xml", // Microsoft Shell
            ".msh2xml",	// Microsoft Shell
            ".msi", 	// Windows Installer File (Microsoft)
            ".msp", 	// Windows Installer Update
            ".mst", 	// Windows SDK Setup Transform Script
            ".ops", 	// Office Profile Settings File
            ".osd", 	// Application virtualized with Microsoft SoftGrid Sequencer
            ".pcd", 	// Visual Test (Microsoft)
            ".pif", 	// Windows Program Information File (Microsoft)
            ".plg", 	// Developer Studio Build Log
            ".prf", 	// Windows System File
            ".prg", 	// Program File
            ".pst", 	// MS Exchange Address Book File, Outlook Personal Folder File (Microsoft)
            ".reg", 	// Registration Information/Key for W95/98, Registry Data File
            ".scf", 	// Windows Explorer Command
            ".scr", 	// Windows Screen Saver
            ".sct", 	// Windows Script Component, Foxpro Screen (Microsoft)
            ".shb", 	// Windows Shortcut into a Document
            ".shs", 	// Shell Scrap Object File
            ".ps1", 	// Windows PowerShell
            ".ps1xml", 	// Windows PowerShell
            ".ps2", 	// Windows PowerShell
            ".ps2xml", 	// Windows PowerShell
            ".psc1", 	// Windows PowerShell
            ".psc2", 	// Windows PowerShell
            ".tmp", 	// Temporary File/Folder
            ".url", 	// Internet Location
            ".vb", 	    // VBScript File or Any VisualBasic Source
            ".vbe", 	// VBScript Encoded Script File
            ".vbp", 	// Visual Basic project file
            ".vbs", 	// VBScript Script File, Visual Basic for Applications Script
            ".vsmacros", 	// Visual Studio .NET Binary-based Macro Project (Microsoft)
            ".vsw", 	// Visio Workspace File (Microsoft)
            ".ws", 	    // Windows Script File
            ".wsc", 	// Windows Script Component
            ".wsf", 	// Windows Script File
            ".wsh", 	// Windows Script Host Settings File
            ".xnk", 	// Exchange Public Folder Shortcut
        };

        private static ICollection<string> readOnlyUnsafeExtensions = new List<string>(unsafeExtensions).AsReadOnly();
        private static ICollection<string> unsafeExtensionsSet = new HashSet<string>(unsafeExtensions, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Determines whether the file is unsafe to open or not.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public static bool IsUnsafeFile(string filePath)
        {
            return IsUnsafeFileExtension(Path.GetExtension(filePath));
        }

        /// <summary>
        /// Determines whether the file extension is unsafe to open or not.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        public static bool IsUnsafeFileExtension(string extension)
        {
            return unsafeExtensionsSet.Contains(extension);
        }

        /// <summary>
        /// Gets the collection of well known unsafe file extensions.
        /// </summary>
        public static ICollection<string> WellKnownUnsafeExtensions
        {
            get { return readOnlyUnsafeExtensions; }
        }
    }
}
