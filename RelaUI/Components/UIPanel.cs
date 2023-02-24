using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RelaUI.DrawHandles;
using RelaUI.Input;
using RelaUI.Text;
using RelaUI.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Components
{
    public class UIPanel : UIComponent
    {
        private int WidthField = 100;
        public int Width
        {
            get { return WidthField; }
            set
            {
                WidthField = value;

                Resize();
            }
        }
        private int HeightField = 100;
        public int Height
        {
            get { return HeightField; }
            set
            {
                HeightField = value;

                Resize();
            }
        }

        public Color Color;
        public bool HasBorder = true;
        public Color BorderColor;
        public int BorderWidth;

        public bool HasTitle = false;
        public string Title = string.Empty;

        public Color TitleBackgroundColor;
        public int TitleMargin = 4;
        
        public RelaFont TitleSFont;
        public TextSettings TitleFontSettings = new TextSettings();

        // scrolling works by manipulating the InnerX and InnerY parameters
        public bool HasScrolling = false;
        private int ScrollWidthField = 100; // the "real" width of the panel
        public int ScrollWidth
        {
            get { return ScrollWidthField; }
            set
            {
                ScrollWidthField = value;

                Resize();
            }
        }
        private int ScrollHeightField = 100; // the "real" height of the panel
        public int ScrollHeight
        {
            get { return ScrollHeightField; }
            set
            {
                ScrollHeightField = value;

                Resize();
            }
        }

        public int ScrollCurrentX = 0; // current x/y pos of scrolling
        public int ScrollCurrentY = 0;
        public float BaseInnerX = 0;
        public float BaseInnerY = 0;
        public int ScrollSize = 8; // pixel size of the scroll bar

        public bool AutoScrollWidth = false;
        public bool AutoScrollHeight = false;
        private bool WasScrollingVert = false;
        private bool WasScrollingHorz = false;

        private float Lastdx = 0; // need to store these for GetFocus
        private float Lastdy = 0;

        public int InnerMargin = 5;

        // AUTOCOMPONENTS
        // these components will have their positions automatically readjusted during init
        // their x and y become their margins
        public eUIOrientation AutoOrientation = eUIOrientation.VERTICAL;
        public List<UIComponent> AutoComponents = new List<UIComponent>();
        private List<UIComponent> OldAutoComponents = new List<UIComponent>();
        private bool DoneAuto = false;
        private Dictionary<int, Vector2> AutoOriginalPositions = new Dictionary<int, Vector2>();

        public bool HasCloseButton = false;
        public UIButton CloseButton = null;

        public UIPanel(float x, float y, int w, int h, bool hastitle = false, string title = "", string titlefont = "", int titlesize = 0,
            eUIOrientation autoorientation = eUIOrientation.VERTICAL,
            bool hasscrolling = false, int scrollw = 0, int scrollh = 0,
            bool hasclose = false)
        {
            this.x = x;
            this.y = y;
            Width = w;
            Height = h;

            HasTitle = hastitle;
            Title = title;
            TitleFontSettings.FontName = titlefont;
            TitleFontSettings.FontSize = titlesize;

            if (hasscrolling)
            {
                HasScrolling = hasscrolling;
                ScrollWidth = scrollw;
                ScrollHeight = scrollh;
            }

            AutoOrientation = autoorientation;

            if (hasclose)
            {
                HasCloseButton = true;
                CloseButton = new UIButton(Width - BorderWidth - TitleMargin - 2, TitleMargin + BorderWidth + 2, 30, 30, "X", font: titlefont, fontsize: titlesize);
                CloseButton.SkipBaseRender = true;
                Add(CloseButton);
            }
        }

        public override void AddAuto(UIComponent comp)
        {
            Add(comp);
            AutoComponents.Add(comp);
        }

        protected override void SelfInit()
        {
            Color = Style.BackgroundColor;
            BorderColor = Style.BackgroundAccent;
            BorderWidth = Style.BorderWidth;

            TitleBackgroundColor = Style.BackgroundAccent;

            TitleSFont = TitleFontSettings.Init(Style);

            InnerX = InnerMargin;
            InnerY = InnerMargin + GetTitleHeight();
            BaseInnerX = InnerX;
            BaseInnerY = InnerY;

            if (!OldAutoComponents.EqualOrderContent(AutoComponents))
            {
                DoneAuto = false;
                AutoOriginalPositions.Clear();
            }

            // handle auto components
            int totalheight = (int)InnerY;
            int totalwidth = (int)InnerX;
            int ax = 0;// GetInnerX();
            int ay = 0;// GetInnerY();
            int c = 0;
            foreach (UIComponent comp in AutoComponents)
            {
                // if auto has already happened, we need to revert the components to their original
                // positions before spacing them again.
                if (!DoneAuto)
                {
                    AutoOriginalPositions.Add(c, new Vector2(comp.x, comp.y));
                }
                else
                {
                    comp.x = AutoOriginalPositions[c].X;
                    comp.y = AutoOriginalPositions[c].Y;
                }
                float origx = comp.x;
                float origy = comp.y;
                comp.x += ax;
                comp.y += ay;
                if (AutoOrientation == eUIOrientation.HORIZONTAL)
                {
                    ax += comp.GetWidth() + (int)origx;
                    totalwidth += comp.GetWidth() + (int)origx;
                    if ((int)InnerY + comp.GetHeight() > totalheight)
                    {
                        totalheight = (int)InnerY + comp.GetHeight();
                    }
                }
                else if (AutoOrientation == eUIOrientation.VERTICAL)
                {
                    ay += comp.GetHeight() + (int)origy;
                    totalheight += comp.GetHeight() + (int)origy;
                    if ((int)InnerX + comp.GetWidth() > totalwidth)
                    {
                        totalwidth = (int)InnerX + comp.GetWidth();
                    }
                }
                c++;
            }
            OldAutoComponents = AutoComponents;
            DoneAuto = true;

            if (AutoScrollHeight)
            {
                ScrollHeight = totalheight;
            }
            if (AutoScrollWidth)
            {
                ScrollWidth = totalwidth;
            }

            if (HasCloseButton)
            {
                CloseButton.Height = GetTitleHeight() - 4;
                CloseButton.Width = CloseButton.Height;
                CloseButton.x = Width - BorderWidth - TitleMargin - CloseButton.Width - InnerX - 2;
                CloseButton.y = TitleMargin + BorderWidth - InnerY + 2;
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

        public int GetTitleHeight()
        {
            // can't use this prior to init...
            int yterm = 0;
            if (HasTitle)
            {
                if (TitleSFont == null)
                {
                    throw new Exception("Font referenced before init!");
                }
                else
                {
                    yterm += TitleSFont.LineSpacing + 2 + TitleMargin + BorderWidth;
                }
            }
            return yterm;
        }

        public override void Render(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            Rectangle oldrect = RenderTargetScope.Open(sb, (int)Math.Floor(dx + x), (int)Math.Floor(dy + y), Width, Height);
            {
                Lastdx = dx;
                Lastdy = dy;
                int mx = input.State.MousePos.X;
                int my = input.State.MousePos.Y;
                if (HasScrolling)
                {
                    InnerX = BaseInnerX - ScrollCurrentX;
                    InnerY = BaseInnerY - ScrollCurrentY;
                }
                BaseRender(elapsedms, g, sb, input, dx, dy);
                // draw scrollbars if needed
                if (HasScrolling)
                {
                    int scw = ScrollSize + (BorderWidth * 2);

                    if (ScrollWidth > Width)
                    {
                        float sfx = dx + x + BorderWidth + 2;
                        float sfy = dy + y + Height - scw - 2 - BorderWidth;
                        int sfw = Width - (BorderWidth * 2) - 4;
                        int sfh = scw;
                        
                        Draw.DrawRectangleHandle(sb, (int)sfx, (int)sfy, sfw, sfh, TitleBackgroundColor);
                        Draw.DrawRectangleHandleOutline(sb, (int)sfx, (int)sfy, sfw, sfh, BorderColor, BorderWidth);

                        int sbarw = GetSliderBarWidth(false);
                        float sliderratio = ((float)(ScrollCurrentX) / (float)((ScrollWidth - Width)));
                        float sliderx = sfx + (sliderratio * (sfw - sbarw));
                        Draw.DrawRectangleHandleOutline(sb, (int)sliderx, (int)(sfy + BorderWidth),
                            GetSliderBarWidth(false), ScrollSize, TitleFontSettings.Color, BorderWidth);
                        if (mx > sfx && mx < sfx + sfw && my > sfy && my < sfy + sfh)
                        {
                            input.Cursor.TrySet(eCursorState.CLICKABLE);
                        }
                    }
                    if (ScrollHeight > Height)
                    {
                        int hmod = BorderWidth * 2;
                        int hstart = BorderWidth + 2;
                        if (HasTitle)
                        {
                            hmod += GetTitleHeight() + 2;
                            hstart += GetTitleHeight() + 2;
                        }
                        if (ScrollWidth > Width)
                        {
                            hmod += scw + 6;
                        }
                        else
                        {
                            hmod += 4;
                        }

                        float sfx = dx + x + Width - scw - 2 - BorderWidth;
                        float sfy = dy + y + hstart;
                        int sfw = scw;
                        int sfh = Height - hmod;
                        
                        Draw.DrawRectangleHandle(sb, (int)sfx, (int)sfy, sfw, sfh, TitleBackgroundColor);
                        Draw.DrawRectangleHandleOutline(sb, (int)sfx, (int)sfy, sfw, sfh, BorderColor, BorderWidth);

                        int sbarw = GetSliderBarWidth(true);
                        float sliderratio = ((float)(ScrollCurrentY) / (float)((GetScrollYMax())));
                        float slidery = sfy + (sliderratio * (sfh - sbarw));
                        Draw.DrawRectangleHandleOutline(sb, (int)(sfx + BorderWidth), (int)slidery,
                            ScrollSize, sbarw, TitleFontSettings.Color, BorderWidth);
                        if (mx > sfx && mx < sfx + sfw && my > sfy && my < sfy + sfh)
                        {
                            input.Cursor.TrySet(eCursorState.CLICKABLE);
                        }
                    }
                }
                RenderTargetScope.Close(sb, oldrect);

                // draw the border last so that it is on top of child elements
                if (HasBorder)
                {
                    Draw.DrawRectangleHandleOutline(sb, (int)(dx + x), (int)(dy + y), Width, Height, BorderColor, BorderWidth);
                }
                if (HasTitle)
                {
                    int titleHeight = GetTitleHeight();

                    Draw.DrawRectangleHandle(sb,
                        (int)(dx + x + BorderWidth),
                        (int)(dy + y + BorderWidth),
                        Width - (BorderWidth * 2),
                        titleHeight + TitleMargin,
                        Color);
                    Draw.DrawRectangleHandle(sb,
                        (int)(dx + x + TitleMargin + BorderWidth),
                        (int)(dy + y + TitleMargin + BorderWidth),
                        Width - (TitleMargin * 2) - (BorderWidth * 2),
                        titleHeight,
                        TitleBackgroundColor);

                    int diff = (titleHeight - TitleSFont.LineSpacing) / 2;
                    Draw.DrawText(g, sb, TitleSFont,
                        dx + x + (TitleMargin * 2) + 2 + BorderWidth,
                        dy + y + diff + BorderWidth + TitleMargin, Title, TitleFontSettings);
                }

                if (HasCloseButton)
                {
                    CloseButton.Render(elapsedms, g, sb, input, dx + x + BaseInnerX, dy + y + BaseInnerY);
                }
            }
        }

        private int GetSliderBarWidth(bool vert)
        {
            // this is just the size of the bar in the slider that the user drags
            int minsize = BorderWidth * 3;
            int w = vert ? Height : Width;
            int sw = vert ? ScrollHeight : ScrollWidth;
            int size = (int)(((float)w / (float)sw) * (((float)w * 3.0f) / 4.0f));

            return size > minsize ? size : minsize;
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            // render background
            Draw.DrawRectangleHandle(sb, (int)dx, (int)dy, Width, Height, Color);
        }

        public override bool PreventChildFocus(float dx, float dy, InputManager input)
        {
            // can override this if you need to prevent children from taking focus in certain conditions
            // check for scrollbars 
            if (HasScrolling)
            {
                return UpdateScrolling(0, input);
            }
            return false;
        }

        protected override void SelfGetFocus(float elapsedms, InputManager input)
        {
            // check for scrollbars 
            if (HasScrolling)
            {
                UpdateScrolling(elapsedms, input);
            }
        }

        public override void SelfFocusedInput(float elapsedms, InputManager input)
        {
            if (HasScrolling)
            {
                if (input.MouseDown(eMouseButtons.Left))
                {
                    UpdateScrolling(elapsedms, input);
                }
                else
                {
                    WasScrollingHorz = false;
                    WasScrollingVert = false;
                }

                HandleInput(elapsedms, input);
            }
        }

        public override void SelfParentInput(float elapsedms, InputManager input)
        {
            if (HasScrolling)
            {
                HandleInput(elapsedms, input);
            }
        }

        private void HandleInput(float elapsedms, InputManager input)
        {
            if (input.IsKeyFresh(Keys.Up))
            {
                input.CaptureInput(Keys.Up);
                ScrollCurrentY -= 5;
                if (ScrollCurrentY < 0)
                {
                    ScrollCurrentY = 0;
                }
            }
            if (input.IsKeyFresh(Keys.Down))
            {
                input.CaptureInput(Keys.Down);
                ScrollCurrentY += 5;
                if (ScrollCurrentY > GetScrollYMax())
                {
                    ScrollCurrentY = GetScrollYMax();
                }
            }
            if (input.IsKeyFresh(Keys.Left))
            {
                input.CaptureInput(Keys.Left);
                ScrollCurrentX -= 5;
                if (ScrollCurrentX < 0)
                {
                    ScrollCurrentX = 0;
                }
            }
            if (input.IsKeyFresh(Keys.Right))
            {
                input.CaptureInput(Keys.Right);
                ScrollCurrentX += 5;
                if (ScrollCurrentX > ScrollWidth - Width)
                {
                    ScrollCurrentX = ScrollWidth - Width;
                }
            }
            int scrolldiff = input.FreshMouseScrollDifference();
            if (scrolldiff > 0)
            {
                ScrollCurrentY -= scrolldiff / 2;
                if (ScrollCurrentY < 0)
                {
                    ScrollCurrentY = 0;
                }
                input.CaptureMouseScroll();
            }
            else if (scrolldiff < 0)
            {
                ScrollCurrentY -= scrolldiff / 2;
                if (ScrollCurrentY > GetScrollYMax())
                {
                    ScrollCurrentY = GetScrollYMax();
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
            int scw = ScrollSize + (BorderWidth * 2);

            if (ScrollWidth > Width)
            {
                float sfx = Lastdx + x + BorderWidth + 2;
                float sfy = Lastdy + y + Height - scw - 2;
                int sfw = Width - (BorderWidth * 2) - 4;
                int sfh = scw;
                if (((mx > sfx && mx < sfx + sfw && my > sfy && my < sfy + sfh) || WasScrollingHorz) && !WasScrollingVert)
                {
                    WasScrollingHorz = true;
                    //int sbarw = GetSliderBarWidth(false);
                    ScrollCurrentX = (int)(Math.Round((1.0f - (((sfx + sfw) - mx) / (sfw))) * (ScrollWidth - Width)));
                    changed = true;
                    if (ScrollCurrentX < 0)
                    {
                        ScrollCurrentX = 0;
                    }
                    if (ScrollCurrentX > ScrollWidth - Width)
                    {
                        ScrollCurrentX = ScrollWidth - Width;
                    }
                }
            }
            if (ScrollHeight > Height)
            {
                int hmod = BorderWidth * 2;
                int hstart = BorderWidth + 2;
                if (HasTitle)
                {
                    hmod += GetTitleHeight() + 2;
                    hstart += GetTitleHeight() + 2;
                }
                if (ScrollWidth > Width)
                {
                    hmod += scw + 4;
                }

                float sfx = Lastdx + x + Width - scw - 2;
                float sfy = Lastdy + y + hstart;
                int sfw = scw;
                int sfh = Height - hmod;
                if (((mx > sfx && mx < sfx + sfw && my > sfy && my < sfy + sfh) || WasScrollingVert) && !WasScrollingHorz)
                {
                    WasScrollingVert = true;
                    //int sbarw = GetSliderBarWidth(true);
                    ScrollCurrentY = (int)(Math.Round((1.0f - (((sfy + sfh) - my) / (sfh))) * (GetScrollYMax())));
                    changed = true;
                    if (ScrollCurrentY < 0)
                    {
                        ScrollCurrentY = 0;
                    }
                    if (ScrollCurrentY > GetScrollYMax())
                    {
                        ScrollCurrentY = GetScrollYMax();
                    }
                }
            }
            return changed;
        }

        public void SnapScrollToBottom()
        {
            if (HasScrolling && ScrollHeight > Height)
            {
                ScrollCurrentY = GetScrollYMax();
            }
        }

        public int GetScrollYMax()
        {
            if (Height >= ScrollHeight)
                return 0;
            return ScrollHeight - Height;
        }

        protected override void SelfRemove(UIComponent comp)
        {
            AutoComponents.Remove(comp);
        }

        public void Resize()
        {
            SelfResize();
        }

        protected virtual void SelfResize()
        {

        }
    }
}
