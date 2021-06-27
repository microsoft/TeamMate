namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data
{
    /// <summary>
    /// Represents an item that can be selected from a list.
    /// </summary>
    public interface ISelectableItem
    {
        /// <summary>
        /// Gets or sets a value indicating whether this item is selected.
        /// </summary>
        bool IsSelected { get; set; }
    }
}
