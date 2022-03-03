using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.Tools.TeamMate.Foundation.Collections;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Xml;
using Microsoft.Tools.TeamMate.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Model
{
    class ProjectContextSerializer
    {
        public ICollection<TileInfo> ReadTiles(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");
            XDocument doc = XDocument.Load(filename);

            var result = new List<TileInfo>();
            foreach (XElement tileElement in doc.Elements(Schema.Tiles, Schema.Tile))
            {
                TileInfo tile = ReadTile(tileElement);
                result.Add(tile);
            }

            return result;
        }

        private TileInfo ReadTile(XElement e)
        {
            TileInfo tileInfo = new TileInfo();
            tileInfo.Type = e.GetAttribute<TileType>(Schema.Type);
            tileInfo.Name = e.GetAttribute<string>(Schema.Name);
            tileInfo.ShowNotifications = e.GetAttribute<bool>(Schema.ShowNotifications);
            tileInfo.IncludeInItemCountSummary = e.GetAttribute<bool>(Schema.IncludeInItemCountSummary);
            tileInfo.LastUpdated = e.GetAttribute<DateTime?>(Schema.LastUpdated);
            tileInfo.BackgroundColor = e.GetAttribute<string>(Schema.BackgroundColor);
            tileInfo.OrderByFieldName = e.GetAttribute<string>(Schema.OrderByFieldName);
            tileInfo.FilterByFieldName = e.GetAttribute<string>(Schema.FilterByFieldName);

            switch (tileInfo.Type)
            {
                case TileType.WorkItemQuery:
                    tileInfo.WorkItemQueryReference = ReadWorkItemQueryTileInfo(e.Element(Schema.WorkItemQueryInfo));
                    break;

                case TileType.PullRequestQuery:
                    tileInfo.PullRequestQueryInfo = ReadPullRequestQueryTileInfo(e.Element(Schema.PullRequestQueryInfo));
                    break;

                case TileType.BuiltIn:
                    tileInfo.BuiltInTileType = e.GetAttribute<BuiltInTileType>(Schema.BuiltInTileType);
                    break;

                default:
                    throw new NotSupportedException(String.Format("Tile type {0} is not supported", tileInfo.Type));
            }

            tileInfo.FontColor = e.GetAttribute<string>(Schema.FontColor);

            return tileInfo;
        }

        private static WorkItemQueryReference ReadWorkItemQueryTileInfo(XElement e)
        {
            Uri projectCollectionUri = e.GetRequiredAttribute<Uri>(Schema.ProjectCollectionUri);
            Guid id = e.GetRequiredAttribute<Guid>(Schema.Id);
            return new WorkItemQueryReference(projectCollectionUri, id);
        }

        private PullRequestQueryInfo ReadPullRequestQueryTileInfo(XElement e)
        {
            PullRequestQueryInfo query = new PullRequestQueryInfo();

            query.Name = e.GetAttribute<string>(Schema.Name);
            query.ReviewStatus = e.GetAttribute<PullRequestQueryReviewStatus>(Schema.ReviewStatus);
            query.AssignedTo = e.GetAttribute<string>(Schema.AssignedTo);
            query.CreatedBy = e.GetAttribute<string>(Schema.CreatedBy);

            return query;
        }

        public XDocument WriteTiles(ICollection<TileInfo> tiles)
        {
            Assert.ParamIsNotNull(tiles, "tiles");

            XDocument doc = new XDocument(
                new XElement(Schema.Tiles,
                    new XAttribute(Schema.Version, Schema.TilesCurrentVersion),
                    tiles.Select(q => WriteTile(q))
                )
            );

            return doc;
        }

        private XElement WriteTile(TileInfo tile)
        {
            XElement e = new XElement(Schema.Tile);
            e.SetAttribute<TileType>(Schema.Type, tile.Type);
            e.SetAttribute<string>(Schema.Name, tile.Name);
            e.SetAttribute<bool>(Schema.ShowNotifications, tile.ShowNotifications);
            e.SetAttribute<bool>(Schema.IncludeInItemCountSummary, tile.IncludeInItemCountSummary);
            e.SetAttribute<DateTime?>(Schema.LastUpdated, tile.LastUpdated);
            e.SetAttribute<string>(Schema.BackgroundColor, tile.BackgroundColor);
            e.SetAttribute<string>(Schema.OrderByFieldName, tile.OrderByFieldName);
            e.SetAttribute<string>(Schema.FilterByFieldName, tile.FilterByFieldName);

            switch (tile.Type)
            {
                case TileType.WorkItemQuery:
                    e.Add(WriteWorkItemQueryTileInfo(tile.WorkItemQueryReference));
                    break;

                case TileType.PullRequestQuery:
                    e.Add(WritePullRequestQueryTileInfo(tile.PullRequestQueryInfo));
                    break;

                case TileType.BuiltIn:
                    e.SetAttribute<BuiltInTileType>(Schema.BuiltInTileType, tile.BuiltInTileType);
                    break;

                default:
                    throw new NotSupportedException(String.Format("Tile type {0} is not supported", tile.Type));
            }

            e.SetAttribute<string>(Schema.FontColor, tile.FontColor);

            return e;
        }

        private XElement WriteWorkItemQueryTileInfo(WorkItemQueryReference query)
        {
            XElement e = new XElement(Schema.WorkItemQueryInfo);
            e.SetAttribute<Guid>(Schema.Id, query.Id);
            e.SetAttribute<Uri>(Schema.ProjectCollectionUri, query.ProjectCollectionUri);
            return e;
        }

        private XElement WritePullRequestQueryTileInfo(PullRequestQueryInfo query)
        {
            XElement e = new XElement(Schema.PullRequestQueryInfo);
            e.SetAttribute<string>(Schema.Name, query.Name);
            e.SetAttribute<PullRequestQueryReviewStatus>(Schema.ReviewStatus, query.ReviewStatus);
            e.SetAttribute<string>(Schema.CreatedBy, query.CreatedBy);
            e.SetAttribute<string>(Schema.AssignedTo, query.AssignedTo);

            return e;
        }

        public ICollection<WorkItemReference> ReadFlaggedWorkItems(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            XDocument doc = XDocument.Load(filename);

            ICollection<WorkItemReference> result = new HashSet<WorkItemReference>();
            foreach (var workItem in doc.Elements(Schema.FlaggedWorkItems, Schema.WorkItem))
            {
                WorkItemReference reference = ReadWorkItemReference(workItem);
                result.Add(reference);
            }

            return result;
        }

        public XDocument WriteFlaggedWorkItems(ICollection<WorkItemReference> workItems)
        {
            Assert.ParamIsNotNull(workItems, "workItems");

            XDocument doc = new XDocument(
                new XElement(Schema.FlaggedWorkItems,
                    new XAttribute(Schema.Version, Schema.FlaggedWorkItemsCurrentVersion),
                    workItems.OrderBy(wi => wi.Id).Select(wi => WriteWorkItemReference(wi))
            ));

            return doc;
        }

        public void ReadRecentItems(string filename, ProjectContext context)
        {
            Assert.ParamIsNotNull(filename, "filename");
            Assert.ParamIsNotNull(context, "context");

            XDocument doc = XDocument.Load(filename);

            XElement recentItems = doc.Element(Schema.RecentItems);
            context.TrackingInfo.RecentlyViewedWorkItems.AddRange(ReadSubItems(recentItems, Schema.RecentlyViewedWorkItems));
            context.TrackingInfo.RecentlyCreatedWorkItems.AddRange(ReadSubItems(recentItems, Schema.RecentlyCreatedWorkItems));
            context.TrackingInfo.RecentlyUpdatedWorkItems.AddRange(ReadSubItems(recentItems, Schema.RecentlyUpdatedWorkItems));
        }

        private IEnumerable<WorkItemReference> ReadSubItems(XElement container, XName subContainerName)
        {
            return container.Elements(subContainerName, Schema.WorkItem).Select(e => ReadWorkItemReference(e));
        }

        public XDocument WriteRecentItems(ProjectContext context)
        {
            Assert.ParamIsNotNull(context, "context");

            XDocument doc = new XDocument(
                new XElement(Schema.RecentItems,
                    new XAttribute(Schema.Version, Schema.RecentWorkItemsCurrentVersion),
                    new XElement(Schema.RecentlyViewedWorkItems, context.TrackingInfo.RecentlyViewedWorkItems.Select(wi => WriteWorkItemReference(wi))),
                    new XElement(Schema.RecentlyCreatedWorkItems, context.TrackingInfo.RecentlyCreatedWorkItems.Select(wi => WriteWorkItemReference(wi))),
                    new XElement(Schema.RecentlyUpdatedWorkItems, context.TrackingInfo.RecentlyUpdatedWorkItems.Select(wi => WriteWorkItemReference(wi)))
                )
            );

            return doc;
        }

        private WorkItemReference ReadWorkItemReference(XElement element)
        {
            int id = element.GetRequiredAttribute<int>(Schema.Id);
            Uri projectCollectionUri = element.GetRequiredAttribute<Uri>(Schema.ProjectCollectionUri);

            return new WorkItemReference(projectCollectionUri, id);
        }

        private PullRequestReference ReadPullRequestReference(XElement element)
        {
            int id = element.GetRequiredAttribute<int>(Schema.PullRequestId);
            Guid projectId = element.GetRequiredAttribute<Guid>(Schema.PullRequestProjectId);

            return new PullRequestReference(projectId, id);
        }

        private XElement WriteWorkItemReference(WorkItemReference workItem)
        {
            XElement result = new XElement(Schema.WorkItem);

            result.SetAttribute(Schema.Id, workItem.Id);
            result.SetAttribute(Schema.ProjectCollectionUri, workItem.ProjectCollectionUri);

            return result;
        }

        public void ReadLastRead(string filename, TrackingInfo model)
        {
            Assert.ParamIsNotNull(filename, "filename");
            Assert.ParamIsNotNull(model, "model");

            XDocument doc = XDocument.Load(filename);

            XElement lastReadElement = doc.Element(Schema.LastRead);

            model.ClearLastReadEntries();

            foreach (XElement element in lastReadElement.Elements(Schema.Entries, Schema.Entry))
            {
                LastReadEntry entry = ReadEntry(element);
                model.AddEntry(entry);
            }
        }

        public ProjectSettings ReadProjectSettings(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            XDocument doc = XDocument.Load(filename);
            XElement settingsElement = doc.Element(Schema.ProjectSettings);

            ProjectSettings settings = new ProjectSettings();
            return settings;
        }

        public XDocument WriteProjectSettings(ProjectSettings settings)
        {
            XDocument doc = new XDocument(
                new XElement(Schema.ProjectSettings,
                    new XAttribute(Schema.Version, Schema.ProjectSettingsCurrentVersion)
                )
            );

            return doc;
        }

        public XDocument WriteLastRead(TrackingInfo lastReadModel)
        {
            XDocument doc = new XDocument(
                new XElement(Schema.LastRead,
                    new XAttribute(Schema.Version, Schema.ReadWorkItemsCurrentVersion),
                    new XElement(Schema.Entries,
                        lastReadModel.LastReadEntries.Select(e => WriteEntry(e))
                    )
                )
            );

            return doc;
        }

        private LastReadEntry ReadEntry(XElement element)
        {
            int revision = element.GetAttribute<int>(Schema.Revision);
            DateTime date = element.GetAttribute<DateTime>(Schema.Date);

            object key = null;
            var workItemElement = element.Element(Schema.WorkItem);
            if (workItemElement != null)
            {
                key = ReadWorkItemReference(workItemElement);
            }
            else
            {
                var pullRequestElement = element.Element(Schema.PullRequest);
                if (pullRequestElement != null)
                {
                    key = ReadPullRequestReference(pullRequestElement);
                }
            }

            if (key == null)
            {
                throw new NotSupportedException("Read a last read XML element with an invalid or unrecognized key");
            }

            return new LastReadEntry(key, revision, date);
        }

        private XElement WriteEntry(LastReadEntry entry)
        {
            XElement result = new XElement(Schema.Entry);
            result.SetAttribute<int>(Schema.Revision, entry.Revision);
            result.SetAttribute<DateTime>(Schema.Date, entry.Date);

            WorkItemReference workItemReference = entry.Key as WorkItemReference;
            PullRequestReference pullRequestReference = entry.Key as PullRequestReference;
            GitPullRequest gitPullRequest = entry.Key as GitPullRequest;
            if (workItemReference != null)
            {
                result.Add(WriteWorkItemReference(workItemReference));
            }
            else if (gitPullRequest != null)
            {
                XElement e = new XElement(Schema.PullRequest);
                e.SetAttribute<Guid>(Schema.PullRequestProjectId, gitPullRequest.GetReference().ProjectId);
                e.SetAttribute<int>(Schema.PullRequestId, gitPullRequest.GetReference().Id);
                result.Add(e);
           }
            else if (pullRequestReference != null)
            {
                XElement e = new XElement(Schema.PullRequest);
                e.SetAttribute<Guid>(Schema.PullRequestProjectId, pullRequestReference.ProjectId);
                e.SetAttribute<int>(Schema.PullRequestId, pullRequestReference.Id);
                result.Add(e);
            }
            else
            {
                throw new NotSupportedException("Reference of type " + entry.Key.GetType().FullName + " cannot be serialized");
            }

            return result;
        }

        private static class Schema
        {
            public static readonly Version TilesCurrentVersion = new Version("1.0");
            public static readonly Version FlaggedWorkItemsCurrentVersion = new Version("1.0");
            public static readonly Version ReadWorkItemsCurrentVersion = new Version("1.0");
            public static readonly Version RecentWorkItemsCurrentVersion = new Version("1.0");
            public static readonly Version ProjectSettingsCurrentVersion = new Version("1.0");

            public const string Version = "Version";

            public static XName FlaggedWorkItems = "FlaggedWorkItems";

            public static XName WorkItem = "WorkItem";
            public const string Id = "Id";
            public const string ProjectCollectionUri = "ProjectCollectionUri";

            public const string ProjectUri = "ProjectUri";
            public const string ProjectName = "ProjectName";
            public const string TypeName = "TypeName";

            public const string AreaPath = "AreaPath";
            public const string AssignedTo = "AssignedTo";
            public const string ChangedBy = "ChangedBy";
            public const string ChangedDate = "ChangedDate";
            public const string CreatedBy = "CreatedBy";
            public const string CreatedDate = "CreatedDate";
            public const string IterationPath = "IterationPath";
            public const string State = "State";
            public const string Title = "Title";
            public const string Revision = "Revision";

            public static XName RecentItems = "RecentItems";
            public static XName RecentlyViewedWorkItems = "RecentlyViewedWorkItems";
            public static XName RecentlyCreatedWorkItems = "RecentlyCreatedWorkItems";
            public static XName RecentlyUpdatedWorkItems = "RecentlyUpdatedWorkItems";

            // Tiles stuff....
            public static readonly XName Tiles = "Tiles";
            public static readonly XName Tile = "Tile";
            public const string Type = "Type";
            public const string Name = "Name";
            public const string ShowNotifications = "ShowNotifications";
            public const string IncludeInItemCountSummary = "IncludeInItemCountSummary";
            public const string BackgroundColor = "BackgroundColor";
            public const string LastUpdated = "LastUpdated";
            public const string OrderByFieldName = "OrderByFieldName";
            public const string FilterByFieldName = "FilterByFieldName";
            public const string FontColor = "FontColor";

            // Work Item Stuff
            public static readonly XName WorkItemQueryInfo = "WorkItemQueryInfo";

            // Pull Request Stuff
            public static readonly XName PullRequestQueryInfo = "PullRequestQueryInfo";
            public static readonly XName Projects = "Projects";
            public const string ReviewStatus = "ReviewStatus";

            public static readonly XName LastRead = "LastRead";
            public static readonly XName Entries = "Entries";
            public static readonly XName Entry = "Entry";
            public const string Date = "Date";

            // Pull Request Reference Stuff
            public static readonly XName PullRequest = "PullRequest";
            public const string PullRequestId = "PullRequestId";
            public const string PullRequestProjectId = "PullRequestProjectId";

            // ProjectSettings Stuff
            public static readonly XName ProjectSettings = "ProjectSettings";

            public const string BuiltInTileType = "BuiltInTileType";
        }
    }
}
