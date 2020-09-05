using RelaUI.Components;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Dialogs
{
    public class DialogTextInput : DialogBase
    {
        private string Title = string.Empty;
        private string Text = string.Empty;
        private int TitleFontSize = 16;
        private int TextFontSize = 12;
        private string DefaultText = string.Empty;
        private string PlaceholderText = string.Empty;

        private UITextField TextField;

        public bool Result = false; // true if yes/OK
        public string ResultText = string.Empty;

        public DialogTextInput(string title, string text, string defaultText, string placeholderText, int titlefontsize = 16, int textfontsize = 12)
        {
            Title = title;
            Text = text;
            TitleFontSize = titlefontsize;
            TextFontSize = textfontsize;

            DefaultText = defaultText;
            PlaceholderText = placeholderText;
        }

        protected override void Construct()
        {
            Tuple<float, float> cpos = GetCenterOfParent();

            int width = ((int)cpos.Item2 * 4 / 3);
            int height = 200;

            Panel = new UIPanel(cpos.Item1 - width / 2, cpos.Item2 - height / 2, width, height,
                hastitle: !string.IsNullOrEmpty(Title), title: Title, titlesize: TitleFontSize);
            Panel.LockFocus = true;

            UILabel textlabel = new UILabel(10, 10, Panel.Width - 20, 80, Text, fontsize: TextFontSize);
            Panel.AddAuto(textlabel);

            TextField = new UITextField(10, 15, Panel.Width - 20, 40, DefaultText, fontsize: TextFontSize, placeholdertext: PlaceholderText);
            Panel.AddAuto(TextField);
            TextField.EventEnterPressed += (sender, args) =>
            {
                var eargs = args as UITextField.EventEnterPressedHandlerArgs;
                OKResult(eargs.ElapsedMS, eargs.Input);
            };

            UIPanel btnspanel = new UIPanel(0, 20, Panel.Width, 30)
            {
                HasBorder = false,
                InnerMargin = 0,
                AutoOrientation = eUIOrientation.HORIZONTAL,
            };
            Panel.AddAuto(btnspanel);

            UIButton btncancel = new UIButton(10, 0, Panel.Width / 4, 30,
                "Cancel", fontsize: TitleFontSize);
            btncancel.EventFocused += (sender, e) =>
            {
                UIComponent.EventFocusedHandlerArgs eargs = e as UIComponent.EventFocusedHandlerArgs;
                CancelResult(eargs.ElapsedMS, eargs.Input);
            };
            btnspanel.AddAuto(btncancel);
            UIButton btnok = new UIButton(Panel.Width - (Panel.Width / 2) - 10, 0, Panel.Width / 4, 30,
                "OK", fontsize: TitleFontSize);
            btnok.EventFocused += (sender, e) =>
            {
                UIComponent.EventFocusedHandlerArgs eargs = e as UIComponent.EventFocusedHandlerArgs;
                OKResult(eargs.ElapsedMS, eargs.Input);
            };
            btnspanel.AddAuto(btnok);

            Panel.EventPostInit += (sender, args) =>
            {
                Panel.Height -= (80 - textlabel.Height);
            };
            // end construct
        }

        protected override void SelfPopup()
        {
            ParentSystem.ProposedFocus = TextField;
        }

        public void OKResult(float elapsedms, InputManager input)
        {
            Result = true;
            ResultText = TextField.Text;
            Close(elapsedms, input);
        }
        public void CancelResult(float elapsedms, InputManager input)
        {
            Result = false;
            Close(elapsedms, input);
        }


    }
}
