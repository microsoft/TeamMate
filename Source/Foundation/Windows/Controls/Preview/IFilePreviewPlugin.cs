using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Preview
{
    /// <summary>
    /// A plugin interface that can help install previewer pre-requisites on demand.
    /// </summary>
    public interface IFilePreviewPlugin
    {
        /// <summary>
        /// Gets the plugin text to be displayed when a file cannot be previewed but
        /// an element can be downloaded to enable preview.
        /// </summary>
        /// <param name="filePath">The file path to be previewd.</param>
        /// <returns>The plug in text, or <c>null</c> if the file cannot be previewed.</returns>
        string GetPluginText(string filePath);

        /// <summary>
        /// Prepares the system for previewing an item by prompting for a download or install.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns><c>true</c> if the file can be previewed and the user was prompted,
        /// otherwise <c>false</c>.</returns>
        Task<bool> PrepareForPreviewAsync(string filePath);
    }
}
