// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf.Tiles
{
    public class TileGroup : ObservableObjectBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { SetProperty(ref this.name, value); }
        }

        private ObservableCollection<Tile> tiles = new ObservableCollection<Tile>();

        public ICollection<Tile> Tiles
        {
            get { return this.tiles; }
        }
    }
}
