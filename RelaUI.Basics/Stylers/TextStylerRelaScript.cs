using Microsoft.Xna.Framework;
using RelaUI.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Basics.Stylers
{
    public class TextStylerRelaScript : ITextStyler
    {
        private enum eStyleType
        {
            BASE = 0,
            VAR = 1,
            FUNC = 2,
            LIB = 3,
            DEFN = 4,
            OBJ = 5,
            KEY = 6,
            NUM = 7,
            STRING = 8,
            COMMENT = 9,
            ARG = 10,
            CAST = 11,
        }

        public static Color Orange = new Color(242, 143, 60, 255);
        public static Color DarkOrange = new Color(200, 97, 13, 255);
        public static Color Red = new Color(195, 0, 0, 255);
        public static Color DarkRed = new Color(135, 30, 30, 255);
        public static Color Blue = new Color(116, 157, 200, 255);
        public static Color DarkBlue = new Color(96, 137, 180, 255);
        public static Color Pink = new Color(255, 117, 181, 255);
        public static Color Grey = new Color(128, 128, 128, 255);

        public Color? ColorVar = null;// Orange;
        public bool ShadowVar = true;

        public Color? ColorFunc = Orange;
        public bool ShadowFunc = false;
        public Color? ColorLib = Red;
        public bool ShadowLib = false;

        public Color? ColorDefn = DarkBlue;
        public bool ShadowDefn = false;
        public Color? ColorObj = Blue;
        public bool ShadowObj = true;

        public Color? ColorKey = Pink;
        public bool ShadowKey = false;

        public Color? ColorNum = Pink;
        public bool ShadowNum = false;

        public Color? ColorString = DarkOrange;
        public bool ShadowString = false;

        public Color? ColorComment = Grey;
        public bool ShadowComment = false;

        public Color? ColorArg = null;// Orange;
        public bool ShadowArg = true;

        public Color? ColorCast = Pink;// Orange;
        public bool ShadowCast = false;

        public TextStyles GetTextStyles(RelaFont font, string text, TextSettings baseStyle)
        {
            // note: expects a monospaced basestyle
            int shadowDepth = 1;
            if (baseStyle.FontSize > 11)
                shadowDepth = 2;

            void applyStyle(TextSettings style, Color? color, bool isShadow)
            {
                if (color == null)
                    return;
                if (isShadow)
                {
                    style.HasShadow = true;
                    style.ShadowDepth = shadowDepth;
                    style.ShadowColor = color.Value;
                }
                else
                {
                    style.Color = color.Value;
                }
            }

            TextSettings stylevar = baseStyle.Clone();
            applyStyle(stylevar, ColorVar, ShadowVar);

            TextSettings stylefunc = baseStyle.Clone();
            applyStyle(stylefunc, ColorFunc, ShadowFunc);

            TextSettings stylelib = baseStyle.Clone();
            applyStyle(stylelib, ColorLib, ShadowLib);

            TextSettings styledefn = baseStyle.Clone();
            applyStyle(styledefn, ColorDefn, ShadowDefn);

            TextSettings styleobj = baseStyle.Clone();
            applyStyle(styleobj, ColorObj, ShadowObj);

            TextSettings stylekey = baseStyle.Clone();
            applyStyle(stylekey, ColorKey, ShadowKey);

            TextSettings stylenum = baseStyle.Clone();
            applyStyle(stylenum, ColorNum, ShadowNum);

            TextSettings stylestring = baseStyle.Clone();
            applyStyle(stylestring, ColorString, ShadowString);

            TextSettings stylecomment = baseStyle.Clone();
            applyStyle(stylecomment, ColorComment, ShadowComment);
            stylecomment.Monospaced = false;

            TextSettings stylearg = baseStyle.Clone();
            applyStyle(stylearg, ColorArg, ShadowArg);

            TextSettings stylecast = baseStyle.Clone();
            applyStyle(stylecast, ColorCast, ShadowCast);

            List<TextSettings> styles = new List<TextSettings>
            {
                baseStyle,      // 0
                stylevar,       // 1
                stylefunc,      // 2
                stylelib,       // 3
                styledefn,      // 4
                styleobj,       // 5
                stylekey,       // 6
                stylenum,       // 7
                stylestring,    // 8
                stylecomment,   // 9
                stylearg,       // 10
                stylecast,       // 10
            };

            List<RelaFont> fonts = new List<RelaFont>();
            for (int i = 0; i < styles.Count; i++)
                fonts.Add(TextHelper.GetBestFont(font, styles[i]));

            List<int> switches = new List<int>() { 0 };
            List<int> switchStyles = new List<int>() { 0 };

            eStyleType cur = eStyleType.BASE;
            bool singlequotestring = false;
            int keybypass = 0;

            void change(int index, eStyleType type)
            {
                cur = type;
                switches.Add(index);
                switchStyles.Add((int)cur);
            }
            bool safecheck(int index, string word)
            {
                if (index + word.Length > text.Length)
                    return false;
                for (int i = 0; i < word.Length; i++)
                {
                    if (text[index + i] != word[i])
                        return false;
                }
                return true;
            }
            bool keycheck(int index, string word)
            {
                if (safecheck(index, word))
                {
                    keybypass = word.Length;
                    change(index, eStyleType.KEY);
                    return true;
                }
                return false;
            }


            text = text.ToLower();

            char lastc = ' ';
            for (int i = 0; i < text.Length; i++)
            {
                if (keybypass > 0)
                {
                    keybypass--;
                    if (keybypass == 0)
                    {
                        change(i, eStyleType.BASE);
                    }
                    continue;
                }

                char c = text[i];

                if (cur == eStyleType.NUM && !IsNumber(c))
                {
                    change(i, eStyleType.BASE);
                }

                if (cur == eStyleType.STRING)
                {
                    if ((singlequotestring && c == '\'') || (!singlequotestring && c == '"'))
                    {
                        change(i + 1, eStyleType.BASE);
                        singlequotestring = false;
                    }
                }
                else
                {
                    bool lastwasletter = Char.IsLetter(lastc);
                    if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
                    {
                        change(i, eStyleType.COMMENT);
                        break; // after a comment, no more is accepted
                    }
                    else if (c == '"')
                    {
                        change(i, eStyleType.STRING);
                    }
                    else if (c == '\'')
                    {
                        singlequotestring = true;
                        change(i, eStyleType.STRING);
                    }
                    else if (!lastwasletter && c == 'v' && i + 1 < text.Length && text[i + 1] == ':')
                    {
                        change(i, eStyleType.VAR);
                    }
                    else if (!lastwasletter && c == 'f' && i + 1 < text.Length && text[i + 1] == ':')
                    {
                        change(i, eStyleType.FUNC);
                    }
                    else if (!lastwasletter && c == 'l' && i + 1 < text.Length && text[i + 1] == ':')
                    {
                        change(i, eStyleType.LIB);
                    }
                    else if (!lastwasletter && c == 'd' && i + 1 < text.Length && text[i + 1] == ':')
                    {
                        change(i, eStyleType.DEFN);
                    }
                    else if (!lastwasletter && c == 'o' && i + 1 < text.Length && text[i + 1] == ':')
                    {
                        change(i, eStyleType.OBJ);
                    }
                    else if (!lastwasletter && c == 'a' && i + 1 < text.Length && text[i + 1] == ':')
                    {
                        change(i, eStyleType.ARG);
                    }
                    else if (!lastwasletter && c == 'c' && i + 1 < text.Length && text[i + 1] == ':')
                    {
                        change(i, eStyleType.CAST);
                    }
                    else if (cur != eStyleType.NUM && c == '-' && i + 1 < text.Length && IsNumber(text[i + 1]))
                    {
                        // special case for negative numbers
                        change(i, eStyleType.NUM);
                    }
                    else if (c == ' ' || IsOperator(c))
                    {
                        change(i, eStyleType.BASE);
                    }
                    else if (cur == eStyleType.BASE && IsNumber(c))
                    {
                        change(i, eStyleType.NUM);
                    }
                    else if (c == '(' || c == ')' || c == '{' || c == '}' || c == '[' || c == ']')
                    {
                        change(i, eStyleType.BASE);
                    }
                    else if (cur == eStyleType.BASE || cur == eStyleType.NUM)
                    {
                        // checks for keywords, only if we're on base or num
                        if (keycheck(i, "if")
                            || keycheck(i, "while")
                            || keycheck(i, "for")
                            || keycheck(i, "else")
                            || keycheck(i, "return")
                            || keycheck(i, "defn")
                            || keycheck(i, "object")
                            || keycheck(i, "import")
                            || keycheck(i, "anon")
                            || keycheck(i, "true")
                            || keycheck(i, "false")
                            || keycheck(i, "free"))
                        {
                            // don't actually need to do anything here
                            // the OR here is just so that this check stops
                            // checking further words once one is matched
                        }
                    }
                }

                lastc = c;
            }

            return new TextStyles(styles, switches, switchStyles, fonts);
        }


        private bool IsOperator(char c)
        {
            return c == '+' || c == '-' || c == '/' || c == '*'
                || c == '^' || c == '%' || c == '←' || c == '['
                || c == '©'
                || c == '=' || c == 'ⱻ' || c == '˭'
                || c == '!' || c == '>' || c == '<' || c == '|' || c == '&'
                || c == '≠' || c == '≥' || c == '≤' || c == '‖' || c == 'Ѯ'
                || c == '.'
                || c == '○' || c == 'ᴥ' || c == 'є' || c == 'ᵜ'
                || c == 'ꝑ' || c == '₥' || c == '×' || c == '÷'
                || c == 'Ꞧ'
                || c == '¶' || c == '⌂'
                || c == 'ꜛ'
                || c == '□';
        }

        private bool IsNumber(char c)
        {
            return c == '0' || c == '1' || c == '2' || c == '3'
                || c == '4' || c == '5' || c == '6' || c == '7'
                || c == '8' || c == '9'
                || c == '.' || c == '-';
        }
    }
}
