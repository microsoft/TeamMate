using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM
{
    public static class ValidationUtilities
    {
        private static readonly DependencyProperty[] ValidationProperties = {
            TextBox.TextProperty,
        };

        public static bool Validate(FrameworkElement root)
        {
            bool isValid = true;

            ValidatableViewModelBase viewModel = root.DataContext as ValidatableViewModelBase;
            if (viewModel != null)
            {
                UpdateSourceTriggerOnSelectedElement(root);

                isValid = viewModel.Validate();

                UpdateValidatableBindings(root);

                if (!isValid)
                {
                    UserFeedback.PlayErrorSound();
                    LogicalTreeUtilities.FocusFirstInvalidElement(root);
                }
            }

            return isValid;
        }

        private static void UpdateValidatableBindings(DependencyObject root)
        {
            List<BindingExpression> bindingExpressions = GetValidatableBindingExpressionsUnderRoot(root);
            foreach (var expression in bindingExpressions)
            {
                UpdateValidation(expression);
            }
        }

        private static void UpdateValidation(BindingExpression expression)
        {
            // KLUDGE: To refresh validation, we unfortunately need to update the source property
            // This is what forces WPF to re-query for INotifyDataErrorInfo values
            expression.UpdateSource();
        }

        private static void UpdateSourceTriggerOnSelectedElement(DependencyObject root)
        {
            FrameworkElement e = Keyboard.FocusedElement as FrameworkElement;
            if (e != null && e.IsDescendantOf(root))
            {
                foreach (var bindingExpression in GetValidatableBindingExpressions(e))
                {
                    // KLUDGE: Flush current value. This addresses an issue with bindings not being flushed
                    // when e.g. you hit enter and there is a default OK button in a form. For e.g. textboxes
                    // with UdpateSourceTrigger=LostFocus, the value has not been flushed for example
                    //
                    // Also see http://stackoverflow.com/questions/5569768/textbox-binding-twoway-doesnt-update-until-focus-lost-wp7
                    bindingExpression.UpdateSource();
                }
            }
        }

        private static List<BindingExpression> GetValidatableBindingExpressionsUnderRoot(DependencyObject root)
        {
            List<BindingExpression> bindingExpressions = new List<BindingExpression>();
            var descendants = LogicalTreeUtilities.Descendants(root);
            foreach (var element in descendants)
            {
                bindingExpressions.AddRange(GetValidatableBindingExpressions(element));
            }
            return bindingExpressions;
        }

        private static IEnumerable<BindingExpression> GetValidatableBindingExpressions(DependencyObject element)
        {
            foreach (var property in ValidationProperties)
            {
                var bindingExpression = BindingOperations.GetBindingExpression(element, property);
                if (bindingExpression != null && bindingExpression.ParentBinding.ValidatesOnNotifyDataErrors)
                {
                    yield return bindingExpression;
                }
            }
        }
    }
}
