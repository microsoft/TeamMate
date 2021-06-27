// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi
{
    public static class HttpClientExtensions
    {
        private const int MaxProjectsBatchSize = 100;

        public static async Task<ICollection<TeamProjectReference>> GetProjectsInBatchesAsync(this ProjectHttpClient projectClient, ProjectState? stateFilter = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            GetPageAsync<TeamProjectReference> getProjectsPage = (top, skip) => projectClient.GetProjects(stateFilter, top: top, skip: skip);
            var allProjects = await PagingUtilities.PageAllAsync(getProjectsPage, MaxProjectsBatchSize, cancellationToken);
            return allProjects;
        }
    }
}
