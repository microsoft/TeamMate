using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;

namespace Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi
{
    public abstract class HyperlinkFactory
    {
        public static HyperlinkFactory Create(ServerVersion version, Uri baseUrl, string projectName)
        {
            switch(version)
            {
                case ServerVersion.Tfs2015:
                    return new Tfs2015HyperlinkFactory(baseUrl, projectName);

                case ServerVersion.Vsts:
                    return new VstsHyperlinkFactory(baseUrl, projectName);
            }

            throw new InvalidOperationException($"Unsupported server version: {version}.");
        }

        public abstract Uri GetNewWorkItemUrl(string workItemType, bool fullScreen = true);

        public Uri GetWorkItemUrl(WorkItem workItem, bool fullScreen = true)
        {
            return GetWorkItemUrl(workItem.Id.Value, fullScreen);
        }

        public abstract Uri GetWorkItemUrl(int id, bool fullScreen = true);

        public abstract Uri GetWorkItemQueryUrl(QueryHierarchyItem item, bool fullScreen = true);
    }
}
