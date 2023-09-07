using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RelaUI.Components;
using RelaUI.DrawHandles;
using RelaUI.Text;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RelaUI
{
    public static class Draw
    {
        // use the following two exclusively.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawRectangleHandle(SpriteBatch s, int x, int y, int w, int h, Color color)
        { 
            s.Draw(DrawHandleStatic.Rect.RectTexture, new Rectangle(new Point(x, y), new Point(w, h)), color);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawRectangleHandleOutline(SpriteBatch s, int x, int y, int w, int h, Color color, int borderwidth = 1)
        {
            s.Draw(DrawHandleStatic.Rect.RectTexture, new Rectangle(new Point(x, y), new Point(borderwidth, h)), color);
            s.Draw(DrawHandleStatic.Rect.RectTexture, new Rectangle(new Point(x, y), new Point(w, borderwidth)), color);
            s.Draw(DrawHandleStatic.Rect.RectTexture, new Rectangle(new Point(x + w - borderwidth, y), new Point(borderwidth, h)), color);
            s.Draw(DrawHandleStatic.Rect.RectTexture, new Rectangle(new Point(x, y + h - borderwidth), new Point(w, borderwidth)), color);
        }

        public static void DrawRectangleHandle(DrawHandleRect rect, SpriteBatch s, float x, float y, Color color)
        {
            s.Draw(rect.RectTexture, new Vector2(x, y), color);
        }

        public static void DrawRectangleHandle(DrawHandleRect rect, SpriteBatch s, int x, int y, int w, int h, Color color)
        {
            s.Draw(rect.RectTexture, new Rectangle(new Point(x, y), new Point(w, h)), color);
        }

        public static DrawHandleRect CreateRectangle(GraphicsDevice g, Color c, int w, int h, bool outline = false, int outlinewidth = 1)
        {
            DrawHandleRect rect = new DrawHandleRect(g, w, h);
            if (!outline)
            {
                for (int i = 0; i < rect.Data.Length; i++)
                    rect.Data[i] = c;

                rect.RectTexture.SetData(rect.Data);
            }
            else
            {
                Color trans = new Color(0, 0, 0, 0);
                int rowpos = 0;
                for (int i = 0; i < rect.Data.Length; i++)
                {
                    if (i < outlinewidth * w || i > rect.Data.Length - outlinewidth * w
                        || rowpos < outlinewidth || rowpos >= w - outlinewidth)
                        rect.Data[i] = c;
                    else
                        rect.Data[i] = trans;
                    rowpos++;
                    if (rowpos >= w)
                        rowpos = 0;
                }

                rect.RectTexture.SetData(rect.Data);
            }
            return rect;
        }

        public static void DrawText(GraphicsDevice g, SpriteBatch s, RelaFont font, float x, float y, string text,
            TextSettings settings, int prevTabSum = 0)
        {

            if (settings.HasShadow)
            {
                /*s.DrawString(font,
                    text,
                    new Vector2(x + settings.ShadowDepth, y + settings.ShadowDepth),
                    (Color)settings.ShadowColor);*/
                DrawString(g, s, font, text, x + settings.ShadowDepth, y + settings.ShadowDepth, settings.ShadowColor, settings.Monospaced, settings.MonospaceSize, settings.TabWidth, prevTabSum);
            }

            /*s.DrawString(font,
                text,
                new Vector2(x, y),
                settings.Color);*/
            DrawString(g, s, font, text, x, y, settings.Color, settings.Monospaced, settings.MonospaceSize, settings.TabWidth, prevTabSum);
        }

        public static void DrawTextMultiStyles(GraphicsDevice g, SpriteBatch s, float x, float y, float dx, float dy, string text,
            List<TextSettings> settingsStyles, List<int> styleSwitchIndices, List<int> styleSwitchStyles, List<RelaFont> fonts,
            List<UIComponent> innerComponents, List<int> componentIndices)
        {
            // draw each chunk
            int totalWidth = 0;
            int compNumber = 0;
            int nextCompIndex = -1;
            if (componentIndices != null && componentIndices.Count > 0)
                nextCompIndex = componentIndices[0];

            for (int styleIndex = 0; styleIndex < styleSwitchIndices.Count; styleIndex++)
            {
                int pos = styleSwitchIndices[styleIndex];
                if (pos >= text.Length)
                    continue; // skip this pos, it's out of bounds
                TextSettings settings = settingsStyles[styleSwitchStyles[styleIndex]];
                RelaFont font = fonts[styleSwitchStyles[styleIndex]];
                if (font == null || settings == null)
                    continue; // don't render null styles

                while (nextCompIndex == pos)
                {
                    // render internal components
                    UIComponent nextComp = innerComponents[compNumber];
                    nextComp.x = totalWidth + x - dx;
                    nextComp.y = y - dy;
                    totalWidth += nextComp.GetWidth();
                    compNumber++;
                    nextCompIndex = -1;
                    if (componentIndices.Count > compNumber)
                        nextCompIndex = componentIndices[compNumber];
                }

                string subtext = null;
                if (styleIndex + 1 < styleSwitchIndices.Count)
                {
                    int nextpos = styleSwitchIndices[styleIndex + 1];
                    if (nextpos >= text.Length)
                        nextpos = text.Length;
                    if (nextpos <= pos)
                        continue; // skip this pos, the next one is right on top of it

                    subtext = text.Substring(pos, nextpos - pos);
                }
                else
                {
                    subtext = text.Substring(pos);
                }

                DrawText(g, s, font, totalWidth + x, y, subtext, settings, totalWidth);
                // unsafe justification: expect text to be sanitized by the time it reaches draw
                totalWidth += TextHelper.GetWidthUnsafe(font, subtext, settings, totalWidth);
            }
        }

        public static void DrawTextMultiStyles(GraphicsDevice g, SpriteBatch s, float x, float y, float dx, float dy, string text,
            TextStyles styles)
        {
            DrawTextMultiStyles(g, s, x, y, dx, dy, text, styles.Styles, styles.StyleSwitchIndices, styles.StyleSwitchStyles, styles.Fonts,
                styles.InnerComponents, styles.ComponentIndices);
        }

        public static void DrawString(GraphicsDevice g, SpriteBatch s, RelaFont font, string text, float x, float y, Color c,
            bool monospaced, int monosize, int tabWidth, int prevTabSum = 0)
        {
            if (!monospaced)
            {
                text = text.Replace("\t", TextHelper.Tab);
                if (font.IsDynamic)
                    s.DrawString(font.DFont, text, new Vector2(x, y), c);
                else
                    s.DrawString(font.SFont, text, new Vector2(x, y), c);
            }
            else
            {
                int sum = 0;
                int tabsize = (tabWidth / monosize) * monosize;
                for (int i = 0; i < text.Length; i++)
                {
                    char letter = text[i];
                    if (letter == '\t')
                    {
                        sum += DetermineTabDistance(tabsize, prevTabSum + sum);
                    }
                    else
                    {
                        if (font.IsDynamic)
                            s.DrawString(font.DFont, "" + letter, new Vector2(x + sum, y), c);
                        else
                            s.DrawString(font.SFont, "" + letter, new Vector2(x + sum, y), c);
                        sum += monosize;
                    }
                }
            }
        }

        public static int DetermineTabDistance(int tabWidth, int x)
        {
            int tabs = (x / tabWidth) + 1;
            return (tabs * tabWidth) - x;
        }
    }
}
