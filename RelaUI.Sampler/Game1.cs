using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RelaUI.Basics;
using RelaUI.Console;
using RelaUI.Input;
using RelaUI.Profiles;
using RelaUI.Styles;
using RelaUI.Text;
using System;
using System.Collections.Generic;

namespace RelaUI.Sampler
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private UIStyle UIStyle;
        private UIBoss UIBoss;
        private FontManager FontManager = new FontManager();
        private InputManager InputManager;
        private CursorManager CursorManager = new CursorManager();
        private RelaConsole Console = new RelaConsole();
        private HotkeyManager HotkeyManager;
        private ProfileManager<SampleProfile> ProfileManager;

        private readonly string DefaultHotkeysLocation = ".\\hotkeys.json";
        private readonly string DefaultUIStyleLocation = ".\\style.json";
        private readonly string DefaultCrashLogLocation = ".\\crash.log";
        private readonly string DefaultNormalLogLocation = ".\\last.log";
        private readonly string DefaultProfilesLocation = ".\\Profiles\\";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Console.Log("Loading Content...");

            try
            {
                // Create a new SpriteBatch, which can be used to draw textures.
                spriteBatch = new SpriteBatch(GraphicsDevice);

                // TODO: use this.Content to load your game content here

                // load the fonts
                FontManager.Load(Content);
                FontManager.DefaultSize = 16;
                CursorManager.Load(Content);
                CursorManager.Enabled = false;

                // load the UI Style
                bool success;
                string errormsg;
                UIStyle = UIStyle.Load(DefaultUIStyleLocation, FontManager, out success, out errormsg);
                if (!success)
                {
                    UIStyle.Save(DefaultUIStyleLocation, UIStyle, out errormsg);
                }
                CursorManager.SetCursor(UIStyle.CursorStyle, UIStyle.CursorColor);

                // load the cursor, create the input manager
                InputManager = new InputManager(CursorManager);
                HotkeyManager = new HotkeyManager((s) => { Console.Error(s); });
                List<HotkeySave> hotkeys = HotkeyManager.Load(DefaultHotkeysLocation);
                if (hotkeys == null)
                    hotkeys = new List<HotkeySave>();
                //Console.AddHandle("Hotkey", new ConsoleHandleHotkeyManager(HotkeyManager));

                // create the profile manager
                ProfileManager = new ProfileManager<SampleProfile>(DefaultProfilesLocation,
                    MakeDefaultProfile, (s) => { Console.Error(s); });

                // load specific hotkeys
                List<HotkeySave> profHotkeys = ProfileManager.CurrentProfile.Hotkeys;
                HotkeyManager.LoadProfile(HotkeyManager.MergeHotkeySaves(hotkeys, profHotkeys));

                // Add the startup UI
                //UIBoss.Add(new TestUI(UIStyle));
                UIBoss = new UIBoss(UIStyle, HotkeyManager);
                TestUI testui = new TestUI(UIStyle, GraphicsDevice);
                UIBoss.Add(testui);
                UIBoss.Add(new ConsoleUI(UIBoss, UIStyle, GraphicsDevice, Console,
                    GraphicsDevice.PresentationParameters.BackBufferWidth, 300));
                UIBoss.Add(new FrameRateUI(UIStyle, GraphicsDevice));
                //Console.AddHandle("UIStyle", new ConsoleHandleUIStyle(UIBoss));

            }
            catch (Exception e)
            {
                Console.Error("FAILED: " + e.Message);
                Console.Error("Could not load content. Aborting.");
                Crash(e);
                return;
            }

            Console.Log("Content Loaded.");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            float elapsedms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            InputManager.Update(elapsedms);
            UIBoss.Update(elapsedms, InputManager);
            // note: it is important that hotkey manager is called last
            // so that other features have a chance to capture text input
            InputManager.PostUpdate();
            HotkeyManager.Update(elapsedms, InputManager);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);//Color.CornflowerBlue);
            spriteBatch.Begin();
            // TODO: Add your drawing code here

            float elapsedms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // now render all UIs
            UIBoss.Draw(elapsedms, GraphicsDevice, spriteBatch, InputManager);

            // draw cursor
            InputManager.Render(GraphicsDevice, spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// This is called when the Exiting event fires.
        /// Primary purpose is to save the console log.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleOnExiting(object sender, EventArgs e)
        {
            Console.Save(DefaultNormalLogLocation);
        }

        /// <summary>
        /// Method for gracefully crashing the game, to be used when we hit an unrecoverable exception or problem.
        /// The log will be saved at DefaultCrashLogLocation
        /// </summary>
        /// <param name="e">The root cause exception. InnerExceptions will be logged recursively.</param>
        public void Crash(Exception e)
        {
            Exception ie = e;
            while (ie != null)
            {
                Console.Error("!!CRASH!!" + ie.Message);
                Console.Error("!!CRASH!!" + ie.StackTrace);
                ie = e.InnerException;
            }

            Console.Save(DefaultCrashLogLocation);
            Exit();
            System.Environment.Exit(0);
        }

        public Profile<SampleProfile> MakeDefaultProfile()
        {
            Profile<SampleProfile> prof = new Profile<SampleProfile>();
            prof.UserName = "User";
            prof.Hotkeys.Add(new HotkeySave("Toggle Console", "ConsoleUI",
                new List<string>() { "OemTilde" }));
            prof.Hotkeys.Add(new HotkeySave("Toggle FPS", "FrameRateUI",
                new List<string>() { "F1" }));
            // can do combined strokes e.g.
            // "LeftControl-O",
            // "RightControl-O",

            return prof;
        }
    }
}
