using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI
{
    public static class RenderTargetScope
    {
        public static RasterizerState ScissorTestEnabledRasterizerState = new RasterizerState() { ScissorTestEnable = true };

        public static Rectangle Open(SpriteBatch sb, int x, int y, int width, int height)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, null, null, null, ScissorTestEnabledRasterizerState);
            Rectangle oldcliprect = sb.GraphicsDevice.ScissorRectangle;
            Rectangle newcliprect = new Rectangle(x, y, width, height);

            // need to do some logic here: if the new clipping goes out-of-bounds with respect
            // to the old one, need to clamp that down
            if (newcliprect.X < oldcliprect.X)
            {
                newcliprect.Width -= (oldcliprect.X - newcliprect.X);
                newcliprect.X = oldcliprect.X;
            }
            if (newcliprect.Y < oldcliprect.Y)
            {
                newcliprect.Height -= (oldcliprect.Y - newcliprect.Y);
                newcliprect.Y = oldcliprect.Y;
            }
            if (newcliprect.X + newcliprect.Width > oldcliprect.X + oldcliprect.Width)
            {
                newcliprect.Width = oldcliprect.X + oldcliprect.Width - newcliprect.X;
            }
            if (newcliprect.Y + newcliprect.Height > oldcliprect.Y + oldcliprect.Height)
            {
                newcliprect.Height = oldcliprect.Y + oldcliprect.Height - newcliprect.Y;
            }

            //Set the current scissor rectangle
            sb.GraphicsDevice.ScissorRectangle = newcliprect;
            return oldcliprect;
        }

        public static void Close(SpriteBatch sb, Rectangle oldcliprect)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, null, null, null, ScissorTestEnabledRasterizerState);
            sb.GraphicsDevice.ScissorRectangle = oldcliprect;
        }
    }
}
