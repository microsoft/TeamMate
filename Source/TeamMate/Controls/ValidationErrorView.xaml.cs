using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for ValidationErrorView.xaml
    /// </summary>
    public partial class ValidationErrorView : UserControl
    {
        public ValidationErrorView()
        {
            InitializeComponent();

            this.DataContextChanged += ValidationErrorView_DataContextChanged;
        }

        private void ValidationErrorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            INotifyDataErrorInfo oldValue = e.OldValue as INotifyDataErrorInfo;
            INotifyDataErrorInfo newValue = e.NewValue as INotifyDataErrorInfo;

            if (oldValue != null)
            {
                oldValue.ErrorsChanged -= HandleErrorsChanged;
            }

            if (newValue != null)
            {
                newValue.ErrorsChanged += HandleErrorsChanged;
            }

            InvalidateError();
        }

        private void HandleErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(e.PropertyName))
            {
                InvalidateError();
            }
        }

        private void InvalidateError()
        {
            INotifyDataErrorInfo info = this.DataContext as INotifyDataErrorInfo;
            var errors = (info != null) ? info.GetErrors(String.Empty) : null;
            var firstError = (errors != null) ? errors.OfType<string>().FirstOrDefault() : null;

            this.ErrorMessage = firstError;
        }

        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(
            "ErrorMessage", typeof(string), typeof(ValidationErrorView)
        );

        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }
    }
}
