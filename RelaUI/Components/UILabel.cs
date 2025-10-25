using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RelaUI.DrawHandles;
using RelaUI.Input;
using RelaUI.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Components
{
    public class UILabel : UIComponent
    {
        public int Width = 100;
        public int Height = 100;
        public bool AutoHeight = true;

        public bool HasBackground = false;
        public Color BackgroundColor;
        public bool HasBorder = false;
        public Color BorderColor;
        public int BorderWidth;

        private ITextStyler TextStyler = null;
        private List<TextStyles> ComputedStyles = new List<TextStyles>();

        public string Text
        {
            get
            {
                return RenderedText.Text;
            }
            set
            {
                RenderedText.Text = value;
            }
        }
        public RenderedText RenderedText = new RenderedText();
        // if you change Text, or Width, or the font/fontsize, call ProcessText to reprocess it
        // TODO: can we make this reactive somehow?
        public bool TextSplitWords = false; // if this is true, allows words to be hyphenated
        public bool TextOneLineOnly = false;
        public List<string> TextLines = new List<string>();
        // NOTE: to update font, updating above will not auto-update the font
        // you must re-init the label or do SFont = FontManager.ResolveFont(Font, FontSize)
        // TODO: can we fix that somehow?
        public RelaFont SFont;
        public TextSettings FontSettings = new TextSettings();

        public UILabel(float x, float y, int w, int h, string text, string font = "", int? fontsize = null,
            bool autoheight = true)
        {
            this.x = x;
            this.y = y;
            Width = w;
            Height = h;
            Text = text;
            FontSettings.FontName = font;
            FontSettings.FontSize = fontsize != null ? (int)fontsize : 0;

            // need to change these for width/height preferences to be actually used
            AutoHeight = autoheight;
        }

        protected override void SelfInit()
        {
            FontSettings.Color = Style.ForegroundColor;
            BackgroundColor = Style.BackgroundColor;
            BorderColor = Style.BackgroundAccent;
            BorderWidth = Style.BorderWidth;

            SFont = FontSettings.Init(Style);

            ProcessText();
        }

        public void ProcessText()
        {
            if (SFont == null)
                return;
            TextLines = TextHelper.FitToBox(SFont, RenderedText, Width, Height, FontSettings, TextSplitWords);

            if (TextOneLineOnly)
            {
                while (TextLines.Count > 1)
                    TextLines.RemoveAt(TextLines.Count - 1);
            }

            if (AutoHeight)
            {
                Height = TextLines.Count * SFont.LineSpacing; // readjust the height
            }
            while (TextLines.Count * SFont.LineSpacing > Height)
            {
                TextLines.RemoveAt(TextLines.Count - 1);
            }

            // try to update styling
            ComputeStyles();
        }

        public override int GetWidth()
        {
            return Width;
        }

        public override int GetHeight()
        {
            return Height;
        }

        public int GetTextWidth()
        {
            if (!Initialized)
                throw new Exception("Tried to get Text Width while uninitialized");

            // unsafe justification: comes from renderedText
            if (TextStyler != null && ComputedStyles.Count > 0)
            {
                return TextHelper.GetWidthMultiStylesUnsafe(SFont, TextLines[0], ComputedStyles[0]);
            }
            return TextHelper.GetWidthUnsafe(SFont, TextLines[0], FontSettings);
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            if (HasBackground)
                Draw.DrawRectangleHandle(sb, (int)dx, (int)dy, Width, Height, BackgroundColor);
            //Draw.DrawRectangleHandle(HandleBackRect, sb, dx, dy, BackgroundColor);
            if (HasBorder)
                Draw.DrawRectangleHandleOutline(sb, (int)dx, (int)dy, Width, Height, BorderColor, BorderWidth);

            for (int i = 0; i < TextLines.Count; i++)
            {
                if (ComputedStyles.Count > i)
                {
                    Draw.DrawTextMultiStyles(g, sb, dx, dy + i * SFont.LineSpacing, 2, SFont.LineSpacing, TextLines[i], ComputedStyles[i]);
                }
                else
                {
                    Draw.DrawText(g, sb, SFont, dx, dy + i * SFont.LineSpacing, TextLines[i], FontSettings);
                }
            }
        }


        public void SetTextStyler(ITextStyler styler)
        {
            TextStyler = styler;
            ComputeStyles(); // recompute styles with the new styler
        }

        private TextStyles DefaultTextStyles = new TextStyles(new List<TextSettings>() { null }, new List<int>() { 0 }, new List<int>() { 0 }, new List<RelaFont>() { null });
        private void ComputeStyles()
        {
            if (SFont == null)
                return; // not inited
            ComputedStyles.Clear();

            if (TextStyler != null)
                TextStyler.PreStyling();
            for (int i = 0; i < TextLines.Count; i++)
            {
                // if we have no styler, use a default styles set
                if (TextStyler == null || TextLines[i].Length <= 0)
                {
                    DefaultTextStyles.Styles[0] = FontSettings;
                    DefaultTextStyles.Fonts[0] = SFont;
                    ComputedStyles.Add(DefaultTextStyles);
                }
                else
                {
                    ComputedStyles.Add(TextStyler.GetTextStyles(SFont, TextLines[i], FontSettings, i));
                }
            }
            if (TextStyler != null)
                TextStyler.PostStyling();
        }
    }
}
