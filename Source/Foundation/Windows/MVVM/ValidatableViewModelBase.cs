using Microsoft.Tools.TeamMate.Foundation.Validation;
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.MVVM
{
    public abstract class ValidatableViewModelBase : ViewModelBase, INotifyDataErrorInfo
    {
        private ValidationContext validationContext;

        public ValidatableViewModelBase()
        {
            this.validationContext = new ValidationContext();
            this.validationContext.PropertyChanged += HandlePropertyChanged;
        }

        protected Validator Validator
        {
            get { return this.ValidationContext.Validator; }
        }

        // TODO: Should this be public? Protected?
        public ValidationContext ValidationContext
        {
            get { return validationContext; }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
        {
            add { ValidationContext.ErrorsChanged += value; }
            remove { ValidationContext.ErrorsChanged -= value; }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return ValidationContext.GetErrors(propertyName);
        }

        public bool HasErrors
        {
            get { return ValidationContext.HasErrors; }
        }

        public bool IsValid
        {
            get { return ValidationContext.IsValid; }
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasErrors" || e.PropertyName == "IsValid")
            {
                OnPropertyChanged(e.PropertyName);
            }
        }

        public bool Validate()
        {
            return ValidationContext.Validate();
        }

        private bool Validate(string propertyName)
        {
            return ValidationContext.Validate(propertyName);
        }

        protected bool SetAndValidateProperty(ref string field, string value, [CallerMemberName] string propertyName = null)
        {
            bool changed = SetProperty(ref field, value, propertyName);
            if (changed)
            {
                Validate(propertyName);
            }

            return changed;
        }
    }
}
