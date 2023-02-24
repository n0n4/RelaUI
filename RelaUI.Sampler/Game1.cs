using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RelaUI.Basics;
using RelaUI.Console;
using RelaUI.Input;
using RelaUI.Integration;
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
        UI<SampleProfile> UI = new UI<SampleProfile>();
        private TestUI TestUI;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            // trickle info about window resizing down to the rest of the app
            // use Window.ClientBounds
            graphics.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
            if (TestUI != null)
            {
                TestUI.Resize(Window.ClientBounds.Width, Window.ClientBounds.Height);
            }
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            UI.Console.Log("Loading Content...");

            try
            {
                // Create a new SpriteBatch, which can be used to draw textures.
                spriteBatch = new SpriteBatch(GraphicsDevice);

                UI.LoadContent(Content, MakeDefaultProfile);

                // Add the startup UI
                //UIBoss.Add(new TestUI(UIStyle));
                TestUI = new TestUI(UI, UI.Style, GraphicsDevice);
                UI.Boss.Add(TestUI);
                UI.Boss.Add(new ConsoleUI(UI.Boss, UI.Style, GraphicsDevice, UI.Console,
                    GraphicsDevice.PresentationParameters.BackBufferWidth, 300));
                UI.Boss.Add(new FrameRateUI(UI.Style, GraphicsDevice));
                //Console.AddHandle("UIStyle", new ConsoleHandleUIStyle(UIBoss));

            }
            catch (Exception e)
            {
                UI.Console.Error("FAILED: " + e.Message);
                UI.Console.Error("Could not load content. Aborting.");
                Crash(e);
                return;
            }

            UI.Console.Log("Content Loaded.");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            UI.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            // TODO: Add your drawing code here

            UI.Draw(gameTime, GraphicsDevice, spriteBatch);

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
            UI.Console.Save(UI.DefaultNormalLogLocation);
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
                UI.Console.Error("!!CRASH!!" + ie.Message);
                UI.Console.Error("!!CRASH!!" + ie.StackTrace);
                ie = e.InnerException;
            }

            UI.Console.Save(UI.DefaultCrashLogLocation);
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
