// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    // Based on ideas from:
    // http://mvvmvalidation.codeplex.com/
    // http://fluentvalidation.codeplex.com/
    // http://csharperimage.jeremylikness.com/2011/07/reflection-lambda-and-expression-magic.html
    // http://dotnetsilverlightprism.wordpress.com/2013/01/15/inotifydataerrorinfo-and-dataannotations-validation-for-wpf-3-useful-sources/
    // http://mark.mymonster.nl/2011/02/22/validating-our-viewmodel

    public class ValidationContext : ObservableObjectBase, INotifyDataErrorInfo
    {
        private static readonly string[] NoErrors = new string[0];

        private IDictionary<string, IList<string>> errors = new Dictionary<string, IList<string>>();
        private bool hasErrors;
        private bool isValid;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public ValidationContext()
        {
            this.Validator = new Validator();
            InvalidateHasErrors();
        }

        private static string Normalize(string propertyName)
        {
            return propertyName ?? String.Empty;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            propertyName = Normalize(propertyName);

            IList<string> result;
            if (!errors.TryGetValue(propertyName, out result))
            {
                result = NoErrors;
            }

            return result;
        }

        public bool HasErrors
        {
            get { return this.hasErrors; }
            private set { SetProperty(ref this.hasErrors, value); }
        }

        public bool IsValid
        {
            get { return this.isValid; }
            private set { SetProperty(ref this.isValid, value); }
        }

        public void AddError(string propertyName, string error)
        {
            propertyName = Normalize(propertyName);

            IList<string> result;
            if (!errors.TryGetValue(propertyName, out result))
            {
                result = new List<string>();
                errors[propertyName] = result;
            }

            result.Add(error);

            InvalidatePropertyErrors(propertyName);
        }

        private void InvalidatePropertyErrors(string propertyName)
        {
            InvalidateHasErrors();
            OnErrorsChanged(propertyName);
        }

        public void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            propertyName = Normalize(propertyName);
            this.errors[propertyName] = errors.ToList();

            InvalidatePropertyErrors(propertyName);
        }

        public void ClearErrors(string propertyName)
        {
            propertyName = Normalize(propertyName);
            if (errors.Remove(propertyName))
            {
                InvalidatePropertyErrors(propertyName);
            }
        }

        public void ClearErrors()
        {
            var propertyNames = errors.Keys.ToArray();
            if (propertyNames.Any())
            {
                errors.Clear();
                InvalidateHasErrors();
                foreach (var propertyName in propertyNames)
                {
                    OnErrorsChanged(propertyName);
                }
            }
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void InvalidateHasErrors()
        {
            this.HasErrors = errors.Any();
            this.IsValid = !this.HasErrors;
        }

        public Validator Validator { get; private set; }

        public bool Validate()
        {
            var results = Validator.Validate();
            SetResults(String.Empty, results);

            var propertyNames = Validator.ValidatedPropertyNames;
            foreach (var propertyName in propertyNames)
            {
                Validate(propertyName);
            }

            return IsValid;
        }

        public bool Validate(string propertyName)
        {
            var results = Validator.Validate(propertyName);
            return SetResults(propertyName, results);
        }

        private bool SetResults(string propertyName, ValidationResult results)
        {
            bool isValid = !results.Failures.Any();

            if (isValid)
            {
                ClearErrors(propertyName);
            }
            else
            {
                SetErrors(propertyName, results.Failures.Select(f => f.Error));
            }

            return isValid;
        }
    }
}
