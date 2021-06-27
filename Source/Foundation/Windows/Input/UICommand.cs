// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Input
{
    public class UICommand : RoutedCommandBase
    {
        #region Generic UI Commands

        private string text;

        public string Text
        {
            get { return this.text; }
            set { SetProperty(ref this.text, value); }
        }

        private ImageSource icon;

        public ImageSource Icon
        {
            get { return this.icon; }
            set { SetProperty(ref this.icon, value); }
        }

        private string description;

        public string Description
        {
            get { return this.description; }
            set { SetProperty(ref this.description, value); }
        }

        public string DescriptionAndShortcut
        {
            get
            {
                return this.AppendShortcut(this.description);
            }
        }

        public string TextAndShortcut
        {
            get
            {
                return this.AppendShortcut(this.Text);
            }
        }

        #endregion

        #region UI commands that use symbols

        private Symbol symbolIcon;

        public Symbol SymbolIcon
        {
            get { return this.symbolIcon; }
            set { SetProperty(ref this.symbolIcon, value); }
        }

        #endregion

        #region Commands displayed in Ribbons

        private ImageSource smallImage;
        private ImageSource largeImage;
        private string screenTipTitle;
        private string screenTipDescription;
        private ImageSource screenTipIcon;
        private string keyTip;

        public string PreferredScreenTipTitle
        {
            get
            {
                string title = (ScreenTipTitle != null) ? ScreenTipTitle : Text;
                return this.AppendShortcut(title);
            }
        }

        private string AppendShortcut(string text)
        {
            KeyGesture gesture = this.GetMainKeyGesture();
            return KeyGestureUtilities.FormatShortcut(text, gesture);
        }

        public string PreferredScreenTipDescription
        {
            get
            {
                return (ScreenTipDescription != null) ? ScreenTipDescription : Description;
            }
        }

        public ImageSource SmallImage
        {
            get { return this.smallImage; }
            set
            {
                SetProperty(ref this.smallImage, value);
                if (this.Icon == null)
                {
                    this.Icon = smallImage;
                }
            }
        }

        public ImageSource LargeImage
        {
            get { return this.largeImage; }
            set { SetProperty(ref this.largeImage, value); }
        }

        public string ScreenTipTitle
        {
            get { return this.screenTipTitle; }
            set { SetProperty(ref this.screenTipTitle, value); }
        }

        public string ScreenTipDescription
        {
            get { return this.screenTipDescription; }
            set { SetProperty(ref this.screenTipDescription, value); }
        }

        public ImageSource ScreenTipIcon
        {
            get { return this.screenTipIcon; }
            set { SetProperty(ref this.screenTipIcon, value); }
        }

        public string KeyTip
        {
            get { return this.keyTip; }
            set { SetProperty(ref this.keyTip, value); }
        }

        #endregion
    }
}
