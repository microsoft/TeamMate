using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Input
{
    public abstract class RoutedCommandBase : RoutedCommand, INotifyPropertyChanged
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        // KLUDGE: Given that we have to inherit from RoutedCommand, we cannot inherit from ObservableObjectBase
        // I just bluntly copy/pasted a little bit of code for notifying on property changes...
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises PropertyChanged event. This method can only be called on the thread associated with this object's dispatcher.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <exception cref="System.InvalidOperationException">The calling thread does not have access to this object.</exception>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            Assert.ParamIsNotNull(propertyName, "propertyName");

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// A helper method that sets property value and raises PropertyChanged event if the value has changed.
        /// </summary>
        /// <param name="propertyDataField">A reference to the data member which is used to store property value.</param>
        /// <param name="value">New property value.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property value has changed, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected bool SetProperty<T>(ref T propertyDataField, T value, [CallerMemberName] string propertyName = null)
        {
            Assert.ParamIsNotNullOrEmpty(propertyName, "propertyName");

            if (!Object.Equals(propertyDataField, value))
            {
                propertyDataField = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// A helper method that sets property value and raises PropertyChanged event if the value has changed.
        /// Optimized implementation for string type.
        /// </summary>
        /// <param name="field">A reference to the data member which is used to store property value.</param>
        /// <param name="value">New property value.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property value has changed, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected bool SetProperty(ref string field, string value, [CallerMemberName] string propertyName = null)
        {
            Assert.ParamIsNotNullOrEmpty(propertyName, "propertyName");

            if (!string.Equals(field, value, StringComparison.Ordinal))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// A helper method that sets property value and raises PropertyChanged event if the value has changed.
        /// Optimized implementation for System.Int32 type.
        /// </summary>
        /// <param name="field">A reference to the data member which is used to store property value.</param>
        /// <param name="value">New property value.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property value has changed, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected bool SetProperty(ref int field, int value, [CallerMemberName] string propertyName = null)
        {
            Assert.ParamIsNotNullOrEmpty(propertyName, "propertyName");

            if (field != value)
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// A helper method that sets property value and raises PropertyChanged event if the value has changed.
        /// Optimized implementation for System.Boolean type.
        /// </summary>
        /// <param name="field">A reference to the data member which is used to store property value.</param>
        /// <param name="value">New property value.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property value has changed, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected bool SetProperty(ref bool field, bool value, [CallerMemberName] string propertyName = null)
        {
            Assert.ParamIsNotNullOrEmpty(propertyName, "propertyName");

            if (field != value)
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }
    }
}
