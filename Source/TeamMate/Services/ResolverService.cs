using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Services
{
    public class ResolverService
    {
        private Dictionary<string, VisualStudio.Services.Common.SubjectDescriptor> GraphUserCache { get; set; }

        private Dictionary<string, VisualStudio.Services.Common.SubjectDescriptor> GraphGroupCache { get; set; }
        
        private List<Task> Tasks = new List<Task>();

        private async Task<Dictionary<string, VisualStudio.Services.Common.SubjectDescriptor>> FetchUsersAsync(
            GraphHttpClient graphClient)
        {
            if (GraphUserCache != null)
            {
                return GraphUserCache;
            }

            var users = new Dictionary<string, VisualStudio.Services.Common.SubjectDescriptor>();

            string continuationToken = null;
            do
            {
                var data = await graphClient.ListUsersAsync(null, continuationToken);
                continuationToken = data.ContinuationToken != null ? data.ContinuationToken.First() : null;
                foreach (var user in data.GraphUsers)
                {
                    if (user.MailAddress != null)
                    {
                        users[user.MailAddress] = user.Descriptor;
                    }
                }
            }
            while (continuationToken != null);

            GraphUserCache = users;

            return users;
        }

        private async Task<Dictionary<string, VisualStudio.Services.Common.SubjectDescriptor>> FetchGroupsAsync(
            GraphHttpClient graphClient)
        {
            if (GraphGroupCache != null)
            {
                return GraphGroupCache;
            }

            var groups = new Dictionary<string, VisualStudio.Services.Common.SubjectDescriptor>();

            string continuationToken = null;
            do
            {
                // ListGroupsAsync
                var data = await graphClient.ListGroupsAsync(null, null, continuationToken);
                continuationToken = data.ContinuationToken != null ? data.ContinuationToken.First() : null;
                foreach (var group in data.GraphGroups)
                {
                    if (group.MailAddress != null)
                    {
                        groups[group.MailAddress] = group.Descriptor;
                    }
                }
            }
            while (continuationToken != null);

            GraphGroupCache = groups;

            return groups;
        }

        public void FetchDataSync(
            GraphHttpClient client)
        {
            Tasks.Add(FetchUsersAsync(client));
            Tasks.Add(FetchGroupsAsync(client));
        }

        public async Task<Guid?> Resolve(
            GraphHttpClient client,
            string value)
        {
            if (value == null)
            {
                return null;
            }

            await Task.Run(() => { foreach (var task in this.Tasks) { task.Wait(); } });

            foreach (var user in GraphUserCache)
            {
                if (user.Key.Contains(value))
                {
                    var storageKey = client.GetStorageKeyAsync(user.Value).Result;

                    return storageKey.Value;
                }
            }

            foreach (var group in GraphGroupCache)
            {
                if (group.Key.Contains(value))
                {
                    var storageKey = client.GetStorageKeyAsync(group.Value).Result;

                    return storageKey.Value;
                }
            }

            throw new ArgumentException("Could not resolve '" + value + "'. Try the full email for the person and/or group.");

        }
    }
}
