using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelaUI.Text
{
    public class RelaFont
    {
        public bool IsDynamic = false;

        public SpriteFont SFont;
        private DynamicSpriteFont DFontField;
        public Dictionary<string, string> ReplaceChecks = new Dictionary<string, string>();
        private HashSet<string> SafeChecks = new HashSet<string>() { "\t" };

        public Dictionary<string, RelaFont> RelatedFonts = new Dictionary<string, RelaFont>();

        public int Size;

        public int LineSpacing
        {
            get
            {
                if (IsDynamic)
                    return (int)DFont.Size;
                return SFont.LineSpacing;
            }
        }

        public DynamicSpriteFont DFont
        {
            get
            {
                DFontField.Size = Size;
                return DFontField;
            }
            set
            {
                DFontField = value;
            }
        }

        private StringBuilder SanitizeBuilder = new StringBuilder();
        public string SanitizeString(string text)
        {
            StringBuilder sb = SanitizeBuilder;
            sb.Clear();
            AppendSanitizeString(text, sb);
            return sb.ToString();
        }
        public void AppendSanitizeString(string text, StringBuilder sb)
        {
            char previousc = ' ';
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // note: high surrogate = start of a combined
                // low surrogate = end of a combined
                // don't check width for high surrogates
                // don't check width for low surrogates alone
                string check = string.Empty;
                if (!char.IsHighSurrogate(c))
                {
                    if (char.IsLowSurrogate(c))
                    {
                        check = TextHelper.GetSurrogatesToString(previousc, c);
                    }
                    else
                    {
                        check = TextHelper.GetCharToString(c);
                    }
                    //if (check[0] == '\t')
                    //    check = TextHelper.Tab;
                    if (ReplaceChecks.TryGetValue(check, out string newCheck))
                        check = newCheck;
                }

                if (IsDynamic)
                {
                    while (ReplaceChecks.TryGetValue(check, out string newCheck))
                    {
                        if (newCheck == check)
                            throw new Exception("Font has recursive replace-check: " + check + " -> " + newCheck);
                        check = newCheck;
                    }

                    if (!SafeChecks.Contains(check))
                    {
                        try
                        {
                            DFont.MeasureString(check);
                            SafeChecks.Add(check);
                        }
                        catch (Exception e)
                        {
                            if (e.Message.StartsWith("Could not find glyph for codepoint "))
                            {
                                string codepointString = e.Message.Substring("Could not find glyph for codepoint ".Length);
                                int codepoint = int.Parse(codepointString);
                                ReplaceChecks.TryAdd("" + (char)codepoint, " ");
                                check = "";
                            }
                            else
                            {
                                throw new Exception("Unrecognized Font Error", e);
                            }
                        }
                    }

                    sb.Append(check);
                }
                else
                {
                    while (ReplaceChecks.TryGetValue(check, out string newCheck))
                    {
                        if (newCheck == check)
                            throw new Exception("Font has recursive replace-check: " + check + " -> " + newCheck);
                        check = newCheck;
                    }

                    for (int o = 0; o < check.Length; o++)
                    {
                        // todo: does this break surrogates?
                        if (SFont.Characters.Contains(check[o]))
                            sb.Append(check[o]);
                    }
                }

                previousc = c;
            }
        }

        private StringBuilder MultiStyleSanitizeStringBuilder = new StringBuilder();
        public string SanitizeStringMultiStyle(string text, TextStyles styles)
        {
            return SanitizeStringMultiStyle(text, styles.StyleSwitchIndices, styles.StyleSwitchStyles, styles.Styles);
        }

        public string SanitizeStringMultiStyle(string text, List<int> switchIndices,
            List<int> switchStyles, List<TextSettings> styles)
        {
            StringBuilder sb = MultiStyleSanitizeStringBuilder;
            sb.Clear();
            for (int i = 0; i < switchIndices.Count; i++)
            {
                // sanitize each section individually based on font overrides
                // find best font for this style
                int pos = switchIndices[i];
                int nextpos = text.Length;
                if (i < switchIndices.Count - 1)
                    nextpos = switchIndices[i + 1];
                if (pos == nextpos)
                    continue; // overlapping styles, skip it
                string subtext = text.Substring(pos, nextpos - pos);
                TextSettings settings = styles[switchStyles[i]];
                RelaFont font = TextHelper.GetBestFont(this, settings);
                font.AppendSanitizeString(subtext, sb);
            }
            return sb.ToString();
        }

        public Vector2 MeasureString(string text, bool safe)
        {
            if (!safe)
            {
                if (IsDynamic)
                    return DFont.MeasureString(text);
                return SFont.MeasureString(text);
            }

            while (true)
            {
                try
                {
                    if (IsDynamic)
                        return DFont.MeasureString(text);
                    return SFont.MeasureString(text);
                }
                catch (Exception e)
                {
                    // Message "Could not find glyph for codepoint 8594"   string
                    if (e.Message.StartsWith("Could not find glyph for codepoint "))
                    {
                        string codepointString = e.Message.Substring("Could not find glyph for codepoint ".Length);
                        int codepoint = int.Parse(codepointString);
                        ReplaceChecks.Add("" + (char)codepoint, " ");
                        text = SanitizeString(text);
                    }
                    else
                    {
                        throw new Exception("Unrecognized Font Error", e);
                    }
                }
            }
        }
    }
}
