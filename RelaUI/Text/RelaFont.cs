using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Text
{
    public class RelaFont
    {
        public bool IsDynamic = false;

        public SpriteFont SFont;
        private DynamicSpriteFont DFontField;

        public int Size;

        public int LineSpacing
        {
            get
            {
                if (IsDynamic)
                    return (int)DFont.Size;
                return SFont.LineSpacing;
            }
        }

        public DynamicSpriteFont DFont
        {
            get
            {
                DFontField.Size = Size;
                return DFontField;
            }
            set
            {
                DFontField = value;
            }
        }
    }
}
