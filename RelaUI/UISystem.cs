using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using RelaUI.Input;
using RelaUI.Components;
using RelaUI.Styles;
using RelaUI.DrawHandles;

namespace RelaUI
{
    public class UISystem : IDisposable
    {
        List<UIComponent> Components = new List<UIComponent>();
        public UIStyle Style = null;
        public UIComponent ProposedFocus = null;

        public GraphicsDevice GraphicsDevice;

        public UISystem(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;

            DrawHandleStatic.Setup(graphics);
        }

        public void Add(UIComponent child)
        {
            if (child.System != null)
            {
                throw new Exception("Cannot add a child to UISystem twice");
            }
            child.System = this;
            child.Parent = null;
            Components.Add(child);
            child.OnAdded();
        }

        private UIComponent RenderLoopComponent;
        public void Render(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                RenderLoopComponent = Components[i];
                if (!RenderLoopComponent.Visible)
                    continue;
                RenderLoopComponent.Render(elapsedms, g, sb, input, 0, 0);
            }
        }

        public void Init(UIStyle style)
        {
            Style = style;
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Init();
            }
        }

        public UIComponent CheckFocus(InputManager input, out float finalDx, out float finalDy)
        {
            finalDx = 0;
            finalDy = 0;
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                UIComponent comp = Components[i];
                UIComponent option = comp.CheckFocus(0, 0, input, out finalDx, out finalDy);
                if (option != null)
                {
                    return option;
                }
            }
            return null;
        }

        public UIComponent ProposeFocus()
        {
            UIComponent comp = ProposedFocus;
            ProposedFocus = null;
            return comp;
        }

        public void Remove(UIComponent comp)
        {
            Components.Remove(comp);
            comp.System = null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (UIComponent u in Components)
                    {
                        u.Dispose();
                    }
                }

                disposedValue = true;
            }
        }


        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}

