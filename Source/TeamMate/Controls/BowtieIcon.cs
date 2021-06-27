// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Controls
{
    public class BowtieIcon : FontIcon
    {
        public static readonly DependencyProperty BowtieProperty = DependencyProperty.Register(
            "Symbol", typeof(Bowtie), typeof(BowtieIcon), new PropertyMetadata((d, e) => ((BowtieIcon)d).OnSymbolChanged())
        );

        public Bowtie Symbol
        {
            get { return (Bowtie)GetValue(BowtieProperty); }
            set { SetValue(BowtieProperty, value); }
        }

        private void OnSymbolChanged()
        {
            this.Glyph = ((char)this.Symbol).ToString();
        }
    }
}
