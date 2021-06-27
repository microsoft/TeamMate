using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data
{
    /// <summary>
    /// Represents a filter for a list view control.
    /// </summary>
    public class ListViewFilter : ObservableObjectBase, ISelectableItem
    {
        private string name;
        private bool isSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewFilter"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ListViewFilter(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewFilter"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="predicate">The predicate use to filter items.</param>
        public ListViewFilter(string name, Predicate<object> predicate)
        {
            this.name = name;
            this.Predicate = predicate;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { SetProperty(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the filter is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get { return this.isSelected; }
            set { SetProperty(ref this.isSelected, value); }
        }

        /// <summary>
        /// Gets or sets the predicate for this filter.
        /// </summary>
        public Predicate<object> Predicate { get; set; }
    }
}
