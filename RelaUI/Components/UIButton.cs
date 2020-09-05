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
    public class UIButton : UIComponent
    {
        public int Width = 75;
        public int Height = 25;

        public bool HasBackground = true;
        public bool HoverBackgroundOnly = false; // if true, only renders background in hover state
        public Color BackgroundColor;
        public Color HoverColor;
        public Color PressedColor;
        public Color DisabledColor;

        public bool HasBorder = true;
        public Color BorderColor;
        public int BorderWidth;

        public Color TextColor;
        public Color HoverTextColor;
        public Color PressedTextColor;
        public Color DisabledTextColor;

        public bool StayPressed = false;


        public string Text;
        // if you change Text, or Width, or the font/fontsize, call ProcessText to reprocess it
        public bool TextSplitWords = false; // if this is true, allows words to be hyphenated
        public List<string> TextLines = new List<string>();
        // NOTE: to update font, updating above will not auto-update the font
        // you must re-init the label or do SFont = FontManager.ResolveFont(Font, FontSize)
        public RelaFont SFont;
        public TextSettings FontSettings = new TextSettings();
        public eUIJustify Justified = eUIJustify.CENTER;

        public int PressedTimer = 0;

        // if both are specified, only autowidth will be adhered to
        public bool AutoHeight = false;
        public bool AutoWidth = false;


        public UIButton(float x, float y, int w, int h, string text, string font = "", int? fontsize = null,
            bool autowidth = false, bool autoheight = false)
        {
            this.x = x;
            this.y = y;
            Width = w;
            Height = h;
            Text = text;
            FontSettings.FontName = font;
            FontSettings.FontSize = fontsize != null ? (int)fontsize : 0;

            AutoWidth = autowidth;
            AutoHeight = autoheight;
        }

        public eUIButtonState GetState(float dx, float dy, InputManager input)
        {
            if (!Enabled)
            {
                return eUIButtonState.DISABLED;
            }
            if (StayPressed)
            {
                if (Hovering(dx, dy, input))
                {
                    return eUIButtonState.HOVER;
                }
                return eUIButtonState.PRESSED;
            }
            if (Clicking(dx, dy, input) || PressedTimer > 0)
            {
                if (PressedTimer == 0)
                    PressedTimer = 5;
                return eUIButtonState.PRESSED;
            }
            if (Hovering(dx, dy, input))
            {
                return eUIButtonState.HOVER;
            }
            return eUIButtonState.NORMAL;
        }

        protected override void SelfInit()
        {
            FontSettings.Color = Style.ForegroundColor;
            BackgroundColor = Style.ButtonBackgroundColor;
            BorderColor = Style.ButtonAccent;
            BorderWidth = Style.ButtonBorderWidth;
            HoverColor = Style.ButtonHoverColor;
            PressedColor = Style.ButtonPressedColor;
            DisabledColor = Style.ButtonDisabledColor;
            HasBorder = Style.ButtonHasBorder;

            SFont = FontSettings.Init(Style);

            TextColor = Style.ButtonTextColor;
            HoverTextColor = Style.ButtonHoverTextColor;
            PressedTextColor = Style.ButtonPressedTextColor;
            DisabledTextColor = Style.ButtonDisabledTextColor;

            if (AutoWidth)
            {
                // in autowidth mode, make the button as wide as the text
                int twid = TextHelper.GetWidth(SFont, Text, FontSettings);
                Width = twid + 4 + (HasBorder ? BorderWidth * 2 : 0);
            }
            else if (AutoHeight) // note the else: can't have both
            {
                // in autoheight mode, make the button as high as the text
                int thei = TextHelper.DetermineHeightFromWidth(SFont, Text, FontSettings, Width);
                Height = thei + 4 + (HasBorder ? BorderWidth * 2 : 0);
            }

            ProcessText();
        }

        public void ProcessText()
        {
            TextLines = TextHelper.FitToBox(SFont, Text, Width, Height, FontSettings, TextSplitWords);
            while (TextLines.Count * SFont.LineSpacing > Height)
            {
                TextLines.RemoveAt(TextLines.Count - 1);
            }
        }

        public override int GetWidth()
        {
            return Width;
        }

        public override int GetHeight()
        {
            return Height;
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, Input.InputManager input, float dx, float dy)
        {
            if (PressedTimer > 0)
                PressedTimer--;

            eUIButtonState state = GetState(dx, dy, input);
            if (state == eUIButtonState.HOVER)
            {
                input.Cursor.TrySet(eCursorState.CLICKABLE);
            }
            if (state == eUIButtonState.PRESSED && Hovering(dx, dy, input) && PressedTimer > 0)
            {
                input.Cursor.TrySet(eCursorState.DOWN);
            }

            if (HasBackground)
            {
                Color bc = BackgroundColor;
                switch (state)
                {
                    case eUIButtonState.NORMAL:
                        break;
                    case eUIButtonState.HOVER:
                        bc = HoverColor;
                        break;
                    case eUIButtonState.DISABLED:
                        bc = DisabledColor;
                        break;
                    case eUIButtonState.PRESSED:
                        bc = PressedColor;
                        break;
                    default:
                        break;
                }
                if (!HoverBackgroundOnly || (state == eUIButtonState.HOVER && HoverBackgroundOnly))
                {
                    Draw.DrawRectangleHandle(sb, (int)dx, (int)dy, Width, Height, bc);
                }
            }

            if (TextLines.Count > 0)
            {
                Color tc = TextColor;
                switch (state)
                {
                    case eUIButtonState.NORMAL:
                        break;
                    case eUIButtonState.HOVER:
                        tc = HoverTextColor;
                        break;
                    case eUIButtonState.DISABLED:
                        tc = DisabledTextColor;
                        break;
                    case eUIButtonState.PRESSED:
                        tc = PressedTextColor;
                        break;
                    default:
                        break;
                }
                int textheight = SFont.LineSpacing * TextLines.Count;
                int diff = Height - textheight;
                int mod = 0;
                if (diff > 0)
                {
                    mod += diff / 2;
                }
                int textwidth = TextHelper.GetWidth(SFont, TextLines[0], FontSettings);
                int wdiff = Width - textwidth;
                int wmod = 0;
                if (wdiff > 0)
                {
                    if (Justified == eUIJustify.CENTER)
                        wmod += wdiff / 2;
                    else if (Justified == eUIJustify.LEFT)
                        wmod += BorderWidth * 2; // add some spacing so it doesn't go off the edge by default
                    else if (Justified == eUIJustify.RIGHT)
                        wmod += wdiff - BorderWidth * 2;
                }
                FontSettings.Color = tc;
                for (int i = 0; i < TextLines.Count; i++)
                {
                    Draw.DrawText(g, sb, SFont, wmod + dx, mod + dy + i * SFont.LineSpacing, TextLines[i], FontSettings);
                }
            }
            if (HasBorder)
                Draw.DrawRectangleHandleOutline(sb, (int)dx, (int)dy, Width, Height, BorderColor, BorderWidth);

        }
    }
}
