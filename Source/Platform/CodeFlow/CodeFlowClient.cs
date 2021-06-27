// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Platform.CodeFlow.Dashboard;
using Microsoft.Tools.TeamMate.Platform.CodeFlow.Discovery;
using Microsoft.Tools.TeamMate.Platform.CodeFlow.Project;
using Microsoft.Tools.TeamMate.Platform.CodeFlow.Review;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Platform.CodeFlow
{
    /// <summary>
    /// A Facade over all of CodeFlows service clients.
    /// </summary>
    public class CodeFlowClient
    {
        private ReviewServiceClient reviewServiceClient;
        private ReviewDashboardServiceClient reviewDashboardServiceClient;
        private DiscoveryServiceClient discoveryServiceClient;
        private ProjectServiceClient projectServiceClient;

        private const string DiscoveryServiceEndpointConfiguration = "WSHttpBinding_IDiscoveryService";
        private const string ProjectServiceEndpointConfiguration = "WSHttpBinding_IProjectService";
        private const string ReviewServiceEndpointConfiguration = "WSHttpBinding_IReviewService";
        private const string ReviewDashboardServiceEndpointConfiguration = "BasicHttpBinding_IReviewDashboardService";

        private Discovery.Discovery discovery;
        private bool isDiscovered;
        private Exception discoveryException;
        private Lazy<Task> discoveryTask;

        public CodeFlowClient()
        {
            this.discoveryTask = new Lazy<Task>(DoDiscoverAsync);
        }

        public Task DiscoverAsync()
        {
            return discoveryTask.Value;
        }

        private async Task DoDiscoverAsync()
        {
            if (!isDiscovered)
            {
                try
                {
                    this.discovery = await this.DiscoveryServiceClient.DiscoverAsync();
                }
                catch (Exception e)
                {
                    this.discoveryException = e;
                }
                finally
                {
                    isDiscovered = true;
                }
            }

            if (this.discoveryException != null)
            {
                ThrowServiceUnavailableException();
            }
        }

        private Uri EnsureEndpointDiscovered(string endpointName)
        {
            if (!isDiscovered)
            {
                DiscoverAsync().Wait();
            }

            Uri endpointAddress = null;
            if(this.discovery == null || !this.discovery.Endpoints.TryGetValue(endpointName, out endpointAddress))
            {
                ThrowServiceUnavailableException();
            }

            return endpointAddress;
        }

        public ReviewServiceClient ReviewServiceClient
        {
            get 
            {
                if (this.reviewServiceClient == null)
                {
                    Uri uri = EnsureEndpointDiscovered(CodeFlowConstants.ReviewServiceUriEndpoint);
                    this.reviewServiceClient = new ReviewServiceClient(ReviewServiceEndpointConfiguration, CreateAddress(uri));
                }

                return this.reviewServiceClient; 
            }
        }

        public ReviewDashboardServiceClient ReviewDashboardServiceClient
        {
            get 
            {
                if (this.reviewDashboardServiceClient == null)
                {
                    Uri uri = EnsureEndpointDiscovered(CodeFlowConstants.ReviewDashboardServiceUriEndpoint);
                    this.reviewDashboardServiceClient = new ReviewDashboardServiceClient(ReviewDashboardServiceEndpointConfiguration, CreateAddress(uri));
                }

                return this.reviewDashboardServiceClient; 
            }
        }

        public DiscoveryServiceClient DiscoveryServiceClient
        {
            get 
            {
                if (this.discoveryServiceClient == null)
                {
                    this.discoveryServiceClient = new DiscoveryServiceClient(DiscoveryServiceEndpointConfiguration);
                }

                return this.discoveryServiceClient; 
            }
        }

        public ProjectServiceClient ProjectServiceClient
        {
            get 
            {
                if (this.projectServiceClient == null)
                {
                    Uri uri = EnsureEndpointDiscovered(CodeFlowConstants.ProjectServiceUriEndpoint);
                    this.projectServiceClient = new ProjectServiceClient(ProjectServiceEndpointConfiguration, CreateAddress(uri));
                }

                return this.projectServiceClient; 
            }
        }

        public Uri ClientUri
        {
            get
            {
                return EnsureEndpointDiscovered(CodeFlowConstants.ClientUriEndpoint);
            }
        }

        private EndpointAddress CreateAddress(Uri uri)
        {
            // KLUDGE: Creating a client with just a URI loses the <identity> settings that were configured in app.config.
            // We need those! The easiest way is to create a full address object here, this is equivalent to using:
            // <identity>
            //   <dns name="localhost" />
            // </identity>
            // in app.config.
            return  new EndpointAddress(uri, EndpointIdentity.CreateDnsIdentity("localhost"));
        }

        private void ThrowServiceUnavailableException()
        {
            throw new Exception("Could not connect to the CodeFlow service, please try again later.\nIf the problem persists, check http://codebox/codeflow for more information.", this.discoveryException);
        }
    }
}
