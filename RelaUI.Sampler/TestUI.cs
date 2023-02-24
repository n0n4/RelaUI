using Microsoft.Xna.Framework.Graphics;
using RelaUI.Basics.Stylers;
using RelaUI.Components;
using RelaUI.Dialogs;
using RelaUI.Input;
using RelaUI.Integration;
using RelaUI.Sampler.Screens;
using RelaUI.Styles;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Sampler
{
    public class TestUI : IUIHandle
    {
        public UISystem UISystem;
        public UI<SampleProfile> UI;
        public GraphicsDevice Graphics;

        private IScreen CurrentScreen;
        public HomeScreen HomeScreen;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TestUI(UI<SampleProfile> ui, UIStyle style, GraphicsDevice graphicsDevice)
        {
            UI = ui;
            UISystem = new UISystem(graphicsDevice);
            UISystem.Init(style);
            Graphics = graphicsDevice;

            Height = graphicsDevice.PresentationParameters.BackBufferHeight;
            Width = graphicsDevice.PresentationParameters.BackBufferWidth;

            HomeScreen = new HomeScreen(this);
            Load(HomeScreen);
        }

        public UISystem GetUISystem()
        {
            return UISystem;
        }

        public string GetName()
        {
            return "TestUI";
        }

        public void Update(float elapsedms, InputManager input)
        {
            if (CurrentScreen != null)
                CurrentScreen.Update(elapsedms);
        }

        public void RegisterHotkeys(HotkeyManager hotkeyManager)
        {

        }

        public void Load(IScreen screen)
        {
            // remove the old panel, call the callback if it exists
            // then add the new panel
            if (CurrentScreen != null)
                UISystem.Remove(CurrentScreen.GetPanel());

            UISystem.Add(screen.GetPanel());

            if (CurrentScreen != null)
                CurrentScreen.Unloaded();

            CurrentScreen = screen;
            // reinit
            UISystem.Init(UI.Style);

            // resize screen in case the window size has changed since we were here last
            screen.Resize(Width, Height);

            // call loaded callback last in case it redirects the screen
            screen.Loaded();
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;

            if (CurrentScreen != null)
                CurrentScreen.Resize(width, height);
        }
    }
}
