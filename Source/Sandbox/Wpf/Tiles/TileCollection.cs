// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf.Tiles
{
    public class TileCollection
    {
        private ObservableCollection<TileGroup> groups = new ObservableCollection<TileGroup>();

        public ICollection<TileGroup> Groups
        {
            get { return this.groups; }
        }
    }
}
