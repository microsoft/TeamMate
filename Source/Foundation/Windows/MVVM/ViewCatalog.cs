using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.MVVM
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ViewCatalog
    {
        private IDictionary<Type, Type> viewMap = new Dictionary<Type, Type>();

        public void RegisterView<TView, TViewModel>()
        {
            RegisterView(typeof(TView), typeof(TViewModel));
        }

        public void RegisterView(Type viewType, Type viewModelType)
        {
            Assert.ParamIsNotNull(viewType, "viewType");
            Assert.ParamIsNotNull(viewModelType, "viewModelType");

            viewMap[viewModelType] = viewType;
        }

        public Type GetViewType<TViewModel>()
        {
            return GetViewType(typeof(TViewModel));
        }

        public Type GetViewType(Type viewModelType)
        {
            Assert.ParamIsNotNull(viewModelType, "viewModelType");

            Type result;
            if (!viewMap.TryGetValue(viewModelType, out result))
            {
                // Commented this out as it was annoying during design time...
                // Debug.Fail(String.Format("Could not resolve view type for view model of type {0}", viewModelType.FullName));
            }

            return result;
        }

        public void RegisterViewsInAssembly(Assembly assembly)
        {
            Assert.ParamIsNotNull(assembly, "assembly");

            var types = GetLoadableTypes(assembly);
            foreach (var type in types)
            {
                ViewAttribute viewAttribute = type.GetCustomAttribute<ViewAttribute>();
                if (viewAttribute != null)
                {
                    RegisterView(type, viewAttribute.ViewModelType);
                }
            }
        }

        public FrameworkElement CreateView(object viewModel)
        {
            Assert.ParamIsNotNull(viewModel, "viewModel");

            Type viewType = GetViewType(viewModel.GetType());
            if (viewType == null)
            {
                throw new NotSupportedException("No view registered for view model type: " + viewModel.GetType().FullName);
            }

            FrameworkElement view = (FrameworkElement)Activator.CreateInstance(viewType);
            view.DataContext = viewModel;
            return view;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException typeLoadException)
            {
                Log.Warn(typeLoadException.Message);
                foreach (var item in typeLoadException.LoaderExceptions)
                {
                    Log.Warn(typeLoadException.Message);
                }

                if (typeLoadException.Types == null || typeLoadException.Types.Length == 0)
                {
                    throw;
                }

                return typeLoadException.Types.Where(x => x != null);
            }
        }
    }
}
