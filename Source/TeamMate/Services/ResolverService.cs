using Microsoft.VisualStudio.Services.Graph.Client;
using System;
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
        
        private List<Task> Tasks = new List<Task>();

        private async Task<List<GraphUser>> FetchUsersAsync(
            GraphHttpClient graphClient)
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

        public async void FetchDataSync(
            GraphHttpClient client)
        {
            Tasks.Add(FetchUsersAsync(client));
            Tasks.Add(FetchGroupsAsync(client));
        }

        public void WaitToComplete()
        {
            foreach (var task in Tasks)
            {
                task.Wait();
            }
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

        public async Task<Guid?> Resolve(
            GraphHttpClient client,
            string value)
        {
            if (value == null)
            {
                return null;
            }

            foreach (var user in GraphUserCache)
            {
                if (user.MailAddress != null &&
                    (user.MailAddress.Contains(value)))
                {
                    var storageKey = client.GetStorageKeyAsync(user.Descriptor).Result;

                    return storageKey.Value;
                }
            }

            foreach (var group in GraphGroupCache)
            {
                if (group.MailAddress != null &&
                    (group.MailAddress.Contains(value)))
                {
                    var storageKey = client.GetStorageKeyAsync(group.Descriptor).Result;

                    return storageKey.Value;
                }
            }

            throw new ArgumentException("Could not resolve '" + value + "'. Try the full email for the person and/or group.");

        }
    }
}
