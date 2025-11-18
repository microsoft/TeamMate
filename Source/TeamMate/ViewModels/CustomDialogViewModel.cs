using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using System.Collections.Generic;
using System.Windows.Media;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class CustomDialogViewModel : ViewModelBase
    {
        private string title;
        private string message;
        private string checkBoxText;
        private bool isCheckBoxChecked;
        private ImageSource icon;
        private ButtonInfo pressedButton;

        public CustomDialogViewModel()
        {
            this.Buttons = new List<ButtonInfo>();
        }

        public string Title
        {
            get { return this.title; }
            set { SetProperty(ref this.title, value); }
        }

        public string Message
        {
            get { return this.message; }
            set { SetProperty(ref this.message, value); }
        }

        public IList<ButtonInfo> Buttons { get; private set; }

        public ButtonInfo AddButton(string text)
        {
            ButtonInfo button = new ButtonInfo(text);
            this.Buttons.Add(button);
            return button;
        }

        public ButtonInfo AddDefaultButton(string text)
        {
            var button = AddButton(text);
            button.IsDefault = true;
            return button;
        }

        public ButtonInfo AddCancelButton(string text)
        {
            var button = AddButton(text);
            button.IsCancel = true;
            return button;
        }

        public ImageSource Icon
        {
            get { return this.icon; }
            set { SetProperty(ref this.icon, value); }
        }

        public string CheckBoxText
        {
            get { return this.checkBoxText; }
            set { SetProperty(ref this.checkBoxText, value); }
        }

        public bool IsCheckBoxChecked
        {
            get { return this.isCheckBoxChecked; }
            set { SetProperty(ref this.isCheckBoxChecked, value); }
        }

        public ButtonInfo PressedButton
        {
            get { return this.pressedButton; }
            set { SetProperty(ref this.pressedButton, value); }
        }
    }

    public class ButtonInfo
    {
        public ButtonInfo()
        {
        }

        public ButtonInfo(string text)
        {
            this.Text = text;
        }

        public string Text {  get; set; }
        public bool IsDefault {  get; set; }
        public bool IsCancel {  get; set; }
    }
}
