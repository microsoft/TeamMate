using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    public class PropertyValidationRule<T> : IPropertyValidationRule, IPropertyValidatorContext
    {
        private ICollection<PropertyValidator<T>> validators = new List<PropertyValidator<T>>();

        public string PropertyName { get; set; }
        public string PropertyDisplayName { get; set; }
        public Func<T> PropertyAccessor { get; set; }

        public object GetPropertyValue()
        {
            return PropertyAccessor();
        }

        public ICollection<PropertyValidator<T>> Validators
        {
            get { return this.validators; }
        }

        public ValidationResult Validate()
        {
            IPropertyValidatorContext context = this;
            List<ValidationFailure> failures = new List<ValidationFailure>();

            foreach (var validator in Validators)
            {
                var results = validator.Validate(context);
                if (results != null && results.Any())
                {
                    failures.AddRange(results);

                    // DEFAULT MODE: DO not continue validation after the first failed result
                    break;
                }
            }

            return new ValidationResult(failures);
        }
    }
}
