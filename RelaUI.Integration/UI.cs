using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RelaUI.Console;
using RelaUI.Input;
using RelaUI.Profiles;
using RelaUI.Styles;
using RelaUI.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Integration
{
    public class UI<T>
    {
        public UIStyle Style = new UIStyle();
        public UIBoss Boss;
        public FontManager FontManager = new FontManager();
        public InputManager InputManager;
        public CursorManager CursorManager = new CursorManager();
        public RelaConsole Console = new RelaConsole();
        public HotkeyManager HotkeyManager;
        public ProfileManager<T> ProfileManager;

        public string DefaultHotkeysLocation = ".\\hotkeys.json";
        public string DefaultUIStyleLocation = ".\\Styles\\";
        public string DefaultCrashLogLocation = ".\\crash.log";
        public string DefaultNormalLogLocation = ".\\last.log";
        public string DefaultProfilesLocation = ".\\Profiles\\";

        public void LoadContent(
            ContentManager content,
            Func<Profile<T>> makeDefaultProfile,
            int defaultFontSize = 16)
        {
            // create the profile manager
            ProfileManager = new ProfileManager<T>(DefaultProfilesLocation,
                makeDefaultProfile, (s) => { Console.Error(s); });

            // load the fonts
            FontManager.Load(content);
            FontManager.DefaultSize = defaultFontSize;
            CursorManager.Load(content);
            CursorManager.Enabled = false;

            // load the UI Style
            bool success = false;
            string errormsg;
            string styleName = ProfileManager.CurrentProfile.Style;
            // important note: the default style is readonly
            // user edits to it will not be persisted
            if (styleName != Profile<T>.DefaultStyle)
            {
                Style = UIStyle.Load(DefaultUIStyleLocation, FontManager, out success, out errormsg);
            }
            else
            {
                // if using default style, we just keep the one from the constructor
                // need to setup fonts on it, though
                Style.Setup(FontManager);
            }
            if (!success)
            {
                UIStyle.Save(DefaultUIStyleLocation, Style, out errormsg);
            }
            CursorManager.SetCursor(Style.CursorStyle, Style.CursorColor);

            // load the cursor, create the input manager
            InputManager = new InputManager(CursorManager);
            HotkeyManager = new HotkeyManager((s) => { Console.Error(s); });
            List<HotkeySave> hotkeys = HotkeyManager.Load(DefaultHotkeysLocation);
            if (hotkeys == null)
                hotkeys = new List<HotkeySave>();
            //Console.AddHandle("Hotkey", new ConsoleHandleHotkeyManager(HotkeyManager));

            // load specific hotkeys
            List<HotkeySave> profHotkeys = ProfileManager.CurrentProfile.Hotkeys;
            HotkeyManager.LoadProfile(HotkeyManager.MergeHotkeySaves(hotkeys, profHotkeys));

            Boss = new UIBoss(Style, HotkeyManager);
        }

        public void Update(GameTime gameTime)
        {
            float elapsedms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            InputManager.Update(elapsedms);
            Boss.Update(elapsedms, InputManager);
            // note: it is important that hotkey manager is called last
            // so that other features have a chance to capture text input
            InputManager.PostUpdate();
            HotkeyManager.Update(elapsedms, InputManager);
        }

        public void Draw(GameTime gameTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            float elapsedms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // now render all UIs
            Boss.Draw(elapsedms, graphics, spriteBatch, InputManager);

            // draw cursor
            InputManager.Render(graphics, spriteBatch);
        }
    }
}
