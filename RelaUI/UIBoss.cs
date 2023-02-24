using Microsoft.Xna.Framework.Graphics;
using RelaUI.Components;
using RelaUI.Input;
using RelaUI.Styles;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI
{
    public class UIBoss
    {
        private List<IUIHandle> UIHandles = new List<IUIHandle>();
        private UIComponent FocusedComponent = null;
        public UIStyle Style;
        private HotkeyManager HotkeyManager;

        public UIBoss(UIStyle style, HotkeyManager hotkeymanager)
        {
            Style = style;
            HotkeyManager = hotkeymanager;
        }

        public void RefreshStyle()
        {
            // call after changing style to propagate changes
            for (int i = 0; i < UIHandles.Count; i++)
            {
                UIHandles[i].GetUISystem().Init(Style);
            }
        }

        public void Draw(float elapsedms, GraphicsDevice graphics, SpriteBatch spriteBatch, InputManager input)
        {
            for (int i = 0; i < UIHandles.Count; i++)
            {
                UIHandles[i].GetUISystem().Render(elapsedms, graphics, spriteBatch, input);
            }
        }

        public void Add(IUIHandle handle)
        {
            UIHandles.Add(handle);
            handle.RegisterHotkeys(HotkeyManager);
        }

        public void Remove(string name)
        {
            int i = 0;
            while (i < UIHandles.Count)
            {
                if (UIHandles[i].GetName() == name)
                {
                    HotkeyManager.Unregister(string.Empty, name);
                    UIHandles.RemoveAt(i);
                    i--;
                }
                i++;
            }
        }

        public void Unfocus(float elapsedms, InputManager input)
        {
            if (FocusedComponent != null)
            {
                FocusedComponent.FocusLost(elapsedms, input);
            }
            FocusedComponent = null;
        }

        public void Focus(UIComponent comp, float elapsedms, InputManager input, float dx, float dy)
        {
            FocusedComponent = comp;
            comp.GetFocus(elapsedms, input, dx, dy);
        }

        public void Update(float elapsedms, InputManager input)
        {
            // handle focus requests from components
            UIComponent nextFocus = null;
            for (int i = 0; i < UIHandles.Count; i++)
            {
                UIComponent comp = UIHandles[i].GetUISystem().ProposeFocus();
                if (comp != null)
                    nextFocus = comp;
            }
            if (nextFocus != null)
                Focus(nextFocus, elapsedms, input, 0, 0);

            // TODO: handle deciding who has focus if mouse is released
            if (input.MousePressed(eMouseButtons.Left) || input.MousePressed(eMouseButtons.Right))
            {
                Unfocus(elapsedms, input);
                for (int i = UIHandles.Count - 1; i >= 0; i--)
                {
                    UIComponent option = UIHandles[i].GetUISystem().CheckFocus(input, out float dx, out float dy);
                    if (option != null)
                    {
                        Focus(option, elapsedms, input, dx, dy);
                        break;
                    }
                }
            }

            // send input to focused component
            if (FocusedComponent != null)
            {
                FocusedComponent.FocusedInput(elapsedms, input);
            }

            // send update to all children
            for (int i = 0; i < UIHandles.Count; i++)
            {
                UIHandles[i].Update(elapsedms, input);
            }
        }
    }
}

