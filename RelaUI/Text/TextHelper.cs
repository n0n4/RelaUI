using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Text
{
    public static class TextHelper
    {
        public const string Tab = "    ";

        private static Dictionary<char, string> CharToString = new Dictionary<char, string>();
        private static Dictionary<char, Dictionary<char, string>> SurrogatesToString = new Dictionary<char, Dictionary<char, string>>();

        public static Tuple<int, int> GetSize(RelaFont font, RenderedText rendered, TextSettings settings)
        {
            string text = rendered.Render(font);
            if (font.IsDynamic)
            {
                if (settings != null && settings.Monospaced)
                {
                    return new Tuple<int, int>(text.Length * settings.MonospaceSize, (int)Math.Ceiling(font.DFont.MeasureString(text).Y));
                }
                return new Tuple<int, int>((int)Math.Ceiling(font.DFont.MeasureString(text).X), (int)Math.Ceiling(font.DFont.MeasureString(text).Y));
            }

            if (settings != null && settings.Monospaced)
            {
                return new Tuple<int, int>(text.Length * settings.MonospaceSize, (int)Math.Ceiling(font.SFont.MeasureString(text).Y));
            }
            return new Tuple<int, int>((int)Math.Ceiling(font.SFont.MeasureString(text).X), (int)Math.Ceiling(font.SFont.MeasureString(text).Y));
        }

        public static int GetWidth(RelaFont font, RenderedText rendered, TextSettings settings, int prevTabDistance = 0)
        {
            string text = rendered.Render(font);
            return GetWidth(font, text, settings, prevTabDistance);
        }
        public static int GetWidthUnsafe(RelaFont font, string text, TextSettings settings, int prevTabDistance = 0)
        {
            return GetWidth(font, text, settings, prevTabDistance);
        }
        private static int GetWidth(RelaFont font, string text, TextSettings settings, int prevTabDistance = 0)
        {
            if (settings != null && settings.Monospaced)
            {
                int sum = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c == '\t')
                    {
                        sum += Draw.DetermineTabDistance((settings.TabWidth / settings.MonospaceSize) * settings.MonospaceSize, prevTabDistance + sum);
                    }
                    else
                    {
                        sum += settings.MonospaceSize;
                    }
                }
                return sum;// text.Length * settings.MonospaceSize;
            }
            if (text.Contains('\t'))
            {
                text = text.Replace("\t", Tab);
            }
            if (font.IsDynamic)
                return (int)Math.Ceiling(font.DFont.MeasureString(text).X);
            return (int)Math.Ceiling(font.SFont.MeasureString(text).X);
        }

        public static int GetWidthMultiStyles(RelaFont font, RenderedText rendered,
            TextSettings[] settingsStyles, int[] styleSwitchIndices, int[] styleSwitchStyles,
            int? start = null, int? length = null)
        {
            string text = rendered.Render(font);
            int totalWidth = 0;
            for (int styleIndex = 0; styleIndex < styleSwitchIndices.Length; styleIndex++)
            {
                int pos = styleSwitchIndices[styleIndex];
                if (start != null && pos < start)
                {
                    pos = start.Value;
                    if (styleIndex + 1 < styleSwitchIndices.Length && pos >= styleSwitchIndices[styleIndex + 1])
                        continue; // start pos is not in this styleswitch
                }
                if (pos >= text.Length)
                    continue; // skip this pos, it's out of bounds
                TextSettings settings = settingsStyles[styleSwitchStyles[styleIndex]];

                string subtext = null;
                int nextpos = text.Length;
                if (styleIndex + 1 < styleSwitchIndices.Length)
                {
                    nextpos = styleSwitchIndices[styleIndex + 1];
                    if (nextpos >= text.Length)
                        nextpos = text.Length;
                }
                if (length != null)
                {
                    length -= (nextpos - pos);
                    if (length < 0)
                    {
                        nextpos += length.Value;
                    }
                }
                if (nextpos <= pos)
                    continue; // skip this pos, the next one is right on top of it

                subtext = text.Substring(pos, nextpos - pos);
                
                totalWidth += TextHelper.GetWidth(font, subtext, settings, totalWidth);
                if (length != null && length <= 0)
                    break;
            }
            return totalWidth;
        }

        public static int GetWidthMultiStyles(RelaFont font, RenderedText rendered,
            TextStyles styles, int? start = null, int? length = null)
        {
            return GetWidthMultiStyles(font, rendered, styles.Styles, styles.StyleSwitchIndices, styles.StyleSwitchStyles, start, length);
        }

        public static int GetHeight(RelaFont font, RenderedText rendered, TextSettings settings)
        {
            string text = rendered.Render(font);
            return GetHeight(font, text, settings);
        }
        private static int GetHeight(RelaFont font, string text, TextSettings settings)
        {
            if (font.IsDynamic)
                return (int)Math.Ceiling(font.DFont.MeasureString(text).Y);
            return (int)Math.Ceiling(font.SFont.MeasureString(text).Y);
        }

        public static int DetermineHeightFromWidth(RelaFont font, RenderedText rendered, TextSettings settings, int width, bool splitwords = false)
        {
            string text = rendered.Render(font);
            List<string> TextLines = TextHelper.FitToBox(font, text, width, 0, settings, splitwords);
            return TextLines.Count * GetHeight(font, text, settings);
        }

        public static string SanitizeString(SpriteFont font, string text)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (font.Characters.Contains(text[i]))
                    sb.Append(text[i]);
            }
            return sb.ToString();
        }

        public static char SanitizeChar(SpriteFont font, char c)
        {
            if (!font.Characters.Contains(c))
                return ' ';
            return c;
        }

        // TODO: this doesn't use the height at all, you need to manually remove the overflow lines afterwards
        // this should be refactored so that the height is an optional parameter and if it is passed in
        // it will automatically remove overflow lines
        public static List<string> FitToBox(RelaFont font, RenderedText render, int w, int h, TextSettings settings, bool splitwords = false)
        {
            string text = render.Render(font);
            return FitToBox(font, text, w, h, settings, splitwords);
        }
        private static List<string> FitToBox(RelaFont font, string text, int w, int h, TextSettings settings, bool splitwords = false)
        {
            List<string> outs = new List<string>();
            if (text.Length <= 1)
            {
                outs.Add(text);
                return outs;
            }
            int currentwidth = 0;
            int cstart = 0;
            int lastspace = 0;
            int c = 0;
            while (c < text.Length)
            {
                if (text[c] == ' ')
                {
                    lastspace = c;
                }

                // note: high surrogate = start of a combined
                // low surrogate = end of a combined
                // don't check width for high surrogates alone
                // don't check width for low surrogates
                string check;
                int width = 0;
                if (!char.IsLowSurrogate(text[c]))
                {
                    if (char.IsHighSurrogate(text[c]))
                    {
                        check = text.Substring(c, 2);
                    }
                    else
                    {
                        check = text.Substring(c, 1);
                    }
                    if (check[0] == '\t')
                    {
                        width = GetWidth(font, Tab, settings);
                    }
                    else if (check[0] != '\n')
                    {
                        width = GetWidth(font, check, settings);
                    }
                }

                if (text[c] == '\n')
                {
                    // force new line
                    if (c - cstart <= 0) // case where you have two \n in a row
                        outs.Add("");
                    else
                        outs.Add(text.Substring(cstart, c - cstart));
                    cstart = c + 1;
                    currentwidth = 0;
                }
                else if (currentwidth != 0 && currentwidth + width > w)
                {
                    // we're in excess, so we need to move to a new line
                    if (splitwords || lastspace <= cstart) // failsafe, if the word is too long split it anyways
                    {
                        string line = string.Empty;
                        if ((c - 2) - cstart > 0)
                        {
                            line = text.Substring(cstart, (c - 2) - cstart);// + "-"
                            cstart = c - 2;
                        }
                        else
                        {
                            line = text.Substring(cstart, 1);
                            cstart = c - 1;
                        }
                        if (line[line.Length - 1] != ' ')
                        {
                            line += "-";
                        }
                        outs.Add(line);

                        currentwidth = 0;
                        if (c - cstart > 0)
                        {
                            currentwidth = GetWidth(font, text.Substring(cstart, c - cstart), settings);
                        }
                    }
                    else
                    {
                        outs.Add(text.Substring(cstart, lastspace - cstart));
                        cstart = lastspace + 1;
                        currentwidth = 0;
                        if (c - cstart > 0)
                        {
                            currentwidth = GetWidth(font, text.Substring(cstart, c - cstart), settings);
                        }
                    }
                }
                else
                {
                    currentwidth += width;
                }
                c++;
            }
            outs.Add(text.Substring(cstart));
            return outs;
        }

        public static string GetCharToString(char c)
        {
            if (CharToString.TryGetValue(c, out string result))
                return result;
            string converted = "" + c;
            CharToString.Add(c, converted);
            return converted;
        }

        public static string GetSurrogatesToString(char c1, char c2)
        {
            if (SurrogatesToString.TryGetValue(c1, out var dict))
            {
                if (dict.TryGetValue(c2, out string result))
                    return result;
                string combined = "" + c1 + c2;
                dict.Add(c2, combined);
                return combined;
            }

            string c1c2 = "" + c1 + c2;
            Dictionary<char, string> newDict = new Dictionary<char, string>();
            newDict.Add(c2, c1c2);
            SurrogatesToString.Add(c1, newDict);
            return c1c2;
        }
    }
}