using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Media
{
    public static class VisualTreeUtilities
    {
        [Conditional("DEBUG")]
        public static void DebugVisualTree(FrameworkElement e)
        {
            DebugVisualTree(e, 0);
        }

        private static void DebugVisualTree(FrameworkElement e, int level)
        {
            if (level == 0)
            {
                Debug.WriteLine("");
                Debug.WriteLine("VISUAL");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(' ', level * 2);
            sb.AppendFormat("{0} ({1})", e.Name, e.GetType().FullName);

            Debug.WriteLine(sb.ToString());
            int childCount = VisualTreeHelper.GetChildrenCount(e);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(e, i) as FrameworkElement;
                if (child != null)
                {
                    DebugVisualTree(child, level + 1);
                }
            }
        }

        /*
         * NOT TESTED
        public static IEnumerable<DependencyObject> AncestorsUntil(DependencyObject source, Predicate<DependencyObject> predicate)
        {
            DependencyObject parent = TryGetParent(source);
            while (parent != null && predicate(parent))
            {
                yield return parent;
                parent = TryGetParent(parent);
            }
        }
         */

        public static IEnumerable<DependencyObject> GetChildren(DependencyObject reference)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(reference);
            for (int i = 0; i < childCount; i++)
                yield return VisualTreeHelper.GetChild(reference, i);
        }

        public static IEnumerable<T> FirstDescendantsOfType<T>(DependencyObject root) where T : DependencyObject
        {
            Queue<DependencyObject> items = new Queue<DependencyObject>();
            items.Enqueue(root);

            while (items.Any())
            {
                DependencyObject item = items.Dequeue();
                foreach (var child in GetChildren(item))
                {
                    T result = child as T;
                    if (result != null)
                    {
                        yield return result;
                    }
                    else
                    {
                        // Keep on searching
                        items.Enqueue(child);
                    }
                }
            }
        }

        public static IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            Queue<DependencyObject> items = new Queue<DependencyObject>();
            items.Enqueue(root);

            while (items.Any())
            {
                DependencyObject item = items.Dequeue();
                foreach (var child in GetChildren(item))
                {
                    items.Enqueue(child);
                }

                if (item != root)
                    yield return item;
            }
        }

        public static bool IsAncestor(DependencyObject ancestor, DependencyObject reference)
        {
            DependencyObject parent = TryGetParent(reference);
            while (parent != null)
            {
                if (parent == ancestor)
                    return true;

                parent = TryGetParent(parent);
            }

            return false;
        }

        public static bool IsAncestorOrSelf(DependencyObject ancestor, DependencyObject reference)
        {
            return ancestor == reference || IsAncestor(ancestor, reference);
        }

        public static T TryFindAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject parent = reference;

            do
            {
                parent = TryGetParent(parent);
            }
            while (parent != null && !(parent is T));

            return parent as T;
        }

        public static DependencyObject TryGetParent(DependencyObject item)
        {
            if (item is Visual || item is Visual3D)
            {
                return VisualTreeHelper.GetParent(item);
            }
            else if (item is FrameworkContentElement)
            {
                return ((FrameworkContentElement)item).Parent;
            }
            else if (item is ContentElement)
            {
                return ContentOperations.GetParent((ContentElement)item);
            }

            return null;
        }

        public static T TryFindAncestorOrSelf<T>(DependencyObject reference) where T : DependencyObject
        {
            return (reference is T) ? (T)reference : TryFindAncestor<T>(reference);
        }

        public static T FindAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            T ancestor = TryFindAncestor<T>(reference);

            if (ancestor == null)
                throw new Exception("Could not find ancestor");

            return ancestor;
        }

        public static T FindAncestorOrSelf<T>(DependencyObject reference) where T : DependencyObject
        {
            T ancestor = TryFindAncestorOrSelf<T>(reference);

            if (ancestor == null)
                throw new Exception("Could not find ancestor");

            return ancestor;
        }

        public static T GetListBoxItemAt<T>(RoutedEventArgs e)
        {
            return GetItemAt<ListBox, ListBoxItem, T>(e);
        }

        public static T GetListViewItemAt<T>(RoutedEventArgs e)
        {
            return GetItemAt<ListView, ListViewItem, T>(e);
        }

        public static T GetTreeViewItemAt<T>(RoutedEventArgs e)
        {
            return GetItemAt<TreeView, TreeViewItem, T>(e);
        }

        public static TContainerItem GetItemAt<TContainerItem>(RoutedEventArgs e)
            where TContainerItem : FrameworkElement
        {
            return TryFindAncestorOrSelf<TContainerItem>((DependencyObject)e.OriginalSource);
        }

        private static TItem GetItemAt<TContainer, TContainerItem, TItem>(RoutedEventArgs e)
            where TContainer : ItemsControl
            where TContainerItem : FrameworkElement
        {
            TContainerItem containerItem = GetItemAt<TContainerItem>(e);
            if (containerItem != null)
            {
                TContainer container = TryFindAncestor<TContainer>(containerItem);
                if (container != null)
                {
                    object item = container.ItemContainerGenerator.ItemFromContainer(containerItem);
                    if (item is TItem)
                        return (TItem)item;
                }
            }

            return default(TItem);
        }

        public static bool TryGetItemContainers(UIElement uiElement, out ItemsControl itemsControl, out DependencyObject itemContainer)
        {
            itemContainer = null;
            itemsControl = TryFindAncestor<ItemsControl>(uiElement);
            if (itemsControl != null)
            {
                itemContainer = itemsControl.ContainerFromElement(uiElement);
            }

            return itemsControl != null && itemContainer != null;
        }
    }
}
