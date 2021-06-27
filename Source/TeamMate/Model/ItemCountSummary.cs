namespace Microsoft.Tools.TeamMate.Model
{
    public class ItemCountSummary
    {
        public ItemCountSummary()
        {
            this.GlobalCounter = new Counter();
            this.ActiveCounter = new Counter();
            this.ResolvedCounter = new Counter();
            this.ClosedCounter = new Counter();
            this.UnknownCounter = new Counter();
        }

        public Counter GlobalCounter { get; private set; }
        public Counter ActiveCounter { get; private set; }
        public Counter ResolvedCounter { get; private set; }
        public Counter ClosedCounter { get; private set; }
        public Counter UnknownCounter { get; private set; }

        internal void Reset()
        {
            this.GlobalCounter.Reset();
            this.ActiveCounter.Reset();
            this.ResolvedCounter.Reset();
            this.ClosedCounter.Reset();
            this.UnknownCounter.Reset();
        }
    }
}
