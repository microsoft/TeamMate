using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for TileCollectionView.xaml
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class TileCollectionView : UserControl
    {
        static TileCollectionView()
        {
            TileTemplateSelector = new TileTemplateSelectorImpl();
        }

        public TileCollectionView()
        {
            InitializeComponent();
            View.Initialize(this);
        }

        public static DataTemplateSelector TileTemplateSelector { get; private set; }

        private class TileTemplateSelectorImpl : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                string templateName = "WorkItemQueryTileTemplate";

                // TODO: Need to handle built in types?

                TileViewModel vm = item as TileViewModel;
                if (vm != null && vm.TileInfo != null && vm.TileInfo.Type == TileType.PullRequestQuery)
                {
                    templateName = "PullRequestQueryTileTemplate";
                }

                return ((FrameworkElement)container).FindResource<DataTemplate>(templateName);
            }
        }
    }
}
