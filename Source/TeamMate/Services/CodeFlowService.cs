// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Platform.CodeFlow;
using System;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Services
{
    public class CodeFlowService
    {
        private CodeFlowClient codeFlowClient;
        private Lazy<Task<CodeFlowClient>> GetCodeFlowClientTask;

        public CodeFlowService()
        {
            this.GetCodeFlowClientTask = new Lazy<Task<CodeFlowClient>>(DoGetCodeFlowClientAsync);
        }

        public Task<CodeFlowClient> GetCodeFlowClientAsync()
        {
            return GetCodeFlowClientTask.Value;
        }

        private async Task<CodeFlowClient> DoGetCodeFlowClientAsync()
        {
            if (this.codeFlowClient == null)
            {
                CodeFlowClient client = new CodeFlowClient();

                try
                {
                    await client.DiscoverAsync();
                }
                finally
                {
                    this.codeFlowClient = client;
                }
            }

            return this.codeFlowClient;
        }
    }
}
