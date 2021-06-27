// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi
{
    public class ResourceLocationHttpClient : VssHttpClientBase
    {
        public ResourceLocationHttpClient(Uri baseUrl, VssCredentials credentials)
        : base(baseUrl, credentials)
        {
        }

        public ResourceLocationHttpClient(Uri baseUrl, VssCredentials credentials, params DelegatingHandler[] handlers)
        : base(baseUrl, credentials, handlers)
        {
        }

        public ResourceLocationHttpClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings)
        : base(baseUrl, credentials, settings)
        {
        }

        public ResourceLocationHttpClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings, params DelegatingHandler[] handlers)
        : base(baseUrl, credentials, settings, handlers)
        {
        }

        public ResourceLocationHttpClient(Uri baseUrl, HttpMessageHandler pipeline, bool disposeHandler)
        : base(baseUrl, pipeline, disposeHandler)
        {
        }

        /// <summary>
        /// Get information about an API resource location by its location id
        /// </summary>
        /// <param name="locationId">Id of the API resource location</param>
        /// <returns></returns>
        public new Task<ApiResourceLocation> GetResourceLocationAsync(
            Guid locationId,
            Object userState = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.GetResourceLocationAsync(locationId, userState, cancellationToken);
        }
    }
}
