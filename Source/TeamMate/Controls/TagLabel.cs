// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Controls
{
    public class TagLabel : Control
    {
        static TagLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TagLabel), new FrameworkPropertyMetadata(typeof(TagLabel)));
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(TagLabel)
        );

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }
}
