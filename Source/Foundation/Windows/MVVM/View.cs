// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.MVVM
{
    public static class View
    {
        public static void OnViewModelChanged<T>(FrameworkElement element, ViewModelChangedHandler<T> viewModelChangedHandler) where T : class
        {
            element.DataContextChanged += delegate (object sender, DependencyPropertyChangedEventArgs e)
            {
                var oldModel = e.OldValue as T;
                var newModel = e.NewValue as T;
                viewModelChangedHandler(oldModel, newModel);
            };
        }

        private static readonly DependencyProperty HasUnregisterOnUnloadProperty = DependencyProperty.RegisterAttached(
            "HasUnregisterOnUnload", typeof(bool), typeof(View)
        );

        private static void SetHasUnregisterOnUnload(DependencyObject element, bool value)
        {
            element.SetValue(HasUnregisterOnUnloadProperty, value);
        }

        private static bool GetHasUnregisterOnUnload(DependencyObject element)
        {
            return (bool)element.GetValue(HasUnregisterOnUnloadProperty);
        }

        private static readonly DependencyProperty ViewModelProperty = DependencyProperty.RegisterAttached(
            "ViewModel", typeof(ViewModelBase), typeof(View), new PropertyMetadata(OnViewModelPropertyChanged)
        );

        private static void SetViewModel(DependencyObject element, ViewModelBase value)
        {
            element.SetValue(ViewModelProperty, value);
        }

        private static ViewModelBase GetViewModel(DependencyObject element)
        {
            return (ViewModelBase)element.GetValue(ViewModelProperty);
        }

        private static ICollection<FrameworkElement> views = new List<FrameworkElement>();

        private static void OnViewModelPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            ViewModelBase oldViewModel = args.OldValue as ViewModelBase;
            ViewModelBase newViewModel = args.NewValue as ViewModelBase;

            FrameworkElement element = source as FrameworkElement;

            if (element != null && !element.IsInDesignMode())
            {
                if (newViewModel != null)
                {
                    ICommandProvider commandProvider = newViewModel as ICommandProvider;
                    if (commandProvider != null)
                    {
                        // TODO: We NEED an unregister at some point.
                        // TODO: This might be subpar. If a datacontext is set at the top and affects many views
                        // underneath, this is called for "every" item that this is a data context for?
                        commandProvider.RegisterBindings(element.CommandBindings);
                    }
                }

                if (element.IsLoaded)
                {
                    TryUnregisterGlobalCommands(element, oldViewModel);
                    TryRegisterGlobalCommands(element, newViewModel);
                }
            }
        }

        public static FrameworkElement GetView(ViewModelBase viewModel)
        {
            return views.FirstOrDefault(v => GetViewModel(v) == viewModel);
        }

        public static Window GetWindow(ViewModelBase viewModel)
        {
            // TODO: Consider walking up the parent stack first, and then picking the window. Might be more efficient,
            // less views to register. Doesn't work if you reuse viewmodels across viewmodel trees.
            FrameworkElement view = (viewModel != null) ? GetView(viewModel) : null;
            return (view != null) ? Window.GetWindow(view) : null;
        }

        public static void Initialize(FrameworkElement e)
        {
            e.SetBinding(ViewModelProperty, new Binding());
            e.Loaded += HandleViewLoaded;
            e.Unloaded += HandleViewUnloaded;
        }

        private static void HandleViewLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement view = (FrameworkElement)sender;
            views.Add(view);

            var viewModel = GetViewModel(view);
            TryRegisterGlobalCommands(view, viewModel);
        }

        private static void HandleViewUnloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement view = (FrameworkElement)sender;
            views.Remove(view);

            // IMPORTANT: TryUnregisterGlobalCommands() will not work here since we can't get the parent
            // window anymore. Unregistering global commands on unload is handled in TryRegisterGlobalCommands itself.
        }

        private static void TryUnregisterGlobalCommands(FrameworkElement view, ViewModelBase viewModel)
        {
            var globalCommandProvider = viewModel as IGlobalCommandProvider;
            if (globalCommandProvider != null && globalCommandProvider.GlobalCommandBindings != null)
            {
                var window = Window.GetWindow(view);
                if (window != null)
                {
                    UnregisterGlobalCommands(window, globalCommandProvider);
                }
            }
        }

        private static void TryRegisterGlobalCommands(FrameworkElement view, ViewModelBase viewModel)
        {
            var globalCommandProvider = viewModel as IGlobalCommandProvider;
            if (globalCommandProvider != null && globalCommandProvider.GlobalCommandBindings != null)
            {
                var window = Window.GetWindow(view);
                if (window != null)
                {
                    var bindings = globalCommandProvider.GlobalCommandBindings;
                    window.CommandBindings.AddRange(bindings);
                    UnregisterOnUnload(view, window);
                }
            }
        }

        private static void UnregisterOnUnload(FrameworkElement view, Window window)
        {
            if (!GetHasUnregisterOnUnload(view))
            {
                RoutedEventHandler unloaded = null;
                unloaded = delegate (object sender, RoutedEventArgs e)
                {
                    var currentProvider = GetViewModel(view) as IGlobalCommandProvider;
                    if (currentProvider != null && currentProvider.GlobalCommandBindings != null)
                    {
                        UnregisterGlobalCommands(window, currentProvider);
                    }

                    view.Unloaded -= unloaded;
                    SetHasUnregisterOnUnload(view, false);
                };

                view.Unloaded += unloaded;
                SetHasUnregisterOnUnload(view, true);
            }
        }

        private static void UnregisterGlobalCommands(Window window, IGlobalCommandProvider globalCommandProvider)
        {
            window.CommandBindings.RemoveRange(globalCommandProvider.GlobalCommandBindings);
        }
    }

    public delegate void ViewModelChangedHandler<T>(T oldModel, T newModel);
}
