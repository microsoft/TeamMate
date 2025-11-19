using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.ViewModels;
using SimpleInjector;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Model
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class Application
    {
        private Container container;

        private void InitializeContainer()
        {
            this.container = new Container();

            // With SimpleInjector v5, the behavior changed to resolve unregistered concrete types is disallowed by default
            // (See https://simpleinjector.org/ructd). For now, we will restore old behavior.
            container.Options.ResolveUnregisteredConcreteTypes = true;
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
