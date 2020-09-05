using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using RelaUI.Input;

namespace RelaUI.Components
{
    public class UITexture : UIComponent
    {
        public Texture2D Texture;
        public double ScaleX = 1.0;
        public double ScaleY = 1.0;
        public Color Color = Color.White;

        public UITexture(float x, float y, Texture2D texture)
        {
            this.x = x;
            this.y = y;
            Texture = texture;
        }

        public override int GetWidth()
        {
            return Texture.Width;
        }

        public override int GetHeight()
        {
            return Texture.Height;
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            //sb.Draw(Texture, new Vector2(dx, dy), Color.White);
            sb.Draw(Texture, new Rectangle((int)dx, (int)dy, (int)(Texture.Width * ScaleX), (int)(Texture.Height * ScaleY)), Color);
        }
    }
}
