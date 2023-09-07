using RelaUI.Components;
using RelaUI.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Sampler
{
    public class TextStylerInnerTest : ITextStyler
    {
        public UIComponent ParentComponent;
        private class ButtonData
        {
            public List<UIComponent> Buttons = new List<UIComponent>();
            public List<UIComponent> ButtonsInUse = new List<UIComponent>();
            public List<int> ButtonIndices = new List<int>();
        }
        private Dictionary<int, ButtonData> ButtonDatas = new Dictionary<int, ButtonData>();

        public TextStylerInnerTest(UIComponent parentComponent) 
        { 
            ParentComponent = parentComponent;
        }

        public TextStyles GetTextStyles(RelaFont font, string text, TextSettings baseStyle, int lineNumber)
        {
            TextSettings style2 = baseStyle.Clone();
            style2.HasShadow = true;
            style2.ShadowDepth = 2;
            style2.ShadowColor = new Microsoft.Xna.Framework.Color(255, 0, 0, 255);

            TextSettings style3 = baseStyle.Clone();
            style3.Monospaced = true;
            style3.MonospaceSize = 9;

            List<TextSettings> styles = new List<TextSettings>()
            {
                baseStyle,
                style2,
                style3,
            };

            List<RelaFont> fonts = new List<RelaFont>();
            for (int i = 0; i < styles.Count; i++)
                fonts.Add(TextHelper.GetBestFont(font, styles[i]));

            List<int> switches = new List<int>() { 0 };
            List<int> switchStyles = new List<int>() { 0 };
            ButtonData data;
            if (!ButtonDatas.TryGetValue(lineNumber, out data))
            {
                data = new ButtonData();
                ButtonDatas.Add(lineNumber, data);
            }
            data.ButtonsInUse.Clear();
            data.ButtonIndices.Clear();

            bool incode = false;
            bool inshadow = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '`')
                {
                    if (!incode)
                    {
                        incode = true;
                        switches.Add(i);
                        switchStyles.Add(2);
                    }
                    else
                    {
                        incode = false;
                        switches.Add(i + 1);
                        switchStyles.Add(0);
                    }
                }
                else if (c == '*')
                {
                    if (!inshadow)
                    {
                        inshadow = true;
                        switches.Add(i);
                        switchStyles.Add(1);
                    }
                    else
                    {
                        inshadow = false;
                        switches.Add(i + 1);
                        switchStyles.Add(0);
                    }
                }
                else if (c == '%')
                {
                    switches.Add(i);
                    switchStyles.Add(0);

                    int buttonSlot = data.ButtonsInUse.Count;
                    UIComponent newbtn;
                    if (data.Buttons.Count == buttonSlot)
                    {
                        // create button since it doesn't exist
                        newbtn = new UIButton(0, 0, 50, font.LineSpacing, "X");
                        data.Buttons.Add(newbtn);
                        ParentComponent.Add(newbtn);
                        newbtn.TryInit();
                    }
                    else
                    {
                        newbtn = data.Buttons[buttonSlot];
                    }
                    newbtn.Visible = true;

                    data.ButtonsInUse.Add(newbtn);
                    data.ButtonIndices.Add(i);
                }
            }

            return new TextStyles(styles, switches, switchStyles, fonts, data.ButtonsInUse, data.ButtonIndices);
        }

        public void PreStyling()
        {

        }

        public void PostStyling()
        {
            // clean up old buttons
            foreach (var kvp in ButtonDatas)
            {
                for (int i = kvp.Value.ButtonsInUse.Count; i < kvp.Value.Buttons.Count; i++)
                {
                    kvp.Value.Buttons[i].Visible = false;
                }
            }
        }
    }
}
