using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using RelaUI.Text;
using System.IO;
using RelaUI.Utilities;
using Newtonsoft.Json;

namespace RelaUI.Styles
{
    public class UIStyle
    {
        public static Color DefaultBackgroundColor = new Color(31, 31, 31);
        public static Color DefaultSecondaryColor = new Color(46, 46, 46);
        public static Color DefaultAccentColor = new Color(56, 56, 56);
        public static Color DefaultForegroundColor = new Color(250, 250, 250);

        public string CursorStyle = "C1";
        public Color CursorColor = Color.Lime;

        public Color BackgroundColor = DefaultBackgroundColor;
        public Color SecondaryBackgroundColor = DefaultSecondaryColor;
        public Color BackgroundAccent = DefaultAccentColor; // used e.g. for panel borders
        public Color ForegroundColor = DefaultForegroundColor; // text color generally
        public Color SecondaryTextColor = Color.Gray; // non-primary text (like placeholders)

        public int BorderWidth = 2;

        // button style
        public Color ButtonBackgroundColor = DefaultSecondaryColor;
        public Color ButtonHoverColor = new Color(61, 61, 61);
        public Color ButtonPressedColor = DefaultAccentColor;
        public Color ButtonDisabledColor = DefaultAccentColor;

        public bool ButtonHasBorder = true; // used for button border
        public int ButtonBorderWidth = 2;
        public Color ButtonAccent = DefaultAccentColor;

        public Color ButtonTextColor = new Color(230, 230, 230);
        public Color ButtonHoverTextColor = DefaultForegroundColor;
        public Color ButtonPressedTextColor = DefaultForegroundColor;
        public Color ButtonDisabledTextColor = new Color(200, 200, 200);

        [JsonIgnore]
        public FontManager FontManager;

        public UIStyle()
        {

        }

        public UIStyle(UIStyle style)
        {
            CursorStyle = style.CursorStyle;
            CursorColor = style.CursorColor;

            BackgroundColor = style.BackgroundColor;
            SecondaryBackgroundColor = style.SecondaryBackgroundColor;
            BackgroundAccent = style.BackgroundAccent;
            ForegroundColor = style.ForegroundColor;
            SecondaryTextColor = style.SecondaryTextColor;

            BorderWidth = style.BorderWidth;

            // button style
            ButtonBackgroundColor = style.ButtonBackgroundColor;
            ButtonHoverColor = style.ButtonHoverColor;
            ButtonPressedColor = style.ButtonPressedColor;
            ButtonDisabledColor = style.ButtonDisabledColor;

            ButtonHasBorder = style.ButtonHasBorder;
            ButtonBorderWidth = style.ButtonBorderWidth;
            ButtonAccent = style.ButtonAccent;

            ButtonTextColor = style.ButtonTextColor;
            ButtonHoverTextColor = style.ButtonHoverTextColor;
            ButtonPressedTextColor = style.ButtonPressedTextColor;
            ButtonDisabledTextColor = style.ButtonDisabledTextColor;

            FontManager = style.FontManager;
        }

        public void Setup(FontManager fontmanager)
        {
            FontManager = fontmanager;
        }

        public static UIStyle Load(string path, FontManager fontmanager, out bool success, out string errormsg)
        {
            UIStyle loaded;
            success = true;
            errormsg = "";
            try
            {
                loaded = JsonConvert.DeserializeObject<UIStyle>(FileUtility.ReadFromFile(path));
            }
            catch (Exception e)
            {
                errormsg = "Failed to load UIStyle from '" + Path.GetFullPath(path) + "': " + e.Message;
                loaded = new UIStyle();
                success = false;
            }

            if (loaded == null)
            {
                errormsg = "Failed to load UIStyle from '" + Path.GetFullPath(path) + ": file does not exist.";
                loaded = new UIStyle();
                success = false;
            }

            loaded.Setup(fontmanager);
            return loaded;
        }

        public static bool Save(string path, UIStyle style, out string errormsg)
        {
            errormsg = "";
            try
            {
                FileUtility.WriteToFile(path, JsonConvert.SerializeObject(style, Formatting.Indented));
            }
            catch (Exception e)
            {
                errormsg = "Failed to save UIStyle to '" + Path.GetFullPath(path) + "': " + e.Message;
                return false;
            }
            return true;
        }
    }
}
