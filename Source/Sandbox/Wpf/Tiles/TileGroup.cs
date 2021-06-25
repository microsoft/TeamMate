using Microsoft.Internal.Tools.TeamMate.Foundation.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf.Tiles
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
