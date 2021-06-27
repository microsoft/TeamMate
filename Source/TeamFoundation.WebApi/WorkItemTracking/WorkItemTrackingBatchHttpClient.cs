// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    [ResourceArea(WitConstants.WorkItemTrackingWebConstants.RestAreaId)]
    public class WorkItemTrackingBatchHttpClient : VssHttpClientBase
    {
        // TODO: Get rid of this and migrate to latest public Nuget once it supports Batch request APIs

        public WorkItemTrackingBatchHttpClient(Uri baseUrl, VssCredentials credentials)
            : base(baseUrl, credentials) { }

        public WorkItemTrackingBatchHttpClient(Uri baseUrl, VssCredentials credentials, params DelegatingHandler[] handlers)
            : base(baseUrl, credentials, handlers) { }

        public WorkItemTrackingBatchHttpClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings)
            : base(baseUrl, credentials, settings) { }

        public WorkItemTrackingBatchHttpClient(Uri baseUrl, HttpMessageHandler pipeline, bool disposeHandler)
            : base(baseUrl, pipeline, disposeHandler) { }

        public WorkItemTrackingBatchHttpClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings, params DelegatingHandler[] handlers)
            : base(baseUrl, credentials, settings, handlers) { }

        public async Task<List<WitBatchResponse>> ExecuteBatchRequest(IEnumerable<WitBatchRequest> requests, object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            if (!requests.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(requests));
            }

            var httpMethod = HttpMethod.Post;
            var apiVersion = new ApiResourceVersion("4.0-preview.2");

            // Batch route does not support location id and is not versioned.
            // So we are composing the location ourselves here.
            var location = new ApiResourceLocation();
            location.Area = "wit";
            location.ResourceName = "batch";
            location.RouteTemplate = "_apis/{area}/${resource}";
            location.MinVersion = apiVersion.ApiVersion;
            location.MaxVersion = apiVersion.ApiVersion;
            location.ReleasedVersion = apiVersion.ApiVersion;

            HttpContent content = new ObjectContent<List<WitBatchRequest>>(requests.ToList(), new VssJsonMediaTypeFormatter(true));
            using (HttpRequestMessage requestMessage = CreateRequestMessage(httpMethod, location, null, apiVersion, content))
            {
                return await SendAsync<List<WitBatchResponse>>(requestMessage, userState, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
