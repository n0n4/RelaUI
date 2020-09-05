using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.DrawHandles
{
    public static class DrawHandleStatic
    {
        public static DrawHandleRect Rect = null;

        public static void Setup(GraphicsDevice g)
        {
            if (Rect == null)
                Rect = Draw.CreateRectangle(g, Color.White, 1, 1);
        }
    }
}
