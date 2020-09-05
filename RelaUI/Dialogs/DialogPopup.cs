using RelaUI.Components;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Dialogs
{
    public class DialogPopup : DialogBase
    {
        private string Title = string.Empty;
        private string Text = string.Empty;
        private int TitleFontSize = 16;
        private int TextFontSize = 12;
        private eDialogPopupButtons ButtonStyle = eDialogPopupButtons.OK;

        public bool Result = false; // true if yes/OK

        public enum eDialogPopupButtons
        {
            OK,
            OK_CANCEL,
            YES_NO
        }

        public DialogPopup(string title, string text, int titlefontsize = 16, int textfontsize = 12,
            eDialogPopupButtons buttonstyle = eDialogPopupButtons.OK)
        {
            Title = title;
            Text = text;
            TitleFontSize = titlefontsize;
            TextFontSize = textfontsize;

            ButtonStyle = buttonstyle;
        }

        protected override void Construct()
        {
            Tuple<float, float> cpos = GetCenterOfParent();

            int width = ((int)cpos.Item2 * 4 / 3);
            int height = 200;

            Panel = new UIPanel(cpos.Item1 - width / 2, cpos.Item2 - height / 2, width, height,
                hastitle: !string.IsNullOrEmpty(Title), title: Title, titlesize: TitleFontSize);
            Panel.LockFocus = true;

            UILabel textlabel = new UILabel(10, 10, Panel.Width - 20, 110, Text, fontsize: TextFontSize);
            Panel.AddAuto(textlabel);

            UIPanel btnspanel = new UIPanel(0, 15, Panel.Width, 30)
            {
                HasBorder = false,
                InnerMargin = 0,
                AutoOrientation = eUIOrientation.HORIZONTAL,
            };
            Panel.AddAuto(btnspanel);

            switch (ButtonStyle)
            {
                case eDialogPopupButtons.OK:
                    {
                        UIButton btnok = new UIButton(Panel.Width - (Panel.Width / 4) - 10, 0, Panel.Width / 4, 30,
                            "OK", fontsize: TitleFontSize);
                        btnok.EventFocused += (sender, e) =>
                        {
                            UIComponent.EventFocusedHandlerArgs eargs = e as UIComponent.EventFocusedHandlerArgs;
                            OKResult(eargs.ElapsedMS, eargs.Input);
                        };
                        btnspanel.AddAuto(btnok);
                        break;
                    }
                case eDialogPopupButtons.OK_CANCEL:
                    {
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
                        break;
                    }
                case eDialogPopupButtons.YES_NO:
                    {
                        UIButton btncancel = new UIButton(10, 0, Panel.Width / 4, 30,
                            "No", fontsize: TitleFontSize);
                        btncancel.EventFocused += (sender, e) =>
                        {
                            UIComponent.EventFocusedHandlerArgs eargs = e as UIComponent.EventFocusedHandlerArgs;
                            CancelResult(eargs.ElapsedMS, eargs.Input);
                        };
                        btnspanel.AddAuto(btncancel);
                        UIButton btnok = new UIButton(Panel.Width - (Panel.Width / 2) - 10, 0, Panel.Width / 4, 30,
                            "Yes", fontsize: TitleFontSize);
                        btnok.EventFocused += (sender, e) =>
                        {
                            UIComponent.EventFocusedHandlerArgs eargs = e as UIComponent.EventFocusedHandlerArgs;
                            OKResult(eargs.ElapsedMS, eargs.Input);
                        };
                        btnspanel.AddAuto(btnok);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            Panel.EventPostInit += (sender, args) =>
            {
                Panel.Height -= (110 - textlabel.Height);
            };
            // end construct
        }

        public void OKResult(float elapsedms, InputManager input)
        {
            Result = true;
            Close(elapsedms, input);
        }
        public void CancelResult(float elapsedms, InputManager input)
        {
            Result = false;
            Close(elapsedms, input);
        }


    }
}
