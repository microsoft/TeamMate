using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Resources
{
    public static class TeamMateGestures
    {
        static TeamMateGestures()
        {
            SelectWorkItem = new KeyGesture(Key.Up, ModifierKeys.Shift | ModifierKeys.Alt);
            NextAttachment = new KeyGesture(Key.Right, ModifierKeys.Shift | ModifierKeys.Alt);
            PreviousAttachment = new KeyGesture(Key.Left, ModifierKeys.Shift | ModifierKeys.Alt);

            Search = new KeyGesture(Key.E, ModifierKeys.Control);
            Filter = new KeyGesture(Key.F, ModifierKeys.Control);

            QuickCreate = new KeyGesture(Key.A, ModifierKeys.Shift | ModifierKeys.Windows);
            QuickCreateWithOptions = new KeyGesture(Key.A, ModifierKeys.Control | ModifierKeys.Windows);
            QuickSearch = new KeyGesture(Key.OemTilde, ModifierKeys.Windows);
            ToggleMainWindow = new KeyGesture(Key.Escape, ModifierKeys.Windows);

            New = new KeyGesture(Key.N, ModifierKeys.Control);
            Minimize = new KeyGesture(Key.Escape);
            StopFilteringOrMinimize = new KeyGesture(Key.Escape);

            Close = new KeyGesture(Key.W, ModifierKeys.Control);
            Close2 = new KeyGesture(Key.Escape);
        }

        // Attachments Bar View
        public static KeyGesture SelectWorkItem { get; private set; }
        public static KeyGesture NextAttachment { get; private set; }
        public static KeyGesture PreviousAttachment { get; private set; }

        public static KeyGesture Search { get; private set; }
        public static KeyGesture Filter { get; private set; }

        public static KeyGesture QuickCreate { get; private set; }
        public static KeyGesture QuickCreateWithOptions { get; private set; }
        public static KeyGesture QuickSearch { get; private set; }
        public static KeyGesture ToggleMainWindow { get; private set; }

        public static KeyGesture New { get; private set; }
        public static KeyGesture Minimize { get; private set; }
        public static KeyGesture StopFilteringOrMinimize { get; private set; }
        public static KeyGesture Close { get; private set; }
        public static KeyGesture Close2 { get; private set; }
    }
}
