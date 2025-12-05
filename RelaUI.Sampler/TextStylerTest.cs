using RelaUI.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Sampler
{
    public class TextStylerTest : ITextStyler
    {
        public int MonospaceSize = 9;

        public List<int> LineConfigs = new List<int>();

        public TextStyles GetTextStyles(RelaFont font, string text, TextSettings baseStyle, int lineNumber)
        {
            TextSettings style2 = baseStyle.Clone();
            style2.HasShadow = true;
            style2.ShadowDepth = 2;
            style2.ShadowColor = new Microsoft.Xna.Framework.Color(255, 0, 0, 255);

            TextSettings style3 = baseStyle.Clone();
            style3.Monospaced = true;
            style3.MonospaceSize = MonospaceSize;

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

            if (LineConfigs.Count > lineNumber - 1 && lineNumber - 1 >= 0)
            {
                var lastConfig = LineConfigs[lineNumber - 1];
                if (lastConfig == 2)
                {
                    incode = true;
                    switchStyles[0] = 2;
                }
                else if (lastConfig == 1)
                {
                    inshadow = true;
                    switchStyles[0] = 1;
                }
            }

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

            while (LineConfigs.Count <= lineNumber)
                LineConfigs.Add(0);
            LineConfigs[lineNumber] = incode ? 2 : inshadow ? 1 : 0;

            return new TextStyles(styles, switches, switchStyles, fonts);
        }

        public void PreStyling()
        {

        }

        public void PostStyling()
        {

        }
    }
}
