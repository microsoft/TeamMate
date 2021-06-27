using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Collections;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.IO;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Model.Settings;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Services
{
    public class ProjectDataService
    {
        private const string ProjectsFolder = "Projects";
        private const string FlaggedWorkItemsFilename = "FlaggedWorkItems.xml";
        private const string RecentWorkItemsFilename = "RecentWorkItems.xml";
        private const string TilesFilename = "Tiles.xml";
        private const string ReadWorkItemsFilename = "ReadWorkItems.xml";
        private const string SettingsFilename = "Settings.xml";

        private ProjectContext projectContext;
        private DeferredAction deferredFlushReadWorkItems;
        private DeferredAction deferredFlushFlaggedWorkItems;

        public ProjectDataService()
        {
            this.deferredFlushReadWorkItems = new DeferredAction(FlushReadWorkItems);
            this.deferredFlushFlaggedWorkItems = new DeferredAction(FlushFlaggedWorkItems);
        }

        [Import]
        public SessionService SessionService { get; set; }

        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public AsyncWriterService AsyncWriterService { get; set; }


        public IDisposable DeferReadWorkItemsUpdate()
        {
            return this.deferredFlushReadWorkItems.Acquire();
        }

        public IDisposable DeferFlaggedWorkItemsUpdate()
        {
            return this.deferredFlushFlaggedWorkItems.Acquire();
        }

        public void Initialize()
        {
            this.SessionService.Session.ProjectContextChanged += HandleProjectContextChanged;
            this.SettingsService.Settings.ProjectsRemoved += HandleProjectsRemoved;
        }

        private void HandleProjectsRemoved(object sender, ProjectsRemovedEventArgs e)
        {
            foreach (var removedProject in e.RemovedProjects)
            {
                ClearData(removedProject.Reference);
            }
        }

        public ProjectContext Load(ProjectReference project)
        {
            Assert.ParamIsNotNull(project, "project");

            TeamMateApplicationInfo.AssertDataDirectoryAccessIsAllowed();

            ProjectContext projectContext = new ProjectContext(project);
            ProjectContextSerializer serializer = new ProjectContextSerializer();

            using (Log.PerformanceBlock("Reading recent and flagged items"))
            {
                try
                {
                    string file = GetFilePath(projectContext, FlaggedWorkItemsFilename);
                    if (File.Exists(file))
                    {
                        projectContext.TrackingInfo.FlaggedWorkItems.AddRange(serializer.ReadFlaggedWorkItems(file));
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }

                try
                {
                    string file = GetFilePath(projectContext, RecentWorkItemsFilename);
                    if (File.Exists(file))
                    {
                        serializer.ReadRecentItems(file, projectContext);
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }
            }

            ICollection<TileInfo> tiles = null;

            try
            {
                string file = GetFilePath(projectContext, TilesFilename);
                if (File.Exists(file))
                {
                    tiles = serializer.ReadTiles(file);
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            if (tiles == null)
            {
                tiles = new List<TileInfo>();
            }

            // Add default tiles if not present
            if (!tiles.Any(t => t.BuiltInTileType == BuiltInTileType.Flagged))
            {
                tiles.Add(TileInfo.CreateBuiltIn(BuiltInTileType.Flagged));
            }

            projectContext.Tiles.AddRange(tiles);

            try
            {
                string file = GetFilePath(projectContext, ReadWorkItemsFilename);
                if (File.Exists(file))
                {
                    serializer.ReadLastRead(file, projectContext.TrackingInfo);
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            try
            {
                string file = GetFilePath(projectContext, SettingsFilename);
                if (File.Exists(file))
                {
                    projectContext.ProjectSettings = serializer.ReadProjectSettings(file);
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            return projectContext;
        }

        public void ClearData(ProjectReference project)
        {
            string projectFolder = GetFolderPath(project);
            if (Directory.Exists(projectFolder))
            {
                PathUtilities.DeleteRecursively(projectFolder, DeleteMode.Force);
            }
        }

        private void SetProjectContext(ProjectContext projectContext)
        {
            if (this.projectContext != null)
            {
                this.projectContext.TrackingInfo.FlaggedWorkItemsChanged -= HandleFlaggedWorkItemsChanged;
                this.projectContext.TrackingInfo.RecentWorkItemsChanged -= HandleRecentWorkItemsChanged;
                this.projectContext.TrackingInfo.LastReadChanged -= HandleLastReadChanged;
                this.projectContext.Tiles.CollectionChanged -= HandleTilesChanged;

                foreach (var tile in this.projectContext.Tiles)
                {
                    tile.Changed -= HandleTileChanged;
                }

                this.projectContext.ProjectSettings.SettingsChanged -= HandleProjectSettingsChanged;
            }

            this.projectContext = projectContext;

            if (this.projectContext != null)
            {
                this.projectContext.TrackingInfo.FlaggedWorkItemsChanged += HandleFlaggedWorkItemsChanged;
                this.projectContext.TrackingInfo.RecentWorkItemsChanged += HandleRecentWorkItemsChanged;
                this.projectContext.TrackingInfo.LastReadChanged += HandleLastReadChanged;
                this.projectContext.Tiles.CollectionChanged += HandleTilesChanged;

                foreach (var tile in this.projectContext.Tiles)
                {
                    tile.Changed += HandleTileChanged;
                }

                this.projectContext.ProjectSettings.SettingsChanged += HandleProjectSettingsChanged;
            }
        }

        private void HandleProjectSettingsChanged(object sender, EventArgs e)
        {
            FlushProjectSettings();
        }

        private void HandleTileChanged(object sender, EventArgs e)
        {
            FlushTiles();
        }

        private string GetFilePath(ProjectContext project, string filename)
        {
            return Path.Combine(GetFolderPath(project.Reference), filename);
        }

        private static string GetFolderPath(ProjectReference project)
        {
            return Path.Combine(TeamMateApplicationInfo.DataDirectory, ProjectsFolder, project.ProjectId.ToString());
        }

        private void HandleProjectContextChanged(object sender, EventArgs e)
        {
            SetProjectContext(((Session)sender).ProjectContext);
        }

        private void HandleRecentWorkItemsChanged(object sender, EventArgs e)
        {
            FlushRecentWorkItems();
        }

        private void HandleFlaggedWorkItemsChanged(object sender, EventArgs e)
        {
            this.deferredFlushFlaggedWorkItems.InvokeIfNotDeferred();
        }

        private void HandleLastReadChanged(object sender, EventArgs e)
        {
            this.deferredFlushReadWorkItems.InvokeIfNotDeferred();
        }

        private void FlushRecentWorkItems()
        {
            ProjectContextSerializer serializer = new ProjectContextSerializer();
            XDocument doc = serializer.WriteRecentItems(projectContext);
            this.AsyncWriterService.Save(doc, GetFilePath(projectContext, RecentWorkItemsFilename));
        }

        private void FlushFlaggedWorkItems()
        {
            // TODO: Ideally only flush if we did see an update.
            ProjectContextSerializer serializer = new ProjectContextSerializer();
            XDocument doc = serializer.WriteFlaggedWorkItems(projectContext.TrackingInfo.FlaggedWorkItems);
            this.AsyncWriterService.Save(doc, GetFilePath(projectContext, FlaggedWorkItemsFilename));
        }

        private void FlushTiles()
        {
            ProjectContextSerializer serializer = new ProjectContextSerializer();
            XDocument doc = serializer.WriteTiles(projectContext.Tiles);
            this.AsyncWriterService.Save(doc, GetFilePath(projectContext, TilesFilename));
        }

        private void FlushReadWorkItems()
        {
            // TODO: Ideally only flush if we did see an update.
            ProjectContextSerializer serializer = new ProjectContextSerializer();
            XDocument doc = serializer.WriteLastRead(projectContext.TrackingInfo);
            this.AsyncWriterService.Save(doc, GetFilePath(projectContext, ReadWorkItemsFilename));
        }

        private void FlushProjectSettings()
        {
            ProjectContextSerializer serializer = new ProjectContextSerializer();
            XDocument doc = serializer.WriteProjectSettings(projectContext.ProjectSettings);
            this.AsyncWriterService.Save(doc, GetFilePath(projectContext, SettingsFilename));
        }

        private void HandleTilesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (TileInfo tile in e.OldItems)
                {
                    tile.Changed -= HandleTileChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (TileInfo tile in e.NewItems)
                {
                    tile.Changed += HandleTileChanged;
                }
            }

            FlushTiles();
        }
    }
}
