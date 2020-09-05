using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Components
{
    public class UIAutoList : UIComponent
    {
        private int Width = 0;
        private int Height = 0;
        public int MaxWidth = 0; // if non-zero, caps the width/height
        public int MaxHeight = 0;
        public int FixedWidthPerIndex = 0; // if non-zero, each index has this width
        public int FixedHeightPerIndex = 0; // if non-zero, each index has this height
        public int StartingIndex = 0;
        public int MaxIndex = -1; // if >=0, only items up and including to this index will be shown
        public int MaxCount = -1; // if >=0, only this many items can be shown at once

        public int MarginX = 8; // extra horizontal space for each component
        public int MarginY = 8; // extra vertical space

        public bool HasBorder = true;
        public int BorderWidth = 2; // border size
        public Color BorderColor;

        public bool HasOuterBorder = true;
        public int OuterBorderWidth = 2; // border size
        public Color OuterBorderColor;


        public Color BackgroundColor;

        public bool AlternateBackgrounds = false;
        public Color BackgroundColor2;

        public eUIOrientation AutoOrientation = eUIOrientation.VERTICAL;
        public List<UIComponent> AutoComponents = new List<UIComponent>();
        private List<UIComponent> OldAutoComponents = new List<UIComponent>();
        private bool DoneAuto = false;
        private Dictionary<int, Vector2> AutoOriginalPositions = new Dictionary<int, Vector2>();

        public bool LeftAlign = false;
        public bool BottomAlign = false;

        public UIAutoList(eUIOrientation orientation, int width, int height, int fixedindexwidth = 0, int fixedindexheight = 0, bool alternatesBackground = false)
        {
            AutoOrientation = orientation;
            MaxWidth = width;
            MaxHeight = height;
            FixedWidthPerIndex = fixedindexwidth;
            FixedHeightPerIndex = fixedindexheight;
            AlternateBackgrounds = alternatesBackground;
        }

        public override void AddAuto(UIComponent comp)
        {
            Add(comp);
            AutoComponents.Add(comp);
        }

        protected override void SelfInit()
        {
            BackgroundColor = Style.BackgroundColor;
            BackgroundColor2 = Style.SecondaryBackgroundColor;
            BorderColor = Style.BackgroundAccent;
            BorderWidth = Style.BorderWidth;
            OuterBorderColor = Style.BackgroundAccent;
            OuterBorderWidth = Style.BorderWidth;

            if (OldAutoComponents != AutoComponents)
            {
                DoneAuto = false;
                AutoOriginalPositions = new Dictionary<int, Vector2>();
            }

            // handle auto components
            int totalheight = (int)InnerY;
            int totalwidth = (int)InnerX;
            int ax = 0;// GetInnerX();
            int ay = 0;// GetInnerY();
            for (int c = 0; c < AutoComponents.Count; c++)
            {
                UIComponent comp = AutoComponents[c];
                // if auto has already happened, we need to revert the components to their original
                // positions before spacing them again.
                if (!DoneAuto || !AutoOriginalPositions.ContainsKey(c))
                {
                    AutoOriginalPositions.Add(c, new Vector2(comp.x, comp.y));
                }
                else
                {
                    comp.x = AutoOriginalPositions[c].X;
                    comp.y = AutoOriginalPositions[c].Y;
                }
                comp.x += ax + MarginX;
                comp.y += ay + MarginY;
                int wid = FixedWidthPerIndex > 0 ? FixedWidthPerIndex : comp.GetWidth();
                int hei = FixedHeightPerIndex > 0 ? FixedHeightPerIndex : comp.GetHeight();
                if (AutoOrientation == eUIOrientation.HORIZONTAL)
                {
                    if (c > 0) // skip adding extra margin to the first element
                    {
                        ax += wid + MarginX;
                        totalwidth += wid + MarginX * 2;
                    }
                    else
                    {
                        ax += wid;
                        totalwidth += wid + MarginX;
                    }
                    if ((int)InnerY + hei + MarginY * 2 > totalheight)
                    {
                        totalheight = (int)InnerY + hei + MarginY * 2;
                    }
                }
                else if (AutoOrientation == eUIOrientation.VERTICAL)
                {
                    if (c > 0) // skip adding extra margin to the first element
                    {
                        ay += hei + MarginY;
                        totalheight += hei + MarginY * 2;
                    }
                    else
                    {
                        ay += hei;
                        totalheight += hei + MarginY;
                    }
                    if ((int)InnerX + wid + MarginX * 2 > totalwidth)
                    {
                        totalwidth = (int)InnerX + wid + MarginX * 2;
                    }
                }
            }
            DoneAuto = true;
            OldAutoComponents = AutoComponents;

            Width = totalwidth;
            Height = totalheight;
            if (MaxHeight != 0 && Height > MaxHeight)
            {
                Height = MaxHeight;
            }
            if (MaxWidth != 0 && Width > MaxWidth)
            {
                Width = MaxWidth;
            }

            if (LeftAlign)
                x = MaxWidth - Width;
            if (BottomAlign)
                y = MaxHeight - Height;

            // exceptions occur if list is empty & these are 0, so clamp them at 1
            if (Width == 0)
            {
                Width = 1;
            }
            if (Height == 0)
            {
                Height = 1;
            }
        }

        public override int GetWidth()
        {
            return Width;
        }

        public override int GetHeight()
        {
            return Height;
        }


        public override void Render(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            // note: this component overwrites the base render logic
            Rectangle oldrect = RenderTargetScope.Open(sb, (int)Math.Floor(dx + x), (int)Math.Floor(dy + y), Width, Height);
            {
                dx += x;
                dy += y;
                SelfRender(elapsedms, g, sb, input, dx, dy);
                if (ComponentsVisible)
                {
                    int count = 0;
                    int totalw = 0;
                    int totalh = 0;
                    for (int i = StartingIndex; i < Components.Count
                        && (MaxIndex == -1 || i <= MaxIndex)
                        && (MaxCount == -1 || count < MaxCount); i++)
                    {
                        UIComponent u = Components[i];
                        if (u.Visible)
                        {
                            int uw = 0;
                            int uh = 0;
                            if (AutoOrientation == eUIOrientation.HORIZONTAL)
                            {
                                u.y = MarginY;
                                uh = Height - MarginY * 2;
                                if (FixedWidthPerIndex > 0)
                                {
                                    u.x = MarginX + count * (MarginX * 2 + FixedWidthPerIndex);
                                    uw = FixedWidthPerIndex;
                                }
                                else
                                {
                                    u.x = MarginX + totalw;
                                    totalw += u.GetWidth() + MarginX * 2;
                                    uw = u.GetWidth();
                                }
                            }
                            else
                            {
                                u.x = MarginX;
                                uw = Width - MarginX * 2;
                                if (FixedHeightPerIndex > 0)
                                {
                                    u.y = MarginY + count * (MarginY * 2 + FixedHeightPerIndex);
                                    uh = FixedHeightPerIndex;
                                }
                                else
                                {
                                    u.y = MarginY + totalh;
                                    totalh += u.GetHeight() + MarginY * 2;
                                    uh = u.GetHeight();
                                }
                            }
                            if (AlternateBackgrounds && count % 2 == 0)
                            {
                                Draw.DrawRectangleHandle(sb, (int)(dx + u.x - MarginX), (int)(dy + u.y - MarginY), uw + MarginX * 2, uh + MarginY * 2, BackgroundColor2);
                            }
                            Rectangle inneroldrect = RenderTargetScope.Open(sb, (int)Math.Floor(dx + u.x), (int)Math.Floor(dy + u.y), uw, uh);
                            {
                                u.Render(elapsedms, g, sb, input, dx + InnerX, dy + InnerY);
                            }
                            RenderTargetScope.Close(sb, inneroldrect);
                            // draw border
                            if (HasBorder)
                            {
                                Draw.DrawRectangleHandleOutline(sb, (int)(dx + u.x - MarginX), (int)(dy + u.y - MarginY), uw + MarginX * 2, uh + MarginY * 2,
                                    BorderColor, BorderWidth);
                            }
                        }
                        count++;
                    }
                }
                if (HasOuterBorder)
                {
                    Draw.DrawRectangleHandleOutline(sb, (int)dx, (int)dy, Width, Height, OuterBorderColor, OuterBorderWidth);
                }
            }
            RenderTargetScope.Close(sb, oldrect);
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            // render background
            Draw.DrawRectangleHandle(sb, (int)dx, (int)dy, Width, Height, BackgroundColor);
        }

        protected override void SelfRemove(UIComponent comp)
        {
            AutoComponents.Remove(comp);
        }
    }
}
