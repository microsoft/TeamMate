using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows
{
    public static class WpfExtensions
    {
        public static T FindResource<T>(this ResourceDictionary dictionary, object resourceKey)
        {
            object result = dictionary[resourceKey];
            if (result == null)
            {
                throw new ResourceReferenceKeyNotFoundException(String.Format("Resource with key {0} was not found", resourceKey), resourceKey);
            }

            AssertIsExpected<T>(resourceKey, result);
            return (T)result;
        }

        private static void AssertIsExpected<T>(object resourceKey, object result)
        {
            if (!(result is T))
                throw new Exception("Expected resource " + resourceKey + " of type " + typeof(T) + ", but found a resource of type " + result.GetType() + " instead.");
        }

        public static T FindResource<T>(this Application application, object resourceKey)
        {
            object result = application.FindResource(resourceKey);
            AssertIsExpected<T>(resourceKey, result);
            return (T)result;
        }

        public static T TryFindResource<T>(this Application application, object resourceKey)
        {
            object result = application.TryFindResource(resourceKey);
            return (result is T) ? (T)result : default(T);
        }

        public static T FindResource<T>(this FrameworkElement element, object resourceKey)
        {
            object result = element.FindResource(resourceKey);
            AssertIsExpected<T>(resourceKey, result);
            return (T)result;
        }

        public static T TryFindResource<T>(this FrameworkElement element, object resourceKey)
        {
            object result = element.TryFindResource(resourceKey);
            return (result is T) ? (T)result : default(T);
        }

        // Command Binding Extensions
        public static CommandBinding Add(this CommandBindingCollection collection, ICommand command,
            Action execute, Func<bool> canExecute = null)
        {
            ExecutedRoutedEventHandler executedHandler = null;
            CanExecuteRoutedEventHandler canExecuteHandler = null;

            if (execute != null)
            {
                executedHandler = delegate (object sender, ExecutedRoutedEventArgs e)
                {
                    execute();
                    e.Handled = true;
                };
            }

            if (canExecute != null)
            {
                canExecuteHandler = delegate (object sender, CanExecuteRoutedEventArgs e)
                {
                    e.CanExecute = canExecute();
                    e.Handled = true;
                };
            }

            return Add(collection, command, executedHandler, canExecuteHandler);
        }

        public static CommandBinding Add<T>(this CommandBindingCollection collection, ICommand command,
            Action<T> execute, Predicate<T> canExecute = null)
        {
            ExecutedRoutedEventHandler executedHandler = null;
            CanExecuteRoutedEventHandler canExecuteHandler = null;

            if (execute != null)
            {
                executedHandler = delegate (object sender, ExecutedRoutedEventArgs e)
                {
                    execute((T)e.Parameter);
                    e.Handled = true;
                };
            }

            if (canExecute != null)
            {
                canExecuteHandler = delegate (object sender, CanExecuteRoutedEventArgs e)
                {
                    e.CanExecute = canExecute((T)e.Parameter);
                    e.Handled = true;
                };
            }

            return Add(collection, command, executedHandler, canExecuteHandler);
        }

        public static CommandBinding Add(this CommandBindingCollection collection, ICommand command,
            ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute = null)
        {
            CommandBinding binding = new CommandBinding(command);

            if (executed != null)
            {
                binding.Executed += executed;
            }

            if (canExecute != null)
            {
                binding.CanExecute += canExecute;
            }

            collection.Add(binding);
            return binding;
        }

        public static void Remove(this CommandBindingCollection bindings, ICommand command)
        {
            var binding = bindings.OfType<CommandBinding>().FirstOrDefault(cb => cb.Command == command);
            if (binding != null)
            {
                bindings.Remove(binding);
            }
        }

        public static void RemoveRange(this CommandBindingCollection collection, System.Collections.ICollection bindings)
        {
            foreach (CommandBinding binding in bindings)
            {
                collection.Remove(binding);
            }
        }

        public static void Remove(this InputBindingCollection bindings, ICommand command)
        {
            var binding = bindings.OfType<InputBinding>().FirstOrDefault(cb => cb.Command == command);
            if (binding != null)
            {
                bindings.Remove(binding);
            }
        }

        public static IDisposable UseTemporaryWaitCursor(this FrameworkElement element)
        {
            return UseTemporaryCursor(element, Cursors.Wait);
        }

        public static IDisposable UseTemporaryCursor(this FrameworkElement element, Cursor cursor)
        {
            return new TemporaryCursorManager(element, cursor);
        }

        public static KeyGesture GetMainKeyGesture(this ICommand command)
        {
            RoutedCommand routedCommand = command as RoutedCommand;
            return (routedCommand != null) ? routedCommand.InputGestures.OfType<KeyGesture>().FirstOrDefault() : null;
        }

        public static Rect GetBounds(this Window w)
        {
            return new Rect(w.Left, w.Top, w.ActualWidth, w.ActualHeight);
        }

        public static bool PerformClick(this Button button)
        {
            if (button.IsEnabled)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(button);
                IInvokeProvider invokeProv = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
                invokeProv.Invoke();
                return true;
            }

            return false;
        }

        public static void WhenLoaded(this FrameworkElement element, Action action)
        {
            Assert.ParamIsNotNull(element, "element");

            if (element.IsLoaded)
            {
                action();
            }
            else
            {
                RoutedEventHandler whenLoaded = null;

                whenLoaded = delegate (object sender, RoutedEventArgs e)
                {
                    element.Loaded -= whenLoaded; ;
                    action();
                };

                element.Loaded += whenLoaded;
            }
        }

        public static void ExecuteOnce(this DispatcherTimer timer, TimeSpan interval, Action action)
        {
            Assert.ParamIsNotNull(timer, "timer");
            Assert.ParamIsNotNegative(interval, "interval");
            Assert.ParamIsNotNull(action, "action");

            EventHandler tick = null;
            tick = delegate (object sender, EventArgs e)
            {
                timer.Tick -= tick;
                action();
                timer.Stop();
            };

            timer.Tick += tick;
            timer.Interval = interval;
            timer.Start();
        }

        public static void InvokeHere(this Dispatcher dispatcher, Action action)
        {
            Assert.ParamIsNotNull(dispatcher, "dispatcher");

            if (Thread.CurrentThread == dispatcher.Thread)
            {
                action();
            }
            else
            {
                dispatcher.BeginInvoke(action);
            }
        }

        public static bool IsInDesignMode(this FrameworkElement element)
        {
            return DesignerProperties.GetIsInDesignMode(element);
        }


        public static bool SelectAndFocusFirstItem(this Selector listBox)
        {
            if (listBox.HasItems)
            {
                listBox.SelectedIndex = 0;

                var firstItem = listBox.ItemContainerGenerator.ContainerFromIndex(0) as IInputElement;
                if (firstItem != null)
                {
                    Keyboard.Focus(firstItem);
                }

                return true;
            }

            return false;
        }

        public static bool SelectAndFocusFirstItem(this TreeView treeView)
        {
            if (treeView.HasItems)
            {
                var firstItem = treeView.ItemContainerGenerator.ContainerFromIndex(0) as IInputElement;
                if (firstItem != null)
                {
                    var tvi = firstItem as TreeViewItem;
                    if (tvi != null)
                    {
                        tvi.IsSelected = true;
                    }

                    Keyboard.Focus(firstItem);
                }

                return true;
            }

            return false;
        }

        public static Window GetActiveWindow(this Application app)
        {
            return app.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        }
    }
}
