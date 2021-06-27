// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Preview
{
    /// <summary>
    /// Defines the contract for a file preview control that can previwe a given file type.
    /// </summary>
    public interface IFilePreviewControl
    {
        /// <summary>
        /// Gets the UI element used to display the preview of the file.
        /// </summary>
        FrameworkElement Host { get; }

        /// <summary>
        /// Determines whether this instance can preview the specified file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns><c>true</c> if this instance supports previewing that file. Otherwise, <c>false</c>.</returns>
        bool CanPreview(string filename);

        /// <summary>
        /// Begins loading the preview for the given file..
        /// </summary>
        /// <param name="filename">The filename.</param>
        void BeginLoad(string filename);

        /// <summary>
        /// Clears the currently previewed file and releases any associated resources.
        /// </summary>
        void Clear();

        /// <summary>
        /// Occurs when file loading has completed (successfully or not).
        /// </summary>
        event EventHandler<LoadEventArgs> LoadCompleted;
    }
}
