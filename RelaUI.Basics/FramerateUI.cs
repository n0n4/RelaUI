using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RelaUI.Components;
using RelaUI.Input;
using RelaUI.Styles;
using RelaUI.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Basics
{
    public class FrameRateUI : IUIHandle
    {
        public UISystem UISystem;
        private GraphicsDevice GraphicsDevice;

        private FramerateListener FRListener;
        private UILabel FRLabel; // update rate
        private UILabel DRLabel; // draw rate
        //private UILabel TLabel;

        private int TrackedFrames = 0;
        private double TrackedTime = 0;

        // todo: look up way of making strings without garbage

        public FrameRateUI(UIStyle style, GraphicsDevice g, string font = "NotoMono-Regular", int? fontsize = 14)
        {
            GraphicsDevice = g;
            UISystem = new UISystem(g);

            FRListener = new FramerateListener();
            UISystem.Add(FRListener);

            FRLabel = new UILabel(g.PresentationParameters.BackBufferWidth, 0, 100, 12, "TTT", font: font, fontsize: fontsize);
            UISystem.Add(FRLabel);

            DRLabel = new UILabel(g.PresentationParameters.BackBufferWidth, 12, 100, 12, "TTT", font: font, fontsize: fontsize);
            FRListener.AttachedLabel = DRLabel;
            UISystem.Add(DRLabel);
            DRLabel.EventPostInit += (s, e) =>
            {
                DRLabel.y = FRLabel.y + FRLabel.Height;
            };

            //TLabel = new UILabel(g.PresentationParameters.BackBufferWidth, 24, 100, 12, "TTT", fontsize: 10);
            //UISystem.Add(TLabel);


            UISystem.Init(style);
        }

        public string GetName()
        {
            return "FrameRateUI";
        }

        public UISystem GetUISystem()
        {
            return UISystem;
        }

        public void Update(float elapsedms, InputManager input)
        {
            TrackedFrames++;
            TrackedTime += elapsedms/1000.0f;
            if (TrackedTime >= 1.0)
            {
                double fps = TrackedFrames / TrackedTime;
                FRLabel.Text = "u" + fps.ToString("0.0");
                FRLabel.ProcessText();
                FRLabel.x = GraphicsDevice.PresentationParameters.BackBufferWidth - TextHelper.GetWidth(FRLabel.SFont, FRLabel.TextLines[0], FRLabel.FontSettings);

                TrackedTime = 0;
                TrackedFrames = 0;
            }

            //TLabel.Text = time.TotalTime.Hours + ":" + time.TotalTime.Minutes + ":" + time.TotalTime.Seconds;
            //TLabel.ProcessText();
            //TLabel.x = GraphicsDevice.PresentationParameters.BackBufferWidth - TextHelper.GetWidth(TLabel.SFont, TLabel.TextLines[0], TLabel.FontSettings);
        }

        public void RegisterHotkeys(HotkeyManager hotkeyManager)
        {
            //add hotkey to turn on/off
            hotkeyManager.Register("Toggle FPS", GetName(), ToggleFPS);
        }

        public bool ToggleFPS(float elapsedms, InputManager input, Keys[] keys, int keyCount)
        {
            if (!input.IsAtLeastOnePressed(keys, keyCount))
                return false;

            //TLabel.Visible = !TLabel.Visible;
            FRLabel.Visible = !FRLabel.Visible;
            DRLabel.Visible = !DRLabel.Visible;
            return true;
        }
    }
}