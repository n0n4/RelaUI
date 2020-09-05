using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Components
{
    public class UITextureIcon : UIComponent
    {
        public Texture2D Texture;
        public double ScaleX = 1.0;
        public double ScaleY = 1.0;
        public Color Color = Color.White;
        public Rectangle SourceRectangle;

        public UITextureIcon(float x, float y, Texture2D texture, Rectangle sourceRect)
        {
            this.x = x;
            this.y = y;
            Texture = texture;
            SourceRectangle = sourceRect;
        }

        public override int GetWidth()
        {
            return SourceRectangle.Width;
        }

        public override int GetHeight()
        {
            return SourceRectangle.Height;
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            //sb.Draw(Texture, new Vector2(dx, dy), Color.White);
            sb.Draw(Texture, new Rectangle((int)dx, (int)dy, (int)(SourceRectangle.Width * ScaleX), (int)(SourceRectangle.Height * ScaleY)), SourceRectangle, Color);
        }
    }
}
