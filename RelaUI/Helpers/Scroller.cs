using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RelaUI.Input;
using RelaUI.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RelaUI.Helpers
{
    public class Scroller
    {
        public Color ScrollBackgroundColor;
        public Color ScrollBarColor;
        public Color BorderColor;
        public int BorderWidth = 2;

        public bool WasScrollingHorz = false;
        public bool WasScrollingVert = false;
        public bool IsDragScrolling = false;

        public int OffsetX = 0;
        public int OffsetY = 0;
        public int ScrollWidth = 0;
        public int ScrollHeight = 0;
        public int ScrollSize = 8; // pixel size of the scroll bar

        public Func<int> GetWidth;
        public Func<int> GetHeight;
        public Func<float> GetX;
        public Func<float> GetY;
        public int VerticalOffset = 0;

        private float Lastdx = 0; // need to store these for GetFocus
        private float Lastdy = 0;

        public Scroller(Func<int> getWidth, Func<int> getHeight, Func<float> getX, Func<float> getY,
            int verticalOffset)
        {
            GetWidth = getWidth;
            GetHeight = getHeight;
            GetX = getX;
            GetY = getY;
            VerticalOffset = verticalOffset;
        }

        public void Init(UIStyle style)
        {
            BorderColor = style.BackgroundAccent;
            BorderWidth = style.BorderWidth;

            ScrollBackgroundColor = style.BackgroundAccent;
            ScrollBarColor = style.ForegroundColor;
        }

        public bool PreventChildFocus(float dx, float dy, InputManager input)
        {
            // check for scrollbars 
            return UpdateScrolling(0, input);
        }

        public bool SelfGetFocus(float elapsedms, InputManager input)
        {
            // check for scrollbars 
            return UpdateScrolling(elapsedms, input);
        }

        public void SelfFocusedInput(float elapsedms, InputManager input)
        {
            if (input.MouseDown(eMouseButtons.Left))
            {
                UpdateScrolling(elapsedms, input);
            }
            else
            {
                WasScrollingHorz = false;
                WasScrollingVert = false;
                IsDragScrolling = false;
            }
            HandleInput(elapsedms, input);
        }

        public void SelfParentInput(float elapsedms, InputManager input)
        {
            HandleInput(elapsedms, input);
        }

        private void HandleInput(float elapsedms, InputManager input)
        {
            if (input.IsKeyFresh(Keys.Up))
            {
                input.CaptureInput(Keys.Up);
                OffsetY -= 5;
                if (OffsetY < 0)
                {
                    OffsetY = 0;
                }
            }
            if (input.IsKeyFresh(Keys.Down))
            {
                input.CaptureInput(Keys.Down);
                OffsetY += 5;
                if (OffsetY > GetScrollYMax())
                {
                    OffsetY = GetScrollYMax();
                }
            }
            if (input.IsKeyFresh(Keys.Left))
            {
                input.CaptureInput(Keys.Left);
                OffsetX -= 5;
                if (OffsetX < 0)
                {
                    OffsetX = 0;
                }
            }
            if (input.IsKeyFresh(Keys.Right))
            {
                input.CaptureInput(Keys.Right);
                int width = GetWidth();
                OffsetX += 5;
                if (OffsetX > ScrollWidth - width)
                {
                    OffsetX = ScrollWidth - width;
                }
            }
            int scrolldiff = input.FreshMouseScrollDifference();
            if (scrolldiff > 0)
            {
                OffsetY -= scrolldiff / 2;
                if (OffsetY < 0)
                {
                    OffsetY = 0;
                }
                input.CaptureMouseScroll();
            }
            else if (scrolldiff < 0)
            {
                OffsetY -= scrolldiff / 2;
                if (OffsetY > GetScrollYMax())
                {
                    OffsetY = GetScrollYMax();
                }
                input.CaptureMouseScroll();
            }
        }

        private bool UpdateScrolling(float elapsedms, InputManager input)
        {
            // MUST support case where time is null
            bool changed = false;
            int mx = input.State.MousePos.X;
            int my = input.State.MousePos.Y;
            int width = GetWidth();
            int height = GetHeight();
            int scw = ScrollSize + (BorderWidth * 2);
            float x = GetX();
            float y = GetY();

            if (ScrollWidth > width)
            {
                float sfx = Lastdx + BorderWidth + 2;
                float sfy = Lastdy + height - scw - 2;
                int sfw = width - (BorderWidth * 2) - 4;
                int sfh = scw;
                if (((mx > sfx && mx < sfx + sfw && my > sfy && my < sfy + sfh) || WasScrollingHorz) && !WasScrollingVert)
                {
                    WasScrollingHorz = true;

                    int sbarw = GetSliderBarWidth(false);
                    float sliderratio = ((float)(OffsetX) / (float)((ScrollWidth - width)));
                    float sliderx = sfx + (sliderratio * (sfw - sbarw));
                    bool inbounds = (mx >= sliderx && mx <= sliderx + sbarw);
                    if (IsDragScrolling || inbounds)
                    {
                        OffsetX += mx - input.LastState.MousePos.X;
                        if (!inbounds)
                        {
                            float dist = mx - sliderx;
                            if (mx > sliderx)
                            {
                                dist -= sbarw;
                            }
                            OffsetX += (int)((dist) / 2);
                        }
                        IsDragScrolling = true;
                    }
                    else
                    {
                        OffsetX = (int)(Math.Round((1.0f - (((sfx + sfw) - mx) / (sfw))) * (ScrollWidth - width)));
                    }

                    changed = true;
                    if (OffsetX < 0)
                    {
                        OffsetX = 0;
                    }
                    if (OffsetX > ScrollWidth - width)
                    {
                        OffsetX = ScrollWidth - width;
                    }
                }
            }
            if (ScrollHeight > height && !changed)
            {
                int hmod = BorderWidth * 2 + VerticalOffset;
                int hstart = BorderWidth + 2 + VerticalOffset;
                if (ScrollWidth > width)
                {
                    hmod += scw + 4;
                }

                float sfx = Lastdx + width - scw - 2;
                float sfy = Lastdy + hstart;
                int sfw = scw;
                int sfh = height - hmod;
                if (((mx > sfx && mx < sfx + sfw && my > sfy && my < sfy + sfh) || WasScrollingVert) && !WasScrollingHorz)
                {
                    WasScrollingVert = true;
                    int sbarw = GetSliderBarWidth(true);
                    float sliderratio = ((float)(OffsetY) / (float)((GetScrollYMax())));
                    float slidery = sfy + (sliderratio * (sfh - sbarw));
                    bool inbounds = (my >= slidery && my <= slidery + sbarw);
                    if (IsDragScrolling || inbounds)
                    {
                        OffsetY += my - input.LastState.MousePos.Y;
                        if (!inbounds)
                        {
                            float dist = my - slidery;
                            if (my > slidery)
                            {
                                dist -= sbarw;
                            }
                            OffsetY += (int)((dist) / 2);
                        }
                        IsDragScrolling = true;
                    }
                    else
                    {
                        OffsetY = (int)(Math.Round((1.0f - (((sfy + sfh) - my) / (sfh))) * (GetScrollYMax())));
                    }

                    changed = true;
                    if (OffsetY < 0)
                    {
                        OffsetY = 0;
                    }
                    if (OffsetY > GetScrollYMax())
                    {
                        OffsetY = GetScrollYMax();
                    }
                }
            }
            return changed;
        }

        public void SnapScrollToBottom()
        {
            if (ScrollHeight > GetHeight())
            {
                OffsetY = GetScrollYMax();
            }
        }

        public int GetScrollYMax()
        {
            int height = GetHeight();
            if (height >= ScrollHeight)
                return 0;
            return ScrollHeight - height;
        }

        public int GetSliderBarWidth(bool vert)
        {
            // this is just the size of the bar in the slider that the user drags
            int minsize = BorderWidth * 3;
            int w = vert ? GetHeight() : GetWidth();
            int sw = vert ? ScrollHeight : ScrollWidth;
            int size = (int)(((float)w / (float)sw) * (((float)w * 3.0f) / 4.0f));

            return size > minsize ? size : minsize;
        }

        public void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            SharedRender(elapsedms, g, sb, input, dx, dy);
        }
        public void Render(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            SharedRender(elapsedms, g, sb, input, dx + GetX(), dy + GetY());
        }

        private void SharedRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            Lastdx = dx;
            Lastdy = dy;
            int scw = ScrollSize + (BorderWidth * 2);
            int width = GetWidth();
            int height = GetHeight();
            int mx = input.State.MousePos.X;
            int my = input.State.MousePos.Y;

            if (ScrollWidth > width)
            {
                float sfx = dx + BorderWidth + 2;
                float sfy = dy + height - scw - 2 - BorderWidth;
                int sfw = width - (BorderWidth * 2) - 4;
                int sfh = scw;

                Draw.DrawRectangleHandle(sb, (int)sfx, (int)sfy, sfw, sfh, ScrollBackgroundColor);
                Draw.DrawRectangleHandleOutline(sb, (int)sfx, (int)sfy, sfw, sfh, BorderColor, BorderWidth);

                int sbarw = GetSliderBarWidth(false);
                float sliderratio = ((float)(OffsetX) / (float)((ScrollWidth - width)));
                float sliderx = sfx + (sliderratio * (sfw - sbarw));
                Draw.DrawRectangleHandleOutline(sb, (int)sliderx, (int)(sfy + BorderWidth),
                    GetSliderBarWidth(false), ScrollSize, ScrollBarColor, BorderWidth);
                if (mx > sfx && mx < sfx + sfw && my > sfy && my < sfy + sfh)
                {
                    input.Cursor.TrySet(eCursorState.CLICKABLE);
                }
            }
            if (ScrollHeight > height)
            {
                int hmod = BorderWidth * 2 + VerticalOffset;
                int hstart = BorderWidth + 2 + VerticalOffset;
                if (ScrollWidth > width)
                {
                    hmod += scw + 6;
                }
                else
                {
                    hmod += 4;
                }

                float sfx = dx + width - scw - 2 - BorderWidth;
                float sfy = dy + hstart;
                int sfw = scw;
                int sfh = height - hmod;

                Draw.DrawRectangleHandle(sb, (int)sfx, (int)sfy, sfw, sfh, ScrollBackgroundColor);
                Draw.DrawRectangleHandleOutline(sb, (int)sfx, (int)sfy, sfw, sfh, BorderColor, BorderWidth);

                int sbarw = GetSliderBarWidth(true);
                float sliderratio = ((float)(OffsetY) / (float)((GetScrollYMax())));
                float slidery = sfy + (sliderratio * (sfh - sbarw));
                Draw.DrawRectangleHandleOutline(sb, (int)(sfx + BorderWidth), (int)slidery,
                    ScrollSize, sbarw, ScrollBarColor, BorderWidth);
                if (mx > sfx && mx < sfx + sfw && my > sfy && my < sfy + sfh)
                {
                    input.Cursor.TrySet(eCursorState.CLICKABLE);
                }
            }
        }
    }
}
