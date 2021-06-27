using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Interop
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationHotKeys
    {
        private const int NoopAction = 0;
        private static int NextActionId = 1;
        private static int RegisteredHotKeyCount = 0;

        private const int WM_HOTKEY = (int) WindowsMessage.WM_HOTKEY;


        private static Dictionary<KeyGesture, int> gestureToActionIdMap = new Dictionary<KeyGesture, int>();
        private static Dictionary<int, Action> actionMap = new Dictionary<int, Action>();

        public static ICollection<KeyGesture> RegisteredHotKeys
        {
            get
            {
                return gestureToActionIdMap.Keys.ToArray();
            }
        }

        public static bool IsHotKeyAvailable(KeyGesture gesture)
        {
            Assert.ParamIsNotNull(gesture, "gesture");

            // Crappy way of checking for registered hotkeys, but it works... If there is anything better, go for it...
            bool success = NativeMethods.RegisterHotKey(IntPtr.Zero, NoopAction, (int)gesture.Modifiers, KeyInterop.VirtualKeyFromKey(gesture.Key));
            if (success)
            {
                NativeMethods.UnregisterHotKey(IntPtr.Zero, NoopAction);
            }

            return success;
        }

        public static ICollection<Key> GetAvailableHotKeys(ModifierKeys modifiers)
        {
            List<Key> result = new List<Key>();

            for (Key key = Key.A; key < Key.Z; key++)
            {
                KeyGesture gesture = new KeyGesture(key, modifiers);
                if (!IsHotKeyAvailable(gesture))
                {
                    result.Add(key);
                }
            }

            return result;
        }

        public static bool TryRegisterHotKey(KeyGesture gesture, Action action)
        {
            Assert.ParamIsNotNull(gesture, "gesture");
            Assert.ParamIsNotNull(action, "action");

            int actionId = NextActionId;
            bool success = NativeMethods.RegisterHotKey(IntPtr.Zero, actionId, (int)gesture.Modifiers, KeyInterop.VirtualKeyFromKey(gesture.Key));
            if(success)
            {
                gestureToActionIdMap[gesture] = actionId;
                actionMap[actionId] = action;

                NextActionId++;

                if (RegisteredHotKeyCount++ == 0)
                {
                    ComponentDispatcher.ThreadPreprocessMessage += HandleThreadPreprocessMessage;
                }
            }

            return success;
        }

        public static void RegisterHotKey(KeyGesture gesture, Action action)
        {
            if(!TryRegisterHotKey(gesture, action))
            {
                // TODO: Better exception
                throw new Exception("Failed to register keyboard hotkey, another one is already registered?");
            }
        }

        public static bool UnregisterHotKey(KeyGesture gesture)
        {
            Assert.ParamIsNotNull(gesture, "gesture");

            bool success = false;
            int actionId;
            if (gestureToActionIdMap.TryGetValue(gesture, out actionId))
            {
                success = NativeMethods.UnregisterHotKey(IntPtr.Zero, actionId);
                if (success)
                {
                    actionMap.Remove(actionId);
                    if (--RegisteredHotKeyCount == 0)
                    {
                        ComponentDispatcher.ThreadPreprocessMessage -= HandleThreadPreprocessMessage;
                    }
                }
            }

            return success;
        }

        public static void UnregisterAllHotKeys()
        {
            foreach (KeyGesture gesture in RegisteredHotKeys)
            {
                UnregisterHotKey(gesture);
            }
        }

        private static void HandleThreadPreprocessMessage(ref System.Windows.Interop.MSG msg, ref bool handled)
        {
            handled = false;

            switch (msg.message)
            {
                case WM_HOTKEY:
                    int actionId = (int) msg.wParam;
                    Action action;
                    if (actionMap.TryGetValue(actionId, out action))
                    {
                        handled = true;

                        try
                        {
                            // Invoke the action in the UI thread. If an exception is thrown, the dispatcher's
                            // exception handling mechanisms will kick in.
                            Dispatcher.CurrentDispatcher.BeginInvoke(action);
                        }
                        catch (Exception e)
                        {
                            Log.ErrorAndBreak(e);
                        }
                    }

                    break;
            }
        }
    }
}
