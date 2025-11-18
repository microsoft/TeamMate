using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using System.Runtime.Versioning;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Services
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ViewService
    {
        private ViewCatalog viewCatalog = new ViewCatalog();

        public void Initialize()
        {
            viewCatalog.RegisterViewsInAssembly(GetType().Assembly);
        }

        public FrameworkElement CreateView(object viewModel)
        {
            return viewCatalog.CreateView(viewModel);
        }
    }
}
