using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf.Tiles
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
