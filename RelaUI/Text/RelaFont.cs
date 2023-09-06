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
            char previousc = ' ';
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // note: high surrogate = start of a combined
                // low surrogate = end of a combined
                // don't check width for high surrogates alone
                // don't check width for low surrogates
                string check = string.Empty;
                if (!char.IsLowSurrogate(c))
                {
                    if (char.IsHighSurrogate(c))
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
                                ReplaceChecks.Add("" + (char)codepoint, "?");
                                check = "?";
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
                        ReplaceChecks.Add("" + (char)codepoint, "?");
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
