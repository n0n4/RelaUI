using Microsoft.Xna.Framework.Graphics;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Components
{
    public class UIDropdown : UIButton
    {
        public UIPanel PopupPanel = null;
        public UIAutoList SubList = null;

        public bool HasOptionBorders
        {
            get { return SubList.HasBorder; }
            set { SubList.HasBorder = value; }
        }

        public bool HasOptionOuterBorder
        {
            get { return SubList.HasOuterBorder; }
            set { SubList.HasOuterBorder = value; }
        }

        // A note...
        // if we remove the popup panel as soon as focus is lost, it is actually flipped
        // to invisible before the input checks
        // what this means is that input checks if we are clicking the popup,
        // sees that it is invisible, and decides we could not have clicked it.
        // therefore, to make dropdown items clickable, we need to persist the dropdown
        // panel for a frame longer after the user clicks. 
        // we use this counter to hold off on hiding the panel
        private int HidePopupCountdown = 0;

        public UIDropdown(float x, float y, int w, int h, string text, string font = "", int? fontsize = null,
            bool autoheight = true)
            : base(x, y, w, h, text, font, fontsize, autoheight)
        {
            AutoWidth = false;
            AutoHeight = false;
            SubList = new UIAutoList(eUIOrientation.VERTICAL, w, h, fixedindexwidth: w, fixedindexheight: h);
            SubList.MarginX = 0;
            SubList.MarginY = 0;
            PopupPanel = new UIPanel(0, h, w, 1, hasscrolling: true);
            PopupPanel.InnerMargin = 0;
            PopupPanel.Visible = false;

            PopupPanel.Add(SubList);
            //this.Add(PopupPanel);

            this.EventFocused += (sender, e) =>
            {
                PopupPanel.Visible = true;
            };

            this.EventFocusLost += (sender, e) =>
            {
                if (HidePopupCountdown == 0)
                    HidePopupCountdown = 2;
            };
        }

        public void AddItem(string name, Action<UIDropdown, EventFocusedHandlerArgs> action, int? fontsize = null)
        {
            UIButton b = new UIButton(0, 0, Width, Height, name, fontsize: fontsize);
            b.AutoWidth = false;
            b.AutoHeight = false;
            b.EventFocused += (sender, e) =>
            {
                EventFocusedHandlerArgs eargs = e as EventFocusedHandlerArgs;
                action(this, eargs);
            };

            SubList.AddAuto(b);
            SubList.MaxHeight += Height;
            PopupPanel.Height += Height;
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            base.SelfRender(elapsedms, g, sb, input, dx, dy);

            if (HidePopupCountdown > 0)
            {
                HidePopupCountdown--;
                if (HidePopupCountdown == 0)
                    PopupPanel.Visible = false;
            }

            // make sure that the whole options popup is visible
            const int margin = 8;
            int windowHeight = g.PresentationParameters.BackBufferHeight;
            int contentHeight = SubList.GetHeight();
            if (contentHeight < 1)
                contentHeight = 1;
            // how much space do we have until we hit the window bottom?
            int bottomSpace = windowHeight - ((int)dy + Height) - margin;
            int topSpace = (int)dy - margin;

            // open up if there's not enough room below us
            bool openUp = bottomSpace < 200;
            int available = bottomSpace;
            if (openUp)
            {
                available = topSpace;
            }

            if (available < 1)
                available = 1;

            int popupHeight = Math.Min(contentHeight, available);
            int popupY = openUp
                ? (int)dy - popupHeight
                : (int)dy + Height;

            if (popupY < margin)
                popupY = margin;
            if (popupY + popupHeight > windowHeight - margin)
                popupY = Math.Max(margin, windowHeight - margin - popupHeight);

            PopupPanel.x = dx;
            PopupPanel.y = popupY;
            PopupPanel.Width = Width;
            PopupPanel.Height = popupHeight;
            PopupPanel.ScrollHeight = contentHeight;

            int maxScroll = PopupPanel.Scroller.GetScrollYMax();
            if (PopupPanel.Scroller.OffsetY > maxScroll)
                PopupPanel.Scroller.OffsetY = maxScroll;
        }

        protected override void SelfPreInit()
        {
            base.SelfPreInit();

            // make the popup panel belong to the root
            if (PopupPanel.Parent == null && PopupPanel.System == null)
            {
                UIComponent root = GetRoot();
                if (root.System != null)
                {
                    root.System.Add(PopupPanel);
                }
                else
                {
                    root.Add(PopupPanel);
                }

                PopupPanel.Init();
            }
        }
    }
}
