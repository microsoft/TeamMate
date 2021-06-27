// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq.Expressions;

namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    public class PropertyValidationRuleBuilder<T>
    {
        public PropertyValidationRuleBuilder(Expression<Func<T>> propertyExpression, string displayName = null)
        {
            PropertyValidationRule<T> rule = new PropertyValidationRule<T>();
            rule.PropertyName = FluentUtilities.GetPropertyName(propertyExpression);
            rule.PropertyDisplayName = displayName ?? rule.PropertyName;
            rule.PropertyAccessor = FluentUtilities.Compile(propertyExpression);

            this.Rule = rule;
        }

        public PropertyValidationRule<T> Rule { get; private set; }



        /* Similar to Fluent's SetValidator, but better
        public ValidationRuleBuilder With(Validator validator)
        {
            return this;
        }
          
        for each element in a collection, Where clause to exclude some?
        public ValidationRuleBuilder WhereEach()
         */
    }
}
