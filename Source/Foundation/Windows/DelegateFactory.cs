using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    public static class DelegateFactory
    {
        public static StyleSelector CreateStyleSelector(Func<object, DependencyObject, Style> selector)
        {
            Assert.ParamIsNotNull(selector, "selector");

            return new DelegateStyleSelector(selector);
        }

        public static DataTemplateSelector CreateTemplateSelector(Func<object, DependencyObject, DataTemplate> selector)
        {
            Assert.ParamIsNotNull(selector, "selector");

            return new DelegateTemplateSelector(selector);
        }

        public static IValueConverter CreateValueConverter(Func<object, object> converter)
        {
            Assert.ParamIsNotNull(converter, "converter");

            Func<object, Type, object, CultureInfo, object> fullConverter = (a, b, c, d) => converter(a);
            return CreateValueConverter(fullConverter);
        }

        public static IValueConverter CreateValueConverter(Func<object, Type, object, CultureInfo, object> converter)
        {
            Assert.ParamIsNotNull(converter, "converter");

            return new DelegateConverter(converter);
        }

        private class DelegateConverter : OneWayConverterBase
        {
            private Func<object, Type, object, CultureInfo, object> converter;

            public DelegateConverter(Func<object, Type, object, CultureInfo, object> converter)
            {
                this.converter = converter;
            }

            public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return converter(value, targetType, parameter, culture);
            }
        }

        private class DelegateTemplateSelector : DataTemplateSelector
        {
            private Func<object, DependencyObject, DataTemplate> selector;

            public DelegateTemplateSelector(Func<object, DependencyObject, DataTemplate> selector)
            {
                this.selector = selector;
            }

            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                return selector(item, container);
            }
        }

        private class DelegateStyleSelector : StyleSelector
        {
            private Func<object, DependencyObject, Style> selector;

            public DelegateStyleSelector(Func<object, DependencyObject, Style> selector)
            {
                this.selector = selector;
            }

            public override Style SelectStyle(object item, DependencyObject container)
            {
                return selector(item, container);
            }
        }
    }
}
