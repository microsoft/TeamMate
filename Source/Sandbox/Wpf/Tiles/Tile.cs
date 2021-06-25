﻿using Microsoft.Internal.Tools.TeamMate.Foundation.ComponentModel;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf.Tiles
{
    public class Tile : ObservableObjectBase
    {
        private TileSize size;

        public TileSize Size
        {
            get { return this.size; }
            set { SetProperty(ref this.size, value); }
        }

        private object data;

        public object Data
        {
            get { return this.data; }
            set { SetProperty(ref this.data, value); }
        }
    }

    public enum TileSize
    {
        Single,
        Double
    }
}
