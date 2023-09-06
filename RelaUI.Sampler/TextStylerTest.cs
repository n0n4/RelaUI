using RelaUI.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Sampler
{
    public class TextStylerTest : ITextStyler
    {
        public TextStyles GetTextStyles(RelaFont font, string text, TextSettings baseStyle)
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
            }

            return new TextStyles(styles, switches, switchStyles, fonts);
        }
    }
}
