using Microsoft.Xna.Framework.Graphics;
using RelaUI.Components;
using RelaUI.Input;
using RelaUI.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Basics
{
    public class FramerateListener : UIComponent
    {
        public UILabel AttachedLabel = null;
        private int TrackedFrames = 0;
        private float TrackedTime = 0;

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            TrackedFrames++;
            TrackedTime += elapsedms/1000.0f;
            if (TrackedTime >= 1.0)
            {
                double fps = TrackedFrames / TrackedTime;
                if (AttachedLabel != null)
                {
                    AttachedLabel.Text = "d" + fps.ToString("0.0");
                    AttachedLabel.ProcessText();
                    AttachedLabel.x = g.PresentationParameters.BackBufferWidth - TextHelper.GetWidth(AttachedLabel.SFont, AttachedLabel.TextLines[0], AttachedLabel.FontSettings);
                }

                TrackedTime = 0;
                TrackedFrames = 0;
            }
        }
    }
}