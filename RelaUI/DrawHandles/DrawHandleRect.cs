using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.DrawHandles
{
    public class DrawHandleRect
    {
        public Color[] Data;
        public Texture2D RectTexture;

        public DrawHandleRect(GraphicsDevice g, int w, int h)
        {
            Data = new Color[w * h];
            RectTexture = new Texture2D(g, w, h);
        }
    }
}
