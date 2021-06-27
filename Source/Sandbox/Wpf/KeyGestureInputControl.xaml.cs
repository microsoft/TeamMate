// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for KeyGestureInputControl.xaml
    /// </summary>
    public partial class KeyGestureInputControl : UserControl
    {
        public KeyGestureInputControl()
        {
            InitializeComponent();

            this.PreviewKeyDown += HandleKeyDown;
            this.PreviewKeyUp += HandleKeyUp;
            this.TextBox.SelectionChanged += HandleSelectionChanged;
            InvalidateText();
        }

        private void HandleSelectionChanged(object sender, RoutedEventArgs e)
        {
            InvalidateCaret();
        }

        private void InvalidateCaret()
        {
            if (this.TextBox.SelectionLength > 0)
            {
                this.TextBox.SelectionLength = 0;
            }

            if (this.TextBox.CaretIndex != this.TextBox.Text.Length)
            {
                this.TextBox.CaretIndex = this.TextBox.Text.Length;
            }
        }

        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                if (IsWindowsKey(e))
                {
                    this.isWindowsKeyDown = false;
                }
            }

            e.Handled = true;
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                bool clear = (e.Key == Key.Back);
                if (clear)
                {
                    this.isWindowsKeyDown = false;
                    this.keyGesture = null;
                }
                else if (IsWindowsKey(e))
                {
                    this.isWindowsKeyDown= true;
                }
                else if(IsValidKey(e.Key))
                {
                    if (this.isWindowsKeyDown)
                    {
                        var modifierKeys = Keyboard.Modifiers | ((isWindowsKeyDown) ? ModifierKeys.Windows : ModifierKeys.None);
                        this.keyGesture = new KeyGesture(e.Key, modifierKeys);
                    }
                }

                InvalidateText();
                InvalidateCaret();
            }

            e.Handled = true;
        }

        private static bool IsWindowsKey(KeyEventArgs e)
        {
            return (e.Key == Key.LWin || e.Key == Key.RWin);
        }

        private bool isWindowsKeyDown;
        private KeyGesture keyGesture;

        private static bool IsValidKey(Key key)
        {
            return (key >= Key.A && key <= Key.Z)
                || key == Key.Escape
                || (key >= Key.OemSemicolon && key <= Key.OemBackslash);
        }

        private void InvalidateText()
        {
            string text = (this.keyGesture != null) ? KeyGestureUtilities.GetDisplayString(this.keyGesture) : String.Empty;
            text = text.Replace("+", " + ");
            this.TextBox.Text = text;
        }
    }
}
