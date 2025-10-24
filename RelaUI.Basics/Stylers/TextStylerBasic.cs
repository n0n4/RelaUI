using Microsoft.Xna.Framework;
using RelaUI.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Basics.Stylers
{
    public class TextStylerBasic : ITextStyler
    {
        public enum EUnderlineType
        {
            None,
            Underline,
            Dashed,
            Redline
        }
        public enum EShadowType
        {
            None,
            Default,
            Dark
        }
        public enum EFontType
        {
            Latin,
            Emoji
        }
        // type, weight, italic, shadow, underline, color
        private
            Dictionary<EFontType,
            Dictionary<EFontWeight,
            Dictionary<bool,
            Dictionary<EShadowType,
            Dictionary<EUnderlineType,
            Dictionary<int, (int, TextSettings)>>>>>> SettingsTypes = new Dictionary<EFontType, Dictionary<EFontWeight, Dictionary<bool, Dictionary<EShadowType, Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>>>>>();

        private List<Color> Colors;

        private RelaFont LastFont;
        private TextSettings LastStyle;
        private List<TextSettings> AllStyles = new List<TextSettings>();
        private List<RelaFont> AllFonts = new List<RelaFont>();

        public string GlobalFontEmoji;

        public TextStylerBasic(List<Color> colors, string globalFontEmoji)
        {
            Colors = colors;
            GlobalFontEmoji = globalFontEmoji;
        }

        private (int, TextSettings) GetSettings(TextSettings baseStyle,
            EFontType type, EFontWeight weight, bool italic, EShadowType shadow,
            EUnderlineType underline, int colorIndex)
        {
            if (SettingsTypes.TryGetValue(type, out var weightDict))
            {
                if (weightDict.TryGetValue(weight, out var italicDict))
                {
                    if (italicDict.TryGetValue(italic, out var shadowDict))
                    {
                        if (shadowDict.TryGetValue(shadow, out var underlineDict))
                        {
                            if (underlineDict.TryGetValue(underline, out var colorDict))
                            {
                                if (colorDict.TryGetValue(colorIndex, out var foundSettings))
                                {
                                    return foundSettings;
                                }
                            }
                            else
                            {
                                underlineDict.Add(underline, new Dictionary<int, (int, TextSettings)>());
                            }
                        }
                        else
                        {
                            shadowDict.Add(shadow, new Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>());
                            shadowDict[shadow].Add(underline, new Dictionary<int, (int, TextSettings)>());
                        }
                    }
                    else
                    {
                        italicDict.Add(italic, new Dictionary<EShadowType, Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>>());
                        italicDict[italic].Add(shadow, new Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>());
                        italicDict[italic][shadow].Add(underline, new Dictionary<int, (int, TextSettings)>());
                    }
                }
                else
                {
                    weightDict.Add(weight, new Dictionary<bool, Dictionary<EShadowType, Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>>>());
                    weightDict[weight].Add(italic, new Dictionary<EShadowType, Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>>());
                    weightDict[weight][italic].Add(shadow, new Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>());
                    weightDict[weight][italic][shadow].Add(underline, new Dictionary<int, (int, TextSettings)>());
                }
            }
            else
            {
                SettingsTypes.Add(type, new Dictionary<EFontWeight, Dictionary<bool, Dictionary<EShadowType, Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>>>>());
                SettingsTypes[type].Add(weight, new Dictionary<bool, Dictionary<EShadowType, Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>>>());
                SettingsTypes[type][weight].Add(italic, new Dictionary<EShadowType, Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>>());
                SettingsTypes[type][weight][italic].Add(shadow, new Dictionary<EUnderlineType, Dictionary<int, (int, TextSettings)>>());
                SettingsTypes[type][weight][italic][shadow].Add(underline, new Dictionary<int, (int, TextSettings)>());
            }

            TextSettings newStyle = baseStyle.Clone();
            
            // set typeset
            if (type == EFontType.Latin)
            {
                newStyle.FontTypeOverride = null;
            }
            else if (type == EFontType.Emoji)
            {
                newStyle.FontTypeOverride = GlobalFontEmoji;
            }

            // set weight
            newStyle.Weight = weight;

            // set italic
            newStyle.Italic = italic;

            // set shadow
            if (shadow == EShadowType.None)
            {
                newStyle.HasShadow = false;
            }
            else
            if (shadow == EShadowType.Default)
            {
                newStyle.HasShadow = true;
            }
            else if (shadow == EShadowType.Dark)
            {
                newStyle.ShadowDepth = 1;
                if (baseStyle.FontSize > 11)
                    newStyle.ShadowDepth = 2;
                newStyle.HasShadow = true;
                newStyle.ShadowColor = Color.DarkGray;
            }

            // set underline
            // TODO

            // set color
            if (colorIndex >= 0)
            {
                newStyle.Color = Colors[colorIndex];
            }

            AllStyles.Add(newStyle);
            AllFonts.Add(null);
            var result = (AllStyles.Count - 1, newStyle);
            SettingsTypes[type][weight][italic][shadow][underline].Add(colorIndex, result);
            return result;
        }

        public TextStyles GetTextStyles(RelaFont font, string text, TextSettings baseStyle, int lineNumber)
        {
            if (font != LastFont || baseStyle != LastStyle)
            {
                LastFont = font;
                for (int i = 0; i < AllFonts.Count; i++)
                    AllFonts[i] = null;
            }
            if (baseStyle != LastStyle)
            {
                AllStyles.Clear();
                SettingsTypes.Clear();
                LastStyle = baseStyle;
            }
            List<int> switches = new List<int>() { 0 };
            List<int> switchStyles = new List<int>() { 0 };

            // first style is always regular
            var firstStyleResult = GetSettings(baseStyle, EFontType.Latin, EFontWeight.Regular, false, EShadowType.None, EUnderlineType.None, -1);
            if (firstStyleResult.Item1 != 0)
                throw new Exception("Default Style no longer Index 0 (this should never happen)");
            if (AllFonts[0] == null)
                AllFonts[0] = TextHelper.GetBestFont(font, firstStyleResult.Item2);
            var emptyStyleResult = GetSettings(baseStyle, EFontType.Latin, EFontWeight.Regular, false, EShadowType.None, EUnderlineType.None, -2);
            if (emptyStyleResult.Item1 != 1)
                throw new Exception("Empty Style no longer Index 1 (this should never happen)");
            AllFonts[1] = null;

            bool bolding = false;
            bool italicing = false;
            bool emojibuffer = false;

            int cur = 0;
            EFontType currentType = EFontType.Latin;
            EFontWeight currentWeight = EFontWeight.Regular;
            bool currentItalics = false;
            EShadowType currentShadow = EShadowType.None;
            EUnderlineType currentUnderline = EUnderlineType.None;
            int currentColor = -1;

            void changeType(int index, EFontType type)
            {
                currentType = type;
                var result = GetSettings(baseStyle, currentType, currentWeight, currentItalics, currentShadow, currentUnderline, currentColor);
                change(index, result.Item1);
            }
            void changeWeight(int index, EFontWeight weight)
            {
                currentWeight = weight;
                var result = GetSettings(baseStyle, currentType, weight, currentItalics, currentShadow, currentUnderline, currentColor);
                change(index, result.Item1);
            }
            void changeItalics(int index, bool italics)
            {
                currentItalics = italics;
                var result = GetSettings(baseStyle, currentType, currentWeight, italics, currentShadow, currentUnderline, currentColor);
                change(index, result.Item1);
            }
            void changeShadow(int index, EShadowType shadow)
            {
                currentShadow = shadow;
                var result = GetSettings(baseStyle, currentType, currentWeight, currentItalics, shadow, currentUnderline, currentColor);
                change(index, result.Item1);
            }
            void changeUnderline(int index, EUnderlineType underline)
            {
                currentUnderline = underline;
                var result = GetSettings(baseStyle, currentType, currentWeight, currentItalics, currentShadow, underline, currentColor);
                change(index, result.Item1);
            }
            void changeColor(int index, int color)
            {
                currentColor = color;
                var result = GetSettings(baseStyle, currentType, currentWeight, currentItalics, currentShadow, currentUnderline, color);
                change(index, result.Item1);
            }
            void change(int index, int type)
            {
                cur = type;
                switches.Add(index);
                switchStyles.Add(cur);
                if (AllFonts[cur] == null && currentColor != -2)
                    AllFonts[cur] = TextHelper.GetBestFont(font, AllStyles[cur]);
            }

            text = text.ToLower();

            int oldColor = -1;
            char lastc = ' ';
            bool undoInvis = false;
            for (int i = 0; i < text.Length; i++)
            {
                bool keepInvis = false;
                char c = text[i];

                if (char.IsLowSurrogate(c))
                {
                    lastc = c;
                    continue;
                }

                if (c == '\\')
                {
                    if (lastc == '\\' && undoInvis)
                    {
                        changeColor(i, oldColor);
                        undoInvis = false;
                    }
                    else
                    {
                        if (currentColor != -2)
                            oldColor = currentColor;
                        changeColor(i, -2);
                        undoInvis = true;
                    }
                    lastc = c;
                    continue; // ignore escaped characters
                }

                if (lastc == '\\')
                {
                    if (undoInvis)
                    {
                        changeColor(i, oldColor);
                        undoInvis = false;
                    }
                    lastc = c;
                    continue; // ignore escaped characters
                }

                // check alternate typesets
                char nextc = ' ';
                if (i < text.Length - 1)
                    nextc = text[i + 1];
                if (currentType != EFontType.Emoji && CharacterHelper.IsEmoji(c, nextc))
                {
                    changeType(i, EFontType.Emoji);
                }
                else if (currentType == EFontType.Emoji && !CharacterHelper.IsEmoji(c, nextc))
                {
                    changeType(i, EFontType.Latin);
                }

                if (c == '*')
                {
                    bolding = !bolding;
                    keepInvis = true;
                    if (!undoInvis)
                    {
                        if (currentColor != -2)
                            oldColor = currentColor;
                        currentColor = -2;
                    }
                    undoInvis = true;
                    if (bolding)
                    {
                        changeWeight(i, EFontWeight.Bold);
                    } 
                    else
                    {
                        changeWeight(i, EFontWeight.Regular);
                    }
                }

                if (c == '/')
                {
                    italicing = !italicing;
                    keepInvis = true;
                    if (!undoInvis)
                    {
                        if (currentColor != -2)
                            oldColor = currentColor;
                        currentColor = -2;
                    }
                    undoInvis = true;
                    changeItalics(i, italicing);
                }

                if (undoInvis && !keepInvis)
                {
                    undoInvis = false;
                    changeColor(i, oldColor);
                }
                lastc = c;
            }

            return new TextStyles(AllStyles, switches, switchStyles, AllFonts);
        }


        public void PreStyling()
        {

        }

        public void PostStyling()
        {

        }
    }
}
