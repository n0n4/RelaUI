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
        public string CursorStyle = "C1";
        public Color CursorColor = Color.Lime;

        public Color BackgroundColor = Color.Black;
        public Color SecondaryBackgroundColor = Color.MidnightBlue;
        public Color BackgroundAccent = Color.DarkSlateBlue; // used e.g. for panel borders
        public Color ForegroundColor = Color.White; // text color generally
        public Color SecondaryTextColor = Color.Gray; // non-primary text (like placeholders)

        public int BorderWidth = 2;

        // button style
        public Color ButtonBackgroundColor = Color.MidnightBlue;
        public Color ButtonHoverColor = Color.Snow;
        public Color ButtonPressedColor = Color.DarkSlateBlue;
        public Color ButtonDisabledColor = Color.DarkSlateBlue;

        public bool ButtonHasBorder = true; // used for button border
        public int ButtonBorderWidth = 2;
        public Color ButtonAccent = Color.DarkSlateBlue;

        public Color ButtonTextColor = Color.White;
        public Color ButtonHoverTextColor = Color.Black;
        public Color ButtonPressedTextColor = Color.White;
        public Color ButtonDisabledTextColor = Color.LightGray;

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
