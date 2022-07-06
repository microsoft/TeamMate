using Microsoft.Tools.TeamMate.Foundation.Collections;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Xml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Model.Settings
{
    class SettingsSerializer
    {
        public ApplicationSettings ReadSettings(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            ApplicationSettings settings = new ApplicationSettings();

            // TODO: Protect against missing elements? Need schema? Protect against version mismatch?
            XDocument doc = XDocument.Load(filename);
            XElement settingsElement = doc.Element(Schema.Settings);

            List<ProjectInfo> projects = new List<ProjectInfo>();

            foreach (XElement projectElement in settingsElement.Elements(Schema.Projects, Schema.Project))
            {
                ProjectInfo project = ReadProject(projectElement);
                projects.Add(project);
            }

            settings.Projects.AddRange(projects);

            var defaultWorkItemInfo = settingsElement.Element(Schema.DefaultWorkItemInfo);
            if (defaultWorkItemInfo != null)
            {
                var defaultWorkItemType = defaultWorkItemInfo.Element(Schema.WorkItemType);

                if (defaultWorkItemType != null)
                {
                    settings.DefaultWorkItemInfo = new DefaultWorkItemInfo(ReadWorkItemType(defaultWorkItemType));
                }
            }
            
            // All of these simple values have good defaults in the input settings. Only override the default value if something
            // actually existed in XML.
            settingsElement.ReadElementValue<bool>(Schema.IsTracingEnabled, (value) => settings.IsTracingEnabled = value);
            settingsElement.ReadElementValue<KeyGesture>(Schema.QuickCreateGesture, (value) => settings.QuickCreateGesture = value);
            settingsElement.ReadElementValue<KeyGesture>(Schema.QuickCreateWithOptionsGesture, (value) => settings.QuickCreateWithOptionsGesture = value);
            settingsElement.ReadElementValue<KeyGesture>(Schema.ToggleMainWindowGesture, (value) => settings.ToggleMainWindowGesture = value);
            settingsElement.ReadElementValue<bool>(Schema.LaunchOnStartup, (value) => settings.LaunchOnStartup = value);
            settingsElement.ReadElementValue<bool>(Schema.ShowSplashScreen, (value) => settings.ShowSplashScreen = value);
            settingsElement.ReadElementValue<bool>(Schema.PlayNotificationSound, (value) => settings.PlayNotificationSound = value);
            settingsElement.ReadElementValue<TimeSpan>(Schema.RefreshInterval, (value) => settings.RefreshInterval = value < ApplicationSettings.MinimumRefreshInterval ? ApplicationSettings.MinimumRefreshInterval : value);
            settingsElement.ReadElementValue<bool>(Schema.SearchIdsAutomatically, (value) => settings.SearchIdsAutomatically = value);
            settingsElement.ReadElementValue<bool>(Schema.ShowCountdown, (value) => settings.ShowCountdown = value);

            settingsElement.ReadElementValue<bool>(Schema.ShowItemCountInNotificationArea, (value) => settings.ShowItemCountInNotificationArea = value);
            settingsElement.ReadElementValue<bool>(Schema.ShowItemCountInOverviewWindow, (value) => settings.ShowItemCountInOverviewWindow = value);
            settingsElement.ReadElementValue<bool>(Schema.ShowItemCountInTaskBar, (value) => settings.ShowItemCountInTaskBar = value);
            
            return settings;
        }

        private ProjectInfo ReadProject(XElement e)
        {
            Uri projectCollectionUri = e.GetRequiredAttribute<Uri>(Schema.ProjectCollectionUri);
            Uri projectUri = e.GetAttribute<Uri>(Schema.ProjectUri);
            string projectName = e.GetAttribute<string>(Schema.Name);

            ProjectReference project = new ProjectReference(projectCollectionUri, projectUri);
            ProjectInfo result = new ProjectInfo(project, projectName);
            result.PreferredName = e.GetAttribute<string>(Schema.PreferredName);

            return result;
        }

        private WorkItemTypeReference ReadWorkItemType(XElement e)
        {
            var name = e.GetRequiredAttribute<string>(Schema.Name);

            Uri projectCollectionUri = e.GetRequiredAttribute<Uri>(Schema.ProjectCollectionUri);
            Uri projectUri = e.GetAttribute<Uri>(Schema.ProjectUri);

            ProjectReference project = new ProjectReference(projectCollectionUri, projectUri);
            return new WorkItemTypeReference(name, project);
        }

        private WindowStateInfo ReadWindowState(XElement e)
        {
            var state = e.GetRequiredAttribute<WindowState>(Schema.State);
            var bounds = ReadRect(e.GetRequiredAttribute<string>(Schema.Bounds));

            return new WindowStateInfo(state, bounds);
        }

        private static Rect ReadRect(string text)
        {
            return Rect.Parse(text);
        }

        private static string WriteRect(Rect rect)
        {
            return rect.ToString(CultureInfo.InvariantCulture);
        }

        public void WriteSettings(ApplicationSettings settings, string filename)
        {
            Assert.ParamIsNotNull(settings, "settings");

            XDocument doc = WriteSettings(settings);
            doc.Save(filename);
        }

        public XDocument WriteSettings(ApplicationSettings settings)
        {
            Assert.ParamIsNotNull(settings, "settings");

            XElement e = new XElement(Schema.Settings);
            e.SetAttribute(Schema.Version, Schema.CurrentVersion);

            e.Add(new XElement(Schema.Projects,
                settings.Projects.Select(p => WriteProject(p))
            ));

            if (settings.DefaultWorkItemInfo != null)
            {
                e.SetElementChild(Schema.DefaultWorkItemInfo, WriteWorkItemType(settings.DefaultWorkItemInfo.WorkItemType));
            }

            e.SetElementValue<bool>(Schema.IsTracingEnabled, settings.IsTracingEnabled);
            e.SetElementValue<bool>(Schema.LaunchOnStartup, settings.LaunchOnStartup);
            e.SetElementValue<bool>(Schema.ShowSplashScreen, settings.ShowSplashScreen);
            e.SetElementValue<bool>(Schema.PlayNotificationSound, settings.PlayNotificationSound);
            e.SetElementValue<TimeSpan>(Schema.RefreshInterval, settings.RefreshInterval);
            e.SetElementValue<KeyGesture>(Schema.QuickCreateGesture, settings.QuickCreateGesture);
            e.SetElementValue<KeyGesture>(Schema.QuickCreateWithOptionsGesture, settings.QuickCreateWithOptionsGesture);
            e.SetElementValue<KeyGesture>(Schema.ToggleMainWindowGesture, settings.ToggleMainWindowGesture);
            e.SetElementValue<bool>(Schema.SearchIdsAutomatically, settings.SearchIdsAutomatically);
            e.SetElementValue<bool>(Schema.ShowCountdown, settings.ShowCountdown);

            e.SetElementValue<bool>(Schema.ShowItemCountInNotificationArea, settings.ShowItemCountInNotificationArea);
            e.SetElementValue<bool>(Schema.ShowItemCountInOverviewWindow, settings.ShowItemCountInOverviewWindow);
            e.SetElementValue<bool>(Schema.ShowItemCountInTaskBar, settings.ShowItemCountInTaskBar);

            return new XDocument(e);
        }

        private XElement WriteProject(ProjectInfo project)
        {
            XElement e = new XElement(Schema.Project);
            e.SetAttribute(Schema.ProjectCollectionUri, project.Reference.ProjectCollectionUri);
            e.SetAttribute(Schema.ProjectUri, project.Reference.ProjectUri);
            e.SetAttribute(Schema.Name, project.ProjectName);
            e.SetAttribute(Schema.PreferredName, project.PreferredName);
            return e;
        }

        private XElement WriteWorkItemType(WorkItemTypeReference workItemType)
        {
            XElement e = new XElement(Schema.WorkItemType);
            e.SetAttribute(Schema.Name, workItemType.Name);
            e.SetAttribute(Schema.ProjectUri, workItemType.Project.ProjectUri);
            e.SetAttribute(Schema.ProjectCollectionUri, workItemType.Project.ProjectCollectionUri);
            return e;
        }

        private XElement WriteWindowState(WindowStateInfo windowState)
        {
            XElement e = new XElement(Schema.WindowState);
            e.SetAttribute(Schema.State, windowState.WindowState);
            e.SetAttribute(Schema.Bounds, WriteRect(windowState.RestoreBounds));
            return e;
        }

        public VolatileSettings ReadVolatileSettings(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            // TODO: Protect against missing elements? Need schema? Protect against version mismatch?
            XDocument doc = XDocument.Load(filename);
            XElement settingsElement = doc.Element(Schema.VolatileSettings);

            VolatileSettings settings = new VolatileSettings();

            foreach (XElement windowStateElement in settingsElement.Elements(Schema.Windows, Schema.WindowState))
            {
                string key = windowStateElement.GetRequiredAttribute<string>(Schema.Key);
                WindowStateInfo windowState = ReadWindowState(windowStateElement);
                settings.SetLastKnownState(key, windowState);
            }

            XElement lastUsedProjectElement = settingsElement.Element(Schema.LastUsedProject, Schema.Project);
            if (lastUsedProjectElement != null)
            {
                settings.LastUsedProject = ReadProject(lastUsedProjectElement);
            }

            settingsElement.ReadElementValue<bool>(Schema.IsWorkItemRibbonMinimized, (value) => settings.IsWorkItemRibbonMinimized = value);
            settingsElement.ReadElementValue<bool>(Schema.TrayIconReminderWasShown, (value) => settings.TrayIconReminderWasShown = value);
            settingsElement.ReadElementValue<bool>(Schema.OverviewWindowWasHiddenBefore, (value) => settings.OverviewWindowWasHiddenBefore = value);

            return settings;
        }

        public XDocument WriteVolatileSettings(VolatileSettings settings)
        {
            Assert.ParamIsNotNull(settings, "settings");

            XElement e = new XElement(Schema.VolatileSettings);
            e.SetAttribute(Schema.Version, Schema.CurrentVersion);

            XElement windows = new XElement(Schema.Windows);

            foreach (string key in settings.LastKnownStateKeys.OrderBy(k => k))
            {
                var state = settings.GetLastKnownState(key);
                if (state != null)
                {
                    XElement window = WriteWindowState(state);
                    window.SetAttribute(Schema.Key, key);
                    windows.Add(window);
                }
            }

            e.Add(windows);

            if (settings.LastUsedProject != null)
            {
                e.SetElementChild(Schema.LastUsedProject, WriteProject(settings.LastUsedProject));
            }

            e.SetElementValue<bool>(Schema.IsWorkItemRibbonMinimized, settings.IsWorkItemRibbonMinimized);
            e.SetElementValue<bool>(Schema.TrayIconReminderWasShown, settings.TrayIconReminderWasShown);
            e.SetElementValue<bool>(Schema.OverviewWindowWasHiddenBefore, settings.OverviewWindowWasHiddenBefore);

            return new XDocument(e);
        }

        private static class Schema
        {
            public static readonly Version CurrentVersion = new Version("1.0");

            public static readonly XName Settings = "Settings";
            public static readonly string Version = "Version";

            public static readonly XName Projects = "Projects";
            public static readonly XName Project = "Project";
            public static readonly string ProjectUri = "ProjectUri";
            public static readonly string ProjectCollectionUri = "ProjectCollectionUri";
            public static readonly string PreferredName = "PreferredName";

            public static readonly XName DefaultWorkItemInfo = "DefaultWorkItemInfo";
            public static readonly XName WorkItemType = "WorkItemType";
            public static readonly string Name = "Name";

            public static readonly XName LastWorkItemWindowState = "LastWorkItemWindowState";
            public static readonly XName LastMainWindowState = "LastMainWindowState";
            public static readonly XName WindowState = "WindowState";
            public static readonly string State = "State";
            public static readonly string Bounds = "Bounds";

            public static readonly XName IsTracingEnabled = "IsTracingEnabled";
            public static readonly XName LaunchOnStartup = "LaunchOnStartup";
            public static readonly XName ShowSplashScreen = "ShowSplashScreen";
            public static readonly XName PlayNotificationSound = "PlayNotificationSound";
            public static readonly XName ShowCountdown = "ShowCountdown";

            public static readonly XName QuickCreateGesture = "QuickCreateGesture";
            public static readonly XName QuickCreateWithOptionsGesture = "QuickCreateWithOptionsGesture";
            public static readonly XName ToggleMainWindowGesture = "ToggleMainWindowGesture";

            public static readonly XName RefreshInterval = "RefreshInterval";
            public static readonly XName SearchIdsAutomatically = "SearchIdsAutomatically";

            public static readonly XName VolatileSettings = "VolatileSettings";
            public static readonly XName Windows = "Windows";
            public static readonly XName LastUsedProject = "LastUsedProject";
            public static readonly XName LastKnownWindowStates = "LastKnownWindowStates";
            public static readonly string Key = "Key";
            public static readonly XName IsWorkItemRibbonMinimized = "IsWorkItemRibbonMinimized";
            public static readonly XName TrayIconReminderWasShown = "TrayIconReminderWasShown";
            public static readonly XName OverviewWindowWasHiddenBefore = "OverviewWindowWasHiddenBefore";

            public static readonly XName ShowItemCountInNotificationArea = "ShowItemCountInNotificationArea";
            public static readonly XName ShowItemCountInOverviewWindow = "ShowItemCountInOverviewWindow";
            public static readonly XName ShowItemCountInTaskBar = "ShowItemCountInTaskBar";
        }
    }
}
