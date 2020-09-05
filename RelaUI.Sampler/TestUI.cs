using Microsoft.Xna.Framework.Graphics;
using RelaUI.Basics.Stylers;
using RelaUI.Components;
using RelaUI.Dialogs;
using RelaUI.Input;
using RelaUI.Styles;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Sampler
{
    public class TestUI : IUIHandle
    {
        public UISystem UISystem;

        public TestUI(UIStyle style, GraphicsDevice graphicsDevice)
        {
            UISystem = new UISystem(graphicsDevice);
            // panel 1
            UIPanel panel = new UIPanel(25, 25, 150, 150, hastitle: true, title: "Test Panel 1", titlesize: 16);
            UISystem.Add(panel);

            UILabel label1 = new UILabel(0, 0, panel.Width - 10, 10, "text goes here let's see if it will wrap automatically, which it should if everything is working properly.", fontsize: 12,
                font: "NotoSans_regular"); // test spritefont
            panel.AddAuto(label1);

            // panel 2
            UIPanel panel2 = new UIPanel(225, 25, 150, 150, hastitle: true, title: "Test Panel 2", titlesize: 16);
            UISystem.Add(panel2);

            UIButton button1 = new UIButton(5, 5, 75, 25, "Test Button");
            panel2.AddAuto(button1);

            UICheckbox chk1 = new UICheckbox(5, 40, 20, 20);
            panel2.Add(chk1);
            UILabel chk1label = new UILabel(30, 40, 100, 20, "unchecked");
            panel2.Add(chk1label);
            chk1.EventCheckChanged += (sender, args) =>
            {
                if (chk1.Checked)
                {
                    chk1label.Text = "checked";
                }
                else
                {
                    chk1label.Text = "unchecked";
                }
                chk1label.ProcessText();
            };


            // panel 3
            UIPanel panel3 = new UIPanel(425, 25, 150, 150, hastitle: true, title: "Test Panel 3", titlesize: 12);
            UISystem.Add(panel3);

            UITextField textfield1 = new UITextField(5, 5, 135, 25, "", fontsize: 16, placeholdertext: "Try Typing...");
            panel3.AddAuto(textfield1);

            UINumberField numfield1 = new UINumberField(5, 5, 135, 35, 40, "0", true, fontsize: 16);
            panel3.AddAuto(numfield1);

            UINumberField numfield2 = new UINumberField(5, 5, 135, 35, 40, "0.0", false, fontsize: 16);
            panel3.AddAuto(numfield2);

            // panel 4
            UIPanel panel4 = new UIPanel(625, 25, 150, 150, hastitle: true, title: "Test Panel 4", titlesize: 14);
            UISystem.Add(panel4);

            UILabel label2 = new UILabel(0, 0, panel4.Width - 10, 10, "text goes here let's see if it will wrap automatically, which it should if everything is working properly.", fontsize: 18);
            label2.TextSplitWords = true;
            panel4.AddAuto(label2);
            UILabel label3 = new UILabel(0, 10, panel4.Width - 10, 10, "A second auto-label. This gal should automatically get cut off when it goes too long", fontsize: 16);
            label3.TextSplitWords = true;
            panel4.AddAuto(label3);

            // panel 5
            UIPanel panel5 = new UIPanel(25, 200, 150, 150, true, "Dropdown Test");
            UISystem.Add(panel5);

            UIDropdown drop1 = new UIDropdown(0, 0, 80, 30, "droptest");
            drop1.AddItem("log A", (uid, args) => { drop1.Text = "AAAA"; drop1.ProcessText(); });
            drop1.AddItem("log B", (uid, args) => { drop1.Text = "BBBB"; drop1.ProcessText(); });
            panel5.AddAuto(drop1);

            // panel 6
            UIPanel panel6 = new UIPanel(200, 200, 150, 150, true, "Scroll Vert Test", hasscrolling: true, scrollh: 400);
            UISystem.Add(panel6);
            UILabel label4 = new UILabel(0, 10, panel6.Width - 20, 350, "newlines \n should be auto processed 1\n 2\n 3\n 4\n 5\n test the bottom \n test", fontsize: 16);
            panel6.AddAuto(label4);

            // panel 7
            UIPanel panel7 = new UIPanel(400, 200, 150, 150, true, "Scroll Horz Test", hasscrolling: true, scrollw: 400);
            UISystem.Add(panel7);
            UILabel label5 = new UILabel(0, 10, panel7.ScrollWidth - 10, 10, "a very long bit a text. It just goes on and on and doesn't end. A very long line that should be much smaller. But it's not, it's very large, instead.", fontsize: 18);
            panel7.AddAuto(label5);

            // panel 8
            UIPanel panel8 = new UIPanel(600, 200, 150, 150, true, "Dialogs Test");
            UISystem.Add(panel8);
            UIButton popupbtn = new UIButton(0, 0, 100, 30, "pop up");
            panel8.AddAuto(popupbtn);
            UIButton filebtn = new UIButton(0, 0, 100, 30, "open file");
            panel8.AddAuto(filebtn);

            popupbtn.EventFocused += (sender, args) =>
            {
                UIComponent.EventFocusedHandlerArgs eargs = args as UIComponent.EventFocusedHandlerArgs;
                DialogPopup popup = new DialogPopup("Popup", "time elapsed: " + eargs.ElapsedMS);
                popup.Popup(UISystem);
                popup.EventClosed += (innersender, innerargs) =>
                {
                    popup.Dispose();
                };
            };
            filebtn.EventFocused += (sender, args) =>
            {
                UIComponent.EventFocusedHandlerArgs eargs = args as UIComponent.EventFocusedHandlerArgs;
                DialogFile filepopup = new DialogFile(DialogFile.eMode.SELECT_FILE, ".\\", "file.txt", "Select a File");
                filepopup.Popup(UISystem);
                filepopup.EventClosed += (innersender, innerargs) =>
                {
                    filepopup.Dispose();
                };
            };

            // panel 9
            UIPanel panel9 = new UIPanel(100, 350, 220, 100);
            UISystem.Add(panel9);
            UITextField multiLineText = new UITextField(0, 0, 200, 80, "", autoheight: false, placeholdertext: "try typing multi lines", fontsize: 11)
            {
                MultiLine = true
            };
            multiLineText.SetTextStyler(new TextStylerTest());
            panel9.Add(multiLineText);

            // panel 10
            UIPanel panel10 = new UIPanel(20, 20, 400, 400, true, "RelaScript", titlesize: 12, hasclose: true);
            panel10.CloseButton.EventFocused += (sender, eargs) =>
            {
                panel10.Visible = false;
            };
            UISystem.Add(panel10);
            UITextField relaMultiText = new UITextField(5, 5, 380, 350, "", autoheight: false, 
                fontsize: 18, font: "NotoMono-Regular")
            {
                MultiLine = true
            };
            relaMultiText.EventPostInit += (sender, eargs) =>
            {
                relaMultiText.TextColor = new Microsoft.Xna.Framework.Color(218, 218, 218, 255);
            };
            relaMultiText.SetTextStyler(new TextStylerRelaScript());
            panel10.Add(relaMultiText);
            
            UISystem.Init(style);
        }

        public UISystem GetUISystem()
        {
            return UISystem;
        }

        public string GetName()
        {
            return "TestUI";
        }

        public void Update(float elapsedms, InputManager input)
        {

        }

        public void RegisterHotkeys(HotkeyManager hotkeyManager)
        {

        }
    }
}
