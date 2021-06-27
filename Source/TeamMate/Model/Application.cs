// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.ViewModels;
using SimpleInjector;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Model
{
    public class Application
    {
        private Container container;

        private void InitializeContainer()
        {
            this.container = new Container();
            ContainerConfiguration.Configure(this.container);

            ViewModelFactory.ViewModelCreator = (viewModelType) => this.container.GetInstance(viewModelType);
        }

        public async Task StartAsync()
        {
            InitializeContainer();

            await this.ApplicationService.StartAsync();
        }

        public void Shutdown()
        {
            this.ApplicationService.Shutdown();

            if (this.container != null)
            {
                this.container.Dispose();
                this.container = null;
            }
        }

        private ApplicationService ApplicationService => this.container.GetInstance<ApplicationService>();
    }
}
