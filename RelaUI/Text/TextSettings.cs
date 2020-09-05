using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RelaUI.Styles;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Text
{
    public class TextSettings
    {
        public Color Color;
        public string FontName = string.Empty;
        public int FontSize = 0;
        public bool HasShadow = false;
        public Color ShadowColor;
        public int ShadowDepth;
        public bool Monospaced = false;
        public int MonospaceSize = 0;
        public int TabWidth = 40;

        public RelaFont Init(UIStyle Style)
        {
            Color = Style.ForegroundColor;
            ShadowColor = Style.BackgroundAccent;

            if (string.IsNullOrWhiteSpace(FontName))
            {
                // if we have no font, inherit the default
                FontName = Style.FontManager.DefaultDynamicFont;
            }
            if (FontSize == 0)
            {
                // If we have no font size, inherit the default
                FontSize = Style.FontManager.DefaultSize;
            }

            RelaFont f = Style.FontManager.ResolveRelaFont(FontName, FontSize);

            if (FontName.ToLower().Contains("_mono"))
            {
                Monospaced = true;
                MonospaceSize = 0;
                char[] test = new char[] { '_', '@', '#', ' ', '%', 'O', 'M', 'W' };
                foreach (char c in test)
                {
                    int size = TextHelper.GetWidth(f, "" + c, null);
                    if (size > MonospaceSize)
                    {
                        MonospaceSize = size;
                    }
                }
                if (FontName.ToLower().Contains("_monoshort"))
                {
                    MonospaceSize -= 1; // testing, put them closer together...
                }
            }

            return f;
        }

        public TextSettings Clone()
        {
            return new TextSettings()
            {
                Color = Color,
                FontName = FontName,
                FontSize = FontSize,
                HasShadow = HasShadow,
                ShadowColor = ShadowColor,
                ShadowDepth = ShadowDepth,
                Monospaced = Monospaced,
                MonospaceSize = MonospaceSize,
                TabWidth = TabWidth,
            };
        }
    }
}