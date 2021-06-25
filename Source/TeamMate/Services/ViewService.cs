using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Services
{
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
