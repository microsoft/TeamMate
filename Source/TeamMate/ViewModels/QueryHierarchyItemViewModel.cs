using Microsoft.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class QueryHierarchyItemViewModel : TreeItemViewModelBase
    {
        private ProjectContext projectContext;

        public QueryHierarchyItemViewModel(ProjectContext projectContext, QueryHierarchyItem item = null)
        {
            this.projectContext = projectContext;
            this.Item = item;

            if (this.Item != null)
            {
                this.ItemType = ToHierarchyItemType(this.Item);
            }

            if (this.Item == null || this.Item.IsFolder == true)
            {
                this.InitializeAsContainer();
            }
        }

        public QueryHierarchyItem Item { get; private set; }

        public string Name { get; set; }

        public QueryHierarchyItemType ItemType { get; set; }

        protected override async Task<IEnumerable<TreeItemViewModelBase>> LoadChildrenAsync()
        {
            var project = this.projectContext.ProjectName;

            IEnumerable<QueryHierarchyItem> children = null;
            if (this.Item != null)
            {
                if (this.Item.IsFolder == true)
                {
                    await ChaosMonkey.ChaosAsync(ChaosScenarios.LoadQueryFolder);
                    children = await this.projectContext.ExecuteWithTokenRefreshAsync(async () =>
                    {
                        var client = this.projectContext.Connection.GetClient<WorkItemTrackingHttpClient>();
                        var selfWithChildren = await client.GetQueryAsync(project, this.Item.Id.ToString(), depth: 1, expand: QueryExpand.Wiql);
                        return selfWithChildren.Children;
                    });
                }
            }
            else
            {
                // This is the root, find root query folders (TODO: consider depth: 1 for 1 less call)
                children = await this.projectContext.ExecuteWithTokenRefreshAsync(async () =>
                {
                    var client = this.projectContext.Connection.GetClient<WorkItemTrackingHttpClient>();
                    return await client.GetQueriesAsync(project, expand: QueryExpand.Wiql);
                });
            }

            if (children == null)
            {
                children = new QueryHierarchyItem[0];
            }

            // Make sure children are sorted by folder first and then name
            children = children.OrderBy(c => c.IsFolder != true).ThenBy(c => c.Name);
            return children.Select(child => new QueryHierarchyItemViewModel(this.projectContext, child)).ToList();
        }

        private static QueryHierarchyItemType ToHierarchyItemType(QueryHierarchyItem item)
        {
            if (item.IsFolder == true)
            {
                return QueryHierarchyItemType.Folder;
            }

            var queryType = item.QueryType;
            if (queryType != null)
            {
                switch (queryType)
                {
                    case QueryType.Flat:
                        return QueryHierarchyItemType.FlatQuery;

                    case QueryType.OneHop:
                        return QueryHierarchyItemType.LinkQuery;

                    case QueryType.Tree:
                        return QueryHierarchyItemType.TreeQuery;
                }
            }

            return QueryHierarchyItemType.FlatQuery;
        }
    }

    public enum QueryHierarchyItemType
    {
        FlatQuery,
        LinkQuery,
        TreeQuery,
        Folder
    }

}
