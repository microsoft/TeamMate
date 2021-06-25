using Microsoft.Internal.Tools.TeamMate.Foundation.ComponentModel;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf
{
    public class TileModel : ObservableObjectBase
    {
        private string title;

        public string Title
        {
            get { return this.title; }
            set { SetProperty(ref this.title, value); }
        }

        private int count;

        public int Count
        {
            get { return this.count; }
            set { SetProperty(ref this.count, value); }
        }

        private System.Collections.ICollection items;

        public System.Collections.ICollection Items
        {
            get { return this.items; }
            set { SetProperty(ref this.items, value); }
        }
    }
}
