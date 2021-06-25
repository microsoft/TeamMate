using System;

namespace Microsoft.Internal.Tools.TeamMate.ViewModels
{
    public static class ViewModelFactory
    {
        public static T Create<T>()
        {
            if (ViewModelCreator != null)
            {
                return (T)ViewModelCreator(typeof(T));
            }

            return Activator.CreateInstance<T>();
        }

        public static ViewModelCreator ViewModelCreator { get; set; }
    }

    public delegate object ViewModelCreator(Type viewModelType);
}
