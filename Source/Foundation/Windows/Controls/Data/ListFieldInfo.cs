using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Windows.Data;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data
{
    /// <summary>
    /// Represents a field displayed in a list view.
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ListFieldInfo : ObservableObjectBase, ISelectableItem
    {
        private string name;
        private string propertyName;
        private bool isSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListFieldInfo"/> class.
        /// </summary>
        public ListFieldInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListFieldInfo"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="name">The name.</param>
        public ListFieldInfo(string propertyName, string name)
        {
            this.propertyName = propertyName;
            this.name = name;
        }

        /// <summary>
        /// Gets or sets the display name of the field.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { SetProperty(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the name of the object property used to render the field value.
        /// </summary>
        public string PropertyName
        {
            get { return this.propertyName; }
            set { SetProperty(ref this.propertyName, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this field is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return this.isSelected; }
            set { SetProperty(ref this.isSelected, value); }
        }

        /// <summary>
        /// Gets or sets the group converter for displaying the group names.
        /// </summary>
        public IValueConverter GroupConverter { get; set; }

        /// <summary>
        /// Gets or sets the sort text when the field is sorted in ascending order.
        /// </summary>
        public string AscendingOrderText { get; set; }

        /// <summary>
        /// Gets or sets the sort text when the field is sorted in desceding order.
        /// </summary>
        public string DescendingOrderText { get; set; }

        /// <summary>
        /// Gets or sets the default sort direction.
        /// </summary>
        public ListSortDirection DefaultSortDirection { get; set; }

        /// <summary>
        /// Creates a new field and initializes properties to default values.
        /// </summary>
        /// <typeparam name="T">The type of the field property.</typeparam>
        /// <param name="propertyName">Name of the object's property.</param>
        /// <param name="name">The display name.</param>
        /// <returns>The created field.</returns>
        public static ListFieldInfo Create<T>(string propertyName, string name)
        {
            ListFieldInfo field = new ListFieldInfo(propertyName, name);

            Type fieldType = typeof(T);
            if (fieldType == typeof(string))
            {
                field.AscendingOrderText = "A to Z";
                field.DescendingOrderText = "Z to A";
            }
            else if (fieldType == typeof(DateTime))
            {
                field.GroupConverter = new DateGroupingConverter();
                field.DescendingOrderText = "Newest";
                field.AscendingOrderText = "Oldest";
                field.DefaultSortDirection = ListSortDirection.Descending;
            }
            else if (fieldType == typeof(int) || fieldType == typeof(double) || fieldType == typeof(long) 
                || fieldType == typeof(float) || fieldType == typeof(short))
            {
                field.AscendingOrderText = "Smallest";
                field.DescendingOrderText = "Largest";
            }
            else
            {
                field.AscendingOrderText = "Ascending";
                field.DescendingOrderText = "Descending";
            }

            return field;
        }
    }
}
