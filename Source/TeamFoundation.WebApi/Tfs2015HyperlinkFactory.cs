// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi
{
    public class Tfs2015HyperlinkFactory : HyperlinkFactory
    {
        public Tfs2015HyperlinkFactory(Uri baseUrl, string projectName)
        {
            this.BaseUrl = baseUrl;
            this.ProjectName = projectName;
        }

        public Uri BaseUrl { get; }

        public string ProjectName { get; }

        public override Uri GetNewWorkItemUrl(string workItemType, bool fullScreen = true)
        {
            // E.g. http://vstfxbox:8080/tfs/IEB/XBox%20Accessories/_workitems#_a=new&witd=Feature&fullScreen=true
            UriBuilder builder = new UriBuilder(this.BaseUrl);
            builder.Path = CombinePath(builder.Path, ProjectName, "_workItems");

            AddFragmentParam(builder, "_a", "new");
            AddFragmentParam(builder, "witd", workItemType);
            if (fullScreen)
            {
                AddFragmentParam(builder, "fullScreen", "true");
            }
            return builder.Uri;
        }

        public override Uri GetWorkItemQueryUrl(QueryHierarchyItem item, bool fullScreen = true)
        {
            // E.g. http://vstfxbox:8080/tfs/IEB/XBox%20Accessories/_workitems#path=Shared+Queries%2FCargo%2FApps%2FBugs%2FActive+Bugs&_a=query
            UriBuilder builder = new UriBuilder(this.BaseUrl);
            builder.Path = CombinePath(builder.Path, ProjectName, "_workItems");

            AddFragmentParam(builder, "_a", "query");
            AddFragmentParam(builder, "path", item.Path);
            if (fullScreen)
            {
                AddFragmentParam(builder, "fullScreen", "true");
            }
            return builder.Uri;
        }

        public override Uri GetWorkItemUrl(int id, bool fullScreen = true)
        {
            // E.g. http://vstfxbox:8080/tfs/IEB/XBox%20Accessories/_workitems#_a=edit&id=2053158&fullScreen=true
            UriBuilder builder = new UriBuilder(this.BaseUrl);
            builder.Path = CombinePath(builder.Path, ProjectName, "_workItems");

            AddFragmentParam(builder, "_a", "edit");
            AddFragmentParam(builder, "id", id);
            if (fullScreen)
            {
                AddFragmentParam(builder, "fullScreen", "true");
            }
            return builder.Uri;
        }

        private string CombinePath(params string[] paths)
        {
            return "/" + string.Join("/", paths.Select(p => p.Trim('/')).Where(p => p.Length > 0));
        }

        private static void AddFragmentParam(UriBuilder builder, string key, object value)
        {
            StringBuilder fragment = new StringBuilder(builder.Fragment);
            if (fragment.Length > 0 && fragment[0] == '#')
            {
                // Trim an initial ?, otherwise setting builder.Query again will duplicate it, it's a confusing API
                fragment.Remove(0, 1);
            }

            if (fragment.Length > 0)
            {
                fragment.Append('&');
            }

            fragment.Append(key);
            fragment.Append('=');
            fragment.Append(WebUtility.UrlEncode(string.Format(CultureInfo.InvariantCulture, "{0}", value)));

            builder.Fragment = fragment.ToString();
        }

    }
}
