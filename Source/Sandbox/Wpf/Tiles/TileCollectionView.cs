using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf.Tiles
{
    public class TileCollectionView : Control
    {
        static TileCollectionView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TileCollectionView), new FrameworkPropertyMetadata(typeof(TileCollectionView)));
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(TileCollectionView)
        );

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register(
            "ItemTemplateSelector", typeof(DataTemplateSelector), typeof(TileCollectionView)
        );

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }
    }
}
