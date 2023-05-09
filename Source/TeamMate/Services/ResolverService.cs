using Microsoft.Tools.TeamMate.Model;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.MemberEntitlementManagement.WebApi;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Services
{
    public class ResolverService
    {
        private List<GraphUser> GraphUserCache { get; set; }

        private List<GraphGroup> GraphGroupCache { get; set; }

        private List<GroupEntitlement> GroupEntitlements { get; set; }

        private List<Task> Tasks = new List<Task>();

        private async Task<List<GraphUser>> FetchUsersAsync(
            GraphHttpClient graphClient,
            MemberEntitlementManagementHttpClient memberEntitlementManagementClient)
        {
            if (GraphUserCache != null)
            {
                return GraphUserCache;
            }

            List<GraphUser> users = new List<GraphUser>();

            string continuationToken = null;
            do
            {
                var data = await graphClient.ListUsersAsync(null, continuationToken);
                continuationToken = data.ContinuationToken != null ? data.ContinuationToken.First() : null;
                foreach (var user in data.GraphUsers)
                {
                    users.Add(user);
                }
            }
            while (continuationToken != null);

            GraphUserCache = users;

            return users;
        }

        private async Task<List<GraphGroup>> FetchGroupsAsync(
            GraphHttpClient graphClient)
        {
            if (GraphGroupCache != null)
            {
                return GraphGroupCache;
            }

            List<GraphGroup> groups = new List<GraphGroup>();

            string continuationToken = null;
            do
            {
                // ListGroupsAsync
                var data = await graphClient.ListGroupsAsync(null, null, continuationToken);
                continuationToken = data.ContinuationToken != null ? data.ContinuationToken.First() : null;
                foreach (var group in data.GraphGroups)
                {
                    groups.Add(group);
                }
            }
            while (continuationToken != null);

            GraphGroupCache = groups;

            return groups;
        }

        private async Task<List<GroupEntitlement>> FetchGroupsEntitlementsAsync(
          MemberEntitlementManagementHttpClient memberEntitlementManagementClient)
        {
            if (GroupEntitlements != null)
            {
                return GroupEntitlements;
            }

            GroupEntitlements = await memberEntitlementManagementClient.GetGroupEntitlementsAsync();
           
            return GroupEntitlements;
        }

        public async void FetchDataSync(
            GraphHttpClient client,
            MemberEntitlementManagementHttpClient memberEntitlementManagementClient)
        {
            Tasks.Add(FetchUsersAsync(client, memberEntitlementManagementClient));
            Tasks.Add(FetchGroupsAsync(client));

            // TODO(MEM)
          // Tasks.Add(FetchGroupsEntitlementsAsync(memberEntitlementManagementClient));
        }

        public ObservableCollection<string> GetDisplayNames()
        {
            foreach (var task in Tasks)
            {
                task.Wait();
            }

            ObservableCollection<string> strings = new ObservableCollection<string>();

            foreach (var user in GraphUserCache)
            {
                strings.Add(user.DisplayName);
            }

            foreach (var user in GraphGroupCache)
            {
                strings.Add(user.DisplayName);
            }

            return strings;
        }

        public async Task<string> Resolve(
            GraphHttpClient client,
            MemberEntitlementManagementHttpClient memberEntitlementManagementClient,
            string value)
        {
            if (value == null)
            {
                return null;
            }

            foreach (var task in Tasks)
            {
                task.Wait();
            }

            foreach (var user in GraphUserCache)
            {
                if (user.MailAddress != null &&
                    (user.MailAddress.StartsWith(value)))
                {
                    var storageKey = client.GetStorageKeyAsync(user.Descriptor).Result;

                    // TODO(MEM)
                    return "{" + storageKey.Value.ToString() + "}";
                }
            }

            foreach (var group in GraphGroupCache)
            {
                if (group.MailAddress != null &&
                    (group.MailAddress.Contains(value)))
                {
                    var storageKey = client.GetStorageKeyAsync(group.Descriptor).Result;

                    // TODO(MEM)
                    return "{" + storageKey.Value.ToString() + "}";
                }
            }

            return null;
        }
    }
}
