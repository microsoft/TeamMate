// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    public static class DefaultValidations
    {
        // Predicate
        public static PropertyValidationRuleBuilder<T> Is<T>(this PropertyValidationRuleBuilder<T> builder, Predicate<T> predicate, string message)
        {
            builder.Rule.Validators.Add(new PropertyValidator<T>(predicate, message));
            return builder;
        }

        // Objects

        public static PropertyValidationRuleBuilder<T> IsNotNull<T>(this PropertyValidationRuleBuilder<T> builder, string message = null) where T : class
        {
            Predicate<T> predicate = (x) => (x != null);
            var defaultMessage = "{PropertyName} cannot be null";

            return builder.Is(predicate, message ?? defaultMessage);
        }

        public static PropertyValidationRuleBuilder<T> IsNotEqual<T>(this PropertyValidationRuleBuilder<T> builder, T value, string message = null)
        {
            Predicate<T> predicate = (x) => !Object.Equals(x, value);
            var defaultMessage = "{PropertyName} cannot be equal to {PropertyValue}";

            return builder.Is(predicate, message ?? defaultMessage);
        }

        // Strings

        public static PropertyValidationRuleBuilder<string> IsNotEmpty(this PropertyValidationRuleBuilder<string> builder, string message = null)
        {
            Predicate<string> predicate = (x) => x != null && !String.IsNullOrEmpty(x.Trim());
            var defaultMessage = "{PropertyName} cannot be empty";

            return builder.Is(predicate, message ?? defaultMessage);
        }

        // TODO: HasLength? With a min and a max? HasMinLength?
        public static PropertyValidationRuleBuilder<string> HasMaxLength(this PropertyValidationRuleBuilder<string> builder, int length, string message = null)
        {
            Predicate<string> predicate = (x) => (x == null || x.Length < length);
            var defaultMessage = "{PropertyName} cannot be greater than " + length + " characters";

            return builder.Is(predicate, message ?? defaultMessage);
        }

        // TODO: IsEmailAddress

        public static PropertyValidationRuleBuilder<string> Matches(this PropertyValidationRuleBuilder<string> builder, Regex regex, string message)
        {
            Predicate<string> predicate = (x) => (x != null && regex.IsMatch(x));

            return builder.Is(predicate, message);
        }

        // Numbers

        public static PropertyValidationRuleBuilder<int> IsLessThan(this PropertyValidationRuleBuilder<int> builder, int value, string message = null)
        {
            Predicate<int> predicate = (x) => (x < value);
            var defaultMessage = "{PropertyName} must be less than " + value;

            return builder.Is(predicate, message ?? defaultMessage);
        }

        public static PropertyValidationRuleBuilder<int> IsLessThanOrEqualTo(this PropertyValidationRuleBuilder<int> builder, int value, string message = null)
        {
            Predicate<int> predicate = (x) => (x <= value);
            var defaultMessage = "{PropertyName} must be less than or equal to " + value;

            return builder.Is(predicate, message ?? defaultMessage);
        }

        public static PropertyValidationRuleBuilder<int> IsGreaterThan(this PropertyValidationRuleBuilder<int> builder, int value, string message = null)
        {
            Predicate<int> predicate = (x) => (x > value);
            var defaultMessage = "{PropertyName} must be greater than " + value;

            return builder.Is(predicate, message ?? defaultMessage);
        }

        public static PropertyValidationRuleBuilder<int> IsGreaterThanOrEqualTo(this PropertyValidationRuleBuilder<int> builder, int value, string message = null)
        {
            Predicate<int> predicate = (x) => (x >= value);
            var defaultMessage = "{PropertyName} must be greater than or equal to " + value;

            return builder.Is(predicate, message ?? defaultMessage);
        }

        public static PropertyValidationRuleBuilder<int> IsBetween(this PropertyValidationRuleBuilder<int> builder, int min, int max, string message = null)
        {
            Predicate<int> predicate = (x) => (min <= x && x <= max);
            var defaultMessage = "{PropertyName} must be between " + min + " and " + max;

            return builder.Is(predicate, message ?? defaultMessage);
        }
    }
}
