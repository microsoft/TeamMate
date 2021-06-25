using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Validation
{
    public class Validator
    {
        private IList<IValidationRule> rules = new List<IValidationRule>();

        public IList<IValidationRule> Rules
        {
            get { return this.rules; }
        }

        public IEnumerable<IPropertyValidationRule> PropertyRules
        {
            get { return Rules.OfType<IPropertyValidationRule>(); }
        }

        public IEnumerable<string> ValidatedPropertyNames
        {
            get
            {
                return PropertyRules.Select(r => r.PropertyName).Distinct();
            }
        }

        public ValidationResult Validate()
        {
            List<ValidationFailure> failures = new List<ValidationFailure>();

            foreach (var globalRule in Rules.OfType<ValidationRule>())
            {
                ValidationResult result = globalRule.Validate();
                if (result != null && result.Failures.Any())
                {
                    failures.AddRange(result.Failures);

                    // DEFAULT MODE: DO not continue validation after the first failed result
                    break;
                }
            }

            return new ValidationResult(failures);
        }

        public ValidationResult Validate(string propertyName)
        {
            var rules = PropertyRules.Where(r => r.PropertyName == propertyName);
            return new ValidationResult(rules.SelectMany(r => r.Validate().Failures));
        }

        public void Rule(Func<bool> validator, string message)
        {
            rules.Add(new ValidationRule(validator, message));
        }

        public PropertyValidationRuleBuilder<T> RuleForProperty<T>(Expression<Func<T>> propertyAccessor, string displayName = null)
        {
            PropertyValidationRuleBuilder<T> builder = new PropertyValidationRuleBuilder<T>(propertyAccessor, displayName);
            this.rules.Add(builder.Rule);
            return builder;
        }
    }
}
