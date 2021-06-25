using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Input
{
    public static class KeyGestureUtilities
    {
        private static readonly Dictionary<Key, string> KeyDisplayStrings = new Dictionary<Key, string>() {
            { Key.OemTilde,         "`" },
            { Key.OemMinus,         "-" },
            { Key.OemPlus,          "=" },
            { Key.OemOpenBrackets,  "[" },
            { Key.OemCloseBrackets, "]" },
            { Key.OemPipe,          "\\" },
            { Key.OemSemicolon,     ";" },
            { Key.OemQuotes ,       "'" },
            { Key.OemComma,         "," },
            { Key.OemPeriod,        "." },
            { Key.OemQuestion,      "/" },
        };

        // Returns human friendly gesture strings, as opposed to the default method.
        public static string GetDisplayString(KeyGesture gesture)
        {
            return GetDisplayStringForCulture(gesture, CultureInfo.CurrentCulture);
        }

        public static string GetDisplayStringForCulture(KeyGesture gesture, CultureInfo culture)
        {
            if (!String.IsNullOrEmpty(gesture.DisplayString))
            {
                return gesture.DisplayString;
            }

            string gestureString = gesture.GetDisplayStringForCulture(culture);

            string keyText;
            if (KeyDisplayStrings.TryGetValue(gesture.Key, out keyText))
            {
                int indexOf = gestureString.LastIndexOf('+');
                if (indexOf >= 0)
                {
                    gestureString = gestureString.Substring(0, indexOf + 1);
                }

                gestureString += keyText;
            }

            return gestureString;
        }

        public static string FormatShortcut(string text, KeyGesture gesture)
        {
            string shortcutString = (gesture != null) ? GetDisplayString(gesture) : null;

            if (text != null && shortcutString != null)
            {
                return String.Format("{0} ({1})", text, shortcutString);
            }
            else if (text != null)
            {
                return text;
            }
            else
            {
                return shortcutString;
            }
        }
    }
}
