using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi
{
    internal class VstsHyperlinkFactory : HyperlinkFactory
    {
        public VstsHyperlinkFactory(Uri baseUrl, string projectName)
        {
            this.BaseUrl = baseUrl;
            this.ProjectName = projectName;
        }

        public Uri BaseUrl { get; }

        public string ProjectName { get; }

        public Uri GetHomePageUrl(string teamName)
        {
            UriBuilder builder = new UriBuilder(this.BaseUrl);
            builder.Path = CombinePath(builder.Path, ProjectName, teamName);
            return builder.Uri;
        }

        public Uri GetIdentityImageUrl(Guid id)
        {
            UriBuilder builder = new UriBuilder(this.BaseUrl);
            builder.Path = CombinePath(builder.Path, "_api/_common/IdentityImage");
            AddQueryParam(builder, "id", id);
            return builder.Uri;
        }

        public override Uri GetNewWorkItemUrl(string workItemType, bool fullScreen = true)
        {
            // E.g. https://microsofthealth.visualstudio.com/Health/_queries?witd=Bug&id=1&_a=new&fullScreen=true
            UriBuilder builder = new UriBuilder(this.BaseUrl);
            builder.Path = CombinePath(builder.Path, ProjectName, "_queries");

            AddQueryParam(builder, "witd", workItemType);
            AddQueryParam(builder, "_a", "new");
            if (fullScreen)
            {
                AddQueryParam(builder, "fullScreen", "true");
            }
            return builder.Uri;
        }

        public override Uri GetWorkItemUrl(int id, bool fullScreen = true)
        {
            UriBuilder builder = new UriBuilder(this.BaseUrl);
            builder.Path = CombinePath(builder.Path, ProjectName, "_workItems");

            AddQueryParam(builder, "id", id);
            if (fullScreen)
            {
                AddQueryParam(builder, "fullScreen", "true");
            }
            return builder.Uri;
        }

        public override Uri GetWorkItemQueryUrl(QueryHierarchyItem item, bool fullScreen = true)
        {
            return GetWorkItemQueryUrl(item.Id, fullScreen);
        }

        public Uri GetWorkItemQueryUrl(Guid id, bool fullScreen = true)
        {
            // E.g. https://microsofthealth.visualstudio.com/Health/_queries?id=b6f55eea-2388-4b2e-8825-e631d8a53916&_a=query
            UriBuilder builder = new UriBuilder(this.BaseUrl);
            builder.Path = CombinePath(builder.Path, ProjectName, "_queries");
            AddQueryParam(builder, "_a", "query");
            AddQueryParam(builder, "id", id);

            if (fullScreen)
            {
                AddQueryParam(builder, "fullScreen", "true");
            }

            return builder.Uri;
        }

        public Uri GetWorkItemQueryUrlForWiql(string wiql, string name = null, bool fullScreen = true)
        {
            UriBuilder builder = new UriBuilder(this.BaseUrl);
            builder.Path = CombinePath(builder.Path, ProjectName, "_workItems");
            AddQueryParam(builder, "_a", "query");
            AddQueryParam(builder, "wiql", wiql);

            if (!string.IsNullOrEmpty(name))
            {
                AddQueryParam(builder, "name", name);
            }

            if (fullScreen)
            {
                AddQueryParam(builder, "fullScreen", "true");
            }

            return builder.Uri;
        }

        public Uri GetTestSuiteUrl(int planId, int suiteId)
        {
            UriBuilder builder = new UriBuilder(this.BaseUrl);
            builder.Path = CombinePath(builder.Path, ProjectName, "_testManagement");
            AddQueryParam(builder, "_a", "tests");
            AddQueryParam(builder, "planId", planId);
            AddQueryParam(builder, "suiteId", suiteId);
            return builder.Uri;
        }

        private string CombinePath(params string[] paths)
        {
            return "/" + string.Join("/", paths.Select(p => p.Trim('/')).Where(p => p.Length > 0));
        }

        private static void AddQueryParam(UriBuilder builder, string key, object value)
        {
            StringBuilder query = new StringBuilder(builder.Query);
            if (query.Length > 0 && query[0] == '?')
            {
                // Trim an initial ?, otherwise setting builder.Query again will duplicate it, it's a confusing API
                query.Remove(0, 1);
            }

            if (query.Length > 0)
            {
                query.Append('&');
            }

            query.Append(key);
            query.Append('=');
            query.Append(WebUtility.UrlEncode(string.Format(CultureInfo.InvariantCulture, "{0}", value)));

            builder.Query = query.ToString();
        }
    }
}
