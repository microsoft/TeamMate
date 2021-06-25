using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SystemValidation = System.Windows.Controls.Validation;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows
{
    public static class LogicalTreeUtilities
    {
        [Conditional("DEBUG")]
        public static void DebugLogicalTree(FrameworkElement e)
        {
            DebugLogicalTree(e, 0);
        }

        private static void DebugLogicalTree(FrameworkElement e, int level)
        {
            if (level == 0)
            {
                Debug.WriteLine("");
                Debug.WriteLine("LOGICAL");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(' ', level * 2);
            sb.AppendFormat("{0} ({1})", e.Name, e.GetType().FullName);

            Debug.WriteLine(sb.ToString());
            var children = LogicalTreeHelper.GetChildren(e);
            foreach (var child in children.OfType<FrameworkElement>())
            {
                DebugLogicalTree(child, level + 1);
            }
        }

        public static IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            Queue<DependencyObject> items = new Queue<DependencyObject>();
            items.Enqueue(root);

            while (items.Any())
            {
                DependencyObject item = items.Dequeue();
                foreach (DependencyObject child in LogicalTreeHelper.GetChildren(item).OfType<DependencyObject>())
                {
                    items.Enqueue(child);
                }

                if (item != root)
                    yield return item;
            }
        }

        public static IEnumerable<DependencyObject> DescendantsDepthFirst(DependencyObject root)
        {
            // We'll use this list as a stack, but it will allow us to insert many items at once
            List<DependencyObject> items = new List<DependencyObject>();
            items.Add(root);

            while (items.Any())
            {
                DependencyObject item = items[0];
                items.RemoveAt(0);

                var children = LogicalTreeHelper.GetChildren(item).OfType<DependencyObject>();
                items.InsertRange(0, children);

                if (item != root)
                    yield return item;
            }
        }

        public static bool IsHierarchyValid(DependencyObject root)
        {
            return GetFirstInvalidElement(root) != null;
        }

        public static DependencyObject GetFirstInvalidElement(DependencyObject root)
        {
            return DescendantsDepthFirst(root).FirstOrDefault(element => SystemValidation.GetHasError(element));
        }

        public static void FocusFirstInvalidElement(DependencyObject root)
        {
            FrameworkElement item = GetFirstInvalidElement(root) as FrameworkElement;
            if (item != null)
            {
                Keyboard.Focus(item);
                if (item is TextBox)
                {
                    ((TextBox)item).SelectAll();
                }
            }
        }
    }
}
