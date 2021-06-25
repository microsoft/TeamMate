using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    public static class WorkItemTrackingExtensions
    {
        // MaxWorkItemBatchSize reduced from default value of 200 due to incompatible behavior with TFS
        // The work item query string generated for 200 work items would be too long and failed processing
        private const int MaxWorkItemBatchSize = 100;

        public static async Task<WorkItemQueryExpandedResult> QueryAsync(this WorkItemTrackingHttpClient client, WorkItemQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            query.Validate();

            QueryHierarchyItem queryHierarchyItem = null;
            WorkItemQueryResult queryResult = null;
            TeamContext teamContext = new TeamContext(query.ProjectName, query.TeamName);

            if (query.QueryPath != null)
            {
                if (query.ProjectId != Guid.Empty)
                {
                    queryHierarchyItem = await client.GetQueryAsync(query.ProjectId, query.QueryPath, QueryExpand.All, cancellationToken: cancellationToken);
                }
                else
                {
                    queryHierarchyItem = await client.GetQueryAsync(query.ProjectName, query.QueryPath, QueryExpand.All, cancellationToken: cancellationToken);
                }

                queryResult = await client.QueryByIdAsync(teamContext, queryHierarchyItem.Id);
            }
            else if (query.QueryId != Guid.Empty)
            {
                queryResult = await client.QueryByIdAsync(teamContext, query.QueryId);
            }
            else
            {
                queryResult = await client.QueryByWiqlAsync(new Wiql { Query = query.Wiql }, teamContext, cancellationToken: cancellationToken);
            }

            var requiredFields = queryResult.Columns.Select(c => c.ReferenceName).ToList();

            if (query.RequiredFields != null)
            {
                foreach (var field in query.RequiredFields)
                {
                    if (!requiredFields.Contains(field))
                    {
                        requiredFields.Add(field);
                    }
                }
            }

            if (queryResult.QueryResultType == QueryResultType.WorkItem)
            {
                var ids = queryResult.WorkItems.Select(wi => wi.Id).ToList();

                if (query.MaxItemsToFetch != null)
                {
                    // If maximum number of items to fetch was specified, limit to that number
                    int maxItemsToFetch = query.MaxItemsToFetch.Value;
                    if (ids.Count > maxItemsToFetch)
                    {
                        ids.RemoveRange(maxItemsToFetch, ids.Count - maxItemsToFetch);
                    }
                }

                var workItems = await client.GetWorkItemsInBatchesAsync(ids, requiredFields, query.AsOf, query.Expand, cancellationToken);
                return new WorkItemQueryExpandedResult
                {
                    Query = query,
                    QueryHierarchyItem = queryHierarchyItem,
                    QueryResult = queryResult,
                    WorkItems = workItems
                };
            }
            else
            {
                var workItemRelations = queryResult.WorkItemRelations.ToList();
                ICollection<WorkItem> workItems;
                var linkIds = workItemRelations.Select(r => r.Target.Id).Distinct().ToArray();
                if (linkIds.Any())
                {
                    workItems = await client.GetWorkItemsInBatchesAsync(linkIds, requiredFields, queryResult.AsOf, query.Expand, cancellationToken);
                }
                else
                {
                    workItems = new WorkItem[0];
                }

                var hierarchy = WorkItemHierarchy.Create(workItems, workItemRelations, queryResult.QueryType == QueryType.Tree);

                return new WorkItemQueryExpandedResult
                {
                    Query = query,
                    QueryHierarchyItem = queryHierarchyItem,
                    QueryResult = queryResult,
                    WorkItemHierarchy = hierarchy,
                    WorkItems = hierarchy.AllWorkItems
                };
            }
        }

        public static async Task<List<WorkItem>> GetWorkItemsInBatchesAsync(this WorkItemTrackingHttpClient client, ICollection<int> ids, ICollection<string> fields, DateTime? asOf = null, WorkItemExpand? expand = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!ids.Any())
            {
                return new List<WorkItem>();
            }
            else if (ids.Count <= MaxWorkItemBatchSize)
            {
                return await client.GetWorkItemsAsync(ids, fields, asOf, expand, cancellationToken: cancellationToken);
            }

            // Need to batch in sets that don't exceed the maximum batch size
            List<WorkItem> all = new List<WorkItem>();

            IEnumerable<int> remainingIds = ids;

            do
            {
                IEnumerable<int> nextBatchOfIds = remainingIds.Take(MaxWorkItemBatchSize);
                remainingIds = remainingIds.Skip(MaxWorkItemBatchSize);
                if (nextBatchOfIds.Any())
                {
                    var batch = await client.GetWorkItemsAsync(nextBatchOfIds, fields, asOf, expand, cancellationToken: cancellationToken);
                    all.AddRange(batch);
                }
            }
            while (remainingIds.Any());

            return all;
        }

        public static async Task<List<WorkItem>> BatchUpdateWorkItemsAsync(this WorkItemTrackingBatchHttpClient client, ICollection<WorkItemBatchUpdateRequest> requests, object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            if (!requests.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(requests));
            }

            var headers = new Dictionary<string, string>();
            headers.Add("Content-type", "application/json-patch+json");

            var batchRequests = requests.Select(r => new WitBatchRequest
            {
                Method = "PATCH",
                Uri = $"/_apis/wit/workItems/{r.Id}?api-version=1.0",
                Headers = headers,
                Body = JsonConvert.SerializeObject(r.Body)
            });

            var okStatus = 200;
            var responses = await client.ExecuteBatchRequest(batchRequests, userState, cancellationToken).ConfigureAwait(false);
            var updatedWorkItems = responses.Where(r => r.Code == okStatus).Select(r => JsonConvert.DeserializeObject<WorkItem>(r.Body)).ToList();

            if (responses.Any(r => r.Code != okStatus))
            {
                var failures = responses.Where(r => r.Code != okStatus);
                var wrappedErrors = failures.Select(r => JsonConvert.DeserializeObject<WorkItemBatchUpdateErrorWrapper>(r.Body));
                var errors = wrappedErrors.Select(w => w.Value).ToList();
                throw new WorkItemBatchUpdateException(errors, updatedWorkItems);
            }

            return updatedWorkItems;
        }
    }

    public class WorkItemBatchUpdateRequest
    {
        public int Id { get; set; }

        public JsonPatchDocument Body { get; set; }
    }

    [Serializable]
    public class WorkItemBatchUpdateException : Exception
    {
        public WorkItemBatchUpdateException(ICollection<WorkItemBatchUpdateError> errors, ICollection<WorkItem> successfulUpdates)
            : base("One or more errors occurred attempting to update work items.")
        {
            this.Errors = errors;
            this.SuccessfulUpdates = successfulUpdates;
        }

        public ICollection<WorkItemBatchUpdateError> Errors { get; private set; }

        public ICollection<WorkItem> SuccessfulUpdates { get; private set; }
    }

    [DataContract]
    public class WorkItemBatchUpdateErrorWrapper
    {
        [DataMember(EmitDefaultValue = false)]
        public WorkItemBatchUpdateError Value { get; set; }
    }

    [DataContract]
    public class WorkItemBatchUpdateError
    {
        [DataMember(EmitDefaultValue = false)]
        public string Message { get; set; }
    }
}
